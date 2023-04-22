CREATE TABLE [dbo].[RequestDuration] (
    [DurationID]       INT           IDENTITY (1, 1) NOT NULL,
    [RequestStartDate] DATETIME      NOT NULL,
    [RequestEndDate]   DATETIME      NOT NULL,
    [ProposalYear]     INT           NOT NULL,
    [CreatedAt]        DATETIME      NOT NULL,
    [UpdatedAt]        DATETIME      NULL,
    [DeletedAt]        DATETIME      NULL,
    [DeleteStatus]     BIT           NULL,
    [DeletedBy]        INT           NULL,
    [ReportEndDate]    DATETIME      NULL,
    [ContactPerson]    VARCHAR (500) NULL,
    PRIMARY KEY CLUSTERED ([DurationID] ASC)
);

