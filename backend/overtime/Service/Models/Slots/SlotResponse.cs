namespace Overtime.Service.Models.Slots;

public sealed record SlotResponse(
    string Id,
    string Date,
    string StartTime,
    string EndTime,
    string Status,
    string LocationId);
