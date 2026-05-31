import './instrument'; // Must be first — starts OTel before Fastify loads
import { createApp } from './app';

const port = Number(process.env.PORT ?? 8002);
const app = createApp();

app.listen({ port, host: '0.0.0.0' }, (err, address) => {
  if (err) {
    console.error(err);
    process.exit(1);
  }
  console.log(`[fastify-api] listening on ${address}`);
});
