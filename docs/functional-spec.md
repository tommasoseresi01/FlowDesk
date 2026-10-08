# FlowDesk — Specifica funzionale MVP

> Stato: **bozza v0.2**. Autenticazione e autorizzazione delegate a **Microsoft Entra ID** (vedi `docs/adr/0008-autenticazione-entra-id.md`). Nessun codice né scaffold tecnico in questa fase.
> Legenda livelli: **FE** frontend React · **BE** backend ASP.NET Core · **DB** SQL Server/EF Core · **TEST** test automatici.
> Priorità: **M** Must (MVP) · **S** Should (MVP se c'è tempo) · **C** Could (versione portfolio).

---

## 1. Product Vision

**FlowDesk** è l'applicazione interna con cui una società di servizi gestisce clienti e pratiche dall'apertura alla chiusura: sa sempre *quali documenti mancano*, *chi deve fare cosa*, *entro quando*, e può dimostrare *chi ha fatto cosa e quando*.

- **Obiettivo di prodotto**: sostituire email, fogli Excel e cartelle condivise con un unico flusso tracciato.
- **Obiettivo di apprendimento**: coprire l'intero ciclo di un'applicazione enterprise (requisiti → progettazione → implementazione → test → deploy → osservabilità) con ASP.NET Core, SQL Server/EF Core e React/TypeScript.
- **Principio guida**: poche entità, regole di business vere, tracciabilità ovunque. Profondità prima che ampiezza.

**Metriche di successo del MVP**
1. Una pratica completa (cliente → documenti validati → task chiusi → completata) eseguibile end-to-end da tre ruoli diversi.
2. 100% delle modifiche di stato e delle azioni sensibili presenti nell'audit log.
3. Nessun endpoint accessibile senza i permessi del ruolo (verificato da test automatici).
4. Copertura test ≥ 70% su dominio e regole di business; test di integrazione sui flussi principali.
5. Dashboard con numeri coerenti con i dati (verificato da test).

## 2. Problema e contesto aziendale simulato

**Azienda fittizia: Meridiana Consulting S.r.l.** Società di consulenza amministrativa con 30 persone. Segue circa 120 aziende clienti per pratiche ricorrenti: *Avvio attività*, *Rinnovo autorizzazione*, *Adempimento fiscale periodico*, *Variazione societaria*.

**Problemi attuali**
- I documenti richiesti ai clienti sono tracciati in email: nessuno sa con certezza cosa manchi.
- Le scadenze stanno in un foglio Excel aggiornato a mano; capita di perderne una.
- Non è chiaro chi sia responsabile di una pratica quando una persona è assente.
- In caso di contestazione non si riesce a ricostruire chi ha approvato un documento o spostato una scadenza.

**Risultati attesi**: meno scadenze mancate, tempi di lavorazione visibili, responsabilità esplicite, storico difendibile.

*(Il contesto è fittizio e serve a dare realismo ai dati demo e ai flussi; non rappresenta un cliente reale.)*

## 3. Persone e ruoli

| Ruolo | Persona demo | Obiettivi | Permessi principali |
|---|---|---|---|
| **Amministratore** | Elena, IT/Segreteria | Configurare il sistema e controllare gli accessi | Attiva/disattiva gli utenti in FlowDesk, gestisce tipi pratica e template; legge audit; i ruoli sono assegnati in Entra ID; **non** lavora sulle pratiche (segregazione dei compiti) |
| **Operatore** | Davide, consulente | Lavorare le pratiche assegnate, caricare documenti, chiudere task | Vede e modifica solo le pratiche a lui assegnate; carica documenti; gestisce i propri task; **non** valida i documenti che ha caricato |
| **Responsabile** | Marta, team leader | Controllare avanzamento e qualità, assegnare il lavoro | Vede tutte le pratiche; assegna/riassegna; valida documenti; approva chiusura e modifica scadenze; legge audit; vede dashboard del team |

### Matrice permessi (sintesi)

| Azione | Admin | Operatore | Responsabile |
|---|:-:|:-:|:-:|
| Attivare/disattivare utenti, gestire tipi pratica | ✔ | ✘ | ✘ |
| Creare/modificare clienti | ✘ | ✔ | ✔ |
| Creare pratica | ✘ | ✔ | ✔ |
| Vedere pratiche | ✘ | solo assegnate | tutte |
| Assegnare/riassegnare pratica | ✘ | ✘ | ✔ |
| Caricare documenti | ✘ | ✔ (sue pratiche) | ✔ |
| Approvare/rifiutare documenti | ✘ | ✘ | ✔ (non i propri upload) |
| Creare/completare task | ✘ | ✔ (sue pratiche) | ✔ |
| Modificare scadenze | ✘ | proporre | ✔ |
| Completare/annullare pratica | ✘ | richiede revisione | ✔ |
| Consultare audit log | ✔ | ✘ | ✔ (solo pratiche) |

## 4. Glossario di dominio

| Termine | Definizione |
|---|---|
| **Cliente** | Azienda per cui lavoriamo. Ha anagrafica, referente e può avere molte pratiche. Si archivia, non si cancella. |
| **Tipo pratica** | Modello configurabile (es. "Avvio attività") che definisce checklist documenti, task standard e durata prevista. |
| **Pratica** | Lavoro concreto per un cliente, creato da un tipo pratica. Ha stato, responsabile di pratica, assegnatario e scadenza. |
| **Responsabile di pratica** | Chi risponde del risultato (un utente con ruolo Responsabile). |
| **Assegnatario** | Operatore che esegue la pratica. |
| **Documento richiesto** | Voce di checklist: documento che il cliente deve fornire. Ha stato proprio. |
| **Documento caricato (versione)** | File concreto collegato a un documento richiesto. Le versioni non si sovrascrivono. |
| **Task** | Attività operativa con assegnatario, scadenza e stato, legata a una pratica. |
| **Scadenza** | Data limite di pratica o task. Stato calcolato: *Nei termini*, *In scadenza* (≤ 3 giorni lavorativi), *Scaduta*. |
| **Audit log** | Registro append-only degli eventi rilevanti: chi, cosa, quando, valori prima/dopo. |
| **Transizione** | Passaggio di stato consentito da una regola. |
| **Soft delete / archiviazione** | Marcatura di un record come non più attivo, senza cancellarlo. |

## 5. Processi principali

### 5.1 Creazione e gestione di una pratica
1. Operatore/Responsabile sceglie (o crea) il cliente e il tipo pratica.
2. Il sistema crea la pratica in **Bozza**, genera la checklist documenti e i task standard dal tipo pratica.
3. Il Responsabile assegna l'Operatore e conferma la scadenza → **Aperta**.
4. L'Operatore avvia la lavorazione → **In lavorazione**.
5. Se mancano documenti, passa a **In attesa documenti**; quando arrivano, torna **In lavorazione**.
6. A lavoro finito l'Operatore chiede revisione → **In revisione**.
7. Il Responsabile verifica che documenti e task siano a posto → **Completata**; altrimenti rimanda **In lavorazione** con motivazione.
8. In qualunque momento prima del completamento il Responsabile può **Annullare** con motivazione.

### 5.2 Richiesta e validazione documenti
1. Alla creazione, ogni voce di checklist nasce **Richiesto**.
2. L'Operatore carica il file ricevuto dal cliente → **Caricato**.
3. Il Responsabile (diverso da chi ha caricato) apre il file e lo **Approva** o lo **Rifiuta** con motivo.
4. Se Rifiutato, l'Operatore carica una nuova versione → di nuovo **Caricato**; lo storico resta consultabile.
5. Un documento non applicabile può essere marcato **Non necessario** dal Responsabile con motivazione.

### 5.3 Creazione e completamento task
1. Task creati da template o a mano, con titolo, assegnatario, scadenza.
2. Stati: Da fare → In corso → Completato (o Bloccato/Annullato).
3. Il completamento registra data e utente; un task completato non è più modificabile (si può riaprire solo il Responsabile).

### 5.4 Gestione scadenze
1. Ogni pratica e task ha una scadenza; lo stato è calcolato, non inserito.
2. Un job periodico (o calcolo a richiesta) individua *In scadenza* e *Scadute* e genera notifiche in-app all'assegnatario e al Responsabile.
3. Modificare una scadenza richiede motivazione e viene tracciato; l'Operatore può solo proporre una nuova data, il Responsabile approva.

### 5.5 Registrazione audit
1. Ogni azione che cambia stato, assegnazioni, documenti, scadenze, permessi o accessi genera una voce di audit **nella stessa transazione** dell'azione.
2. La voce contiene utente, ruolo, timestamp UTC, entità, azione, valori prima/dopo, motivazione, correlation id.
3. L'audit è solo consultabile: nessuna modifica né cancellazione, nemmeno per l'Admin.

## 6. Stati, transizioni e regole di business

### 6.1 Pratica

| Da → A | Chi | Condizioni (guard) |
|---|---|---|
| Bozza → Aperta | Responsabile | Cliente, tipo, assegnatario e scadenza valorizzati; scadenza ≥ oggi |
| Aperta → In lavorazione | Operatore assegnato / Responsabile | — |
| In lavorazione → In attesa documenti | Operatore / Responsabile | Almeno un documento obbligatorio in stato *Richiesto* o *Rifiutato* |
| In attesa documenti → In lavorazione | Operatore / Responsabile | Nessun documento obbligatorio in stato *Richiesto* |
| In lavorazione → In revisione | Operatore / Responsabile | Tutti i documenti obbligatori *Caricati/Approvati/Non necessari*; tutti i task obbligatori *Completati* |
| In revisione → Completata | Responsabile | Tutti i documenti obbligatori *Approvati* o *Non necessari*; nessun task aperto |
| In revisione → In lavorazione | Responsabile | Motivazione obbligatoria |
| Bozza/Aperta/In lavorazione/In attesa documenti/In revisione → Annullata | Responsabile | Motivazione obbligatoria |
| Completata, Annullata | — | **Stati finali**: nessuna transizione. Eventuale riapertura = nuova pratica collegata (post-MVP) |

### 6.2 Documento richiesto
`Richiesto → Caricato → Approvato | Rifiutato`; `Rifiutato → Caricato` (nuova versione); `Richiesto → Non necessario` (Responsabile, con motivo). `Approvato` è finale salvo annullamento da parte del Responsabile con motivo (torna *Caricato*).

### 6.3 Task
`Da fare → In corso → Completato`; `Da fare/In corso → Bloccato` (motivo) `→ In corso`; `Da fare/In corso/Bloccato → Annullato`; `Completato → In corso` solo Responsabile con motivo.

### 6.4 Regole di business

| ID | Regola |
|---|---|
| BR-01 | Una pratica ha esattamente un cliente, un tipo, un assegnatario e un responsabile di pratica. |
| BR-02 | Un cliente archiviato non può ricevere nuove pratiche; le esistenti restano consultabili. |
| BR-03 | Le transizioni di stato sono ammesse solo se presenti nella tabella 6.1–6.3 e se le guard sono soddisfatte; altrimenti errore 409/422 con messaggio chiaro. |
| BR-04 | **Segregazione dei compiti**: chi carica una versione di documento non può approvarla/rifiutarla. |
| BR-05 | Ogni rifiuto, annullamento, riapertura e modifica scadenza richiede una motivazione (min. 10 caratteri). |
| BR-06 | Non si cancellano fisicamente clienti, pratiche, documenti, task, audit: solo archiviazione/annullamento. |
| BR-07 | Un Operatore agisce solo su pratiche a lui assegnate; il Responsabile su tutte. |
| BR-08 | La scadenza di un task non può superare la scadenza della pratica (salvo approvazione del Responsabile, tracciata). |
| BR-09 | I file ammessi sono PDF, JPG, PNG, DOCX, max 10 MB; hash SHA-256 salvato; nome originale conservato, nome fisico generato. |
| BR-10 | Le versioni dei documenti sono immutabili; ogni nuovo upload crea una nuova versione. |
| BR-11 | Lo stato di scadenza è calcolato su giorni lavorativi (lun–ven; festività fuori scope MVP). |
| BR-12 | Un utente disattivato (flag locale `IsActive`, effetto immediato) non può usare l'app anche se autenticato da Microsoft; i suoi dati storici e le sue voci di audit restano. |
| BR-13 | La riassegnazione di una pratica riassegna anche i suoi task non completati, previa conferma esplicita. |
| BR-14 | Le modifiche concorrenti sulla stessa entità sono rilevate (token di concorrenza) e rifiutate con 409. |
| BR-15 | L'audit si scrive nella stessa transazione dell'azione: se l'audit fallisce, fallisce l'azione. |

## 7. Requisiti funzionali

| ID | Requisito | Pri | Livelli |
|---|---|:-:|---|
| **Accesso e utenti** | | | |
| FR-01 | Login/logout con account Microsoft (Entra ID, OpenID Connect); sessione con cookie a scadenza; accesso consentito solo a utenti con un ruolo dell'app assegnato | M | FE BE TEST |
| FR-02 | Admin vede gli utenti che hanno già effettuato l'accesso e li attiva/disattiva in FlowDesk; il ruolo (assegnato in Entra ID) è mostrato in sola lettura | M | FE BE DB TEST |
| FR-03 | Autorizzazione basata su ruolo **e** su appartenenza (Operatore → solo pratiche assegnate) su ogni endpoint | M | BE DB TEST (FE nasconde le azioni) |
| **Clienti** | | | |
| FR-04 | Creare, modificare, archiviare/ripristinare clienti (ragione sociale, P.IVA univoca, referente, email, telefono) | M | FE BE DB TEST |
| FR-05 | Elenco clienti con ricerca testuale, filtro per stato, paginazione, ordinamento | M | FE BE DB |
| **Pratiche** | | | |
| FR-06 | Creare pratica da tipo pratica: genera checklist documenti e task standard | M | FE BE DB TEST |
| FR-07 | Elenco pratiche con filtri (stato, cliente, assegnatario, scadenza), paginazione, ordinamento | M | FE BE DB |
| FR-08 | Dettaglio pratica a tab: panoramica, documenti, task, storico | M | FE BE |
| FR-09 | Modificare dati pratica e assegnazione/riassegnazione (Responsabile) | M | FE BE DB TEST |
| FR-10 | Eseguire transizioni di stato validate dalle regole 6.1 | M | FE BE DB TEST |
| FR-11 | Annullare pratica con motivazione | M | FE BE DB TEST |
| **Documenti** | | | |
| FR-12 | Admin gestisce tipi pratica e relative checklist/task template | S | FE BE DB |
| FR-13 | Aggiungere manualmente una richiesta documento a una pratica | S | FE BE DB |
| FR-14 | Caricare un file (validazione tipo, dimensione, hash) come nuova versione | M | FE BE DB TEST |
| FR-15 | Scaricare un documento (con controllo permessi) | M | FE BE TEST |
| FR-16 | Approvare/rifiutare una versione con motivo; segregazione BR-04 | M | FE BE DB TEST |
| FR-17 | Storico versioni di un documento | S | FE BE DB |
| FR-18 | Marcare documento "Non necessario" | S | FE BE DB TEST |
| **Task** | | | |
| FR-19 | Creare task manuale (titolo, descrizione, assegnatario, scadenza) | M | FE BE DB TEST |
| FR-20 | Cambiare stato del task e completarlo; riaprire (Responsabile) | M | FE BE DB TEST |
| FR-21 | Riassegnare un task | S | FE BE DB TEST |
| FR-22 | Vista "I miei task" con filtri per stato e scadenza | M | FE BE DB |
| **Scadenze** | | | |
| FR-23 | Calcolo stato scadenza (Nei termini / In scadenza / Scaduta) per pratiche e task | M | BE DB TEST FE |
| FR-24 | Modifica scadenza con motivazione (Responsabile); proposta da parte dell'Operatore | S | FE BE DB TEST |
| FR-25 | Vista "Scadenze" ordinata per urgenza | S | FE BE |
| FR-26 | Job periodico che genera notifiche per scadenze imminenti/scadute | S | BE DB TEST |
| FR-27 | Notifiche in-app (lista, segna come letta, contatore) | S | FE BE DB |
| **Audit** | | | |
| FR-28 | Registrazione automatica degli eventi (vedi 5.5) nella stessa transazione | M | BE DB TEST |
| FR-29 | Consultazione audit con filtri (utente, entità, azione, periodo) per Admin e Responsabile | M | FE BE DB |
| FR-30 | Audit append-only: nessun endpoint/permesso di modifica o cancellazione | M | BE DB TEST |
| **Dashboard e supporto** | | | |
| FR-31 | Dashboard Operatore: i miei task e pratiche per stato/scadenza | M | FE BE DB TEST |
| FR-32 | Dashboard Responsabile: pratiche per stato, carico per operatore, scadute, documenti in attesa di validazione | M | FE BE DB TEST |
| FR-33 | Commenti/note su pratica | C | FE BE DB |
| FR-34 | Esportazione CSV degli elenchi | C | FE BE |
| FR-35 | Seeding dei dati demo (comando ripetibile) | M | BE DB TEST |
| FR-36 | Provisioning al primo accesso: creazione del record utente locale a partire dall'`oid` Microsoft (nome, ruolo dal claim `roles`) e aggiornamento ai login successivi | M | BE DB TEST |
| FR-37 | Schema di login "demo" per sviluppo locale, test e ambiente demo, attivabile solo da configurazione e mai in produzione | M | BE TEST |

## 8. Requisiti non funzionali

| ID | Area | Requisito | Verifica |
|---|---|---|---|
| NFR-01 | Sicurezza | Nessuna password gestita dall'app (delegata a Entra ID, con MFA); nessun segreto (client secret) nel repository: user-secrets in locale, Key Vault su Azure | Revisione + test |
| NFR-02 | Sicurezza | HTTPS ovunque, CORS ristretto al frontend, header di sicurezza, protezione CSRF se si usano cookie | Test integrazione |
| NFR-03 | Sicurezza | Validazione input lato server (mai fidarsi del client); query solo parametrizzate (EF Core); protezione da upload malevoli (estensione, content-type, dimensione, nome generato) | Test |
| NFR-04 | Sicurezza | Login, MFA, blocco e recupero password gestiti da Entra ID; l'app non rivela informazioni sugli utenti non autorizzati (accesso negato generico) | Test |
| NFR-05 | Autorizzazioni | Default deny: ogni endpoint richiede autenticazione e policy esplicita; matrice permessi coperta da test per ogni ruolo | Test automatici |
| NFR-06 | Prestazioni | Elenchi sempre paginati lato server; p95 < 500 ms su 10.000 pratiche demo con indici adeguati; niente N+1 | Test di carico leggero + log query |
| NFR-07 | Prestazioni | Upload fino a 10 MB senza caricare il file intero in memoria (streaming) | Test |
| NFR-08 | Auditabilità | Vedi BR-15, FR-28–30; timestamp in UTC; correlation id per richiesta | Test |
| NFR-09 | Accessibilità | Conformità WCAG 2.1 AA di base: navigazione da tastiera, etichette sui campi, contrasto, focus visibile, errori annunciati agli screen reader | Lint a11y + controlli manuali |
| NFR-10 | Error handling | API con ProblemDetails (RFC 9457): 400 validazione per campo, 401, 403, 404, 409 conflitto/transizione non valida, 422 regola di business; nessuno stack trace al client | Test |
| NFR-11 | Error handling | Frontend: stati di loading/errore/vuoto in ogni schermata; error boundary; messaggi comprensibili | Test FE |
| NFR-12 | Osservabilità | Logging strutturato (Serilog o equivalente) con correlation id e senza dati sensibili; health check `/health`; metriche e tracing OpenTelemetry (base) | Verifica manuale + test |
| NFR-13 | Manutenibilità | Architettura a livelli, regole di dominio testabili senza database, migrazioni versionate, CI che esegue build e test | CI |
| NFR-14 | Compatibilità | Browser evergreen recenti; layout usabile da 1024 px in su (mobile fuori scope MVP) | Manuale |
| NFR-15 | Privacy | Dati demo fittizi; nessun dato personale reale nel repository | Revisione |

## 9. User story prioritarie

Formato: *Come [ruolo], voglio [azione], così da [beneficio].* Priorità e requisiti collegati tra parentesi.

**US-01 Accesso** (M · FR-01, FR-03, FR-36)
Come utente, voglio accedere con il mio account Microsoft, così da vedere solo ciò che mi compete.
- *Given* un utente Microsoft con ruolo Manager, *When* accede con Microsoft, *Then* vede la home del Responsabile.
- *Given* un utente Microsoft senza ruolo assegnato, *When* prova ad accedere, *Then* l'accesso è negato (se arriva comunque all'app, 403) e l'evento è nell'audit.
- *Given* un utente al suo primo accesso, *When* completa il login, *Then* esiste un record utente locale con il suo `oid` e il suo ruolo.

