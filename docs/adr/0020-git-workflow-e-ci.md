# ADR-0020 — Git workflow e CI

- **Stato**: accettato · **Data**: 2026-10-08

## Contesto
Progetto personale pubblico su GitHub, sviluppato da una persona, con l'obiettivo di mostrare un processo professionale.

## Decisione
**Workflow**
- GitHub Flow: `main` sempre rilasciabile e protetta (richiede CI verde e PR), branch brevi (≤ 2–3 giorni), **squash merge**.
- Nomi branch: `feat/FR-14-upload-document`, `fix/…`, `chore/…`, `docs/…`, `test/…`.
- Commit secondo **Conventional Commits** (`feat(documents): add version upload`).
- Backlog su GitHub: ogni user story è una Issue con label (`module:*`, `priority:*`) e Milestone; una Project board. Le PR chiudono le issue con `Closes #n`.
- Tag SemVer (`v0.1.0-mvp`, `v1.0.0`) e GitHub Releases. Template di PR con checklist (criteri di accettazione, test, migrazione, audit, documentazione).
- **Privacy**: email dei commit = indirizzo `noreply` di GitHub; mai email aziendali o universitarie.

**CI (GitHub Actions)**
- `ci.yml` su PR e push su `main`: job *backend* (setup .NET da `global.json`, build con warning come errori, `dotnet format --verify-no-changes`, test con coverage contro SQL Server in service container), job *frontend* (`npm ci`, lint, `tsc --noEmit`, Vitest, build), job *sicurezza* (pacchetti vulnerabili, `npm audit`), CodeQL in workflow separato.
- Dopo: Dependabot, `e2e.yml` (M9), `deploy.yml` con OIDC verso Azure (M10).

## Alternative
- **GitFlow**: pensato per release pianificate e team numerosi.
- **Azure DevOps Pipelines**: separa il codice dalla CI e vale meno come vetrina su GitHub.

## Conseguenze
- (+) Storia leggibile, changelog automatizzabile, qualità verificata a ogni PR.
- (−) Anche lavorando da solo serve disciplina nel passare dalle PR.
