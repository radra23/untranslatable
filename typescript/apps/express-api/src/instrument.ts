import { startTelemetry } from '@untranslatable/telemetry';
import { HttpInstrumentation } from '@opentelemetry/instrumentation-http';
import { ExpressInstrumentation } from '@opentelemetry/instrumentation-express';

startTelemetry({
  instrumentations: [new HttpInstrumentation(), new ExpressInstrumentation()],
});
