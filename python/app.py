"""Application factory for the Untranslatable Flask API.

Flask's development server discovers ``create_app`` automatically when
``FLASK_APP`` points at this module (set in ``.flaskenv``).
"""

from __future__ import annotations

import os

from flask import Flask
from opentelemetry.instrumentation.flask import FlaskInstrumentor
from opentelemetry.instrumentation.requests import RequestsInstrumentor

import telemetry  # noqa: F401  — side-effect import: configures OTel providers
from api.words import words_bp


def create_app() -> Flask:
    """Create and return a configured Flask application instance.

    Initialises OTel auto-instrumentation, registers all blueprints, and
    attaches the root welcome route before returning the app.

    Returns
    -------
    Flask
        A fully configured Flask application ready to serve requests.
    """
    app = Flask(__name__)

    # Auto-instrumentation must be applied after the OTel providers in
    # telemetry.py are set up, and before the first request is handled.
    FlaskInstrumentor().instrument_app(app)
    RequestsInstrumentor().instrument()

    @app.route("/")
    @app.route("/home")
    @app.route("/index")
    def welcome() -> tuple[str, int]:
        """Return a plain-text welcome message."""
        return "Welcome to untranslatable!", 200

    app.register_blueprint(words_bp)

    return app


if __name__ == "__main__":
    debug = os.environ.get("FLASK_DEBUG", "0") == "1"
    create_app().run(debug=debug, use_reloader=False)
