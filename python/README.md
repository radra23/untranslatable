# Untranslatable — Python / Flask

Python implementation of the Untranslatable API, instrumented with OpenTelemetry.

See the [root README](../README.md) for full project context, architecture, and getting-started instructions.

---

## Quick start

```bash
# Install dependencies
pip install -e .

# (Optional) point at a remote OTLP collector
export OTLP_ENDPOINT=http://localhost:4317

# Start the dev server
flask run   # → http://localhost:8000
```

Start the observability stack (Jaeger + OTel Collector) first:

```bash
docker compose up -d
```

---

## Endpoints

| Method | Path | Description |
|--------|------|-------------|
| `GET` | `/` | Welcome message |
| `GET` | `/words` | All words, or filtered by `?language=<code>` |
| `GET` | `/words/random` | A random untranslatable word |

---

## OTel instrumentation

| File | What it does |
|------|-------------|
| [app.py](app.py) | Configures `TracerProvider` + `MeterProvider` (both OTLP); registers Flask & Requests auto-instrumentation; defines route handlers with manual spans |
| [data/file_reader.py](data/file_reader.py) | Loads the JSON word list once via `@lru_cache`; wraps the load in a child span with a `words.count` attribute |

### Metrics emitted

| Name | Type | Labels |
|------|------|--------|
| `words.requests` | Counter | `language` |

---

## Dependencies

Managed in [setup.cfg](setup.cfg). Key packages:

| Package | Purpose |
|---------|---------|
| `Flask>=3.0` | Web framework |
| `opentelemetry-sdk` | Core OTel SDK |
| `opentelemetry-exporter-otlp-proto-grpc` | OTLP trace + metric export |
| `opentelemetry-instrumentation-flask` | Auto-instrumentation for Flask |
| `opentelemetry-instrumentation-requests` | Auto-instrumentation for outbound HTTP |
