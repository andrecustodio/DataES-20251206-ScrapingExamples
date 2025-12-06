# Criando Scrapers com Playwright para .NET

Este repositório contém exemplos práticos de web scraping usando Playwright.NET, organizados em exercícios progressivos para desenvolvedores de nível intermediário.

## Visão Geral

Este workshop apresenta técnicas de web scraping usando Playwright.NET através de 8 exercícios práticos que cobrem desde conceitos básicos até padrões avançados, IA generativa e observabilidade.

## Estrutura do Projeto

```
palestra/
├── src/                          # Código fonte
│   ├── Shared/                   # Biblioteca compartilhada
│   │   └── PlaywrightHelpers/   # Gerenciamento de browser e páginas
│   ├── 01-BasicSetup/            # Configuração inicial
│   ├── 02-ElementSelectionAndWaits/  # Seletores e esperas
│   ├── 03-ResponseInterception/  # Interceptação de respostas HTTP
│   ├── 04-ErrorHandling/         # Tratamento de erros
│   ├── 05-CompleteExample/       # Exemplo completo
│   ├── 06-PageObjectModel/       # Padrão POM
│   ├── 07-AgenticScraping/       # Agentic Scraping com IA (Gemini)
│   └── 08-Observability/         # Observabilidade completa
└── source-pages/                 # Páginas alvo para scraping
    ├── static-grid/              # Página HTML estática
    ├── vue-grid/                 # Aplicação Vue.js com lazy loading
    └── docker-compose.yml        # Infraestrutura completa
```

## Início Rápido

### Pré-requisitos

Consulte o arquivo [PREREQUISITES.md](PREREQUISITES.md) para informações detalhadas sobre os requisitos do sistema.

### Configuração Inicial

1. Clone o repositório
2. Instale o .NET SDK (versão 6.0 ou superior, ou .NET 10.0)
3. Instale os navegadores do Playwright:
   ```bash
   dotnet tool install --global Microsoft.Playwright.CLI
   playwright install
   ```
4. Inicie a infraestrutura (páginas alvo e serviços):
   ```bash
   cd source-pages
   docker-compose up -d
   ```

### Executando os Exercícios

Cada exercício é independente e pode ser executado separadamente:

```bash
cd src/01-BasicSetup
dotnet run
```

Consulte o README.md de cada exercício para instruções específicas.

## Exercícios

1. **01-BasicSetup** - Configuração inicial, lançamento do navegador e navegação básica
2. **02-ElementSelectionAndWaits** - Seletores CSS/XPath, extração de dados e estratégias de espera
3. **03-ResponseInterception** - Interceptação de respostas HTTP para extrair dados de APIs
4. **04-ErrorHandling** - Lógica de retry, tratamento de exceções e logging estruturado
5. **05-CompleteExample** - Projeto completo combinando todas as técnicas
6. **06-PageObjectModel** - Implementação do padrão Page Object Model
7. **07-AgenticScraping** - Agentic Scraping com IA generativa (Gemini) para extração automática de dados
8. **08-Observability** - Observabilidade completa com OpenTelemetry, Prometheus e Seq

## Páginas Alvo

As páginas alvo estão disponíveis em `source-pages/`:

- **static-grid**: Página HTML estática com grid de livros
- **vue-grid**: Aplicação Vue.js com carregamento lazy e API backend integrada

## Tecnologias Utilizadas

- .NET 10.0 / .NET 6.0+
- Playwright.NET
- OpenTelemetry.NET
- Prometheus.NET
- Seq (para logging estruturado)
- Vue.js + TypeScript (páginas alvo)
- Docker & Docker Compose

## Licença

Este projeto está licenciado sob a licença MIT - veja o arquivo [LICENSE](LICENSE) para detalhes.

## Contribuindo

Este é um projeto de workshop educacional. Sinta-se livre para usar e adaptar conforme necessário.


