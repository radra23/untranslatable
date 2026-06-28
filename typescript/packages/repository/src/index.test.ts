import { describe, it, expect } from 'vitest';
import { WordsRepository } from './index';

describe('WordsRepository', () => {
  const repo = new WordsRepository();

  it('getAllWords() returns all words', () => {
    const words = repo.getAllWords();
    expect(words.length).toBeGreaterThan(0);
    expect(words[0]).toHaveProperty('language');
    expect(words[0]).toHaveProperty('word');
    expect(words[0]).toHaveProperty('meaning');
  });

  it('getAllWords(language) filters by language', () => {
    const all = repo.getAllWords();
    const lang = all[0].language;
    const filtered = repo.getAllWords(lang);
    expect(filtered.length).toBeGreaterThan(0);
    expect(filtered.every(w => w.language === lang)).toBe(true);
    expect(filtered.length).toBeLessThan(all.length);
  });

  it('getAllWords() with unknown language returns empty array', () => {
    expect(repo.getAllWords('xx')).toEqual([]);
  });

  it('getRandomWord() returns a valid word', () => {
    const word = repo.getRandomWord();
    expect(word).toHaveProperty('language');
    expect(word).toHaveProperty('word');
    expect(word).toHaveProperty('meaning');
    expect(typeof word.language).toBe('string');
  });

  it('getRandomWord() returns different words over multiple calls', () => {
    const results = new Set(Array.from({ length: 20 }, () => repo.getRandomWord().word));
    expect(results.size).toBeGreaterThan(1);
  });

  it('getRandomWord() throws when the repository is empty', () => {
    const empty = new WordsRepository([]);
    expect(() => empty.getRandomWord()).toThrowError('WordsRepository: no words available');
  });
});
