CREATE TABLE [dbo].[ZoneFieldOffice] (
    [ZoneFieldOffice_id] INT      IDENTITY (1, 1) NOT NULL,
    [Zone_id]            INT      NOT NULL,
    [FieldOffice_id]     INT      NOT NULL,
    [CreatedAt]          DATETIME NOT NULL,
    [UpdatedAt]          DATETIME NULL,
    [DeleteStatus]       BIT      NOT NULL,
    [DeletedBy]          INT      NULL,
    [DeletedAt]          DATETIME NULL,
    CONSTRAINT [PK_ZoneFieldOffice] PRIMARY KEY CLUSTERED ([ZoneFieldOffice_id] ASC)
);

