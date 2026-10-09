# FlowDesk

Applicazione aziendale per gestire **clienti, pratiche, documenti richiesti, task, scadenze e audit**, con ruoli e accesso tramite Microsoft Entra ID.

> **Stato: in sviluppo.** Frontend e backend sono collegati: login con Microsoft, utenti e ruoli, e la gestione dei clienti. Il progetto è sviluppato per imparare l'intero ciclo di un'applicazione enterprise, dai requisiti al deploy.

## Contesto simulato

Meridiana Consulting S.r.l. (azienda fittizia) segue circa 120 clienti per pratiche ricorrenti. Oggi documenti, scadenze e responsabilità vivono in email e fogli Excel. FlowDesk rende ogni pratica tracciata: cosa manca, chi deve fare cosa, entro quando, e chi ha deciso cosa.

## Stack

| Livello | Tecnologie |
|---|---|
| Backend | ASP.NET Core Web API (.NET 10), C#, controller MVC, architettura a livelli |
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
| M0 | Fondamenta: documenti, ADR, repository, CI | Fatto |
| M1 | Identità con Entra ID e ruoli | Fatto (da verificare con un login reale) |
| M2 | Clienti | Fatto |
| M3 | Audit e pratiche | Da fare |
| M4 | Documenti | Da fare |
| M5 | Task | Da fare |
| M6 | Scadenze e notifiche | Da fare |
| M7 | Audit in lettura e dashboard | Da fare |
| M8 | Seeding e hardening → **MVP** | Da fare |
| M9 | Qualità e osservabilità | Da fare |
| M10 | Deploy su Azure e versione portfolio | Da fare |

## Come eseguire il progetto

### Backend

Requisiti: .NET SDK 10, SQL Server (anche Express o LocalDB) e lo strumento `dotnet-ef` (`dotnet tool install -g dotnet-ef`).

Una sola volta, dalla radice del repository. I valori non finiscono in git: restano nei *user secrets* del tuo PC.

```bash
dotnet user-secrets set "ConnectionStrings:AppDbContext" "Server=localhost;Database=FlowDesk;Trusted_Connection=True;TrustServerCertificate=True" --project src/FlowDesk.Web
dotnet user-secrets set "AzureAd:TenantId" "<ID directory (tenant)>" --project src/FlowDesk.Web
dotnet user-secrets set "AzureAd:ClientId" "<ID applicazione (client) della registrazione in Entra ID>" --project src/FlowDesk.Web
dotnet ef database update --project src/FlowDesk.Infrastructure --startup-project src/FlowDesk.Web
```

Poi, per avviare l'API:

```bash
dotnet run --project src/FlowDesk.Web --launch-profile https
```

L'API ascolta su `https://localhost:7080`. Il browser deve fidarsi del certificato di sviluppo: se non è già così, `dotnet dev-certs https --trust`.

Verifiche rapide: `https://localhost:7080/health/live` (processo attivo) e `https://localhost:7080/health/ready` (database raggiungibile). In Development la descrizione OpenAPI è su `https://localhost:7080/openapi/v1.json`.

Gli utenti non si creano a mano: al primo accesso con Microsoft l'API registra l'utente nella tabella `Users`, con il ruolo dell'App Role che ha in Entra ID (`Admin`, `Manager` o `Operator`). Chi non ha un ruolo assegnato riceve 401 e il frontend mostra "utente non abilitato".

Test e controlli:

```bash
dotnet build FlowDesk.slnx
dotnet test FlowDesk.slnx
dotnet format FlowDesk.slnx --verify-no-changes
```

I test di integrazione usano un SQL Server vero e ricreano il database `FlowDesk_Test` a ogni esecuzione. Per usare un altro server, imposta la variabile d'ambiente `FLOWDESK_TEST_CONNECTION`.

### Frontend (richiede Node 18 o superiore)

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

Il frontend non contiene dati finti: dopo il login Microsoft chiama l'API. `VITE_API_URL` può anche finire con `/api/v1.0`: l'API risponde su entrambi i prefissi.

Altri comandi in `web/`: `npm run lint`, `npm run typecheck`, `npm test`, `npm run build`, `npm run format`.

## Licenza

[MIT](LICENSE)
