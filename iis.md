1. Install the auto-instrumentation core

```
Set-ExecutionPolicy -Scope Process -ExecutionPolicy Bypass -Force

$module_url   = "https://github.com/open-telemetry/opentelemetry-dotnet-instrumentation/releases/latest/download/OpenTelemetry.DotNet.Auto.psm1"
$downloadPath = Join-Path $env:TEMP "OpenTelemetry.DotNet.Auto.psm1"

Invoke-WebRequest -Uri $module_url -OutFile $downloadPath -UseBasicParsing
Import-Module $downloadPath

Install-OpenTelemetryCore
```

This puts the profiler/runtime bits on the box and exposes commands like Register-OpenTelemetryForIIS.

2.  Enable auto-instrumentation for IIS (once per server)

```
Register-OpenTelemetryForIIS
```

This:

Sets the CLR profiler env vars for IIS worker processes.

Ensures any .NET Framework apps running under IIS can be auto-instrumented.

You do not run this 60 times. Once per machine is enough.

3. 
   - 3.1 Method 1 — Modify every web.config programmatically (most explicit, simplest to audit)

Works for all IIS apps, regardless of .NET Framework version.

Script: Add/Update OTEL_SERVICE_NAME in every web.config

```
$apps = Get-ChildItem "C:\inetpub\wwwroot" -Directory -Recurse

foreach ($app in $apps) {
    $configPath = Join-Path $app.FullName "web.config"

    if (Test-Path $configPath) {
        Write-Host "Updating: $configPath"

        [xml]$xml = Get-Content $configPath

        # Ensure <appSettings> exists
        if (-not $xml.configuration.appSettings) {
            $appSettings = $xml.CreateElement("appSettings")
            $xml.configuration.AppendChild($appSettings) | Out-Null
        }

        $node = $xml.configuration.appSettings.add | Where-Object { $_.key -eq "OTEL_SERVICE_NAME" }

        if ($node) {
            # Update existing value
            $node.value = $app.Name    # or any logic you want
        }
        else {
            # Add new <add> node
            $newNode = $xml.CreateElement("add")
            $newNode.SetAttribute("key", "OTEL_SERVICE_NAME")
            $newNode.SetAttribute("value", $app.Name)
            $xml.configuration.appSettings.AppendChild($newNode) | Out-Null
        }

        # Save changes
        $xml.Save($configPath)
    }
}
```

Result

Every app gets:

```
<add key="OTEL_SERVICE_NAME" value="AppDirectoryName" />
```

Or you can generate names using:

```
$app.FullName

$app.Name + "-prod"

"mycompany-" + $app.Name
```

A mapping table

Anything you need

Pros

- Full control of naming
- Permanent config checked into code
- Easy to reason about

Cons

- Changes every web.config


3

- 3.2 Method 2 — Set environment variables per App Pool (NOT modifying web.config)

IIS allows app-pool-level environment variables.

This is often the cleanest + most professional solution, especially with many apps.

PowerShell script (per app pool):

```
$appPools = Get-ChildItem IIS:\AppPools

foreach ($pool in $appPools) {
    $serviceName = $pool.Name  # or any logic

    Write-Host "Setting OTEL_SERVICE_NAME=$serviceName for AppPool: $($pool.Name)"

    Set-ItemProperty "IIS:\AppPools\$($pool.Name)" -Name environmentVariables -Value @{ OTEL_SERVICE_NAME = $serviceName }
}
```

After running:
Restart IIS:

```
iisreset
```

Result

Every worker process (w3wp.exe) in that app pool gets:

OTEL_SERVICE_NAME = `<AppPoolName>`


Each app pool is treated as one service.

Pros

- No need to edit web.config
- Clear separation per app pool
- Best for large multi-app servers
- Easy to maintain

Cons

- All apps inside same app pool share one OTEL_SERVICE_NAME

If you truly need one per application, put each app into its own pool (very common in enterprise IIS servers).