IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
CREATE TABLE [Authors] (
    [AuthorId] int NOT NULL IDENTITY,
    [Name] nvarchar(100) NOT NULL,
    CONSTRAINT [PK_Authors] PRIMARY KEY ([AuthorId])
);

CREATE TABLE [Editors] (
    [EditorId] int NOT NULL IDENTITY,
    [Name] nvarchar(100) NOT NULL,
    CONSTRAINT [PK_Editors] PRIMARY KEY ([EditorId])
);

CREATE TABLE [Genres] (
    [GenreId] int NOT NULL IDENTITY,
    [Name] nvarchar(100) NOT NULL,
    CONSTRAINT [PK_Genres] PRIMARY KEY ([GenreId])
);

CREATE TABLE [Tags] (
    [TagId] int NOT NULL IDENTITY,
    [Name] nvarchar(50) NOT NULL,
    CONSTRAINT [PK_Tags] PRIMARY KEY ([TagId])
);

CREATE TABLE [Users] (
    [UserId] int NOT NULL IDENTITY,
    [Name] nvarchar(100) NOT NULL,
    [Email] nvarchar(100) NOT NULL,
    [Password] nvarchar(100) NOT NULL,
    [Address] nvarchar(200) NOT NULL,
    [Phone] nvarchar(max) NOT NULL,
    [Role] nvarchar(max) NOT NULL,
    [IsEmailConfirmed] bit NOT NULL,
    [EmailConfirmationToken] nvarchar(1000) NULL,
    [PasswordResetToken] nvarchar(1000) NULL,
    [PasswordResetTokenExpires] datetime2 NULL,
    [IsApproved] bit NOT NULL,
    CONSTRAINT [PK_Users] PRIMARY KEY ([UserId])
);

CREATE TABLE [Zones] (
    [ZoneId] int NOT NULL IDENTITY,
    [Name] nvarchar(100) NOT NULL,
    [FloorNumber] int NOT NULL,
    [AisleCode] nvarchar(20) NOT NULL,
    [Description] nvarchar(255) NULL,
    CONSTRAINT [PK_Zones] PRIMARY KEY ([ZoneId])
);

