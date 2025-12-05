import { createApp } from 'vue'
import { createRouter, createWebHistory } from 'vue-router'
import App from './App.vue'
import BookList from './views/BookList.vue'
import BookDetail from './views/BookDetail.vue'

const routes = [
  { path: '/', component: BookList },
  { path: '/book/:key', component: BookDetail, props: true }
]

const router = createRouter({
  history: createWebHistory(),
  routes
})

const app = createApp(App)
app.use(router)
app.mount('#app')

