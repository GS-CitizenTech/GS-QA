# GS-QA — Alerta Cidadão

Entrega de **Compliance, Quality Assurance & Tests — Sprint 4**.

## Plano manual de validação de sistema

[Plano de testes manuais Sprint 4](docs/qa/Plano-Testes-Manuais-Sprint4.md) — 13 casos com objetivo, entradas controladas, saídas esperadas, pré-condições e passos.

## Automação de integração

[Plano e instruções dos quatro testes automatizados](docs/qa/Automacao-Testes-Sprint4.md)  
[Runner PowerShell](docs/qa/executar-testes-evidencia.ps1)

A execução usa xUnit + ASP.NET Core TestServer para a API. Para executá-la neste repositório, informe a raiz local do projeto AlertaCidadao:

```powershell
.\docs\qa\executar-testes-evidencia.ps1 -AppRoot "C:\caminho\para\AlertaCidadao"
```

## Resultado registrado

[Relatório de resultados](docs/qa/Resultados-Automatizados-Sprint4.md): quatro casos automatizados aprovados e suíte completa com 34 aprovados na execução registrada.

A evidência em vídeo da configuração e execução e o link do Azure Boards serão acrescentados após a gravação e a criação da organização/projeto Azure DevOps.
