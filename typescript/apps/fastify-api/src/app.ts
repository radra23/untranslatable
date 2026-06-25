import Fastify, { FastifyInstance, FastifyRequest, FastifyReply } from 'fastify';
import { SpanStatusCode } from '@opentelemetry/api';
import { tracer, wordCounter, logger } from '@untranslatable/telemetry';
import { WordsRepository } from '@untranslatable/repository';

export function createApp(repo: WordsRepository = new WordsRepository()): FastifyInstance {
  const app = Fastify({ logger: false });

  app.get('/', async (_request: FastifyRequest, _reply: FastifyReply) => {
    return { message: 'Welcome to the Untranslatable API (Fastify)' };
  });

  app.get('/healthz', async (_request: FastifyRequest, _reply: FastifyReply) => {
    return { status: 'ok' };
  });

  app.get<{ Querystring: { language?: string } }>('/words', async (request, reply) => {
    return tracer.startActiveSpan('words.list', span => {
      try {
        const language = request.query.language;
        const langLabel = language ?? 'all';
        const words = repo.getAllWords(language);
        span.setAttributes({ 'words.count': words.length, ...(language !== undefined ? { 'words.language': language } : {}) });
        wordCounter.add(words.length, { language: langLabel });
        logger.info('Words listed', { count: words.length, language: langLabel });
        return words;
      } catch (err) {
        span.recordException(err as Error);
        span.setStatus({ code: SpanStatusCode.ERROR });
        logger.error('Failed to list words', { error: String(err) });
        reply.status(500);
        return { error: 'Internal server error' };
      } finally {
        span.end();
      }
    });
  });

  app.get('/words/random', async (_request: FastifyRequest, reply: FastifyReply) => {
    return tracer.startActiveSpan('words.random', span => {
      try {
        const word = repo.getRandomWord();
        span.setAttributes({ 'word.language': word.language, 'word.word': word.word });
        span.addEvent('word.selected');
        wordCounter.add(1, { language: word.language });
        logger.info('Random word served', { language: word.language });
        return word;
      } catch (err) {
        span.recordException(err as Error);
        span.setStatus({ code: SpanStatusCode.ERROR });
        logger.error('Failed to fetch random word', { error: String(err) });
        reply.status(500);
        return { error: 'Internal server error' };
      } finally {
        span.end();
      }
    });
  });

  return app;
}
