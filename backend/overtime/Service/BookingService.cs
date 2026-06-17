using System.Data;
using System.Diagnostics;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Npgsql;
using Overtime.Common.Exceptions;
using Overtime.Data;
using Overtime.Data.Entities;
using Overtime.Service.Models.Bookings;

namespace Overtime.Service;

public sealed partial class BookingService : IBookingService
{
    private readonly OvertimeDbContext _db;
    private readonly EmailQueue _emailQueue;
    private readonly ILogger<BookingService> _logger;

    public BookingService(OvertimeDbContext db, EmailQueue emailQueue, ILogger<BookingService> logger)
    {
        ArgumentNullException.ThrowIfNull(db);
        ArgumentNullException.ThrowIfNull(emailQueue);
        ArgumentNullException.ThrowIfNull(logger);
        _db = db;
        _emailQueue = emailQueue;
        _logger = logger;
    }

    public async Task<CreateBookingResponse> CreateBookingAsync(
        CreateBookingRequest request, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var (slug, date, startTime) = ParseSlotId(request.SlotId);

        // Resolve location outside the transaction (read-only, no contention risk).
        var location = await _db.Locations
            .AsNoTracking()
            .FirstOrDefaultAsync(l => l.Slug == slug, ct)
            ?? throw new NotFoundException("Location", slug);

        BookingEntity booking = null!;

        await _db.Database.CreateExecutionStrategy().ExecuteAsync(async () =>
        {
            await using var tx = await _db.Database.BeginTransactionAsync(IsolationLevel.Serializable, ct);

            var activeStaff = await _db.Users
                .AsNoTracking()
                .Where(u => u.LocationId == location.Id && u.Role == "staff")
                .OrderBy(u => u.FullName)
                .ToListAsync(ct);

            if (activeStaff.Count == 0)
                throw new SlotUnavailableException(request.SlotId);

            var bookedStaffIds = await _db.Bookings
                .AsNoTracking()
                .Where(b => b.Staff.LocationId == location.Id
                         && b.SlotDate == date
                         && b.StartTime == startTime)
                .Select(b => b.StaffId)
                .ToListAsync(ct);

            if (bookedStaffIds.Count >= activeStaff.Count)
                throw new SlotUnavailableException(request.SlotId);

            var availableStaff = activeStaff.First(u => !bookedStaffIds.Contains(u.Id));

            var bookingRef = GenerateBookingRef();
            booking = new BookingEntity
            {
                Id = Guid.NewGuid(),
                BookingRef = bookingRef,
                StaffId = availableStaff.Id,
                SlotDate = date,
                StartTime = startTime,
                EndTime = startTime.AddMinutes(30),
                CustomerName = request.Name,
                CustomerEmail = request.Email,
                CustomerPhone = request.Phone,
                Notes = request.Notes,
                CreatedAt = DateTimeOffset.UtcNow
            };

            try
            {
                _db.Bookings.Add(booking);
                await _db.SaveChangesAsync(ct);
                await tx.CommitAsync(ct);
            }
            catch (DbUpdateException ex)
                when (ex.InnerException is PostgresException pg && pg.SqlState == "23505")
            {
                if (pg.ConstraintName == "uq_staff_slot")
                    throw new SlotUnavailableException(request.SlotId);
                throw;
            }
        });

        Debug.Assert(booking.Id != Guid.Empty, "Booking ID must be assigned after save.");

        var staffEmail = await _db.Users
            .AsNoTracking()
            .Where(u => u.Id == booking.StaffId)
            .Select(u => u.Email)
            .FirstOrDefaultAsync(ct) ?? string.Empty;

        var confirmation = new BookingConfirmation(
            BookingRef: booking.BookingRef,
            CustomerName: request.Name,
            CustomerEmail: request.Email,
            StaffEmail: staffEmail,
            Date: date,
            StartTime: startTime);

        _emailQueue.TryEnqueue(confirmation);

        return new CreateBookingResponse(
            BookingId: booking.BookingRef,
            SlotId: request.SlotId,
            StartTime: startTime.ToString("HH:mm"),
            Date: date.ToString("yyyy-MM-dd"),
            Name: request.Name);
    }

    public async Task<IReadOnlyList<BookingResponse>> GetBookingsAsync(
        Guid staffId, CancellationToken ct = default)
    {
        if (staffId == Guid.Empty)
            throw new ArgumentException("staffId must not be empty.", nameof(staffId));

        var fromDate = DateOnly.FromDateTime(DateTime.UtcNow);

        return await _db.Bookings
            .AsNoTracking()
            .Where(b => b.StaffId == staffId && b.SlotDate >= fromDate)
            .OrderBy(b => b.SlotDate)
            .ThenBy(b => b.StartTime)
            .Select(b => new BookingResponse(
                b.BookingRef,
                b.SlotDate.ToString("yyyy-MM-dd"),
                b.StartTime.ToString("HH:mm"),
                b.EndTime.ToString("HH:mm"),
                b.CustomerName))
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<BookingResponse>> GetBookingsByUserIdAsync(
        Guid userId, CancellationToken ct = default)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("userId must not be empty.", nameof(userId));

        var fromDate = DateOnly.FromDateTime(DateTime.UtcNow);

        return await _db.Bookings
            .AsNoTracking()
            .Where(b => b.StaffId == userId && b.SlotDate >= fromDate)
            .OrderBy(b => b.SlotDate)
            .ThenBy(b => b.StartTime)
            .Select(b => new BookingResponse(
                b.BookingRef,
                b.SlotDate.ToString("yyyy-MM-dd"),
                b.StartTime.ToString("HH:mm"),
                b.EndTime.ToString("HH:mm"),
                b.CustomerName))
            .ToListAsync(ct);
    }

    internal static (string Slug, DateOnly Date, TimeOnly StartTime) ParseSlotId(string slotId)
    {
        ArgumentException.ThrowIfNullOrEmpty(slotId);

        var match = SlotIdRegex().Match(slotId);
        if (!match.Success)
            throw new ArgumentException($"SlotId '{slotId}' does not match expected format {{slug}}-{{YYYY-MM-DD}}-{{HH:mm}}.", nameof(slotId));

        var slug = match.Groups["slug"].Value;
        var date = DateOnly.Parse(match.Groups["date"].Value);
        var time = TimeOnly.Parse(match.Groups["time"].Value);

        return (slug, date, time);
    }

    internal static string GenerateBookingRef()
    {
        const string chars = "abcdefghijklmnopqrstuvwxyz0123456789";
        var random = new char[8];
        for (var i = 0; i < 8; i++)
        {
            Debug.Assert(i < random.Length); // loop invariant
            random[i] = chars[System.Security.Cryptography.RandomNumberGenerator.GetInt32(chars.Length)];
        }
        Debug.Assert(random.Length == 8); // post-condition: ref is always 8 characters
        return $"bk-{new string(random)}";
    }

    [GeneratedRegex(@"^(?<slug>.+)-(?<date>\d{4}-\d{2}-\d{2})-(?<time>\d{2}:\d{2})$")]
    private static partial Regex SlotIdRegex();
}
