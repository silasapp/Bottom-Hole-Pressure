CREATE TABLE [dbo].[Schdules] (
    [SchduleID]         INT           IDENTITY (1, 1) NOT NULL,
    [RequestID]         INT           NOT NULL,
    [SchduleBy]         INT           NOT NULL,
    [SchduleType]       VARCHAR (12)  NOT NULL,
    [SchduleLocation]   VARCHAR (40)  NOT NULL,
    [SchduleDate]       DATETIME      NOT NULL,
    [Supervisor]        INT           NULL,
    [SupervisorApprove] INT           NULL,
    [CustomerAccept]    INT           NULL,
    [Comment]           VARCHAR (MAX) NULL,
    [CustomerComment]   VARCHAR (MAX) NULL,
    [SupervisorComment] VARCHAR (MAX) NULL,
    [CreatedAt]         DATETIME      NOT NULL,
    [UpdatedAt]         DATETIME      NULL,
    [DeletedBy]         INT           NULL,
    [DeletedAt]         DATETIME      NULL,
    [DeletedStatus]     BIT           NULL,
    [IsDone]            BIT           NULL,
    CONSTRAINT [PK_Schdules] PRIMARY KEY CLUSTERED ([SchduleID] ASC)
);

