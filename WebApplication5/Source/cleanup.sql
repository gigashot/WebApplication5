-- ============================================================
-- EncryptApp - Vycisteni databaze
-- ============================================================
-- Smaze VSECHNA data ze vsech tabulek a resetuje ID pocitadla.
-- Pouzit pred registraci novych uzivatelu pres webove rozhrani
-- nebo pred spustenim seed-data.sql.
-- ============================================================

USE encryptapp;
GO

-- Poradi mazani: nejdriv tabulky s cizimi klici, pak referencovane
DELETE FROM Messages;
DELETE FROM Connections;
DELETE FROM Friends;
DELETE FROM FriendRequests;
DELETE FROM Users;
GO

-- Reset auto-increment pocitadel na 0
DBCC CHECKIDENT ('Users', RESEED, 0);
DBCC CHECKIDENT ('Friends', RESEED, 0);
DBCC CHECKIDENT ('FriendRequests', RESEED, 0);
DBCC CHECKIDENT ('Messages', RESEED, 0);
GO

PRINT 'Databaze vycistena. Vsechny tabulky jsou prazdne.';
