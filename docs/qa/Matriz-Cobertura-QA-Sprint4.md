# Matriz de cobertura — Compliance QA — Sprint 4

## Escopo

Esta matriz compara os requisitos de **Compliance, Quality Assurance & Tests — 4º Sprint** do desafio com os casos cadastrados no Azure Boards e com a implementação verificada na cópia do projeto Alerta Cidadão. As demais disciplinas do documento não são avaliadas aqui.

## Requisitos de QA

| Requisito | Evidência atual | Situação |
|---|---|---|
| Plano manual de sistema com objetivo, entrada, saída e passos | 14 casos MAN-01 a MAN-14, cadastrados como Issues 1–14 na iteração `Qa-rm563088\Sprint 4`. | Coberto para os fluxos e endpoints implementados. |
| Valores de entrada controlados e resultados esperados | Corpos JSON, headers, IDs dinâmicos controlados, códigos HTTP e regras de saída estão descritos nos itens e no plano. | Coberto. |
| Alinhamento ao release e às tarefas da Sprint | Os itens indicam Release 4/Sprint 4. O projeto Azure não tinha backlog/release importado; a iteração Sprint 4 foi criada sem datas. | Parcial: falta relacionar os testes às tarefas/PBIs oficiais do grupo, caso existam em outro quadro. |
| Professor Basic na organização e administrador no projeto | O aluno informou que já enviou o convite. | Considerado feito conforme informado; confira os dois níveis de acesso. |
| Repositório público e branch `develop` | Repositório `GS-QA` confirmado público; branch `develop` existe com os artefatos de QA. | Feito. |
| Pelo menos quatro testes automatizados | AUT-01 a AUT-04 com xUnit + ASP.NET Core TestServer. Execução registrada: 4/4 aprovados; suíte completa: 34/34 aprovados. | Coberto para integração funcional. |
| Evidência em vídeo de configuração, execução e resultados | Nenhum vídeo foi produzido neste trabalho. | Pendente: o aluno fará a gravação. |

## Cobertura dos casos manuais

| Caso | Fluxo |
|---|---|
| MAN-01 | Raiz da API e contrato Swagger. |
| MAN-02–MAN-03 | Liveness e readiness com Oracle disponível/indisponível. |
| MAN-04–MAN-07 | Cadastro, validação, login JWT e autorização por perfil. |
| MAN-08 | CRUD e desativação lógica de ponto de monitoramento. |
| MAN-09 | Idempotência de gravação. |
| MAN-10 | Criação, consulta, filtro e duplicidade de leitura. |
| MAN-11 | Criação/resolução de alerta e integridade da leitura relacionada. |
| MAN-12 | Rate limiting e header `Retry-After`. |
| MAN-13 | CI, testes e artefato da branch `develop` (planejado; depende de pipeline configurada). |
| MAN-14 | Consulta, atualização e desativação lógica de usuário. |

## Lacunas verificadas frente aos requisitos técnicos .NET da Sprint 4

Estas lacunas não devem ser marcadas como aprovadas por um teste QA enquanto a funcionalidade ou o contrato não existir:

- **Paginação e ordenação selecionáveis:** as assinaturas atuais dos endpoints não expõem parâmetros de página/tamanho ou ordenação. Existem filtros como `idPonto`, `status` e `incluirInativos`, e alguns repositórios aplicam ordenação fixa.
- **HATEOAS:** não foram encontrados links de navegação/ação nas respostas atuais.
- **MongoDB:** não foram encontradas dependências ou integração MongoDB na solução verificada.
- **Migração em ambiente limpo:** há arquivos de migração EF Core e scripts SQL; os casos de CRUD exercitam Oracle QA, mas o plano não valida a aplicação das migrações em uma base Oracle vazia.
- **Logs estruturados/correlação:** há chamadas `ILogger` e níveis configurados; ainda não há caso que verifique formato estruturado, correlação de requisição e ausência de dados sensíveis nos logs.
- **Estresse/desempenho:** AUT-03 valida a regra funcional de 429; não mede concorrência, throughput ou latência. Não há endpoint/deployment QA com Oracle informado para executar carga com segurança.

Para fechar a rastreabilidade do release, associe MAN-01–MAN-14 aos PBIs/tarefas oficiais da Sprint 4 quando eles estiverem disponíveis. Os itens continuam `To Do`; não representam execução manual aprovada.