CREATE TABLE [Notifications] (
    [NotificationId] int NOT NULL IDENTITY,
    [UserId] int NOT NULL,
    [Title] nvarchar(100) NOT NULL,
    [Message] nvarchar(255) NOT NULL,
    [Timestamp] datetime2 NOT NULL,
    CONSTRAINT [PK_Notifications] PRIMARY KEY ([NotificationId]),
    CONSTRAINT [FK_Notifications_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([UserId]) ON DELETE CASCADE
);

CREATE TABLE [Reports] (
    [ReportId] int NOT NULL IDENTITY,
    [UserId] int NOT NULL,
    [Title] nvarchar(255) NOT NULL,
    [Content] nvarchar(1000) NOT NULL,
    [GeneratedDate] datetime2 NOT NULL,
    CONSTRAINT [PK_Reports] PRIMARY KEY ([ReportId]),
    CONSTRAINT [FK_Reports_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([UserId]) ON DELETE CASCADE
);

CREATE TABLE [UserGenres] (
    [UserId] int NOT NULL,
    [GenreId] int NOT NULL,
    CONSTRAINT [PK_UserGenres] PRIMARY KEY ([UserId], [GenreId]),
    CONSTRAINT [FK_UserGenres_Genres_GenreId] FOREIGN KEY ([GenreId]) REFERENCES [Genres] ([GenreId]) ON DELETE CASCADE,
    CONSTRAINT [FK_UserGenres_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([UserId]) ON DELETE CASCADE
);

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

CREATE TABLE [Books] (
    [BookId] int NOT NULL IDENTITY,
    [Title] nvarchar(255) NOT NULL,
    [Isbn] nvarchar(13) NOT NULL,
    [PublicationDate] datetime2 NOT NULL,
    [AuthorId] int NOT NULL,
    [GenreId] int NOT NULL,
    [EditorId] int NOT NULL,
    [ShelfLevelId] int NOT NULL,
    [CoverUrl] nvarchar(max) NULL,
    CONSTRAINT [PK_Books] PRIMARY KEY ([BookId]),
    CONSTRAINT [FK_Books_Authors_AuthorId] FOREIGN KEY ([AuthorId]) REFERENCES [Authors] ([AuthorId]) ON DELETE NO ACTION,
    CONSTRAINT [FK_Books_Editors_EditorId] FOREIGN KEY ([EditorId]) REFERENCES [Editors] ([EditorId]) ON DELETE NO ACTION,
    CONSTRAINT [FK_Books_Genres_GenreId] FOREIGN KEY ([GenreId]) REFERENCES [Genres] ([GenreId]) ON DELETE NO ACTION,
    CONSTRAINT [FK_Books_ShelfLevels_ShelfLevelId] FOREIGN KEY ([ShelfLevelId]) REFERENCES [ShelfLevels] ([ShelfLevelId]) ON DELETE CASCADE
);

CREATE TABLE [BookTags] (
    [BookId] int NOT NULL,
    [TagId] int NOT NULL,
    CONSTRAINT [PK_BookTags] PRIMARY KEY ([BookId], [TagId]),
    CONSTRAINT [FK_BookTags_Books_BookId] FOREIGN KEY ([BookId]) REFERENCES [Books] ([BookId]) ON DELETE CASCADE,
    CONSTRAINT [FK_BookTags_Tags_TagId] FOREIGN KEY ([TagId]) REFERENCES [Tags] ([TagId]) ON DELETE CASCADE
);

CREATE TABLE [Recommendation] (
    [RecommendationId] int NOT NULL IDENTITY,
    [UserId] int NOT NULL,
    [RecommendationBookId] int NOT NULL,
    CONSTRAINT [PK_Recommendation] PRIMARY KEY ([RecommendationId]),
    CONSTRAINT [FK_Recommendation_Books_RecommendationBookId] FOREIGN KEY ([RecommendationBookId]) REFERENCES [Books] ([BookId]) ON DELETE CASCADE,
    CONSTRAINT [FK_Recommendation_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([UserId]) ON DELETE CASCADE
);

CREATE TABLE [Reservations] (
    [ReservationId] int NOT NULL IDENTITY,
    [BookId] int NOT NULL,
    [UserId] int NOT NULL,
    [ReservationDate] datetime2 NOT NULL,
    [Status] int NOT NULL,
    [AssignedStockId] int NULL,
    [AvailableAt] datetime2 NULL,
    [CreatedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_Reservations] PRIMARY KEY ([ReservationId]),
    CONSTRAINT [FK_Reservations_Books_BookId] FOREIGN KEY ([BookId]) REFERENCES [Books] ([BookId]) ON DELETE CASCADE,
    CONSTRAINT [FK_Reservations_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([UserId]) ON DELETE CASCADE
);

CREATE TABLE [Stocks] (
    [StockId] int NOT NULL IDENTITY,
    [BookId] int NOT NULL,
    [Quantity] int NOT NULL,
    [IsAvailable] bit NOT NULL,
    CONSTRAINT [PK_Stocks] PRIMARY KEY ([StockId]),
    CONSTRAINT [FK_Stocks_Books_BookId] FOREIGN KEY ([BookId]) REFERENCES [Books] ([BookId]) ON DELETE CASCADE
);

CREATE TABLE [Loans] (
    [LoanId] int NOT NULL IDENTITY,
    [BookId] int NOT NULL,
    [UserId] int NOT NULL,
    [LoanDate] datetime2 NOT NULL,
    [DueDate] datetime2 NOT NULL,
    [ReturnDate] datetime2 NULL,
    [Fine] decimal(10,2) NOT NULL,
    [StockId] int NOT NULL,
    CONSTRAINT [PK_Loans] PRIMARY KEY ([LoanId]),
    CONSTRAINT [FK_Loans_Books_BookId] FOREIGN KEY ([BookId]) REFERENCES [Books] ([BookId]) ON DELETE CASCADE,
    CONSTRAINT [FK_Loans_Stocks_StockId] FOREIGN KEY ([StockId]) REFERENCES [Stocks] ([StockId]),
    CONSTRAINT [FK_Loans_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([UserId]) ON DELETE CASCADE
);

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

CREATE UNIQUE INDEX [IX_Authors_Name] ON [Authors] ([Name]);

CREATE INDEX [IX_Books_AuthorId] ON [Books] ([AuthorId]);

CREATE INDEX [IX_Books_EditorId] ON [Books] ([EditorId]);

CREATE INDEX [IX_Books_GenreId] ON [Books] ([GenreId]);

CREATE UNIQUE INDEX [IX_Books_Isbn] ON [Books] ([Isbn]);

CREATE INDEX [IX_Books_PublicationDate] ON [Books] ([PublicationDate]);

CREATE INDEX [IX_Books_ShelfLevelId] ON [Books] ([ShelfLevelId]);

CREATE INDEX [IX_Books_Title] ON [Books] ([Title]);

CREATE INDEX [IX_BookTags_TagId] ON [BookTags] ([TagId]);

CREATE UNIQUE INDEX [IX_Editors_Name] ON [Editors] ([Name]);

CREATE UNIQUE INDEX [IX_Genres_Name] ON [Genres] ([Name]);

CREATE INDEX [IX_Histories_LoanId] ON [Histories] ([LoanId]);

CREATE INDEX [IX_Histories_ReservationId] ON [Histories] ([ReservationId]);

CREATE INDEX [IX_Histories_UserId] ON [Histories] ([UserId]);

CREATE INDEX [IX_Loans_BookId] ON [Loans] ([BookId]);

CREATE INDEX [IX_Loans_StockId] ON [Loans] ([StockId]);

CREATE INDEX [IX_Loans_UserId] ON [Loans] ([UserId]);

CREATE INDEX [IX_Notifications_UserId] ON [Notifications] ([UserId]);

CREATE INDEX [IX_Recommendation_RecommendationBookId] ON [Recommendation] ([RecommendationBookId]);

CREATE INDEX [IX_Recommendation_UserId] ON [Recommendation] ([UserId]);

CREATE INDEX [IX_Reports_UserId] ON [Reports] ([UserId]);

CREATE INDEX [IX_Reservations_BookId] ON [Reservations] ([BookId]);

CREATE INDEX [IX_Reservations_UserId] ON [Reservations] ([UserId]);

CREATE INDEX [IX_ShelfLevels_ShelfId] ON [ShelfLevels] ([ShelfId]);

CREATE INDEX [IX_Shelves_GenreId] ON [Shelves] ([GenreId]);

CREATE INDEX [IX_Shelves_ZoneId] ON [Shelves] ([ZoneId]);

CREATE UNIQUE INDEX [IX_Stocks_BookId] ON [Stocks] ([BookId]);

CREATE INDEX [IX_UserGenres_GenreId] ON [UserGenres] ([GenreId]);

CREATE UNIQUE INDEX [IX_Users_Email] ON [Users] ([Email]);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250623192719_InitialCreate', N'9.0.6');

DECLARE @var sysname;
SELECT @var = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Users]') AND [c].[name] = N'Role');
IF @var IS NOT NULL EXEC(N'ALTER TABLE [Users] DROP CONSTRAINT [' + @var + '];');
ALTER TABLE [Users] ALTER COLUMN [Role] nvarchar(20) NOT NULL;

ALTER TABLE [Users] ADD [SecurityStamp] nvarchar(1000) NOT NULL DEFAULT N'';

DECLARE @var1 sysname;
SELECT @var1 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Books]') AND [c].[name] = N'CoverUrl');
IF @var1 IS NOT NULL EXEC(N'ALTER TABLE [Books] DROP CONSTRAINT [' + @var1 + '];');
ALTER TABLE [Books] ALTER COLUMN [CoverUrl] nvarchar(2048) NULL;

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250626195921_AddSecurityStampToUser', N'9.0.6');

DECLARE @var2 sysname;
SELECT @var2 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Stocks]') AND [c].[name] = N'IsAvailable');
IF @var2 IS NOT NULL EXEC(N'ALTER TABLE [Stocks] DROP CONSTRAINT [' + @var2 + '];');
ALTER TABLE [Stocks] DROP COLUMN [IsAvailable];

ALTER TABLE [Books] ADD [Description] nvarchar(4000) NULL;

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250713134630_AddBookDescriptionToBook', N'9.0.6');

DECLARE @var3 sysname;
SELECT @var3 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Users]') AND [c].[name] = N'Name');
IF @var3 IS NOT NULL EXEC(N'ALTER TABLE [Users] DROP CONSTRAINT [' + @var3 + '];');
ALTER TABLE [Users] DROP COLUMN [Name];

EXEC sp_rename N'[Users].[Address]', N'Address1', 'COLUMN';

ALTER TABLE [Users] ADD [Address2] nvarchar(200) NULL;

ALTER TABLE [Users] ADD [DateOfBirth] datetime2 NULL;

ALTER TABLE [Users] ADD [FirstName] nvarchar(60) NOT NULL DEFAULT N'';

ALTER TABLE [Users] ADD [LastName] nvarchar(60) NOT NULL DEFAULT N'';

ALTER TABLE [Users] ADD [ProfileImagePath] nvarchar(1000) NULL;

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250809130424_UpdateUserEntity', N'9.0.6');

ALTER TABLE [Loans] ADD [ExtensionsCount] int NOT NULL DEFAULT 0;

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250924150305_AddExtensionsCountToLoan', N'9.0.6');

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250924204048_CheckNoChanges', N'9.0.6');

ALTER TABLE [Recommendation] DROP CONSTRAINT [FK_Recommendation_Books_RecommendationBookId];

EXEC sp_rename N'[Recommendation].[RecommendationBookId]', N'BookId', 'COLUMN';

EXEC sp_rename N'[Recommendation].[IX_Recommendation_RecommendationBookId]', N'IX_Recommendation_BookId', 'INDEX';

DECLARE @var4 sysname;
SELECT @var4 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Users]') AND [c].[name] = N'ProfileImagePath');
IF @var4 IS NOT NULL EXEC(N'ALTER TABLE [Users] DROP CONSTRAINT [' + @var4 + '];');
ALTER TABLE [Users] ALTER COLUMN [ProfileImagePath] nvarchar(2000) NULL;

DECLARE @var5 sysname;
SELECT @var5 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Users]') AND [c].[name] = N'Phone');
IF @var5 IS NOT NULL EXEC(N'ALTER TABLE [Users] DROP CONSTRAINT [' + @var5 + '];');
ALTER TABLE [Users] ALTER COLUMN [Phone] nvarchar(max) NULL;

