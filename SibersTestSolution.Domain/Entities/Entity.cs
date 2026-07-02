using SibersTestSolution.Domain.Abstractions;

namespace SibersTestSolution.Domain.Entities;

/// <summary>
/// Base class for entities persisted in the application database.
/// </summary>
public class Entity : IEntity
{
    /// <summary>
    /// Gets or sets the database identifier.
    /// </summary>
    public int Id { get; set; }
}
