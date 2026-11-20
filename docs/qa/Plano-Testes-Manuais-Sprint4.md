# Plano de testes manuais — Alerta Cidadão — Sprint 4

## Objetivo e escopo

Validar em nível de sistema os fluxos de maior risco da API REST Alerta Cidadão: disponibilidade e documentação, autenticação e autorização JWT, cadastro e gestão de usuários, pontos de monitoramento, leituras de enchente, alertas, idempotência, limites de chamadas e integração com Oracle. O escopo foi montado a partir dos endpoints, README, documentação de health checks, testes automatizados e pipeline presentes no repositório. Não foi fornecido um backlog/exportação do Azure Boards; por isso, os casos abaixo estão vinculados às entregas descritas para a Sprint 4 e devem ser ajustados se o quadro oficial tiver critérios diferentes.

**Release:** 4 · **Sprint:** 4 · **Produto:** Alerta Cidadão · **Tipo:** validação manual de sistema · **Estado inicial dos casos:** Proposed.

| ID | Fluxo coberto | Objetivo | Prioridade |
|---|---|---|---:|
| MAN-01 | Raiz e Swagger | Confirmar que a API e a especificação publicada respondem. | 1 |
| MAN-02 | Liveness | Confirmar que a aplicação responde mesmo sem Oracle. | 1 |
| MAN-03 | Readiness Oracle | Validar pronto e indisponível sem expor detalhes da conexão. | 1 |
| MAN-04 | Cadastro de cidadão | Criar usuário com dados válidos e proteger a senha. | 1 |
| MAN-05 | Validação de cadastro | Rejeitar e-mail inválido e senha curta sem gravar usuário. | 1 |
| MAN-06 | Login JWT | Emitir token para credenciais corretas e rejeitar incorretas. | 1 |
| MAN-07 | Autorização por perfil | Diferenciar sem token, cidadão autenticado e gestor. | 1 |
| MAN-08 | CRUD de pontos | Criar, consultar, atualizar e desativar um ponto. | 1 |
| MAN-09 | Idempotência | Reproduzir resposta na repetição e rejeitar chave com payload distinto. | 1 |
| MAN-10 | Leituras | Registrar e consultar leitura; rejeitar duplicidade controlada. | 1 |
| MAN-11 | Alertas | Criar e resolver alerta e preservar integridade da leitura associada. | 1 |
| MAN-12 | Rate limiting | Aplicar limite de login e devolver Retry-After. | 2 |
| MAN-13 | Pipeline da Sprint 4 | Confirmar build, testes e artefato para develop. | 1 |
| MAN-14 | CRUD de usuários | Consultar, atualizar e desativar logicamente um cidadão. | 1 |
| MAN-15 | Carga controlada | Confirmar respostas e latência p95 do liveness sob concorrência controlada. | 2 |

## Ambiente e dados controlados

- Executar somente em ambiente QA/local isolado, nunca no banco de produção. Base sugerida: `http://localhost:5075`; URL da API hospedada deverá ser preenchida depois de validada.
- Usar uma instância Oracle de QA limpa ou restaurável. Antes de cada execução, remover somente os registros de teste identificados pelo prefixo `QA-S4-` e os usuários `qa.s4.*@example.test`.
- O domínio `.test` é reservado para testes. Não substituir por e-mails de pessoas reais.
- Criar previamente um usuário gestor no banco de QA, porque o cadastro público só deve criar perfil `CIDADAO`. Credenciais fixas sugeridas: `qa.s4.gestor@example.test` / `Gestor-QA-2026!`. Criar cidadão: `qa.s4.cidadao@example.test` / `Cidadao-QA-2026!`.
- Cabeçalhos comuns: `Accept: application/json`; para JSON, `Content-Type: application/json`; usar `Authorization: Bearer <token>` nos endpoints protegidos.
- `Idempotency-Key` sugerida: `qa-s4-man-001`, com até 100 caracteres. Usar uma chave nova por cenário, salvo quando o objetivo for testar repetição.
- Fixar a data da leitura em `2026-11-19T12:00:00-03:00`; os horários gerados pelo sistema (`DataCadastro`, `DataRecebimento`, `DataGeracao`, `DataResolucao`) são dinâmicos e devem ser validados como presentes/faixas, não comparados a um instante fixo.
- Identificadores de banco são gerados pela API. Armazenar o `idPonto`, `idLeitura` e `idAlerta` retornados para os passos seguintes; não presumir valores numéricos.

