using System.Text.Json.Serialization;

namespace AgenticScraping;

/// <summary>
/// Classe para representar informações de um livro extraído pelo agente.
/// </summary>
public class BookInfo
{
    [JsonPropertyName("key")]
    public string Key { get; set; } = "";

    [JsonPropertyName("title")]
    public string Title { get; set; } = "";

    [JsonPropertyName("authors")]
    public List<string> Authors { get; set; } = new();

    [JsonPropertyName("isbn")]
    public string Isbn { get; set; } = "";
}

