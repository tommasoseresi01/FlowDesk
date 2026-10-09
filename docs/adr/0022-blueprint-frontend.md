# ADR-0022 — Adozione del blueprint frontend

- **Stato**: accettato · **Data**: 2026-10-09
- **Sostituisce**: ADR-0008 (autenticazione lato server con cookie)
- **Modifica**: ADR-0007 (chiavi), `architecture.md` sezioni 4, 7, 8, 9 e 12, decisioni D-08 e D-09 della specifica

## Contesto

Il primo frontend era stato costruito con Mantine, TanStack Query e dati finti (MSW). L'autore del progetto ha fornito un blueprint di riferimento per SPA gestionali con Entra ID e ha chiesto di adottarne struttura, pattern e convenzioni, eliminando ogni dato finto.

## Decisione

Il frontend segue il blueprint. In sintesi:

1. **Struttura a livelli a senso unico**: `pages` e `modals` → `repositories` → `utils/apiFetch`. Le pagine non chiamano mai `fetch`; ogni dato dell'API passa da un mapper.
2. **Cartelle**: `components` (per tipo), `hooks`, `locales`, `mappers`, `modals` (per funzione), `models`, `modules/main`, `pages`, `providers`, `repositories`, `routes`, `utils`.
3. **Stack**: React 18.2, Vite 5, TypeScript 5.3, React Router 6 (data router), AdminLTE 3 con Bootstrap 4, react-bootstrap 1.x, Material React Table su MUI 5, React Hook Form, react-modal, react-toastify, i18next, date-fns. Niente store globale: lo stato condiviso vive nei Context provider.
4. **Autenticazione**: MSAL nel browser (`@azure/msal-browser`, `@azure/msal-react`), flusso authorization code con PKCE, token Bearer su ogni chiamata. La registrazione in Entra ID diventa di tipo **SPA** ed espone uno scope dell'API.
5. **Contratto API**: ogni risposta ha l'envelope `{success, result, errors, totResultNumber}`. Per ogni entità: `GET /list`, `POST /search`, `GET /{id}`, `POST`, `PUT`, `DELETE /{id}`. Endpoint di sistema: `GET /users/current` e `GET /users/menu`.
6. **Identificativi numerici** chiamati `id<Entity>`; `0` significa "nuovo record".
7. **Ruoli** numerici restituiti da `/users/current` (`RoleEnum`: Admin 1, Manager 2, Operator 3). Il menu arriva dal backend già filtrato per l'utente.
8. **Nessun dato finto**: senza backend l'app si ferma al login e poi mostra "utente non abilitato".

## Adattamenti rispetto al blueprint

| Punto | Scelta | Motivo |
|---|---|---|
| Lingua | Solo `it`, lingua di default | ADR-0005: interfaccia in italiano |
| Entità di esempio | `Customer` al posto di `Item`, senza entità di lookup | Prima funzione del dominio; il cliente non ha lookup |
| Cancellazione | `DELETE /customers/{id}` archivia; `POST /customers/{id}/restore` ripristina | BR-06: nessuna cancellazione fisica |
| Pagina di dettaglio a tab e `@mui/lab` | Non incluse | Servono dalla funzione Pratiche |
| `useSessionStorage`, `blobUtils` e funzioni non usate | Non inclusi | Il blueprint vieta il codice morto; si aggiungono quando servono |
| `@fortawesome/fontawesome-free` | Dipendenza esplicita, importata in `index.css` | La cartella `plugins` di AdminLTE nasce dal suo script di installazione, che resta disattivato |
| `npm install --ignore-scripts` | Obbligatorio in locale e in CI | Lo script di una dipendenza indiretta di AdminLTE (`summernote`) fallisce |
| `MenuItem` | `<button>` al posto di `<a role="button">`, senza `setState` dentro un effetto | Accessibilità e regole di lint del progetto |
| Test e lint | Vitest per mapper, utility, repository e `useApiError`; oxlint | Il blueprint raccomanda i test unitari; la CI li esegue |

## Conseguenze

**Positive**
- Struttura prevedibile: ogni nuova entità si clona seguendo la stessa ricetta (DTO, mapper, repository, traduzioni, modali, pagina, rotta).
- Nessun dato finto da mantenere allineato al backend.
- Il backend ha un contratto preciso da implementare.

**Negative**
- Versioni non recenti (React 18, Vite 5, MUI 5, Bootstrap 4): `npm audit` segnala vulnerabilità note. Vanno valutate prima del deploy pubblico.
- Con MSAL i token vivono nel browser (`localStorage`): un XSS avrebbe più impatto rispetto al cookie `HttpOnly` dell'ADR-0008. La mitigazione è non introdurre HTML non fidato e tenere aggiornate le dipendenze.
- Nessuna cache client: ogni pagina ricarica i dati.
- Il frontend non è verificabile end-to-end finché il backend non esiste.

## Da fare prima di costruire il backend

1. **Entra ID**: aggiungere la piattaforma *Single-page application* con redirect `http://localhost:5173`; esporre l'API (`api://<client-id>`) con lo scope `access_as_user`. Il client secret non serve al frontend.
2. **Backend**: validare il token Bearer (JWT) al posto del cookie; mappare gli App Roles del token sui ruoli numerici; implementare `/users/current`, `/users/menu` e gli endpoint dei clienti con l'envelope.
3. **Documenti da riallineare**: `architecture.md` (sezioni 4, 7, 8, 9, 12), ADR-0007 (chiavi numeriche al posto dei GUID v7), ADR-0010 non ancora scritto (envelope al posto di ProblemDetails), requisiti FR-01, FR-36 e FR-37 della specifica (lo schema di login demo non esiste più nel frontend).
