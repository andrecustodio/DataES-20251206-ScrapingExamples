import express from 'express';
import cors from 'cors';
import * as fs from 'fs';
import * as path from 'path';
import type { Book } from '../src/types/book';

const app = express();
const PORT = process.env.PORT ? parseInt(process.env.PORT, 10) : 8000;

// Middleware
app.use(cors());
app.use(express.json());

// Serve static files from the dist directory (frontend build)
const distPath = path.join(process.cwd(), 'dist');
if (fs.existsSync(distPath)) {
  app.use(express.static(distPath));
}

// Carregar dados dos livros na memória
let booksDatabase: Book[] = [];

function loadBooksFromJson(): void {
  // In production (Docker), data is at /app/data (not /app/server/data)
  // Try production path first, then fallback to development path
  const productionDataDir = path.join(process.cwd(), 'data');
  const developmentDataDir = path.join(__dirname, '../data');
  
  let dataDir: string;
  if (fs.existsSync(productionDataDir)) {
    dataDir = productionDataDir;
  } else if (fs.existsSync(developmentDataDir)) {
    dataDir = developmentDataDir;
  } else {
    throw new Error(`Data directory not found. Tried: ${productionDataDir} and ${developmentDataDir}`);
  }
  
  const files = fs.readdirSync(dataDir).filter(f => f.endsWith('.json'));
  
  booksDatabase = [];
  
  for (const file of files) {
    try {
      const filePath = path.join(dataDir, file);
      const fileContent = fs.readFileSync(filePath, 'utf-8');
      const books: Book[] = JSON.parse(fileContent);
      
      if (Array.isArray(books)) {
        booksDatabase.push(...books);
      } else if (books && typeof books === 'object') {
        // Se for um objeto único, adicionar como array
        booksDatabase.push(books as Book);
      }
    } catch (error) {
      console.error(`Erro ao carregar arquivo ${file}:`, error);
    }
  }
  
  console.log(`Total de livros carregados: ${booksDatabase.length}`);
}

// Carregar dados na inicialização
loadBooksFromJson();

// Helper function to generate random delay between 1 and 3 seconds
function randomDelay(): Promise<void> {
  const delayMs = Math.floor(Math.random() * 2000) + 1000; // 1000-3000ms
  return new Promise(resolve => setTimeout(resolve, delayMs));
}

// Endpoint: Listar livros com paginação
app.get('/api/books', async (req, res) => {
  await randomDelay();
  
  const page = parseInt(req.query.page as string) || 1;
  const limit = parseInt(req.query.limit as string) || 20;
  const start = (page - 1) * limit;
  const end = start + limit;
  
  const paginatedBooks = booksDatabase.slice(start, end);
  
  res.json({
    data: paginatedBooks,
    total: booksDatabase.length,
    page: page,
    limit: limit,
    hasMore: end < booksDatabase.length
  });
});

// Endpoint: Obter detalhes de um livro específico
app.get('/api/books/:key', async (req, res) => {
  await randomDelay();
  
  const key = decodeURIComponent(req.params.key);
  const book = booksDatabase.find(b => b.key === key);
  
  if (!book) {
    return res.status(404).json({ error: 'Livro não encontrado' });
  }
  
  res.json(book);
});

// Health check
app.get('/health', (req, res) => {
  res.json({ status: 'ok', booksLoaded: booksDatabase.length });
});

// Serve index.html for all non-API routes (SPA fallback)
app.get('*', (req, res) => {
  // Don't serve index.html for API routes
  if (req.path.startsWith('/api')) {
    return res.status(404).json({ error: 'Not found' });
  }
  
  const indexPath = path.join(process.cwd(), 'dist', 'index.html');
  if (fs.existsSync(indexPath)) {
    res.sendFile(indexPath);
  } else {
    res.status(404).send('Frontend build not found');
  }
});

app.listen(PORT, '0.0.0.0', () => {
  console.log(`Servidor rodando em http://0.0.0.0:${PORT}`);
  console.log(`Total de livros disponíveis: ${booksDatabase.length}`);
});

