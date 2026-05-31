import rawData from './data.json';

export interface Word {
  language: string;
  word: string;
  meaning: string;
}

const words: Word[] = rawData as Word[];

export class WordsRepository {
  getAllWords(language?: string): Word[] {
    if (language === undefined) return words;
    return words.filter(w => w.language === language);
  }

  getRandomWord(): Word {
    return words[Math.floor(Math.random() * words.length)];
  }
}
