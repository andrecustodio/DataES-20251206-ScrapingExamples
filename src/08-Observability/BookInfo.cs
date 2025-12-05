using System.Text.Json.Serialization;

namespace Observability;

/// <summary>
/// Classe para representar informações de um livro extraído.
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

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("publish_date")]
    public string? PublishDate { get; set; }

    [JsonPropertyName("number_of_pages")]
    public int? NumberOfPages { get; set; }
}

