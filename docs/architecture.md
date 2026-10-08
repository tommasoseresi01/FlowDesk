# FlowDesk — Architettura tecnica (MVP)

> Stato: **v0.1**. Le decisioni sono motivate negli ADR in `docs/adr/`. Il requisito di riferimento è `docs/functional-spec.md`.

## 1. Vista logica

```
┌──────────────────────── Browser ────────────────────────┐
│ React + TypeScript SPA (Vite)                           │
│ router · TanStack Query · form · client API generato    │
└──────────────┬──────────────────────────────────────────┘
               │ HTTPS · JSON · cookie HttpOnly + header anti-CSRF
┌──────────────▼──────────── ASP.NET Core (un processo) ──────────────┐
│ FlowDesk.Api            endpoint per modulo · auth · errori · OpenAPI│
│        │ chiama                                                      │
│ FlowDesk.Application    casi d'uso · validazione · porte · DTO       │
│        │ usa                        ▲ implementa le porte            │
│ FlowDesk.Domain                     │ FlowDesk.Infrastructure        │
│  aggregate · regole · state machine │  EF Core · storage · clock·job │
└──────────────────────────────────────┬───────────────────────────────┘
                                       │
                         ┌─────────────▼────────────┐   ┌─────────────┐
                         │ SQL Server               │   │ File storage│
                         │ schemi identity/core/audit│  │ (disco→Blob)│
                         └──────────────────────────┘   └─────────────┘
Servizio esterno: Microsoft Entra ID (login OIDC) — vedi ADR-0008
```

**Regola di dipendenza**: `Api → Application → Domain` e `Infrastructure → Application → Domain`. Il Domain non dipende da nessuno. L'Api è la *composition root*. (ADR-0002)

## 2. Confini di responsabilità

| Livello | È responsabile di | Non deve |
|---|---|---|
| React | Presentazione, navigazione, stato di UI, validazione di comodo, nascondere azioni non permesse | Decidere permessi, applicare regole di business |
| API (Api + Application) | Autenticazione/autorizzazione, validazione definitiva, orchestrazione, transazioni, mapping errori | Contenere regole di dominio |
| Domain | Invarianti, transizioni di stato, guard | Conoscere HTTP, EF o file system |
| SQL Server | Persistenza, integrità (FK, unique, check), concorrenza (`rowversion`), immutabilità dell'audit | Logica di business |

## 3. Struttura della solution (da creare in M0.2)

```
FlowDesk/
├─ FlowDesk.slnx · global.json · Directory.Build.props · Directory.Packages.props
├─ src/
│  ├─ FlowDesk.Domain/          Common, Customers, Practices, Documents, Tasks, Notifications, Audit
│  ├─ FlowDesk.Application/     Common (pipeline, porte), un'area per modulo (Commands/Queries/Dto/Validators)
│  ├─ FlowDesk.Infrastructure/  Persistence (DbContext, Configurations, Migrations, Seed), Storage, BackgroundJobs, Time
│  └─ FlowDesk.Api/             Endpoints/<Module>, Auth, Errors, Program.cs, wwwroot
├─ tests/
│  ├─ FlowDesk.Domain.Tests · FlowDesk.Application.Tests
│  ├─ FlowDesk.Api.IntegrationTests · FlowDesk.Architecture.Tests
├─ web/  docs/  scripts/  .github/
```

Le migrazioni stanno in Infrastructure e si lanciano con `--startup-project FlowDesk.Api`; non si modificano a mano.

## 4. Struttura del frontend (da creare in M0.3)

```
web/src/
  app/       provider (Query, Router, Theme), layout, route guard
  api/       client generato dall'OpenAPI + wrapper (errori, CSRF)
  features/  auth, users, customers, practices, documents, tasks, deadlines,
             notifications, audit, dashboard  → api/ components/ pages/ schemas/
  shared/    componenti UI comuni, hook, formattazione, accessibilità
  test/      setup Vitest, handler MSW
web/e2e/     Playwright
```

