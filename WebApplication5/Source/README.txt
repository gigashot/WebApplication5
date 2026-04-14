================================================================
EncryptApp - Source / SQL skripty
================================================================

Tato slozka obsahuje SQL skripty pro praci s databazi.
Vsechny skripty spoustet v SQL Server Management Studio (SSMS).

Pripojeni k databazi:
  Server:   MARA
  Databaze: encryptapp
  Autentizace: Windows Authentication (Integrated Security)

================================================================
SOUBORY
================================================================

1. seed-data.sql
   - Vlozi testovaci data: 5 uzivatelu, 3 pratelstvi,
     5 zadosti o pratelstvi, 7 zprav
   - Heslo vsech uzivatelu: Test1234!
   - Na konci spusti kontrolni dotazy pro overeni
   - POZOR: Smaze existujici data pred vlozenim!

2. verify-schema.sql
   - Zobrazi strukturu databaze: tabulky, sloupce, primarni klice,
     cizi klice (foreign keys), indexy
   - Nemenni zadna data — pouze cteni

3. cleanup.sql
   - Smaze vsechna data ze vsech tabulek
   - Resetuje auto-increment pocitadla
   - Pouzit pred cistou registraci pres web

================================================================
POSTUP PRO TESTOVANI
================================================================

Varianta A — Plne funkcni test (doporuceno):
  1. Spustit cleanup.sql (vycistit DB)
  2. Spustit aplikaci ve Visual Studiu (F5)
  3. Otevrit http://localhost:12345/Views/register.html
  4. Zaregistrovat 2 uzivatele (kazdy v jinem prohlizeci/profilu)
  5. Poslat zadost o pratelstvi, prijmout ji
  6. Otevrit chat a poslat zpravy
  7. V SSMS spustit: SELECT EncryptedContent FROM Messages
     → zpravy jsou zasifrovane, server nikdy nevidi plaintext

Varianta B — Pouze overeni DB struktury:
  1. Spustit seed-data.sql (vlozi testovaci data)
  2. Spustit verify-schema.sql (zobrazi strukturu)
  3. Prohlednout vysledky v SSMS

================================================================
OMEZENI TESTOVACICH DAT
================================================================

Testovaci uzivatele z seed-data.sql NELZE pouzit pro skutecny
chat se sifrovanim, protoze:

  1. Registrace pres web vygeneruje RSA par klicu v prohlizeci
  2. Soukromy klic se ulozi do IndexedDB (nikdy na server)
  3. Seed data obsahuji placeholder verejne klice
  4. Bez skutecneho soukromeho klice nelze desifrovat zpravy

Pro demonstraci sifrovaneho chatu je nutne registrovat
uzivatele pres webove rozhrani (Varianta A).
