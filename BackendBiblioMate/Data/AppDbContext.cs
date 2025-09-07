using BackendBiblioMate.Models;
using BackendBiblioMate.Services;
using BackendBiblioMate.Services.Infrastructure.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace BackendBiblioMate.Data
{
    /// <summary>
    /// Entity Framework Core database context for BiblioMate.
    /// Configures entity sets, indexes, relationships, and
    /// encryption of sensitive fields.
    /// </summary>
    public class BiblioMateDbContext : DbContext
    {
        private readonly EncryptionService _encryptionService;

        #region Constructor

        /// <summary>
        /// Initializes a new instance of <see cref="BiblioMateDbContext"/>.
        /// </summary>
        /// <param name="options">
        /// The options to be used by this DbContext.
        /// </param>
        /// <param name="encryptionService">
        /// Service for encrypting and decrypting sensitive user data.
        /// </param>
        public BiblioMateDbContext(
            DbContextOptions<BiblioMateDbContext> options,
            EncryptionService encryptionService)
            : base(options)
        {
            _encryptionService = encryptionService;
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
        /// Configures the EF Core model.
        /// Defines indexes, relationships, precision, and encryption.
        /// </summary>
        /// <param name="modelBuilder">
        /// The builder being used to construct the model for this context.
        /// </param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // 1) Unique constraints & indexes
            ConfigureIndexes(modelBuilder);

            // 2) Decimal precision for fines
            modelBuilder.Entity<Loan>()
                .Property(l => l.Fine)
                .HasColumnType("decimal(10,2)");

            // 3) Loan ↔ Stock (no cascade delete)
            modelBuilder.Entity<Loan>()
                .HasOne(l => l.Stock)
                .WithMany(s => s.Loans)
                .HasForeignKey(l => l.StockId)
                .OnDelete(DeleteBehavior.NoAction);

            // 4) Many-to-many: BookTag
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
            
            // Book ↔ Stock : 1–1 (Stock porte la FK BookId)
            modelBuilder.Entity<Book>()
                .HasOne(b => b.Stock)
                .WithOne(s => s.Book)
                .HasForeignKey<Stock>(s => s.BookId)
                .OnDelete(DeleteBehavior.Cascade);

            // Un seul Stock par Book
            modelBuilder.Entity<Stock>()
                .HasIndex(s => s.BookId)
                .IsUnique();
            
            // 5) Restrict delete for Book → Author/Editor/Genre
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

            // 6) Composite key: UserGenre
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

            // 7) Encrypt sensitive User fields
            var encString        = CreateEncryptionConverter();
            var encNullableString = CreateNullableEncryptionConverter();

            modelBuilder.Entity<User>(entity =>
            {
                entity.Property(u => u.Address1)
                      .HasConversion(encString);

                entity.Property(u => u.Address2)
                      .HasConversion(encNullableString);

                entity.Property(u => u.Phone)
                      .HasConversion(encString);
            });

            base.OnModelCreating(modelBuilder);
        }

        #endregion

        #region Private Helpers

        /// <summary>
        /// Configures unique constraints and indexes for entities.
        /// </summary>
        /// <param name="modelBuilder">
        /// The model builder to configure.
        /// </param>
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
            => new ValueConverter<string, string>(
                v => _encryptionService.Encrypt(v),
                v => _encryptionService.Decrypt(v)
            );

        /// <summary>
        /// Builds a value converter that encrypts/decrypts nullable string properties.
        /// </summary>
        private ValueConverter<string?, string?> CreateNullableEncryptionConverter()
            => new ValueConverter<string?, string?>(
                v => v == null ? null : _encryptionService.Encrypt(v),
                v => v == null ? null : _encryptionService.Decrypt(v)
            );

        #endregion
    }
}
