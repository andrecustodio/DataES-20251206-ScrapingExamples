using PlaywrightHelpers;
using PlaywrightHelpers.Dtos;
using Microsoft.Playwright;
using OpenTelemetry;
using OpenTelemetry.Trace;
using Prometheus;
using Serilog;
using System.Diagnostics;
using System.Text.Json;
using Observability;

// URLs e configurações
const string VueGridUrl = "http://localhost:8000";
const string SeqUrl = "http://localhost:5343";
const string PrometheusPushgatewayUrl = "http://localhost:9091";

// Schema da classe BookInfo para enviar ao Gemini
const string BookInfoSchema = """
    public class BookInfo
    {
        public string Key { get; set; } = "";
        public string Title { get; set; } = "";
        public List<string> Authors { get; set; } = new();
        public string Isbn { get; set; } = "";
        public string? Description { get; set; }
        public string? PublishDate { get; set; }
        public int? NumberOfPages { get; set; }
    }
    """;

Console.WriteLine("=== Exercício 08: Observabilidade Completa com Agentic Scraping ===");
Console.WriteLine("Este exercício demonstra:");
Console.WriteLine("- OpenTelemetry distributed tracing com spans detalhados");
Console.WriteLine("- Prometheus metrics exportados via Pushgateway");
Console.WriteLine("- Seq logging estruturado com correlação de traces");
Console.WriteLine("- Page Object Model (POM) para interação com páginas");
Console.WriteLine("- Response Interception para captura de dados da API");
Console.WriteLine("- Agentic Scraping com Gemini para parsing de dados");
Console.WriteLine();

// Configurar Serilog com correlação de trace
Log.Logger = new LoggerConfiguration()
    .Enrich.WithProperty("Application", "PlaywrightScraping")
    .WriteTo.Seq(SeqUrl)
    .WriteTo.Console()
    .CreateLogger();

// Configurar OpenTelemetry para tracing
using var tracerProvider = Sdk.CreateTracerProviderBuilder()
    .AddSource("PlaywrightScraping")
    .AddConsoleExporter()
    .Build();

var tracer = tracerProvider.GetTracer("PlaywrightScraping");

// ==========================================
// Criar Métricas Prometheus para todos os Dashboards
// ==========================================

// Overview Dashboard Metrics
var booksScrapedCounter = Metrics.CreateCounter("books_scraped_total", "Total de livros extraídos");
var booksPerSecondGauge = Metrics.CreateGauge("books_scraped_per_second", "Livros extraídos por segundo");
var scrapingErrorsCounter = Metrics.CreateCounter("scraping_errors_total", "Total de erros durante scraping", new[] { "error_type" });

// Response Times Dashboard Metrics
var pageLoadTimeHistogram = Metrics.CreateHistogram("page_load_time_seconds", "Tempo de carregamento de página em segundos",
    new HistogramConfiguration { Buckets = Histogram.ExponentialBuckets(0.1, 2, 10) });
var elementWaitTimeHistogram = Metrics.CreateHistogram("element_wait_time_seconds", "Tempo de espera por elementos em segundos",
    new HistogramConfiguration { Buckets = Histogram.ExponentialBuckets(0.05, 2, 8) });
var apiResponseTimeHistogram = Metrics.CreateHistogram("api_response_time_seconds", "Tempo de resposta da API em segundos", 
    new[] { "endpoint" },
    new HistogramConfiguration { Buckets = Histogram.ExponentialBuckets(0.01, 2, 10) });

// Performance Dashboard Metrics
var paginationRequestsCounter = Metrics.CreateCounter("pagination_requests_total", "Total de requisições de paginação", new[] { "page_number" });
var scrollAttemptsCounter = Metrics.CreateCounter("scroll_attempts_total", "Total de tentativas de scroll");
var booksPerPageGauge = Metrics.CreateGauge("books_per_page", "Livros carregados por página", new[] { "page_number" });

// Agentic Scraping Metrics
var geminiRequestsCounter = Metrics.CreateCounter("gemini_requests_total", "Total de requisições ao Gemini", new[] { "status" });
var geminiLatencyHistogram = Metrics.CreateHistogram("gemini_latency_seconds", "Latência das requisições ao Gemini",
    new HistogramConfiguration { Buckets = Histogram.ExponentialBuckets(0.5, 2, 8) });

