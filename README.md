# GS-QA — Alerta Cidadão

Entrega de **Compliance, Quality Assurance & Tests — Sprint 4**.

## Validação manual

[Plano de testes manuais](docs/qa/Plano-Testes-Manuais-Sprint4.md) — 13 cenários com objetivos, entradas controladas, saídas esperadas e procedimentos.

O projeto [Qa-rm563088 no Azure DevOps](https://dev.azure.com/gs-qa-rm5630880147/Qa-rm563088) possui a iteração `Qa-rm563088\Sprint 4`. Os casos MAN-01 a MAN-13 foram cadastrados nos Boards como Issues 1–13. O projeto usa o processo Basic, que não oferece o tipo Test Case no menu de Boards, e o Azure Test Plans mostra acesso limitado.

| Caso | Azure Boards |
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

## Automação

[Plano e instruções dos quatro testes automatizados](docs/qa/Automacao-Testes-Sprint4.md)  
[Runner PowerShell](docs/qa/executar-testes-evidencia.ps1)

Para executar o runner neste repositório, informe a raiz local do projeto AlertaCidadao:

```powershell
.\docs\qa\executar-testes-evidencia.ps1 -AppRoot "C:\caminho\para\AlertaCidadao"
```

## Resultado registrado

[Relatório de resultados](docs/qa/Resultados-Automatizados-Sprint4.md): quatro testes automatizados aprovados e suíte completa com 34 aprovados na execução registrada.

O vídeo solicitado da configuração e execução ainda precisa ser acrescentado.
