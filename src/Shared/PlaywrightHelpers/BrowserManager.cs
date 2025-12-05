using Microsoft.Playwright;

namespace PlaywrightHelpers;

/// <summary>
/// Gerencia o ciclo de vida do navegador e fornece objetos IPage para as aplicações que o utilizam.
/// Esta classe encapsula a inicialização, criação de páginas e limpeza de recursos do Playwright.
/// </summary>
public class BrowserManager : IDisposable, IAsyncDisposable
{
    private IPlaywright? _playwright;
    private IBrowser? _browser;
    private bool _disposed = false;

    /// <summary>
    /// Inicializa o Playwright e lança o navegador Chromium em modo headless.
    /// </summary>
    /// <param name="headless">Se true, o navegador será executado em modo headless (sem interface gráfica). Padrão: true.</param>
    /// <param name="browserType">Tipo de navegador a ser usado. Padrão: Chromium.</param>
    /// <returns>Task que representa a operação assíncrona.</returns>
    public async Task InitializeAsync(bool headless = true, BrowserType browserType = BrowserType.Chromium)
    {
        if (_playwright != null)
        {
            return; // Já inicializado
        }

        _playwright = await Playwright.CreateAsync();

        var browserTypeOption = browserType switch
        {
            BrowserType.Chromium => _playwright.Chromium,
            BrowserType.Firefox => _playwright.Firefox,
            BrowserType.WebKit => _playwright.Webkit,
            _ => _playwright.Chromium
        };

        _browser = await browserTypeOption.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Headless = headless
        });
    }

    /// <summary>
    /// Cria uma nova página no navegador.
    /// </summary>
    /// <returns>Uma instância de IPage que representa a nova página.</returns>
    /// <exception cref="InvalidOperationException">Lança exceção se o navegador não foi inicializado.</exception>
    public async Task<IPage> CreatePageAsync()
    {
        if (_browser == null)
        {
            throw new InvalidOperationException("O navegador não foi inicializado. Chame InitializeAsync() primeiro.");
        }

        return await _browser.NewPageAsync();
    }

    /// <summary>
    /// Obtém uma página existente ou cria uma nova se não houver páginas abertas.
    /// </summary>
    /// <returns>Uma instância de IPage.</returns>
    public async Task<IPage> GetPageAsync()
    {
        if (_browser == null)
        {
            await InitializeAsync();
        }

        var pages = _browser?.Contexts.SelectMany(c => c.Pages).ToList() ?? new List<IPage>();
        
        if (pages.Any())
        {
            return pages.First();
        }

        return await CreatePageAsync();
    }

    /// <summary>
    /// Libera todos os recursos do navegador e do Playwright de forma assíncrona.
    /// </summary>
    public async ValueTask DisposeAsync()
    {
        if (!_disposed)
        {
            if (_browser != null)
            {
                await _browser.CloseAsync();
                _browser = null;
            }
            _playwright?.Dispose();
            _playwright = null;
            _disposed = true;
        }
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Libera todos os recursos do navegador e do Playwright.
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Implementação do padrão Dispose para liberação de recursos.
    /// </summary>
    /// <param name="disposing">Indica se os recursos gerenciados devem ser liberados.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed && disposing)
        {
            _browser?.DisposeAsync().AsTask().Wait();
            _playwright?.Dispose();
            _disposed = true;
        }
    }
}

/// <summary>
/// Enumeração dos tipos de navegador suportados pelo Playwright.
/// </summary>
public enum BrowserType
{
    /// <summary>
    /// Navegador Chromium (baseado no Chrome).
    /// </summary>
    Chromium,

    /// <summary>
    /// Navegador Firefox.
    /// </summary>
    Firefox,

    /// <summary>
    /// Navegador WebKit (baseado no Safari).
    /// </summary>
    WebKit
}


