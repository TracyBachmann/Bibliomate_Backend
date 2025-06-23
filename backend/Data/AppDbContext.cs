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
        public DbSet<History> Histories { get; set; }

        // Library infrastructure
        public DbSet<Zone> Zones { get; set; }
        public DbSet<Genre> Genres { get; set; }
        public DbSet<Shelf> Shelves { get; set; }
        public DbSet<ShelfLevel> ShelfLevels { get; set; }
        public DbSet<Stock> Stocks { get; set; }

        // Tag system and relations
        public DbSet<Tag> Tags { get; set; }
        public DbSet<BookTag> BookTags { get; set; }
        public DbSet<UserGenre> UserGenres { get; set; }

        // Normalized relations for authors and editors
        public DbSet<Author> Authors { get; set; }
        public DbSet<Editor> Editors { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // ------------------------------
            // 1) Unique constraints & indexes
            // ------------------------------
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

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

            // ------------------------------
            // 2) Precision on decimal
            // ------------------------------
            modelBuilder.Entity<Loan>()
                .Property(l => l.Fine)
                .HasColumnType("decimal(10,2)");

            // ------------------------------
            // 3) Loan ↔ Stock (no cascade)
            // ------------------------------
            modelBuilder.Entity<Loan>()
                .HasOne(l => l.Stock)
                .WithMany(s => s.Loans)
                .HasForeignKey(l => l.StockId)
                .OnDelete(DeleteBehavior.NoAction);

            // ------------------------------
            // 4) BookTag (many-to-many)
            // ------------------------------
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

            // ------------------------------
            // 5) Book → Author/Editor/Genre (restrict)
            // ------------------------------
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

            // ------------------------------
            // 6) UserGenre (composite key)
            // ------------------------------
            modelBuilder.Entity<UserGenre>()
                .HasKey(ug => new { ug.UserId, ug.GenreId });

            modelBuilder.Entity<UserGenre>()
                .HasOne(ug => ug.User)
                .WithMany(u => u.UserGenres)
                .HasForeignKey(ug => ug.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<UserGenre>()
                .HasOne(ug => ug.Genre)
                .WithMany()
                .HasForeignKey(ug => ug.GenreId)
                .OnDelete(DeleteBehavior.Cascade);

            base.OnModelCreating(modelBuilder);
        }
    }
}

