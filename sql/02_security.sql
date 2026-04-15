USE IMDB;
GO

IF NOT EXISTS (SELECT 1 FROM sys.database_principals WHERE name = N'imdb_ui_executor')
BEGIN
    CREATE ROLE imdb_ui_executor;
END;
GO

DENY SELECT, INSERT, UPDATE, DELETE ON OBJECT::dbo.Titles TO imdb_ui_executor;
DENY SELECT, INSERT, UPDATE, DELETE ON OBJECT::dbo.Genres TO imdb_ui_executor;
DENY SELECT, INSERT, UPDATE, DELETE ON OBJECT::dbo.TitleGenres TO imdb_ui_executor;
DENY SELECT, INSERT, UPDATE, DELETE ON OBJECT::dbo.People TO imdb_ui_executor;
DENY SELECT, INSERT, UPDATE, DELETE ON OBJECT::dbo.Professions TO imdb_ui_executor;
DENY SELECT, INSERT, UPDATE, DELETE ON OBJECT::dbo.PersonProfessions TO imdb_ui_executor;
DENY SELECT, INSERT, UPDATE, DELETE ON OBJECT::dbo.PersonKnownForTitles TO imdb_ui_executor;
DENY SELECT, INSERT, UPDATE, DELETE ON OBJECT::dbo.TitlePeopleRoles TO imdb_ui_executor;
DENY SELECT, INSERT, UPDATE, DELETE ON OBJECT::staging.TitleBasicsImport TO imdb_ui_executor;
DENY SELECT, INSERT, UPDATE, DELETE ON OBJECT::staging.NameBasicsImport TO imdb_ui_executor;
DENY SELECT, INSERT, UPDATE, DELETE ON OBJECT::staging.TitleCrewImport TO imdb_ui_executor;
GO

GRANT SELECT ON OBJECT::dbo.vw_TitleSearch TO imdb_ui_executor;
GRANT SELECT ON OBJECT::dbo.vw_PersonSearch TO imdb_ui_executor;
GRANT EXECUTE ON OBJECT::dbo.usp_SearchTitles TO imdb_ui_executor;
GRANT EXECUTE ON OBJECT::dbo.usp_SearchPeople TO imdb_ui_executor;
GRANT EXECUTE ON OBJECT::dbo.usp_AddTitle TO imdb_ui_executor;
GRANT EXECUTE ON OBJECT::dbo.usp_AddPerson TO imdb_ui_executor;
GRANT EXECUTE ON OBJECT::dbo.usp_UpdateTitle TO imdb_ui_executor;
GRANT EXECUTE ON OBJECT::dbo.usp_DeleteTitle TO imdb_ui_executor;
GO

PRINT 'Create a SQL login/user for the UI and add it to role imdb_ui_executor.';
GO
