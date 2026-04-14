-- ============================================================
-- EncryptApp - Testovaci data pro maturitni komisi
-- ============================================================
-- Pouziti: Otevrit v SSMS, pripojit se k MARA\encryptapp, spustit (F5)
--
-- DULEZITE: Tyto uzivatele lze pouzit POUZE pro testovani
-- backendu a overeni dat v databazi.
-- Pro plne funkcni chat (sifrovani/desifrovani) je nutne
-- uzivatele registrovat pres webove rozhrani, protoze:
--   1. Registrace vygeneruje RSA par klicu v prohlizeci
--   2. Soukromy klic se ulozi do IndexedDB (nikdy na server)
--   3. Bez soukromeho klice nelze desifrovat zpravy
--
-- Hesla vsech testovacich uzivatelu: Test1234!
-- (bcrypt hash nize odpovida tomuto heslu)
-- ============================================================

USE encryptapp;
GO

-- Smazani existujicich dat (v poradi kvuli FK)
DELETE FROM Messages;
DELETE FROM Friends;
DELETE FROM FriendRequests;
DELETE FROM Connections;
DELETE FROM Users;
GO

-- Reset identity counteru
DBCC CHECKIDENT ('Users', RESEED, 0);
DBCC CHECKIDENT ('Friends', RESEED, 0);
DBCC CHECKIDENT ('FriendRequests', RESEED, 0);
DBCC CHECKIDENT ('Messages', RESEED, 0);
GO

-- ============================================================
-- 1. UZIVATELE
-- ============================================================
-- Heslo: Test1234!
-- Bcrypt hash vygenerovany algoritmem bcrypt s cost factor 11
-- PublicKey: placeholder — nelze pouzit pro skutecne sifrovani,
--           ale staci pro zobrazeni v UI a testovani API

INSERT INTO Users (Username, PasswordHash, PublicKey, CreatedAt, LastLogin)
VALUES
    -- Uzivatel 1: Alice (hlavni testovaci ucet)
    ('alice',
     '$2a$11$rQbHvBVKzLKCYx4XqGJmXOZCqkHOHiPVJr3fNJGRHZ6G5FqXKxJLi',
     'MIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEA0000ALICE000TEST000KEY',
     '2025-01-15 10:00:00', '2025-04-10 14:30:00'),

    -- Uzivatel 2: Bob
    ('bob',
     '$2a$11$rQbHvBVKzLKCYx4XqGJmXOZCqkHOHiPVJr3fNJGRHZ6G5FqXKxJLi',
     'MIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEA0000BOB0000TEST000KEY',
     '2025-01-16 11:00:00', '2025-04-10 15:00:00'),

    -- Uzivatel 3: Charlie
    ('charlie',
     '$2a$11$rQbHvBVKzLKCYx4XqGJmXOZCqkHOHiPVJr3fNJGRHZ6G5FqXKxJLi',
     'MIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEA0000CHARLIE0TEST000KEY',
     '2025-02-01 09:00:00', '2025-04-09 12:00:00'),

    -- Uzivatel 4: Dana (ma pending friend request)
    ('dana',
     '$2a$11$rQbHvBVKzLKCYx4XqGJmXOZCqkHOHiPVJr3fNJGRHZ6G5FqXKxJLi',
     'MIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEA0000DANA000TEST000KEY',
     '2025-03-01 08:00:00', NULL),

    -- Uzivatel 5: Erik (novy uzivatel, zadni pratele)
    ('erik',
     '$2a$11$rQbHvBVKzLKCYx4XqGJmXOZCqkHOHiPVJr3fNJGRHZ6G5FqXKxJLi',
     'MIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEA0000ERIK000TEST000KEY',
     '2025-04-01 16:00:00', NULL);
GO

-- ============================================================
-- 2. PRATELSTVI
-- ============================================================
-- UserId1 < UserId2 (vynuceno unikatnim indexem)

INSERT INTO Friends (UserId1, UserId2, CreatedAt)
VALUES
    -- Alice (1) a Bob (2) jsou pratele
    (1, 2, '2025-01-20 12:00:00'),

    -- Alice (1) a Charlie (3) jsou pratele
    (1, 3, '2025-02-05 14:00:00'),

    -- Bob (2) a Charlie (3) jsou pratele
    (2, 3, '2025-02-10 16:00:00');
GO

-- ============================================================
-- 3. ZADOSTI O PRATELSTVI
-- ============================================================

INSERT INTO FriendRequests (SenderId, ReceiverId, Status, CreatedAt)
VALUES
    -- Schvalene zadosti (odpovida pratelstvim vyse)
    (1, 2, 'accepted', '2025-01-18 10:00:00'),
    (1, 3, 'accepted', '2025-02-03 09:00:00'),
    (2, 3, 'accepted', '2025-02-08 11:00:00'),

    -- Cekajici zadost: Dana (4) chce byt pritel s Alice (1)
    (4, 1, 'pending', '2025-04-10 10:00:00'),

    -- Zamitnuta zadost: Erik (5) byl odmitnut Bobem (2)
    (5, 2, 'rejected', '2025-04-05 13:00:00');
GO

-- ============================================================
-- 4. ZPRAVY (simulovane sifrovane zpravy)
-- ============================================================
-- EncryptedContent: v realnem provozu JSON {wrappedKey, iv, ciphertext}
-- Zde pouzivame citelny placeholder aby komise videla strukturu
-- a zaroven se presvedcila ze server NIKDY nevidi plaintext.
--
-- V produkcni aplikaci by tento sloupec obsahoval napr.:
-- {"wrappedKey":"base64...","iv":"base64...","ciphertext":"base64..."}

