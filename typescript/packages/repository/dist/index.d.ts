export interface Word {
    language: string;
    word: string;
    meaning: string;
}
export declare class WordsRepository {
    getAllWords(language?: string): Word[];
    getRandomWord(): Word;
}
//# sourceMappingURL=index.d.ts.map