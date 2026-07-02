using SibersTestSolution.Domain.Entities;

namespace SibersTestSolution.Application.DTOs.Projects;

/// <summary>
/// Response containing project document metadata.
/// </summary>
public sealed record ProjectDocumentInfoResponse(
    int Id,
    string OriginalFileName,
    string ContentType,
    long SizeInBytes,
    DateTime UploadedAtUtc)
{
    /// <summary>
    /// Maps a domain project document to the response model.
    /// </summary>
    public static ProjectDocumentInfoResponse From(ProjectDocument document)
    {
        return new ProjectDocumentInfoResponse(
            document.Id,
            document.OriginalFileName,
            document.ContentType,
            document.SizeInBytes,
            document.UploadedAtUtc);
    }
}
