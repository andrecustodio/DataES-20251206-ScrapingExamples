// Interfaces TypeScript para o modelo de livro baseado em OpenLibrary.org

export interface Author {
  key: string;
  name: string;
}

export interface Cover {
  small?: string;
  medium?: string;
  large?: string;
}

export interface Book {
  key: string;
  title: string;
  authors: Author[];
  isbn?: string[];
  publish_date?: string;
  number_of_pages?: number;
  cover?: Cover;
  subjects?: string[];
  description?: string;
  first_sentence?: string;
  language?: string;
}

export interface BookResponse {
  data: Book[];
  total: number;
  page: number;
  limit: number;
}

