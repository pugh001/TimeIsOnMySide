namespace Overtime.Common.Exceptions;

public sealed class SlotUnavailableException : Exception
{
    public SlotUnavailableException(string slotId)
        : base($"Slot '{slotId}' is unavailable.") { }
}
