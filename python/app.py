import json
import random
from flask import Flask, Response, request
from word import WordModel
from data.file_reader import read_json_from_file

app = Flask(__name__)


@app.route("/")
@app.route("/home")
@app.route("/index")
def index():
    return Response("Welcome to Unstranslatable!", status=200)


@app.route("/words/random", methods=["GET"])
def word_random():
    data = read_json_from_file()
    data_str = json.dumps(data, ensure_ascii=False)
    words = WordModel.schema().loads(data_str, many=True)
    random_word = random.choice(words)
    return Response(random_word.to_json(), mimetype="application/json", status=200)


@app.route("/words", methods=["GET"])
def words_language():
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
    app.run(host="localhost", port=8000, debug=True)
