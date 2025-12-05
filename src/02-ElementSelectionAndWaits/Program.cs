using PlaywrightHelpers;
using Microsoft.Playwright;

// URLs das páginas alvo
const string StaticGridUrl = "http://localhost:8080";
const string VueGridUrl = "http://localhost:8000";

Console.WriteLine("=== Exercício 02: Seleção de Elementos e Estratégias de Espera ===");
Console.WriteLine("Este exercício demonstra:");
Console.WriteLine("- Seletores CSS e XPath");
Console.WriteLine("- Extração de texto e atributos");
Console.WriteLine("- Estratégias de espera baseadas em seletores específicos");
Console.WriteLine("- Tratamento de conteúdo estático e dinâmico");
Console.WriteLine();

using var browserManager = new BrowserManager();

try
{
    await browserManager.InitializeAsync(headless: false);
    var page = await browserManager.CreatePageAsync();

    // ==========================================
    // Parte 1: Seletores CSS na página estática
    // ==========================================
    Console.WriteLine("--- Parte 1: Seletores CSS na Página Estática ---");
    Console.WriteLine($"Navegando para: {StaticGridUrl}");
    await page.GotoAsync(StaticGridUrl, new PageGotoOptions
    {
        WaitUntil = WaitUntilState.NetworkIdle
    });
    Console.WriteLine("✓ Página carregada");
    Console.WriteLine();

    // Seletores CSS básicos
    Console.WriteLine("1. Seletores CSS básicos:");

    // Selecionar elemento por ID
    var header = await page.QuerySelectorAsync("#page-header");
    if (header != null)
    {
        var headerText = await header.TextContentAsync();
        Console.WriteLine($"   - Cabeçalho (por ID): {headerText?.Trim()}");
    }

    // Selecionar elementos por classe
    var bookRows = await page.QuerySelectorAllAsync(".book-row");
    Console.WriteLine($"   - Linhas de livros encontradas (por classe): {bookRows.Count}");

    // Selecionar primeiro elemento de uma lista
    if (bookRows.Count > 0)
    {
        var firstBook = bookRows[0];
        var bookTitle = await firstBook.QuerySelectorAsync(".book-title");
        if (bookTitle != null)
        {
            var titleText = await bookTitle.TextContentAsync();
            Console.WriteLine($"   - Título do primeiro livro: {titleText?.Trim()}");
        }
    }
    Console.WriteLine();

    // Seletores CSS avançados
    Console.WriteLine("2. Seletores CSS avançados:");

    // Selecionar por atributo
    var isbnElements = await page.QuerySelectorAllAsync("[data-isbn]");
    Console.WriteLine($"   - Elementos com atributo data-isbn: {isbnElements.Count}");

    // Selecionar por nth-child
    var secondBook = await page.QuerySelectorAsync(".book-row:nth-child(3)");
    if (secondBook != null)
    {
        // É o segundo por que, o Parent tem 11 filhos, sendo o primeiro a barra de status, e os demais os livros.
        // Sendo assim, o (1) é a div de status, (2) é o primeiro livro, (3) é o livro em questão
        var secondBookTitle = await secondBook.QuerySelectorAsync(".book-title");
        if (secondBookTitle != null)
        {
            var secondTitleText = await secondBookTitle.TextContentAsync();
            Console.WriteLine($"   - Título do segundo livro (nth-child): {secondTitleText?.Trim()}");
        }
    }
    Console.WriteLine();

    // ==========================================
    // Parte 2: Seletores XPath
    // ==========================================
    Console.WriteLine("--- Parte 2: Seletores XPath ---");

    // XPath básico
    var xpathHeader = page.Locator("xpath=//div[@id=\"page-header\"]").First;
    var xpathHeaderText = await xpathHeader.TextContentAsync();
    Console.WriteLine($"   - Cabeçalho (XPath): {xpathHeaderText?.Trim()}");

    // XPath com condições
    var xpathBooks = page.Locator("xpath=//div[@class=\"book-row\"]");
    var xpathBookCount = await xpathBooks.CountAsync();
    Console.WriteLine($"   - Total de livros (XPath): {xpathBookCount}");

    // XPath com texto
    var xpathTitle = page.Locator("xpath=//div[@class=\"book-title\"]").First;
    var xpathTitleText = await xpathTitle.TextContentAsync();
    Console.WriteLine($"   - Primeiro título (XPath): {xpathTitleText?.Trim()}");
    Console.WriteLine();

    // ==========================================
    // Parte 3: Extração de Atributos
    // ==========================================
    Console.WriteLine("--- Parte 3: Extração de Atributos ---");

    if (bookRows.Count > 0)
    {
        var firstRow = bookRows[0];

        // Obter atributo data-isbn
        var isbn = await firstRow.GetAttributeAsync("data-isbn");
        Console.WriteLine($"   - ISBN do primeiro livro: {isbn}");

        // Obter atributo href de um link
        var link = await firstRow.QuerySelectorAsync("a");
        if (link != null)
        {
            var href = await link.GetAttributeAsync("href");
            Console.WriteLine($"   - Link do primeiro livro: {href}");
        }

        // Obter todos os atributos
        // Aqui fazemos o parse das tags para um json
        var allAttributes = await firstRow.EvaluateAsync<string>("el => JSON.stringify([...el.attributes].reduce((acc, attr) => { acc[attr.name] = attr.value; return acc; }, {}))");
        Console.WriteLine($"   - Todos os atributos: {allAttributes}");
    }
    Console.WriteLine();

    // ==========================================
    // Parte 4: Estratégias de Espera na Página Dinâmica
    // ==========================================
    Console.WriteLine("--- Parte 4: Estratégias de Espera na Página Dinâmica ---");
    Console.WriteLine($"Navegando para: {VueGridUrl}");
    await page.GotoAsync(VueGridUrl, new PageGotoOptions
    {
        WaitUntil = WaitUntilState.NetworkIdle
    });
    Console.WriteLine("✓ Página Vue.js carregada");
    Console.WriteLine();

    // Esperar por elemento aparecer
    Console.WriteLine("1. Esperando elemento aparecer:");
    var gridContainer = page.Locator(".book-grid");
    await gridContainer.WaitForAsync(new LocatorWaitForOptions
    {
        State = WaitForSelectorState.Visible,
        Timeout = 10000 // 10 segundos
    });
    Console.WriteLine("   ✓ Grid de livros apareceu");
    Console.WriteLine();

    // Esperar por elemento estar visível
    Console.WriteLine("2. Esperando elemento estar visível:");
    var firstBookCard = page.Locator(".book-card").First;
    await firstBookCard.WaitForAsync(new LocatorWaitForOptions
    {
        State = WaitForSelectorState.Visible
    });
    Console.WriteLine("   ✓ Primeiro card de livro está visível");

    var firstCardTitle = await firstBookCard.Locator(".book-title").TextContentAsync();
    Console.WriteLine($"   - Título do primeiro card: {firstCardTitle?.Trim()}");
    Console.WriteLine();

    // Esperar por atributo específico
    Console.WriteLine("3. Esperando atributo específico:");
    await page.WaitForFunctionAsync("document.querySelector('.book-card')?.getAttribute('data-key') !== null");
    Console.WriteLine("   ✓ Atributo data-key presente");

    var dataKey = await firstBookCard.GetAttributeAsync("data-key");
    Console.WriteLine($"   - Valor do data-key: {dataKey}");
    Console.WriteLine();

    // Esperar por múltiplos elementos
    Console.WriteLine("4. Esperando múltiplos elementos:");
    await page.WaitForSelectorAsync(".book-card", new PageWaitForSelectorOptions
    {
        State = WaitForSelectorState.Visible
    });

    var allCards = await page.Locator(".book-card").CountAsync();
    Console.WriteLine($"   ✓ Total de cards carregados: {allCards}");
    Console.WriteLine();

    // Esperar por condição customizada
    Console.WriteLine("5. Esperando condição customizada:");
    await page.WaitForFunctionAsync("document.querySelectorAll('.book-card').length >= 5");
    Console.WriteLine("   ✓ Pelo menos 5 cards foram carregados");

    var finalCount = await page.Locator(".book-card").CountAsync();
    Console.WriteLine($"   - Total final de cards: {finalCount}");
    Console.WriteLine();

    // ==========================================
    // Parte 5: Extração de Dados em Lote
    // ==========================================
    Console.WriteLine("--- Parte 5: Extração de Dados em Lote ---");

    // Aqui, recuperamos todas as tags que tenham essa classe específica
    var bookTitles = await page.Locator(".book-title").AllTextContentsAsync();
    Console.WriteLine($"Títulos extraídos ({bookTitles.Count}):");
    for (int i = 0; i < Math.Min(5, bookTitles.Count); i++)
    {
        Console.WriteLine($"   {i + 1}. {bookTitles[i].Trim()}");
    }
    if (bookTitles.Count > 5)
    {
        Console.WriteLine($"   ... e mais {bookTitles.Count - 5} títulos");
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