Stack: React, TypeScript `strict`, Vite, React Router, TanStack Query, React Hook Form + Zod, Mantine, ESLint con `jsx-a11y`, Vitest + Testing Library + MSW + vitest-axe, Playwright.

## 5. Modello di dominio

```
Customer (AR) 1──* Practice (AR)
PracticeType (AR) 1──* Practice          [checklist e task COPIATI alla creazione]
PracticeType ├─ DocumentTemplate*  └─ TaskTemplate*
Practice (AR) ├─ RequiredDocument* └─ DocumentVersion*  (immutabile)
WorkTask (AR) *──1 Practice               [riferimento per Id]
Notification (AR) *──1 User
AuditEntry  (append-only)
User  (locale, creato al primo accesso da Entra ID; ruolo dal claim `roles`)
```

| Aggregate | Invarianti | Comportamenti principali |
|---|---|---|
| Customer | P.IVA valida e unica; archiviato ⇒ nessuna nuova pratica | Rename, Archive, Restore |
| PracticeType | Nome unico | Gestione template |
| Practice | State machine (spec 6.1); stati finali; motivazioni; segregazione sui documenti | Open, Start, RequestDocuments, SubmitForReview, Complete, SendBack, Cancel, Reassign, ChangeDueDate, UploadVersion, ReviewVersion, WaiveDocument |
| WorkTask | Transizioni (spec 6.3); scadenza ≤ pratica; completato ⇒ immutabile | Start, Block, Complete, Reopen, Reassign |

`RequiredDocument` e `DocumentVersion` stanno dentro `Practice` (le guard richiedono consistenza immediata). `WorkTask` è un aggregate separato; la guard "nessun task aperto" legge tramite una porta di query. Compromesso e alternativa in ADR-0011.

Value object: `VatNumber`, `EmailAddress`, `Reason` (min. 10 caratteri), `PracticeReference` (es. `PR-2026-0001`), `DueDate`.

## 6. Schema SQL Server preliminare

Schemi `identity`, `core`, `audit`. Nessun cascade delete. Chiavi GUID v7 (audit: `bigint IDENTITY`). Timestamp `datetime2(3)` UTC, scadenze `date`, stati come stringhe con CHECK, `RowVersion` sulle entità modificabili. (ADR-0007)

| Tabella | Colonne principali | Vincoli / indici |
|---|---|---|
| `core.Users` | `Id`, `EntraObjectId`, `DisplayName`, `Role`, `IsActive`, `LastLoginAtUtc` | UNIQUE(`EntraObjectId`) |
| `core.Customers` | `Id`, `LegalName`, `VatNumber`, `ContactName`, `Email`, `Phone`, `IsArchived`, `CreatedAtUtc`, `RowVersion` | UNIQUE(`VatNumber`); indice `LegalName` |
| `core.PracticeTypes` | `Id`, `Name`, `DefaultDurationDays`, `IsActive` | UNIQUE(`Name`) |
| `core.PracticeTypeDocuments` / `PracticeTypeTasks` | `Id`, `PracticeTypeId`, nome/titolo, `IsRequired`, ordine/offset | |
| `core.Practices` | `Id`, `Reference`, `CustomerId`, `PracticeTypeId`, `Status`, `AssignedToUserId`, `ManagerUserId`, `DueDate`, `CancellationReason`, `RowVersion` | UNIQUE(`Reference`); CHECK `Status`; indici (`Status`,`DueDate`), (`AssignedToUserId`,`Status`), (`CustomerId`) |
| `core.RequiredDocuments` | `Id`, `PracticeId`, `Name`, `IsRequired`, `Status`, `WaivedReason`, `RowVersion` | indice `PracticeId` |
| `core.DocumentVersions` | `Id`, `RequiredDocumentId`, `VersionNo`, nomi file, `ContentType`, `SizeBytes`, `Sha256`, upload e review (utente/data/esito/motivo) | UNIQUE(`RequiredDocumentId`,`VersionNo`); CHECK dimensione ≤ 10 MB; CHECK rifiuto ⇒ motivo |
| `core.WorkTasks` | `Id`, `PracticeId`, `Title`, `AssignedToUserId`, `DueDate`, `Status`, `IsRequired`, `BlockedReason`, completamento, `RowVersion` | indici (`AssignedToUserId`,`Status`,`DueDate`), (`PracticeId`) |
| `core.Notifications` | `Id`, `UserId`, `Type`, `Message`, riferimento entità, `DedupeKey`, `CreatedAtUtc`, `ReadAtUtc` | UNIQUE(`UserId`,`DedupeKey`) |
| `audit.AuditEntries` | `Id`, `OccurredAtUtc`, utente (id, nome, ruolo), `Action`, `EntityType`, `EntityId`, `PracticeId`, `BeforeJson`, `AfterJson`, `Reason`, `CorrelationId`, `IpAddress` | indici (`PracticeId`,`Id`), (`UserId`,`OccurredAtUtc`); trigger `INSTEAD OF UPDATE, DELETE` che rifiuta |

