# Untranslatable

> *"When translators talk about untranslatable, they often reinforce the notion that each language has its own 'genius', an 'essence' that naturally sets it apart from all other languages and reflects something of the 'soul' of its culture or people."*
> — Alexandra Jaffe

---

An untranslatable word has no equivalent when moved into another language. The term describes the difficulty of achieving a perfect translation, based on the notion that certain concepts are so intertwined with their culture that an exact rendering becomes impossible. Meaning, however, can almost always be conveyed.

![API diagram](/assets/image1.png)

## What is this?

This repository demonstrates **OpenTelemetry (OTel) instrumentation** across two parallel API implementations — one in **.NET 9** and one in **Python/Flask** — serving a collection of untranslatable words from various languages. Both APIs expose the same endpoints and produce the same telemetry signals, illustrating how OTel provides a consistent observability layer regardless of the technology stack.

![Architecture diagram](/assets/image2.png)

Telemetry is exported via **OTLP** to an [OpenTelemetry Collector](https://opentelemetry.io/docs/collector/), which fans out all three signals to the [Grafana LGTM stack](https://grafana.com/oss/): Tempo (traces), Loki (logs), and Prometheus (metrics). Grafana provides the unified UI with pre-configured cross-signal correlation.

---

## Features

- **Distributed tracing** — automatic HTTP instrumentation plus manual spans with events and attributes
- **Metrics** — request counters and duration histograms exported via OTLP
- **Consistent API** across Python and .NET — same endpoints, same OTel signals
- **OTLP-first** — a single exporter pipeline works with every modern observability backend
- **Logs** — all log output flows through OTel to Loki; trace and log records are correlated by trace ID
- **Docker Compose** — spins up the full Grafana LGTM stack (Loki + Grafana + Tempo + Prometheus) locally with one command

---

## Endpoints

| Method | Path | Description |
|--------|------|-------------|
| `GET` | `/` | Welcome message |
| `GET` | `/words` | All words (or filtered by `?language=<code>`) |
| `GET` | `/words/random` | A random untranslatable word |
| `GET` | `/healthz` | Health check |

### Language codes

| Code | Language |
|------|----------|
| `da` | Danish |
| `de` | German |
| `es` | Spanish |
| `fi` | Finnish |
| `fr` | French |
| `is` | Icelandic |
| `it` | Italian |
| `ja` | Japanese |
| `nl` | Dutch |
| `nn` | Norwegian (Nynorsk) |
| `pt` | Portuguese |
| `sv` | Swedish |

---

## Project structure

```
untranslatable/
├── python/                         # Python / Flask implementation
│   ├── app.py                      # Application entry point + OTel setup
│   ├── data/
│   │   ├── file_reader.py          # Cached JSON loader with manual spans
│   │   └── data.json               # Word dataset
│   ├── docker-compose.yml          # Grafana LGTM stack + OTel Collector
│   ├── otel-collector-config.yml   # Collector pipeline configuration
│   └── setup.cfg                   # Package metadata & dependencies
└── dotnet/                         # .NET 9 / ASP.NET Core implementation
    └── src/
        ├── Untranslatable.Api/     # Controllers, Swagger, OTel setup
        ├── Untranslatable.Data/    # Repository pattern, JSON data source
        └── Untranslatable.Shared/  # Metrics factory & timing helpers
```

---

## Getting started

### Prerequisites

- **Python** — Python 3.13 and `pip`
- **.NET** — .NET 9 SDK (`dotnet --version` should show `9.x`)
- **Docker** — for running the Grafana LGTM stack and OTel Collector

### 1 — Start the observability infrastructure

```bash
cd python
docker compose up -d
```

Grafana will be available at <http://localhost:3000>. The "Untranslatable"
dashboard is pre-provisioned under **Dashboards → Untranslatable**. All three
OTel signals (traces, metrics, logs) are visible once both APIs are running.

### 2 — Run the Python API

```bash
cd python
pip install -e .
flask run          # runs on http://localhost:8000
```

To point the Python app at a remote collector, set:

```bash
export OTLP_ENDPOINT=http://<collector-host>:4317
```

### 3 — Run the .NET API

```bash
cd dotnet
dotnet run --project src/Untranslatable.Api
```

Swagger UI is available at <http://localhost:5000/swagger> when running in Development mode.

To point the .NET app at a remote collector, set:

```bash
export OTEL_EXPORTER_OTLP_ENDPOINT=http://<collector-host>:4317
```

---

## OpenTelemetry instrumentation

Both implementations follow the same OTel patterns:

| Signal | Mechanism | Details |
|--------|-----------|---------|
| **Traces** | Auto + manual | HTTP requests are auto-instrumented; key operations get manual spans with events and attributes |
| **Metrics** | Manual counters + histograms | `words.requests` counter per language; endpoint duration histograms |
| **Logs** | Bridge + explicit | Python: `LoggingHandler` bridges stdlib `logging` → OTel; route handlers emit explicit log records. .NET: `ILogger<T>` wired to OTel via `AddOpenTelemetry()` |
| **Export** | OTLP gRPC | Single pipeline to the OTel Collector; Tempo (traces), Loki (logs), Prometheus (metrics) |

### Python OTel highlights (`app.py` / `file_reader.py`)

- `TracerProvider` and `MeterProvider` both configured with the same `Resource`
- `FlaskInstrumentor` + `RequestsInstrumentor` for zero-effort HTTP tracing
- `@lru_cache` in `file_reader.py` — the JSON file is parsed once per process, not per request; the read is still wrapped in a span for visibility
- Custom `words.requests` counter labelled by `language`
- `LoggingHandler` bridges Python's `logging` module to OTel — Flask internals and explicit `_log.*` calls in route handlers both reach Loki

### .NET OTel highlights (`Program.cs`)

- `AddOpenTelemetry().WithTracing().WithMetrics()` — the modern unified setup
- `AddAspNetCoreInstrumentation()` registered for both traces and metrics
- Custom meter (`Untranslatable.Shared`) registered with `.AddMeter(Metrics.MeterName)`
- Manual spans created via an injected `Tracer` in the controller
- `builder.Logging.AddOpenTelemetry()` — `ILogger<T>` calls are forwarded to Loki with `trace_id`/`span_id` automatically stamped on each record
- `TimeMeasurement` records endpoint duration in **milliseconds** via a `Histogram<double>`

---

## Configuration

### Python

| Environment variable | Default | Description |
|----------------------|---------|-------------|
| `OTLP_ENDPOINT` | `http://localhost:4317` | OTLP gRPC collector endpoint |
| `FLASK_DEBUG` | `0` | Set to `1` to enable debug mode |
| `FLASK_RUN_PORT` | `8000` | Port the Flask dev server listens on |
| `OTEL_SERVICE_NAME` | `untranslatable-python` | Service name shown in Grafana |
| `OTEL_SERVICE_VERSION` | `0.1.0` | Service version shown in Grafana |
| `OTEL_DEPLOYMENT_ENVIRONMENT` | `local` | Deployment environment label |

### .NET

| Environment variable | Default | Description |
|----------------------|---------|-------------|
| `OTEL_EXPORTER_OTLP_ENDPOINT` | `http://localhost:4317` | OTLP gRPC collector endpoint (standard OTel env var) |
| `Words__ContentFilePath` | `./content/words.json` | Path to the word list JSON file |
| `OTEL_SERVICE_NAME` | `untranslatable-dotnet` | Service name shown in Grafana |
| `OTEL_SERVICE_VERSION` | `0.1.0` | Service version shown in Grafana |
| `OTEL_DEPLOYMENT_ENVIRONMENT` | `local` | Deployment environment label |

---

## Contributors

<a href="https://github.com/radra23/untranslatable/graphs/contributors">
  <img src="https://contrib.rocks/image?repo=radra23/untranslatable" />
</a>
