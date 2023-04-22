CREATE TABLE [dbo].[Countries] (
    [Country_id]   INT          IDENTITY (1, 1) NOT NULL,
    [CountryName]  VARCHAR (20) NOT NULL,
    [CreatedAt]    DATETIME     NOT NULL,
    [UpdatedAt]    DATETIME     NULL,
    [DeleteStatus] BIT          NOT NULL,
    [DeletedBy]    INT          NULL,
    [DeletedAt]    DATETIME     NULL,
    CONSTRAINT [PK_Countries] PRIMARY KEY CLUSTERED ([Country_id] ASC)
);

