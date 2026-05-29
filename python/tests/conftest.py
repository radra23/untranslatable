"""Shared pytest fixtures for the untranslatable test suite."""

from __future__ import annotations

import pytest

import telemetry
from app import create_app


@pytest.fixture(autouse=True)
def _null_telemetry(monkeypatch):
    """Replace live OTel objects with no-op stubs for every test.

    This prevents tests from needing a real OTLP collector and keeps
    telemetry failures from masking assertion failures.
    """
    monkeypatch.setattr(telemetry, "tracer", telemetry._NullTracer())
    monkeypatch.setattr(telemetry, "word_counter", telemetry._NullCounter())
    monkeypatch.setattr(telemetry, "logger", telemetry._NullLogger())


@pytest.fixture()
def app():
    """Return a Flask application instance configured for testing."""
    flask_app = create_app()
    flask_app.config["TESTING"] = True
    return flask_app


@pytest.fixture()
def client(app):
    """Return a Flask test client bound to the test application."""
    return app.test_client()
