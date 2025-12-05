using Microsoft.Playwright;

/// <summary>
/// Representa a página de detalhes de um livro.
/// Encapsula todas as interações e seletores relacionados a esta página.
/// </summary>
class BookDetailPage
{
    private readonly IPage _page;

    public BookDetailPage(IPage page)
    {
        _page = page;
    }

    /// <summary>
    /// Aguarda a página de detalhes carregar para um livro específico.
    /// </summary>
    public async Task WaitForPageLoadAsync(string bookKey)
    {
        // Aguarda a URL conter /book/ (a key será codificada pelo Vue router)
        // Usando um padrão flexível pois a key pode conter barras que serão codificadas
        await _page.WaitForURLAsync("**/book/**", new PageWaitForURLOptions
        {
            Timeout = 10000
        });
        
        // Aguarda o conteúdo da página de detalhes carregar
        await _page.WaitForSelectorAsync(".book-detail-title", new PageWaitForSelectorOptions
        {
            State = WaitForSelectorState.Visible,
            Timeout = 10000
        });
    }

    /// <summary>
    /// Obtém o título do livro.
    /// </summary>
    public async Task<string> GetTitleAsync()
    {
        var title = await _page.Locator(".book-detail-title").TextContentAsync();
        return title?.Trim() ?? "";
    }

    /// <summary>
    /// Obtém a lista de autores do livro.
    /// </summary>
    public async Task<List<string>> GetAuthorsAsync()
    {
        var authorElements = await _page.Locator(".book-detail-authors .author-name").AllTextContentsAsync();
        return authorElements.Select(a => a.Trim()).ToList();
    }

    /// <summary>
    /// Obtém a descrição do livro.
    /// </summary>
    public async Task<string?> GetDescriptionAsync()
    {
        var description = await _page.Locator(".book-detail-description").TextContentAsync();
        return description?.Trim();
    }

    /// <summary>
    /// Obtém o ISBN do livro.
    /// </summary>
    public async Task<string> GetIsbnAsync()
    {
        var isbn = await _page.Locator(".book-detail-isbn").TextContentAsync();
        return isbn?.Trim() ?? "";
    }

    /// <summary>
    /// Retorna para a página de lista.
    /// </summary>
    public async Task GoBackToListAsync()
    {
        await _page.GoBackAsync();
        await _page.WaitForSelectorAsync(".book-grid", new PageWaitForSelectorOptions
        {
            State = WaitForSelectorState.Visible
        });
    }
}