## Casos de teste

### MAN-01 — Raiz da API e contrato Swagger

**Objetivo:** confirmar que a aplicação publicada responde e que a documentação representa as rotas da Sprint 4.

**Pré-condições:** API iniciada no ambiente QA.

**Dados de entrada:** `GET /`; em seguida `GET /swagger/v1/swagger.json`. Sem corpo ou autenticação.

**Saída esperada:** ambos `200 OK`; a raiz retorna `aplicacao = "AlertaCidadao"`, mensagem de execução e `documentacao = "/swagger"`; o JSON Swagger declara a API e contém rotas `/api/usuarios`, `/api/pontos-monitoramento`, `/api/leituras-enchente`, `/api/alertas` e `/health`.

**Procedimento:**

1. Abrir a base da API no cliente HTTP.
2. Enviar `GET {{baseUrl}}/` e conferir código e os três campos do JSON.
3. Enviar `GET {{baseUrl}}/swagger/v1/swagger.json`.
4. Pesquisar no corpo as rotas listadas e confirmar que o documento é JSON válido.

### MAN-02 — Liveness sem dependência do Oracle

**Objetivo:** confirmar que a sondagem de processo continua saudável quando o banco não está disponível.

**Pré-condições:** ambiente QA isolado; Oracle QA desconectado ou endereço de banco de teste propositalmente indisponível.

**Dados de entrada:** `GET /health/live`; sem corpo, token ou cabeçalho de idempotência.

**Saída esperada:** `200 OK`, JSON com `status = "Healthy"` e check `self`; resposta sem check `oracle`.

**Procedimento:**

1. Confirmar que o banco de QA não aceita conexão.
2. Enviar `GET {{baseUrl}}/health/live`.
3. Conferir o código HTTP, `status` e o check `self`.
4. Confirmar que o processo da API permaneceu ativo; restaurar o banco ao final do teste.

### MAN-03 — Readiness com Oracle disponível e indisponível

**Objetivo:** verificar a dependência Oracle da readiness e a resposta segura em falha.

**Pré-condições:** ambiente QA com uma conexão Oracle válida e uma forma controlada de bloquear a conexão; não interromper banco compartilhado/produção.

**Dados de entrada:** `GET /health/ready` nos dois estados da dependência.

**Saída esperada:** Oracle conectado: `200 OK`, `Healthy`, check `oracle`. Oracle indisponível: `503 Service Unavailable`, `Unhealthy`, check `oracle`, sem connection string, usuário, senha ou mensagem de driver expostos.

**Procedimento:**

1. Com Oracle QA disponível, enviar `GET {{baseUrl}}/health/ready` e registrar o resultado.
2. Bloquear a conexão apenas no ambiente QA, sem alterar dados.
3. Repetir a chamada e conferir `503` e `Unhealthy`.
4. Inspecionar corpo e logs permitidos; confirmar que os detalhes de conexão não aparecem na resposta HTTP.
5. Restabelecer a conexão e confirmar `200` novamente.

### MAN-04 — Cadastro válido de cidadão

**Objetivo:** criar um cidadão, validar o retorno e confirmar que a senha não é exposta.

**Pré-condições:** banco QA limpo para o e-mail; API conectada ao Oracle QA.

**Dados de entrada:** `POST /api/usuarios`, header `Idempotency-Key: qa-s4-man-usuario-001`, corpo `{"nome":"Cidadão QA Sprint 4","email":"qa.s4.cidadao@example.test","senha":"Cidadao-QA-2026!","telefone":"11987654321","perfil":"CIDADAO"}`.

**Saída esperada:** `201 Created`; `Location` aponta para `/api/usuarios/{id}`; corpo contém `idUsuario > 0`, nome, e-mail, perfil `CIDADAO`, `ativo = true` e data de cadastro. Corpo não contém `senha`, `senhaHash` ou `password`.