**US-02 Gestione utenti** (M · FR-02)
Come Amministratore, voglio vedere e disattivare gli utenti di FlowDesk, così da controllare gli accessi (il ruolo si assegna in Entra ID).
- *Given* sono Admin, *When* disattivo Davide, *Then* alla richiesta successiva non può più usare l'app anche se Microsoft lo autentica, e le sue pratiche restano visibili ai Responsabili.
- *Given* sono Operatore, *When* provo a chiamare l'API utenti, *Then* ricevo 403.

**US-03 Clienti** (M · FR-04, FR-05)
Come Operatore, voglio registrare e cercare clienti, così da avviare pratiche rapidamente.
- *Given* una P.IVA già esistente, *When* creo un cliente con la stessa P.IVA, *Then* ricevo errore di duplicato sul campo.
- *Given* un cliente archiviato, *When* provo ad aprire una pratica, *Then* l'azione è rifiutata con messaggio chiaro.

**US-04 Creazione pratica** (M · FR-06)
Come Operatore, voglio creare una pratica da un tipo, così da avere subito checklist e task standard.
- *Given* il tipo "Avvio attività" con 4 documenti e 3 task, *When* creo la pratica, *Then* esistono 4 documenti *Richiesto* e 3 task *Da fare*.

**US-05 Assegnazione** (M · FR-09, FR-03)
Come Responsabile, voglio assegnare e riassegnare pratiche, così da bilanciare il carico.
- *Given* una pratica con 2 task aperti, *When* la riassegno a un altro Operatore e confermo, *Then* pratica e task aperti passano al nuovo Operatore e l'audit riporta vecchio e nuovo valore.
- *Given* sono Operatore, *When* provo a riassegnare, *Then* 403.

