﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BackendBiblioMate.Models
{
    /// <summary>
    /// Represents an analytics or statistics report generated by a user.
    /// Contains metadata and content for the report.
    /// </summary>
    public class Report
    {
        /// <summary>
        /// Gets the primary key of the report.
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ReportId { get; init; }

        /// <summary>
        /// Gets or sets the identifier of the user who generated the report.
        /// </summary>
        [Required(ErrorMessage = "UserId is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "UserId must be a positive integer.")]
        public int UserId { get; init; }

        /// <summary>
        /// Gets or sets the title of the report.
        /// </summary>
        [Required(ErrorMessage = "Title is required.")]
        [StringLength(255, MinimumLength = 1, ErrorMessage = "Title must be between 1 and 255 characters.")]
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the detailed content of the report.
        /// </summary>
        [Required(ErrorMessage = "Content is required.")]
        [StringLength(1000, MinimumLength = 1, ErrorMessage = "Content must be between 1 and 1000 characters.")]
        public string Content { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the date and time when the report was generated.
        /// </summary>
        [Required(ErrorMessage = "GeneratedDate is required.")]
        public DateTime GeneratedDate { get; init; }

        /// <summary>
        /// Navigation property for the user who generated the report.
        /// </summary>
        [ForeignKey(nameof(UserId))]
        public User User { get; init; } = null!;
    }
}