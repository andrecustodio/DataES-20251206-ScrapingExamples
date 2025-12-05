using Microsoft.Playwright;

// URL da página estática de grid (será iniciada via docker-compose)
// Em um ambiente real, você obteria esta URL de configuração ou variáveis de ambiente
const string StaticGridUrl = "http://localhost:8080";

Console.WriteLine("=== Exercício 01: Configuração Básica ===");
Console.WriteLine("Este exercício demonstra:");
Console.WriteLine("- Inicialização do navegador usando Playwright diretamente");
Console.WriteLine("- Navegação para uma página");
Console.WriteLine("- Interações básicas com a página");
Console.WriteLine();

try
{
    // Criar uma instância do Playwright
    Console.WriteLine("Inicializando o Playwright...");
    using var playwright = await Playwright.CreateAsync();
    Console.WriteLine("✓ Playwright inicializado com sucesso!");
    Console.WriteLine();

    // Lançar o navegador Chromium em modo não-headless (com interface gráfica)
    // Para executar em modo headless, altere Headless para true
    Console.WriteLine("Lançando o navegador Chromium...");
    await using var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
    {
        Headless = false
    });
    Console.WriteLine("✓ Navegador lançado com sucesso!");
    Console.WriteLine();

    // Criar uma nova página no navegador
    Console.WriteLine("Criando uma nova página...");
    var page = await browser.NewPageAsync();
    Console.WriteLine("✓ Página criada com sucesso!");
    Console.WriteLine();

    // Navegar para a URL da página estática
    Console.WriteLine($"Navegando para: {StaticGridUrl}");
    await page.GotoAsync(StaticGridUrl, new PageGotoOptions
    {
        WaitUntil = WaitUntilState.NetworkIdle // Aguarda até que a rede esteja inativa
    });
    Console.WriteLine("✓ Navegação concluída!");
    Console.WriteLine();

    // Obter o título da página
    var title = await page.TitleAsync();
    Console.WriteLine($"Título da página: {title}");
    Console.WriteLine();

    // Obter a URL atual
    var url = page.Url;
    Console.WriteLine($"URL atual: {url}");
    Console.WriteLine();

    // Aguardar alguns segundos para visualizar (em modo headless, isso não é necessário,
    // mas é útil para demonstração)
    Console.WriteLine("Aguardando 2 segundos...");
    await page.WaitForTimeoutAsync(2000);
    Console.WriteLine();

    // Fazer uma captura de tela (útil para debug e documentação)
    Console.WriteLine("Capturando screenshot...");
    await page.ScreenshotAsync(new PageScreenshotOptions
    {
        Path = "screenshot.png",
        FullPage = true
    });
    Console.WriteLine("✓ Screenshot salvo como 'screenshot.png'");
    Console.WriteLine();

    Console.WriteLine("=== Exercício concluído com sucesso! ===");
}
catch (Exception ex)
{
    Console.WriteLine($"Erro durante a execução: {ex.Message}");
    Console.WriteLine($"Detalhes: {ex}");
    Environment.Exit(1);
}
