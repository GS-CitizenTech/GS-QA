param(
    [string]$AppRoot
)

$ErrorActionPreference = 'Stop'

if ([string]::IsNullOrWhiteSpace($AppRoot)) {
    $candidateRoot = (Resolve-Path (Join-Path $PSScriptRoot '..\..')).Path
    if (Test-Path -LiteralPath (Join-Path $candidateRoot 'AlertaCidadao.sln')) {
        $AppRoot = $candidateRoot
    } else {
        throw 'Informe -AppRoot com o caminho da raiz do projeto AlertaCidadao.'
    }
}

$repoRoot = (Resolve-Path -LiteralPath $AppRoot).Path
$project = Join-Path $repoRoot 'test\AlertaCidadao.UnitTests\AlertaCidadao.UnitTests.csproj'
$resultsDirectory = Join-Path $repoRoot 'artifacts\qa-evidence'
$trxPath = Join-Path $resultsDirectory 'sprint4-automacao.trx'
$logPath = Join-Path $resultsDirectory 'sprint4-automacao-console.log'

$filter = 'FullyQualifiedName~EndpointProtegido_SemToken_DeveRetornar401|FullyQualifiedName~EndpointGestor_ComTokenCidadao_DeveRetornar403|FullyQualifiedName~PoliticaLogin_DeveBloquearAposDuasRequisicoesERetornarRetryAfter|FullyQualifiedName~Readiness_DeveRetornar503QuandoOracleNaoPuderSerResolvido'

New-Item -ItemType Directory -Path $resultsDirectory -Force | Out-Null
Write-Host 'Alerta Cidadão | Automação de integração | Sprint 4'
Write-Host 'Ferramenta: xUnit + ASP.NET Core TestServer | Ambiente: local isolado | Oracle: não conectado'
Write-Host 'Limite de login de teste: 2 chamadas por 60 segundos; o terceiro POST deve retornar 429.'
Write-Host 'Executando os quatro casos selecionados...'

$testArguments = @(
    'test', $project,
    '--configuration', 'Release',
    '--filter', $filter,
    '--logger', 'console;verbosity=minimal',
    '--logger', 'trx;LogFileName=sprint4-automacao.trx',
    '--results-directory', $resultsDirectory
)

& dotnet @testArguments *> $logPath
$testExitCode = $LASTEXITCODE

if (-not (Test-Path -LiteralPath $trxPath)) {
    Write-Host 'Falha: o runner não gerou o arquivo TRX.' -ForegroundColor Red
    Get-Content -LiteralPath $logPath -Tail 30
    exit 1
}

[xml]$trx = Get-Content -LiteralPath $trxPath -Raw
$results = @($trx.TestRun.Results.UnitTestResult)
$failed = @($results | Where-Object { $_.outcome -ne 'Passed' })

foreach ($result in $results) {
    $shortName = ($result.testName -split '\.')[-1]
    Write-Host ("{0}: {1}" -f $shortName, $result.outcome)
}

Write-Host ("Resumo: {0} executados | {1} aprovados | {2} com falha" -f $results.Count, ($results.Count - $failed.Count), $failed.Count)
Write-Host 'Arquivo TRX gerado em artifacts/qa-evidence/sprint4-automacao.trx.'

if ($testExitCode -ne 0 -or $results.Count -ne 4 -or $failed.Count -ne 0) {
    Get-Content -LiteralPath $logPath -Tail 30
    exit 1
}

exit 0

