# Grafana Dashboards para Playwright Scraping

Este diretório contém dashboards pré-configurados do Grafana para visualizar métricas coletadas dos projetos de scraping com Playwright.

## Estrutura

```
grafana/
├── dashboards/              # Dashboards JSON
│   ├── 01-overview.json           # Dashboard geral com visão de alto nível
│   ├── 02-scraping-performance.json # Métricas detalhadas de performance
│   ├── 03-error-monitoring.json    # Monitoramento de erros
│   └── 04-response-times.json      # Tempos de resposta e latência
└── provisioning/           # Configuração de provisionamento
    ├── datasources/
    │   └── prometheus.yml    # Configuração do datasource Prometheus
    └── dashboards/
        └── dashboards.yml    # Configuração de auto-carregamento de dashboards
```

## Dashboards Disponíveis

### 1. Overview (Visão Geral)
**Arquivo:** `01-overview.json`

Dashboard de alto nível com métricas principais:
- Total de livros extraídos
- Taxa de extração (livros/segundo)
- Total de erros
- Tempo médio de carregamento
- Gráfico de livros extraídos ao longo do tempo

### 2. Scraping Performance
**Arquivo:** `02-scraping-performance.json`

Métricas detalhadas de performance do scraping:
- Taxa de extração de livros
- Throughput (livros por segundo)
- Total acumulado de livros extraídos
- Requisições de paginação
- Distribuição de requisições por página

### 3. Error Monitoring
**Arquivo:** `03-error-monitoring.json`

Monitoramento completo de erros:
- Total de erros
- Taxa de erros
- Taxa de sucesso
- Erros ao longo do tempo (por tipo)
- Distribuição de erros por tipo (gráfico de pizza e barras)

### 4. Response Times
**Arquivo:** `04-response-times.json`

Análise de tempos de resposta e latência:
- Tempo de carregamento de página (p50, p95, p99)
- Tempo de espera por elementos (p50, p95, p99)
- Tempo de resposta da API (p50, p95, p99)
- Gráficos de percentis ao longo do tempo

## Métricas Monitoradas

Os dashboards visualizam as seguintes métricas do Prometheus:

- `books_scraped_total` - Contador total de livros extraídos
- `books_scraped_per_second` - Gauge de throughput
- `page_load_time_seconds` - Histograma de tempo de carregamento
- `element_wait_time_seconds` - Histograma de tempo de espera
- `scraping_errors_total` - Contador de erros por tipo
- `api_response_time_seconds` - Histograma de tempo de resposta da API
- `pagination_requests_total` - Contador de requisições de paginação

## Configuração Automática

Os dashboards são carregados automaticamente quando o Grafana inicia, graças à configuração de provisionamento:

1. **Datasource:** O Prometheus é configurado automaticamente via `provisioning/datasources/prometheus.yml`
2. **Dashboards:** Os dashboards são carregados automaticamente via `provisioning/dashboards/dashboards.yml`

## Acesso aos Dashboards

1. Inicie a infraestrutura:
   ```bash
   cd source-pages
   docker-compose up -d
   ```

2. Acesse o Grafana:
   - URL: http://localhost:3001
   - Usuário padrão: `admin`
   - Senha padrão: `admin`

3. Os dashboards estarão disponíveis no menu lateral em "Dashboards"

## Personalização

Os dashboards podem ser editados diretamente no Grafana:
1. Acesse um dashboard
2. Clique no ícone de engrenagem (⚙️) no topo
3. Selecione "Edit" ou "Settings"
4. Faça suas alterações
5. Salve o dashboard

**Nota:** Alterações feitas na UI do Grafana não são salvas automaticamente nos arquivos JSON. Para persistir alterações, exporte o dashboard atualizado e substitua o arquivo correspondente.

## Troubleshooting

### Dashboards não aparecem

1. Verifique se os arquivos estão no diretório correto: `grafana/dashboards/`
2. Verifique as permissões do volume Docker
3. Verifique os logs do Grafana: `docker-compose logs grafana`
4. Certifique-se de que o Prometheus está configurado como datasource

### Métricas não aparecem

1. Verifique se o Prometheus está coletando métricas:
   - Acesse: http://localhost:9090
   - Execute a query: `books_scraped_total`
2. Verifique se o Pushgateway está funcionando:
   - Acesse: http://localhost:9091
3. Certifique-se de que o projeto Observability está enviando métricas

### Datasource não conecta

1. Verifique se o Prometheus está rodando: `docker-compose ps`
2. Verifique a URL do Prometheus no datasource (deve ser `http://prometheus:9090` dentro do Docker)
3. Teste a conexão no Grafana: Configuration → Data Sources → Prometheus → "Save & Test"

