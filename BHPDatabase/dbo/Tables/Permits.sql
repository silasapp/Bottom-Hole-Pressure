CREATE TABLE [dbo].[Permits] (
    [PermitID]       INT          IDENTITY (1, 1) NOT NULL,
    [PermitElpsID]   INT          NULL,
    [RequestID]      INT          NOT NULL,
    [PermitNo]       VARCHAR (50) NOT NULL,
    [IssuedDate]     DATETIME     NOT NULL,
    [ExpireDate]     DATETIME     NOT NULL,
    [Printed]        BIT          NOT NULL,
    [isRenewed]      BIT          NULL,
    [ApprovedBy]     INT          NOT NULL,
    [CreatedAt]      DATETIME     NOT NULL,
    [PermitSequence] INT          NOT NULL,
    CONSTRAINT [PK_Permits] PRIMARY KEY CLUSTERED ([PermitID] ASC)
);

