<template>
  <div class="book-detail-page">
    <button @click="goBack" class="back-button">← Voltar</button>

    <div v-if="loading" class="loading">Carregando detalhes do livro...</div>
    <div v-else-if="error" class="error">Erro: {{ error }}</div>
    <div v-else-if="book" class="book-detail">
      <div class="book-header">
        <div class="book-cover-large">
          {{ book.title.substring(0, 2) }}
        </div>
        <div class="book-header-info">
          <h1 class="book-detail-title">{{ book.title }}</h1>
          <div class="book-detail-authors">
            <span
              v-for="(author, index) in book.authors"
              :key="author.key"
              class="author-name"
            >
              {{ author.name }}<span v-if="index < book.authors.length - 1">, </span>
            </span>
          </div>
          <div class="book-detail-meta">
            <span v-if="book.publish_date">Publicado em {{ book.publish_date }}</span>
            <span v-if="book.number_of_pages"> • {{ book.number_of_pages }} páginas</span>
            <span v-if="book.language"> • Idioma: {{ book.language }}</span>
          </div>
        </div>
      </div>

      <div class="book-detail-content">
        <div v-if="book.isbn && book.isbn.length > 0" class="book-detail-section">
          <h3>ISBN</h3>
          <p class="book-detail-isbn">{{ book.isbn.join(', ') }}</p>
        </div>

        <div v-if="book.description" class="book-detail-section">
          <h3>Descrição</h3>
          <p class="book-detail-description">{{ book.description }}</p>
        </div>

        <div v-if="book.first_sentence" class="book-detail-section">
          <h3>Primeira Frase</h3>
          <p class="book-detail-first-sentence">"{{ book.first_sentence }}"</p>
        </div>

        <div v-if="book.subjects && book.subjects.length > 0" class="book-detail-section">
          <h3>Assuntos</h3>
          <div class="book-detail-subjects">
            <span
              v-for="subject in book.subjects"
              :key="subject"
              class="subject-tag"
            >
              {{ subject }}
            </span>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useRouter, useRoute } from 'vue-router'
import type { Book } from '../types/book'

const router = useRouter()
const route = useRoute()
const book = ref<Book | null>(null)
const loading = ref(true)
const error = ref<string | null>(null)

const loadBookDetail = async () => {
  const key = route.params.key as string
  loading.value = true
  error.value = null

  try {
    const response = await fetch(`/api/books/${encodeURIComponent(key)}`)
    if (!response.ok) {
      throw new Error(`HTTP error! status: ${response.status}`)
    }
    
    const data = await response.json()
    book.value = data
  } catch (err) {
    error.value = err instanceof Error ? err.message : 'Erro desconhecido'
    console.error('Erro ao carregar detalhes do livro:', err)
  } finally {
    loading.value = false
  }
}

const goBack = () => {
  router.push('/')
}

onMounted(() => {
  loadBookDetail()
})
</script>

<style scoped>
.book-detail-page {
  max-width: 1000px;
  margin: 0 auto;
  padding: 20px;
}

.back-button {
  background: #667eea;
  color: white;
  border: none;
  padding: 10px 20px;
  border-radius: 5px;
  cursor: pointer;
  margin-bottom: 20px;
  font-size: 1em;
  transition: background 0.2s;
}

.back-button:hover {
  background: #5568d3;
}

.book-detail {
  background: white;
  border-radius: 10px;
  padding: 30px;
  box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);
}

.book-header {
  display: flex;
  gap: 30px;
  margin-bottom: 30px;
  padding-bottom: 30px;
  border-bottom: 2px solid #f0f0f0;
}

.book-cover-large {
  width: 150px;
  height: 225px;
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  border-radius: 8px;
  display: flex;
  align-items: center;
  justify-content: center;
  color: white;
  font-weight: bold;
  font-size: 2em;
  flex-shrink: 0;
}

.book-header-info {
  flex: 1;
}

.book-detail-title {
  font-size: 2.5em;
  color: #333;
  margin-bottom: 15px;
}

.book-detail-authors {
  font-size: 1.2em;
  color: #666;
  margin-bottom: 10px;
}

.author-name {
  font-weight: 500;
}

.book-detail-meta {
  color: #999;
  font-size: 1em;
}

.book-detail-content {
  display: flex;
  flex-direction: column;
  gap: 25px;
}

.book-detail-section h3 {
  color: #667eea;
  margin-bottom: 10px;
  font-size: 1.3em;
}

.book-detail-isbn {
  font-family: monospace;
  background: #f5f5f5;
  padding: 10px;
  border-radius: 5px;
  display: inline-block;
}

.book-detail-description {
  line-height: 1.6;
  color: #555;
  font-size: 1.1em;
}

.book-detail-first-sentence {
  font-style: italic;
  color: #666;
  font-size: 1.1em;
  padding: 15px;
  background: #f9f9f9;
  border-left: 4px solid #667eea;
  border-radius: 4px;
}

.book-detail-subjects {
  display: flex;
  flex-wrap: wrap;
  gap: 10px;
}

.subject-tag {
  background: #e3f2fd;
  color: #1976d2;
  padding: 5px 15px;
  border-radius: 20px;
  font-size: 0.9em;
}

.loading {
  text-align: center;
  padding: 40px;
  color: #666;
  font-size: 1.2em;
}

.error {
  text-align: center;
  padding: 40px;
  color: #d32f2f;
  background: #ffebee;
  border-radius: 8px;
  font-size: 1.1em;
}
</style>

