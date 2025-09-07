using System.ComponentModel.DataAnnotations;

namespace BackendBiblioMate.DTOs
{
    public class ResendEmailConfirmationDto
    {
        [Required, EmailAddress]
        public string Email { get; init; } = string.Empty;
    }
}