import json
from functools import lru_cache
from os.path import dirname, join

from opentelemetry import trace

tracer = trace.get_tracer(__name__)
_DATA_FILE = join(dirname(__file__), "data.json")


@lru_cache(maxsize=1)
def _load_data() -> list:
    """Load words from the JSON file.  Called once per process; result is cached."""
    with open(_DATA_FILE, encoding="utf-8") as f:
        return json.load(f)


def read_json_from_file() -> list:
    """Return the cached word list, wrapped in an OTel span."""
    with tracer.start_as_current_span("read_json_from_file") as span:
        span.add_event("Loading words from data store.")
        data = _load_data()
        span.set_attribute("words.count", len(data))
        span.add_event("Words loaded successfully.")
    return data
