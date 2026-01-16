# Implementação Frontend GestAuto (Login e Menus por Perfil) - Resumo de Tarefas

## Tarefas

- [ ] 1.0 Provisionar client do Keycloak (gestauto-frontend)
- [ ] 2.0 Expor frontend via Traefik/Docker (gestauto.tasso.local)
- [ ] 3.0 Scaffold do frontend + config runtime
- [ ] 4.0 Implementar login/logout via Keycloak (PKCE)
- [ ] 5.0 Implementar RBAC (menus + guards) + páginas placeholder + testes

## Análise de Paralelização

### Caminho Crítico

```
1.0 → 4.0 → 5.0
					 ↑
				 3.0
```

### Lanes (paralelizáveis)

| Lane | Tarefas | Observação |
|------|---------|------------|
| Infra/Auth | 1.0, 2.0 | Keycloak + Traefik para host `gestauto.tasso.local` |
| Frontend Base | 3.0 | Pode iniciar antes do Keycloak, mas sem validar login |
| Frontend Auth+RBAC | 4.0, 5.0 | Depende do client do Keycloak (1.0) |
