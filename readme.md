📘 .NET Framework 4.8 Telemetry with OpenTelemetry + SigNoz

This project demonstrates how to instrument a .NET Framework 4.8 application using OpenTelemetry, and export traces, metrics, and logs to SigNoz Cloud.

Because .NET Framework 4.8 cannot run in Linux containers (and Windows containers are unavailable in the current environment), the app runs locally on Windows, and a separate OpenTelemetry Collector runs in Docker.
The Collector receives telemetry via OTLP, prints it for debugging, and forwards it to SigNoz.

🏗 Architecture Overview
┌──────────────────────────────────┐
│     .NET Framework 4.8 App      │   (runs on Windows host)
│  - OpenTelemetry SDK            │
│  - Traces, Metrics, Logs        │
│  - OTLP (gRPC) Exporter         │
└───────────────┬─────────────────┘
                │  localhost:4317
                ▼
┌──────────────────────────────────┐
│      OpenTelemetry Collector     │   (runs in Docker - Linux)
│  - Receives OTLP data           │
│  - Debug exporter (stdout)      │
│  - SigNoz exporter              │
└───────────────┬─────────────────┘
                │  HTTPS (OTLP)
                ▼
┌──────────────────────────────────┐
│           SigNoz Cloud          │
│ ingest.us.staging.signoz.cloud │
└──────────────────────────────────┘

❓ Why Use a Separate OpenTelemetry Collector?
✔ .NET Framework 4.8 Cannot Run in Linux Containers

Only Windows Server Core containers support .NET 4.8 — but your environment only supports Linux containers.
So the app must run directly on Windows, not in Docker.

✔ The Collector Adds Production-Grade Features

The OpenTelemetry Collector provides:

Central aggregation point

Buffering, batching, retries

Multi-export pipelines

Local debug exporter

TLS handling

Ability to export to SigNoz Cloud directly

✔ Cleaner App Code

Your application only talks to one endpoint:

OTLP gRPC → http://localhost:4317


The Collector handles the rest.

📂 Project Structure
net48-signoz-demo/
│
├── app/
│   ├── Net48OtelSignozDemo.csproj
│   ├── Program.cs
│
└── otel/
    ├── docker-compose.yml
    └── signoz-collector.yaml

⚙️ Collector Configuration

The Collector:

Listens on 4317 (gRPC) and 4318 (HTTP)

Uses the debug exporter to print received telemetry

Sends data to SigNoz Cloud

SigNoz staging ingest endpoint:

https://ingest.us.staging.signoz.cloud:443


Your ingestion key:

z-DZtRQnZQ6iIafBH9cPUCUb-NkxW0gJamvj

🚀 How to Run
✅ 1. Start the OpenTelemetry Collector

Open PowerShell and run:

cd otel
docker compose up -d


Check logs:

docker logs -f otel-collector


You should see the collector running and waiting for OTLP data.

✅ 2. Build the .NET Framework 4.8 App

Inside the app/ folder:

If you have .NET SDK:
dotnet restore
dotnet build -c Release

If dotnet is not installed (common on Win machines):

Use MSBuild:

"C:\Path\To\MSBuild.exe" Net48OtelSignozDemo.csproj /p:Configuration=Release

✅ 3. Run the App
cd app\bin\Release\net48
.\Net48OtelSignozDemo.exe


As the app runs:

Traces, metrics, logs → Collector (localhost:4317)

Collector shows them in Docker logs (debug exporter)

Collector forwards to SigNoz Cloud

🔍 Verify in SigNoz

Go to your SigNoz staging dashboard.

You should see a new service:

net48-signoz-demo


Available telemetry:

Traces

Runtime metrics (GC, CPU, memory)

Logs

🛠 Troubleshooting
❗ “I don’t see anything in SigNoz”

Run:

docker logs -f otel-collector


If you see data in debug exporter:

App → Collector ✔

Collector → SigNoz ❌ (check ingestion key or firewall)

If you see nothing:

App not sending to correct endpoint

Collector not listening on 4317

Firewall blocking localhost traffic

❗ “App cannot start exporter”

Ensure OTLP endpoint is:

o.Endpoint = new Uri("http://localhost:4317");
o.Protocol = OtlpExportProtocol.Grpc;

❗ “Collector fails to load config”

New OTel Collector versions reject the old logging exporter.
Use debug exporter instead (included in this repo).

🧩 Key Design Decisions

The .NET app is not containerized because .NET Framework cannot run in Linux Docker.

The OTel Collector is containerized because it is cross-platform.

Telemetry flows through the Collector to:

Allow local debugging

Simplify endpoint configuration

Decouple the app from backend specifics

Support retries + batching

This setup mirrors how production microservices handle telemetry.

🎯 Summary

This project demonstrates:

Full OpenTelemetry instrumentation for .NET Framework 4.8

A local OTel Collector running in Docker

Telemetry flowing to SigNoz Cloud

Full debuggability using the debug exporter

You now have a working template to instrument any .NET Framework application.