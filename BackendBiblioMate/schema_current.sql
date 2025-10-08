CREATE TABLE [Authors] (
    [AuthorId] int NOT NULL IDENTITY,
    [Name] nvarchar(100) NOT NULL,
    CONSTRAINT [PK_Authors] PRIMARY KEY ([AuthorId])
);
GO


CREATE TABLE [Editors] (
    [EditorId] int NOT NULL IDENTITY,
    [Name] nvarchar(100) NOT NULL,
    CONSTRAINT [PK_Editors] PRIMARY KEY ([EditorId])
);
GO


CREATE TABLE [Genres] (
    [GenreId] int NOT NULL IDENTITY,
    [Name] nvarchar(100) NOT NULL,
    CONSTRAINT [PK_Genres] PRIMARY KEY ([GenreId])
);
GO


CREATE TABLE [Tags] (
    [TagId] int NOT NULL IDENTITY,
    [Name] nvarchar(50) NOT NULL,
    CONSTRAINT [PK_Tags] PRIMARY KEY ([TagId])
);
GO


CREATE TABLE [Users] (
    [UserId] int NOT NULL IDENTITY,
    [FirstName] nvarchar(60) NOT NULL,
    [LastName] nvarchar(60) NOT NULL,
    [Email] nvarchar(100) NOT NULL,
    [Password] nvarchar(100) NOT NULL,
    [Address1] nvarchar(200) NULL,
    [Address2] nvarchar(200) NULL,
    [Phone] nvarchar(max) NULL,
    [DateOfBirth] datetime2 NULL,
    [ProfileImagePath] nvarchar(2000) NULL,
    [Role] nvarchar(20) NOT NULL,
    [IsEmailConfirmed] bit NOT NULL,
    [EmailConfirmationToken] nvarchar(1000) NULL,
    [PasswordResetToken] nvarchar(1000) NULL,
    [PasswordResetTokenExpires] datetime2 NULL,
    [IsApproved] bit NOT NULL,
    [SecurityStamp] nvarchar(1000) NOT NULL,
    CONSTRAINT [PK_Users] PRIMARY KEY ([UserId])
);
GO


CREATE TABLE [Zones] (
    [ZoneId] int NOT NULL IDENTITY,
    [Name] nvarchar(100) NOT NULL,
    [FloorNumber] int NOT NULL,
    [AisleCode] nvarchar(20) NOT NULL,
    [Description] nvarchar(255) NULL,
    CONSTRAINT [PK_Zones] PRIMARY KEY ([ZoneId])
);
GO


