# ADR-0023 — Adozione del template di architettura backend

- **Stato**: accettato · **Data**: 2026-10-09
- **Sostituisce**: ADR-0003 (Minimal API) e ADR-0004 (handler senza MediatR)
- **Modifica**: ADR-0007 (chiavi numeriche invece di GUID), ADR-0008 (la validazione del token sta nel backend), ADR-0022 (contratto API ora implementato)

## Contesto

Il frontend segue un blueprint (ADR-0022) e chiama un contratto API preciso. Per il backend l'autore ha fornito un template di architettura a livelli e ha chiesto di adottarlo e di far rispondere l'API a quelle chiamate.

## Decisione

Il backend segue il template. In sintesi:

1. **Progetti**: `FlowDesk.Domain`, `FlowDesk.Application`, `FlowDesk.Infrastructure`, `FlowDesk.Web`. Regola di dipendenza verso l'interno, verificata da test architetturali (il Domain e l'Application non conoscono nemmeno EF Core).
2. **Controller MVC** con `[Authorize]` sulla classe e i ruoli sulle azioni. Nessun `try/catch` nei controller.
3. **Servizi applicativi** che usano `IUnitOfWork` e i repository; **mapper statici** scritti a mano; **FluentValidation** con validazione automatica; il mapping verso i DTO avviene nel controller.
4. **Envelope di risposta** `{success, result, errors, totResultNumber}` per ogni risposta, anche per 401, 403, 404, 422 e 500.
5. **Autenticazione**: il backend valida soltanto il token JWT di Entra ID (accetta token v1 e v2). Non esistono endpoint di login.
6. **Database**: EF Core con configurazioni Fluent API, repository per aggregato, chiavi `Id<Entità>` numeriche, cancellazione logica.
7. **Routing**: i controller rispondono sia su `api/<controller>` (usato dal frontend) sia su `api/v1.0/<controller>` (versionato, come nel template).

## Dove si discosta dal template

| Punto | Template | Qui | Motivo |
|---|---|---|---|
| Versione | .NET 8, EF Core 8 | .NET 10, EF Core 10 | ADR D-10; SDK già installato e pinnato |
| Origine del ruolo | Salvato nel database, assegnato da un amministratore | **App Roles di Entra ID**, claim `roles`; l'utente si crea al primo accesso e il ruolo si allinea a ogni accesso | ADR-0008 e configurazione già fatta su Azure |
| Utente sconosciuto o disattivato | L'autenticazione riesce senza claim applicativi (miglioria suggerita dal template) | `ctx.Fail`: 401 se manca un ruolo applicativo o l'utente è disattivato | Migliorie del template, applicate subito |
| Audit | Overload `SaveChangesAsync(int idUser)` chiamato da ogni repository | `SaveChangesInterceptor` che legge `ICurrentUserService` | Miglioria del template: non si può aggirare e toglie `idCurrentUser` da tutte le firme |
| `ICurrentUserService` | Nel progetto Web | Interfaccia in Application, implementazione in Web | Miglioria del template |
| Nome del progetto host | `...Web` | `FlowDesk.Web` | Allineato (prima si chiamava `Api`) |
| Documentazione API | Swashbuckle con login OAuth2 | OpenAPI integrato di .NET 10 (`/openapi/v1.json`, solo Development), senza interfaccia | Swashbuckle e Microsoft.OpenApi 2 hanno cambiato API in modo incompatibile; si può aggiungere un'interfaccia in seguito |
| Schema del database | Gestito a mano nel database | **Migrazioni EF Core** | Miglioria "alta" del template: lo schema è versionato e ricreabile |
| Ordinamento dinamico | Per nome di colonna | Stesso, ma solo su valori semplici; una colonna sconosciuta ripiega sull'ordine di default | Evita ordinamenti su collezioni o oggetti e un 500 per un nome sbagliato |
| Eccezioni | Tutte 500 | Mappate: regola di business 422, non trovato 404, vietato 403, il resto 500 con messaggio generico | Miglioria del template |
| Policy di default | Nessuna | `FallbackPolicy`: tutto richiede un utente autenticato, salvo gli health check | Miglioria del template; un test lo verifica |
| Logging | Serilog su console e tabella SQL | Serilog solo su console | Il file `appsettings.Development.json` non era modificabile in questa sessione: il sink SQL è una modifica di configurazione |
| Route di creazione e modifica | `POST .../create`, `PUT .../edit` | `POST /customers`, `PUT /customers` | Le chiama così il frontend (blueprint) |
| Non incluso | Azure Functions, code, blob, SendGrid, Graph | Non incluso | Non servono finché non esistono funzioni asincrone, documenti ed email |

## Conseguenze

**Positive**
- Struttura prevedibile: ogni nuova entità segue la ricetta del template (entità, configurazione, repository, DTO, request, response, mapper, validator, servizio, controller, migrazione).
- Il contratto con il frontend è coperto da test di integrazione su un SQL Server vero.
- L'audit non dipende dalla disciplina dei chiamanti.

**Negative / da sapere**
- `FluentValidation.AspNetCore` non è più mantenuto: funziona, ma è l'ultima versione (11.3). In futuro andrà sostituito con la validazione manuale o con un filtro.
- Il ruolo vive in Entra ID: per cambiarlo si usa il portale Azure, non l'applicazione (estensione possibile con Microsoft Graph).
- Una query al database per richiesta (l'utente corrente); la scrittura dell'ultimo accesso è limitata a una ogni quindici minuti. Una cache breve è un'ottimizzazione futura.
- Il login Microsoft reale non è coperto da test automatici: i test usano uno schema di autenticazione di prova che emette gli stessi claim.

## Da fare

- Aggiungere il sink SQL di Serilog quando si potrà modificare `appsettings.Development.json`.
- Riallineare `docs/architecture.md` (sezioni su API, errori e test) a questo ADR.
- Valutare un'interfaccia per la documentazione OpenAPI.
