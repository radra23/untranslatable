"use strict";
var __importDefault = (this && this.__importDefault) || function (mod) {
    return (mod && mod.__esModule) ? mod : { "default": mod };
};
Object.defineProperty(exports, "__esModule", { value: true });
exports.createApp = createApp;
const express_1 = __importDefault(require("express"));
const api_1 = require("@opentelemetry/api");
const telemetry_1 = require("@untranslatable/telemetry");
const repository_1 = require("@untranslatable/repository");
function createApp(repo = new repository_1.WordsRepository()) {
    const app = (0, express_1.default)();
    app.use(express_1.default.json());
    app.get('/', (_req, res) => {
        res.json({ message: 'Welcome to the Untranslatable API (Express)' });
    });
    app.get('/healthz', (_req, res) => {
        res.json({ status: 'ok' });
    });
    app.get('/words', (req, res) => {
        telemetry_1.tracer.startActiveSpan('words.list', span => {
            try {
                const language = typeof req.query.language === 'string' ? req.query.language : undefined;
                const words = repo.getAllWords(language);
                span.setAttributes({ 'words.count': words.length });
                if (language)
                    span.setAttributes({ 'words.language': language });
                telemetry_1.wordCounter.add(words.length, { language: language ?? 'all' });
                telemetry_1.logger.info('Words listed', { count: words.length, language: language ?? 'all' });
                res.json(words);
            }
            catch (err) {
                span.recordException(err);
                span.setStatus({ code: api_1.SpanStatusCode.ERROR });
                telemetry_1.logger.error('Failed to list words', { error: String(err) });
                res.status(500).json({ error: 'Internal server error' });
            }
            finally {
                span.end();
            }
        });
    });
    app.get('/words/random', (_req, res) => {
        telemetry_1.tracer.startActiveSpan('words.random', span => {
            try {
                const word = repo.getRandomWord();
                span.setAttributes({ 'word.language': word.language, 'word.word': word.word });
                span.addEvent('word.selected');
                telemetry_1.wordCounter.add(1, { language: word.language });
                telemetry_1.logger.info('Random word served', { language: word.language });
                res.json(word);
            }
            catch (err) {
                span.recordException(err);
                span.setStatus({ code: api_1.SpanStatusCode.ERROR });
                telemetry_1.logger.error('Failed to fetch random word', { error: String(err) });
                res.status(500).json({ error: 'Internal server error' });
            }
            finally {
                span.end();
            }
        });
    });
    return app;
}
//# sourceMappingURL=app.js.map