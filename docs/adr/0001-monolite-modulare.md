# ADR-0001 — Monolite modulare invece di microservizi

- **Stato**: accettato · **Data**: 2026-10-08

## Contesto
Il progetto è sviluppato da una persona, con un solo database e un MVP da completare. L'obiettivo è imparare l'intero ciclo di sviluppo di un'applicazione enterprise.

## Decisione
Un unico processo ASP.NET Core con moduli separati per funzionalità (`Customers`, `Practices`, `Documents`, `Tasks`, `Notifications`, `Audit`) dentro un'architettura a livelli. Un solo database SQL Server, un solo deploy.

## Alternative
- **Microservizi**: confini di servizio, comunicazione via rete o messaggi, consistenza distribuita, deploy multipli.

## Conseguenze
- (+) Un solo deploy, transazioni semplici (l'audit nella stessa transazione), debug locale facile.
- (+) I confini di modulo permettono di estrarre un servizio in futuro.
- (−) Nessun scaling indipendente per modulo; i confini vanno protetti con test architetturali (ADR-0002).
- Si impara prima il dominio e i livelli; i microservizi hanno senso quando c'è un problema reale da risolvere.
