"use strict";
var __importDefault = (this && this.__importDefault) || function (mod) {
    return (mod && mod.__esModule) ? mod : { "default": mod };
};
Object.defineProperty(exports, "__esModule", { value: true });
exports.createApp = createApp;
const fastify_1 = __importDefault(require("fastify"));
const api_1 = require("@opentelemetry/api");
const telemetry_1 = require("@untranslatable/telemetry");
const repository_1 = require("@untranslatable/repository");
function createApp(repo = new repository_1.WordsRepository()) {
    const app = (0, fastify_1.default)({ logger: false });
    app.get('/', async (_request, _reply) => {
        return { message: 'Welcome to the Untranslatable API (Fastify)' };
    });
    app.get('/healthz', async (_request, _reply) => {
        return { status: 'ok' };
    });
    app.get('/words', async (request, reply) => {
        return telemetry_1.tracer.startActiveSpan('words.list', span => {
            try {
                const language = request.query.language;
                const words = repo.getAllWords(language);
                span.setAttributes({ 'words.count': words.length });
                if (language)
                    span.setAttributes({ 'words.language': language });
                telemetry_1.wordCounter.add(words.length, { language: language ?? 'all' });
                telemetry_1.logger.info('Words listed', { count: words.length, language: language ?? 'all' });
                return words;
            }
            catch (err) {
                span.recordException(err);
                span.setStatus({ code: api_1.SpanStatusCode.ERROR });
                telemetry_1.logger.error('Failed to list words', { error: String(err) });
                reply.status(500);
                return { error: 'Internal server error' };
            }
            finally {
                span.end();
            }
        });
    });
    app.get('/words/random', async (_request, reply) => {
        return telemetry_1.tracer.startActiveSpan('words.random', span => {
            try {
                const word = repo.getRandomWord();
                span.setAttributes({ 'word.language': word.language, 'word.word': word.word });
                span.addEvent('word.selected');
                telemetry_1.wordCounter.add(1, { language: word.language });
                telemetry_1.logger.info('Random word served', { language: word.language });
                return word;
            }
            catch (err) {
                span.recordException(err);
                span.setStatus({ code: api_1.SpanStatusCode.ERROR });
                telemetry_1.logger.error('Failed to fetch random word', { error: String(err) });
                reply.status(500);
                return { error: 'Internal server error' };
            }
            finally {
                span.end();
            }
        });
    });
    return app;
}
//# sourceMappingURL=app.js.map