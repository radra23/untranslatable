import json
import random
from flask import Flask, Response, request
from word import WordModel
from data.file_reader import read_json_from_file
from opentelemetry import trace
from opentelemetry.sdk.trace import TracerProvider
from opentelemetry.exporter.jaeger.thrift import JaegerExporter
from opentelemetry.sdk.resources import SERVICE_NAME, Resource
from opentelemetry.sdk.trace.export import BatchSpanProcessor
from opentelemetry.instrumentation.flask import FlaskInstrumentor
from opentelemetry.instrumentation.requests import RequestsInstrumentor

jaeger_exporter = JaegerExporter(
    agent_host_name="localhost",
    agent_port=6831,
    collector_endpoint='http://localhost:14268/api/traces?format=jaeger.thrift',
)

tracer_provider = TracerProvider(resource = Resource.create(attributes={SERVICE_NAME: "unstranslatable-python"}))
trace.set_tracer_provider(tracer_provider)
tracer = trace.get_tracer(__name__)
tracer_provider.add_span_processor(BatchSpanProcessor(jaeger_exporter))


app = Flask(__name__)

FlaskInstrumentor().instrument_app(app)
RequestsInstrumentor().instrument()


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
        data_str = json.dumps(data, ensure_ascii=False)
        words = WordModel.schema().loads(data_str, many=True)
        random_word = random.choice(words)
    return Response(random_word.to_json(), mimetype="application/json", status=200)


@app.route("/words", methods=["GET"])
def words_language():
    with tracer.start_as_current_span("word-by-language"):
        language = request.args.get("language")
        data = read_json_from_file()
        data_str = json.dumps(data, ensure_ascii=False)
        words = WordModel.schema().loads(data_str, many=True)
        words_for_language = [word for word in words if word.language == language]
    return Response(
        WordModel.schema().dumps(words_for_language, many=True),
        mimetype="application/json",
        status=200,
    )


if __name__ == "__main__":
    app.run(debug=True, use_reloader=False)
