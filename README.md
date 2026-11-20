# Testes do Alerta Cidadão

Este repositório junta os materiais de QA da Sprint 4 do Alerta Cidadão.

## Plano manual

O plano tem 15 casos com objetivo, dados de entrada, resultado esperado e passos. Os casos estão no Azure Boards, na iteração Sprint 4, como Issues 1 a 15.

- [Plano de testes manuais](docs/qa/Plano-Testes-Manuais-Sprint4.md)
- [Projeto no Azure DevOps](https://dev.azure.com/gs-qa-rm5630880147/Qa-rm563088)
- [Caso MAN-15 no Azure Boards](https://dev.azure.com/gs-qa-rm5630880147/Qa-rm563088/_workitems/edit/15)

## Automação

Há cinco casos xUnit selecionados para a demonstração e um runner separado para carga leve com a API iniciada em Kestrel.

- [Como preparar e executar](docs/qa/Automacao-Testes-Sprint4.md)
- [Script dos cinco casos xUnit](docs/qa/executar-testes-evidencia.ps1)
- [Runner de carga](tools/AlertaCidadao.LoadTest/Program.cs)
- [Matriz de cobertura](docs/qa/Matriz-Cobertura-QA-Sprint4.md)
- [Resultados registrados](docs/qa/Resultados-Automatizados-Sprint4.md)
- [CSV da carga](docs/qa/evidencias/teste-carga-sprint4.csv)
- [Resumo da carga](docs/qa/evidencias/teste-carga-sprint4.md)

Na execução registrada, passaram 5/5 casos selecionados e 35/35 testes da suíte. O teste de carga enviou 1.000 chamadas locais para /health/live: todas retornaram HTTP 200 e o p95 foi 7,05 ms. A medição é local e não representa capacidade de produção.

Para repetir os cinco casos xUnit, na pasta deste repositório QA:

    .\docs\qa\executar-testes-evidencia.ps1 -AppRoot "C:\caminho\para\AlertaCidadao"

Para repetir a carga, na raiz do projeto Alerta Cidadão:

    dotnet run --project .\tools\AlertaCidadao.LoadTest\AlertaCidadao.LoadTest.csproj --configuration Release -- --base-url http://127.0.0.1:5075 --path /health/live --users 20 --requests 50 --p95-limit-ms 2000 --output .\artifacts\qa-evidence\teste-carga-sprint4.csv
