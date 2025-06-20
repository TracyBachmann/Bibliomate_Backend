using Microsoft.EntityFrameworkCore;
using backend.Models;

namespace backend.Data
{
    public class BiblioMateDbContext : DbContext
    {
        public BiblioMateDbContext(DbContextOptions<BiblioMateDbContext> options)
            : base(options)
        {
        }

        // Core entities
        public DbSet<User> Users { get; set; }
        public DbSet<Book> Books { get; set; }
        public DbSet<Loan> Loans { get; set; }
        public DbSet<Reservation> Reservations { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<Report> Reports { get; set; }

        // Library infrastructure
        public DbSet<Zone> Zones { get; set; }
        public DbSet<Genre> Genres { get; set; }
        public DbSet<Shelf> Shelves { get; set; }
        public DbSet<ShelfLevel> ShelfLevels { get; set; }
        public DbSet<Stock> Stocks { get; set; }

        // Tag system and relations
        public DbSet<Tag> Tags { get; set; }
        public DbSet<BookTag> BookTags { get; set; }

        // Normalized relations for authors and editors
        public DbSet<Author> Authors { get; set; }
        public DbSet<Editor> Editors { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            modelBuilder.Entity<Loan>()
                .Property(l => l.Fine)
                .HasColumnType("decimal(10,2)");

            // Configure composite key for Book <-> Tag relation
            modelBuilder.Entity<BookTag>()
                .HasKey(bt => new { bt.BookId, bt.TagId });

            modelBuilder.Entity<BookTag>()
                .HasOne(bt => bt.Book)
                .WithMany(b => b.BookTags)
                .HasForeignKey(bt => bt.BookId);

            modelBuilder.Entity<BookTag>()
                .HasOne(bt => bt.Tag)
                .WithMany(t => t.BookTags)
                .HasForeignKey(bt => bt.TagId);

            // Add useful indexes for search and filtering
            modelBuilder.Entity<Book>()
                .HasIndex(b => b.Title);

            modelBuilder.Entity<Book>()
                .HasIndex(b => b.GenreId);

            modelBuilder.Entity<Book>()
                .HasIndex(b => b.AuthorId);

            modelBuilder.Entity<Book>()
                .HasIndex(b => b.EditorId);

            modelBuilder.Entity<Book>()
                .HasIndex(b => b.PublicationDate);

            modelBuilder.Entity<Book>()
                .HasIndex(b => b.Isbn)
                .IsUnique();

            modelBuilder.Entity<Author>()
                .HasIndex(a => a.Name)
                .IsUnique();

            modelBuilder.Entity<Editor>()
                .HasIndex(e => e.Name)
                .IsUnique();

            modelBuilder.Entity<Genre>()
                .HasIndex(g => g.Name)
                .IsUnique();

            // Restrict cascade deletes on key relations
            modelBuilder.Entity<Book>()
                .HasOne(b => b.Author)
                .WithMany(a => a.Books)
                .HasForeignKey(b => b.AuthorId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Book>()
                .HasOne(b => b.Editor)
                .WithMany(e => e.Books)
                .HasForeignKey(b => b.EditorId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Book>()
                .HasOne(b => b.Genre)
                .WithMany(g => g.Books)
                .HasForeignKey(b => b.GenreId)
                .OnDelete(DeleteBehavior.Restrict);

            base.OnModelCreating(modelBuilder);
        }
    }
}
