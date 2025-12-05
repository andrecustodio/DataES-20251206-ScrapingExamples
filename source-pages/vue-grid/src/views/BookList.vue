<template>
  <div class="book-list-page">
    <header class="page-header">
      <h1>Biblioteca de Livros</h1>
      <p>Grid com carregamento lazy para exercícios de web scraping</p>
    </header>

    <div class="book-grid" v-if="books.length > 0">
      <div
        v-for="book in books"
        :key="book.key"
        class="book-card"
        :data-key="book.key"
        @click="navigateToDetail(book.key)"
      >
        <div class="book-cover-placeholder">
          {{ book.title.substring(0, 2) }}
        </div>
        <div class="book-info">
          <h3 class="book-title">{{ book.title }}</h3>
          <p class="book-authors">
            {{ book.authors.map(a => a.name).join(', ') }}
          </p>
          <p class="book-meta" v-if="book.publish_date">
            {{ book.publish_date }} • {{ book.number_of_pages }} páginas
          </p>
        </div>
      </div>
    </div>

    <div v-if="loading" class="loading">
      Carregando mais livros...
    </div>

    <div v-if="error" class="error">
      Erro ao carregar livros: {{ error }}
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted, onUnmounted, nextTick } from 'vue'
import { useRouter } from 'vue-router'
import type { Book } from '../types/book'

const router = useRouter()
const books = ref<Book[]>([])
const loading = ref(false)
const error = ref<string | null>(null)
const currentPage = ref(1)
const hasMore = ref(true)
const limit = 20

// Função para verificar se a página precisa de scroll
const needsScroll = (): boolean => {
  const documentHeight = document.documentElement.scrollHeight
  const windowHeight = window.innerHeight
  return documentHeight <= windowHeight
}

// Função para carregar livros
const loadBooks = async (page: number) => {
  if (loading.value || !hasMore.value) return

  loading.value = true
  error.value = null

  try {
    const response = await fetch(`/api/books?page=${page}&limit=${limit}`)
    if (!response.ok) {
      throw new Error(`HTTP error! status: ${response.status}`)
    }
    
    const data = await response.json()
    
    if (data.data && data.data.length > 0) {
      books.value.push(...data.data)
      currentPage.value = page
      hasMore.value = data.hasMore !== false && data.data.length === limit
    } else {
      hasMore.value = false
    }
  } catch (err) {
    error.value = err instanceof Error ? err.message : 'Erro desconhecido'
    console.error('Erro ao carregar livros:', err)
  } finally {
    loading.value = false
    
    // Aguardar DOM atualizar e verificar se precisa de mais dados
    await nextTick()
    if (needsScroll() && hasMore.value) {
      // Se não há scroll e ainda há mais dados, carregar próxima página
      loadBooks(page + 1)
    }
  }
}

// Função para detectar scroll e carregar mais
const handleScroll = () => {
  const scrollTop = window.pageYOffset || document.documentElement.scrollTop
  const windowHeight = window.innerHeight
  const documentHeight = document.documentElement.scrollHeight

  // Carregar mais quando estiver próximo do final (100px antes)
  if (scrollTop + windowHeight >= documentHeight - 100 && hasMore.value && !loading.value) {
    loadBooks(currentPage.value + 1)
  }
}

// Navegar para página de detalhes
const navigateToDetail = (key: string) => {
  router.push(`/book/${encodeURIComponent(key)}`)
}

// Carregar livros iniciais
onMounted(() => {
  loadBooks(1)
  window.addEventListener('scroll', handleScroll)
})

onUnmounted(() => {
  window.removeEventListener('scroll', handleScroll)
})
</script>

<style scoped>
.book-list-page {
  max-width: 1200px;
  margin: 0 auto;
  padding: 20px;
}

.page-header {
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  color: white;
  padding: 30px;
  border-radius: 10px;
  margin-bottom: 30px;
  text-align: center;
  box-shadow: 0 4px 6px rgba(0, 0, 0, 0.1);
}

.page-header h1 {
  font-size: 2.5em;
  margin-bottom: 10px;
}

.page-header p {
  font-size: 1.1em;
  opacity: 0.9;
}

.book-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(280px, 1fr));
  gap: 20px;
  margin-bottom: 20px;
}

.book-card {
  background: white;
  border-radius: 8px;
  padding: 20px;
  box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);
  transition: transform 0.2s, box-shadow 0.2s;
  cursor: pointer;
  display: flex;
  gap: 15px;
}

.book-card:hover {
  transform: translateY(-2px);
  box-shadow: 0 4px 8px rgba(0, 0, 0, 0.15);
}

.book-cover-placeholder {
  width: 60px;
  height: 90px;
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  border-radius: 4px;
  display: flex;
  align-items: center;
  justify-content: center;
  color: white;
  font-weight: bold;
  font-size: 1.2em;
  flex-shrink: 0;
}

.book-info {
  flex: 1;
}

.book-title {
  font-size: 1.2em;
  font-weight: bold;
  color: #333;
  margin-bottom: 8px;
}

.book-authors {
  color: #666;
  margin-bottom: 5px;
  font-size: 0.9em;
}

.book-meta {
  color: #999;
  font-size: 0.85em;
}

.loading {
  text-align: center;
  padding: 20px;
  color: #666;
}

.error {
  text-align: center;
  padding: 20px;
  color: #d32f2f;
  background: #ffebee;
  border-radius: 8px;
}
</style>