**US-06 Avanzamento stato** (M · FR-10)
Come Operatore, voglio far avanzare la pratica solo quando le condizioni sono rispettate, così da non saltare passaggi.
- *Given* un documento obbligatorio ancora *Richiesto*, *When* chiedo "In revisione", *Then* ricevo 409 con l'elenco di cosa manca.
- *Given* una pratica *Completata*, *When* tento qualsiasi transizione, *Then* è rifiutata.

**US-07 Upload documento** (M · FR-14, FR-15)
Come Operatore, voglio caricare il documento ricevuto dal cliente, così da registrarne la presenza.
- *Given* un PDF di 2 MB, *When* lo carico, *Then* il documento passa a *Caricato* con versione 1.
- *Given* un file .exe o > 10 MB, *When* lo carico, *Then* è rifiutato con messaggio specifico e nessun file resta su disco.

**US-08 Validazione documento** (M · FR-16)
Come Responsabile, voglio approvare o rifiutare un documento, così da garantire la qualità.
- *Given* un documento caricato da Davide, *When* Marta lo rifiuta senza motivo, *Then* l'operazione è rifiutata.
- *Given* un documento caricato da Marta stessa, *When* tenta di approvarlo, *Then* riceve errore per segregazione dei compiti.

**US-09 Task** (M · FR-19, FR-20, FR-22)
Come Operatore, voglio vedere e completare i miei task, così da sapere cosa fare oggi.
- *Given* 5 task assegnati a me e 3 ad altri, *When* apro "I miei task", *Then* ne vedo 5 ordinati per scadenza.
- *Given* un task completato, *When* tento di modificarlo, *Then* è rifiutato; solo il Responsabile può riaprirlo con motivo.

