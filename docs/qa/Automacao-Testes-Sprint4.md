# Automação de testes — Sprint 4

## Ferramenta e objetivo

**Ferramentas:** xUnit + `Microsoft.AspNetCore.TestHost` para integração funcional, e um runner .NET 10 com `HttpClient` para carga controlada via Kestrel. O projeto é API-only e não requer janela de interface. A carga padrão fica restrita a localhost para evitar chamadas acidentais em produção.

**Escopo demonstrado:** cinco casos xUnit selecionados da suíte e um teste de carga controlada contra a API real iniciada localmente. Os quatro primeiros casos validam autenticação, autorização, rate limiting e readiness; os demais registram latência sob concorrência.

| ID automação | Teste xUnit existente | Dados controlados | Resultado esperado |
|---|---|---|---|
| AUT-01 | `EndpointProtegido_SemToken_DeveRetornar401` | GET a endpoint protegido sem header `Authorization`. | HTTP 401 Unauthorized. |
| AUT-02 | `EndpointGestor_ComTokenCidadao_DeveRetornar403` | GET de recurso gestor com token assinado de perfil `CIDADAO`. | HTTP 403 Forbidden. |
| AUT-03 | `PoliticaLogin_DeveBloquearAposDuasRequisicoesERetornarRetryAfter` | Política isolada: limite login 2 por 60 s, fila 0; três POSTs sequenciais do mesmo cliente de teste. | Chamadas 1 e 2 HTTP 200; chamada 3 HTTP 429 e `Retry-After` numérico positivo. |
| AUT-04 | `Readiness_DeveRetornar503QuandoOracleNaoPuderSerResolvido` | TestServer sem contexto Oracle disponível; GET `/health/ready`. | HTTP 503, corpo contém `Unhealthy` e `oracle`. |
| AUT-05 | `Liveness_SobCargaControlada_DeveResponder200EManterP95AbaixoDoLimite` | TestServer; 20 chamadas concorrentes × 50 requisições a `/health/live`; limite p95 2.000 ms. | 1.000/1.000 respostas HTTP 200; corpo `Healthy/self`, sem Oracle; p95 até 2.000 ms. |
| AUT-06 | `tools/AlertaCidadao.LoadTest` (Kestrel) | API local; 20 usuários virtuais × 50 GETs a `/health/live`; sem token, corpo ou escrita no banco. | 1.000/1.000 respostas válidas, HTTP 200, p95 até 2.000 ms; CSV e resumo Markdown. |

## Preparação

1. Abrir a raiz do projeto AlertaCidadao com .NET SDK 10.
2. Conferir o projeto `test/AlertaCidadao.UnitTests/AlertaCidadao.UnitTests.csproj`: target `net10.0`, xUnit 2.9.2, `Microsoft.AspNetCore.TestHost` 10.0.0.
3. Conferir os casos nos arquivos `Security/JwtAuthenticationIntegrationTests.cs`, `Integration/RateLimitingIntegrationTests.cs`, `HealthChecks/HealthCheckEndpointTests.cs` e `Performance/HealthCheckLoadIntegrationTests.cs`.
4. Restaurar dependências da solução: `dotnet restore .\AlertaCidadao.sln`.

## Execução dos cinco casos xUnit

No PowerShell, na raiz do repositório da entrega QA, informar o caminho da cópia local do projeto AlertaCidadao:

```powershell
.\docs\qa\executar-testes-evidencia.ps1 -AppRoot "C:\caminho\para\AlertaCidadao"
```

O script acima executa cinco casos xUnit, confere os resultados do TRX e imprime um resumo sanitizado. O comando equivalente, para inspeção ou execução direta, é:

```powershell
dotnet test .\test\AlertaCidadao.UnitTests\AlertaCidadao.UnitTests.csproj `
  --configuration Release `
  --filter "FullyQualifiedName~EndpointProtegido_SemToken_DeveRetornar401|FullyQualifiedName~EndpointGestor_ComTokenCidadao_DeveRetornar403|FullyQualifiedName~PoliticaLogin_DeveBloquearAposDuasRequisicoesERetornarRetryAfter|FullyQualifiedName~Readiness_DeveRetornar503QuandoOracleNaoPuderSerResolvido|FullyQualifiedName~Liveness_SobCargaControlada_DeveResponder200EManterP95AbaixoDoLimite" `
  --logger "console;verbosity=normal" `
  --logger "trx;LogFileName=sprint4-automacao.trx" `
  --results-directory .\artifacts\qa-evidence
```

O script está neste repositório de QA e recebe `-AppRoot`; o comando direto acima deve ser executado na raiz da cópia do projeto.

**Critério de aprovação:** 5 testes descobertos, 5 aprovados, 0 falhos, 0 ignorados; o arquivo `artifacts/qa-evidence/sprint4-automacao.trx` deve mostrar os cinco nomes AUT-01 a AUT-05. Qualquer diferença exige análise antes de gravar Pass.

### Carga com a API em Kestrel (AUT-06)

1. Iniciar a API com configuração de QA em `http://127.0.0.1:5075`. O teste consulta somente `/health/live`, sem dependência do Oracle. Não usar produção.
2. Em outro terminal, na raiz do projeto, executar:

   ```powershell
   dotnet run --project .\tools\AlertaCidadao.LoadTest\AlertaCidadao.LoadTest.csproj --configuration Release -- --base-url http://127.0.0.1:5075 --path /health/live --users 20 --requests 50 --p95-limit-ms 2000 --output .\artifacts\qa-evidence\teste-carga-sprint4.csv
   ```

3. Conferir 1.000 respostas 200, p95 até 2.000 ms, o resultado APROVADO e os arquivos CSV/Markdown. O runner rejeita URL externa por padrão; só use `--allow-remote` para um ambiente de QA autorizado pelo grupo.

Para repetir somente após alterar código, configuração ou testes, limpar o diretório de evidências da execução anterior e executar novamente o comando. O rate limiter usado por AUT-03 é recriado pelo teste e configurado com limite 2, independente do padrão de produção (5/minuto).

## Limites conhecidos

- O TestServer valida o contrato HTTP e os componentes isolados registrados por cada teste. AUT-04 simula ausência do Oracle; não demonstra conexão real com o banco.
- A suíte do projeto tem testes adicionais para cadastro, hash PBKDF2, JWT, idempotência, rate limiting, health checks e serviços. Replay/conflict de idempotência usa EF Core InMemory; não reproduz concorrência nem índice único Oracle. O próprio repositório documenta esse limite.
- AUT-05 é uma verificação de carga leve no TestServer; AUT-06 mede a rota de liveness em Kestrel local. Nenhum dos dois mede operações de escrita, Oracle, rede pública ou capacidade de produção. Para um benchmark representativo, é necessário executar o mesmo runner contra um deployment de QA autorizado e definir limites com o grupo.
- Não usar token, senha, hash, connection string, dados pessoais reais ou gravações de produção no vídeo.

## Evidência em vídeo

Gravar a preparação dos testes, o comando filtrado, os cinco casos xUnit e a execução AUT-06 com 20 usuários × 50 chamadas. Mostrar o resumo, p95 e arquivos CSV/Markdown gerados, sem exibir segredos. A captura deve ser um registro da execução real; não editar contagem ou mensagens do runner.
