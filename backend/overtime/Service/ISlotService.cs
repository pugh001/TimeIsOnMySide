using Overtime.Service.Models.Slots;

namespace Overtime.Service;

public interface ISlotService
{
    Task<IReadOnlyList<SlotResponse>> GetSlotsAsync(string date, string locationId, CancellationToken ct = default);
}
