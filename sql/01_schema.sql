IF DB_ID(N'IMDB') IS NULL
BEGIN
    CREATE DATABASE IMDB;
END;
GO

USE IMDB;
GO

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = N'staging')
BEGIN
    EXEC('CREATE SCHEMA staging');
END;
GO

IF OBJECT_ID(N'staging.TitleBasicsImport', N'U') IS NULL
BEGIN
    CREATE TABLE staging.TitleBasicsImport
    (
        TConst VARCHAR(12) NOT NULL,
        TitleType NVARCHAR(50) NULL,
        PrimaryTitle NVARCHAR(500) NULL,
        OriginalTitle NVARCHAR(500) NULL,
        IsAdult NVARCHAR(10) NULL,
        StartYear NVARCHAR(10) NULL,
        EndYear NVARCHAR(10) NULL,
        RuntimeMinutes NVARCHAR(10) NULL,
        GenresCsv NVARCHAR(200) NULL
    );
END;
GO

IF OBJECT_ID(N'staging.NameBasicsImport', N'U') IS NULL
BEGIN
    CREATE TABLE staging.NameBasicsImport
    (
        NConst VARCHAR(12) NOT NULL,
        PrimaryName NVARCHAR(255) NULL,
        BirthYear NVARCHAR(10) NULL,
        DeathYear NVARCHAR(10) NULL,
        PrimaryProfessionCsv NVARCHAR(255) NULL,
        KnownForTitlesCsv NVARCHAR(255) NULL
    );
END;
GO

IF OBJECT_ID(N'staging.TitleCrewImport', N'U') IS NULL
BEGIN
    CREATE TABLE staging.TitleCrewImport
    (
        TConst VARCHAR(12) NOT NULL,
        DirectorsCsv NVARCHAR(2000) NULL,
        WritersCsv NVARCHAR(2000) NULL
    );
END;
GO

