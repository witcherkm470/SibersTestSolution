namespace SibersTestSolution.Application.Common;

/// <summary>
/// Exception thrown when an expected entity cannot be found.
/// </summary>
public sealed class NotFoundException : Exception
{
    /// <summary>
    /// Creates a not found exception with the specified message.
    /// </summary>
    public NotFoundException(string message)
        : base(message)
    {
    }
}
