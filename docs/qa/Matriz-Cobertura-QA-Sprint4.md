# Matriz de cobertura — QA da Sprint 4

## Resultado da revisão

Esta matriz compara a parte de Compliance, Quality Assurance & Tests — 4º Sprint do desafio com os itens do Azure Boards, os testes feitos e a implementação que está disponível no projeto Alerta Cidadão.

| Requisito | Evidência | Situação |
|---|---|---|
| Plano manual em nível de sistema | 15 casos MAN-01 a MAN-15, com objetivo, entradas, saídas e passos; Issues 1–15 na iteração Sprint 4. | Coberto para os fluxos e endpoints implementados. |
| Dados controlados | O plano define corpos JSON, headers, usuários QA, limites, códigos HTTP e resultados esperados. | Coberto. |
| Relação com a Release 4 e as tarefas realizadas | A matriz relaciona cada grupo de casos às funcionalidades, rotas e arquivos de código disponíveis. | Parcial: o projeto Azure não contém o backlog funcional/PBIs anteriores do grupo, então não foi possível ligar os testes a IDs históricos. |
| Professor com acesso Basic e administrador do projeto | O aluno informou que já enviou o convite e concedeu acesso. | Considerado feito conforme informado; confirmar Basic na organização e Project Administrator no projeto. |
| Repositório público e branch develop | GS-QA está público e contém os documentos, runner e evidências de teste. | Feito. |
| Pelo menos quatro testes automatizados | AUT-01 a AUT-05 em xUnit; execução direcionada: 5/5 aprovados. AUT-06 executado contra a API local em Kestrel. | Coberto para integração e carga leve. |
| Execução da suíte completa | xUnit: 35 aprovados, 0 falhas. | Aprovado. |
| Carga/performance controlada | 20 usuários virtuais × 50 chamadas a /health/live em Kestrel local; 1.000/1.000 HTTP 200; p95 7,05 ms; CSV e resumo gerados. | Feito como smoke de carga leve, não como benchmark de produção. |
| Vídeo de configuração e execução | Não foi produzido neste trabalho. | Pendente: o aluno fará a gravação. |

## Rastreabilidade funcional

Os casos foram planejados para a Release 4 / Sprint 4 como validação e regressão das funcionalidades existentes. A ligação abaixo usa referências observáveis no código. Ela não inventa a sprint histórica em que cada funcionalidade foi criada.

| Funcionalidade validada | Casos | Evidência no projeto |
|---|---|---|
| Raiz da API, Swagger e health checks | MAN-01–MAN-03, MAN-15 | Program.cs; HealthChecks; rotas /, /swagger, /health/live e /health/ready. |
| Cadastro e gestão de usuários | MAN-04–MAN-05, MAN-14 | UsuarioEndpoints.cs; UsuarioService.cs; validações e repositório de usuário. |
| Login, JWT e perfis de acesso | MAN-06–MAN-07 | JwtTokenService.cs; configuração de autenticação e política Gestor no Program.cs. |
| Pontos de monitoramento, leituras e alertas | MAN-08, MAN-10–MAN-11 | Endpoints, serviços e repositórios de ponto, leitura e alerta. |
| Idempotência e limite de chamadas | MAN-09, MAN-12 | IdempotencyEndpointFilter.cs; RateLimiting/. |
| CI para develop | MAN-13 | azure-pipelines.yml. |
| Carga leve do liveness | MAN-15 | tools/AlertaCidadao.LoadTest/; resultado CSV e resumo Markdown. |

O Azure Boards disponível tinha a iteração Sprint 4 e os casos de teste, sem itens funcionais prévios ou plano de release importado. A ligação aos PBIs históricos ficou limitada às funcionalidades e arquivos verificáveis. Se o grupo tiver os IDs do release plan em outro quadro, basta acrescentá-los às referências desta matriz.

## Cobertura dos casos manuais

| Caso | Fluxo |
|---|---|
| MAN-01 | Raiz da API e contrato Swagger. |
| MAN-02–MAN-03 | Liveness e readiness com Oracle disponível/indisponível. |
| MAN-04–MAN-07 | Cadastro, validação, login JWT e autorização por perfil. |
| MAN-08 | CRUD de ponto de monitoramento. |
| MAN-09 | Idempotência. |
| MAN-10 | Criação e consulta de leitura de enchente. |
| MAN-11 | Criação e resolução de alerta. |
| MAN-12 | Rate limiting e Retry-After. |
| MAN-13 | Pipeline e artefato de develop. |
| MAN-14 | Consulta, atualização e desativação de usuário. |
| MAN-15 | Carga controlada de liveness e latência p95. |

Os itens manuais permanecem To Do no Azure Boards até que cada execução manual seja registrada. A execução automatizada de carga está documentada separadamente em AUT-06.

## Limites verificados no código

Estes pontos não devem ser marcados como aprovados enquanto a funcionalidade ou o ambiente necessário não estiver disponível:

- Paginação e ordenação selecionáveis: os endpoints expõem filtros, mas não parâmetros para escolher página, tamanho ou ordem.
- HATEOAS: não foram encontrados links de navegação/ação nas respostas atuais.
- MongoDB: não foram encontradas dependências ou integração MongoDB na solução verificada.
- Migração em ambiente limpo: existem migrations EF Core e scripts SQL; a aplicação delas numa base Oracle vazia não foi executada.
- Logs estruturados e correlação: há chamadas ILogger e níveis configurados; ainda não há verificação de formato estruturado, correlação e ausência de dados sensíveis.
- Carga de negócio e Oracle: AUT-06 consulta apenas /health/live, que não acessa Oracle. Não foi fornecido um deployment QA conectado a banco para medir operações reais com segurança.

O teste de carga registrado é um smoke local para comparar execuções no mesmo ambiente. Os números não representam capacidade da aplicação em produção.
