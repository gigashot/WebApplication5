-- ============================================================
-- EncryptApp - Overeni struktury databaze
-- ============================================================
-- Spustit v SSMS pro kontrolu ze DB odpovida ocekavane strukture
-- ============================================================

USE encryptapp;
GO

-- ============================================================
-- 1. VSECHNY TABULKY V DATABAZI
-- ============================================================
SELECT
    t.name AS Tabulka,
    SUM(p.rows) AS Pocet_radku
FROM sys.tables t
JOIN sys.partitions p ON t.object_id = p.object_id AND p.index_id IN (0, 1)
GROUP BY t.name
ORDER BY t.name;

-- ============================================================
-- 2. SLOUPCE KAZDE TABULKY (datovy typ, nullable, max delka)
-- ============================================================
SELECT
    t.name AS Tabulka,
    c.name AS Sloupec,
    ty.name AS Datovy_typ,
    c.max_length AS Max_delka,
    CASE c.is_nullable WHEN 0 THEN 'NOT NULL' ELSE 'NULL' END AS Nullable,
    CASE WHEN ic.object_id IS NOT NULL THEN 'IDENTITY' ELSE '' END AS Identita
FROM sys.tables t
JOIN sys.columns c ON t.object_id = c.object_id
JOIN sys.types ty ON c.user_type_id = ty.user_type_id
LEFT JOIN sys.identity_columns ic ON c.object_id = ic.object_id AND c.column_id = ic.column_id
ORDER BY t.name, c.column_id;

-- ============================================================
-- 3. PRIMARNI KLICE
-- ============================================================
SELECT
    t.name AS Tabulka,
    kc.name AS PK_nazev,
    c.name AS PK_sloupec
FROM sys.key_constraints kc
JOIN sys.tables t ON kc.parent_object_id = t.object_id
JOIN sys.index_columns ic ON kc.unique_index_id = ic.index_id AND kc.parent_object_id = ic.object_id
JOIN sys.columns c ON ic.object_id = c.object_id AND ic.column_id = c.column_id
WHERE kc.type = 'PK'
ORDER BY t.name;

-- ============================================================
-- 4. CIZI KLICE (FOREIGN KEYS)
-- ============================================================
SELECT
    fk.name AS FK_nazev,
    tp.name AS Tabulka,
    cp.name AS Sloupec,
    tr.name AS Odkazuje_na_tabulku,
    cr.name AS Odkazuje_na_sloupec,
    CASE fk.delete_referential_action
        WHEN 0 THEN 'NO ACTION'
        WHEN 1 THEN 'CASCADE'
        WHEN 2 THEN 'SET NULL'
    END AS Pri_smazani
FROM sys.foreign_keys fk
JOIN sys.foreign_key_columns fkc ON fk.object_id = fkc.constraint_object_id
JOIN sys.tables tp ON fkc.parent_object_id = tp.object_id
JOIN sys.columns cp ON fkc.parent_object_id = cp.object_id AND fkc.parent_column_id = cp.column_id
JOIN sys.tables tr ON fkc.referenced_object_id = tr.object_id
JOIN sys.columns cr ON fkc.referenced_object_id = cr.object_id AND fkc.referenced_column_id = cr.column_id
ORDER BY tp.name, fk.name;

-- ============================================================
-- 5. INDEXY
-- ============================================================
SELECT
    t.name AS Tabulka,
    i.name AS Index_nazev,
    CASE i.is_unique WHEN 1 THEN 'UNIQUE' ELSE '' END AS Unikatni,
    STRING_AGG(c.name, ', ') WITHIN GROUP (ORDER BY ic.key_ordinal) AS Sloupce
FROM sys.indexes i
JOIN sys.tables t ON i.object_id = t.object_id
JOIN sys.index_columns ic ON i.object_id = ic.object_id AND i.index_id = ic.index_id
JOIN sys.columns c ON ic.object_id = c.object_id AND ic.column_id = c.column_id
WHERE i.name IS NOT NULL AND i.is_primary_key = 0
GROUP BY t.name, i.name, i.is_unique
ORDER BY t.name, i.name;
