namespace TaskManager.Domain.Enums;

/// <summary>
/// Represents the role of a user in the system.
/// </summary>
public enum UserRole
{
    /// <summary>
    /// Regular user with limited permissions.
    /// </summary>
    User = 0,

    /// <summary>
    /// Administrator with full permissions.
    /// </summary>
    Admin = 1
}
