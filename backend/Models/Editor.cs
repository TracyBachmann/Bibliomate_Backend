using System.ComponentModel.DataAnnotations;

namespace backend.Models
{
    public class Editor
    {
        [Key]
        public int EditorId { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        public ICollection<Book> Books { get; set; } = new List<Book>();
    }
}