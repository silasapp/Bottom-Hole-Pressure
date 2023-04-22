CREATE TABLE [dbo].[UserRoles] (
    [Role_id]      INT          IDENTITY (1, 1) NOT NULL,
    [RoleName]     VARCHAR (20) NOT NULL,
    [CreatedAt]    DATETIME     NOT NULL,
    [UpdatedAt]    DATETIME     NULL,
    [DeleteStatus] BIT          NOT NULL,
    [DeletedBy]    INT          NULL,
    [DeletedAt]    DATETIME     NULL,
    CONSTRAINT [PK_UserRoles] PRIMARY KEY CLUSTERED ([Role_id] ASC)
);

