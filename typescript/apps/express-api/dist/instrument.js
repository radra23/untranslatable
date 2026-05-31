"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
const telemetry_1 = require("@untranslatable/telemetry");
const instrumentation_http_1 = require("@opentelemetry/instrumentation-http");
const instrumentation_express_1 = require("@opentelemetry/instrumentation-express");
(0, telemetry_1.startTelemetry)({
    instrumentations: [new instrumentation_http_1.HttpInstrumentation(), new instrumentation_express_1.ExpressInstrumentation()],
});
//# sourceMappingURL=instrument.js.map