CREATE TABLE [dbo].[NominationRequest]
(
	[RequestId] INT NOT NULL PRIMARY KEY IDENTITY, 
    [AppId] INT NOT NULL, 
    [StaffId] INT NOT NULL, 
    [Comment] VARCHAR(MAX) NOT NULL, 
    [hasDone] BIT NOT NULL, 
    [CreatedAt] DATETIME NOT NULL, 
    [UpdatedAt] DATETIME NULL, 
    [ReminderDate] DATETIME NULL
)
