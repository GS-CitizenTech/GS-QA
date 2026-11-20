# Automação de testes — Sprint 4

## Ferramenta e objetivo

**Ferramenta:** xUnit + `Microsoft.AspNetCore.TestHost` (`WebApplication` em TestServer), já incluídos no projeto .NET 10. Esta é a opção adequada ao sistema API-only e não requer janela de interface. Os cenários exercitam respostas HTTP, autenticação/autorização, middleware de limite e health checks.

**Escopo demonstrado:** quatro casos automatizados selecionados da suíte existente. Cada execução usa uma aplicação de teste criada pelo próprio caso e encerra o host ao final; nenhum dado é gravado em Oracle nem em produção.

| ID automação | Teste xUnit existente | Dados controlados | Resultado esperado |
|---|---|---|---|
| AUT-01 | `EndpointProtegido_SemToken_DeveRetornar401` | GET a endpoint protegido sem header `Authorization`. | HTTP 401 Unauthorized. |
| AUT-02 | `EndpointGestor_ComTokenCidadao_DeveRetornar403` | GET de recurso gestor com token assinado de perfil `CIDADAO`. | HTTP 403 Forbidden. |
| AUT-03 | `PoliticaLogin_DeveBloquearAposDuasRequisicoesERetornarRetryAfter` | Política isolada: limite login 2 por 60 s, fila 0; três POSTs sequenciais do mesmo cliente de teste. | Chamadas 1 e 2 HTTP 200; chamada 3 HTTP 429 e `Retry-After` numérico positivo. |
| AUT-04 | `Readiness_DeveRetornar503QuandoOracleNaoPuderSerResolvido` | TestServer sem contexto Oracle disponível; GET `/health/ready`. | HTTP 503, corpo contém `Unhealthy` e `oracle`. |

## Preparação

1. Abrir a raiz do projeto AlertaCidadao com .NET SDK 10.
2. Conferir o projeto `test/AlertaCidadao.UnitTests/AlertaCidadao.UnitTests.csproj`: target `net10.0`, xUnit 2.9.2, `Microsoft.AspNetCore.TestHost` 10.0.0.
3. Conferir os casos nos arquivos `Security/JwtAuthenticationIntegrationTests.cs`, `Integration/RateLimitingIntegrationTests.cs` e `HealthChecks/HealthCheckEndpointTests.cs`.
4. Restaurar dependências da solução: `dotnet restore .\AlertaCidadao.sln`.

## Execução dos quatro casos

No PowerShell, na raiz do repositório da entrega QA, informar o caminho da cópia local do projeto AlertaCidadao:

```powershell
.\docs\qa\executar-testes-evidencia.ps1 -AppRoot "C:\caminho\para\AlertaCidadao"
```

O script acima executa a seleção, confere os quatro resultados do TRX e imprime um resumo sanitizado. O comando equivalente, para inspeção ou execução direta, é:

```powershell
dotnet test .\test\AlertaCidadao.UnitTests\AlertaCidadao.UnitTests.csproj `
  --configuration Release `
  --filter "FullyQualifiedName~EndpointProtegido_SemToken_DeveRetornar401|FullyQualifiedName~EndpointGestor_ComTokenCidadao_DeveRetornar403|FullyQualifiedName~PoliticaLogin_DeveBloquearAposDuasRequisicoesERetornarRetryAfter|FullyQualifiedName~Readiness_DeveRetornar503QuandoOracleNaoPuderSerResolvido" `
  --logger "console;verbosity=normal" `
  --logger "trx;LogFileName=sprint4-automacao.trx" `
  --results-directory .\artifacts\qa-evidence
```

O script está neste repositório de QA e recebe `-AppRoot`; o comando direto acima deve ser executado na raiz da cópia do projeto.

**Critério de aprovação:** 4 testes descobertos, 4 aprovados, 0 falhos, 0 ignorados; o arquivo `artifacts/qa-evidence/sprint4-automacao.trx` deve mostrar os quatro nomes acima. Qualquer diferença exige análise antes de gravar Pass.

Para repetir somente após alterar código, configuração ou testes, limpar o diretório de evidências da execução anterior e executar novamente o comando. O rate limiter usado por AUT-03 é recriado pelo teste e configurado com limite 2, independente do padrão de produção (5/minuto).

## Limites conhecidos

- O TestServer valida o contrato HTTP e os componentes isolados registrados por cada teste. AUT-04 simula ausência do Oracle; não demonstra conexão real com o banco.
- A suíte do projeto tem testes adicionais para cadastro, hash PBKDF2, JWT, idempotência, rate limiting, health checks e serviços. Replay/conflict de idempotência usa EF Core InMemory; não reproduz concorrência nem índice único Oracle. O próprio repositório documenta esse limite.
- O cenário AUT-03 valida comportamento funcional do limitador, não teste de carga/performance. A avaliação de desempenho ou concorrência real deve ocorrer em QA com autorização, métricas e Oracle de teste.
- Não usar token, senha, hash, connection string, dados pessoais reais ou gravações de produção no vídeo.

## Evidência em vídeo

Gravar a preparação do projeto/testes, o comando filtrado acima e o resumo final do runner. O vídeo entregue deve mostrar a configuração (limite 2/60 s do teste isolado e perfil usado no JWT), os quatro nomes executados e a contagem final, sem mostrar segredos. A captura deve ser um registro da execução real; não editar contagem ou mensagens do runner.

