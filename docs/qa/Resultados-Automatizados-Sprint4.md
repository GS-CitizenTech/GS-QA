# Resultado dos testes automatizados — Sprint 4

**Data da execução:** 19/11/2026 · **Ambiente:** local, .NET 10.0.404, `Microsoft.AspNetCore.TestHost` · **Banco:** nenhum banco real; as integrações isoladas usam TestServer e dependências simuladas.

## Resultado registrado

- Casos direcionados para a evidência: **4 aprovados, 0 falhas**.
- Suíte completa xUnit: **34 aprovados, 0 falhas, 0 ignorados**.
- Casos direcionados: ausência de JWT (`401`); cidadão em recurso exclusivo de gestor (`403`); terceira chamada após limite de duas no host isolado (`429` + `Retry-After`); readiness Oracle indisponível (`503` + `Unhealthy`).

Os resultados vêm da execução local dos comandos documentados em `Automacao-Testes-Sprint4.md`. O TRX da execução completa pode ser gerado novamente com `--logger "trx;LogFileName=sprint4-suite-completa.trx"`; não se inclui o TRX local neste repositório para evitar caminhos absolutos da máquina.

## Observações da execução

- A restauração/build reportou aviso `NU1900`: a fonte NuGet não respondeu ao pedido de consulta de vulnerabilidades durante esta execução. Os pacotes necessários já estavam disponíveis e os testes executaram; o aviso não é um resultado de teste nem uma auditoria de dependências atualizada.
- O host de teste JWT usa proteção de dados efêmera, e os hosts de teste removem providers de log dependentes do Event Log do Windows. Isso mantém as verificações reproduzíveis em CI e em ambiente sem permissão de escrita no log do sistema; não altera a configuração da API publicada.
- A readiness real do Oracle e os CRUDs devem ser executados manualmente em Oracle QA, conforme o plano. Não marcar esses casos como aprovados com base nos testes simulados.


