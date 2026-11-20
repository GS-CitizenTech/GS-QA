using System.Collections.Concurrent;
using System.Diagnostics;
using System.Net;
using AlertaCidadao.API.HealthChecks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace AlertaCidadao.UnitTests.Performance;

public sealed class HealthCheckLoadIntegrationTests(ITestOutputHelper output)
{
    private const int UsuariosVirtuais = 20;
    private const int RequisicoesPorUsuario = 50;
    private const int TotalRequisicoes = UsuariosVirtuais * RequisicoesPorUsuario;
    private const double LimiteP95Milissegundos = 2000;

    [Fact]
    public async Task Liveness_SobCargaControlada_DeveResponder200EManterP95AbaixoDoLimite()
    {
        await using var app = await CriarAplicacaoAsync();
        using var client = app.GetTestClient();
        var resultados = new ConcurrentBag<(HttpStatusCode Status, double DuracaoMs, string Corpo)>();
        var cronometroTotal = Stopwatch.StartNew();

        await Parallel.ForEachAsync(
            Enumerable.Range(0, UsuariosVirtuais),
            new ParallelOptions { MaxDegreeOfParallelism = UsuariosVirtuais },
            async (_, cancellationToken) =>
            {
                for (var numero = 0; numero < RequisicoesPorUsuario; numero++)
                {
                    var cronometro = Stopwatch.StartNew();
                    using var resposta = await client.GetAsync("/health/live", cancellationToken);
                    var corpo = await resposta.Content.ReadAsStringAsync(cancellationToken);
                    cronometro.Stop();
                    resultados.Add((resposta.StatusCode, cronometro.Elapsed.TotalMilliseconds, corpo));
                }
            });

        cronometroTotal.Stop();
        var ordenadas = resultados.Select(resultado => resultado.DuracaoMs).Order().ToArray();
        var indiceP95 = (int)Math.Ceiling(ordenadas.Length * 0.95) - 1;
        var p95 = ordenadas[indiceP95];
        var vazao = resultados.Count / cronometroTotal.Elapsed.TotalSeconds;

        output.WriteLine($"Requisições: {resultados.Count}; concorrência: {UsuariosVirtuais}; duração: {cronometroTotal.Elapsed.TotalMilliseconds:F0} ms.");
        output.WriteLine($"Latência média: {ordenadas.Average():F2} ms; p95: {p95:F2} ms; vazão observada: {vazao:F2} req/s.");

        Assert.Equal(TotalRequisicoes, resultados.Count);
        Assert.All(resultados, resultado =>
        {
            Assert.Equal(HttpStatusCode.OK, resultado.Status);
            Assert.Contains("Healthy", resultado.Corpo, StringComparison.Ordinal);
            Assert.Contains("self", resultado.Corpo, StringComparison.Ordinal);
            Assert.DoesNotContain("oracle", resultado.Corpo, StringComparison.OrdinalIgnoreCase);
        });
        Assert.True(
            p95 <= LimiteP95Milissegundos,
            $"O p95 foi {p95:F2} ms; o limite deste smoke test é {LimiteP95Milissegundos:F0} ms.");
    }

    private static async Task<WebApplication> CriarAplicacaoAsync()
    {
        var builder = WebApplication.CreateBuilder();
        builder.Logging.ClearProviders();
        builder.WebHost.UseTestServer();
        builder.Services.AddHealthChecks()
            .AddCheck(
                "self",
                () => HealthCheckResult.Healthy("A API está em execução."),
                tags: new[] { "live" });

        var app = builder.Build();
        app.MapHealthChecks("/health/live", new HealthCheckOptions
        {
            Predicate = check => check.Tags.Contains("live"),
            ResponseWriter = HealthCheckResponseWriter.WriteResponse
        })
        .AllowAnonymous()
        .DisableRateLimiting();

        await app.StartAsync();
        return app;
    }
}
