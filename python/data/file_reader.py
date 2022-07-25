from os.path import dirname, join
from opentelemetry import trace
import json

tracer = trace.get_tracer(__name__)
ctx = trace.get_current_span().get_span_context()

link_from_current = trace.Link(ctx)


def read_json_from_file():
    here = dirname(__file__)
    with tracer.start_as_current_span(
        "read_json_from_file", links=[link_from_current]
    ) as current_span:
        current_span.add_event("Opening data file.")
        with open(join(here, "./data.json"), encoding="utf-8") as f:
            data = json.loads(f.read())
        current_span.add_event("Finished reading data file.")
    return data