// POM Metrics
var pageNavigationsCounter = Metrics.CreateCounter("page_navigations_total", "Total de navegações de página", new[] { "page_type" });
var bookDetailViewsCounter = Metrics.CreateCounter("book_detail_views_total", "Total de visualizações de detalhes de livro");

using var browserManager = new BrowserManager();

// Obter API Key do Gemini (opcional - se não tiver, pula a parte de agentic scraping)
var geminiApiKey = Environment.GetEnvironmentVariable("GEMINI_API_KEY");
var hasGeminiKey = !string.IsNullOrEmpty(geminiApiKey);

if (!hasGeminiKey)
{
    Console.WriteLine("⚠ GEMINI_API_KEY não definida - Agentic Scraping será pulado");
    Console.WriteLine("  Para habilitar: set GEMINI_API_KEY=sua_api_key");
    Console.WriteLine();
}

try
{
    var activitySource = new ActivitySource("PlaywrightScraping");
    var interceptedBooks = new List<BookDto>();
    var interceptedJsonResponses = new List<string>();
    int currentPage = 0;

    // ==========================================
    // Parte 1: Inicialização e Navegação com Tracing
    // ==========================================
    Console.WriteLine("--- Parte 1: Navegação com Tracing e Métricas ---");

    using var navigationActivity = activitySource.StartActivity("NavigateToPage");
    navigationActivity?.SetTag("url", VueGridUrl);
    navigationActivity?.SetTag("operation", "navigation");

    await browserManager.InitializeAsync(headless: false);
    var page = await browserManager.CreatePageAsync();

    // ==========================================
    // Parte 2: Configurar Response Interception
    // ==========================================
    Console.WriteLine("--- Parte 2: Configurando Interceptação de Respostas ---");

    await page.RouteAsync("**/api/books**", async route =>
    {
        var requestStopwatch = Stopwatch.StartNew();
        
        var response = await route.FetchAsync();
        var body = await response.TextAsync();

        requestStopwatch.Stop();
        
        // Registrar métricas de API response time
        apiResponseTimeHistogram.WithLabels("/api/books").Observe(requestStopwatch.Elapsed.TotalSeconds);
        
        currentPage++;
        paginationRequestsCounter.WithLabels(currentPage.ToString()).Inc();

        Console.WriteLine($"   Interceptada: {route.Request.Url}");
        Console.WriteLine($"   Status: {response.Status}, Tempo: {requestStopwatch.ElapsedMilliseconds}ms");

        if (response.Headers["content-type"]?.Contains("application/json") == true)
        {
            try
            {
                var booksResponse = JsonSerializer.Deserialize<BooksResponseDto>(body);
                if (booksResponse?.Data != null && booksResponse.Data.Count > 0)
                {
                    interceptedBooks.AddRange(booksResponse.Data);
                    interceptedJsonResponses.Add(body);
                    
                    booksPerPageGauge.WithLabels(currentPage.ToString()).Set(booksResponse.Data.Count);
                    
                    Console.WriteLine($"   ✓ {booksResponse.Data.Count} livros interceptados (Página {booksResponse.Page})");
                    
                    Log.Information("API Response interceptada: {Count} livros, página {Page}",
                        booksResponse.Data.Count, booksResponse.Page);
                }
            }
            catch (Exception ex)
            {
                scrapingErrorsCounter.WithLabels("json_parse").Inc();
                Log.Warning(ex, "Erro ao deserializar resposta JSON");
            }
        }

        await route.FulfillAsync(new RouteFulfillOptions
        {
            Response = response,
            Body = body
        });
    });

    Console.WriteLine("✓ Interceptação configurada para /api/books**");
    Console.WriteLine();

    // ==========================================
    // Parte 3: Navegação com Métricas de Tempo
    // ==========================================
    Console.WriteLine("--- Parte 3: Navegação e Carregamento ---");

    var navigationStopwatch = Stopwatch.StartNew();
    
    Log.Information("Iniciando navegação para {Url}", VueGridUrl);
    
    await page.GotoAsync(VueGridUrl, new PageGotoOptions
    {
        WaitUntil = WaitUntilState.NetworkIdle
    });
    
    navigationStopwatch.Stop();
    pageLoadTimeHistogram.Observe(navigationStopwatch.Elapsed.TotalSeconds);
    pageNavigationsCounter.WithLabels("list").Inc();
    
    navigationActivity?.SetTag("status", "success");
    navigationActivity?.SetTag("duration_ms", navigationStopwatch.ElapsedMilliseconds);
    
    Log.Information("Navegação concluída em {Duration}ms. TraceId: {TraceId}", 
        navigationStopwatch.ElapsedMilliseconds, Activity.Current?.TraceId.ToString());
    
    Console.WriteLine($"✓ Página carregada em {navigationStopwatch.ElapsedMilliseconds}ms");
    Console.WriteLine();

    // ==========================================
    // Parte 4: Usar POM para Interagir com a Lista
    // ==========================================
    Console.WriteLine("--- Parte 4: Page Object Model - Lista de Livros ---");

    using var pomActivity = activitySource.StartActivity("PageObjectModel_BookList");
    
    var bookListPage = new BookListPage(page);
    
    var waitStopwatch = Stopwatch.StartNew();
    await bookListPage.WaitForPageLoadAsync();
    waitStopwatch.Stop();
    elementWaitTimeHistogram.Observe(waitStopwatch.Elapsed.TotalSeconds);
    
    pomActivity?.SetTag("initial_wait_ms", waitStopwatch.ElapsedMilliseconds);
    
    Console.WriteLine($"✓ Grid carregado em {waitStopwatch.ElapsedMilliseconds}ms");

    // Scroll para carregar todos os livros com métricas
    var scrollStopwatch = Stopwatch.StartNew();
    var scrollAttempts = 0;
    var previousCount = await bookListPage.GetTotalBookCountAsync();
    
    Console.WriteLine($"   Livros iniciais: {previousCount}");
    
    while (scrollAttempts < 15)
    {
        scrollAttemptsCounter.Inc();
        var loadedMore = await bookListPage.ScrollToLoadMoreBooksAsync();
        
        if (!loadedMore) break;
        
        scrollAttempts++;
        var currentCount = await bookListPage.GetTotalBookCountAsync();
        Console.WriteLine($"   Scroll #{scrollAttempts}: {currentCount} livros");
    }
    
    scrollStopwatch.Stop();
    var totalBooksLoaded = await bookListPage.GetTotalBookCountAsync();
    
    pomActivity?.SetTag("total_books", totalBooksLoaded);
    pomActivity?.SetTag("scroll_attempts", scrollAttempts);
    pomActivity?.SetTag("scroll_time_ms", scrollStopwatch.ElapsedMilliseconds);
    
    Console.WriteLine($"✓ Total: {totalBooksLoaded} livros carregados após {scrollAttempts} scrolls");
    Console.WriteLine();

    // ==========================================
    // Parte 5: Extrair Dados da Lista usando POM
    // ==========================================
    Console.WriteLine("--- Parte 5: Extração de Dados com POM ---");

    using var extractionActivity = activitySource.StartActivity("ExtractBookData");
    var extractionStopwatch = Stopwatch.StartNew();
    
    var booksFromPom = await bookListPage.GetBooksFromListAsync(Math.Min(20, totalBooksLoaded));
    
    foreach (var book in booksFromPom)
    {
        booksScrapedCounter.Inc();
    }
    
    extractionStopwatch.Stop();
    extractionActivity?.SetTag("books_extracted", booksFromPom.Count);
    extractionActivity?.SetTag("extraction_time_ms", extractionStopwatch.ElapsedMilliseconds);
    
    var elapsedSeconds = extractionStopwatch.Elapsed.TotalSeconds;
    if (elapsedSeconds > 0)
    {
        booksPerSecondGauge.Set(booksFromPom.Count / elapsedSeconds);
    }
    
    Log.Information("POM extraiu {Count} livros em {Duration}ms",
        booksFromPom.Count, extractionStopwatch.ElapsedMilliseconds);
    
    Console.WriteLine($"✓ {booksFromPom.Count} livros extraídos via POM em {extractionStopwatch.ElapsedMilliseconds}ms");
    Console.WriteLine();

    // ==========================================
    // Parte 6: Navegar para Detalhes usando POM
    // ==========================================
    Console.WriteLine("--- Parte 6: Page Object Model - Página de Detalhes ---");

    if (booksFromPom.Count > 0)
    {
        using var detailActivity = activitySource.StartActivity("ViewBookDetail");
        
        var detailStopwatch = Stopwatch.StartNew();
        var bookKey = await bookListPage.ClickBookAsync(0);
        
        pageNavigationsCounter.WithLabels("detail").Inc();
        bookDetailViewsCounter.Inc();
        
        var bookDetailPage = new BookDetailPage(page);
        await bookDetailPage.WaitForPageLoadAsync(bookKey);
        
        detailStopwatch.Stop();
        pageLoadTimeHistogram.Observe(detailStopwatch.Elapsed.TotalSeconds);
        
        // Extrair informações detalhadas
        var detailedBook = await bookDetailPage.GetBookInfoAsync(bookKey);
        
        detailActivity?.SetTag("book_key", bookKey);
        detailActivity?.SetTag("book_title", detailedBook.Title);
        detailActivity?.SetTag("detail_load_time_ms", detailStopwatch.ElapsedMilliseconds);
        
        Log.Information("Detalhes do livro {Key}: {Title}", bookKey, detailedBook.Title);
        
        Console.WriteLine($"✓ Detalhes carregados em {detailStopwatch.ElapsedMilliseconds}ms");
        Console.WriteLine($"   Título: {detailedBook.Title}");
        Console.WriteLine($"   Autores: {string.Join(", ", detailedBook.Authors)}");
        if (!string.IsNullOrEmpty(detailedBook.Isbn))
            Console.WriteLine($"   ISBN: {detailedBook.Isbn}");
        
        // Voltar para a lista
        await bookDetailPage.GoBackToListAsync();
        pageNavigationsCounter.WithLabels("list").Inc();
        Console.WriteLine("✓ Retornou para a lista");
    }
    Console.WriteLine();

    // ==========================================
    // Parte 7: Agentic Scraping com Gemini (se API Key disponível)
    // ==========================================
    if (hasGeminiKey && interceptedJsonResponses.Count > 0)
    {
        Console.WriteLine("--- Parte 7: Agentic Scraping com Gemini ---");
        
        using var geminiActivity = activitySource.StartActivity("AgenticScraping");
        
        var geminiService = new GeminiAgentService(geminiApiKey!);
        
        // Usar apenas a primeira resposta interceptada para demonstração
        var jsonToProcess = interceptedJsonResponses[0];
        
        Console.WriteLine("Enviando JSON interceptado para o Gemini...");
        var geminiStopwatch = Stopwatch.StartNew();
        
        try
        {
            var geminiBooks = await geminiService.ExtractBooksFromJsonAsync(jsonToProcess, BookInfoSchema);
            
            geminiStopwatch.Stop();
            geminiLatencyHistogram.Observe(geminiStopwatch.Elapsed.TotalSeconds);
            geminiRequestsCounter.WithLabels("success").Inc();
            
            geminiActivity?.SetTag("gemini_books_extracted", geminiBooks.Count);
            geminiActivity?.SetTag("gemini_latency_ms", geminiStopwatch.ElapsedMilliseconds);
            
            Log.Information("Gemini extraiu {Count} livros em {Duration}ms",
                geminiBooks.Count, geminiStopwatch.ElapsedMilliseconds);
            
            Console.WriteLine($"✓ Gemini extraiu {geminiBooks.Count} livros em {geminiStopwatch.ElapsedMilliseconds}ms");
            
            // Comparar resultados
            var matchingTitles = geminiBooks
                .Select(b => b.Title.ToLowerInvariant())
                .Intersect(booksFromPom.Select(b => b.Title.ToLowerInvariant()))
                .Count();
            
            Console.WriteLine($"   Títulos em comum com POM: {matchingTitles}");
        }
        catch (Exception ex)
        {
            geminiStopwatch.Stop();
            geminiRequestsCounter.WithLabels("error").Inc();
            scrapingErrorsCounter.WithLabels("gemini_error").Inc();
            
            Log.Error(ex, "Erro na chamada ao Gemini");
            Console.WriteLine($"⚠ Erro no Gemini: {ex.Message}");
        }
        Console.WriteLine();
    }

    // ==========================================
    // Parte 8: Demonstração de Erro com Correlação
    // ==========================================
    Console.WriteLine("--- Parte 8: Demonstração de Erro com Correlação ---");

    using var errorActivity = activitySource.StartActivity("SimulateError");
    errorActivity?.SetTag("operation", "error_simulation");

    try
    {
        Log.Information("Simulando erro para demonstrar correlação. TraceId: {TraceId}",
            Activity.Current?.TraceId.ToString());
        
        await page.WaitForSelectorAsync(".elemento-inexistente", new PageWaitForSelectorOptions
        {
            Timeout = 2000
        });
    }
    catch (TimeoutException ex)
    {
        scrapingErrorsCounter.WithLabels("timeout").Inc();
        errorActivity?.SetTag("error_type", "timeout");
        errorActivity?.SetTag("status", "error");
        
        Log.Error(ex, 
            "Timeout ao aguardar elemento. TraceId: {TraceId}, SpanId: {SpanId}",
            Activity.Current?.TraceId.ToString(),
            Activity.Current?.SpanId.ToString());
        
        Console.WriteLine($"   ⚠ Erro simulado capturado (TraceId: {Activity.Current?.TraceId})");
    }
    Console.WriteLine();

    // ==========================================
    // Parte 9: Resumo e Comparação de Métricas
    // ==========================================
    Console.WriteLine("--- Parte 9: Resumo de Extração ---");
    
    Console.WriteLine($"📊 Dados coletados:");
    Console.WriteLine($"   Via Interceptação: {interceptedBooks.Count} livros");
    Console.WriteLine($"   Via POM:           {booksFromPom.Count} livros");
    Console.WriteLine($"   Páginas carregadas: {currentPage}");
    Console.WriteLine($"   Scrolls realizados: {scrollAttempts}");
    Console.WriteLine();
    
    // Mostrar alguns livros interceptados
    if (interceptedBooks.Count > 0)
    {
        Console.WriteLine("Primeiros 5 livros (via interceptação):");
        foreach (var book in interceptedBooks.Take(5))
        {
            Console.WriteLine($"   • {book.Title}");
        }
    }
    Console.WriteLine();

    // ==========================================
    // Parte 10: Enviar Métricas para Pushgateway
    // ==========================================
    Console.WriteLine("--- Parte 10: Enviando Métricas para Prometheus Pushgateway ---");

    await SendMetricsToPushgateway();

    Console.WriteLine();
    Console.WriteLine("=== Exercício concluído com sucesso! ===");
    Console.WriteLine($"Verifique os dados em:");
    Console.WriteLine($"  - Seq: {SeqUrl}");
    Console.WriteLine($"  - Prometheus: http://localhost:9090");
    Console.WriteLine($"  - Grafana: http://localhost:3001");
    Console.WriteLine($"  - TraceId atual: {Activity.Current?.TraceId}");
}
catch (Exception ex)
{
    scrapingErrorsCounter.WithLabels("fatal_error").Inc();
    
    Log.Fatal(ex, 
        "Erro fatal durante execução. TraceId: {TraceId}",
        Activity.Current?.TraceId.ToString());
    
    Console.WriteLine($"Erro fatal: {ex.Message}");
    
    // Tentar enviar métricas mesmo em caso de erro
    try { await SendMetricsToPushgateway(); } catch { }
    
    Environment.Exit(1);
}
finally
{
    Log.CloseAndFlush();
}

