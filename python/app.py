import json
import random

from flask import Flask, Response, request
from opentelemetry import metrics, trace
from opentelemetry.exporter.jaeger.thrift import JaegerExporter
from opentelemetry.exporter.otlp.proto.grpc.trace_exporter import OTLPSpanExporter
from opentelemetry.instrumentation.flask import FlaskInstrumentor
from opentelemetry.instrumentation.requests import RequestsInstrumentor
from opentelemetry.sdk.resources import SERVICE_NAME, Resource
from opentelemetry.sdk.trace import TracerProvider
from opentelemetry.sdk.trace.export import BatchSpanProcessor

from data.file_reader import read_json_from_file

resource = Resource(attributes={SERVICE_NAME: "unstranslatable-python"})
jaeger_exporter = JaegerExporter(
    agent_host_name="localhost",
    agent_port=6831,
    collector_endpoint="http://localhost:14268/api/traces?format=jaeger.thrift",
)

otlp_exporter = OTLPSpanExporter(endpoint="http://localhost:4317", insecure=True)
span_processor = BatchSpanProcessor(otlp_exporter)


tracer_provider = TracerProvider(resource=resource)
jaeger_processor = BatchSpanProcessor(jaeger_exporter)
tracer_provider.add_span_processor(jaeger_processor)

trace.set_tracer_provider(tracer_provider)
trace.get_tracer_provider().add_span_processor(span_processor)
tracer = trace.get_tracer(__name__)
meter = metrics.get_meter(__name__)

app = Flask(__name__)

FlaskInstrumentor().instrument_app(app)
RequestsInstrumentor().instrument()

word_counter = meter.create_counter(
    "words.counter", description="Counts the number of words returned."
)


@tracer.start_as_current_span("welcome-message")
@app.route("/")
@app.route("/home")
@app.route("/index")
def index():
    return Response("Welcome to Unstranslatable!", status=200)


@app.route("/words/random", methods=["GET"])
def word_random():
    with tracer.start_as_current_span("random-word"):
        data = read_json_from_file()
        words = json.dumps(data, ensure_ascii=False)
        random_word = random.choice(words)

    word_counter.add(
        1, {"word": random_word["word"], "language": random_word["language"]}
    )

    return Response(random_word, mimetype="application/json", status=200)


@app.route("/words", methods=["GET"])
def words_language():
    with tracer.start_as_current_span("word-by-language"):
        language = request.args.get("language")
        data = read_json_from_file()
        words = json.dumps(data, ensure_ascii=False)
        words_for_language = [word for word in words if word["language"] == language]

        word_counter.add(len(words_for_language), words_for_language)

    return Response(
        words_for_language,
        mimetype="application/json",
        status=200,
    )


if __name__ == "__main__":
    app.run(debug=True, use_reloader=False)
