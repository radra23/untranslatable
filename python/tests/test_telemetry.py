"""Unit tests for telemetry no-op stubs.

Verifies that all stub objects silently discard operations rather than
raising, matching the production OTel interface contract.
"""

from telemetry import _NullCounter, _NullLogger, _NullSpan, _NullTracer


def test_null_span_set_attribute_does_not_raise():
    _NullSpan().set_attribute("key", "value")


def test_null_span_add_event_does_not_raise():
    _NullSpan().add_event("event-name")


def test_null_tracer_yields_span_that_accepts_calls():
    tracer = _NullTracer()
    with tracer.start_as_current_span("test-span") as span:
        span.set_attribute("language", "pt")
        span.add_event("word-served")


def test_null_counter_add_with_attributes_does_not_raise():
    _NullCounter().add(1, {"language": "pt"})


def test_null_counter_add_without_attributes_does_not_raise():
    _NullCounter().add(5)


def test_null_logger_info_does_not_raise():
    _NullLogger().info("word served: %s", "saudade")


def test_null_logger_warning_does_not_raise():
    _NullLogger().warning("language not found: %s", "xx")


def test_null_logger_error_does_not_raise():
    _NullLogger().error("dataset is empty")
