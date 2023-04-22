CREATE TABLE [dbo].[Transactions] (
    [TransactionID]     INT           IDENTITY (1, 1) NOT NULL,
    [ElpsTransID]       INT           NULL,
    [TransRef]          VARCHAR (30)  NULL,
    [RequestID]         INT           NOT NULL,
    [TransactionType]   VARCHAR (8)   NOT NULL,
    [TransactionStatus] VARCHAR (20)  NOT NULL,
    [AmtPaid]           INT           NULL,
    [ServiceCharge]     INT           NULL,
    [TotalAmt]          INT           NULL,
    [TransactionDate]   DATETIME      NOT NULL,
    [Description]       VARCHAR (MAX) NULL,
    [RRR]               VARCHAR (15)  NULL,
    [UpdatedAt]         DATETIME      NULL,
    CONSTRAINT [PK_Transactions] PRIMARY KEY CLUSTERED ([TransactionID] ASC)
);

