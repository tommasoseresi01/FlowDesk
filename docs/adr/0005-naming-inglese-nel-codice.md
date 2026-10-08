# ADR-0005 — Naming: inglese nel codice e nel database, italiano nell'interfaccia

- **Stato**: accettato · **Data**: 2026-10-08

## Contesto
Il dominio è descritto in italiano (specifica), ma librerie, framework e documentazione tecnica sono in inglese.

## Decisione
Identificatori di codice, tabelle, colonne, endpoint e messaggi di log in **inglese**. Testi dell'interfaccia e messaggi mostrati all'utente in **italiano**, centralizzati in un catalogo. La specifica e gli ADR restano in italiano.

## Glossario IT → EN

| Italiano | Inglese (codice) |
|---|---|
| Cliente | `Customer` |
| Pratica | `Practice` |
| Tipo pratica | `PracticeType` |
| Documento richiesto | `RequiredDocument` |
| Versione documento | `DocumentVersion` |
| Task | `WorkTask` |
| Scadenza | `DueDate` / `Deadline` |
| Audit | `AuditEntry` |
| Amministratore · Responsabile · Operatore | `Admin` · `Manager` · `Operator` |

## Alternative
- **Tutto in italiano**: attrito con librerie e convenzioni.

## Conseguenze
- (+) Vocabolario standard del settore, nessun mix di lingue nelle API.
- (−) Serve il glossario per tradurre i termini di dominio; il catalogo errori mappa `code` → messaggio italiano.
