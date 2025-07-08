using AutoMapper;
using BackendBiblioMate.DTOs;
using BackendBiblioMate.Models;
using BackendBiblioMate.Models.Enums;

namespace BackendBiblioMate.Configuration
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            //
            // === AUTHOR ===
            //
            CreateMap<Author, AuthorReadDto>()
                .ForMember(d => d.AuthorId,   o => o.MapFrom(s => s.AuthorId))
                .ForMember(d => d.Name,       o => o.MapFrom(s => s.Name));

            CreateMap<AuthorCreateDto, Author>()
                .ForMember(d => d.Name,       o => o.MapFrom(s => s.Name));


            //
            // === USER / AUTH ===
            //
            CreateMap<RegisterDto, User>()
                .ForMember(d => d.Name,   o => o.MapFrom(s => s.Name))
                .ForMember(d => d.Email,      o => o.MapFrom(s => s.Email))
                .ForMember(d => d.Phone,      o => o.MapFrom(s => s.Phone))
                .ForMember(d => d.Address,    o => o.MapFrom(s => s.Address))
                .ForMember(d => d.Password, o => o.Ignore());


            //
            // === BOOK ===
            //
            CreateMap<Book, BookReadDto>()
                .ForMember(d => d.BookId,         o => o.MapFrom(s => s.BookId))
                .ForMember(d => d.Title,          o => o.MapFrom(s => s.Title))
                .ForMember(d => d.Isbn,           o => o.MapFrom(s => s.Isbn))
                .ForMember(d => d.PublicationYear,o => o.MapFrom(s => s.PublicationDate.Year))
                .ForMember(d => d.AuthorName,     o => o.MapFrom(s => s.Author.Name))
                .ForMember(d => d.GenreName,      o => o.MapFrom(s => s.Genre.Name))
                .ForMember(d => d.EditorName,     o => o.MapFrom(s => s.Editor.Name))
                .ForMember(d => d.IsAvailable,    o => o.MapFrom(s => s.Stock.Any(st => st.Quantity > 0)))
                .ForMember(d => d.CoverUrl,       o => o.MapFrom(s => s.CoverUrl))
                .ForMember(d => d.Tags,           o => o.MapFrom(s => s.BookTags.Select(bt => bt.Tag.Name)));

            CreateMap<BookCreateDto, Book>()
                .ForMember(d => d.Title,           o => o.MapFrom(s => s.Title))
                .ForMember(d => d.Isbn,            o => o.MapFrom(s => s.Isbn))
                .ForMember(d => d.PublicationDate, o => o.MapFrom(s => s.PublicationDate))
                .ForMember(d => d.AuthorId,        o => o.MapFrom(s => s.AuthorId))
                .ForMember(d => d.GenreId,         o => o.MapFrom(s => s.GenreId))
                .ForMember(d => d.EditorId,        o => o.MapFrom(s => s.EditorId))
                .ForMember(d => d.ShelfLevelId,    o => o.MapFrom(s => s.ShelfLevelId))
                .ForMember(d => d.CoverUrl,        o => o.MapFrom(s => s.CoverUrl))
                // Les BookTags se gèrent en service
                ;

            CreateMap<BookUpdateDto, Book>()
                .ForMember(d => d.BookId,          o => o.MapFrom(s => s.BookId))
                .ForMember(d => d.Title,           o => o.MapFrom(s => s.Title))
                .ForMember(d => d.Isbn,            o => o.MapFrom(s => s.Isbn))
                .ForMember(d => d.PublicationDate, o => o.MapFrom(s => s.PublicationDate))
                .ForMember(d => d.AuthorId,        o => o.MapFrom(s => s.AuthorId))
                .ForMember(d => d.GenreId,         o => o.MapFrom(s => s.GenreId))
                .ForMember(d => d.EditorId,        o => o.MapFrom(s => s.EditorId))
                .ForMember(d => d.ShelfLevelId,    o => o.MapFrom(s => s.ShelfLevelId))
                .ForMember(d => d.CoverUrl,        o => o.MapFrom(s => s.CoverUrl))
                // Tags idem
                ;


            //
            // === LOAN ===
            //
            CreateMap<LoanCreateDto, Loan>()
                .ForMember(d => d.UserId, o => o.MapFrom(s => s.UserId))
                .ForMember(d => d.BookId, o => o.MapFrom(s => s.BookId));

            CreateMap<Loan, LoanReadDto>()
                .ForMember(d => d.LoanId,         o => o.MapFrom(s => s.LoanId))
                .ForMember(d => d.UserId,         o => o.MapFrom(s => s.UserId))
                .ForMember(d => d.UserName,       o => o.MapFrom(s => s.User.FullName))
                .ForMember(d => d.BookId,         o => o.MapFrom(s => s.BookId))
                .ForMember(d => d.BookTitle,      o => o.MapFrom(s => s.Stock.Book.Title))
                .ForMember(d => d.LoanDate,       o => o.MapFrom(s => s.LoanDate))
                .ForMember(d => d.DueDate,        o => o.MapFrom(s => s.DueDate))
                .ForMember(d => d.ReturnDate,     o => o.MapFrom(s => s.ReturnDate))
                .ForMember(d => d.Fine,           o => o.MapFrom(s => s.Fine));

            CreateMap<Loan, LoanReturnedResult>()
                .ForMember(d => d.ReservationNotified, o => o.Ignore())
                .ForMember(d => d.Fine,               o => o.MapFrom(s => s.Fine));


            //
            // === EDITOR ===
            //
            CreateMap<Editor, EditorReadDto>()
                .ForMember(d => d.EditorId, o => o.MapFrom(s => s.EditorId))
                .ForMember(d => d.Name,     o => o.MapFrom(s => s.Name));

            CreateMap<EditorCreateDto, Editor>()
                .ForMember(d => d.Name,     o => o.MapFrom(s => s.Name));

            CreateMap<EditorUpdateDto, Editor>()
                .ForMember(d => d.Name,     o => o.MapFrom(s => s.Name));


            //
            // === GENRE ===
            //
            CreateMap<Genre, GenreReadDto>()
                .ForMember(d => d.GenreId, o => o.MapFrom(s => s.GenreId))
                .ForMember(d => d.Name,    o => o.MapFrom(s => s.Name));

            CreateMap<GenreCreateDto, Genre>()
                .ForMember(d => d.Name,    o => o.MapFrom(s => s.Name));

            CreateMap<GenreUpdateDto, Genre>()
                .ForMember(d => d.Name,    o => o.MapFrom(s => s.Name));


            //
            // === HISTORY ===
            //
            CreateMap<History, HistoryReadDto>()
                .ForMember(d => d.HistoryId,     o => o.MapFrom(s => s.HistoryId))
                .ForMember(d => d.EventType,     o => o.MapFrom(s => s.EventType))
                .ForMember(d => d.EventDate,     o => o.MapFrom(s => s.EventDate))
                .ForMember(d => d.LoanId,        o => o.MapFrom(s => s.LoanId))
                .ForMember(d => d.ReservationId, o => o.MapFrom(s => s.ReservationId));


            //
            // === NOTIFICATION ===
            //
            CreateMap<Notification, NotificationReadDto>()
                .ForMember(d => d.NotificationId,o => o.MapFrom(s => s.NotificationId))
                .ForMember(d => d.UserId,       o => o.MapFrom(s => s.UserId))
                .ForMember(d => d.UserName,     o => o.MapFrom(s => s.User.FullName))
                .ForMember(d => d.Title,        o => o.MapFrom(s => s.Title))
                .ForMember(d => d.Message,      o => o.MapFrom(s => s.Message))
                .ForMember(d => d.Timestamp,    o => o.MapFrom(s => s.CreatedAt));

            CreateMap<NotificationCreateDto, Notification>()
                .ForMember(d => d.UserId,   o => o.MapFrom(s => s.UserId))
                .ForMember(d => d.Title,    o => o.MapFrom(s => s.Title))
                .ForMember(d => d.Message,  o => o.MapFrom(s => s.Message));

            CreateMap<NotificationUpdateDto, Notification>()
                .ForMember(d => d.NotificationId, o => o.MapFrom(s => s.NotificationId))
                .ForMember(d => d.UserId,         o => o.MapFrom(s => s.UserId))
                .ForMember(d => d.Title,          o => o.MapFrom(s => s.Title))
                .ForMember(d => d.Message,        o => o.MapFrom(s => s.Message));

            CreateMap<NotificationLogCreateDto, NotificationLog>()
                .ForMember(d => d.UserId, o => o.MapFrom(s => s.UserId))
                .ForMember(d => d.Type,   o => o.MapFrom(s => s.Type))
                .ForMember(d => d.Message,o => o.MapFrom(s => s.Message))
                .ForMember(d => d.SentAt, o => o.MapFrom(s => s.SentAt));


            //
            // === RECOMMENDATION ===
            //
            CreateMap<Recommendation, RecommendationReadDto>()
                .ForMember(d => d.BookId,  o => o.MapFrom(s => s.BookId))
                .ForMember(d => d.Title,   o => o.MapFrom(s => s.Book.Title))
                .ForMember(d => d.Genre,   o => o.MapFrom(s => s.Book.Genre.Name))
                .ForMember(d => d.Author,  o => o.MapFrom(s => s.Book.Author.Name))
                .ForMember(d => d.CoverUrl,o => o.MapFrom(s => s.Book.CoverUrl));


            //
            // === REPORT ===
            //
            CreateMap<Report, ReportReadDto>()
                .ForMember(d => d.ReportId,     o => o.MapFrom(s => s.ReportId))
                .ForMember(d => d.Title,        o => o.MapFrom(s => s.Title))
                .ForMember(d => d.Content,      o => o.MapFrom(s => s.Content))
                .ForMember(d => d.GeneratedDate,o => o.MapFrom(s => s.GeneratedAt))
                .ForMember(d => d.UserId,       o => o.MapFrom(s => s.UserId))
                .ForMember(d => d.UserName,     o => o.MapFrom(s => s.User.FullName));

            CreateMap<ReportCreateDto, Report>()
                .ForMember(d => d.Title,         o => o.MapFrom(s => s.Title))
                .ForMember(d => d.Content,       o => o.Ignore())
                .ForMember(d => d.GeneratedAt,   o => o.Ignore())
                .ForMember(d => d.UserId,        o => o.Ignore());

            CreateMap<ReportUpdateDto, Report>()
                .ForMember(d => d.ReportId,      o => o.MapFrom(s => s.ReportId))
                .ForMember(d => d.Title,         o => o.MapFrom(s => s.Title))
                .ForMember(d => d.Content,       o => o.MapFrom(s => s.Content))
                .ForMember(d => d.GeneratedAt,   o => o.Ignore())
                .ForMember(d => d.UserId,        o => o.Ignore());


            //
            // === RESERVATION ===
            //
            CreateMap<Reservation, ReservationReadDto>()
                .ForMember(d => d.ReservationId,   o => o.MapFrom(s => s.ReservationId))
                .ForMember(d => d.UserId,          o => o.MapFrom(s => s.UserId))
                .ForMember(d => d.UserName,        o => o.MapFrom(s => s.User.FullName))
                .ForMember(d => d.BookId,          o => o.MapFrom(s => s.BookId))
                .ForMember(d => d.BookTitle,       o => o.MapFrom(s => s.Book.Title))
                .ForMember(d => d.ReservationDate, o => o.MapFrom(s => s.CreatedAt))
                .ForMember(d => d.Status,          o => o.MapFrom(s => s.Status));

            CreateMap<ReservationCreateDto, Reservation>()
                .ForMember(d => d.UserId,        o => o.MapFrom(s => s.UserId))
                .ForMember(d => d.BookId,        o => o.MapFrom(s => s.BookId))
                .ForMember(d => d.Status,        o => o.MapFrom(_ => ReservationStatus.Pending))
                .ForMember(d => d.CreatedAt,     o => o.MapFrom(_ => DateTime.UtcNow));

            CreateMap<ReservationUpdateDto, Reservation>()
                .ForMember(d => d.ReservationId, o => o.MapFrom(s => s.ReservationId))
                .ForMember(d => d.UserId,        o => o.MapFrom(s => s.UserId))
                .ForMember(d => d.BookId,        o => o.MapFrom(s => s.BookId))
                .ForMember(d => d.CreatedAt,     o => o.MapFrom(s => s.ReservationDate))
                .ForMember(d => d.Status,        o => o.MapFrom(s => s.Status));


            //
            // === SHELF LEVEL ===
            //
            CreateMap<ShelfLevel, ShelfLevelReadDto>()
                .ForMember(d => d.ShelfLevelId, o => o.MapFrom(s => s.ShelfLevelId))
                .ForMember(d => d.LevelNumber,  o => o.MapFrom(s => s.LevelNumber))
                .ForMember(d => d.ShelfId,      o => o.MapFrom(s => s.ShelfId))
                .ForMember(d => d.ShelfName,    o => o.MapFrom(s => s.Shelf.Name))
                .ForMember(d => d.MaxHeight,    o => o.MapFrom(s => s.MaxHeight ?? 0))
                .ForMember(d => d.Capacity,     o => o.MapFrom(s => s.Capacity ?? 0))
                .ForMember(d => d.CurrentLoad,  o => o.MapFrom(s => s.CurrentLoad ?? 0));

            CreateMap<ShelfLevelCreateDto, ShelfLevel>()
                .ForMember(d => d.LevelNumber, o => o.MapFrom(s => s.LevelNumber))
                .ForMember(d => d.ShelfId,     o => o.MapFrom(s => s.ShelfId))
                .ForMember(d => d.MaxHeight,   o => o.MapFrom(s => s.MaxHeight))
                .ForMember(d => d.Capacity,    o => o.MapFrom(s => s.Capacity))
                .ForMember(d => d.CurrentLoad, o => o.MapFrom(s => s.CurrentLoad));

            CreateMap<ShelfLevelUpdateDto, ShelfLevel>()
                .ForMember(d => d.ShelfLevelId, o => o.MapFrom(s => s.ShelfLevelId))
                .ForMember(d => d.LevelNumber,  o => o.MapFrom(s => s.LevelNumber))
                .ForMember(d => d.ShelfId,      o => o.MapFrom(s => s.ShelfId))
                .ForMember(d => d.MaxHeight,    o => o.MapFrom(s => s.MaxHeight))
                .ForMember(d => d.Capacity,     o => o.MapFrom(s => s.Capacity))
                .ForMember(d => d.CurrentLoad,  o => o.MapFrom(s => s.CurrentLoad));


            //
            // === SHELF ===
            //
            CreateMap<Shelf, ShelfReadDto>()
                .ForMember(d => d.ShelfId,     o => o.MapFrom(s => s.ShelfId))
                .ForMember(d => d.Name,        o => o.MapFrom(s => s.Name))
                .ForMember(d => d.ZoneId,      o => o.MapFrom(s => s.ZoneId))
                .ForMember(d => d.ZoneName,    o => o.MapFrom(s => s.Zone.Name))
                .ForMember(d => d.GenreId,     o => o.MapFrom(s => s.GenreId))
                .ForMember(d => d.GenreName,   o => o.MapFrom(s => s.Genre.Name))
                .ForMember(d => d.Capacity,    o => o.MapFrom(s => s.Capacity))
                .ForMember(d => d.CurrentLoad, o => o.MapFrom(s => s.ShelfLevels.Sum(sl => sl.CurrentLoad ?? 0)));

            CreateMap<ShelfCreateDto, Shelf>()
                .ForMember(d => d.ZoneId,   o => o.MapFrom(s => s.ZoneId))
                .ForMember(d => d.GenreId,  o => o.MapFrom(s => s.GenreId))
                .ForMember(d => d.Name,     o => o.MapFrom(s => s.Name))
                .ForMember(d => d.Capacity, o => o.MapFrom(s => s.Capacity));

            CreateMap<ShelfUpdateDto, Shelf>()
                .ForMember(d => d.ShelfId,  o => o.MapFrom(s => s.ShelfId))
                .ForMember(d => d.ZoneId,   o => o.MapFrom(s => s.ZoneId))
                .ForMember(d => d.GenreId,  o => o.MapFrom(s => s.GenreId))
                .ForMember(d => d.Name,     o => o.MapFrom(s => s.Name))
                .ForMember(d => d.Capacity, o => o.MapFrom(s => s.Capacity));


            //
            // === STOCK ===
            //
            CreateMap<Stock, StockReadDto>()
                .ForMember(d => d.StockId,    o => o.MapFrom(s => s.StockId))
                .ForMember(d => d.BookId,     o => o.MapFrom(s => s.BookId))
                .ForMember(d => d.BookTitle,  o => o.MapFrom(s => s.Book.Title))
                .ForMember(d => d.Quantity,   o => o.MapFrom(s => s.Quantity))
                .ForMember(d => d.IsAvailable,o => o.MapFrom(s => s.IsAvailable));

            CreateMap<StockCreateDto, Stock>()
                .ForMember(d => d.BookId,      o => o.MapFrom(s => s.BookId))
                .ForMember(d => d.Quantity,    o => o.MapFrom(s => s.Quantity))
                .ForMember(d => d.IsAvailable, o => o.MapFrom(s => s.Quantity > 0));

            CreateMap<StockAdjustmentDto, Stock>()
                .ForMember(d => d.Quantity,    o => o.MapFrom((src, dest) => dest.Quantity + src.Adjustment))
                .ForMember(d => d.IsAvailable, o => o.MapFrom((src, dest) => (dest.Quantity + src.Adjustment) > 0));


            //
            // === TAG ===
            //
            CreateMap<Tag, TagReadDto>()
                .ForMember(d => d.TagId, o => o.MapFrom(s => s.TagId))
                .ForMember(d => d.Name,  o => o.MapFrom(s => s.Name));

            CreateMap<TagCreateDto, Tag>()
                .ForMember(d => d.Name, o => o.MapFrom(s => s.Name));

            CreateMap<TagUpdateDto, Tag>()
                .ForMember(d => d.TagId, o => o.MapFrom(s => s.TagId))
                .ForMember(d => d.Name,  o => o.MapFrom(s => s.Name));
        }
    }
}