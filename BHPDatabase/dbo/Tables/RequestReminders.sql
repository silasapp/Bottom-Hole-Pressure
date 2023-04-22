CREATE TABLE [dbo].[RequestReminders] (
    [ReminderID] INT            IDENTITY (1, 1) NOT NULL,
    [RequestID]  INT            NOT NULL,
    [Subject]    VARCHAR (100)  NOT NULL,
    [Content]    VARCHAR (1000) NOT NULL,
    [SentAt]     DATETIME       NOT NULL,
    [isSent]     BIT            NOT NULL,
    PRIMARY KEY CLUSTERED ([ReminderID] ASC)
);

