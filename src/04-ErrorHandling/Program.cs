using PlaywrightHelpers;
using Microsoft.Playwright;
using Serilog;

// URLs das páginas alvo
const string StaticGridUrl = "http://localhost:8080";
const string VueGridUrl = "http://localhost:8000";
const string SeqUrl = "http://localhost:5343"; // Porta padrão do Seq

Console.WriteLine("=== Exercício 04: Tratamento de Erros ===");
Console.WriteLine("Este exercício demonstra:");
Console.WriteLine("- Lógica de retry para operações que podem falhar");
Console.WriteLine("- Tratamento de exceções em web scraping");
Console.WriteLine("- Captura de screenshots quando erros ocorrem");
Console.WriteLine("- Envio de mensagens de erro personalizadas para Seq");
Console.WriteLine("- Boas práticas para tratamento de erros");
Console.WriteLine();

// Configurar Serilog para enviar logs para Seq
Log.Logger = new LoggerConfiguration()
    .WriteTo.Seq(SeqUrl)
    .WriteTo.Console()
    .CreateLogger();

using var browserManager = new BrowserManager();

try
{
    await browserManager.InitializeAsync(headless: false);
    var page = await browserManager.CreatePageAsync();

    // ==========================================
    // Parte 1: Retry Logic com Backoff Exponencial
    // ==========================================
    Console.WriteLine("--- Parte 1: Retry Logic com Backoff Exponencial ---");

    async Task<T> RetryWithBackoffAsync<T>(
        Func<Task<T>> operation,
        int maxRetries = 3,
        int initialDelayMs = 1000)
    {
        int attempt = 0;
        Exception? lastException = null;

        while (attempt < maxRetries)
        {
            try
            {
                return await operation();
            }
            catch (Exception ex)
            {
                lastException = ex;
                attempt++;
                
                if (attempt < maxRetries)
                {
                    int delay = initialDelayMs * (int)Math.Pow(2, attempt - 1);
                    Console.WriteLine($"   Tentativa {attempt} falhou. Tentando novamente em {delay}ms...");
                    Console.WriteLine($"   Erro: {ex.Message}");
                    
                    // Log do erro no Seq
                    Log.Warning(ex, "Tentativa {Attempt} falhou. Retry em {Delay}ms", attempt, delay);
                    
                    await Task.Delay(delay);
                }
            }
        }

        throw new Exception($"Operação falhou após {maxRetries} tentativas", lastException);
    }

    // Exemplo de uso: navegação com retry
    await RetryWithBackoffAsync(async () =>
    {
        Console.WriteLine($"   Navegando para: {StaticGridUrl}");
        await page.GotoAsync(StaticGridUrl, new PageGotoOptions
        {
            WaitUntil = WaitUntilState.NetworkIdle,
            Timeout = 10000
        });
        Console.WriteLine("   ✓ Navegação bem-sucedida");
        return true;
    });

    Console.WriteLine();

    // ==========================================
    // Parte 2: Tratamento de Exceções Específicas
    // ==========================================
    Console.WriteLine("--- Parte 2: Tratamento de Exceções Específicas ---");

    async Task SafeElementInteractionAsync(IPage page, string selector, string action)
    {
        try
        {
            var element = await page.WaitForSelectorAsync(selector, new PageWaitForSelectorOptions
            {
                Timeout = 5000,
                State = WaitForSelectorState.Visible
            });

            if (element == null)
            {
                throw new Exception($"Elemento não encontrado: {selector}");
            }

            // Simular ação
            Console.WriteLine($"   ✓ {action} executado com sucesso no elemento: {selector}");
        }
        catch (TimeoutException ex)
        {
            // Capturar screenshot em caso de timeout
            var screenshotPath = $"error-timeout-element-{selector}-{DateTime.Now:yyyyMMddHHmmss}.png";
            await page.ScreenshotAsync(new PageScreenshotOptions
            {
                Path = screenshotPath,
                FullPage = true
            });

            var errorMessage = $"Timeout ao aguardar elemento: {selector}. Screenshot salvo em: {screenshotPath}";
            Console.WriteLine($"   ⚠ {errorMessage}");
            
            Log.Error(ex, "Timeout ao aguardar elemento {Selector}. Screenshot: {Screenshot}", 
                selector, screenshotPath);
            
            throw new Exception(errorMessage, ex);
        }
        catch (PlaywrightException ex)
        {
            var screenshotPath = $"error-playwright-{DateTime.Now:yyyyMMddHHmmss}.png";
            await page.ScreenshotAsync(new PageScreenshotOptions
            {
                Path = screenshotPath,
                FullPage = true
            });

            var errorMessage = $"Erro do Playwright: {ex.Message}. Screenshot: {screenshotPath}";
            Console.WriteLine($"   ⚠ {errorMessage}");
            
            Log.Error(ex, "Erro do Playwright ao interagir com {Selector}. Screenshot: {Screenshot}",
                selector, screenshotPath);
            
            throw;
        }
    }

    // Testar com seletor válido
    await SafeElementInteractionAsync(page, "#page-header", "Leitura do cabeçalho");
    
    // Testar com seletor inválido (vai gerar erro, mas será tratado)
    try
    {
        await SafeElementInteractionAsync(page, "#elemento-inexistente", "Tentativa de leitura");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"   Erro esperado capturado: {ex.Message}");
    }
    Console.WriteLine();

    // ==========================================
    // Parte 3: Screenshots Automáticos em Erros
    // ==========================================
    Console.WriteLine("--- Parte 3: Screenshots Automáticos em Erros ---");

    async Task<T> ExecuteWithScreenshotOnErrorAsync<T>(
        IPage page,
        Func<Task<T>> operation,
        string operationName)
    {
        try
        {
            return await operation();
        }
        catch (Exception ex)
        {
            var timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");
            var screenshotPath = $"error-{operationName}-{timestamp}.png";
            
            try
            {
                await page.ScreenshotAsync(new PageScreenshotOptions
                {
                    Path = screenshotPath,
                    FullPage = true
                });
                
                // Log estruturado no Seq
                Log.Error(ex, 
                    "Erro durante operação {Operation}. Screenshot: {Screenshot}. URL: {Url}",
                    operationName, screenshotPath, page.Url);

                Console.WriteLine($"   ⚠ Screenshot de erro salvo: {screenshotPath}");
            }
            catch (Exception screenshotEx)
            {
                Log.Warning(screenshotEx, "Falha ao capturar screenshot durante erro");
            }

            throw;
        }
    }

    // Exemplo de uso
    try
    {
        await ExecuteWithScreenshotOnErrorAsync(page, async () =>
        {
            // Simular uma operação que pode falhar
            await page.GotoAsync("http://localhost:9999/invalid-url", new PageGotoOptions
            {
                Timeout = 5000
            });
            return true;
        }, "Navegação para URL inválida");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"   Erro capturado e screenshot criado: {ex.Message}");
    }
    Console.WriteLine();

    // ==========================================
    // Parte 4: Mensagens Personalizadas para Seq
    // ==========================================
    Console.WriteLine("--- Parte 4: Mensagens Personalizadas para Seq ---");

    // Navegar para a página Vue.js
    Console.WriteLine($"Navegando para: {VueGridUrl}");
    await page.GotoAsync(VueGridUrl, new PageGotoOptions
    {
        WaitUntil = WaitUntilState.NetworkIdle
    });
    Console.WriteLine("✓ Página carregada");
    Console.WriteLine();

    // Função para log estruturado com contexto
    void LogScrapingError(string operation, Exception ex, IPage page, Dictionary<string, object>? additionalContext = null)
    {
        var context = new Dictionary<string, object>
        {
            ["Operation"] = operation,
            ["Url"] = page.Url,
            ["Title"] = page.TitleAsync().Result ?? "N/A",
            ["Timestamp"] = DateTime.UtcNow
        };

        if (additionalContext != null)
        {
            foreach (var kvp in additionalContext)
            {
                context[kvp.Key] = kvp.Value;
            }
        }

        Log.Error(ex, 
            "Erro durante scraping: {Operation} em {Url}. Contexto: {@Context}",
            operation, page.Url, context);
    }

    // Simular diferentes tipos de erros com contexto
    try
    {
        var element = await page.WaitForSelectorAsync(".elemento-que-nao-existe", new PageWaitForSelectorOptions
        {
            Timeout = 3000
        });
    }
    catch (TimeoutException ex)
    {
        LogScrapingError("Busca de elemento", ex, page, new Dictionary<string, object>
        {
            ["Selector"] = ".elemento-que-nao-existe",
            ["ErrorType"] = "ElementNotFound",
            ["Suggestion"] = "Verificar se o seletor está correto ou se o elemento é carregado dinamicamente"
        });
        
        Console.WriteLine("   ✓ Erro logado no Seq com contexto completo");
    }
    Console.WriteLine();

    // ==========================================
    // Parte 5: Boas Práticas - Wrapper de Operações
    // ==========================================
    Console.WriteLine("--- Parte 5: Boas Práticas - Wrapper de Operações ---");

    async Task<T> SafeScrapingOperationAsync<T>(
        IPage page,
        Func<Task<T>> operation,
        string operationName,
        int maxRetries = 3)
    {
        int attempt = 0;
        Exception? lastException = null;

        while (attempt < maxRetries)
        {
            try
            {
                var result = await operation();
                
                Log.Information("Operação {Operation} concluída com sucesso na tentativa {Attempt}",
                    operationName, attempt + 1);
                
                return result;
            }
            catch (Exception ex)
            {
                lastException = ex;
                attempt++;

                if (attempt < maxRetries)
                {
                    var delay = 1000 * attempt;
                    Log.Warning(ex, 
                        "Tentativa {Attempt} de {Operation} falhou. Retry em {Delay}ms",
                        attempt, operationName, delay);
                    
                    await Task.Delay(delay);
                }
                else
                {
                    // Última tentativa falhou - capturar screenshot e logar
                    var screenshotPath = $"final-error-{operationName}-{DateTime.Now:yyyyMMddHHmmss}.png";
                    await page.ScreenshotAsync(new PageScreenshotOptions
                    {
                        Path = screenshotPath,
                        FullPage = true
                    });

                    LogScrapingError(operationName, ex, page, new Dictionary<string, object>
                    {
                        ["MaxRetries"] = maxRetries,
                        ["FinalAttempt"] = attempt,
                        ["Screenshot"] = screenshotPath
                    });
                }
            }
        }

        throw new Exception($"Operação {operationName} falhou após {maxRetries} tentativas", lastException);
    }

    // Exemplo de uso do wrapper
    var bookCount = await SafeScrapingOperationAsync(page, async () =>
    {
        var books = await page.Locator(".book-card").CountAsync();
        if (books == 0)
        {
            throw new Exception("Nenhum livro encontrado");
        }
        return books;
    }, "Contagem de livros");

    Console.WriteLine($"   ✓ Total de livros encontrados: {bookCount}");
    Console.WriteLine();

    Console.WriteLine("=== Exercício concluído com sucesso! ===");
    Console.WriteLine("Verifique os logs no Seq em: " + SeqUrl);
}
catch (Exception ex)
{
    Console.WriteLine($"Erro fatal durante a execução: {ex.Message}");
    Console.WriteLine($"Detalhes: {ex}");
    
    Log.Fatal(ex, "Erro fatal durante execução do exercício");
    Environment.Exit(1);
}
finally
{
    Log.CloseAndFlush();
}