**Procedimento:**

1. Confirmar que `qa.s4.cidadao@example.test` não existe na base QA.
2. Enviar o POST com os dados fixos e a chave indicada.
3. Validar status, cabeçalho `Location` e campos de resposta.
4. Pesquisar o corpo por termos de senha/hash e confirmar ausência.
5. Confirmar a persistência do usuário na base de QA sem ler/exportar o hash.

### MAN-05 — Validação de cadastro inválido

**Objetivo:** rejeitar campos inválidos antes de persistir o usuário.

**Pré-condições:** API disponível; e-mail de teste inexistente.

**Dados de entrada:** `POST /api/usuarios`, header `Idempotency-Key: qa-s4-man-usuario-invalido-001`, corpo `{"nome":"Q","email":"qa-invalido","senha":"123","perfil":"CIDADAO"}`.

**Saída esperada:** `400 Bad Request`; resposta de validação com `codigo = "VALIDACAO"` e erros para nome, e-mail e senha; nenhuma linha criada para `qa-invalido`.

**Procedimento:**

1. Confirmar que o e-mail não existe na base QA.
2. Enviar o corpo inválido.
3. Conferir `400`, `codigo = "VALIDACAO"` e as propriedades reportadas em `erros`.
4. Consultar a base QA pelo e-mail e confirmar zero registros.

### MAN-06 — Login válido e senha incorreta

**Objetivo:** validar emissão do JWT e rejeição de credencial incorreta.

**Pré-condições:** usuário cidadão do MAN-04 ativo; janela de rate limit de login reiniciada.

**Dados de entrada:** `POST /api/usuarios/login` com `{"email":"qa.s4.cidadao@example.test","senha":"Cidadao-QA-2026!"}`; depois repetir com senha `Senha-Errada-2026!`.

**Saída esperada:** credencial correta: `200 OK`, `accessToken` não vazio, `tokenType = "Bearer"`, `expiraEm` futura e perfil/identidade do cidadão. Senha incorreta: `401 Unauthorized`, sem token e sem revelar se o e-mail existe.

**Procedimento:**

1. Enviar login com a senha correta e guardar o token apenas no ambiente local de teste.
2. Conferir `200`, `accessToken`, `tokenType`, data de expiração, ID e perfil.
3. Enviar login com a senha incorreta.
4. Conferir `401` e ausência de token/detalhes de conta na resposta.

### MAN-07 — Autenticação e autorização por perfil

**Objetivo:** distinguir acesso anônimo, cidadão autenticado e gestor.

**Pré-condições:** cidadão e gestor ativos; tokens válidos emitidos pelo endpoint de login; existência de ao menos um usuário cadastrado.

**Dados de entrada:** `GET /api/usuarios` sem token; com token cidadão; com token gestor.

**Saída esperada:** sem token `401`; token cidadão `403`; token gestor `200` e lista JSON. Token adulterado ou expirado deve resultar em `401`.

**Procedimento:**

1. Enviar a rota sem header `Authorization` e registrar `401`.
2. Enviar `Authorization: Bearer {{tokenCidadao}}` e registrar `403`.
3. Enviar `Authorization: Bearer {{tokenGestor}}` e confirmar `200`.
4. Alterar um caractere de uma cópia do token cidadão e confirmar `401`.

### MAN-08 — CRUD de ponto e desativação lógica

**Objetivo:** validar cadastro, consulta, atualização e desativação de um ponto de monitoramento.

**Pré-condições:** token gestor válido; código `QA-S4-PM-001` ainda não existe; banco QA limpo/restaurável.

**Dados de entrada:** POST `/api/pontos-monitoramento` com `{"codigo":"QA-S4-PM-001","nome":"Ponto QA Interlagos","descricao":"Massa controlada Sprint 4","logradouro":"Av. QA, 100","bairro":"Interlagos","cidade":"São Paulo","uf":"sp","latitude":-23.7000,"longitude":-46.7000}`. PUT altera nome para `Ponto QA Interlagos Atualizado`. Usar gestor em operações protegidas.

