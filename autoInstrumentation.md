Set-ExecutionPolicy -Scope Process -ExecutionPolicy Bypass -Force

$module_url   = "https://github.com/open-telemetry/opentelemetry-dotnet-instrumentation/releases/latest/download/OpenTelemetry.DotNet.Auto.psm1"
$downloadPath = Join-Path $env:TEMP "OpenTelemetry.DotNet.Auto.psm1"

Invoke-WebRequest -Uri $module_url -OutFile $downloadPath -UseBasicParsing

Import-Module $downloadPath

Install-OpenTelemetryCore




Register-OpenTelemetryForCurrentSession -OTelServiceName "net48-auto-demo"

$env:OTEL_TRACES_EXPORTER="otlp"
$env:OTEL_METRICS_EXPORTER="otlp"
$env:OTEL_LOGS_EXPORTER="otlp"

$env:OTEL_EXPORTER_OTLP_ENDPOINT="http://localhost:4318"
$env:OTEL_LOG_LEVEL="debug"



Application -> Otel Collector -> SigNoz Collector

Application Server 
    AppService 1
    AppService 2