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

-- Create table AppService
CREATE TABLE [dbo].[AppService]
(
	[Id] INT NOT NULL IDENTITY(1, 1),
	[Name] NVARCHAR(500) NOT NULL,
	[Created] DATETIMEOFFSET NOT NULL DEFAULT(GETDATE()),

	CONSTRAINT [PK_AppService_Id] PRIMARY KEY ([Id] ASC)
);

CREATE NONCLUSTERED INDEX [IX_AppService_Name] on [dbo].[AppService] ([Name]);
GO

-- Create table ContainerInstance
CREATE TABLE [dbo].[ContainerInstance]
(
	[Id] INT NOT NULL IDENTITY(1, 1),
	[Name] NVARCHAR(500) NOT NULL,
	[Created] DATETIMEOFFSET NOT NULL DEFAULT(GETDATE()),
	[AppServiceId] INT NOT NULL,

	CONSTRAINT [PK_KuduInstance_Id] PRIMARY KEY ([Id] ASC),
	CONSTRAINT [FK_Logbook_AppServicea_Id] FOREIGN KEY ([AppServiceId]) REFERENCES [dbo].[AppService] ([Id])
);

CREATE NONCLUSTERED INDEX [IX_ContainerInstance_Name] on [dbo].[ContainerInstance] ([Name]);
GO

-- Create table Logbook
CREATE TABLE [dbo].[Logbook]
(
	[Id] INT NOT NULL IDENTITY(1, 1),
	[Created] DATETIMEOFFSET NOT NULL DEFAULT(GETDATE()),
	[LastUpdated] DATETIMEOFFSET NOT NULL,
	[ContainerInstanceId] INT NOT NULL,
	CONSTRAINT [PK_Logbook_Id] PRIMARY KEY ([Id] ASC),
	CONSTRAINT [FK_Logbook_ContainerInstance_Id] FOREIGN KEY ([ContainerInstanceId]) REFERENCES [dbo].[ContainerInstance] ([Id])
);

GO

CREATE NONCLUSTERED INDEX [IX_Logbook_LastUpdated] on [dbo].[Logbook] ([ContainerInstanceId], [LastUpdated]);
GO

-- Create table LogEntry
CREATE TABLE [dbo].[LogEntry]
(
	[Id] INT NOT NULL IDENTITY(1, 1),
	[Created] DATETIMEOFFSET NOT NULL DEFAULT(GETDATE()),
	[ImageName] NVARCHAR(500) NOT NULL,
	[ContainerName] NVARCHAR(500) NOT NULL,
	[Date] DATETIMEOFFSET NOT NULL,
	[InboundPort] INT NOT NULL,
	[OutboundPort] INT NOT NULL,
	[LogBookId] INT NOT NULL,

	CONSTRAINT [PK_LogEntry_Id] PRIMARY KEY ([Id] ASC),
	CONSTRAINT [FK_LogEntry_LogBook_Id] FOREIGN KEY ([LogBookId]) REFERENCES [dbo].[LogBook] ([Id])
);

GO

CREATE NONCLUSTERED INDEX [IX_LogEntry_AppServiceId] ON [dbo].[LogEntry] ([LogBookId], [Created] DESC);
GO