INSERT INTO Messages (SenderId, ReceiverId, EncryptedContent, SenderEncryptedContent, SentAt, Delivered, [Read])
VALUES
    -- Konverzace Alice (1) <-> Bob (2)
    (1, 2,
     '{"wrappedKey":"ENCRYPTED_FOR_BOB_aes_key_01","iv":"random_iv_01","ciphertext":"ENCRYPTED_ahoj_bobe"}',
     '{"wrappedKey":"ENCRYPTED_FOR_ALICE_aes_key_01","iv":"random_iv_01","ciphertext":"ENCRYPTED_ahoj_bobe_sender_copy"}',
     '2025-04-10 14:30:00', 1, 1),

    (2, 1,
     '{"wrappedKey":"ENCRYPTED_FOR_ALICE_aes_key_02","iv":"random_iv_02","ciphertext":"ENCRYPTED_ahoj_alice_jak_se_mas"}',
     '{"wrappedKey":"ENCRYPTED_FOR_BOB_aes_key_02","iv":"random_iv_02","ciphertext":"ENCRYPTED_ahoj_alice_jak_se_mas_sender_copy"}',
     '2025-04-10 14:31:00', 1, 1),

    (1, 2,
     '{"wrappedKey":"ENCRYPTED_FOR_BOB_aes_key_03","iv":"random_iv_03","ciphertext":"ENCRYPTED_dobre_diky_a_ty"}',
     '{"wrappedKey":"ENCRYPTED_FOR_ALICE_aes_key_03","iv":"random_iv_03","ciphertext":"ENCRYPTED_dobre_diky_a_ty_sender_copy"}',
     '2025-04-10 14:32:00', 1, 0),

    (2, 1,
     '{"wrappedKey":"ENCRYPTED_FOR_ALICE_aes_key_04","iv":"random_iv_04","ciphertext":"ENCRYPTED_taky_dobre"}',
     '{"wrappedKey":"ENCRYPTED_FOR_BOB_aes_key_04","iv":"random_iv_04","ciphertext":"ENCRYPTED_taky_dobre_sender_copy"}',
     '2025-04-10 14:33:00', 0, 0),

    -- Konverzace Alice (1) <-> Charlie (3)
    (3, 1,
     '{"wrappedKey":"ENCRYPTED_FOR_ALICE_aes_key_05","iv":"random_iv_05","ciphertext":"ENCRYPTED_mas_cas_vecer"}',
     '{"wrappedKey":"ENCRYPTED_FOR_CHARLIE_aes_key_05","iv":"random_iv_05","ciphertext":"ENCRYPTED_mas_cas_vecer_sender_copy"}',
     '2025-04-09 18:00:00', 1, 1),

    (1, 3,
     '{"wrappedKey":"ENCRYPTED_FOR_CHARLIE_aes_key_06","iv":"random_iv_06","ciphertext":"ENCRYPTED_jasne_v_8"}',
     '{"wrappedKey":"ENCRYPTED_FOR_ALICE_aes_key_06","iv":"random_iv_06","ciphertext":"ENCRYPTED_jasne_v_8_sender_copy"}',
     '2025-04-09 18:05:00', 1, 1),

    -- Konverzace Bob (2) <-> Charlie (3) — nedorucena zprava
    (2, 3,
     '{"wrappedKey":"ENCRYPTED_FOR_CHARLIE_aes_key_07","iv":"random_iv_07","ciphertext":"ENCRYPTED_zitra_trenink"}',
     '{"wrappedKey":"ENCRYPTED_FOR_BOB_aes_key_07","iv":"random_iv_07","ciphertext":"ENCRYPTED_zitra_trenink_sender_copy"}',
     '2025-04-10 20:00:00', 0, 0);
GO

-- ============================================================
-- OVERENI: Kontrolni dotazy pro komisi
-- ============================================================

-- Pocty zaznamu v kazde tabulce
SELECT 'Users' AS Tabulka, COUNT(*) AS Pocet FROM Users
UNION ALL
SELECT 'Friends', COUNT(*) FROM Friends
UNION ALL
SELECT 'FriendRequests', COUNT(*) FROM FriendRequests
UNION ALL
SELECT 'Messages', COUNT(*) FROM Messages;

-- Pratelstvi s uzivatelskymi jmeny
SELECT
    f.FriendId,
    u1.Username AS Uzivatel1,
    u2.Username AS Uzivatel2,
    f.CreatedAt
FROM Friends f
JOIN Users u1 ON f.UserId1 = u1.UserId
JOIN Users u2 ON f.UserId2 = u2.UserId;

-- Zadosti o pratelstvi se jmeny
SELECT
    fr.RequestId,
    s.Username AS Odesilatel,
    r.Username AS Prijemce,
    fr.Status,
    fr.CreatedAt
FROM FriendRequests fr
JOIN Users s ON fr.SenderId = s.UserId
JOIN Users r ON fr.ReceiverId = r.UserId;

-- Zpravy — dukaz ze server vidi jen sifrovany text
SELECT
    m.MessageId,
    s.Username AS Odesilatel,
    r.Username AS Prijemce,
    m.EncryptedContent AS [Server_vidi_pouze_toto],
    m.Delivered AS Doruceno,
    m.[Read] AS Precteno,
    m.SentAt
FROM Messages m
JOIN Users s ON m.SenderId = s.UserId
JOIN Users r ON m.ReceiverId = r.UserId
ORDER BY m.SentAt;