CREATE TABLE [Notifications] (
    [NotificationId] int NOT NULL IDENTITY,
    [UserId] int NOT NULL,
    [Title] nvarchar(200) NOT NULL,
    [Message] nvarchar(1000) NOT NULL,
    [Timestamp] datetime2 NOT NULL,
    CONSTRAINT [PK_Notifications] PRIMARY KEY ([NotificationId]),
    CONSTRAINT [FK_Notifications_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([UserId]) ON DELETE CASCADE
);
GO


CREATE TABLE [Reports] (
    [ReportId] int NOT NULL IDENTITY,
    [UserId] int NOT NULL,
    [Title] nvarchar(255) NOT NULL,
    [Content] nvarchar(1000) NOT NULL,
    [GeneratedDate] datetime2 NOT NULL,
    CONSTRAINT [PK_Reports] PRIMARY KEY ([ReportId]),
    CONSTRAINT [FK_Reports_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([UserId]) ON DELETE CASCADE
);
GO


CREATE TABLE [UserGenres] (
    [UserId] int NOT NULL,
    [GenreId] int NOT NULL,
    CONSTRAINT [PK_UserGenres] PRIMARY KEY ([UserId], [GenreId]),
    CONSTRAINT [FK_UserGenres_Genres_GenreId] FOREIGN KEY ([GenreId]) REFERENCES [Genres] ([GenreId]) ON DELETE CASCADE,
    CONSTRAINT [FK_UserGenres_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([UserId]) ON DELETE CASCADE
);
GO


CREATE TABLE [Shelves] (
    [ShelfId] int NOT NULL IDENTITY,
    [ZoneId] int NOT NULL,
    [GenreId] int NOT NULL,
    [Name] nvarchar(100) NOT NULL,
    [Capacity] int NOT NULL,
    [CurrentLoad] int NOT NULL,
    CONSTRAINT [PK_Shelves] PRIMARY KEY ([ShelfId]),
    CONSTRAINT [FK_Shelves_Genres_GenreId] FOREIGN KEY ([GenreId]) REFERENCES [Genres] ([GenreId]) ON DELETE CASCADE,
    CONSTRAINT [FK_Shelves_Zones_ZoneId] FOREIGN KEY ([ZoneId]) REFERENCES [Zones] ([ZoneId]) ON DELETE CASCADE
);
GO


CREATE TABLE [ShelfLevels] (
    [ShelfLevelId] int NOT NULL IDENTITY,
    [ShelfId] int NOT NULL,
    [LevelNumber] int NOT NULL,
    [MaxHeight] int NOT NULL,
    [Capacity] int NOT NULL,
    [CurrentLoad] int NOT NULL,
    CONSTRAINT [PK_ShelfLevels] PRIMARY KEY ([ShelfLevelId]),
    CONSTRAINT [FK_ShelfLevels_Shelves_ShelfId] FOREIGN KEY ([ShelfId]) REFERENCES [Shelves] ([ShelfId]) ON DELETE CASCADE
);
GO


CREATE TABLE [Books] (
    [BookId] int NOT NULL IDENTITY,
    [Title] nvarchar(255) NOT NULL,
    [Isbn] nvarchar(13) NOT NULL,
    [Description] nvarchar(4000) NULL,
    [PublicationDate] datetime2 NOT NULL,
    [AuthorId] int NOT NULL,
    [GenreId] int NOT NULL,
    [EditorId] int NOT NULL,
    [ShelfLevelId] int NOT NULL,
    [CoverUrl] nvarchar(2048) NULL,
    CONSTRAINT [PK_Books] PRIMARY KEY ([BookId]),
    CONSTRAINT [FK_Books_Authors_AuthorId] FOREIGN KEY ([AuthorId]) REFERENCES [Authors] ([AuthorId]) ON DELETE NO ACTION,
    CONSTRAINT [FK_Books_Editors_EditorId] FOREIGN KEY ([EditorId]) REFERENCES [Editors] ([EditorId]) ON DELETE NO ACTION,
    CONSTRAINT [FK_Books_Genres_GenreId] FOREIGN KEY ([GenreId]) REFERENCES [Genres] ([GenreId]) ON DELETE NO ACTION,
    CONSTRAINT [FK_Books_ShelfLevels_ShelfLevelId] FOREIGN KEY ([ShelfLevelId]) REFERENCES [ShelfLevels] ([ShelfLevelId]) ON DELETE CASCADE
);
GO


CREATE TABLE [BookTags] (
    [BookId] int NOT NULL,
    [TagId] int NOT NULL,
    CONSTRAINT [PK_BookTags] PRIMARY KEY ([BookId], [TagId]),
    CONSTRAINT [FK_BookTags_Books_BookId] FOREIGN KEY ([BookId]) REFERENCES [Books] ([BookId]) ON DELETE CASCADE,
    CONSTRAINT [FK_BookTags_Tags_TagId] FOREIGN KEY ([TagId]) REFERENCES [Tags] ([TagId]) ON DELETE CASCADE
);
GO


CREATE TABLE [Recommendation] (
    [RecommendationId] int NOT NULL IDENTITY,
    [UserId] int NOT NULL,
    [BookId] int NOT NULL,
    CONSTRAINT [PK_Recommendation] PRIMARY KEY ([RecommendationId]),
    CONSTRAINT [FK_Recommendation_Books_BookId] FOREIGN KEY ([BookId]) REFERENCES [Books] ([BookId]) ON DELETE CASCADE,
    CONSTRAINT [FK_Recommendation_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([UserId]) ON DELETE CASCADE
);
GO


CREATE TABLE [Reservations] (
    [ReservationId] int NOT NULL IDENTITY,
    [BookId] int NOT NULL,
    [UserId] int NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [ReservationDate] datetime2 NOT NULL,
    [Status] int NOT NULL,
    [AssignedStockId] int NULL,
    [AvailableAt] datetime2 NULL,
    CONSTRAINT [PK_Reservations] PRIMARY KEY ([ReservationId]),
    CONSTRAINT [FK_Reservations_Books_BookId] FOREIGN KEY ([BookId]) REFERENCES [Books] ([BookId]) ON DELETE CASCADE,
    CONSTRAINT [FK_Reservations_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([UserId]) ON DELETE CASCADE
);
GO


CREATE TABLE [Stocks] (
    [StockId] int NOT NULL IDENTITY,
    [BookId] int NOT NULL,
    [Quantity] int NOT NULL,
    CONSTRAINT [PK_Stocks] PRIMARY KEY ([StockId]),
    CONSTRAINT [FK_Stocks_Books_BookId] FOREIGN KEY ([BookId]) REFERENCES [Books] ([BookId]) ON DELETE CASCADE
);
GO


CREATE TABLE [Loans] (
    [LoanId] int NOT NULL IDENTITY,
    [BookId] int NOT NULL,
    [UserId] int NOT NULL,
    [LoanDate] datetime2 NOT NULL,
    [DueDate] datetime2 NOT NULL,
    [ReturnDate] datetime2 NULL,
    [Fine] decimal(10,2) NOT NULL,
    [StockId] int NOT NULL,
    [ExtensionsCount] int NOT NULL,
    CONSTRAINT [PK_Loans] PRIMARY KEY ([LoanId]),
    CONSTRAINT [FK_Loans_Books_BookId] FOREIGN KEY ([BookId]) REFERENCES [Books] ([BookId]) ON DELETE CASCADE,
    CONSTRAINT [FK_Loans_Stocks_StockId] FOREIGN KEY ([StockId]) REFERENCES [Stocks] ([StockId]),
    CONSTRAINT [FK_Loans_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([UserId]) ON DELETE CASCADE
);
GO


CREATE TABLE [Histories] (
    [HistoryId] int NOT NULL IDENTITY,
    [UserId] int NOT NULL,
    [LoanId] int NULL,
    [ReservationId] int NULL,
    [EventDate] datetime2 NOT NULL,
    [EventType] nvarchar(50) NOT NULL,
    CONSTRAINT [PK_Histories] PRIMARY KEY ([HistoryId]),
    CONSTRAINT [FK_Histories_Loans_LoanId] FOREIGN KEY ([LoanId]) REFERENCES [Loans] ([LoanId]),
    CONSTRAINT [FK_Histories_Reservations_ReservationId] FOREIGN KEY ([ReservationId]) REFERENCES [Reservations] ([ReservationId]),
    CONSTRAINT [FK_Histories_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([UserId]) ON DELETE CASCADE
);
GO


CREATE UNIQUE INDEX [IX_Authors_Name] ON [Authors] ([Name]);
GO


CREATE INDEX [IX_Books_AuthorId] ON [Books] ([AuthorId]);
GO


CREATE INDEX [IX_Books_EditorId] ON [Books] ([EditorId]);
GO


CREATE INDEX [IX_Books_GenreId] ON [Books] ([GenreId]);
GO


CREATE UNIQUE INDEX [IX_Books_Isbn] ON [Books] ([Isbn]);
GO


CREATE INDEX [IX_Books_PublicationDate] ON [Books] ([PublicationDate]);
GO


CREATE INDEX [IX_Books_ShelfLevelId] ON [Books] ([ShelfLevelId]);
GO


CREATE INDEX [IX_Books_Title] ON [Books] ([Title]);
GO


CREATE INDEX [IX_BookTags_TagId] ON [BookTags] ([TagId]);
GO


CREATE UNIQUE INDEX [IX_Editors_Name] ON [Editors] ([Name]);
GO


CREATE UNIQUE INDEX [IX_Genres_Name] ON [Genres] ([Name]);
GO


CREATE INDEX [IX_Histories_LoanId] ON [Histories] ([LoanId]);
GO


CREATE INDEX [IX_Histories_ReservationId] ON [Histories] ([ReservationId]);
GO


CREATE INDEX [IX_Histories_UserId] ON [Histories] ([UserId]);
GO


CREATE INDEX [IX_Loans_BookId] ON [Loans] ([BookId]);
GO


CREATE INDEX [IX_Loans_StockId] ON [Loans] ([StockId]);
GO


CREATE INDEX [IX_Loans_UserId] ON [Loans] ([UserId]);
GO


CREATE INDEX [IX_Notifications_UserId] ON [Notifications] ([UserId]);
GO


CREATE INDEX [IX_Recommendation_BookId] ON [Recommendation] ([BookId]);
GO


CREATE INDEX [IX_Recommendation_UserId] ON [Recommendation] ([UserId]);
GO


CREATE INDEX [IX_Reports_UserId] ON [Reports] ([UserId]);
GO


CREATE INDEX [IX_Reservations_BookId] ON [Reservations] ([BookId]);
GO


CREATE INDEX [IX_Reservations_UserId] ON [Reservations] ([UserId]);
GO


CREATE INDEX [IX_ShelfLevels_ShelfId] ON [ShelfLevels] ([ShelfId]);
GO


CREATE INDEX [IX_Shelves_GenreId] ON [Shelves] ([GenreId]);
GO


CREATE INDEX [IX_Shelves_ZoneId] ON [Shelves] ([ZoneId]);
GO


CREATE UNIQUE INDEX [IX_Stocks_BookId] ON [Stocks] ([BookId]);
GO


CREATE INDEX [IX_UserGenres_GenreId] ON [UserGenres] ([GenreId]);
GO


CREATE UNIQUE INDEX [IX_Users_Email] ON [Users] ([Email]);
GO


