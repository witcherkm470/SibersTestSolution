namespace SibersTestSolution.Application.DTOs.Tasks;

/// <summary>
/// Lightweight project data included in task responses.
/// </summary>
public sealed record TaskProjectResponse(
    int Id,
    string Name,
    int? ProjectManagerId)
{
    /// <summary>
    /// Maps a domain project to the lightweight task project response.
    /// </summary>
    public static TaskProjectResponse From(Domain.Entities.Project project)
    {
        return new TaskProjectResponse(project.Id, project.Name, project.ProjectManagerId);
    }
}
