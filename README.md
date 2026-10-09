# FlowDesk

Applicazione aziendale per gestire **clienti, pratiche, documenti richiesti, task, scadenze e audit**, con ruoli e accesso tramite Microsoft Entra ID.

> **Stato: M0 — fondamenta.** C'è lo scheletro della solution, senza funzionalità. Il progetto è sviluppato per imparare l'intero ciclo di un'applicazione enterprise, dai requisiti al deploy.

## Contesto simulato

Meridiana Consulting S.r.l. (azienda fittizia) segue circa 120 clienti per pratiche ricorrenti. Oggi documenti, scadenze e responsabilità vivono in email e fogli Excel. FlowDesk rende ogni pratica tracciata: cosa manca, chi deve fare cosa, entro quando, e chi ha deciso cosa.

## Stack

| Livello | Tecnologie |
|---|---|
| Backend | ASP.NET Core Web API (.NET 10), C#, Minimal API |
| Database | SQL Server, Entity Framework Core |
| Frontend | React 18, TypeScript, Vite, AdminLTE 3 (Bootstrap 4), Material React Table |
| Identità | Microsoft Entra ID con MSAL nel browser e token Bearer verso l'API |
| Qualità | xUnit, Vitest, GitHub Actions |
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

Requisiti: .NET SDK 10.

```bash
dotnet build FlowDesk.slnx
dotnet test FlowDesk.slnx
dotnet run --project src/FlowDesk.Api
```

L'API risponde su `https://localhost:7080/health/live`. Per ora espone solo il controllo di salute.

Frontend (richiede Node 18 o superiore):

```bash
cd web
npm install --ignore-scripts
npm run dev
```

`--ignore-scripts` è necessario: lo script di installazione di una dipendenza indiretta di AdminLTE fallisce.

Prima di avviare, crea `web/.env.local` (ignorato da git) con questi valori. Non contengono segreti: le variabili `VITE_*` finiscono in chiaro nel bundle.

```dotenv
VITE_AUTH_CLIENT_ID=<ID applicazione (client) della registrazione in Entra ID>
VITE_AUTH_TENANT_ID=<ID directory (tenant)>
VITE_AUTH_SCOPE=api://<ID applicazione>/access_as_user
VITE_AUTH_REDIRECT_URI=http://localhost:5173
VITE_API_URL=https://localhost:7080/api
```

Il frontend non contiene dati finti: dopo il login Microsoft chiama l'API. Finché il backend non espone `/users/current`, l'app mostra la pagina "utente non abilitato".

Altri comandi in `web/`: `npm run lint`, `npm run typecheck`, `npm test`, `npm run build`, `npm run format`.

## Licenza

[MIT](LICENSE)
