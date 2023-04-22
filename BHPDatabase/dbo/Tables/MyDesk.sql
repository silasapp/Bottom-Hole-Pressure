CREATE TABLE [dbo].[MyDesk] (
    [DeskID]    INT           IDENTITY (1, 1) NOT NULL,
    [ProcessID] INT           NOT NULL,
    [RequestID] INT           NOT NULL,
    [StaffID]   INT           NOT NULL,
    [Sort]      INT           NOT NULL,
    [hasWork]   BIT           NOT NULL,
    [CreatedAt] DATETIME      NOT NULL,
    [UpdatedAt] DATETIME      NULL,
    [hasPushed] BIT           NOT NULL,
    [Comment]   VARCHAR (MAX) NULL,
    CONSTRAINT [PK_MyDesk] PRIMARY KEY CLUSTERED ([DeskID] ASC)
);

