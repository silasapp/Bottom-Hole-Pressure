CREATE TABLE [dbo].[PermitHistory] (
    [PermitHistoryID] INT           IDENTITY (1, 1) NOT NULL,
    [PermitID]        INT           NOT NULL,
    [ViewType]        VARCHAR (10)  NOT NULL,
    [UserDetails]     VARCHAR (200) NOT NULL,
    [PreviewedAt]     DATETIME      NULL,
    [DownloadedAt]    DATETIME      NULL,
    PRIMARY KEY CLUSTERED ([PermitHistoryID] ASC)
);

