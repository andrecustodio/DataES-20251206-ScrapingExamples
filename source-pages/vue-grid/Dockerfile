# Build stage
FROM node:20-alpine AS builder

# Install Yarn v1 (classic) to match yarn.lock format
RUN apk add --no-cache yarn

WORKDIR /app

# Copiar arquivos de dependências e código fonte necessários para o build
COPY package.json yarn.lock ./
COPY tsconfig*.json ./
COPY vite.config.ts ./
COPY index.html ./
COPY data ./data
COPY server ./server
COPY src ./src

# Instalar dependências
RUN yarn install --frozen-lockfile

# Build frontend
RUN yarn build:frontend

# Build backend
RUN yarn build:backend

# Fix backend output structure (rootDir="../" creates server/dist/server/, move to server/dist/)
RUN if [ -d "server/dist/server" ]; then \
      mv server/dist/server/* server/dist/ && \
      rmdir server/dist/server; \
    fi

# Production stage
FROM node:20-alpine

# Install Yarn v1 (classic) to match yarn.lock format
RUN apk add --no-cache yarn

WORKDIR /app

# Copiar package.json e yarn.lock e instalar dependências (incluindo vite para preview)
COPY package.json yarn.lock ./
RUN yarn install --frozen-lockfile 

# Copiar arquivos buildados
COPY --from=builder /app/dist ./dist
COPY --from=builder /app/server/dist ./server/dist
COPY --from=builder /app/data ./data

# Expor portas
EXPOSE 8000

# Comando para iniciar o servidor (que serve tanto frontend quanto backend)
CMD ["node", "server/dist/index.js"]
