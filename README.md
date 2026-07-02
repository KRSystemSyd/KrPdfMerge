# KrPDFmerge

Ett enkelt WinForms-program (.NET Framework 4.6.2) för att slå ihop flera PDF-filer till en sammanslagen PDF.

## Innehåll
- [Översikt](#översikt)
- [Teknik och beroenden](#teknik-och-beroenden)
- [Projektstruktur](#projektstruktur)
- [Så fungerar programmet](#så-fungerar-programmet)
- [Sorteringslägen](#sorteringslägen)
- [Körning via kommandorad](#körning-via-kommandorad)
- [Körning via GUI](#körning-via-gui)
- [Testscript i `bin\\Debug\\test`](#testscript-i-bindebugtest)
- [Loggning och felhantering](#loggning-och-felhantering)
- [Bygga projektet](#bygga-projektet)
- [Begränsningar](#begränsningar)

## Översikt
Programmet läser PDF-filer från en källmapp och skapar en sammanslagen PDF i en utpekad målfil.

Det går att köra på två sätt:
1. **GUI-läge** (formulär med fält för källa och mål)
2. **CLI-läge** (argument till `KrPDFmerge.exe`)

## Teknik och beroenden
- Målplattform: `.NET Framework 4.6.2`
- UI: `Windows Forms`
- PDF-bibliotek: `iTextSharp 5.5.13.3`
- Övrigt: `Fody` + `Costura` (paketering av beroenden)

## Projektstruktur
- `Program.cs`
  - Entrypoint (`Main`) och tolkning av kommandoradsargument.
- `frmKrPDFmerge.cs`
  - Kärnlogik för sortering, sammanslagning, loggning och felhantering.
- `frmKrPDFmerge.Designer.cs`
  - WinForms-komponenter (source folder, output file, merge-knapp).
- `bin\\Debug\\test\\merge-pdfs.cmd`
  - Hjälpscript för testkörning i testmapp.

## Så fungerar programmet
1. Programmet tar emot:
   - källmapp med PDF-filer
   - målfil för sammanslagen PDF
   - valfritt sorteringsläge
2. Sökvägar normaliseras.
3. PDF-filer hämtas från källmappen (`*.pdf`).
4. Filer med `merged` i filnamnet exkluderas.
5. Filer sorteras enligt valt sorteringsläge.
6. Alla sidor från varje PDF läggs till i ordning i målfilen.
7. Logg skrivs till `log\\yyyyMM.txt` under programmets startkatalog.

## Sorteringslägen
Programmet har fem stödda sorteringslägen:

- `SORT_INVOICE_FIRST` (**default**)
  - Filer vars namn innehåller `invoice` eller `faktura` läggs först.
  - Övriga filer sorteras alfabetiskt.

- `SORT_ALPHABETIC`
  - Sorterar alla PDF-filer alfabetiskt på filnamn.

- `SORT_PREFIX`
  - Försöker läsa numeriskt prefix i början av filnamn, t.ex. `1_faktura.pdf`, `2_bilaga.pdf`.
  - Filer med numeriskt prefix kommer först, sorterat på prefix.
  - Därefter sorteras resterande alfabetiskt.

- `SORT_BY_DATE`
  - **Legacy alias** för `SORT_BY_DATE_DESC`.
  - Behålls för bakåtkompatibilitet.

- `SORT_BY_DATE_DESC`
  - Sorterar på filens senast ändrade datum (`LastWriteTime`).
  - **Nyast fil kommer först**.

- `SORT_BY_DATE_ASC`
  - Sorterar på filens senast ändrade datum (`LastWriteTime`).
  - **Äldst fil kommer först**.

Om okänt sorteringsläge skickas in används `SORT_INVOICE_FIRST`.

## Körning via kommandorad
Format:

`KrPDFmerge.exe <sourceFolder> <outputFile> [sortMode]`

Exempel:
- Default (`SORT_INVOICE_FIRST`):
  - `KrPDFmerge.exe "C:\\pdfs" "C:\\out\\merged.pdf"`
- Alfabetisk:
  - `KrPDFmerge.exe "C:\\pdfs" "C:\\out\\merged.pdf" SORT_ALPHABETIC`
- Prefix:
  - `KrPDFmerge.exe "C:\\pdfs" "C:\\out\\merged.pdf" SORT_PREFIX`
- Datum nyast först:
  - `KrPDFmerge.exe "C:\\pdfs" "C:\\out\\merged.pdf" SORT_BY_DATE_DESC`
- Datum äldst först:
  - `KrPDFmerge.exe "C:\\pdfs" "C:\\out\\merged.pdf" SORT_BY_DATE_ASC`
- Legacy alias (nyast först):
  - `KrPDFmerge.exe "C:\\pdfs" "C:\\out\\merged.pdf" SORT_BY_DATE`

## Körning via GUI
1. Starta `KrPDFmerge.exe` utan argument.
2. Fyll i:
   - `Source folder`
   - `Output file`
3. Klicka `Merge`.

> GUI-körning använder intern standard för sortering (`SORT_INVOICE_FIRST`) eftersom inget tredje argument anges i formuläret.

## Testscript i `bin\\Debug\\test`
Script: `bin\\Debug\\test\\merge-pdfs.cmd`

Funktion:
- Läser in PDF:er från `bin\\Debug\\test\\pdfs`
- Skapar output i `bin\\Debug\\test\\new_pdf`
- Döpning av output: `merged_yyyyMMdd_HHmmss.pdf`
- Kör `..\\KrPDFmerge.exe` relativt testmappen

Användning från testmappen:
- Default:
  - `merge-pdfs.cmd`
- Alfabetisk:
  - `merge-pdfs.cmd SORT_ALPHABETIC`
- Prefix:
  - `merge-pdfs.cmd SORT_PREFIX`
- Datum nyast först:
  - `merge-pdfs.cmd SORT_BY_DATE_DESC`
- Datum äldst först:
  - `merge-pdfs.cmd SORT_BY_DATE_ASC`
- Legacy alias (nyast först):
  - `merge-pdfs.cmd SORT_BY_DATE`

## Loggning och felhantering
- Logg skrivs till: `log\\yyyyMM.txt`
- Loggen innehåller bland annat:
  - exe-sökväg
  - startup path
  - vald källa/mål/sortering
  - filnamn i merge-ordning
  - felmeddelande + stacktrace vid undantag
- Vid fel visas även en `MessageBox` i applikationen.

## Bygga projektet
- Öppna `KrPDFmerge.csproj` i Visual Studio.
- Säkerställ NuGet restore.
- Bygg i `Debug` eller `Release`.

Output hamnar normalt i:
- `bin\\Debug\\KrPDFmerge.exe`
- `bin\\Release\\KrPDFmerge.exe`

## Begränsningar
- Programmet söker endast i angiven mapp, inte rekursivt i undermappar.
- Krypterade/skadade PDF-filer kan ge fel i merge-processen.
- Sortering `SORT_PREFIX` läser bara numeriska tecken i början av filnamnet.
