"""HTTP layer for word endpoints.

This module is a Flask Blueprint. It handles request parsing and response
serialisation only — no business logic, no file I/O, no OTel wiring lives
here. Swap the Blueprint for a FastAPI router (or anything else) without
touching the layers below.
"""

from __future__ import annotations

from flask import Blueprint, Response, jsonify, request

import telemetry
from data.repository import WordsRepository

words_bp = Blueprint("words", __name__)

# Module-level singleton — WordsRepository is stateless (reads from an
# in-memory cache), so one shared instance is safe and allocation-free.
_repository = WordsRepository()


@words_bp.route("/words/random", methods=["GET"])
def word_random() -> tuple[Response, int]:
    """Return a single untranslatable word chosen at random.

    Returns
    -------
    flask.Response
        200 — a random word object.
        503 — if the dataset is empty.
    """
    with telemetry.tracer.start_as_current_span("random-word") as span:
        word = _repository.get_random()

        if word is None:
            return jsonify({"error": "No words available"}), 503

        span.set_attribute("word.language", word["language"])
        span.set_attribute("word.value", word["word"])

    try:
        telemetry.word_counter.add(1, {"language": word["language"]})
    except Exception:  # noqa: BLE001
        pass

    return jsonify(word), 200


@words_bp.route("/words", methods=["GET"])
def words_by_language() -> tuple[Response, int]:
    """Return untranslatable words, optionally filtered by language.

    Query Parameters
    ----------------
    language : str, optional
        ISO 639-1 language code (e.g. ``"pt"``). Omit to return all words.

    Returns
    -------
    flask.Response
        200 — a list of matching word objects (may be the full dataset).
        404 — if ``language`` was supplied but no words match it.
    """
    language = request.args.get("language", "").strip()

    with telemetry.tracer.start_as_current_span("word-by-language") as span:
        span.set_attribute("language", language or "all")

        if not language:
            words = _repository.get_all()
            try:
                telemetry.word_counter.add(len(words), {"language": "all"})
            except Exception:  # noqa: BLE001
                pass
            return jsonify(words), 200

        words = _repository.get_by_language(language)

    if not words:
        return jsonify({"error": f"No words found for language '{language}'"}), 404

    try:
        telemetry.word_counter.add(len(words), {"language": language})
    except Exception:  # noqa: BLE001
        pass

    return jsonify(words), 200
