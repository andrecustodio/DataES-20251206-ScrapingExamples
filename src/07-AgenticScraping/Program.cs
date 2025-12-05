using PlaywrightHelpers;
using Microsoft.Playwright;
using AgenticScraping;

// URL da aplicação Vue.js
const string VueGridUrl = "http://localhost:8000";

// Schema da classe BookInfo para enviar ao Gemini
const string BookInfoSchema = """
    public class BookInfo
    {
        public string Key { get; set; } = "";
        public string Title { get; set; } = "";
        public List<string> Authors { get; set; } = new();
        public string Isbn { get; set; } = "";
    }
    """;

Console.WriteLine("=== Exercício 07: Agentic Scraping com Gemini ===");
Console.WriteLine("Este exercício demonstra:");
Console.WriteLine("- Extração de dados usando IA generativa");
Console.WriteLine("- Envio do código fonte da página para o Gemini");
Console.WriteLine("- Parsing automático de dados estruturados");
Console.WriteLine("- Comparação entre scraping tradicional e agentic scraping");
Console.WriteLine();

// Obter API Key do Gemini
var apiKey = Environment.GetEnvironmentVariable("GEMINI_API_KEY");
if (string.IsNullOrEmpty(apiKey))
{
    Console.WriteLine("⚠ ATENÇÃO: A variável de ambiente GEMINI_API_KEY não está definida.");
    Console.WriteLine("Por favor, defina a variável com sua API Key do Google AI Studio:");
    Console.WriteLine("  Windows: set GEMINI_API_KEY=sua_api_key");
    Console.WriteLine("  Linux/Mac: export GEMINI_API_KEY=sua_api_key");
    Console.WriteLine();
    Console.WriteLine("Você pode obter uma API Key em: https://aistudio.google.com/apikey");
    Environment.Exit(1);
}

using var browserManager = new BrowserManager();

try
{
    await browserManager.InitializeAsync(headless: false);
    var page = await browserManager.CreatePageAsync();

    // ==========================================
    // Parte 1: Navegar e Capturar HTML da Página
    // ==========================================
    Console.WriteLine("--- Parte 1: Navegando e Capturando HTML da Página ---");

    Console.WriteLine($"Navegando para: {VueGridUrl}");
    await page.GotoAsync(VueGridUrl, new PageGotoOptions
    {
        WaitUntil = WaitUntilState.NetworkIdle
    });
    Console.WriteLine("✓ Página carregada");
    Console.WriteLine();

    // Usar o Page Object Model para interagir com a página
    var bookGridPage = new BookGridPage(page);
    await bookGridPage.WaitForPageLoadAsync();
    Console.WriteLine("✓ Grid de livros carregado");
    Console.WriteLine();

    // Rolar a página até carregar todos os livros
    var totalBooksLoaded = await bookGridPage.ScrollUntilAllBooksLoadedAsync();
    Console.WriteLine();

    // Capturar o HTML da página
    var pageHtml = await bookGridPage.GetPageSourceAsync();
    Console.WriteLine($"✓ HTML capturado ({pageHtml.Length} caracteres)");
    Console.WriteLine();

    // ==========================================
    // Parte 2: Enviar para o Agente Gemini
    // ==========================================
    Console.WriteLine("--- Parte 2: Enviando para o Agente Gemini ---");

    var geminiAgent = new GeminiAgentService(apiKey);

    Console.WriteLine("Enviando HTML e schema para o Gemini...");
    Console.WriteLine("(Isso pode levar alguns segundos)");
    Console.WriteLine();

    var extractedBooks = await geminiAgent.ExtractBooksFromHtmlAsync(pageHtml, BookInfoSchema);

    Console.WriteLine($"✓ Gemini retornou {extractedBooks.Count} livro(s)");
    Console.WriteLine();

    // ==========================================
    // Parte 3: Exibir Resultados
    // ==========================================
    Console.WriteLine("--- Parte 3: Livros Extraídos pelo Agente ---");

    if (extractedBooks.Count > 0)
    {
        Console.WriteLine($"Total de livros extraídos: {extractedBooks.Count}");
        Console.WriteLine();

        foreach (var book in extractedBooks.Take(10))
        {
            Console.WriteLine($"  ✓ {book.Title}");
            Console.WriteLine($"     Key: {book.Key}");
            Console.WriteLine($"     Autores: {(book.Authors.Any() ? string.Join(", ", book.Authors) : "N/A")}");
            Console.WriteLine($"     ISBN: {(string.IsNullOrEmpty(book.Isbn) ? "N/A" : book.Isbn)}");
            Console.WriteLine();
        }

        if (extractedBooks.Count > 10)
        {
            Console.WriteLine($"  ... e mais {extractedBooks.Count - 10} livro(s)");
            Console.WriteLine();
        }
    }
    else
    {
        Console.WriteLine("Nenhum livro foi extraído pelo agente.");
        Console.WriteLine();
    }

    // ==========================================
    // Parte 4: Comparação com Scraping Tradicional
    // ==========================================
    Console.WriteLine("--- Parte 4: Comparação com Scraping Tradicional ---");

    // Scraping tradicional usando seletores
    var traditionalBooks = new List<BookInfo>();
    var bookCards = await page.Locator(".book-card").CountAsync();

    for (int i = 0; i < Math.Min(10, bookCards); i++)
    {
        var card = page.Locator(".book-card").Nth(i);
        var title = await card.Locator(".book-title").TextContentAsync();
        var key = await card.GetAttributeAsync("data-key");
        var authorElements = await card.Locator(".book-author").AllTextContentsAsync();

        traditionalBooks.Add(new BookInfo
        {
            Key = key ?? "",
            Title = title?.Trim() ?? "",
            Authors = authorElements.Select(a => a.Trim()).ToList()
        });
    }

    Console.WriteLine($"Scraping Tradicional: {traditionalBooks.Count} livro(s)");
    Console.WriteLine($"Agentic Scraping:     {extractedBooks.Count} livro(s)");
    Console.WriteLine();

    // Comparar resultados
    var matchingTitles = extractedBooks
        .Select(b => b.Title.ToLowerInvariant())
        .Intersect(traditionalBooks.Select(b => b.Title.ToLowerInvariant()))
        .Count();

    Console.WriteLine($"Títulos em comum: {matchingTitles}");
    Console.WriteLine();

    Console.WriteLine("=== Exercício concluído com sucesso! ===");
    Console.WriteLine();
    Console.WriteLine("Observações:");
    Console.WriteLine("- O Agentic Scraping usa IA para entender a estrutura da página");
    Console.WriteLine("- Não requer conhecimento prévio dos seletores CSS/XPath");
    Console.WriteLine("- Pode ser mais resiliente a mudanças na estrutura da página");
    Console.WriteLine("- Porém tem custo de API e latência adicional");
}
catch (Exception ex)
{
    Console.WriteLine($"Erro durante a execução: {ex.Message}");
    Console.WriteLine($"Detalhes: {ex}");
    Environment.Exit(1);
}

