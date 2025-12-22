# syntax=docker/dockerfile:1

FROM node:20-alpine AS base
WORKDIR /app

# Install dependencies first (better layer caching)
COPY package.json package-lock.json ./
RUN npm ci

# Copy the rest of the project
COPY . .

EXPOSE 5173

# Default: dev server (use docker-compose for overrides)
CMD ["npm", "run", "dev", "--", "--host", "0.0.0.0", "--port", "5173"]
