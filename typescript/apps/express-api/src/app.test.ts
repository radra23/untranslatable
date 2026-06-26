import { describe, it, expect } from 'vitest';
import request from 'supertest';
import { createApp } from './app';
import { WordsRepository } from '@untranslatable/repository';

const app = createApp();

describe('Express API routes', () => {
  it('GET / returns 200 with message', async () => {
    const res = await request(app).get('/');
    expect(res.status).toBe(200);
    expect(res.body).toHaveProperty('message');
  });

  it('GET /healthz returns { status: "ok" }', async () => {
    const res = await request(app).get('/healthz');
    expect(res.status).toBe(200);
    expect(res.body).toEqual({ status: 'ok' });
  });

  it('GET /words returns array of word objects', async () => {
    const res = await request(app).get('/words');
    expect(res.status).toBe(200);
    expect(Array.isArray(res.body)).toBe(true);
    expect(res.body.length).toBeGreaterThan(0);
    expect(res.body[0]).toHaveProperty('language');
    expect(res.body[0]).toHaveProperty('word');
    expect(res.body[0]).toHaveProperty('meaning');
  });

  it('GET /words?language=da filters by language', async () => {
    const res = await request(app).get('/words?language=da');
    expect(res.status).toBe(200);
    expect(Array.isArray(res.body)).toBe(true);
    expect(res.body.length).toBeGreaterThan(0);
    expect(res.body.every((w: { language: string }) => w.language === 'da')).toBe(true);
  });

  it('GET /words?language=xx returns empty array', async () => {
    const res = await request(app).get('/words?language=xx');
    expect(res.status).toBe(200);
    expect(res.body).toEqual([]);
  });

  it('GET /words/random returns a single word', async () => {
    const res = await request(app).get('/words/random');
    expect(res.status).toBe(200);
    expect(res.body).toHaveProperty('language');
    expect(res.body).toHaveProperty('word');
    expect(res.body).toHaveProperty('meaning');
  });
});

describe('Express API error handling', () => {
  const throwingRepo = {
    getAllWords: (): never => { throw new Error('db error'); },
    getRandomWord: (): never => { throw new Error('db error'); },
  } as unknown as WordsRepository;

  it('GET /words returns 500 when repo throws', async () => {
    const app = createApp(throwingRepo);
    const res = await request(app).get('/words');
    expect(res.status).toBe(500);
    expect(res.body).toEqual({ error: 'Internal server error' });
  });

  it('GET /words/random returns 500 when repo throws', async () => {
    const app = createApp(throwingRepo);
    const res = await request(app).get('/words/random');
    expect(res.status).toBe(500);
    expect(res.body).toEqual({ error: 'Internal server error' });
  });
});
