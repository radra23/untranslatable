"""Unit tests for WordsRepository."""

from __future__ import annotations

from unittest.mock import patch

import pytest

from data.repository import WordsRepository

# ---------------------------------------------------------------------------
# Helpers
# ---------------------------------------------------------------------------

_FAKE_WORDS = [
    {"language": "pt", "word": "saudade", "meaning": "a melancholic longing"},
    {"language": "pt", "word": "desenrascanço", "meaning": "creative improvisation"},
    {"language": "de", "word": "Fernweh", "meaning": "an ache for distant places"},
    {"language": "de", "word": "Weltschmerz", "meaning": "deep world-weariness"},
    {"language": "ja", "word": "komorebi", "meaning": "sunlight through leaves"},
]


@pytest.fixture()
def repo():
    """Return a WordsRepository with a patched data loader."""
    with patch("data.repository.read_json_from_file", return_value=_FAKE_WORDS):
        yield WordsRepository()


# ---------------------------------------------------------------------------
# get_all
# ---------------------------------------------------------------------------


def test_get_all_returns_every_word(repo):
    result = repo.get_all()
    assert len(result) == len(_FAKE_WORDS)


def test_get_all_returns_list_of_dicts(repo):
    result = repo.get_all()
    assert all(isinstance(w, dict) for w in result)


def test_get_all_contains_expected_keys(repo):
    for word in repo.get_all():
        assert {"language", "word", "meaning"} <= word.keys()


# ---------------------------------------------------------------------------
# get_by_language
# ---------------------------------------------------------------------------


def test_get_by_language_returns_matching_words(repo):
    result = repo.get_by_language("pt")
    assert len(result) == 2
    assert all(w["language"] == "pt" for w in result)


def test_get_by_language_returns_empty_for_unknown_code(repo):
    result = repo.get_by_language("xx")
    assert result == []


def test_get_by_language_is_case_sensitive(repo):
    # Language codes are lower-case; "PT" should not match "pt".
    assert repo.get_by_language("PT") == []


def test_get_by_language_single_word_language(repo):
    result = repo.get_by_language("ja")
    assert len(result) == 1
    assert result[0]["word"] == "komorebi"


# ---------------------------------------------------------------------------
# get_random
# ---------------------------------------------------------------------------


def test_get_random_returns_a_word(repo):
    word = repo.get_random()
    assert word is not None
    assert {"language", "word", "meaning"} <= word.keys()


def test_get_random_returns_word_from_dataset(repo):
    word = repo.get_random()
    assert word in _FAKE_WORDS


def test_get_random_returns_none_when_dataset_empty():
    with patch("data.repository.read_json_from_file", return_value=[]):
        empty_repo = WordsRepository()
        assert empty_repo.get_random() is None


def test_get_random_is_non_deterministic(repo):
    # With 5 items, drawing 20 times should not always return the same word.
    results = {repo.get_random()["word"] for _ in range(20)}
    assert len(results) > 1
