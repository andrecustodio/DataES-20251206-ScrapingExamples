using Microsoft.Playwright;

namespace AgenticScraping;

/// <summary>
/// Page Object Model para a página de grid de livros.
/// Encapsula interações para scroll e captura de HTML.
/// </summary>
public class BookGridPage
{
    private readonly IPage _page;

    public BookGridPage(IPage page)
    {
        _page = page;
    }

    /// <summary>
    /// Aguarda a página de grid carregar completamente.
    /// </summary>
    public async Task WaitForPageLoadAsync()
    {
        await _page.WaitForSelectorAsync(".book-grid", new PageWaitForSelectorOptions
        {
            State = WaitForSelectorState.Visible,
            Timeout = 10000
        });

        // Aguardar pelo menos alguns cards carregarem
        await _page.WaitForFunctionAsync("document.querySelectorAll('.book-card').length >= 3");
    }

    /// <summary>
    /// Obtém o total de livros visíveis na lista.
    /// </summary>
    public async Task<int> GetTotalBookCountAsync()
    {
        return await _page.Locator(".book-card").CountAsync();
    }

    /// <summary>
    /// Rola a página para baixo e aguarda novos livros carregarem.
    /// Retorna true se novos livros foram carregados, false caso contrário.
    /// </summary>
    /// <param name="waitTimeMs">Tempo de espera após o scroll. Padrão: 2000ms.</param>
    /// <param name="maxWaitTimeMs">Tempo máximo de espera para novos livros. Padrão: 5000ms.</param>
    private async Task<bool> ScrollToLoadMoreBooksAsync(int waitTimeMs = 2000, int maxWaitTimeMs = 5000)
    {
        var previousCount = await GetTotalBookCountAsync();

        // Scroll para o final da página
        await _page.EvaluateAsync("window.scrollTo(0, document.body.scrollHeight)");

        // Aguardar um tempo para que o lazy loading seja acionado
        await _page.WaitForTimeoutAsync(waitTimeMs);

        // Aguardar até que novos livros apareçam ou até o timeout máximo
        var startTime = DateTime.UtcNow;
        var currentCount = previousCount;

        while ((DateTime.UtcNow - startTime).TotalMilliseconds < maxWaitTimeMs)
        {
            currentCount = await GetTotalBookCountAsync();

            // Se a contagem aumentou, novos livros foram carregados
            if (currentCount > previousCount)
            {
                return true;
            }

            // Aguardar um pouco antes de verificar novamente
            await _page.WaitForTimeoutAsync(500);
        }

        // Verificar uma última vez
        currentCount = await GetTotalBookCountAsync();
        return currentCount > previousCount;
    }

    /// <summary>
    /// Rola a página até o fim do loading da listagem de livros,
    /// carregando todos os livros disponíveis via lazy loading.
    /// </summary>
    /// <param name="maxScrollAttempts">Número máximo de tentativas de scroll. Padrão: 20.</param>
    /// <param name="waitTimeMs">Tempo de espera após cada scroll. Padrão: 2000ms.</param>
    /// <param name="maxWaitTimeMs">Tempo máximo de espera por tentativa. Padrão: 5000ms.</param>
    /// <returns>O número total de livros carregados após todas as tentativas de scroll.</returns>
    public async Task<int> ScrollUntilAllBooksLoadedAsync(int maxScrollAttempts = 20, int waitTimeMs = 2000, int maxWaitTimeMs = 5000)
    {
        Console.WriteLine("Iniciando scroll para carregar todos os livros...");

        var scrollAttempts = 0;
        var initialCount = await GetTotalBookCountAsync();
        Console.WriteLine($"  Livros iniciais: {initialCount}");

        while (scrollAttempts < maxScrollAttempts)
        {
            var newBooksLoaded = await ScrollToLoadMoreBooksAsync(waitTimeMs, maxWaitTimeMs);

            if (!newBooksLoaded)
            {
                // Fazer uma última tentativa com scroll suave
                await _page.EvaluateAsync(@"
                    new Promise((resolve) => {
                        window.scrollTo({ top: document.body.scrollHeight, behavior: 'smooth' });
                        setTimeout(resolve, 1000);
                    })
                ");

                var finalCheck = await ScrollToLoadMoreBooksAsync(waitTimeMs, maxWaitTimeMs);
                if (!finalCheck)
                {
                    Console.WriteLine("  Não há mais livros para carregar.");
                    break;
                }
            }

            scrollAttempts++;
            var currentCount = await GetTotalBookCountAsync();
            Console.WriteLine($"  Scroll #{scrollAttempts}: {currentCount} livros carregados");
        }

        var totalBooks = await GetTotalBookCountAsync();
        Console.WriteLine($"✓ Total de livros carregados: {totalBooks}");

        // Rolar de volta ao topo da página
        await _page.EvaluateAsync("window.scrollTo(0, 0)");

        return totalBooks;
    }

    /// <summary>
    /// Retorna o código fonte (HTML) completo da página atual.
    /// </summary>
    /// <returns>O HTML completo da página.</returns>
    public async Task<string> GetPageSourceAsync()
    {
        return await _page.ContentAsync();
    }

}

