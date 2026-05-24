"""
Data access layer for untranslatable words.

WordsRepository wraps the cached file loader with domain-level query
operations. All data is loaded from disk once and kept in memory for the
lifetime of the process, so every method here is effectively O(n) in-memory
filtering — suitable for the current dataset size.
"""

import random
from typing import List, Optional

from data.file_reader import read_json_from_file


class WordsRepository:
    """Provides read access to the untranslatable-word dataset."""

    def get_all(self) -> List[dict]:
        """Return every word in the dataset."""
        return read_json_from_file()

    def get_by_language(self, language: str) -> List[dict]:
        """Return all words whose language code matches *language* exactly."""
        return [w for w in self.get_all() if w["language"] == language]

    def get_random(self) -> Optional[dict]:
        """Return a single word chosen uniformly at random.

        Returns ``None`` only when the dataset is completely empty —
        callers should treat that as a 503.
        """
        words = self.get_all()
        return random.choice(words) if words else None
