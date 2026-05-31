"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
const vitest_1 = require("vitest");
const app_1 = require("./app");
(0, vitest_1.describe)('Fastify API routes', () => {
    let app;
    (0, vitest_1.afterEach)(async () => {
        await app.close();
    });
    (0, vitest_1.it)('GET / returns 200 with message', async () => {
        app = (0, app_1.createApp)();
        const res = await app.inject({ method: 'GET', url: '/' });
        (0, vitest_1.expect)(res.statusCode).toBe(200);
        (0, vitest_1.expect)(JSON.parse(res.body)).toHaveProperty('message');
    });
    (0, vitest_1.it)('GET /healthz returns { status: "ok" }', async () => {
        app = (0, app_1.createApp)();
        const res = await app.inject({ method: 'GET', url: '/healthz' });
        (0, vitest_1.expect)(res.statusCode).toBe(200);
        (0, vitest_1.expect)(JSON.parse(res.body)).toEqual({ status: 'ok' });
    });
    (0, vitest_1.it)('GET /words returns array of word objects', async () => {
        app = (0, app_1.createApp)();
        const res = await app.inject({ method: 'GET', url: '/words' });
        (0, vitest_1.expect)(res.statusCode).toBe(200);
        const body = JSON.parse(res.body);
        (0, vitest_1.expect)(Array.isArray(body)).toBe(true);
        (0, vitest_1.expect)(body.length).toBeGreaterThan(0);
        (0, vitest_1.expect)(body[0]).toHaveProperty('language');
        (0, vitest_1.expect)(body[0]).toHaveProperty('word');
        (0, vitest_1.expect)(body[0]).toHaveProperty('meaning');
    });
    (0, vitest_1.it)('GET /words?language=da filters by language', async () => {
        app = (0, app_1.createApp)();
        const res = await app.inject({ method: 'GET', url: '/words?language=da' });
        (0, vitest_1.expect)(res.statusCode).toBe(200);
        const body = JSON.parse(res.body);
        (0, vitest_1.expect)(body.length).toBeGreaterThan(0);
        (0, vitest_1.expect)(body.every((w) => w.language === 'da')).toBe(true);
    });
    (0, vitest_1.it)('GET /words?language=xx returns empty array', async () => {
        app = (0, app_1.createApp)();
        const res = await app.inject({ method: 'GET', url: '/words?language=xx' });
        (0, vitest_1.expect)(res.statusCode).toBe(200);
        (0, vitest_1.expect)(JSON.parse(res.body)).toEqual([]);
    });
    (0, vitest_1.it)('GET /words/random returns a single word', async () => {
        app = (0, app_1.createApp)();
        const res = await app.inject({ method: 'GET', url: '/words/random' });
        (0, vitest_1.expect)(res.statusCode).toBe(200);
        const body = JSON.parse(res.body);
        (0, vitest_1.expect)(body).toHaveProperty('language');
        (0, vitest_1.expect)(body).toHaveProperty('word');
        (0, vitest_1.expect)(body).toHaveProperty('meaning');
    });
});
//# sourceMappingURL=app.test.js.map