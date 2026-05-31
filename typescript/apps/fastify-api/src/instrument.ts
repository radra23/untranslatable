import { startTelemetry } from '@untranslatable/telemetry';
import { HttpInstrumentation } from '@opentelemetry/instrumentation-http';
import { FastifyInstrumentation } from '@opentelemetry/instrumentation-fastify';

startTelemetry({
  instrumentations: [new HttpInstrumentation(), new FastifyInstrumentation()],
});
