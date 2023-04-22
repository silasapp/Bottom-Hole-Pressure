CREATE TABLE [dbo].[Companies] (
    [CompanyID]          INT           IDENTITY (1, 1) NOT NULL,
    [CompanyElpsID]      INT           NOT NULL,
    [RoleID]             INT           NOT NULL,
    [LocationID]         INT           NULL,
    [CompanyName]        VARCHAR (100) NOT NULL,
    [CompanyEmail]       VARCHAR (80)  NOT NULL,
    [Address]            VARCHAR (100) NULL,
    [City]               VARCHAR (15)  NULL,
    [StateName]          VARCHAR (15)  NULL,
    [ActiveStatus]       BIT           NOT NULL,
    [CreatedAt]          DATETIME      NOT NULL,
    [UpdatedAt]          DATETIME      NULL,
    [DeleteStatus]       BIT           NOT NULL,
    [DeletedBy]          INT           NULL,
    [DeletedAt]          DATETIME      NULL,
    [isFirstTime]        BIT           NULL,
    [Avarta]             VARCHAR (200) NULL,
    [IdentificationCode] VARCHAR (15)  NULL,
    CONSTRAINT [PK_Companies] PRIMARY KEY CLUSTERED ([CompanyID] ASC)
);

