# ADR-0002 — Livelli e regola di dipendenza

- **Stato**: accettato · **Data**: 2026-10-08

## Contesto
Serve una struttura che separi regole di business, orchestrazione, accesso ai dati e HTTP, e che sia verificabile automaticamente.

## Decisione
Quattro progetti: `FlowDesk.Domain`, `FlowDesk.Application`, `FlowDesk.Infrastructure`, `FlowDesk.Api`. Dentro ciascuno, cartelle per modulo.

Regola: `Api → Application → Domain` e `Infrastructure → Application → Domain`. Il Domain non ha dipendenze. L'Api è la composition root e registra le implementazioni di Infrastructure.

Un progetto di test (`FlowDesk.Architecture.Tests`, NetArchTest) verifica le regole di dipendenza e vieta l'uso diretto di `DateTime.Now`.

## Alternative
- **Un progetto per modulo e per livello**: troppi progetti per una persona sola.
- **Un solo progetto**: confini non applicabili dal compilatore.

## Conseguenze
- (+) Regole di dominio testabili senza database né HTTP.
- (+) Le violazioni di dipendenza fanno fallire i test.
- (−) Più cerimonia iniziale (mapping tra livelli), accettata perché è parte dell'apprendimento.
