using System.ComponentModel.DataAnnotations;

namespace BackendBiblioMate.DTOs
{
    /// <summary>Payload pour créer un prêt.</summary>
    public class LoanCreateDto
    {
        /// <summary>
        /// Identifiant de l’utilisateur emprunteur. Optionnel : si non fourni,
        /// on le déduit du token (utilisateur standard). Requis seulement pour le staff.
        /// </summary>
        public int? UserId { get; init; }

        /// <summary>Identifiant du livre à emprunter.</summary>
        [Required(ErrorMessage = "BookId is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "BookId must be a positive integer.")]
        public int BookId { get; init; }
    }
}