import './instrument'; // Must be first — starts OTel before Express loads
import { createApp } from './app';

const port = Number(process.env.PORT ?? 8001);
const app = createApp();

app.listen(port, () => {
  console.log(`[express-api] listening on http://localhost:${port}`);
});
