CREATE TABLE [dbo].[NominatedStaff] (
    [NominateID]  INT          IDENTITY (1, 1) NOT NULL,
    [RequestID]   INT          NOT NULL,
    [StaffID]     INT          NOT NULL,
    [Designation] VARCHAR (50) NULL,
    [PhoneNumber] VARCHAR (20) NULL,
    [CreatedAt]   DATETIME     NOT NULL,
    [CreatedBy]   INT          NULL,
    [hasSubmitted] BIT NULL, 
    [DocSource] VARCHAR(1000) NULL, 
    [ElpsDocId] INT NULL, 
    [AppDocId] INT NULL, 
    [UpdatedAt] DATETIME NULL, 
    [isActive] BIT NULL, 
    [Title] VARCHAR(200) NULL, 
    [Contents] VARCHAR(MAX) NULL, 
    [SubmittedAt] DATETIME NULL, 
    [RespondStatus] VARCHAR(10) NULL, 
    [RespondComment] VARCHAR(200) NULL, 
    PRIMARY KEY CLUSTERED ([NominateID] ASC)
);

