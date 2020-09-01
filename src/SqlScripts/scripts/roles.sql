USE [rooster]
GO

CREATE USER rooster_user FOR LOGIN [rooster_app] WITH DEFAULT_SCHEMA=[dbo]
GO

ALTER ROLE [db_datareader] ADD MEMBER [rooster_user]
GO

ALTER ROLE [db_datawriter] ADD MEMBER [rooster_user]
GO