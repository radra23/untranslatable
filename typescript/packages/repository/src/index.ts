import rawData from './data.json';

export interface Word {
  language: string;
  word: string;
  meaning: string;
}

const words: Word[] = rawData as Word[];

export class WordsRepository {
  getAllWords(language?: string): readonly Word[] {
    if (language === undefined) return words;
    return words.filter(w => w.language === language);
  }

  getRandomWord(): Word {
    if (words.length === 0) throw new Error('WordsRepository: no words available');
    return words[Math.floor(Math.random() * words.length)];
  }
}