DECLARE @var6 sysname;
SELECT @var6 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Users]') AND [c].[name] = N'Address1');
IF @var6 IS NOT NULL EXEC(N'ALTER TABLE [Users] DROP CONSTRAINT [' + @var6 + '];');
ALTER TABLE [Users] ALTER COLUMN [Address1] nvarchar(200) NULL;

DECLARE @var7 sysname;
SELECT @var7 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Notifications]') AND [c].[name] = N'Title');
IF @var7 IS NOT NULL EXEC(N'ALTER TABLE [Notifications] DROP CONSTRAINT [' + @var7 + '];');
ALTER TABLE [Notifications] ALTER COLUMN [Title] nvarchar(200) NOT NULL;

DECLARE @var8 sysname;
SELECT @var8 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Notifications]') AND [c].[name] = N'Message');
IF @var8 IS NOT NULL EXEC(N'ALTER TABLE [Notifications] DROP CONSTRAINT [' + @var8 + '];');
ALTER TABLE [Notifications] ALTER COLUMN [Message] nvarchar(1000) NOT NULL;

ALTER TABLE [Recommendation] ADD CONSTRAINT [FK_Recommendation_Books_BookId] FOREIGN KEY ([BookId]) REFERENCES [Books] ([BookId]) ON DELETE CASCADE;

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250927220714_TentativeDébug', N'9.0.6');

COMMIT;
GO

