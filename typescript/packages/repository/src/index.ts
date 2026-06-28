import rawData from './data.json';

export interface Word {
  language: string;
  word: string;
  meaning: string;
}

const defaultWords: Word[] = rawData as Word[];

export class WordsRepository {
  private readonly words: Word[];

  constructor(words: Word[] = defaultWords) {
    this.words = words;
  }

  getAllWords(language?: string): readonly Word[] {
    if (language === undefined) return this.words;
    return this.words.filter(w => w.language === language);
  }

  getRandomWord(): Word {
    if (this.words.length === 0) throw new Error('WordsRepository: no words available');
    return this.words[Math.floor(Math.random() * this.words.length)];
  }
}
