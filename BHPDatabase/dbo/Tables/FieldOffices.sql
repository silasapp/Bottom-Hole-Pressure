CREATE TABLE [dbo].[FieldOffices] (
    [FieldOffice_id] INT          IDENTITY (1, 1) NOT NULL,
    [OfficeName]     VARCHAR (50) NOT NULL,
    [CreatedAt]      DATETIME     NOT NULL,
    [UpdatedAt]      DATETIME     NULL,
    [DeleteStatus]   BIT          NOT NULL,
    [DeletedBy]      INT          NULL,
    [DeletedAt]      DATETIME     NULL,
    CONSTRAINT [PK_FieldOffice] PRIMARY KEY CLUSTERED ([FieldOffice_id] ASC)
);

