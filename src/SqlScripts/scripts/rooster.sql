-- Create a new database called 'rat'
-- Connect to the 'master' database to run this snippet
USE master
GO

-- Create the new database if it does not exist already
IF EXISTS (SELECT [name] FROM sys.databases WHERE [name] = N'Rooster')
BEGIN
	ALTER DATABASE Rooster SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
	DROP DATABASE Rooster
END

CREATE DATABASE Rooster
GO

USE Rooster

-- Create table LogEntry
CREATE TABLE [dbo].[LogEntry]
(
	[Id] INT NOT NULL IDENTITY(1, 1),
	[Created] DATETIMEOFFSET NOT NULL DEFAULT(GETDATE()),
	[ServiceName] VARCHAR(512) NOT NULL,
	[ContainerName] VARCHAR(512) NOT NULL,
	[ImageName] VARCHAR(512) NOT NULL,
	[ImageTag] VARCHAR(256) NOT NULL,
	[EventDate] DATETIMEOFFSET NOT NULL,
	[InboundPort] INT NOT NULL,
	[OutboundPort] INT NOT NULL,

	CONSTRAINT [PK_LogEntry_Id] PRIMARY KEY ([Id] ASC)
);

CREATE NONCLUSTERED INDEX [IX_LogEntry_Created_ServiceName_ContainerName] on [dbo].[LogEntry] ([Created] DESC, [ServiceName], [ContainerName])

GO