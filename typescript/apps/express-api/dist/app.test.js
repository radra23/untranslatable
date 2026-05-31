"use strict";
var __importDefault = (this && this.__importDefault) || function (mod) {
    return (mod && mod.__esModule) ? mod : { "default": mod };
};
Object.defineProperty(exports, "__esModule", { value: true });
const vitest_1 = require("vitest");
const supertest_1 = __importDefault(require("supertest"));
const app_1 = require("./app");
const app = (0, app_1.createApp)();
(0, vitest_1.describe)('Express API routes', () => {
    (0, vitest_1.it)('GET / returns 200 with message', async () => {
        const res = await (0, supertest_1.default)(app).get('/');
        (0, vitest_1.expect)(res.status).toBe(200);
        (0, vitest_1.expect)(res.body).toHaveProperty('message');
    });
    (0, vitest_1.it)('GET /healthz returns { status: "ok" }', async () => {
        const res = await (0, supertest_1.default)(app).get('/healthz');
        (0, vitest_1.expect)(res.status).toBe(200);
        (0, vitest_1.expect)(res.body).toEqual({ status: 'ok' });
    });
    (0, vitest_1.it)('GET /words returns array of word objects', async () => {
        const res = await (0, supertest_1.default)(app).get('/words');
        (0, vitest_1.expect)(res.status).toBe(200);
        (0, vitest_1.expect)(Array.isArray(res.body)).toBe(true);
        (0, vitest_1.expect)(res.body.length).toBeGreaterThan(0);
        (0, vitest_1.expect)(res.body[0]).toHaveProperty('language');
        (0, vitest_1.expect)(res.body[0]).toHaveProperty('word');
        (0, vitest_1.expect)(res.body[0]).toHaveProperty('meaning');
    });
    (0, vitest_1.it)('GET /words?language=da filters by language', async () => {
        const res = await (0, supertest_1.default)(app).get('/words?language=da');
        (0, vitest_1.expect)(res.status).toBe(200);
        (0, vitest_1.expect)(Array.isArray(res.body)).toBe(true);
        (0, vitest_1.expect)(res.body.length).toBeGreaterThan(0);
        (0, vitest_1.expect)(res.body.every((w) => w.language === 'da')).toBe(true);
    });
    (0, vitest_1.it)('GET /words?language=xx returns empty array', async () => {
        const res = await (0, supertest_1.default)(app).get('/words?language=xx');
        (0, vitest_1.expect)(res.status).toBe(200);
        (0, vitest_1.expect)(res.body).toEqual([]);
    });
    (0, vitest_1.it)('GET /words/random returns a single word', async () => {
        const res = await (0, supertest_1.default)(app).get('/words/random');
        (0, vitest_1.expect)(res.status).toBe(200);
        (0, vitest_1.expect)(res.body).toHaveProperty('language');
        (0, vitest_1.expect)(res.body).toHaveProperty('word');
        (0, vitest_1.expect)(res.body).toHaveProperty('meaning');
    });
});
//# sourceMappingURL=app.test.js.map