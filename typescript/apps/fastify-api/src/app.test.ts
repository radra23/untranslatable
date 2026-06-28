import { describe, it, expect, afterEach } from 'vitest';
import { createApp } from './app';
import { WordsRepository } from '@untranslatable/repository';

describe('Fastify API routes', () => {
  let app: ReturnType<typeof createApp>;

  afterEach(async () => {
    await app.close();
  });

  it('GET / returns 200 with message', async () => {
    app = createApp();
    const res = await app.inject({ method: 'GET', url: '/' });
    expect(res.statusCode).toBe(200);
    expect(JSON.parse(res.body)).toHaveProperty('message');
  });

  it('GET /healthz returns { status: "ok" }', async () => {
    app = createApp();
    const res = await app.inject({ method: 'GET', url: '/healthz' });
    expect(res.statusCode).toBe(200);
    expect(JSON.parse(res.body)).toEqual({ status: 'ok' });
  });

  it('GET /words returns array of word objects', async () => {
    app = createApp();
    const res = await app.inject({ method: 'GET', url: '/words' });
    expect(res.statusCode).toBe(200);
    const body = JSON.parse(res.body);
    expect(Array.isArray(body)).toBe(true);
    expect(body.length).toBeGreaterThan(0);
    expect(body[0]).toHaveProperty('language');
    expect(body[0]).toHaveProperty('word');
    expect(body[0]).toHaveProperty('meaning');
  });

  it('GET /words?language=da filters by language', async () => {
    app = createApp();
    const res = await app.inject({ method: 'GET', url: '/words?language=da' });
    expect(res.statusCode).toBe(200);
    const body = JSON.parse(res.body);
    expect(body.length).toBeGreaterThan(0);
    expect(body.every((w: { language: string }) => w.language === 'da')).toBe(true);
  });

  it('GET /words?language=xx returns empty array', async () => {
    app = createApp();
    const res = await app.inject({ method: 'GET', url: '/words?language=xx' });
    expect(res.statusCode).toBe(200);
    expect(JSON.parse(res.body)).toEqual([]);
  });

  it('GET /words/random returns a single word', async () => {
    app = createApp();
    const res = await app.inject({ method: 'GET', url: '/words/random' });
    expect(res.statusCode).toBe(200);
    const body = JSON.parse(res.body);
    expect(body).toHaveProperty('language');
    expect(body).toHaveProperty('word');
    expect(body).toHaveProperty('meaning');
  });
});

describe('Fastify API error handling', () => {
  const throwingRepo = {
    getAllWords: (): never => { throw new Error('db error'); },
    getRandomWord: (): never => { throw new Error('db error'); },
  } as unknown as WordsRepository;

  it('GET /words returns 500 when repo throws', async () => {
    const app = createApp(throwingRepo);
    const res = await app.inject({ method: 'GET', url: '/words' });
    await app.close();
    expect(res.statusCode).toBe(500);
    expect(JSON.parse(res.body)).toEqual({ error: 'Internal server error' });
  });

  it('GET /words/random returns 500 when repo throws', async () => {
    const app = createApp(throwingRepo);
    const res = await app.inject({ method: 'GET', url: '/words/random' });
    await app.close();
    expect(res.statusCode).toBe(500);
    expect(JSON.parse(res.body)).toEqual({ error: 'Internal server error' });
  });
});
