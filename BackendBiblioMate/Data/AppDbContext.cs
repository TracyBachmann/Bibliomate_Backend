using BackendBiblioMate.Models;
using BackendBiblioMate.Services.Infrastructure.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace BackendBiblioMate.Data
{
    /// <summary>
    /// Entity Framework Core database context for BiblioMate.
    /// Manages entity sets, relationships, indexes, constraints,
    /// and encryption of sensitive fields using <see cref="EncryptionService"/>.
    /// </summary>
    public class BiblioMateDbContext : DbContext
    {
        private readonly EncryptionService _encryptionService;

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="BiblioMateDbContext"/> class.
        /// </summary>
        /// <param name="options">EF Core DbContext configuration options.</param>
        /// <param name="encryptionService">Service responsible for encrypting sensitive data.</param>
        public BiblioMateDbContext(
            DbContextOptions<BiblioMateDbContext> options,
            EncryptionService encryptionService)
            : base(options)
        {
            _encryptionService = encryptionService ?? throw new ArgumentNullException(nameof(encryptionService));
        }

        #endregion

        #region DbSet Properties

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

        // Normalized relations
        public DbSet<Author> Authors { get; set; }
        public DbSet<Editor> Editors { get; set; }

        #endregion

        #region Model Configuration

        /// <summary>
        /// Configures the EF Core model:
        /// <list type="bullet">
        ///   <item><description>Unique constraints & indexes</description></item>
        ///   <item><description>Relationships (1-1, 1-n, n-n)</description></item>
        ///   <item><description>Composite keys</description></item>
        ///   <item><description>Decimal precision</description></item>
        ///   <item><description>Encryption for sensitive fields</description></item>
        /// </list>
        /// </summary>
        /// <param name="modelBuilder">The <see cref="ModelBuilder"/> used to configure entities.</param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // 1) Unique constraints & indexes
            ConfigureIndexes(modelBuilder);

            // 2) Decimal precision for Loan fines
            modelBuilder.Entity<Loan>()
                .Property(l => l.Fine)
                .HasColumnType("decimal(10,2)");

            // 3) Loan → Stock : one Stock can have many Loans (no cascade delete)
            modelBuilder.Entity<Loan>()
                .HasOne(l => l.Stock)
                .WithMany(s => s.Loans)
                .HasForeignKey(l => l.StockId)
                .OnDelete(DeleteBehavior.NoAction);

            // 4) Book ↔ Tag (many-to-many via BookTag)
            modelBuilder.Entity<BookTag>(entity =>
            {
                entity.HasKey(bt => new { bt.BookId, bt.TagId });

                entity.HasOne(bt => bt.Book)
                      .WithMany(b => b.BookTags)
                      .HasForeignKey(bt => bt.BookId);

                entity.HasOne(bt => bt.Tag)
                      .WithMany(t => t.BookTags)
                      .HasForeignKey(bt => bt.TagId);
            });

            // 5) Book ↔ Stock : one-to-one (cascade delete)
            modelBuilder.Entity<Book>()
                .HasOne(b => b.Stock)
                .WithOne(s => s.Book)
                .HasForeignKey<Stock>(s => s.BookId)
                .OnDelete(DeleteBehavior.Cascade);

            // Guarantee one Stock per Book
            modelBuilder.Entity<Stock>()
                .HasIndex(s => s.BookId)
                .IsUnique();

            // 6) Restrict delete for Book → Author/Editor/Genre
            modelBuilder.Entity<Book>(entity =>
            {
                entity.HasOne(b => b.Author)
                      .WithMany(a => a.Books)
                      .HasForeignKey(b => b.AuthorId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(b => b.Editor)
                      .WithMany(e => e.Books)
                      .HasForeignKey(b => b.EditorId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(b => b.Genre)
                      .WithMany(g => g.Books)
                      .HasForeignKey(b => b.GenreId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // 7) User ↔ Genre : composite key with cascade delete
            modelBuilder.Entity<UserGenre>(entity =>
            {
                entity.HasKey(ug => new { ug.UserId, ug.GenreId });

                entity.HasOne(ug => ug.User)
                      .WithMany(u => u.UserGenres)
                      .HasForeignKey(ug => ug.UserId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(ug => ug.Genre)
                      .WithMany()
                      .HasForeignKey(ug => ug.GenreId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // 8) Encrypt sensitive User fields
            var encString         = CreateEncryptionConverter();
            var encNullableString = CreateNullableEncryptionConverter();

            modelBuilder.Entity<User>(entity =>
            {
                entity.Property(u => u.Address1).HasConversion(encString);
                entity.Property(u => u.Address2).HasConversion(encNullableString);
                entity.Property(u => u.Phone).HasConversion(encString);
            });

            base.OnModelCreating(modelBuilder);
        }

        #endregion

        #region Private Helpers

        /// <summary>
        /// Configures unique indexes across entities to enforce data integrity.
        /// </summary>
        private static void ConfigureIndexes(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>()
                        .HasIndex(u => u.Email)
                        .IsUnique();

            modelBuilder.Entity<Book>().HasIndex(b => b.Title);
            modelBuilder.Entity<Book>().HasIndex(b => b.GenreId);
            modelBuilder.Entity<Book>().HasIndex(b => b.AuthorId);
            modelBuilder.Entity<Book>().HasIndex(b => b.EditorId);
            modelBuilder.Entity<Book>().HasIndex(b => b.PublicationDate);

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
        }

        /// <summary>
        /// Builds a value converter that encrypts/decrypts non-null string properties.
        /// </summary>
        private ValueConverter<string, string> CreateEncryptionConverter()
            => new(
                v => _encryptionService.Encrypt(v),
                v => _encryptionService.Decrypt(v)
            );


        /// <summary>
        /// Builds a value converter that encrypts/decrypts nullable string properties.
        /// </summary>
        private ValueConverter<string?, string?> CreateNullableEncryptionConverter()
            => new(
                v => string.IsNullOrEmpty(v) ? null : _encryptionService.Encrypt(v),
                v => string.IsNullOrEmpty(v) ? null : _encryptionService.Decrypt(v)
            );

        #endregion
    }
}