// ==========================================
// Função auxiliar para enviar métricas
// ==========================================
async Task SendMetricsToPushgateway()
{
    try
    {
        using var stream = new MemoryStream();
        await Metrics.DefaultRegistry.CollectAndExportAsTextAsync(stream);
        stream.Position = 0;
        using var reader = new StreamReader(stream);
        string metricsText = await reader.ReadToEndAsync();
        
        using var httpClient = new HttpClient();
        var content = new StringContent(metricsText, System.Text.Encoding.UTF8, "text/plain");
        
        var response = await httpClient.PostAsync(
            $"{PrometheusPushgatewayUrl}/metrics/job/playwright_scraping/instance/exercise_08", 
            content);
        
        if (response.IsSuccessStatusCode)
        {
            Console.WriteLine("✓ Métricas enviadas para Pushgateway com sucesso");
            Log.Information("Métricas enviadas para Prometheus Pushgateway");
        }
        else
        {
            Console.WriteLine($"⚠ Falha ao enviar métricas: {response.StatusCode}");
            Log.Warning("Falha ao enviar métricas para Pushgateway: {StatusCode}", response.StatusCode);
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"⚠ Erro ao enviar métricas: {ex.Message}");
        Log.Warning(ex, "Erro ao enviar métricas para Pushgateway");
    }
}