**US-10 Scadenze** (M/S · FR-23–FR-26)
Come Responsabile, voglio vedere cosa è in scadenza o scaduto, così da intervenire prima del danno.
- *Given* una pratica con scadenza fra 2 giorni lavorativi, *When* apro la vista scadenze, *Then* è marcata *In scadenza*.
- *Given* una scadenza superata, *When* gira il job, *Then* assegnatario e Responsabile ricevono una notifica una sola volta.

**US-11 Modifica scadenza** (S · FR-24)
Come Responsabile, voglio spostare una scadenza con motivazione, così da lasciare traccia della decisione.
- *Given* una pratica, *When* sposto la scadenza senza motivo, *Then* è rifiutato.
- *Given* una motivazione valida, *When* confermo, *Then* l'audit mostra data precedente, nuova data e motivo.

**US-12 Completamento pratica** (M · FR-10)
Come Responsabile, voglio completare una pratica in revisione, così da chiuderla con evidenza che tutto è a posto.
- *Given* tutti i documenti approvati e nessun task aperto, *When* completo, *Then* lo stato è *Completata* e l'audit registra chi e quando.
- *Given* un task ancora aperto, *When* provo a completare, *Then* 409 con il dettaglio.

**US-13 Audit** (M · FR-28–FR-30)
Come Responsabile o Admin, voglio consultare lo storico, così da ricostruire le decisioni.
- *Given* una pratica con 10 eventi, *When* apro lo storico, *Then* vedo 10 voci complete di utente, ruolo, data UTC e valori prima/dopo.
- *Given* qualsiasi ruolo, *When* provo a modificare o cancellare una voce, *Then* non esiste modo di farlo (endpoint assente/403).

