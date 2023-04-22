CREATE TABLE [dbo].[SubmittedDocuments] (
    [SubDocID]      INT           IDENTITY (1, 1) NOT NULL,
    [RequestID]     INT           NOT NULL,
    [AppDocID]      INT           NOT NULL,
    [CompElpsDocID] INT           NULL,
    [CreatedAt]     DATETIME      NOT NULL,
    [UpdatedAt]     DATETIME      NULL,
    [DeletedStatus] BIT           NOT NULL,
    [DeletedBy]     INT           NULL,
    [DeletedAt]     DATETIME      NULL,
    [DocSource]     VARCHAR (MAX) NULL,
    PRIMARY KEY CLUSTERED ([SubDocID] ASC)
);

