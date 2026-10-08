# ADR-0007 — Persistenza con EF Core code-first su SQL Server

- **Stato**: accettato · **Data**: 2026-10-08

## Contesto
Serve un modello dati relazionale con integrità, concorrenza ottimistica e migrazioni versionate.

## Decisione
- **EF Core 10 code-first** con `IEntityTypeConfiguration` per ogni entità e migrazioni in `FlowDesk.Infrastructure`.
- `NoTracking` di default; si traccia esplicitamente dove si modifica. `SplitQuery` come comportamento globale.
- **Chiavi GUID v7** generate dall'applicazione (`Guid.CreateVersion7()`); `bigint IDENTITY` per `audit.AuditEntries`.
- Schemi `identity`, `core`, `audit`. Nessun cascade delete: l'applicazione non cancella fisicamente.
- Stati come stringhe con vincolo CHECK. `rowversion` per la concorrenza.
- Riferimento umano di pratica da una `SEQUENCE` (es. `PR-2026-0001`).
- Le migrazioni si creano solo con la CLI e non si modificano a mano.

## Alternative
- **Dapper**: più controllo, ma l'obiettivo è imparare EF Core. Query di sola lettura complesse (dashboard) possono usare proiezioni LINQ o SQL mirato.
- **Chiavi `int IDENTITY`**: enumerabili e con round-trip per ottenere l'id.

## Conseguenze
- (+) Chiavi quasi sequenziali, non enumerabili, generate senza accesso al DB.
- (+) Concorrenza e integrità dichiarate nello schema.
- (−) Con `NoTracking` le modifiche richiedono attenzione esplicita (documentata nelle convenzioni).
