using System.Collections.Concurrent;
using System.Diagnostics;
using System.Globalization;
using System.Net;
using System.Net.Http;
using System.Text;

var opcoes = OpcoesCarga.Ler(args);
if (opcoes is null)
{
    return 2;
}

var baseUri = new Uri(opcoes.BaseUrl);
if (!baseUri.IsLoopback && !opcoes.PermitirHostRemoto)
{
    Console.Error.WriteLine("Por segurança, o teste só aceita loopback por padrão. Para um ambiente de QA autorizado, use --allow-remote.");
    return 2;
}

using var handler = new SocketsHttpHandler
{
    MaxConnectionsPerServer = opcoes.UsuariosVirtuais,
    PooledConnectionLifetime = TimeSpan.FromMinutes(2)
};
using var cliente = new HttpClient(handler)
{
    BaseAddress = baseUri,
    Timeout = TimeSpan.FromSeconds(15)
};

var resultados = new ConcurrentBag<ResultadoRequisicao>();
var cronometroTotal = Stopwatch.StartNew();
var alvoExibido = baseUri.GetLeftPart(UriPartial.Path).TrimEnd('/') + opcoes.Caminho;
Console.WriteLine("Alerta Cidadão | teste de carga controlada");
Console.WriteLine($"Alvo: {alvoExibido}");
Console.WriteLine($"Carga: {opcoes.UsuariosVirtuais} usuários virtuais × {opcoes.RequisicoesPorUsuario} chamadas = {opcoes.TotalRequisicoes} requisições");
Console.WriteLine($"Limite de aprovação: HTTP 200 em todas as chamadas e p95 ≤ {opcoes.LimiteP95Ms.ToString("F0", CultureInfo.InvariantCulture)} ms");

using var cancelamento = new CancellationTokenSource(TimeSpan.FromMinutes(3));
try
{
    await Parallel.ForEachAsync(
        Enumerable.Range(0, opcoes.UsuariosVirtuais),
        new ParallelOptions
        {
            MaxDegreeOfParallelism = opcoes.UsuariosVirtuais,
            CancellationToken = cancelamento.Token
        },
        async (_, token) =>
        {
            for (var numero = 0; numero < opcoes.RequisicoesPorUsuario; numero++)
            {
                var inicio = Stopwatch.GetTimestamp();
                try
                {
                    using var resposta = await cliente.GetAsync(opcoes.Caminho, token);
                    var corpo = await resposta.Content.ReadAsStringAsync(token);
                    var latencia = Stopwatch.GetElapsedTime(inicio).TotalMilliseconds;
                    var corpoValido = opcoes.Caminho != "/health/live" ||
                        (corpo.Contains("Healthy", StringComparison.Ordinal) &&
                         corpo.Contains("self", StringComparison.Ordinal) &&
                         !corpo.Contains("oracle", StringComparison.OrdinalIgnoreCase));
                    resultados.Add(new(resposta.StatusCode, latencia, corpoValido, null));
                }
                catch (Exception erro) when (erro is HttpRequestException or TaskCanceledException)
                {
                    resultados.Add(new(HttpStatusCode.ServiceUnavailable, Stopwatch.GetElapsedTime(inicio).TotalMilliseconds, false, erro.GetType().Name));
                }
            }
        });
}
catch (OperationCanceledException)
{
    Console.Error.WriteLine("O teste excedeu o limite de três minutos e foi interrompido.");
}
finally
{
    cronometroTotal.Stop();
}

var ordenadas = resultados.OrderBy(resultado => resultado.LatenciaMs).ToArray();
if (ordenadas.Length == 0)
{
    Console.Error.WriteLine("Nenhuma resposta foi registrada.");
    return 1;
}

var p50 = Percentil(ordenadas, 0.50);
var p95 = Percentil(ordenadas, 0.95);
var p99 = Percentil(ordenadas, 0.99);
var aprovadas = ordenadas.Count(resultado =>
    resultado.Status == HttpStatusCode.OK && resultado.CorpoValido);
var vazao = ordenadas.Length / cronometroTotal.Elapsed.TotalSeconds;
var passou = ordenadas.Length == opcoes.TotalRequisicoes &&
    aprovadas == opcoes.TotalRequisicoes &&
    p95 <= opcoes.LimiteP95Ms;

Console.WriteLine($"Concluídas: {ordenadas.Length}/{opcoes.TotalRequisicoes}; respostas válidas: {aprovadas}; falhas: {ordenadas.Length - aprovadas}");
Console.WriteLine($"Latência (ms): mín {ordenadas[0].LatenciaMs:F2} | média {ordenadas.Average(resultado => resultado.LatenciaMs):F2} | p50 {p50:F2} | p95 {p95:F2} | p99 {p99:F2} | máx {ordenadas[^1].LatenciaMs:F2}");
Console.WriteLine($"Duração: {cronometroTotal.Elapsed.TotalSeconds:F2} s | vazão observada: {vazao:F2} req/s");
Console.WriteLine(passou ? "RESULTADO: APROVADO" : "RESULTADO: REPROVADO");

var caminhoCsv = Path.GetFullPath(opcoes.ArquivoCsv);
Directory.CreateDirectory(Path.GetDirectoryName(caminhoCsv)!);
var csv = new StringBuilder("numero,status,latencia_ms,corpo_valido,erro\n");
for (var indice = 0; indice < ordenadas.Length; indice++)
{
    var resultado = ordenadas[indice];
    csv.Append(indice + 1).Append(',')
        .Append((int)resultado.Status).Append(',')
        .Append(resultado.LatenciaMs.ToString("F3", CultureInfo.InvariantCulture)).Append(',')
        .Append(resultado.CorpoValido ? "sim" : "nao").Append(',')
        .Append(resultado.Erro ?? string.Empty).AppendLine();
}
await File.WriteAllTextAsync(caminhoCsv, csv.ToString(), Encoding.UTF8);

