CREATE TABLE [dbo].[States] (
    [State_id]     INT          IDENTITY (1, 1) NOT NULL,
    [Country_id]   INT          NOT NULL,
    [StateName]    VARCHAR (15) NOT NULL,
    [CreatedAt]    DATETIME     NOT NULL,
    [UpdatedAt]    DATETIME     NULL,
    [DeleteStatus] BIT          NOT NULL,
    [DeletedBy]    INT          NULL,
    [DeletedAt]    DATETIME     NULL,
    CONSTRAINT [PK_States] PRIMARY KEY CLUSTERED ([State_id] ASC)
);

