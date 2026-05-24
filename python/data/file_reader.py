"""Low-level JSON file loader with in-memory caching and OTel tracing."""

from __future__ import annotations

import json
from functools import lru_cache
from os.path import dirname, join

# Import the facade tracer — gracefully degrades to a no-op if OTel is unavailable.
from telemetry import tracer

_DATA_FILE = join(dirname(__file__), "data.json")


@lru_cache(maxsize=1)
def _load_data() -> list:
    """Load words from the JSON data file.

    The result is cached after the first call; the file is read exactly
    once per process lifetime.

    Returns
    -------
    list
        Parsed list of word dicts, each with ``language``, ``word``,
        and ``meaning`` keys.
    """
    with open(_DATA_FILE, encoding="utf-8") as f:
        return json.load(f)


def read_json_from_file() -> list:
    """Return the cached word list, wrapping the lookup in a best-effort OTel span.

    Data access is always guaranteed; the telemetry span is best-effort only.
    Any OTel failure is silently swallowed so that a broken exporter can never
    affect the response.

    Returns
    -------
    list
        All words from the data file.
    """
    # Fetch data first — the primary operation must never fail due to telemetry.
    data = _load_data()

    try:
        with tracer.start_as_current_span("read_json_from_file") as span:
            span.add_event("Words loaded from cache.")
            span.set_attribute("words.count", len(data))
    except Exception:  # noqa: BLE001
        pass  # telemetry failure is non-fatal

    return data
