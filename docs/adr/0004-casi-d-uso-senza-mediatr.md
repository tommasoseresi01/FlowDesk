# ADR-0004 — Casi d'uso con handler espliciti, senza MediatR

- **Stato**: accettato · **Data**: 2026-10-08

## Contesto
Il livello Application orchestra casi d'uso (comandi e query) con aspetti trasversali: validazione, transazione, audit, logging.

## Decisione
Handler espliciti (`ICommandHandler<TCommand, TResult>`, `IQueryHandler<TQuery, TResult>`) iniettati direttamente dagli endpoint. Gli aspetti trasversali si applicano con **decorator** nell'ordine: logging → validazione → transazione e audit → handler.

## Alternative
- **MediatR**: dalla v13 ha licenza commerciale.
- **Libreria *Mediator* (source generator, MIT)**: valida alternativa gratuita.

## Conseguenze
- (+) Nessuna dipendenza a pagamento né "magia" a runtime: il flusso si legge nel codice.
- (+) Si impara il pipeline pattern.
- (−) Un po' di codice di registrazione in più; si può sostituire con una libreria in seguito senza toccare il dominio.
