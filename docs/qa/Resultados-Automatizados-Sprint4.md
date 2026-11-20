# Resultados automatizados — QA da Sprint 4

## Suíte xUnit

**Data:** 19/11/2026 · **Ambiente:** local, .NET SDK 10.0.404 · **Banco:** nenhum Oracle real; os testes usam TestServer, dependências isoladas e EF Core InMemory onde indicado.

- Casos direcionados AUT-01 a AUT-05: **5 aprovados, 0 falhas**.
- Suíte completa xUnit: **35 aprovados, 0 falhas, 0 ignorados**.
- Casos cobrem autorização 401/403, rate limiting 429 com Retry-After, readiness indisponível 503 e carga concorrente leve de liveness no TestServer.

Os resultados direcionados foram gerados pelo script executar-testes-evidencia.ps1. A suíte completa também foi executada com saída TRX. Os relatórios TRX locais ficam na pasta artifacts/qa-evidence do projeto e não foram incluídos aqui por conterem caminhos locais.

## Carga no Kestrel (AUT-06)

**Data:** 19/11/2026 23:25 BRT · **Alvo:** API local em 127.0.0.1:5187/health/live · **Oracle:** não consultado.

- Configuração: 20 usuários virtuais × 50 chamadas GET = 1.000 requisições.
- Resultado: **1.000/1.000 válidas**, todas HTTP 200, 0 falhas.
- Latência: média 2,68 ms; p50 0,79 ms; p95 7,05 ms; p99 67,90 ms; máxima 70,34 ms.
- Vazão observada: 5.894,07 requisições/s.
- Critério definido antes da execução: 100% HTTP 200 e p95 ≤ 2.000 ms.
- Resultado: **APROVADO**.

Arquivos da execução: docs/qa/evidencias/teste-carga-sprint4.csv e docs/qa/evidencias/teste-carga-sprint4.md.

Este foi um smoke de carga leve na máquina local, sobre loopback e somente no endpoint de liveness. A rota não acessa Oracle, não grava dados e os números não devem ser tratados como benchmark de produção.

## Observações

- A restauração/build mostrou aviso NU1900 porque NuGet não conseguiu consultar o serviço de auditoria de vulnerabilidades durante a execução. Os pacotes já estavam disponíveis e os testes passaram; a auditoria de dependências não foi atualizada.
- A suíte de testes existente exibe também um aviso do analisador xUnit2013 em UsuarioServiceTests; não afeta o resultado da execução.
- Os testes de readiness com banco disponível usam dependência em memória; CRUD real e migrações continuam dependendo de Oracle QA.
- O arquivo CSV registra somente número da amostra, código HTTP, latência, validade do corpo e tipo de erro. Não contém senha, token, dados pessoais ou connection string.
