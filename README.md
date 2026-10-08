# FlowDesk

Applicazione aziendale per gestire **clienti, pratiche, documenti richiesti, task, scadenze e audit**, con ruoli e accesso tramite Microsoft Entra ID.

> **Stato: M0 — fondamenta (solo documentazione).** Non c'è ancora codice. Il progetto è sviluppato per imparare l'intero ciclo di un'applicazione enterprise, dai requisiti al deploy.

## Contesto simulato

Meridiana Consulting S.r.l. (azienda fittizia) segue circa 120 clienti per pratiche ricorrenti. Oggi documenti, scadenze e responsabilità vivono in email e fogli Excel. FlowDesk rende ogni pratica tracciata: cosa manca, chi deve fare cosa, entro quando, e chi ha deciso cosa.

## Stack

| Livello | Tecnologie |
|---|---|
| Backend | ASP.NET Core Web API (.NET 10), C#, Minimal API |
| Database | SQL Server, Entity Framework Core |
| Frontend | React, TypeScript, Vite |
| Identità | Microsoft Entra ID (OpenID Connect) |
| Qualità | xUnit, Vitest, Playwright, GitHub Actions |
| Deploy (futuro) | Azure (App Service, Azure SQL, Blob Storage) |

## Documentazione

- [Specifica funzionale](docs/functional-spec.md): requisiti, ruoli, stati, user story, casi limite, milestone.
- [Architettura tecnica](docs/architecture.md): componenti, struttura, modello dati, API, test, CI.
- [Decisioni architetturali (ADR)](docs/adr/): le scelte e le alternative scartate.

## Roadmap

| Milestone | Obiettivo | Stato |
|---|---|---|
| M0 | Fondamenta: documenti, ADR, repository, CI | In corso |
| M1 | Identità con Entra ID, ruoli, schema demo | Da fare |
| M2 | Clienti | Da fare |
| M3 | Audit e pratiche | Da fare |
| M4 | Documenti | Da fare |
| M5 | Task | Da fare |
| M6 | Scadenze e notifiche | Da fare |
| M7 | Audit in lettura e dashboard | Da fare |
| M8 | Seeding e hardening → **MVP** | Da fare |
| M9 | Qualità e osservabilità | Da fare |
| M10 | Deploy su Azure e versione portfolio | Da fare |

## Come eseguire il progetto

Non ancora disponibile: le istruzioni arriveranno con M1.

## Licenza

[MIT](LICENSE)