**Saída esperada:** POST `201` e `Location`; código normalizado para `qa-s4-pm-001`; GET por ID `200` com dados; PUT `200` com nome alterado; DELETE `204`. Após DELETE, listagem ativa não contém o ponto; `/todos` mantém o registro com `ativo = false`; consulta por ID retorna `404`.

**Procedimento:**

1. Enviar POST com o token gestor e guardar `idPonto`.
2. Consultar `/api/pontos-monitoramento/{{idPonto}}` e comparar campos.
3. Enviar PUT com o mesmo código e nome atualizado; conferir `200`.
4. Enviar DELETE do ID; conferir `204`.
5. Consultar a listagem ativa e `/todos`; confirmar exclusão lógica.
6. Consultar o ID desativado e confirmar `404`.

### MAN-09 — Idempotência de gravação

**Objetivo:** impedir duplicidade ao repetir operação e rejeitar reutilização da chave com outro conteúdo.

**Pré-condições:** token gestor; código `QA-S4-IDEM-001` livre; chave `qa-s4-idem-001` ainda não usada nos últimos 24 h.

**Dados de entrada:** dois POST idênticos para `/api/pontos-monitoramento` com `Idempotency-Key: qa-s4-idem-001`; terceiro POST com a mesma chave e nome `Ponto QA Alterado`.

**Saída esperada:** primeira chamada `201`; segunda chamada repete status, corpo e `Location` da resposta original sem criar outro ponto; terceira chamada `409 Conflict` com `codigo = "CHAVE_IDEMPOTENCIA_REUTILIZADA"`; somente um ponto correspondente existe.

**Procedimento:**

1. Enviar o primeiro POST e guardar corpo e `Location`.
2. Reenviar método, rota, chave e corpo exatamente iguais.
3. Comparar status, JSON e `Location`; confirmar mesmo identificador.
4. Alterar apenas o nome e reenviar com a mesma chave.
5. Confirmar `409` e contar registros com o código no banco QA.

### MAN-10 — Registro e consulta de leitura de enchente

**Objetivo:** validar leitura vinculada a ponto ativo, filtro e rejeição de duplicidade.

**Pré-condições:** ponto ativo do MAN-08 (ou massa equivalente); token gestor; chave de evento `QA-S4-EVT-001` livre.

**Dados de entrada:** POST `/api/leituras-enchente`, `{"idPonto":{{idPonto}},"nivelAguaCm":125.50,"chuvaMm":32.00,"elevacaoCmH":4.20,"dataHoraLeitura":"2026-11-19T12:00:00-03:00","origem":"SENSOR","chaveEvento":"QA-S4-EVT-001"}`.

**Saída esperada:** POST `201` e ID positivo; valores retornados equivalem ao input e origem normalizada `SENSOR`; GET com `?idPonto={{idPonto}}` inclui a leitura. Nova leitura com mesma chave de evento resulta `409`; leitura com `idPonto = 0`, valor negativo ou data ausente resulta `400` sem gravação.

**Procedimento:**

1. Enviar o POST com token gestor e guardar `idLeitura`.
2. Consultar `/api/leituras-enchente/{{idLeitura}}` e comparar medidas, ponto, horário, origem e chave.
3. Consultar `/api/leituras-enchente?idPonto={{idPonto}}` e confirmar presença.
4. Tentar novo POST com `ChaveEvento` igual; confirmar `409`.
5. Enviar um caso inválido controlado; confirmar `400` e zero novos registros.

### MAN-11 — Ciclo de vida do alerta e integridade da leitura

**Objetivo:** criar alerta associado à leitura, resolver e proteger leitura vinculada.

**Pré-condições:** leitura do MAN-10 sem alerta; token gestor válido.

**Dados de entrada:** POST `/api/alertas` com `{"idLeitura":{{idLeitura}},"classificacao":"ALTO","probabilidade":0.91,"mensagemExplicativa":"Nível em elevação no ponto de QA.","tipoMensagem":"SMS","modeloMl":"QA-S4-v1"}`. PUT do alerta com mesma classificação e `status = "RESOLVIDO"`.

