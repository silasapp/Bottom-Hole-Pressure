CREATE TABLE [dbo].[ZoneStates] (
    [ZoneStates_id] INT      IDENTITY (1, 1) NOT NULL,
    [Zone_id]       INT      NOT NULL,
    [State_id]      INT      NOT NULL,
    [CreatedAt]     DATETIME NOT NULL,
    [UpdatedAt]     DATETIME NULL,
    [DeleteStatus]  BIT      NOT NULL,
    [DeletedBy]     INT      NULL,
    [DeletedAt]     DATETIME NULL,
    CONSTRAINT [PK_ZoneStates] PRIMARY KEY CLUSTERED ([ZoneStates_id] ASC)
);

