"use strict";
var __importDefault = (this && this.__importDefault) || function (mod) {
    return (mod && mod.__esModule) ? mod : { "default": mod };
};
Object.defineProperty(exports, "__esModule", { value: true });
exports.WordsRepository = void 0;
const data_json_1 = __importDefault(require("./data.json"));
const words = data_json_1.default;
class WordsRepository {
    getAllWords(language) {
        if (language === undefined)
            return words;
        return words.filter(w => w.language === language);
    }
    getRandomWord() {
        return words[Math.floor(Math.random() * words.length)];
    }
}
exports.WordsRepository = WordsRepository;
//# sourceMappingURL=index.js.map