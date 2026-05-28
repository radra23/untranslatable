"""Shared type definitions for the data layer."""

from __future__ import annotations

from typing import TypedDict


class Word(TypedDict):
    """A single untranslatable word entry from the JSON dataset.

    Attributes
    ----------
    language : str
        ISO 639-1 language code (e.g. ``"pt"``, ``"de"``).
    word : str
        The untranslatable word or phrase in the original language.
    meaning : str
        An English explanation of the concept the word captures.
    """

    language: str
    word: str
    meaning: str
