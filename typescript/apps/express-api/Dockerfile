FROM node:22-alpine
RUN npm install -g pnpm@9
WORKDIR /app
COPY . .
RUN pnpm install --frozen-lockfile
RUN pnpm -r build
WORKDIR /app/apps/express-api
EXPOSE 8001
CMD ["node", "dist/index.js"]
