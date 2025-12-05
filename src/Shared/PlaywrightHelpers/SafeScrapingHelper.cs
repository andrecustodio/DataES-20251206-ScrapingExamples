using Microsoft.Playwright;

namespace PlaywrightHelpers;

/// <summary>
/// Classe auxiliar para operações de web scraping seguras com lógica de retry, tratamento de erros e captura de screenshots.
/// </summary>
public static class SafeScrapingHelper
{
    /// <summary>
    /// Executa uma operação de scraping com lógica automática de retry, tratamento de erros e captura de screenshot em caso de falha.
    /// </summary>
    /// <typeparam name="T">O tipo de retorno da operação.</typeparam>
    /// <param name="page">A instância da página do Playwright.</param>
    /// <param name="operation">A operação a ser executada.</param>
    /// <param name="operationName">Um nome descritivo para a operação (usado em logs e mensagens de erro).</param>
    /// <param name="maxRetries">Número máximo de tentativas de retry. Padrão: 3.</param>
    /// <param name="getRetryDelayMs">Função opcional para calcular o delay entre retries. Parâmetros: número da tentativa, exceção. Retorna delay em milissegundos. Se null, usa backoff exponencial (1000ms, 2000ms, 4000ms...).</param>
    /// <param name="onSuccess">Callback opcional invocado quando a operação é bem-sucedida. Parâmetros: operationName, número da tentativa.</param>
    /// <param name="onRetry">Callback opcional invocado quando um retry é necessário. Parâmetros: operationName, número da tentativa, delayMs, exceção.</param>
    /// <param name="onFinalFailure">Callback opcional invocado quando todas as tentativas são esgotadas. Parâmetros: operationName, maxRetries, tentativa final, exceção, caminho do screenshot.</param>
    /// <returns>O resultado da operação.</returns>
    /// <exception cref="Exception">Lançada quando a operação falha após todas as tentativas de retry.</exception>
    public static async Task<T> SafeScrapingOperationAsync<T>(
        IPage page,
        Func<Task<T>> operation,
        string operationName,
        int maxRetries = 3,
        Func<int, Exception, int>? getRetryDelayMs = null,
        Action<string, int>? onSuccess = null,
        Action<string, int, int, Exception>? onRetry = null,
        Action<string, int, int, Exception, string>? onFinalFailure = null)
    {
        int attempt = 0;
        Exception? lastException = null;

        while (attempt < maxRetries)
        {
            try
            {
                var result = await operation();
                onSuccess?.Invoke(operationName, attempt + 1);
                return result;
            }
            catch (Exception ex)
            {
                lastException = ex;
                attempt++;

                if (attempt < maxRetries)
                {
                    var delay = getRetryDelayMs?.Invoke(attempt, ex) ?? (1000 * (int)Math.Pow(2, attempt - 1));
                    onRetry?.Invoke(operationName, attempt, delay, ex);
                    await Task.Delay(delay);
                }
                else
                {
                    var screenshotPath = $"final-error-{operationName}-{DateTime.Now:yyyyMMddHHmmss}.png";
                    try
                    {
                        await page.ScreenshotAsync(new PageScreenshotOptions
                        {
                            Path = screenshotPath,
                            FullPage = true
                        });
                    }
                    catch { }

                    onFinalFailure?.Invoke(operationName, maxRetries, attempt, ex, screenshotPath);
                }
            }
        }

        throw new Exception($"Operação {operationName} falhou após {maxRetries} tentativas", lastException);
    }

    /// <summary>
    /// Executa uma operação de scraping com lógica automática de retry, tratamento de erros e captura de screenshot em caso de falha.
    /// Esta sobrecarga é para operações que não retornam valor.
    /// </summary>
    /// <param name="page">A instância da página do Playwright.</param>
    /// <param name="operation">A operação a ser executada.</param>
    /// <param name="operationName">Um nome descritivo para a operação (usado em logs e mensagens de erro).</param>
    /// <param name="maxRetries">Número máximo de tentativas de retry. Padrão: 3.</param>
    /// <param name="getRetryDelayMs">Função opcional para calcular o delay entre retries. Parâmetros: número da tentativa, exceção. Retorna delay em milissegundos. Se null, usa backoff exponencial (1000ms, 2000ms, 4000ms...).</param>
    /// <param name="onSuccess">Callback opcional invocado quando a operação é bem-sucedida. Parâmetros: operationName, número da tentativa.</param>
    /// <param name="onRetry">Callback opcional invocado quando um retry é necessário. Parâmetros: operationName, número da tentativa, delayMs, exceção.</param>
    /// <param name="onFinalFailure">Callback opcional invocado quando todas as tentativas são esgotadas. Parâmetros: operationName, maxRetries, tentativa final, exceção, caminho do screenshot.</param>
    /// <exception cref="Exception">Lançada quando a operação falha após todas as tentativas de retry.</exception>
    public static async Task SafeScrapingOperationAsync(
        IPage page,
        Func<Task> operation,
        string operationName,
        int maxRetries = 3,
        Func<int, Exception, int>? getRetryDelayMs = null,
        Action<string, int>? onSuccess = null,
        Action<string, int, int, Exception>? onRetry = null,
        Action<string, int, int, Exception, string>? onFinalFailure = null)
    {
        await SafeScrapingOperationAsync<bool>(
            page,
            async () =>
            {
                await operation();
                return true;
            },
            operationName,
            maxRetries,
            getRetryDelayMs,
            onSuccess,
            onRetry,
            onFinalFailure);
    }
}
