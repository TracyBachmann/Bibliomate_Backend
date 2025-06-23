namespace backend.Models;

public class UserGenre
{
    public int UserId { get; set; }
    public User User { get; set; }

    public int GenreId { get; set; }
    public Genre Genre { get; set; }
}
