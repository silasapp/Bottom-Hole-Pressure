CREATE TABLE [dbo].[ZonalOffice] (
    [Zone_id]      INT          IDENTITY (1, 1) NOT NULL,
    [ZoneName]     VARCHAR (30) NOT NULL,
    [CreatedAt]    DATETIME     NOT NULL,
    [UpdatedAt]    DATETIME     NULL,
    [DeleteStatus] BIT          NOT NULL,
    [DeletedBy]    INT          NULL,
    [DeletedAt]    DATETIME     NULL,
    CONSTRAINT [PK_Zones] PRIMARY KEY CLUSTERED ([Zone_id] ASC)
);