**Saída esperada:** criação `201`, `status = "ATIVO"`, ID positivo; PUT `200`, `status = "RESOLVIDO"`, `dataResolucao` preenchida; listagem por status respeita filtro. DELETE da leitura associada retorna `409`; após excluir o alerta, remoção da leitura pode ser repetida no cleanup.

**Procedimento:**

1. Criar alerta com a leitura do MAN-10 e guardar `idAlerta`.
2. Consultar `GET /api/alertas/{{idAlerta}}`; conferir associação e dados.
3. Atualizar status para `RESOLVIDO`; conferir `dataResolucao` preenchida.
4. Consultar `/api/alertas?status=RESOLVIDO` e confirmar presença; consultar com status inválido e confirmar `400`.
5. Tentar excluir a leitura associada e confirmar `409`.
6. Limpar primeiro o alerta e depois a leitura, conforme as regras do ambiente QA.

### MAN-12 — Rate limiting específico de login

**Objetivo:** bloquear chamadas acima do limite configurado e informar intervalo de retry.

**Pré-condições:** ambiente QA isolado; janela login reiniciada; política padrão do projeto: cinco requisições por minuto por IP, sem fila. Não executar contra usuário real.

**Dados de entrada:** seis chamadas consecutivas em menos de 60 s para `POST /api/usuarios/login`, corpo inválido fixo `{"email":"qa-invalido","senha":"123"}`.

**Saída esperada:** chamadas 1–5 retornam `400` de validação; chamada 6 retorna `429 Too Many Requests`, header `Retry-After` numérico maior que zero. A sondagem `/health/live` permanece acessível.

**Procedimento:**

1. Reiniciar somente a instância QA ou aguardar o fim da janela anterior.
2. Enviar seis POSTs sequenciais, registrando o código de cada resposta e horário.
3. Conferir cinco `400` seguidos por `429`.
4. Conferir `Retry-After` na sexta resposta.
5. Consultar `/health/live` e confirmar que o rate limiting não bloqueia a sondagem.

### MAN-13 — Pipeline de build, testes e artefato em `develop`

**Objetivo:** garantir que a branch solicitada pela entrega percorra CI antes de publicar o release.

**Pré-condições:** pipeline Azure configurada para `master` e `develop`, agente .NET 10, variável `sprint4-secrets` configurada como secreta; nenhuma execução manual em produção.

**Dados de entrada:** commit aprovado na branch `develop`, sem credenciais no código.

**Saída esperada:** CI executa restore, build Release, xUnit e publicação dos resultados; etapa de publish produz artefato `drop`. CD só inicia se CI tiver sucesso e usar a conexão/variáveis protegidas configuradas no Azure.

**Procedimento:**

1. Enviar o commit de QA para `develop`.
2. Abrir a execução correspondente da pipeline e confirmar branch/commit.
3. Conferir sucesso de restore/build e o total de testes publicados.
4. Conferir o artefato `drop`.
5. Confirmar que segredo Oracle/JWT não aparece nos logs. Se o deployment de QA for habilitado, validar `/health/live`, Swagger e readiness após deploy.

### MAN-14 — CRUD de usuário: consulta, atualização e desativação lógica

**Objetivo:** validar consulta individual, atualização e desativação lógica de cidadão, completando o CRUD do recurso usuário.

**Pré-condições:** usuário criado no MAN-04; token gestor válido; e-mail atualizado ainda não usado em QA.

**Dados de entrada:** `GET /api/usuarios/{{idUsuario}}`; `PUT /api/usuarios/{{idUsuario}}` com `Idempotency-Key: qa-s4-man-usuario-atualizar-001` e corpo `{"nome":"Cidadão QA Sprint 4 Atualizado","email":"qa.s4.cidadao.atualizado@example.test","senha":null,"telefone":"11999990000","perfil":"CIDADAO","ativo":true}`; `DELETE /api/usuarios/{{idUsuario}}` com `Idempotency-Key: qa-s4-man-usuario-desativar-001`; `GET /api/usuarios?incluirInativos=true` e `GET /api/usuarios`.

