namespace School.Shared.Core.Authentication.Claims;

public enum ClaimLevel : int
{
    /// <summary>
    /// No Access
    /// </summary>
    None = 0,
    
    /// <summary>
    /// Read only access
    /// </summary>
    ReadOnly = 1,
    
    /// <summary>
    /// Read / Write access for self.
    /// Inherits access from <see cref="ClaimLevel.ReadOnly"/>
    /// </summary>
    User = 2,
    
    /// <summary>
    /// Full access to all system administrative features.
    /// Inherits access from <see cref="ClaimLevel.User"/>
    /// </summary>
    Admin = 3
}