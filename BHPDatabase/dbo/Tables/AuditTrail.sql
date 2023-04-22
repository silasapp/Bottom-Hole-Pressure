CREATE TABLE [dbo].[AuditTrail] (
    [AuditLogID]  INT           IDENTITY (1, 1) NOT NULL,
    [AuditAction] VARCHAR (MAX) NOT NULL,
    [CreatedAt]   DATETIME      NOT NULL,
    [UserID]      VARCHAR (80)  NULL,
    CONSTRAINT [PK_AuditTrail] PRIMARY KEY CLUSTERED ([AuditLogID] ASC)
);

