# Testes do Alerta Cidadão

Este repositório reúne os materiais de QA da Sprint 4 do projeto Alerta Cidadão.

## O que foi preparado

- [Plano de testes manuais](docs/qa/Plano-Testes-Manuais-Sprint4.md), com 14 casos, dados de entrada, resultados esperados e passos para execução.
- [Automação](docs/qa/Automacao-Testes-Sprint4.md), com quatro testes de integração feitos em xUnit e ASP.NET Core TestServer.
- [Script para executar os testes](docs/qa/executar-testes-evidencia.ps1).
- [Resultados registrados](docs/qa/Resultados-Automatizados-Sprint4.md). Na execução documentada, os quatro testes selecionados passaram e a suíte completa fechou com 34 aprovados.
- [Matriz de cobertura](docs/qa/Matriz-Cobertura-QA-Sprint4.md), com os pontos cobertos e o que ainda depende de configuração ou implementação.

## Azure Boards

Os casos MAN-01 a MAN-14 estão cadastrados na iteração Sprint 4 do projeto [Qa-rm563088](https://dev.azure.com/gs-qa-rm5630880147/Qa-rm563088). Os links de cada item estão no plano de testes.

## Como executar

O script precisa do caminho local para a pasta do projeto Alerta Cidadão:

```powershell
.\docs\qa\executar-testes-evidencia.ps1 -AppRoot "C:\caminho\para\AlertaCidadao"
```

O vídeo mostrando a configuração e a execução será acrescentado depois da gravação.
