CREATE TABLE [dbo].[SubItemCs]
(
    [Id] UNIQUEIDENTIFIER NOT NULL,
    [ValueC] VARCHAR (50) NOT NULL,
    [ItemId] UNIQUEIDENTIFIER NOT NULL,
    CONSTRAINT [PK_SubItemCs] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_SubItemCs_Items] FOREIGN KEY ([ItemId]) REFERENCES [dbo].[Items] ([Id])
);

