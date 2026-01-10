# RBAC-002 — Seller vê menu “Avaliações” e “Configurações” (potencial excesso de permissão)

## Severidade
Alta

## Ambiente
- URL: https://gestauto.tasso.local
- Usuário: `seller / 123456`

## Contexto (README)
- README de validação manual do frontend indica:
  - seller → deve ver menu **Comercial**

## Passos para reproduzir
1. Logar com `seller / 123456`
2. Observar o menu lateral

## Resultado atual
- Seller visualiza “Avaliações” e “Configurações”.

## Resultado esperado
- Se a regra for restritiva por perfil (como descrito no README), seller não deve ver “Avaliações”.
- “Configurações” só deve aparecer se for global e aplicável ao seller (e com conteúdo apropriado ao perfil).

## Evidência
- Menu lateral contém: `Home`, `Comercial`, `Avaliações`, `Configurações`.

## Decisão aplicada (matriz de acesso)
- `seller`: acesso ao módulo **Comercial**; não vê **Avaliações** e não acessa `/evaluations`.
- `evaluator`: acesso ao módulo **Avaliações**.
- `viewer`: acesso somente ao módulo **Avaliações**.
- Menus/guards seguem a mesma matriz para evitar “vazamento” por URL direta.

## Reteste (2026-01-09)

### Resultado
- `seller` ainda visualiza “Avaliações” e “Configurações” no menu lateral.
- Não foi evidenciado bloqueio por UI/guard ao nível de menu (itens clicáveis).

## Critérios de aceite
- [ ] Alinhar regra de negócio (README vs RBAC): documentar qual deve ser a matriz final de acesso.
- [ ] Se for restritivo: seller não vê “Avaliações” e não acessa `/evaluations`.
- [ ] Se “Configurações” for global: definir o que seller pode ver/editar e manter coerência de permissões.

## Sugestão de correção
- Revisar mapeamento de roles → menus e rotas.
- Adicionar testes de RBAC para garantir menus e guards por perfil.