## 7. API REST (sintesi)

Base `/api/v1`, JSON camelCase, paginazione `?page=&pageSize=&sort=`, mutazioni con header anti-CSRF e `rowVersion` nel corpo. Legenda: A Admin · R Responsabile (Manager) · O Operatore · O\* solo se la pratica è sua.

| Area | Endpoint | Chi |
|---|---|---|
| Auth | `GET /auth/login` (redirect a Microsoft), `POST /auth/logout`, `GET /auth/me` | tutti |
| Utenti | `GET /users`, `POST /users/{id}/deactivate`, `/activate` | A |
| Clienti | `GET /customers`, `POST`, `PUT /{id}`, `POST /{id}/archive`, `/restore` | O, R (archive/restore: R) |
| Tipi pratica | `GET /practice-types`; scrittura (Should) | tutti / A |
| Pratiche | `GET /practices`, `POST`, `GET /{id}`, `PUT /{id}`, `POST /{id}/assign` (R), `POST /{id}/transitions`, `POST /{id}/due-date`, `GET /{id}/audit` | O\*, R |
| Documenti | `GET /practices/{id}/documents`, `POST /documents/{docId}/versions` (multipart), `GET /documents/{docId}/versions/{n}/file`, `POST …/review` (R), `POST /documents/{docId}/waive` (R) | O\*, R |
| Task | `GET /practices/{id}/tasks`, `POST`, `GET /tasks/mine`, `POST /tasks/{id}/transitions`, `POST /tasks/{id}/reassign` (R) | O\*, R |
| Scadenze | `GET /deadlines` | O\*, R |
| Notifiche | `GET /notifications`, `GET /unread-count`, `POST /{id}/read`, `/read-all` | tutti (proprie) |
| Audit | `GET /audit` | A, R |
| Dashboard | `GET /dashboard/operator`, `/dashboard/manager` | O / R |
| Salute | `GET /health/live`, `/health/ready` | anonimo |

Esempio di errore (guard non soddisfatta) — `409 application/problem+json`:

```json
{ "type": "https://flowdesk/errors/practice.transition.guard_failed",
  "title": "La pratica non può passare in revisione", "status": 409,
  "code": "practice.transition.guard_failed",
  "missing": [{ "kind": "document", "id": "…", "name": "Visura camerale" }],
  "traceId": "00-…" }
```

## 8. Autenticazione e autorizzazione

Vedi ADR-0008 (Microsoft Entra ID, flusso lato server, cookie, App Roles) e ADR-0009 (policy, handler per risorsa, 404 invece di 403, default deny).

## 9. Errori e validazione

