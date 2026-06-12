namespace Overtime.Common.Exceptions;

public sealed class NotFoundException : Exception
{
    public NotFoundException(string resource, string identifier)
        : base($"{resource} '{identifier}' was not found.") { }
}
