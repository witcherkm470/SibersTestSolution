using SibersTestSolution.Domain.Entities;

namespace SibersTestSolution.Api.DTOs;

/// <summary>
/// Response containing uploaded project document metadata.
/// </summary>
public sealed record ProjectDocumentResponse(
    int Id,
    string OriginalFileName,
    string ContentType,
    long SizeInBytes,
    DateTime UploadedAtUtc)
{
    /// <summary>
    /// Maps a domain project document to the response model.
    /// </summary>
    public static ProjectDocumentResponse From(ProjectDocument document)
    {
        return new ProjectDocumentResponse(
            document.Id,
            document.OriginalFileName,
            document.ContentType,
            document.SizeInBytes,
            document.UploadedAtUtc);
    }
}
