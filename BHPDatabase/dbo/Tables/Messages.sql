CREATE TABLE [dbo].[Messages] (
    [MessageID]   INT           IDENTITY (1, 1) NOT NULL,
    [CompanyID]   INT           NOT NULL,
    [RequestID]   INT           NOT NULL,
    [Subject]     VARCHAR (100) NOT NULL,
    [MesgContent] VARCHAR (MAX) NOT NULL,
    [Seen]        BIT           NOT NULL,
    [CreatedAt]   DATETIME      NOT NULL,
    CONSTRAINT [PK_Messages] PRIMARY KEY CLUSTERED ([MessageID] ASC)
);

