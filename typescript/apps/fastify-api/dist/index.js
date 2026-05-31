"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
require("./instrument"); // Must be first — starts OTel before Fastify loads
const app_1 = require("./app");
const port = Number(process.env.PORT ?? 8002);
const app = (0, app_1.createApp)();
app.listen({ port, host: '0.0.0.0' }, (err, address) => {
    if (err) {
        console.error(err);
        process.exit(1);
    }
    console.log(`[fastify-api] listening on ${address}`);
});
//# sourceMappingURL=index.js.map