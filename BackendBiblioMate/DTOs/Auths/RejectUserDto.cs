namespace BackendBiblioMate.DTOs;

/// <summary>
/// DTO for rejecting a user account.
/// </summary>
public class RejectUserDto
{
    /// <summary>
    /// Optional reason for rejection.
    /// </summary>
    public string? Reason { get; set; }
}