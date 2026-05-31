"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
const vitest_1 = require("vitest");
const index_1 = require("./index");
(0, vitest_1.describe)('WordsRepository', () => {
    const repo = new index_1.WordsRepository();
    (0, vitest_1.it)('getAllWords() returns all words', () => {
        const words = repo.getAllWords();
        (0, vitest_1.expect)(words.length).toBeGreaterThan(0);
        (0, vitest_1.expect)(words[0]).toHaveProperty('language');
        (0, vitest_1.expect)(words[0]).toHaveProperty('word');
        (0, vitest_1.expect)(words[0]).toHaveProperty('meaning');
    });
    (0, vitest_1.it)('getAllWords(language) filters by language', () => {
        const all = repo.getAllWords();
        const lang = all[0].language;
        const filtered = repo.getAllWords(lang);
        (0, vitest_1.expect)(filtered.length).toBeGreaterThan(0);
        (0, vitest_1.expect)(filtered.every(w => w.language === lang)).toBe(true);
        (0, vitest_1.expect)(filtered.length).toBeLessThan(all.length);
    });
    (0, vitest_1.it)('getAllWords() with unknown language returns empty array', () => {
        (0, vitest_1.expect)(repo.getAllWords('xx')).toEqual([]);
    });
    (0, vitest_1.it)('getRandomWord() returns a valid word', () => {
        const word = repo.getRandomWord();
        (0, vitest_1.expect)(word).toHaveProperty('language');
        (0, vitest_1.expect)(word).toHaveProperty('word');
        (0, vitest_1.expect)(word).toHaveProperty('meaning');
        (0, vitest_1.expect)(typeof word.language).toBe('string');
    });
    (0, vitest_1.it)('getRandomWord() returns different words over multiple calls', () => {
        const results = new Set(Array.from({ length: 20 }, () => repo.getRandomWord().word));
        (0, vitest_1.expect)(results.size).toBeGreaterThan(1);
    });
});
//# sourceMappingURL=index.test.js.map