var resumoPath = Path.ChangeExtension(caminhoCsv, ".md");
var resumo = "# Resultado do teste de carga controlada\n\n" +
    $"- Data/hora local: {DateTimeOffset.Now:yyyy-MM-dd HH:mm:ss zzz}\n" +
    $"- Alvo: {alvoExibido}\n" +
    $"- Carga: {opcoes.UsuariosVirtuais} usuários virtuais, {opcoes.RequisicoesPorUsuario} chamadas por usuário, {opcoes.TotalRequisicoes} no total\n" +
    $"- Respostas válidas: {aprovadas}/{opcoes.TotalRequisicoes}\n" +
    $"- Latência: média {ordenadas.Average(resultado => resultado.LatenciaMs):F2} ms; p50 {p50:F2} ms; p95 {p95:F2} ms; p99 {p99:F2} ms\n" +
    $"- Vazão observada: {vazao:F2} requisições/s\n" +
    $"- Critério: HTTP 200 em todas as chamadas e p95 até {opcoes.LimiteP95Ms:F0} ms\n" +
    $"- Resultado: {(passou ? "APROVADO" : "REPROVADO")}\n\n" +
    "Este teste é uma carga leve, controlada e local. A rota /health/live não consulta o Oracle. Os números servem para comparar execuções no mesmo ambiente e não representam capacidade de produção.\n";
await File.WriteAllTextAsync(resumoPath, resumo, Encoding.UTF8);
Console.WriteLine($"CSV: {caminhoCsv}");
Console.WriteLine($"Resumo: {resumoPath}");
return passou ? 0 : 1;

static double Percentil(ResultadoRequisicao[] resultadosOrdenados, double percentual)
{
    var indice = Math.Clamp((int)Math.Ceiling(resultadosOrdenados.Length * percentual) - 1, 0, resultadosOrdenados.Length - 1);
    return resultadosOrdenados[indice].LatenciaMs;
}

internal sealed record ResultadoRequisicao(
    HttpStatusCode Status,
    double LatenciaMs,
    bool CorpoValido,
    string? Erro);

internal sealed record OpcoesCarga(
    string BaseUrl,
    string Caminho,
    int UsuariosVirtuais,
    int RequisicoesPorUsuario,
    double LimiteP95Ms,
    string ArquivoCsv,
    bool PermitirHostRemoto)
{
    public int TotalRequisicoes => UsuariosVirtuais * RequisicoesPorUsuario;

    public static OpcoesCarga? Ler(string[] args)
    {
        var valores = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var permitirHostRemoto = false;
        for (var indice = 0; indice < args.Length; indice++)
        {
            if (args[indice].Equals("--allow-remote", StringComparison.OrdinalIgnoreCase))
            {
                permitirHostRemoto = true;
                continue;
            }

            if (!args[indice].StartsWith("--", StringComparison.Ordinal) || indice + 1 >= args.Length)
            {
                Console.Error.WriteLine($"Argumento inválido: {args[indice]}");
                ImprimirAjuda();
                return null;
            }
            valores[args[indice][2..]] = args[++indice];
        }

        var baseUrl = valores.GetValueOrDefault("base-url", "http://127.0.0.1:5075");
        var caminho = valores.GetValueOrDefault("path", "/health/live");
        var usuarios = LerInteiro(valores, "users", 20, 1, 100);
        var requisicoes = LerInteiro(valores, "requests", 50, 1, 1000);
        var limiteP95 = LerDouble(valores, "p95-limit-ms", 2000);
        var csv = valores.GetValueOrDefault("output", "artifacts/qa-evidence/teste-carga-sprint4.csv");

        if (!Uri.TryCreate(baseUrl, UriKind.Absolute, out var uri) ||
            uri.Scheme is not ("http" or "https") ||
            string.IsNullOrWhiteSpace(caminho) || !caminho.StartsWith("/", StringComparison.Ordinal) ||
            caminho.StartsWith("//", StringComparison.Ordinal) ||
            usuarios is null || requisicoes is null || limiteP95 is null ||
            usuarios.Value * requisicoes.Value > 100_000)
        {
            Console.Error.WriteLine("Parâmetros inválidos. Verifique URL, rota, limites e total (máximo 100.000 requisições).");
            ImprimirAjuda();
            return null;
        }

        return new(baseUrl.TrimEnd('/'), caminho, usuarios.Value, requisicoes.Value, limiteP95.Value, csv, permitirHostRemoto);
    }

    private static int? LerInteiro(Dictionary<string, string> valores, string chave, int padrao, int minimo, int maximo)
    {
        if (!valores.TryGetValue(chave, out var texto))
            return padrao;
        return int.TryParse(texto, NumberStyles.Integer, CultureInfo.InvariantCulture, out var valor) &&
            valor >= minimo && valor <= maximo ? valor : null;
    }

    private static double? LerDouble(Dictionary<string, string> valores, string chave, double padrao)
    {
        if (!valores.TryGetValue(chave, out var texto))
            return padrao;
        return double.TryParse(texto, NumberStyles.Float, CultureInfo.InvariantCulture, out var valor) && valor > 0
            ? valor
            : null;
    }

    private static void ImprimirAjuda() =>
        Console.WriteLine("Uso: dotnet run --project tools/AlertaCidadao.LoadTest -- [--base-url http://127.0.0.1:5075] [--path /health/live] [--users 20] [--requests 50] [--p95-limit-ms 2000] [--output artifacts/qa-evidence/teste-carga-sprint4.csv] [--allow-remote]");
}
