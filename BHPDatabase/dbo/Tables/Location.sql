CREATE TABLE [dbo].[Location] (
    [LocationID]   INT         IDENTITY (1, 1) NOT NULL,
    [LocationName] VARCHAR (5) NOT NULL,
    [CreatedAt]    DATETIME    NOT NULL,
    [CreatedBy]    INT         NOT NULL,
    [UpdatedAt]    DATETIME    NULL,
    [UpdatedBy]    INT         NULL,
    [DeleteStatus] BIT         NOT NULL,
    [DeletedAt]    DATETIME    NULL,
    [DeletedBy]    INT         NULL,
    CONSTRAINT [PK_Location] PRIMARY KEY CLUSTERED ([LocationID] ASC)
);

