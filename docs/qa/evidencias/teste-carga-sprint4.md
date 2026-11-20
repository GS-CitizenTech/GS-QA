# Resultado do teste de carga controlada

- Data/hora local: 2026-11-19 23:25:26 -03:00
- Alvo: http://127.0.0.1:5187/health/live
- Carga: 20 usuários virtuais, 50 chamadas por usuário, 1000 no total
- Respostas válidas: 1000/1000
- Latência: média 2,68 ms; p50 0,79 ms; p95 7,05 ms; p99 67,90 ms
- Vazão observada: 5894,07 requisições/s
- Critério: HTTP 200 em todas as chamadas e p95 até 2000 ms
- Resultado: APROVADO

Este teste é uma carga leve, controlada e local. A rota /health/live não consulta o Oracle. Os números servem para comparar execuções no mesmo ambiente e não representam capacidade de produção.
