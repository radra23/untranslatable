"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
require("./instrument"); // Must be first — starts OTel before Express loads
const app_1 = require("./app");
const port = Number(process.env.PORT ?? 8001);
const app = (0, app_1.createApp)();
app.listen(port, () => {
    console.log(`[express-api] listening on http://localhost:${port}`);
});
//# sourceMappingURL=index.js.map