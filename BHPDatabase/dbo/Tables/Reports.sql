CREATE TABLE [dbo].[Reports] (
    [ReportID]      INT           IDENTITY (1, 1) NOT NULL,
    [RequestID]     INT           NOT NULL,
    [StaffID]       INT           NOT NULL,
    [Comment]       VARCHAR (MAX) NOT NULL,
    [Subject]       VARCHAR (200)  NOT NULL,
    [ElpsDocId]     INT            NULL,
    [AppDocId]      INT            NULL,
    [DocSource]     VARCHAR (1000) NULL,
    [CreatedAt]     DATETIME      NOT NULL,
    [UpdatedAt]     DATETIME      NULL,
    [DeletedBy]     INT           NULL,
    [DeletedStatus] BIT           NULL,
    [DeletedAt]     DATETIME      NULL,
    CONSTRAINT [PK_Reports] PRIMARY KEY CLUSTERED ([ReportID] ASC)
);