**Saída esperada:** GET inicial `200 OK` com os dados do MAN-04. PUT `200 OK` com nome, e-mail e telefone atualizados, `ativo = true` e sem senha/hash no corpo. DELETE `204 No Content`. Com `incluirInativos=true`, o usuário aparece com `ativo = false`; na listagem padrão, não aparece.

**Procedimento:**

1. Consultar o usuário por ID com token gestor e conferir os dados criados no MAN-04.
2. Atualizar com os valores controlados e a chave indicada.
3. Validar `200`, os campos atualizados e a ausência de senha/hash.
4. Desativar com uma chave idempotente nova e validar `204`.
5. Consultar a listagem com `incluirInativos=true` e confirmar `ativo = false`.
6. Consultar a listagem padrão e confirmar que o usuário desativado não aparece.

### MAN-15 — Carga controlada no endpoint de liveness

**Objetivo:** verificar que o processo da API continua respondendo sob uma carga leve, concorrente e reproduzível, registrando latência e vazão.

**Pré-condições:** API executando em ambiente local/QA isolado em http://127.0.0.1:5075; nenhum tráfego de usuários; executar apenas contra localhost ou ambiente QA expressamente autorizado. O teste usa /health/live, que não depende do Oracle e não grava dados.

**Dados de entrada:** GET /health/live; 20 usuários virtuais; 50 requisições por usuário; total de 1.000 chamadas; limite de aprovação p95 de 2.000 ms; sem token, corpo ou dados pessoais.

**Saída esperada:** 1.000 respostas HTTP 200, com status = "Healthy" e check self, sem check oracle; p95 menor ou igual a 2.000 ms; arquivos CSV e resumo Markdown criados em artifacts/qa-evidence.

**Procedimento:**

1. Iniciar a API em http://127.0.0.1:5075 usando configuração local de QA. Não usar produção nem conexão de produção.
2. Confirmar uma resposta 200 em GET /health/live.
3. Na raiz do projeto Alerta Cidadão, executar:

   ```powershell
   dotnet run --project .\tools\AlertaCidadao.LoadTest\AlertaCidadao.LoadTest.csproj --configuration Release -- --base-url http://127.0.0.1:5075 --path /health/live --users 20 --requests 50 --p95-limit-ms 2000 --output .\artifacts\qa-evidence\teste-carga-sprint4.csv
   ```

4. Conferir no resumo que 1.000 de 1.000 respostas são válidas e que o p95 atende ao limite.
5. Abrir o CSV e confirmar que ele contém apenas número da amostra, código HTTP, latência e validade do corpo; anexar CSV e resumo ao registro do teste.
6. Parar a API local e manter o caso como To Do até o responsável registrar a execução no Azure Boards.

**Limite do teste:** esta carga leve mede o endpoint de liveness no host e na máquina usados na execução. Ela não representa benchmark de produção nem valida latência de Oracle ou de operações de escrita.

## Rastreabilidade dos testes para a Release 4

Os casos estão reunidos na Release 4 / Sprint 4 para validar as funcionalidades que estão no código entregue. A rastreabilidade abaixo aponta a funcionalidade e os arquivos/endpoints observáveis no repositório; ela não atribui uma sprint histórica a uma funcionalidade sem o plano original do grupo.

| Grupo funcional | Casos | Referência no projeto |
|---|---|---|
| Inicialização, Swagger e health checks | MAN-01–MAN-03, MAN-15 | src/AlertaCidadao.API/Program.cs; src/AlertaCidadao.API/HealthChecks/; rotas /, /swagger, /health/live e /health/ready. |
| Cadastro, autenticação e autorização | MAN-04–MAN-07, MAN-14 | src/AlertaCidadao.API/Endpoints/UsuarioEndpoints.cs; src/AlertaCidadao.API/Authentication/; src/AlertaCidadao.Application/Services/UsuarioService.cs. |
| Pontos, leituras e alertas | MAN-08, MAN-10–MAN-11 | src/AlertaCidadao.API/Endpoints/; serviços em src/AlertaCidadao.Application/Services/; repositórios em src/AlertaCidadao.Infrastructure/Repositories/. |
| Idempotência e limitação de chamadas | MAN-09, MAN-12 | src/AlertaCidadao.API/Filters/IdempotencyEndpointFilter.cs; src/AlertaCidadao.API/RateLimiting/. |
| Integração contínua | MAN-13 | azure-pipelines.yml; branch develop. |
| Desempenho sob carga leve | MAN-15 | tools/AlertaCidadao.LoadTest/; artefato CSV e resumo de execução. |

