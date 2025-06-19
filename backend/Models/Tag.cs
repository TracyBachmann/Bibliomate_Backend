using System.ComponentModel.DataAnnotations;

namespace backend.Models
{
    public class Tag
    {
        public int TagId { get; set; }

        [Required]
        [MaxLength(50)]
        public string Name { get; set; }

        public ICollection<BookTag> BookTags { get; set; } = new List<BookTag>();
    }
}