FROM node:22-alpine AS dev

WORKDIR /app

COPY package.json package-lock.json ./
RUN npm ci

CMD ["npm", "run", "dev"]
