import express, { Request, Response } from 'express';
import { SpanStatusCode } from '@opentelemetry/api';
import { tracer, wordCounter, logger } from '@untranslatable/telemetry';
import { WordsRepository } from '@untranslatable/repository';

export function createApp(repo: WordsRepository = new WordsRepository()): express.Application {
  const app = express();
  app.use(express.json());

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
        const words = repo.getAllWords(language);
        span.setAttributes({ 'words.count': words.length });
        if (language) span.setAttributes({ 'words.language': language });
        wordCounter.add(words.length, { language: language ?? 'all' });
        logger.info('Words listed', { count: words.length, language: language ?? 'all' });
        res.json(words);
      } catch (err) {
        span.recordException(err as Error);
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
        span.recordException(err as Error);
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
