"""Integration tests for Flask routes."""

from __future__ import annotations

from unittest.mock import patch

# ---------------------------------------------------------------------------
# Helpers
# ---------------------------------------------------------------------------

_FAKE_WORDS = [
    {"language": "pt", "word": "saudade", "meaning": "a melancholic longing"},
    {"language": "pt", "word": "desenrascanço", "meaning": "creative improvisation"},
    {"language": "de", "word": "Fernweh", "meaning": "an ache for distant places"},
]

_PT_WORDS = [w for w in _FAKE_WORDS if w["language"] == "pt"]


# ---------------------------------------------------------------------------
# Welcome / home routes
# ---------------------------------------------------------------------------


def test_root_returns_200(client):
    response = client.get("/")
    assert response.status_code == 200


def test_home_alias_returns_200(client):
    assert client.get("/home").status_code == 200


def test_index_alias_returns_200(client):
    assert client.get("/index").status_code == 200


def test_welcome_message(client):
    response = client.get("/")
    assert b"Welcome" in response.data


# ---------------------------------------------------------------------------
# GET /healthz
# ---------------------------------------------------------------------------


def test_healthz_returns_200(client):
    assert client.get("/healthz").status_code == 200


def test_healthz_response_is_json(client):
    assert client.get("/healthz").is_json


def test_healthz_body_contains_healthy_status(client):
    data = client.get("/healthz").get_json()
    assert data == {"status": "healthy"}


# ---------------------------------------------------------------------------
# GET /words/random
# ---------------------------------------------------------------------------


def test_random_word_returns_200(client):
    with patch("data.repository.read_json_from_file", return_value=_FAKE_WORDS):
        response = client.get("/words/random")
    assert response.status_code == 200


def test_random_word_response_is_json(client):
    with patch("data.repository.read_json_from_file", return_value=_FAKE_WORDS):
        response = client.get("/words/random")
    assert response.is_json


def test_random_word_has_expected_keys(client):
    with patch("data.repository.read_json_from_file", return_value=_FAKE_WORDS):
        data = client.get("/words/random").get_json()
    assert {"language", "word", "meaning"} <= data.keys()


def test_random_word_value_is_from_dataset(client):
    with patch("data.repository.read_json_from_file", return_value=_FAKE_WORDS):
        data = client.get("/words/random").get_json()
    words = [w["word"] for w in _FAKE_WORDS]
    assert data["word"] in words


def test_random_word_returns_503_when_empty(client):
    with patch("data.repository.read_json_from_file", return_value=[]):
        response = client.get("/words/random")
    assert response.status_code == 503


def test_random_word_503_body_contains_error_key(client):
    with patch("data.repository.read_json_from_file", return_value=[]):
        data = client.get("/words/random").get_json()
    assert "error" in data


# ---------------------------------------------------------------------------
# GET /words
# ---------------------------------------------------------------------------


def test_words_no_filter_returns_200(client):
    with patch("data.repository.read_json_from_file", return_value=_FAKE_WORDS):
        response = client.get("/words")
    assert response.status_code == 200


def test_words_no_filter_returns_full_list(client):
    with patch("data.repository.read_json_from_file", return_value=_FAKE_WORDS):
        data = client.get("/words").get_json()
    assert len(data) == len(_FAKE_WORDS)


def test_words_filter_by_language_returns_200(client):
    with patch("data.repository.read_json_from_file", return_value=_FAKE_WORDS):
        response = client.get("/words?language=pt")
    assert response.status_code == 200


def test_words_filter_by_language_returns_only_matching(client):
    with patch("data.repository.read_json_from_file", return_value=_FAKE_WORDS):
        data = client.get("/words?language=pt").get_json()
    assert len(data) == len(_PT_WORDS)
    assert all(w["language"] == "pt" for w in data)


def test_words_unknown_language_returns_404(client):
    with patch("data.repository.read_json_from_file", return_value=_FAKE_WORDS):
        response = client.get("/words?language=xx")
    assert response.status_code == 404


def test_words_unknown_language_body_has_error_key(client):
    with patch("data.repository.read_json_from_file", return_value=_FAKE_WORDS):
        data = client.get("/words?language=xx").get_json()
    assert "error" in data


def test_words_language_param_whitespace_trimmed(client):
    # Leading/trailing spaces in the query string should be stripped.
    with patch("data.repository.read_json_from_file", return_value=_FAKE_WORDS):
        response = client.get("/words?language= pt ")
    assert response.status_code == 200


def test_words_empty_language_param_treated_as_no_filter(client):
    # An empty language= query string must return all words, not a 404.
    with patch("data.repository.read_json_from_file", return_value=_FAKE_WORDS):
        data = client.get("/words?language=").get_json()
    assert len(data) == len(_FAKE_WORDS)
