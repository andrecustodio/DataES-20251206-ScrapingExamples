using PlaywrightHelpers;
using Microsoft.Playwright;

// URL da aplicação Vue.js
const string VueGridUrl = "http://localhost:8000";

Console.WriteLine("=== Exercício 06: Page Object Model (POM) ===");
Console.WriteLine("Este exercício demonstra:");
Console.WriteLine("- Implementação do padrão Page Object Model");
Console.WriteLine("- Encapsulamento de lógica de página em classes");
Console.WriteLine("- Reutilização de código e manutenibilidade");
Console.WriteLine("- Page factories e helpers");
Console.WriteLine();

using var browserManager = new BrowserManager();

try
{
    await browserManager.InitializeAsync(headless: false);
    var page = await browserManager.CreatePageAsync();

    // Navegar para a página inicial
    Console.WriteLine($"Navegando para: {VueGridUrl}");
    await page.GotoAsync(VueGridUrl, new PageGotoOptions
    {
        WaitUntil = WaitUntilState.NetworkIdle
    });
    Console.WriteLine("✓ Página carregada");
    Console.WriteLine();

    // ==========================================
    // Parte 1: Usando Page Objects
    // ==========================================
    Console.WriteLine("--- Parte 1: Usando Page Objects ---");

    // Criar instância da página de lista de livros
    var bookListPage = new BookListPage(page);
    
    // Aguardar página carregar
    await bookListPage.WaitForPageLoadAsync();
    Console.WriteLine("✓ Página de lista carregada");
    Console.WriteLine();

    // Usar métodos da página
    var totalBooks = await bookListPage.GetTotalBookCountAsync();
    Console.WriteLine($"Total de livros na lista: {totalBooks}");
    
    var firstBookTitle = await bookListPage.GetBookTitleAsync(0);
    Console.WriteLine($"Título do primeiro livro: {firstBookTitle}");
    Console.WriteLine();

    // ==========================================
    // Parte 2: Navegação entre Páginas
    // ==========================================
    Console.WriteLine("--- Parte 2: Navegação entre Páginas ---");

    // Clicar no primeiro livro para ver detalhes
    var bookKey = await bookListPage.ClickBookAsync(0);
    Console.WriteLine($"Clicado no livro com key: {bookKey}");
    Console.WriteLine();

    // Criar instância da página de detalhes
    var bookDetailPage = new BookDetailPage(page);
    await bookDetailPage.WaitForPageLoadAsync(bookKey);
    Console.WriteLine("✓ Página de detalhes carregada");
    Console.WriteLine();

    // Extrair informações detalhadas
    var detailTitle = await bookDetailPage.GetTitleAsync();
    var authors = await bookDetailPage.GetAuthorsAsync();
    var description = await bookDetailPage.GetDescriptionAsync();
    var isbn = await bookDetailPage.GetIsbnAsync();

    Console.WriteLine("Informações do livro:");
    Console.WriteLine($"  Título: {detailTitle}");
    Console.WriteLine($"  Autores: {string.Join(", ", authors)}");
    Console.WriteLine($"  ISBN: {isbn}");
    Console.WriteLine($"  Descrição: {(description?.Length > 100 ? description.Substring(0, 100) + "..." : description)}");
    Console.WriteLine();

    // ==========================================
    // Parte 3: Retornar para Lista
    // ==========================================
    Console.WriteLine("--- Parte 3: Retornando para Lista ---");

    await bookDetailPage.GoBackToListAsync();
    await bookListPage.WaitForPageLoadAsync();
    Console.WriteLine("✓ Retornado para a lista");
    Console.WriteLine();

    // ==========================================
    // Parte 4: Carregar Mais Livros via Scroll
    // ==========================================
    Console.WriteLine("--- Parte 4: Carregando Mais Livros via Scroll ---");

    var initialCount = await bookListPage.GetTotalBookCountAsync();
    Console.WriteLine($"Livros iniciais: {initialCount}");

    var totalBooksLoaded = await bookListPage.ScrollToLoadAllBooksAsync(maxScrollAttempts: 10);
    Console.WriteLine($"✓ Total de livros carregados após scroll: {totalBooksLoaded}");
    Console.WriteLine($"  Novos livros carregados: {totalBooksLoaded - initialCount}");
    Console.WriteLine();

    // ==========================================
    // Parte 5: Extração em Lote usando Page Object
    // ==========================================
    Console.WriteLine("--- Parte 5: Extração em Lote usando Page Object ---");

    var allBooks = await bookListPage.GetAllBooksAsync(25); // Obter primeiros 25
    Console.WriteLine($"Livros extraídos ({allBooks.Count}):");
    foreach (var book in allBooks)
    {
        Console.WriteLine($"  - {book.Title} (Key: {book.Key})");
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

