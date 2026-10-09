# ADR-0003 — Minimal API invece di controller MVC

- **Stato**: sostituito da [ADR-0023](0023-template-backend.md) il 2026-10-09 (controller MVC al posto delle Minimal API) · **Data**: 2026-10-08

## Contesto
Serve un modo per esporre gli endpoint REST dell'API.

## Decisione
Minimal API di ASP.NET Core con gruppi di endpoint per modulo (`MapGroup`), filtri per cross-cutting, OpenAPI nativo di .NET 10. Un file di endpoint per modulo in `FlowDesk.Api/Endpoints/<Module>`.

## Alternative
- **Controller MVC**: ancora molto diffusi nei progetti esistenti.

## Conseguenze
- (+) Meno cerimonia, endpoint come funzioni sottili che delegano ai casi d'uso.
- (+) Policy, binding, filtri e validazione sono gli stessi concetti dei controller, quindi trasferibili.
- (−) Nei codebase aziendali esistenti si incontrano molti controller: ne studieremo le differenze a parte.