ProblemDetails (RFC 9457) con estensioni `code`, `traceId`, `errors` (per campo) e `missing`. Mappatura: validazione 400 · non autenticato 401 · vietato 403 · non trovato/non visibile 404 · transizione o regola violata 409 · valore non ammesso 422 · conflitto di concorrenza 409 con `code = concurrency.conflict` · imprevisto 500 generico con `traceId`. Result pattern per gli esiti di business attesi, eccezioni per l'inatteso, un `IExceptionHandler` globale. (ADR-0010)

## 10. Audit

Ogni azione rilevante solleva eventi di dominio; un decorator dell'handler li converte in `AuditEntry` nello stesso `DbContext`, quindi nella stessa transazione (se l'audit fallisce, fallisce l'azione). Immutabilità con trigger SQL; nessun endpoint di modifica. (ADR-0012)

## 11. Osservabilità

Serilog con log strutturati e correlation id (senza dati sensibili), OpenTelemetry per tracce e metriche, health check `live`/`ready`. In locale su console/file; su Azure Application Insights. (ADR-0021)

## 12. Strategia di test

| Livello | Cosa | Strumenti |
|---|---|---|
| Dominio | State machine, guard, segregazione (test parametrici) | xUnit |
| Application | Handler con fake delle porte | xUnit |
| Integrazione API | Flussi su SQL Server reale: autorizzazioni, 401/403/404/409, concorrenza, audit, upload | `WebApplicationFactory`, DB di test, Respawn, schema di auth di test |
| Matrice autorizzazioni | Ruolo × endpoint; fallisce se un endpoint è senza policy | xUnit |
| Architettura | Regole di dipendenza tra livelli, niente `DateTime.Now` | NetArchTest |
| Contratto | Snapshot OpenAPI | Verify |
| Frontend | Componenti e hook con API simulate; accessibilità | Vitest, Testing Library, MSW, vitest-axe |
| E2E | 2–3 flussi | Playwright + axe |

I test di integrazione usano SQL Server reale (non EF InMemory/SQLite) per fedeltà su `rowversion`, indici filtrati e trigger. Obiettivo copertura ≥ 70% su Domain e Application. (ADR-0018)

## 13. Docker

Percorso predefinito: **nessun Docker** fino a M9 (processi locali, SQL Server locale, `user-secrets`). Docker Compose con profili `db` e `full` solo in M10. (ADR-0019)

## 14. Convenzioni Git e CI

GitHub Flow, branch brevi, squash merge, Conventional Commits, backlog come Issue/Milestone, tag SemVer. CI minima su GitHub Actions: backend (build `-warnaserror`, `dotnet format`, test con SQL Server in service container), frontend (lint, `tsc`, Vitest, build), sicurezza (pacchetti vulnerabili, CodeQL). (ADR-0020)

## 15. Piano di implementazione

M0 Fondamenta → M1 Identità (Entra ID) → M2 Clienti → M3 Audit e pratiche → M4 Documenti → M5 Task → M6 Scadenze e notifiche → M7 Audit in lettura e dashboard → M8 Seeding e hardening (**MVP**) → M9 Qualità e osservabilità → M10 Portfolio e Azure.

Ogni passo è una PR piccola che lascia `main` verde. L'infrastruttura di audit arriva **prima** delle funzioni che la usano (M3).

## 16. Indice ADR

| ADR | Titolo | Stato |
|---|---|---|
| 0001 | Monolite modulare | Accettato |
| 0002 | Livelli e regola di dipendenza | Accettato |
| 0003 | Minimal API | Accettato |
| 0004 | Casi d'uso senza MediatR | Accettato |
| 0005 | Naming: inglese nel codice, italiano nella UI | Accettato |
| 0006 | Gestione del tempo | Accettato |
| 0007 | Persistenza con EF Core | Accettato |
| 0008 | Autenticazione con Entra ID | Accettato |
| 0020 | Git workflow e CI | Accettato |
| 0009–0019, 0021 | Vedi piano: da scrivere nelle milestone indicate | Da scrivere |
