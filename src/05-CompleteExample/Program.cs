using PlaywrightHelpers;
using Microsoft.Playwright;
using System.Text.Json;

// URL da aplicação Vue.js
const string VueGridUrl = "http://localhost:8000";

Console.WriteLine("=== Exercício 05: Exemplo Completo ===");
Console.WriteLine("Este exercício combina todas as técnicas aprendidas:");
Console.WriteLine("- Navegação e seleção de elementos");
Console.WriteLine("- Interceptação de respostas HTTP");
Console.WriteLine("- Tratamento de erros com retry");
Console.WriteLine("- Extração e processamento de dados");
Console.WriteLine();

using var browserManager = new BrowserManager();
await browserManager.InitializeAsync(headless: false);
var page = await browserManager.CreatePageAsync();

try
{

    // Lista para armazenar todos os livros coletados
    var allBooks = new List<BookInfo>();

    // ==========================================
    // Parte 1: Configurar Interceptação de API
    // ==========================================
    Console.WriteLine("--- Parte 1: Configurando Interceptação de API ---");

    await page.RouteAsync("**/api/books**", async route =>
    {
        var response = await route.FetchAsync();
        var body = await response.TextAsync();

        if (response.Headers.GetValueOrDefault("content-type", "").Contains("application/json"))
        {
            try
            {
                var jsonDoc = JsonDocument.Parse(body);
                var root = jsonDoc.RootElement;

                if (root.ValueKind == JsonValueKind.Array)
                {
                    foreach (var book in root.EnumerateArray())
                    {
                        var bookInfo = ExtractBookInfo(book);
                        if (bookInfo != null)
                        {
                            allBooks.Add(bookInfo);
                        }
                    }
                }
                else if (root.TryGetProperty("data", out var data) && data.ValueKind == JsonValueKind.Array)
                {
                    foreach (var book in data.EnumerateArray())
                    {
                        var bookInfo = ExtractBookInfo(book);
                        if (bookInfo != null)
                        {
                            allBooks.Add(bookInfo);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"   ⚠ Erro ao processar resposta: {ex.Message}");
            }
        }

        await route.FulfillAsync(new RouteFulfillOptions
        {
            Response = response,
            Body = body
        });
    });

    Console.WriteLine("✓ Interceptação configurada");
    Console.WriteLine();

    // ==========================================
    // Parte 2: Navegação com Retry
    // ==========================================
    Console.WriteLine("--- Parte 2: Navegação com Retry ---");

    await SafeScrapingHelper.SafeScrapingOperationAsync(
        page,
        async () =>
        {
            Console.WriteLine($"   Navegando para {VueGridUrl}");
            await page.GotoAsync(VueGridUrl, new PageGotoOptions
            {
                WaitUntil = WaitUntilState.NetworkIdle,
                Timeout = 15000
            });
            Console.WriteLine("   ✓ Navegação bem-sucedida");
        },
        "Navegação para VueGrid",
        maxRetries: 3,
        onSuccess: (opName, attempt) => Console.WriteLine($"   ✓ {opName} concluída na tentativa {attempt}"),
        onRetry: (opName, attempt, delay, ex) => Console.WriteLine($"   ⚠ Tentativa {attempt} de {opName} falhou. Retry em {delay}ms... Erro: {ex.Message}"),
        onFinalFailure: (opName, maxRetries, finalAttempt, ex, screenshot) => Console.WriteLine($"   ⚠ {opName} falhou após {maxRetries} tentativas. Screenshot: {screenshot}")
    );
    Console.WriteLine();

    // ==========================================
    // Parte 3: Aguardar Carregamento Dinâmico
    // ==========================================
    Console.WriteLine("--- Parte 3: Aguardando Carregamento Dinâmico ---");

    // Aguardar grid aparecer
    await SafeScrapingHelper.SafeScrapingOperationAsync(
        page,
        async () =>
        {
            await page.WaitForSelectorAsync(".book-grid", new PageWaitForSelectorOptions
            {
                State = WaitForSelectorState.Visible,
                Timeout = 10000
            });
            Console.WriteLine("✓ Grid de livros carregado");
        },
        "Aguardar grid de livros",
        maxRetries: 3,
        onSuccess: (opName, attempt) => { },
        onRetry: (opName, attempt, delay, ex) => Console.WriteLine($"   ⚠ Tentativa {attempt} de {opName} falhou. Retry em {delay}ms..."),
        onFinalFailure: (opName, maxRetries, finalAttempt, ex, screenshot) => Console.WriteLine($"   ⚠ {opName} falhou após {maxRetries} tentativas. Screenshot: {screenshot}")
    );

    // Aguardar alguns cards aparecerem
    await SafeScrapingHelper.SafeScrapingOperationAsync(
        page,
        async () =>
        {
            await page.WaitForFunctionAsync("document.querySelectorAll('.book-card').length >= 5");
            Console.WriteLine("✓ Pelo menos 5 cards carregados");
        },
        "Aguardar cards de livros",
        maxRetries: 3,
        onSuccess: (opName, attempt) => { },
        onRetry: (opName, attempt, delay, ex) => Console.WriteLine($"   ⚠ Tentativa {attempt} de {opName} falhou. Retry em {delay}ms..."),
        onFinalFailure: (opName, maxRetries, finalAttempt, ex, screenshot) => Console.WriteLine($"   ⚠ {opName} falhou após {maxRetries} tentativas. Screenshot: {screenshot}")
    );
    Console.WriteLine();

    // ==========================================
    // Parte 4: Scroll para Carregar Mais Dados
    // ==========================================
    Console.WriteLine("--- Parte 4: Carregando Mais Dados via Scroll ---");

    int previousCount = allBooks.Count;
    int scrollAttempts = 0;
    const int maxScrollAttempts = 5;

    while (scrollAttempts < maxScrollAttempts)
    {
        // Scroll para o final da página
        await page.EvaluateAsync("window.scrollTo(0, document.body.scrollHeight)");
        await page.WaitForTimeoutAsync(2000);

        // Verificar se novos dados foram carregados
        int currentCount = allBooks.Count;
        if (currentCount > previousCount)
        {
            Console.WriteLine($"   Scroll {scrollAttempts + 1}: {currentCount - previousCount} novos livros carregados (Total: {currentCount})");
            previousCount = currentCount;
        }
        else
        {
            Console.WriteLine($"   Scroll {scrollAttempts + 1}: Nenhum novo dado carregado");
        }

        scrollAttempts++;
    }

    Console.WriteLine($"✓ Total de livros coletados: {allBooks.Count}");
    Console.WriteLine();

    // ==========================================
    // Parte 5: Navegação para Páginas de Detalhes
    // ==========================================
    Console.WriteLine("--- Parte 5: Navegando para Páginas de Detalhes ---");

    // Pegar o primeiro card e clicar para ver detalhes
    var bookKey = await SafeScrapingHelper.SafeScrapingOperationAsync(
        page,
        async () =>
        {
            var firstCard = page.Locator(".book-card").First;
            return await firstCard.GetAttributeAsync("data-key");
        },
        "Obter chave do primeiro livro",
        maxRetries: 3,
        onSuccess: (opName, attempt) => { },
        onRetry: (opName, attempt, delay, ex) => Console.WriteLine($"   ⚠ Tentativa {attempt} de {opName} falhou. Retry em {delay}ms..."),
        onFinalFailure: (opName, maxRetries, finalAttempt, ex, screenshot) => Console.WriteLine($"   ⚠ {opName} falhou após {maxRetries} tentativas. Screenshot: {screenshot}")
    );

    if (!string.IsNullOrEmpty(bookKey))
    {
        Console.WriteLine($"   Clicando no primeiro livro (Key: {bookKey})");

        // Clicar no card e navegar para detalhes
        await SafeScrapingHelper.SafeScrapingOperationAsync(
            page,
            async () =>
            {
                var firstCard = page.Locator(".book-card").First;
                await firstCard.ClickAsync();

                // Aguardar página de detalhes carregar
                await page.WaitForURLAsync($"**/book/{bookKey}**", new PageWaitForURLOptions
                {
                    Timeout = 10000
                });
                Console.WriteLine("✓ Página de detalhes carregada");
            },
            "Navegação para página de detalhes",
            maxRetries: 3,
            onSuccess: (opName, attempt) => { },
            onRetry: async (opName, attempt, delay, ex) =>
            {
                Console.WriteLine($"   ⚠ Tentativa {attempt} de {opName} falhou. Retry em {delay}ms...");
                
                // Navegando de volta para a lista antes de tentar novamente
                await page.GotoAsync(VueGridUrl, new PageGotoOptions
                {
                    WaitUntil = WaitUntilState.NetworkIdle,
                    Timeout = 10000
                });
            },
            onFinalFailure: (opName, maxRetries, finalAttempt, ex, screenshot) => Console.WriteLine($"   ⚠ {opName} falhou após {maxRetries} tentativas. Screenshot: {screenshot}")
        );

        // Extrair informações detalhadas
        var detailTitle = await page.Locator(".book-detail-title").TextContentAsync();
        var detailAuthors = await page.Locator(".book-detail-authors").AllTextContentsAsync();
        var detailDescription = await page.Locator(".book-detail-description").TextContentAsync();

        Console.WriteLine($"   Título: {detailTitle?.Trim()}");
        Console.WriteLine($"   Autores: {string.Join(", ", detailAuthors.Select(a => a.Trim()))}");
        Console.WriteLine($"   Descrição: {(detailDescription?.Length > 100 ? detailDescription.Substring(0, 100) + "..." : detailDescription?.Trim())}");
        Console.WriteLine();

        // Voltar para a lista
        await SafeScrapingHelper.SafeScrapingOperationAsync(
            page,
            async () =>
            {
                await page.GoBackAsync();
                await page.WaitForSelectorAsync(".book-grid");
                Console.WriteLine("✓ Retornado para a lista de livros");
            },
            "Retornar para lista de livros",
            maxRetries: 3,
            onSuccess: (opName, attempt) => { },
            onRetry: (opName, attempt, delay, ex) => Console.WriteLine($"   ⚠ Tentativa {attempt} de {opName} falhou. Retry em {delay}ms..."),
            onFinalFailure: (opName, maxRetries, finalAttempt, ex, screenshot) => Console.WriteLine($"   ⚠ {opName} falhou após {maxRetries} tentativas. Screenshot: {screenshot}")
        );
    }
    Console.WriteLine();

    // ==========================================
    // Parte 6: Processamento e Exibição de Dados
    // ==========================================
    Console.WriteLine("--- Parte 6: Processamento e Exibição de Dados ---");

    Console.WriteLine($"Total de livros coletados: {allBooks.Count}");
    Console.WriteLine();

    if (allBooks.Count > 0)
    {
        Console.WriteLine("Primeiros 10 livros:");
        for (int i = 0; i < Math.Min(10, allBooks.Count); i++)
        {
            var book = allBooks[i];
            Console.WriteLine($"   {i + 1}. {book.Title}");
            Console.WriteLine($"      Key: {book.Key}");
            Console.WriteLine($"      Autores: {string.Join(", ", book.Authors)}");
            if (!string.IsNullOrEmpty(book.Isbn))
            {
                Console.WriteLine($"      ISBN: {book.Isbn}");
            }
            Console.WriteLine();
        }

        // Estatísticas
        var uniqueAuthors = allBooks
            .SelectMany(b => b.Authors)
            .Distinct()
            .Count();

        Console.WriteLine($"Estatísticas:");
        Console.WriteLine($"   - Total de livros: {allBooks.Count}");
        Console.WriteLine($"   - Autores únicos: {uniqueAuthors}");
        Console.WriteLine($"   - Livros com ISBN: {allBooks.Count(b => !string.IsNullOrEmpty(b.Isbn))}");
    }

    Console.WriteLine();
    Console.WriteLine("=== Exercício concluído com sucesso! ===");
}
catch (Exception ex)
{
    Console.WriteLine($"Erro durante a execução: {ex.Message}");
    Console.WriteLine($"Detalhes: {ex}");

    // Capturar screenshot em caso de erro
    try
    {
        var screenshotPath = $"error-complete-example-{DateTime.Now:yyyyMMddHHmmss}.png";
        await page.ScreenshotAsync(new PageScreenshotOptions
        {
            Path = screenshotPath,
            FullPage = true
        });
        Console.WriteLine($"Screenshot de erro salvo em: {screenshotPath}");
    }
    catch { }

    Environment.Exit(1);
}

// Função auxiliar para extrair informações de um livro do JSON
BookInfo? ExtractBookInfo(JsonElement bookElement)
{
    try
    {
        var title = bookElement.TryGetProperty("title", out var titleProp)
            ? titleProp.GetString()
            : null;

        var key = bookElement.TryGetProperty("key", out var keyProp)
            ? keyProp.GetString()
            : null;

        var authors = new List<string>();
        if (bookElement.TryGetProperty("authors", out var authorsProp) && authorsProp.ValueKind == JsonValueKind.Array)
        {
            foreach (var author in authorsProp.EnumerateArray())
            {
                if (author.TryGetProperty("name", out var nameProp))
                {
                    var name = nameProp.GetString();
                    if (!string.IsNullOrEmpty(name))
                    {
                        authors.Add(name);
                    }
                }
            }
        }

        var isbn = "";
        if (bookElement.TryGetProperty("isbn", out var isbnProp) && isbnProp.ValueKind == JsonValueKind.Array)
        {
            var isbns = isbnProp.EnumerateArray()
                .Select(e => e.GetString())
                .Where(s => !string.IsNullOrEmpty(s))
                .ToList();

            if (isbns.Any())
            {
                isbn = isbns.First() ?? "";
            }
        }

        if (string.IsNullOrEmpty(title) || string.IsNullOrEmpty(key))
        {
            return null;
        }

        return new BookInfo
        {
            Key = key ?? "",
            Title = title ?? "",
            Authors = authors,
            Isbn = isbn
        };
    }
    catch
    {
        return null;
    }
}