No Azure Boards havia a iteração Sprint 4 e os casos manuais, sem backlog funcional anterior do grupo. Por isso, a relação com as tarefas históricas/PBIs oficiais não foi inventada: os testes apontam para as funcionalidades e evidências de código disponíveis. Se o grupo mantiver um release plan em outro quadro, acrescente os IDs desses itens a esta tabela.

## Evidência e registro no Azure Boards

Projeto Azure: [Qa-rm563088](https://dev.azure.com/gs-qa-rm5630880147/Qa-rm563088). Os itens abaixo foram criados na iteração `Qa-rm563088\Sprint 4`; todos permanecem em `To Do` porque o plano é de testes, não um registro de execução manual.

| ID do plano | Item Azure Boards |
|---|---|
| MAN-01 | [Issue 1](https://dev.azure.com/gs-qa-rm5630880147/Qa-rm563088/_workitems/edit/1) |
| MAN-02 | [Issue 2](https://dev.azure.com/gs-qa-rm5630880147/Qa-rm563088/_workitems/edit/2) |
| MAN-03 | [Issue 3](https://dev.azure.com/gs-qa-rm5630880147/Qa-rm563088/_workitems/edit/3) |
| MAN-04 | [Issue 4](https://dev.azure.com/gs-qa-rm5630880147/Qa-rm563088/_workitems/edit/4) |
| MAN-05 | [Issue 5](https://dev.azure.com/gs-qa-rm5630880147/Qa-rm563088/_workitems/edit/5) |
| MAN-06 | [Issue 6](https://dev.azure.com/gs-qa-rm5630880147/Qa-rm563088/_workitems/edit/6) |
| MAN-07 | [Issue 7](https://dev.azure.com/gs-qa-rm5630880147/Qa-rm563088/_workitems/edit/7) |
| MAN-08 | [Issue 8](https://dev.azure.com/gs-qa-rm5630880147/Qa-rm563088/_workitems/edit/8) |
| MAN-09 | [Issue 9](https://dev.azure.com/gs-qa-rm5630880147/Qa-rm563088/_workitems/edit/9) |
| MAN-10 | [Issue 10](https://dev.azure.com/gs-qa-rm5630880147/Qa-rm563088/_workitems/edit/10) |
| MAN-11 | [Issue 11](https://dev.azure.com/gs-qa-rm5630880147/Qa-rm563088/_workitems/edit/11) |
| MAN-12 | [Issue 12](https://dev.azure.com/gs-qa-rm5630880147/Qa-rm563088/_workitems/edit/12) |
| MAN-13 | [Issue 13](https://dev.azure.com/gs-qa-rm5630880147/Qa-rm563088/_workitems/edit/13) |
| MAN-14 | [Issue 14](https://dev.azure.com/gs-qa-rm5630880147/Qa-rm563088/_workitems/edit/14) |
| MAN-15 | [Issue 15](https://dev.azure.com/gs-qa-rm5630880147/Qa-rm563088/_workitems/edit/15) |

O projeto usa o processo Azure Basic, que oferece os work items Epic, Issue e Task; o recurso Test Plans informa licença limitada. Por isso, cada caso foi registrado como Issue com objetivo, entrada, saída esperada e procedimento completos na descrição, em vez de um Test Case nativo. Para cada execução futura, registrar data/hora e ambiente; resultado Passed/Failed/Blocked; build/commit; código HTTP e resposta sanitizada; IDs gerados de QA; captura de tela/log sem token, senha, connection string ou hash. Casos dependentes de Oracle só podem ser marcados Passed após execução em Oracle QA.