IF OBJECT_ID(N'dbo.Titles', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Titles
    (
        TConst VARCHAR(12) NOT NULL PRIMARY KEY,
        TitleType NVARCHAR(50) NOT NULL,
        PrimaryTitle NVARCHAR(500) NOT NULL,
        OriginalTitle NVARCHAR(500) NOT NULL,
        IsAdult BIT NOT NULL,
        StartYear INT NULL,
        EndYear INT NULL,
        RuntimeMinutes INT NULL
    );
END;
GO

IF OBJECT_ID(N'dbo.Genres', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Genres
    (
        GenreId INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        GenreName NVARCHAR(100) NOT NULL UNIQUE
    );
END;
GO

IF OBJECT_ID(N'dbo.TitleGenres', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.TitleGenres
    (
        TConst VARCHAR(12) NOT NULL,
        GenreId INT NOT NULL,
        CONSTRAINT PK_TitleGenres PRIMARY KEY (TConst, GenreId),
        CONSTRAINT FK_TitleGenres_Titles FOREIGN KEY (TConst) REFERENCES dbo.Titles(TConst),
        CONSTRAINT FK_TitleGenres_Genres FOREIGN KEY (GenreId) REFERENCES dbo.Genres(GenreId)
    );
END;
GO

IF OBJECT_ID(N'dbo.People', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.People
    (
        NConst VARCHAR(12) NOT NULL PRIMARY KEY,
        PrimaryName NVARCHAR(255) NOT NULL,
        BirthYear INT NULL,
        DeathYear INT NULL
    );
END;
GO

IF OBJECT_ID(N'dbo.Professions', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Professions
    (
        ProfessionId INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        ProfessionName NVARCHAR(100) NOT NULL UNIQUE
    );
END;
GO

IF OBJECT_ID(N'dbo.PersonProfessions', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.PersonProfessions
    (
        NConst VARCHAR(12) NOT NULL,
        ProfessionId INT NOT NULL,
        CONSTRAINT PK_PersonProfessions PRIMARY KEY (NConst, ProfessionId),
        CONSTRAINT FK_PersonProfessions_People FOREIGN KEY (NConst) REFERENCES dbo.People(NConst),
        CONSTRAINT FK_PersonProfessions_Professions FOREIGN KEY (ProfessionId) REFERENCES dbo.Professions(ProfessionId)
    );
END;
GO

IF OBJECT_ID(N'dbo.PersonKnownForTitles', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.PersonKnownForTitles
    (
        NConst VARCHAR(12) NOT NULL,
        TConst VARCHAR(12) NOT NULL,
        CONSTRAINT PK_PersonKnownForTitles PRIMARY KEY (NConst, TConst),
        CONSTRAINT FK_PersonKnownForTitles_People FOREIGN KEY (NConst) REFERENCES dbo.People(NConst),
        CONSTRAINT FK_PersonKnownForTitles_Titles FOREIGN KEY (TConst) REFERENCES dbo.Titles(TConst)
    );
END;
GO

IF OBJECT_ID(N'dbo.TitlePeopleRoles', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.TitlePeopleRoles
    (
        TConst VARCHAR(12) NOT NULL,
        NConst VARCHAR(12) NOT NULL,
        RoleCode NVARCHAR(20) NOT NULL,
        CONSTRAINT PK_TitlePeopleRoles PRIMARY KEY (TConst, NConst, RoleCode),
        CONSTRAINT FK_TitlePeopleRoles_Titles FOREIGN KEY (TConst) REFERENCES dbo.Titles(TConst),
        CONSTRAINT FK_TitlePeopleRoles_People FOREIGN KEY (NConst) REFERENCES dbo.People(NConst),
        CONSTRAINT CK_TitlePeopleRoles_RoleCode CHECK (RoleCode IN (N'director', N'writer'))
    );
END;
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Titles_PrimaryTitle' AND object_id = OBJECT_ID(N'dbo.Titles'))
BEGIN
    CREATE INDEX IX_Titles_PrimaryTitle ON dbo.Titles (PrimaryTitle);
END;
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_People_PrimaryName' AND object_id = OBJECT_ID(N'dbo.People'))
BEGIN
    CREATE INDEX IX_People_PrimaryName ON dbo.People (PrimaryName);
END;
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_TitleGenres_GenreId' AND object_id = OBJECT_ID(N'dbo.TitleGenres'))
BEGIN
    CREATE INDEX IX_TitleGenres_GenreId ON dbo.TitleGenres (GenreId);
END;
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_PersonProfessions_ProfessionId' AND object_id = OBJECT_ID(N'dbo.PersonProfessions'))
BEGIN
    CREATE INDEX IX_PersonProfessions_ProfessionId ON dbo.PersonProfessions (ProfessionId);
END;
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_PersonKnownForTitles_TConst' AND object_id = OBJECT_ID(N'dbo.PersonKnownForTitles'))
BEGIN
    CREATE INDEX IX_PersonKnownForTitles_TConst ON dbo.PersonKnownForTitles (TConst);
END;
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_TitlePeopleRoles_NConst' AND object_id = OBJECT_ID(N'dbo.TitlePeopleRoles'))
BEGIN
    CREATE INDEX IX_TitlePeopleRoles_NConst ON dbo.TitlePeopleRoles (NConst, RoleCode);
END;
GO

CREATE OR ALTER VIEW dbo.vw_TitleSearch
AS
SELECT
    TConst,
    PrimaryTitle,
    TitleType,
    StartYear,
    RuntimeMinutes
FROM dbo.Titles;
GO

CREATE OR ALTER VIEW dbo.vw_PersonSearch
AS
SELECT
    NConst,
    PrimaryName,
    BirthYear,
    DeathYear
FROM dbo.People;
GO

CREATE OR ALTER PROCEDURE dbo.usp_RebuildNormalizedDataFromStaging
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    BEGIN TRANSACTION;

    DELETE FROM dbo.TitlePeopleRoles;
    DELETE FROM dbo.PersonKnownForTitles;
    DELETE FROM dbo.PersonProfessions;
    DELETE FROM dbo.TitleGenres;
    DELETE FROM dbo.People;
    DELETE FROM dbo.Professions;
    DELETE FROM dbo.Genres;
    DELETE FROM dbo.Titles;

    INSERT INTO dbo.Titles
    (
        TConst,
        TitleType,
        PrimaryTitle,
        OriginalTitle,
        IsAdult,
        StartYear,
        EndYear,
        RuntimeMinutes
    )
    SELECT
        TConst,
        ISNULL(TitleType, N'unknown'),
        ISNULL(PrimaryTitle, N''),
        ISNULL(OriginalTitle, N''),
        CASE WHEN IsAdult = N'1' THEN 1 ELSE 0 END,
        TRY_CONVERT(INT, StartYear),
        TRY_CONVERT(INT, EndYear),
        TRY_CONVERT(INT, RuntimeMinutes)
    FROM staging.TitleBasicsImport;

    INSERT INTO dbo.Genres (GenreName)
    SELECT DISTINCT
        LTRIM(RTRIM(splitter.value))
    FROM staging.TitleBasicsImport titleImport
    CROSS APPLY STRING_SPLIT(ISNULL(titleImport.GenresCsv, N''), N',') splitter
    WHERE NULLIF(LTRIM(RTRIM(splitter.value)), N'') IS NOT NULL;

    INSERT INTO dbo.TitleGenres (TConst, GenreId)
    SELECT DISTINCT
        titleImport.TConst,
        genre.GenreId
    FROM staging.TitleBasicsImport titleImport
    CROSS APPLY STRING_SPLIT(ISNULL(titleImport.GenresCsv, N''), N',') splitter
    INNER JOIN dbo.Genres genre
        ON genre.GenreName = LTRIM(RTRIM(splitter.value))
    WHERE NULLIF(LTRIM(RTRIM(splitter.value)), N'') IS NOT NULL;

    INSERT INTO dbo.People
    (
        NConst,
        PrimaryName,
        BirthYear,
        DeathYear
    )
    SELECT
        NConst,
        ISNULL(PrimaryName, N''),
        TRY_CONVERT(INT, BirthYear),
        TRY_CONVERT(INT, DeathYear)
    FROM staging.NameBasicsImport;

    INSERT INTO dbo.Professions (ProfessionName)
    SELECT DISTINCT
        LTRIM(RTRIM(splitter.value))
    FROM staging.NameBasicsImport personImport
    CROSS APPLY STRING_SPLIT(ISNULL(personImport.PrimaryProfessionCsv, N''), N',') splitter
    WHERE NULLIF(LTRIM(RTRIM(splitter.value)), N'') IS NOT NULL;

    INSERT INTO dbo.PersonProfessions (NConst, ProfessionId)
    SELECT DISTINCT
        personImport.NConst,
        profession.ProfessionId
    FROM staging.NameBasicsImport personImport
    CROSS APPLY STRING_SPLIT(ISNULL(personImport.PrimaryProfessionCsv, N''), N',') splitter
    INNER JOIN dbo.Professions profession
        ON profession.ProfessionName = LTRIM(RTRIM(splitter.value))
    WHERE NULLIF(LTRIM(RTRIM(splitter.value)), N'') IS NOT NULL;

    INSERT INTO dbo.PersonKnownForTitles (NConst, TConst)
    SELECT DISTINCT
        personImport.NConst,
        title.TConst
    FROM staging.NameBasicsImport personImport
    CROSS APPLY STRING_SPLIT(ISNULL(personImport.KnownForTitlesCsv, N''), N',') splitter
    INNER JOIN dbo.Titles title
        ON title.TConst = LTRIM(RTRIM(splitter.value))
    WHERE NULLIF(LTRIM(RTRIM(splitter.value)), N'') IS NOT NULL;

    INSERT INTO dbo.TitlePeopleRoles (TConst, NConst, RoleCode)
    SELECT DISTINCT
        crewImport.TConst,
        person.NConst,
        N'director'
    FROM staging.TitleCrewImport crewImport
    CROSS APPLY STRING_SPLIT(ISNULL(crewImport.DirectorsCsv, N''), N',') splitter
    INNER JOIN dbo.People person
        ON person.NConst = LTRIM(RTRIM(splitter.value))
    INNER JOIN dbo.Titles title
        ON title.TConst = crewImport.TConst
    WHERE NULLIF(LTRIM(RTRIM(splitter.value)), N'') IS NOT NULL

    UNION ALL

    SELECT DISTINCT
        crewImport.TConst,
        person.NConst,
        N'writer'
    FROM staging.TitleCrewImport crewImport
    CROSS APPLY STRING_SPLIT(ISNULL(crewImport.WritersCsv, N''), N',') splitter
    INNER JOIN dbo.People person
        ON person.NConst = LTRIM(RTRIM(splitter.value))
    INNER JOIN dbo.Titles title
        ON title.TConst = crewImport.TConst
    WHERE NULLIF(LTRIM(RTRIM(splitter.value)), N'') IS NOT NULL;

    COMMIT TRANSACTION;
END;
GO

CREATE OR ALTER PROCEDURE dbo.usp_SearchTitles
    @SearchTerm NVARCHAR(200)
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @Pattern NVARCHAR(200) = REPLACE(LTRIM(RTRIM(@SearchTerm)), N'*', N'%');

    IF @Pattern NOT LIKE N'%[%_]%'
    BEGIN
        SET @Pattern = N'%' + @Pattern + N'%';
    END;

    SELECT
        TConst,
        PrimaryTitle,
        TitleType,
        StartYear,
        RuntimeMinutes
    FROM dbo.vw_TitleSearch
    WHERE PrimaryTitle LIKE @Pattern
    ORDER BY PrimaryTitle, TConst;
END;
GO

CREATE OR ALTER PROCEDURE dbo.usp_SearchPeople
    @SearchTerm NVARCHAR(200)
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @Pattern NVARCHAR(200) = REPLACE(LTRIM(RTRIM(@SearchTerm)), N'*', N'%');

    IF @Pattern NOT LIKE N'%[%_]%'
    BEGIN
        SET @Pattern = N'%' + @Pattern + N'%';
    END;

    SELECT
        NConst,
        PrimaryName,
        BirthYear,
        DeathYear
    FROM dbo.vw_PersonSearch
    WHERE PrimaryName LIKE @Pattern
    ORDER BY PrimaryName, NConst;
END;
GO

CREATE OR ALTER PROCEDURE dbo.usp_AddTitle
    @TitleType NVARCHAR(50),
    @PrimaryTitle NVARCHAR(500),
    @OriginalTitle NVARCHAR(500),
    @IsAdult BIT,
    @StartYear INT = NULL,
    @EndYear INT = NULL,
    @RuntimeMinutes INT = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    DECLARE @NextNumber BIGINT;
    DECLARE @NewTConst VARCHAR(12);

    BEGIN TRANSACTION;

    SELECT
        @NextNumber = ISNULL(MAX(TRY_CONVERT(BIGINT, SUBSTRING(TConst, 3, 10))), 0) + 1
    FROM dbo.Titles WITH (UPDLOCK, HOLDLOCK);

    SET @NewTConst = CONCAT('tt', RIGHT(REPLICATE('0', 10) + CAST(@NextNumber AS VARCHAR(10)), 10));

    INSERT INTO dbo.Titles
    (
        TConst,
        TitleType,
        PrimaryTitle,
        OriginalTitle,
        IsAdult,
        StartYear,
        EndYear,
        RuntimeMinutes
    )
    VALUES
    (
        @NewTConst,
        @TitleType,
        @PrimaryTitle,
        @OriginalTitle,
        @IsAdult,
        @StartYear,
        @EndYear,
        @RuntimeMinutes
    );

    COMMIT TRANSACTION;

    SELECT @NewTConst;
END;
GO

CREATE OR ALTER PROCEDURE dbo.usp_AddPerson
    @PrimaryName NVARCHAR(255),
    @BirthYear INT = NULL,
    @DeathYear INT = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    DECLARE @NextNumber BIGINT;
    DECLARE @NewNConst VARCHAR(12);

    BEGIN TRANSACTION;

    SELECT
        @NextNumber = ISNULL(MAX(TRY_CONVERT(BIGINT, SUBSTRING(NConst, 3, 10))), 0) + 1
    FROM dbo.People WITH (UPDLOCK, HOLDLOCK);

    SET @NewNConst = CONCAT('nm', RIGHT(REPLICATE('0', 10) + CAST(@NextNumber AS VARCHAR(10)), 10));

    INSERT INTO dbo.People
    (
        NConst,
        PrimaryName,
        BirthYear,
        DeathYear
    )
    VALUES
    (
        @NewNConst,
        @PrimaryName,
        @BirthYear,
        @DeathYear
    );

    COMMIT TRANSACTION;

    SELECT @NewNConst;
END;
GO

CREATE OR ALTER PROCEDURE dbo.usp_UpdateTitle
    @TConst VARCHAR(12),
    @TitleType NVARCHAR(50),
    @PrimaryTitle NVARCHAR(500),
    @OriginalTitle NVARCHAR(500),
    @IsAdult BIT,
    @StartYear INT = NULL,
    @EndYear INT = NULL,
    @RuntimeMinutes INT = NULL
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE dbo.Titles
    SET
        TitleType = @TitleType,
        PrimaryTitle = @PrimaryTitle,
        OriginalTitle = @OriginalTitle,
        IsAdult = @IsAdult,
        StartYear = @StartYear,
        EndYear = @EndYear,
        RuntimeMinutes = @RuntimeMinutes
    WHERE TConst = @TConst;

    IF @@ROWCOUNT = 0
    BEGIN
        THROW 50001, 'Title not found.', 1;
    END;
END;
GO

CREATE OR ALTER PROCEDURE dbo.usp_DeleteTitle
    @TConst VARCHAR(12)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    BEGIN TRANSACTION;

    DELETE FROM dbo.TitlePeopleRoles WHERE TConst = @TConst;
    DELETE FROM dbo.PersonKnownForTitles WHERE TConst = @TConst;
    DELETE FROM dbo.TitleGenres WHERE TConst = @TConst;
    DELETE FROM dbo.Titles WHERE TConst = @TConst;

    IF @@ROWCOUNT = 0
    BEGIN
        ROLLBACK TRANSACTION;
        THROW 50002, 'Title not found.', 1;
    END;

    COMMIT TRANSACTION;
END;
GO
