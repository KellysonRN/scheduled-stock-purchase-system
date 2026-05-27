# Contributing

Obrigado por contribuir com o Scheduled Stock Purchase System! Essas diretrizes ajudam a preservar as decisões arquiteturais e garantem qualidade nas mudanças.

Sumário rápido
- Leia `docs/architecture-spec.md` antes de propor mudanças arquiteturais.
- Abra uma *issue* descrevendo a motivação e o impacto antes de qualquer PR grande.
- Para mudanças arquiteturais ou contratuais, adicione/atualize um ADR (veja `docs/adr-template.md`).

Processo recomendado

1. Fork & branch
   - Crie uma branch a partir de `client-domain` (ou `main` conforme instrução do maintainer):
     - `feat/{scope}/{short-description}` — novas funcionalidades
     - `fix/{scope}/{short-description}` — correções
     - `chore/{scope}/{short-description}` — manutenção

2. Abra uma *issue* primeiro
   - Para mudanças não triviais, descreva o problema, alternativas consideradas e o impacto (migrations, contratos, performance).

3. ADR (decisões arquiteturais)
   - Se a mudança altera a arquitetura, crie/atualize um ADR em `docs/` (use `docs/adr-template.md`).
   - Link o ADR e a issue no PR.

4. Código e testes
   - Mantenha as features organizadas em `src/Scheduled.Stock.Purchase.Api/Features/...`.
   - Não mova o domínio para `Features` sem justificativa (veja `docs/architecture-spec.md`).
   - Inclua testes unitários para regras de domínio; para mudanças de fluxo inclua testes de integração.
   - Formate o código: `dotnet format` (se disponível) e rode `dotnet build` e `dotnet test` localmente.

5. PR pequeno e focado
   - Prefira PRs pequenos e revisáveis.
   - No título do PR: `[feat|fix|chore] {area}: {curta descrição}`
   - Use o template de PR (.github/PULL_REQUEST_TEMPLATE.md) e responda a checklist.

Checklist mínimo para PRs
- [ ] Link para *issue* relacionada
- [ ] `dotnet build` passou localmente
- [ ] `dotnet test` passou localmente
- [ ] Novos testes adicionados quando aplicável
- [ ] Documentação atualizada (README/docs/ADR) quando aplicável
- [ ] Migrations geradas e descritas (se houver alterações no DB)

Code review
- Mantenha o scope pequeno. Explique decisões não óbvias nos comentários do PR.
- Resolva comentários por commits separados para facilitar reverts parciais.

CI
- A pipeline valida `dotnet build` e `dotnet test`. PRs com falhas não devem ser mesclados.

Obrigado por ajudar — cada contribuição conta!