**US-14 Dashboard Responsabile** (M · FR-32)
Come Responsabile, voglio una vista di sintesi, così da individuare colli di bottiglia.
- *Given* i dati demo, *When* apro la dashboard, *Then* i contatori per stato, scadute e documenti da validare coincidono con i dati nel database.

**US-15 Concorrenza** (M · BR-14)
Come utente, voglio essere avvisato se qualcun altro ha modificato lo stesso dato, così da non sovrascriverlo.
- *Given* due utenti che aprono la stessa pratica, *When* il secondo salva dopo il primo, *Then* riceve 409 e un invito a ricaricare.

**US-16 Demo dati** (M · FR-35)
Come sviluppatore, voglio caricare dati demo con un comando, così da mostrare l'applicazione a un recruiter in pochi minuti.
- *Given* un database vuoto, *When* lancio il seeding, *Then* trovo utenti, clienti, pratiche in tutti gli stati e almeno una pratica scaduta.
- *Given* il seeding già eseguito, *When* lo rilancio, *Then* non duplica i dati.

## 10. Casi limite ed errori attesi

| # | Caso | Comportamento atteso |
|---|---|---|
| E-01 | Transizione di stato non prevista (es. Bozza → Completata) | 409, messaggio con transizioni ammesse |
| E-02 | Transizione con guard non soddisfatta | 409/422 con elenco di cosa manca |
| E-03 | Operatore accede a pratica non sua (per id diretto) | 404 (non rivela l'esistenza) |
| E-04 | Ruolo non autorizzato chiama un endpoint | 403 e voce di audit di accesso negato |
| E-05 | Token scaduto o assente | 401; il frontend riporta al login mantenendo la destinazione |
| E-06 | Upload di tipo non ammesso, vuoto, > 10 MB, nome con path traversal | 400; nessun file salvato; nome fisico sempre generato |
| E-07 | Upload interrotto a metà | Nessun record/file orfano (transazione + pulizia) |
| E-08 | Due modifiche concorrenti | La seconda riceve 409; i dati della prima sono intatti |
| E-09 | Utente disattivato con sessione attiva | Alla richiesta successiva 401 |
| E-10 | Riassegnazione a utente disattivato o non Operatore | 422 |
| E-11 | Scadenza nel passato in creazione/apertura | 400/422 sul campo |
| E-12 | Task con scadenza oltre quella della pratica | 422 salvo approvazione del Responsabile |
| E-13 | Cliente con P.IVA duplicata o formalmente invalida | 400 sul campo |
| E-14 | Tipo pratica modificato dopo la creazione di pratiche | Le pratiche esistenti non cambiano (checklist copiata, non referenziata) |
| E-15 | Fallimento scrittura audit | L'azione fallisce e viene annullata (BR-15) |
| E-16 | Errore del database o eccezione non gestita | 500 ProblemDetails generico con correlation id; dettaglio solo nei log |
| E-17 | Elenco senza risultati / backend irraggiungibile | Frontend mostra stato vuoto o errore con "riprova" |
| E-18 | Download di documento senza permesso | 403/404; evento registrato |
| E-19 | Fusi orari / cambio ora legale | Date salvate in UTC; scadenze come date (senza ora) interpretate nel fuso aziendale Europe/Rome |
| E-20 | Riassegnazione con task completati | I task completati non cambiano assegnatario (storico) |

## 11. Dati iniziali di demo / seeding

*Tutti fittizi. Le credenziali demo sono solo per ambiente locale/demo e vengono documentate nel README, non nel codice di produzione.*

- **Utenti (7)**: 3 utenti reali nel tenant Entra ID (Elena Admin, Marta Manager, Davide Operator) più 4 utenti solo locali con `oid` fittizio (Giorgio Manager, Sara e Luca Operator, Paola disattivata), usabili con lo schema di login demo (FR-37).
- **Clienti (10)**: aziende inventate di settori diversi; 1 archiviato; P.IVA valide ma fittizie.
- **Tipi pratica (4)**: Avvio attività (4 documenti, 3 task), Rinnovo autorizzazione (3 doc, 2 task), Adempimento fiscale periodico (2 doc, 2 task), Variazione societaria (5 doc, 4 task).
- **Pratiche (~25)**: distribuite su tutti gli stati; almeno 3 *Scadute*, 3 *In scadenza*, 2 *In attesa documenti* da oltre 10 giorni, 2 *In revisione*, 1 *Annullata* con motivo.
- **Documenti**: mix di stati incluso 1 rifiutato con 2 versioni; file di esempio minuscoli e innocui (PDF generato).
- **Task (~70)**: stati vari, alcuni bloccati con motivo, alcuni scaduti.
- **Audit**: generato eseguendo le azioni attraverso lo stesso codice applicativo (non con insert diretti), per avere uno storico coerente.
- **Notifiche**: alcune non lette per l'Operatore e per il Responsabile.
- **Modalità**: comando idempotente (non duplica), con opzione "reset ambiente demo".

## 12. Fuori scope nel MVP

- Multi-tenant e multi-azienda; fatturazione; ordini.
- Firma elettronica; OCR; anteprima documenti nel browser avanzata.
- Invio email/SMS/push (solo notifiche in-app).
- Integrazioni esterne (PEC, ERP, calendari). Il login con Microsoft Entra ID è invece **dentro** il MVP.
- Password locali, registrazione autonoma, recupero password (delegati a Entra ID).
- Assegnazione dei ruoli tramite gruppi o Microsoft Graph dall'interfaccia di FlowDesk (i ruoli si assegnano nel portale Entra).
- Calendario festività e SLA configurabili; regole di workflow configurabili da interfaccia.
- Pratiche collegate/riapertura; sotto-task; dipendenze tra task.
- App mobile e layout responsive sotto i 1024 px.
- Localizzazione multilingua (solo italiano).
- Funzioni AI (riassunto pratica, estrazione dati dai documenti, suggerimento task) → fase portfolio opzionale.
- Reportistica avanzata ed export oltre il CSV.

## 13. Piano in milestone (da MVP a portfolio)

*Stime indicative per ~8–10 ore/settimana; riviste a fine di ogni milestone.*

| M | Nome | Contenuto | Uscita (criteri di fine) | Stima |
|---|---|---|---|---|
| **M0** | Fondamenta | Repo GitHub, README, docs, convenzioni, CI base, decisioni architetturali (ADR) | Documenti approvati; pipeline che compila | 1 sett. |
| **M1** | Walking skeleton | Solution a livelli, DB, login Microsoft (Entra ID), ruoli dai claim, provisioning utenti, schema demo (FR-01–03, FR-36–37), prima schermata | Login Microsoft funzionante end-to-end con test 401/403 | 2 sett. |
| **M2** | Clienti e pratiche | FR-04–11, stati e guard del dominio, test della state machine | Pratica creata, assegnata e avanzata di stato | 3 sett. |
| **M3** | Documenti | FR-14–18, storage dietro interfaccia, validazione e segregazione | Ciclo documento completo con versioni | 2 sett. |
| **M4** | Task e scadenze | FR-19–27, calcolo scadenze, job, notifiche in-app | Vista "I miei task" e scadenze coerenti | 2 sett. |
| **M5** | Audit e dashboard | FR-28–32, consultazione audit, dashboard per ruolo | Audit completo e dashboard verificata dai test | 2 sett. |
| **M6** | Hardening MVP | Seeding (FR-35), error handling, accessibilità, performance, copertura test, documentazione | **MVP completo**: scenario demo eseguibile da zero | 2 sett. |
| **M7** | Versione portfolio | Deploy su Azure (App Service, SQL, Blob), CI/CD, osservabilità (Application Insights), README con screenshot/GIF, video demo | URL pubblico, pipeline verde, README per recruiter | 2–3 sett. |
| **M8** | Estensioni (opzionale) | Commenti, export CSV, gestione ruoli via Microsoft Graph, funzione AI (riassunto pratica / estrazione dati) | Una estensione completa e documentata | a scelta |

**Definition of Done (per ogni storia)**: criteri di accettazione verificati da test · migrazione DB applicata · audit coperto dove serve · errori gestiti (ProblemDetails/UI) · lint e build verdi · documentazione aggiornata.

## 14. Domande da decidere prima dell'architettura

| # | Decisione | Opzioni | Raccomandazione |
|---|---|---|---|
| D-01 | Forma del backend | Monolite modulare a livelli vs microservizi | **Monolite modulare** (Domain/Application/Infrastructure/Api): un solo sviluppatore, un solo database, confini separabili in futuro |
| D-02 | Autenticazione | ASP.NET Core Identity + JWT / Identity + cookie / Entra ID subito | **DECISO: Microsoft Entra ID** (OIDC lato server, cookie HttpOnly, App Roles, tenant personale). Vedi ADR-0008 |
| D-03 | Stile applicativo | Controller + MediatR (CQRS leggero) vs servizi applicativi semplici | **Casi d'uso espliciti in Application** (MediatR o servizi): da scegliere in base a cosa vuoi imparare |
| D-04 | Storage documenti | Filesystem locale dietro interfaccia vs Azure Blob da subito | **Filesystem in locale, Blob in M7**, stessa interfaccia |
| D-05 | Job scadenze | `BackgroundService` interno vs Hangfire/Quartz | **BackgroundService** nell'MVP; scheduler dedicato solo se necessario |
| D-06 | Audit | Scritto dal codice applicativo vs trigger SQL / temporal tables | **Codice applicativo** (stessa transazione) per le azioni di dominio; temporal tables come possibile integrazione |
| D-07 | Notifiche in tempo reale | Polling vs SignalR | **Polling** nell'MVP; SignalR in M8 |
| D-08 | Libreria UI React | Componenti pronti (MUI/Mantine/shadcn) vs CSS custom | **Libreria pronta con buona accessibilità** per concentrarsi sulla logica |
| D-09 | Gestione dati lato React | TanStack Query vs Redux Toolkit/RTK Query | **TanStack Query** + stato locale |
| D-10 | Versione .NET | .NET 10 (installato) | **.NET 10 LTS** |
| D-11 | Schema multilingua | Solo italiano vs i18n | **Solo italiano**, testi centralizzati |
| D-12 | Database | Istanza SQL Server locale `MSSQLSERVER` già in esecuzione vs LocalDB | **Istanza locale** (più vicina ad Azure SQL) |
| D-13 | Repository GitHub | Pubblico da subito vs privato fino all'MVP | **Pubblico**, con licenza e senza dati reali (verifica pre-push dei segreti) |
| D-14 | Test | xUnit + Testcontainers non utilizzabile (niente Docker) → SQL Server locale/LocalDB per i test di integrazione | **xUnit + WebApplicationFactory** su database dedicato di test; Vitest + Testing Library lato React; Playwright per 2–3 flussi E2E |
| D-15 | Ambito del ruolo Admin | Solo configurazione (segregazione) vs anche lavoro sulle pratiche | **Solo configurazione e audit** |
| D-16 | Visibilità del Responsabile | Vede tutto vs solo il proprio team | **Vede tutto** nell'MVP; team come estensione |
| D-17 | Nome e licenza | "FlowDesk" confermato? Licenza MIT? | Confermare |

---
*Prossimo passo dopo l'approvazione: ADR delle decisioni D-01…D-17, poi `architecture.md` e scaffold tecnico (M0–M1).*
