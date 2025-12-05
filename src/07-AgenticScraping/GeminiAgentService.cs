using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AgenticScraping;

/// <summary>
/// Serviço para interagir com a API do Gemini para extração de dados via IA.
/// </summary>
public class GeminiAgentService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly string _model;

    public GeminiAgentService(string apiKey, string model = "gemini-2.0-flash")
    {
        _apiKey = apiKey;
        _model = model;
        _httpClient = new HttpClient();
    }

    /// <summary>
    /// Extrai dados de livros do HTML da página usando o Gemini.
    /// </summary>
    /// <param name="pageHtml">O código HTML da página.</param>
    /// <param name="schema">O schema da classe BookInfo.</param>
    /// <returns>Lista de livros extraídos pelo Gemini.</returns>
    public async Task<List<BookInfo>> ExtractBooksFromHtmlAsync(string pageHtml, string schema)
    {
        var prompt = BuildPrompt(pageHtml, schema);
        var response = await CallGeminiAsync(prompt);
        return ParseBooksFromResponse(response);
    }

    private string BuildPrompt(string pageHtml, string schema)
    {
        return $"""
            Você é um agente especializado em web scraping e extração de dados estruturados.
            
            ## Tarefa
            Analise o código HTML da página fornecido abaixo e extraia todas as informações de livros que encontrar.
            
            ## Schema de Saída
            Retorne os dados no formato JSON seguindo exatamente este schema C#:
            
            ```csharp
            {schema}
            ```
            
            ## Regras
            1. Retorne APENAS um array JSON válido, sem markdown, sem explicações, sem código.
            2. Extraia todos os livros que encontrar na página.
            3. Para o campo "Authors", extraia os nomes dos autores como uma lista de strings.
            4. Para o campo "Isbn", extraia o primeiro ISBN encontrado (ou string vazia se não houver).
            5. Se algum campo não for encontrado, use string vazia "" ou lista vazia [].
            6. O campo "Key" deve conter o identificador único do livro (data-key ou similar).
            
            ## Código HTML da Página
            ```html
            {pageHtml}
            ```
            
            ## Resposta
            Retorne apenas o array JSON com os livros extraídos:
            """;
    }

    private async Task<string> CallGeminiAsync(string prompt)
    {
        var url = $"https://generativelanguage.googleapis.com/v1beta/models/{_model}:generateContent?key={_apiKey}";

        var requestBody = new GeminiRequest
        {
            Contents = new List<GeminiContent>
            {
                new GeminiContent
                {
                    Parts = new List<GeminiPart>
                    {
                        new GeminiPart { Text = prompt }
                    }
                }
            },
            GenerationConfig = new GeminiGenerationConfig
            {
                Temperature = 0.1,
                MaxOutputTokens = 8192,
                ResponseMimeType = "application/json"
            }
        };

        var json = JsonSerializer.Serialize(requestBody);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync(url, content);
        var responseBody = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            throw new Exception($"Erro na chamada ao Gemini: {response.StatusCode} - {responseBody}");
        }

        var geminiResponse = JsonSerializer.Deserialize<GeminiResponse>(responseBody);
        var textResponse = geminiResponse?.Candidates?.FirstOrDefault()?.Content?.Parts?.FirstOrDefault()?.Text;

        return textResponse ?? "[]";
    }

    private List<BookInfo> ParseBooksFromResponse(string response)
    {
        try
        {
            // Limpar possíveis marcadores de código markdown
            var cleanedResponse = response
                .Replace("```json", "")
                .Replace("```", "")
                .Trim();

            var books = JsonSerializer.Deserialize<List<BookInfo>>(cleanedResponse, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            return books ?? new List<BookInfo>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro ao fazer parse da resposta: {ex.Message}");
            Console.WriteLine($"Resposta recebida: {response}");
            return new List<BookInfo>();
        }
    }
}

#region Gemini API Models

public class GeminiRequest
{
    [JsonPropertyName("contents")]
    public List<GeminiContent> Contents { get; set; } = new();

    [JsonPropertyName("generationConfig")]
    public GeminiGenerationConfig? GenerationConfig { get; set; }
}

public class GeminiContent
{
    [JsonPropertyName("parts")]
    public List<GeminiPart> Parts { get; set; } = new();
}

public class GeminiPart
{
    [JsonPropertyName("text")]
    public string Text { get; set; } = "";
}

public class GeminiGenerationConfig
{
    [JsonPropertyName("temperature")]
    public double Temperature { get; set; } = 0.1;

    [JsonPropertyName("maxOutputTokens")]
    public int MaxOutputTokens { get; set; } = 8192;

    [JsonPropertyName("responseMimeType")]
    public string ResponseMimeType { get; set; } = "application/json";
}

public class GeminiResponse
{
    [JsonPropertyName("candidates")]
    public List<GeminiCandidate>? Candidates { get; set; }
}

public class GeminiCandidate
{
    [JsonPropertyName("content")]
    public GeminiContent? Content { get; set; }
}

#endregion