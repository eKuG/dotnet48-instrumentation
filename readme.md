.NET Framework 4.8 Telemetry with OpenTelemetry and SigNoz

This repository demonstrates how to instrument a .NET Framework 4.8 application using OpenTelemetry, and export traces, metrics, and logs to SigNoz Cloud.

Because .NET Framework 4.8 cannot run in Linux containers, and Windows container support is unavailable in this environment, the application runs directly on the Windows host, while the OpenTelemetry Collector runs in Docker (Linux).

The Collector receives telemetry via OTLP, prints it locally using a debug exporter, and forwards it to SigNoz Cloud.


Why Use a Separate OpenTelemetry Collector?

.NET Framework 4.8 cannot run inside Linux containers.
Docker Desktop is running Linux containers, so your .NET 4.8 app must run on the host.

Collector is cross-platform and safe to run in Docker.
It can receive telemetry from any app and forward it to multiple backends.

Collector adds production features:

batching

retrying

TLS handling

multi-export (SigNoz + local debug)

consistent ingestion endpoint


Why This Setup Works

Avoids the need for Windows containers.

Allows .NET Framework 4.8 to be instrumented cleanly.

Collector provides export stability and debugging visibility.

Works identically in local, staging, and production environments.

Summary

This project provides:

A fully instrumented .NET Framework 4.8 application

A Dockerized OpenTelemetry Collector

Automatic export to SigNoz Cloud

Local debugging via the Collector

A reproducible template for real applications

