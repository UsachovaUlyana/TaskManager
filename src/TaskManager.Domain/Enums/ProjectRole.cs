namespace TaskManager.Domain.Enums;

/// <summary>
/// Represents the role of a user within a project.
/// </summary>
public enum ProjectRole
{
    /// <summary>
    /// Project member with basic access.
    /// </summary>
    Member = 0,

    /// <summary>
    /// Project owner with full access.
    /// </summary>
    Owner = 1
}
