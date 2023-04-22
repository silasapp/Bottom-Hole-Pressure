CREATE TABLE [dbo].[Staff] (
    [StaffID]       INT            IDENTITY (1, 1) NOT NULL,
    [StaffElpsID]   VARCHAR (45)   NOT NULL,
    [FieldOfficeID] INT            NOT NULL,
    [RoleID]        INT            NOT NULL,
    [LocationID]    INT            NULL,
    [StaffEmail]    VARCHAR (50)   NOT NULL,
    [FirstName]     VARCHAR (20)   NOT NULL,
    [LastName]      VARCHAR (20)   NOT NULL,
    [Theme]         VARCHAR (5)    NOT NULL,
    [CreatedAt]     DATETIME       NOT NULL,
    [ActiveStatus]  BIT            NOT NULL,
    [UpdatedAt]     DATETIME       NULL,
    [DeleteStatus]  BIT            NOT NULL,
    [DeletedBy]     INT            NULL,
    [DeletedAt]     DATETIME       NULL,
    [CreatedBy]     INT            NULL,
    [UpdatedBy]     INT            NULL,
    [SignaturePath] VARCHAR (1000) NULL,
    [SignatureName] VARCHAR (1000) NULL,
    CONSTRAINT [PK_Staff] PRIMARY KEY CLUSTERED ([StaffID] ASC)
);

