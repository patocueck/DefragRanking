CREATE TABLE [dbo].[Player]
(
	[id] INT NOT NULL AUTO_INCREMENT, 
    [name] NCHAR(20) NULL, 
    [account] NCHAR(10) NOT NULL, 
    [password] NCHAR(10) NOT NULL,
	PRIMARY KEY(id)
)
GO
CREATE TABLE [dbo].[Map]
(
	[id] INT NOT NULL AUTO_INCREMENT,
	[name] nCHAR(40) NOT NULL,
	PRIMARY KEY(id)
)
GO
CREATE TABLE [dbo].[Time]
(
	[idPlayer] INT NOT NULL ,
	[idMap] INT NOT NULL ,
	[mapTime] NCHAR NOT NULL,
	[physic] nCHAR(5) NOT NULL ,
	PRIMARY KEY(idPlayer,idMap, mapTime,physic)
)