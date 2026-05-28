"""Telemetry facade for the untranslatable API.

All OpenTelemetry wiring lives here. The rest of the application imports
``tracer`` and ``word_counter`` from this module — it never touches OTel
directly — so the provider can be swapped in one place without changing
any business logic.

Notes
-----
Resilience contract:

* If OTel initialisation fails for any reason the module falls back to
  lightweight no-op stubs; the application always starts successfully.
* Individual telemetry calls are guarded so that an unexpected runtime
  exception from the SDK never propagates to the caller.
"""

from __future__ import annotations

import logging
import os
from collections.abc import Generator
from contextlib import AbstractContextManager, contextmanager
from typing import Protocol

_log = logging.getLogger(__name__)

OTLP_ENDPOINT: str = os.environ.get("OTLP_ENDPOINT", "http://localhost:4317")
SERVICE_NAME: str = "untranslatable-python"


# ---------------------------------------------------------------------------
# Structural Protocols — define the minimum surface the rest of the app uses.
# Both the no-op stubs and the real OTel objects satisfy these Protocols
# through duck typing; no explicit inheritance is required.
# ---------------------------------------------------------------------------


class _SpanLike(Protocol):
    """Minimum span interface used by application code."""

    def set_attribute(self, key: str, value: object) -> None:
        """Set a key/value attribute on the span."""
        ...

    def add_event(self, name: str) -> None:
        """Add a named event to the span timeline."""
        ...


class _TracerLike(Protocol):
    """Minimum tracer interface used by application code."""

    def start_as_current_span(
        self, name: str, **kwargs: object
    ) -> AbstractContextManager[_SpanLike]:
        """Start a span and return it as a context manager."""
        ...


class _CounterLike(Protocol):
    """Minimum counter interface used by application code."""

    def add(self, amount: int, attributes: dict[str, str] | None = None) -> None:
        """Increment the counter by ``amount``."""
        ...


# ---------------------------------------------------------------------------
# No-op stubs — used when OTel is unavailable.
# ---------------------------------------------------------------------------


class _NullSpan:
    """Span that silently discards every call."""

    def set_attribute(self, key: str, value: object) -> None:
        """Accept and discard a span attribute."""

    def add_event(self, name: str) -> None:
        """Accept and discard a span event."""

    def __enter__(self) -> _NullSpan:
        """Enter the context manager."""
        return self

    def __exit__(self, *_: object) -> None:
        """Exit the context manager."""


class _NullTracer:
    """Tracer that produces no-op spans."""

    @contextmanager
    def start_as_current_span(self, name: str, **_: object) -> Generator[_NullSpan, None, None]:
        """Yield a no-op span without recording anything.

        Parameters
        ----------
        name : str
            Span name (ignored).
        **_ : object
            Any additional keyword arguments (ignored).

        Yields
        ------
        _NullSpan
            A span whose every method is a silent no-op.
        """
        yield _NullSpan()


class _NullCounter:
    """Metric counter that silently discards every increment."""

    def add(self, amount: int, attributes: dict[str, str] | None = None) -> None:
        """Accept and discard a counter increment.

        Parameters
        ----------
        amount : int
            Increment value (ignored).
        attributes : dict[str, str] or None, optional
            Metric attributes (ignored).
        """


# ---------------------------------------------------------------------------
# Module-level facade objects — initially no-ops; replaced by _setup().
# ---------------------------------------------------------------------------

tracer: _TracerLike = _NullTracer()
word_counter: _CounterLike = _NullCounter()


def _setup() -> None:
    """Configure OTel providers and assign the module-level facade objects.

    Sets up a ``TracerProvider`` and a ``MeterProvider``, both backed by
    OTLP gRPC exporters, then reassigns the module-level ``tracer`` and
    ``word_counter`` to the live OTel implementations.

    Called exactly once at import time; the caller catches all exceptions.

    Raises
    ------
    Exception
        Re-raised to the module-level try/except so the fallback stubs
        remain in place.
    """
    global tracer, word_counter  # noqa: PLW0603

    from opentelemetry import metrics as _metrics
    from opentelemetry import trace as _trace
    from opentelemetry.exporter.otlp.proto.grpc.metric_exporter import OTLPMetricExporter
    from opentelemetry.exporter.otlp.proto.grpc.trace_exporter import OTLPSpanExporter
    from opentelemetry.sdk.metrics import MeterProvider
    from opentelemetry.sdk.metrics.export import PeriodicExportingMetricReader
    from opentelemetry.sdk.resources import SERVICE_NAME as _SN_KEY
    from opentelemetry.sdk.resources import Resource
    from opentelemetry.sdk.trace import TracerProvider
    from opentelemetry.sdk.trace.export import BatchSpanProcessor

    resource = Resource(attributes={_SN_KEY: SERVICE_NAME})

    tracer_provider = TracerProvider(resource=resource)
    tracer_provider.add_span_processor(
        BatchSpanProcessor(OTLPSpanExporter(endpoint=OTLP_ENDPOINT, insecure=True))
    )
    _trace.set_tracer_provider(tracer_provider)

    meter_provider = MeterProvider(
        resource=resource,
        metric_readers=[
            PeriodicExportingMetricReader(OTLPMetricExporter(endpoint=OTLP_ENDPOINT, insecure=True))
        ],
    )
    _metrics.set_meter_provider(meter_provider)

    # The OTel Tracer stubs use _AgnosticContextManager and specific kwargs
    # rather than AbstractContextManager/**kwargs, so the structural check
    # fails even though both satisfy the Protocol at runtime.
    tracer = _trace.get_tracer(__name__)  # type: ignore[assignment]
    word_counter = _metrics.get_meter(__name__).create_counter(
        "words.requests",
        description="Number of words returned, labelled by language.",
    )

    _log.info("[telemetry] OpenTelemetry initialised (endpoint: %s)", OTLP_ENDPOINT)


try:
    _setup()
except Exception as _exc:  # noqa: BLE001
    _log.warning(
        "[telemetry] OTel setup failed — running without instrumentation. Reason: %s",
        _exc,
    )
    # tracer and word_counter remain as the no-op stubs assigned above
