# ADR-0008 — Autenticazione e autorizzazione con Microsoft Entra ID

- **Stato**: sostituito da [ADR-0022](0022-blueprint-frontend.md) il 2026-10-09 (login con MSAL nel browser e token Bearer). Resta valido l'uso di Entra ID, del tenant personale e degli App Roles.
- **Data**: 2026-10-08
- **Requisiti collegati**: FR-01, FR-02, FR-03, FR-36, FR-37, NFR-01, NFR-04, NFR-05

## Contesto

FlowDesk ha tre ruoli (Admin, Manager, Operator) e regole di accesso per risorsa (l'Operatore vede solo le pratiche assegnate). Serve un sistema di autenticazione sicuro, realistico per un contesto aziendale e che si colleghi al deploy futuro su Azure. Il progetto è personale e pubblico: non può dipendere dal tenant dell'azienda o dell'università dello sviluppatore.

## Decisione

1. **Provider**: Microsoft Entra ID, tenant personale (account Azure gratuito, licenza Entra ID Free), tenant **single tenant**.
2. **Flusso**: OpenID Connect *authorization code*, gestito **lato server** dall'API ASP.NET Core con `Microsoft.Identity.Web`. L'API emette un cookie di sessione `HttpOnly; Secure; SameSite=Lax`. React non vede mai i token.
3. **Hosting**: in produzione la SPA è servita dalla stessa origine dell'API. In sviluppo Vite fa da proxy verso l'API.
4. **Una sola registrazione app** (`FlowDesk`, piattaforma Web, client confidenziale con client secret).
5. **Ruoli**: *App Roles* `Admin`, `Manager`, `Operator`, assegnati ai singoli utenti dall'applicazione aziendale. Arrivano nel claim `roles` e sono mappati su policy di autorizzazione. Un utente ha un solo ruolo. *Assegnazione utente obbligatoria* attiva: chi non ha un ruolo non entra.
6. **Utenti locali**: tabella `Users` (`oid`, `DisplayName`, `Role`, `IsActive`, date), creata al primo accesso (*just-in-time provisioning*) e aggiornata ai successivi. Serve per chiavi esterne, audit e per disattivare un utente subito con `IsActive`. **La fonte di verità del ruolo è Entra ID**.
7. **Autorizzazione**: policy nominate per le azioni, `IAuthorizationHandler` per risorsa per l'accesso alle pratiche (risposta 404 se non visibile), segregazione dei compiti come regola di dominio. Default deny con `FallbackPolicy`.
8. **Schema "demo"**: schema di autenticazione alternativo che permette di scegliere un utente di prova, attivo **solo** se la configurazione lo abilita (sviluppo locale, test automatici, ambiente demo). Vietato in produzione, con controllo all'avvio che rifiuta la combinazione "produzione + demo".
9. **Segreti**: il client secret non entra mai nel repository. `dotnet user-secrets` in locale, Azure Key Vault in produzione. Il secret scade (6 mesi) e va ruotato.

## Alternative considerate

| Alternativa | Perché non scelta |
|---|---|
| ASP.NET Core Identity con password locali | Si gestiscono password, blocco, recupero, MFA: più codice sensibile e meno vicino alle realtà aziendali Microsoft |
| MSAL.js nel browser + API con JWT Bearer (percorso B) | Token nel browser (esposti a XSS), due registrazioni, più codice frontend. Resta una possibile estensione per imparare MSAL |
| Gruppi Entra per i ruoli | L'assegnazione di gruppi ad app richiede Entra ID P1 (a pagamento); con Free si assegnano utenti singoli. Inoltre con molti gruppi il token può non contenerli tutti |
| Tenant aziendale o universitario | Permessi insufficienti, proprietà non tua, account che scade |

## Conseguenze

**Positive**
- Nessuna password, nessun blocco account, nessun recupero da implementare. MFA gratuita (security defaults).
- Il frontend resta semplice (`GET /auth/me`, redirect al login).
- Si impara OIDC, app registration, App Roles, gestione dei segreti: competenze richieste nei ruoli .NET/Azure.
- Prepara l'uso di Key Vault e Azure nel deploy.

**Negative / costi**
- Dipendenza da un servizio esterno anche in sviluppo (serve connessione).
- Chi guarda il progetto non può accedere con Microsoft: serve lo schema demo e un ambiente demo.
- I test automatici non possono usare il login reale: serve uno schema di test e va coperto il rischio che il test non rappresenti il flusso vero (mitigato da una verifica manuale per release).
- Il ruolo si cambia nel portale Entra, non nell'app (compromesso del MVP; estensione con Microsoft Graph).
- Il client secret scade e va ruotato.

## Da verificare nella M1

- Compatibilità di `Microsoft.Identity.Web` con .NET 10.
- Configurazione definitiva delle porte e dei redirect URI (`/signin-oidc`, `/signout-oidc`) per sviluppo e produzione.
- Comportamento del cookie dietro il proxy Vite (host e schema corretti).

## Valori di configurazione

Tenant ID, client ID e client secret **non** sono nel repository: si leggono da `user-secrets` (locale) o Key Vault (produzione). Nel repository restano solo chiavi di configurazione vuote e documentate.
