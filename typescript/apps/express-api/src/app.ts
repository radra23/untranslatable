import express, { Request, Response } from 'express';
import { SpanStatusCode } from '@opentelemetry/api';
import { tracer, wordCounter, logger } from '@untranslatable/telemetry';
import { WordsRepository } from '@untranslatable/repository';

export function createApp(repo: WordsRepository = new WordsRepository()): express.Application {
  const app = express();
  // No body parser: all routes are read-only GET endpoints

  app.get('/', (_req: Request, res: Response) => {
    res.json({ message: 'Welcome to the Untranslatable API (Express)' });
  });

  app.get('/healthz', (_req: Request, res: Response) => {
    res.json({ status: 'ok' });
  });

  app.get('/words', (req: Request, res: Response) => {
    tracer.startActiveSpan('words.list', span => {
      try {
        const language = typeof req.query.language === 'string' ? req.query.language : undefined;
        const langLabel = language ?? 'all';
        const words = repo.getAllWords(language);
        span.setAttributes({ 'words.count': words.length, ...(language !== undefined ? { 'words.language': language } : {}) });
        // Metric labels must stay bounded: only a language that matched the dataset
        // becomes a label; any other client-supplied value collapses to 'other'.
        const metricLabel = language === undefined || words.length > 0 ? langLabel : 'other';
        wordCounter.add(words.length, { language: metricLabel });
        logger.info('Words listed', { count: words.length, language: langLabel });
        res.json(words);
      } catch (err) {
        span.recordException(err instanceof Error ? err : new Error(String(err)));
        span.setStatus({ code: SpanStatusCode.ERROR });
        logger.error('Failed to list words', { error: String(err) });
        res.status(500).json({ error: 'Internal server error' });
      } finally {
        span.end();
      }
    });
  });

  app.get('/words/random', (_req: Request, res: Response) => {
    tracer.startActiveSpan('words.random', span => {
      try {
        const word = repo.getRandomWord();
        span.setAttributes({ 'word.language': word.language, 'word.word': word.word });
        span.addEvent('word.selected');
        wordCounter.add(1, { language: word.language });
        logger.info('Random word served', { language: word.language });
        res.json(word);
      } catch (err) {
        span.recordException(err instanceof Error ? err : new Error(String(err)));
        span.setStatus({ code: SpanStatusCode.ERROR });
        logger.error('Failed to fetch random word', { error: String(err) });
        res.status(500).json({ error: 'Internal server error' });
      } finally {
        span.end();
      }
    });
  });

  return app;
}
