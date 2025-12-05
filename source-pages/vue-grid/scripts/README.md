# Gerador de Dados de Livros

Este script C# gera arquivos JSON com dados de livros no formato OpenLibrary.org para uso nos exercícios de web scraping.

## Como Executar

```bash
cd source-pages/vue-grid/scripts
dotnet run
```

O script irá:
1. Gerar dados de 70+ livros da literatura brasileira
2. Criar arquivos JSON na pasta `../data/` (10 livros por arquivo)
3. Cada livro terá todos os campos necessários no formato OpenLibrary.org

## Estrutura dos Dados Gerados

Cada arquivo JSON contém um array de objetos `Book` com os seguintes campos:

- `key`: Identificador único (ex: `/works/OL123456W`)
- `title`: Título do livro
- `authors`: Array de autores com `key` e `name`
- `isbn`: Array com ISBN-10 e ISBN-13
- `publish_date`: Ano de publicação
- `number_of_pages`: Número de páginas
- `cover`: URLs para capas (small, medium, large)
- `subjects`: Array de assuntos/genres
- `description`: Descrição do livro
- `first_sentence`: Primeira frase do livro
- `language`: Código do idioma (ex: "por")

## Localização dos Arquivos

Os arquivos JSON serão gerados em:
```
source-pages/vue-grid/data/books_1.json
source-pages/vue-grid/data/books_2.json
...
```

## Nota

Os dados gerados são fictícios e servem apenas para fins de demonstração nos exercícios de web scraping.

