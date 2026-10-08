# ADR-0006 — Gestione del tempo

- **Stato**: accettato · **Data**: 2026-10-08

## Contesto
Scadenze, audit e job dipendono dalla data corrente; servono test deterministici e correttezza con il cambio ora legale.

## Decisione
- I timestamp si salvano in **UTC** (`datetime2(3)`).
- Le scadenze sono **date** (`date`), interpretate nel fuso aziendale `Europe/Rome`.
- Il tempo si ottiene da `TimeProvider` iniettato; `DateTime.Now` e `DateTime.UtcNow` sono vietati (test architetturale).
- I giorni lavorativi sono lunedì–venerdì; le festività sono fuori scope nel MVP.

## Alternative
- **`DateTime.Now` ovunque**: test non deterministici e bug con ora legale.

## Conseguenze
- (+) Test sulle scadenze con orologio finto.
- (−) Serve convertire esplicitamente tra UTC e fuso aziendale ai margini (UI e calcolo "oggi").
