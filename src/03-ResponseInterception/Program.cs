using PlaywrightHelpers;
using PlaywrightHelpers.Dtos;
using Microsoft.Playwright;
using System.Text.Json;
using System;
using System.Linq;

// URL da aplicação Vue.js que faz chamadas de API
const string VueGridUrl = "http://localhost:8000";

Console.WriteLine("=== Exercício 03: Interceptação de Respostas HTTP ===");
Console.WriteLine("Este exercício demonstra:");
Console.WriteLine("- Interceptar respostas HTTP usando route interception");
Console.WriteLine("- Filtrar respostas por padrões de URL ou tipos de conteúdo");
Console.WriteLine("- Extrair dados diretamente de respostas JSON/XML");
Console.WriteLine("- Processar respostas paginadas de APIs");
Console.WriteLine("- Monitorar e extrair dados de requisições XHR/fetch");
Console.WriteLine();

using var browserManager = new BrowserManager();

try
{
    await browserManager.InitializeAsync(headless: false);
    var page = await browserManager.CreatePageAsync();

    // Lista para armazenar as respostas interceptadas (cada resposta pode conter múltiplos livros)
    var interceptedBooks = new List<BookDto>();

    // ==========================================
    // Parte 1: Interceptação Básica de Respostas
    // ==========================================
    Console.WriteLine("--- Parte 1: Interceptação Básica de Respostas ---");

    // Configurar interceptação de rotas antes de navegar
    await page.RouteAsync("**/api/books**", async route =>
    {
        // Continuar com a requisição normalmente
        var response = await route.FetchAsync();

        // Obter o corpo da resposta
        var body = await response.TextAsync();

        Console.WriteLine($"   Interceptada: {route.Request.Url}");
        Console.WriteLine($"   Status: {response.Status}");
        Console.WriteLine($"   Content-Type: {response.Headers["content-type"]}");

        // Se for JSON, deserializar e armazenar
        if (response.Headers["content-type"]?.Contains("application/json") == true)
        {
            try
            {
                // Tentar deserializar como BooksResponseDto (formato padrão da API)
                var booksResponse = JsonSerializer.Deserialize<BooksResponseDto>(body);
                if (booksResponse != null && booksResponse.Data != null)
                {
                    interceptedBooks.AddRange(booksResponse.Data);
                    Console.WriteLine($"   ✓ Resposta JSON interceptada: {booksResponse.Data.Count} livro(s) adicionado(s)");
                    Console.WriteLine($"     Página: {booksResponse.Page}, Total: {booksResponse.Total}, HasMore: {booksResponse.HasMore}");
                }
                else
                {
                    // Fallback: tentar como array direto (compatibilidade)
                    var jsonDoc = JsonDocument.Parse(body);
                    var root = jsonDoc.RootElement;

                    if (root.ValueKind == JsonValueKind.Array)
                    {
                        var books = JsonSerializer.Deserialize<List<BookDto>>(body);
                        if (books != null)
                        {
                            interceptedBooks.AddRange(books);
                            Console.WriteLine($"   ✓ Resposta JSON interceptada (formato array): {books.Count} livro(s) adicionado(s)");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"   ⚠ Erro ao deserializar JSON: {ex.Message}");
            }
        }

        // Continuar com a resposta original
        await route.FulfillAsync(new RouteFulfillOptions
        {
            Response = response,
            Body = body
        });
    });

    Console.WriteLine("✓ Interceptação de rotas configurada para /api/books**");
    Console.WriteLine();

    // Navegar para a página que fará requisições de API
    Console.WriteLine($"Navegando para: {VueGridUrl}");
    await page.GotoAsync(VueGridUrl, new PageGotoOptions
    {
        WaitUntil = WaitUntilState.NetworkIdle
    });
    Console.WriteLine("✓ Página carregada");
    Console.WriteLine();

    // Aguardar um pouco para que as requisições iniciais sejam feitas
    Console.WriteLine("Aguardando requisições iniciais de API...");
    await page.WaitForTimeoutAsync(2000);
    Console.WriteLine($"✓ Respostas iniciais interceptadas: {interceptedBooks.Count} livro(s)");
    Console.WriteLine();

    // ==========================================
    // Parte 2: Extração de Dados de Respostas JSON
    // ==========================================
    Console.WriteLine("--- Parte 2: Extração de Dados de Respostas JSON ---");

    if (interceptedBooks.Count > 0)
    {
        var firstBook = interceptedBooks[0];

        Console.WriteLine($"   Total de livros interceptados: {interceptedBooks.Count}");

        if (firstBook is not null)
        {
            Console.WriteLine($"   Primeiro livro (usando DTO):");

            // Key
            if (!string.IsNullOrEmpty(firstBook.Key))
            {
                Console.WriteLine($"     - Key: {firstBook.Key}");
            }

            // Title
            if (!string.IsNullOrEmpty(firstBook.Title))
            {
                Console.WriteLine($"     - Título: {firstBook.Title}");
            }

            // Authors
            if (firstBook.Authors != null && firstBook.Authors.Count > 0)
            {
                Console.WriteLine($"     - Autores ({firstBook.Authors.Count}):");
                foreach (var author in firstBook.Authors)
                {
                    var authorKey = author.Key ?? "N/A";
                    var authorName = author.Name ?? "N/A";
                    Console.WriteLine($"       • {authorName} (Key: {authorKey})");
                }
            }

            // ISBN
            if (firstBook.Isbn != null && firstBook.Isbn.Count > 0)
            {
                Console.WriteLine($"     - ISBN ({firstBook.Isbn.Count}): {string.Join(", ", firstBook.Isbn)}");
            }

            // Publish Date
            if (!string.IsNullOrEmpty(firstBook.PublishDate))
            {
                Console.WriteLine($"     - Data de Publicação: {firstBook.PublishDate}");
            }

            // Number of Pages
            if (firstBook.NumberOfPages.HasValue)
            {
                Console.WriteLine($"     - Número de Páginas: {firstBook.NumberOfPages.Value}");
            }

            // Cover
            if (firstBook.Cover != null)
            {
                Console.WriteLine($"     - Capa:");
                if (!string.IsNullOrEmpty(firstBook.Cover.Small))
                    Console.WriteLine($"       • Small: {firstBook.Cover.Small}");
                if (!string.IsNullOrEmpty(firstBook.Cover.Medium))
                    Console.WriteLine($"       • Medium: {firstBook.Cover.Medium}");
                if (!string.IsNullOrEmpty(firstBook.Cover.Large))
                    Console.WriteLine($"       • Large: {firstBook.Cover.Large}");
            }

            // Subjects
            if (firstBook.Subjects != null && firstBook.Subjects.Count > 0)
            {
                Console.WriteLine($"     - Assuntos ({firstBook.Subjects.Count}): {string.Join(", ", firstBook.Subjects)}");
            }

            // Description
            if (!string.IsNullOrEmpty(firstBook.Description))
            {
                var shortDesc = firstBook.Description.Length > 100
                    ? firstBook.Description.Substring(0, 100) + "..."
                    : firstBook.Description;
                Console.WriteLine($"     - Descrição: {shortDesc}");
            }

            // First Sentence
            if (!string.IsNullOrEmpty(firstBook.FirstSentence))
            {
                var shortSentence = firstBook.FirstSentence.Length > 100
                    ? firstBook.FirstSentence.Substring(0, 100) + "..."
                    : firstBook.FirstSentence;
                Console.WriteLine($"     - Primeira Frase: {shortSentence}");
            }

            // Language
            if (!string.IsNullOrEmpty(firstBook.Language))
            {
                Console.WriteLine($"     - Idioma: {firstBook.Language}");
            }
        }



    }
    Console.WriteLine();

    // ==========================================
    // Parte 3: Interceptação com Filtro por Tipo de Conteúdo
    // ==========================================
    Console.WriteLine("--- Parte 3: Interceptação com Filtro por Tipo de Conteúdo ---");

    var jsonResponses = new List<string>();

    // Limpar interceptações anteriores e configurar nova
    await page.UnrouteAsync("**/api/books**");

    await page.RouteAsync("**/*", async route =>
    {
        var response = await route.FetchAsync();
        var contentType = response.Headers.GetValueOrDefault("content-type", "");

        // Filtrar apenas respostas JSON
        if (contentType.Contains("application/json"))
        {
            var body = await response.TextAsync();
            jsonResponses.Add(body);
            Console.WriteLine($"   JSON interceptado: {route.Request.Url}");
        }

        await route.FulfillAsync(new RouteFulfillOptions
        {
            Response = response
        });
    });

    // Recarregar a página para capturar novas requisições
    Console.WriteLine("Recarregando página para capturar novas requisições...");
    await page.ReloadAsync(new PageReloadOptions
    {
        WaitUntil = WaitUntilState.NetworkIdle
    });

    await page.WaitForTimeoutAsync(2000);
    Console.WriteLine($"✓ Total de respostas JSON interceptadas: {jsonResponses.Count}");
    Console.WriteLine();

    // ==========================================
    // Parte 4: Interceptação de Respostas Paginadas
    // ==========================================
    Console.WriteLine("--- Parte 4: Interceptação de Respostas Paginadas ---");

    var allBooks = new List<BookDto>();
    int? apiTotal = null;
    bool hasMore = true;
    int scrollAttempts = 0;
    const int maxScrollAttempts = 100; // Limite de segurança
    const int requestTimeout = 5000; // Timeout para aguardar novas requisições (5 segundos)

    // Configurar interceptação específica para paginação
    await page.UnrouteAsync("**/*");

    await page.RouteAsync("**/api/books**", async route =>
    {
        var response = await route.FetchAsync();
        var body = await response.TextAsync();

        // Verificar status da resposta
        if (response.Status != 200)
        {
            Console.WriteLine($"   ⚠ Resposta com status {response.Status}, parando scroll");
            hasMore = false;
            await route.FulfillAsync(new RouteFulfillOptions
            {
                Response = response,
                Body = body
            });
            return;
        }

        if (response.Headers.GetValueOrDefault("content-type", "").Contains("application/json"))
        {
            try
            {
                // Deserializar como BooksResponseDto (formato padrão da API)
                var booksResponse = JsonSerializer.Deserialize<BooksResponseDto>(body);
                if (booksResponse != null && booksResponse.Data != null && booksResponse.Data.Count > 0)
                {
                    allBooks.AddRange(booksResponse.Data);

                    // Atualizar total da API se disponível
                    if (booksResponse.Total.HasValue)
                    {
                        apiTotal = booksResponse.Total.Value;
                    }

                    // Verificar se há mais páginas
                    if (booksResponse.HasMore.HasValue && !booksResponse.HasMore.Value)
                    {
                        hasMore = false;
                    }

                    Console.WriteLine($"   Página interceptada: {route.Request.Url}");
                    Console.WriteLine($"   Livros nesta página: {booksResponse.Data.Count}");
                    Console.WriteLine($"   Total coletado: {allBooks.Count}" + (apiTotal.HasValue ? $"/{apiTotal.Value}" : ""));
                    Console.WriteLine($"   Página: {booksResponse.Page}, HasMore: {booksResponse.HasMore}");

                    // Verificar se já coletamos todos os livros
                    if (apiTotal.HasValue && allBooks.Count >= apiTotal.Value)
                    {
                        hasMore = false;
                        Console.WriteLine($"   ✓ Todos os livros foram coletados ({allBooks.Count}/{apiTotal.Value})");
                    }
                }
                else
                {
                    // Resposta vazia ou sem livros
                    Console.WriteLine($"   ⚠ Resposta sem livros ou vazia, parando scroll");
                    hasMore = false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"   ⚠ Erro ao deserializar resposta: {ex.Message}");
                hasMore = false;
            }
        }

        await route.FulfillAsync(new RouteFulfillOptions
        {
            Response = response,
            Body = body
        });
    });

    // Aguardar requisição inicial
    Console.WriteLine("Aguardando requisição inicial...");
    await page.ReloadAsync();
    await page.WaitForTimeoutAsync(2000);

    int previousCount = allBooks.Count;

    while (hasMore && scrollAttempts < maxScrollAttempts)
    {

        try
        {
            // Aguardar por uma nova requisição à API (Antes do Scroll)
            var responseTask = page.WaitForResponseAsync(
                response => response.Url.Contains("/api/books")
            );

            // Scroll para o final da página
            Console.WriteLine("Rolando a página para carregar todos os itens (lazy loading)...");
            await page.EvaluateAsync("window.scrollTo(0, document.body.scrollHeight)");

            await responseTask.ConfigureAwait(false);

            // A resposta já foi processada pelo route handler, não é necessário aguardar
            int currentCount = allBooks.Count;

            // Verificar se houve novos livros adicionados
            if (currentCount == previousCount)
            {
                Console.WriteLine($"   Scroll {scrollAttempts + 1}: Nenhum novo livro adicionado, parando scroll");
                break;
            }

            Console.WriteLine($"   Scroll {scrollAttempts + 1}: {currentCount - previousCount} novo(s) livro(s) adicionado(s)");
            previousCount = currentCount;

            // Verificar se já coletamos todos os livros
            if (apiTotal.HasValue && allBooks.Count >= apiTotal.Value) break;
        }
        catch (TimeoutException)
        {
            // Timeout significa que nenhuma nova requisição foi feita
            Console.WriteLine($"   ✓ Nenhuma nova requisição detectada após scroll. Fim do grid alcançado.");
            break;
        }

        scrollAttempts++;
    }

    if (scrollAttempts >= maxScrollAttempts)
    {
        Console.WriteLine($"   ⚠ Limite de tentativas de scroll atingido ({maxScrollAttempts})");
    }

    Console.WriteLine($"✓ Total de livros coletados de todas as páginas: {allBooks.Count}" + (apiTotal.HasValue ? $" (de {apiTotal.Value} total)" : ""));
    Console.WriteLine($"✓ Total de scrolls realizados: {scrollAttempts}");
    Console.WriteLine();

    // ==========================================
    // Parte 5: Extração Completa de Dados
    // ==========================================
    Console.WriteLine("--- Parte 5: Extração Completa de Dados ---");

    if (allBooks.Count > 0)
    {
        Console.WriteLine("Primeiros 5 livros extraídos (usando DTO):");
        for (int i = 0; i < Math.Min(5, allBooks.Count); i++)
        {
            var book = allBooks[i];
            var title = book.Title ?? "N/A";
            var isbn = book.Isbn != null && book.Isbn.Count > 0
                ? string.Join(", ", book.Isbn)
                : "N/A";

            Console.WriteLine($"   {i + 1}. {title}");
            Console.WriteLine($"      ISBN: {isbn}");
        }
    }

    Console.WriteLine();
    Console.WriteLine("=== Exercício concluído com sucesso! ===");
}
catch (Exception ex)
{
    Console.WriteLine($"Erro durante a execução: {ex.Message}");
    Console.WriteLine($"Detalhes: {ex}");
    Environment.Exit(1);
}
