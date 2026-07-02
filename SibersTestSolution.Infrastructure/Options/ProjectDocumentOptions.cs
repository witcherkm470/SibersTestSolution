namespace SibersTestSolution.Infrastructure.Options;

/// <summary>
/// Configuration for project document uploads.
/// </summary>
public class ProjectDocumentOptions
{
    /// <summary>
    /// Gets the configuration section name.
    /// </summary>
    public const string SectionName = "ProjectDocuments";

    /// <summary>
    /// Gets or sets the directory where project documents are stored.
    /// </summary>
    public string StoragePath { get; set; } = "App_Data/ProjectDocuments";

    /// <summary>
    /// Gets or sets the maximum allowed upload size in bytes.
    /// </summary>
    public long MaxFileSizeBytes { get; set; } = 5 * 1024 * 1024;
}
