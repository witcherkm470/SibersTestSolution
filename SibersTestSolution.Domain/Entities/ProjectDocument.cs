namespace SibersTestSolution.Domain.Entities;

/// <summary>
/// Represents a file uploaded for a project.
/// </summary>
public class ProjectDocument : Entity
{
    private ProjectDocument()
    {
    }

    /// <summary>
    /// Creates a document record for a stored project file.
    /// </summary>
    public ProjectDocument(
        int projectId,
        string originalFileName,
        string storedFileName,
        string contentType,
        long sizeInBytes,
        string relativePath)
    {
        SetProjectId(projectId);
        SetOriginalFileName(originalFileName);
        SetStoredFileName(storedFileName);
        SetContentType(contentType);
        SetSizeInBytes(sizeInBytes);
        SetRelativePath(relativePath);
        UploadedAtUtc = DateTime.UtcNow;
    }

    /// <summary>
    /// Gets the project identifier.
    /// </summary>
    public int ProjectId { get; private set; }

    /// <summary>
    /// Gets the project navigation property.
    /// </summary>
    public Project Project { get; private set; } = null!;

    /// <summary>
    /// Gets the file name provided by the user.
    /// </summary>
    public string OriginalFileName { get; private set; } = null!;

    /// <summary>
    /// Gets the unique file name used on disk.
    /// </summary>
    public string StoredFileName { get; private set; } = null!;

    /// <summary>
    /// Gets the uploaded file content type.
    /// </summary>
    public string ContentType { get; private set; } = null!;

    /// <summary>
    /// Gets the uploaded file size in bytes.
    /// </summary>
    public long SizeInBytes { get; private set; }

    /// <summary>
    /// Gets the path to the stored file relative to the configured storage root.
    /// </summary>
    public string RelativePath { get; private set; } = null!;

    /// <summary>
    /// Gets the UTC date and time when the file was uploaded.
    /// </summary>
    public DateTime UploadedAtUtc { get; private set; }

    private void SetProjectId(int projectId)
    {
        if (projectId <= 0)
        {
            throw new ArgumentException("Project id must be greater than 0.", nameof(projectId));
        }

        ProjectId = projectId;
    }

    private void SetOriginalFileName(string originalFileName)
    {
        if (string.IsNullOrWhiteSpace(originalFileName))
        {
            throw new ArgumentException("Original file name cannot be empty.", nameof(originalFileName));
        }

        OriginalFileName = originalFileName.Trim();
    }

    private void SetStoredFileName(string storedFileName)
    {
        if (string.IsNullOrWhiteSpace(storedFileName))
        {
            throw new ArgumentException("Stored file name cannot be empty.", nameof(storedFileName));
        }

        StoredFileName = storedFileName.Trim();
    }

    private void SetContentType(string contentType)
    {
        ContentType = string.IsNullOrWhiteSpace(contentType)
            ? "application/octet-stream"
            : contentType.Trim();
    }

    private void SetSizeInBytes(long sizeInBytes)
    {
        if (sizeInBytes <= 0)
        {
            throw new ArgumentException("File size must be greater than 0.", nameof(sizeInBytes));
        }

        SizeInBytes = sizeInBytes;
    }

    private void SetRelativePath(string relativePath)
    {
        if (string.IsNullOrWhiteSpace(relativePath))
        {
            throw new ArgumentException("Relative path cannot be empty.", nameof(relativePath));
        }

        RelativePath = relativePath.Trim();
    }
}
