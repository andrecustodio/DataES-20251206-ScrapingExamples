using Microsoft.Playwright;

namespace Observability;

/// <summary>
/// Page Object Model para a página de lista de livros.
/// Encapsula todas as interações e seletores relacionados a esta página.
/// </summary>
public class BookListPage
{
    private readonly IPage _page;

    public BookListPage(IPage page)
    {
        _page = page;
    }

    /// <summary>
    /// Aguarda a página de lista carregar completamente.
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
    /// Obtém o título de um livro específico pelo índice.
    /// </summary>
    public async Task<string> GetBookTitleAsync(int index)
    {
        var bookCard = _page.Locator(".book-card").Nth(index);
        var title = await bookCard.Locator(".book-title").TextContentAsync();
        return title?.Trim() ?? "";
    }

    /// <summary>
    /// Clica em um livro específico e retorna sua key.
    /// </summary>
    public async Task<string> ClickBookAsync(int index)
    {
        var bookCard = _page.Locator(".book-card").Nth(index);
        var key = await bookCard.GetAttributeAsync("data-key") ?? "";
        
        await bookCard.ClickAsync();
        
        // Aguardar navegação
        await _page.WaitForURLAsync($"**/book/**", new PageWaitForURLOptions
        {
            Timeout = 10000
        });
        
        return key;
    }

    /// <summary>
    /// Extrai informações básicas de múltiplos livros da lista.
    /// </summary>
    public async Task<List<BookInfo>> GetBooksFromListAsync(int maxCount = 10)
    {
        var books = new List<BookInfo>();
        var count = await GetTotalBookCountAsync();
        var actualCount = Math.Min(count, maxCount);

        for (int i = 0; i < actualCount; i++)
        {
            var bookCard = _page.Locator(".book-card").Nth(i);
            
            var title = await bookCard.Locator(".book-title").TextContentAsync();
            var key = await bookCard.GetAttributeAsync("data-key");
            var authorElements = await bookCard.Locator(".book-author").AllTextContentsAsync();
            
            books.Add(new BookInfo
            {
                Key = key ?? "",
                Title = title?.Trim() ?? "",
                Authors = authorElements.Select(a => a.Trim()).ToList()
            });
        }

        return books;
    }

    /// <summary>
    /// Rola a página para baixo para carregar mais livros via lazy loading.
    /// Retorna true se novos livros foram carregados, false caso contrário.
    /// </summary>
    public async Task<bool> ScrollToLoadMoreBooksAsync(int waitTimeMs = 1500, int maxWaitTimeMs = 5000)
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
            
            if (currentCount > previousCount)
            {
                return true;
            }
            
            await _page.WaitForTimeoutAsync(500);
        }
        
        currentCount = await GetTotalBookCountAsync();
        return currentCount > previousCount;
    }

    /// <summary>
    /// Rola a página múltiplas vezes para carregar todos os livros disponíveis.
    /// </summary>
    public async Task<int> ScrollToLoadAllBooksAsync(int maxScrollAttempts = 15)
    {
        var scrollAttempts = 0;
        
        while (scrollAttempts < maxScrollAttempts)
        {
            var newBooksLoaded = await ScrollToLoadMoreBooksAsync();
            
            if (!newBooksLoaded)
            {
                break;
            }
            
            scrollAttempts++;
        }
        
        return await GetTotalBookCountAsync();
    }

    /// <summary>
    /// Retorna o código fonte (HTML) completo da página atual.
    /// </summary>
    public async Task<string> GetPageSourceAsync()
    {
        return await _page.ContentAsync();
    }
}

