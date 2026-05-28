"""Data access layer for untranslatable words.

``WordsRepository`` wraps the cached file loader with domain-level query
operations. All data is loaded from disk once and kept in memory for the
lifetime of the process, so every method is effectively O(n) in-memory
filtering — suitable for the current dataset size.
"""

from __future__ import annotations

import random

from data.file_reader import read_json_from_file
from data.types import Word


class WordsRepository:
    """Read-only access to the untranslatable-word dataset.

    All data is loaded from disk on first access and cached in memory.
    The class is stateless; a single shared instance is safe across threads.
    """

    def get_all(self) -> list[Word]:
        """Return every word in the dataset.

        Returns
        -------
        list of Word
            All words, each with ``language``, ``word``, and ``meaning`` keys.
        """
        return read_json_from_file()

    def get_by_language(self, language: str) -> list[Word]:
        """Return all words for a given language code.

        Parameters
        ----------
        language : str
            ISO 639-1 language code (e.g. ``"pt"``, ``"de"``).

        Returns
        -------
        list of Word
            Matching words, or an empty list if the code is not found.
        """
        return [w for w in self.get_all() if w["language"] == language]

    def get_random(self) -> Word | None:
        """Return a single word chosen uniformly at random.

        Returns
        -------
        Word or None
            A word dict with ``language``, ``word``, and ``meaning`` keys,
            or ``None`` if the dataset is empty (callers should treat this
            as a 503 condition).
        """
        words = self.get_all()
        return random.choice(words) if words else None
