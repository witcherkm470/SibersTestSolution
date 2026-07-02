namespace SibersTestSolution.Domain.Abstractions;

/// <summary>
/// Defines the numeric identifier shared by persisted domain entities.
/// </summary>
public interface IEntity
{
    /// <summary>
    /// Gets or sets the database identifier.
    /// </summary>
    public int Id { get; set; }
}
