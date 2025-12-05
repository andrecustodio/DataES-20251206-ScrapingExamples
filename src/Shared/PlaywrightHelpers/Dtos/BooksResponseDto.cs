using System.Text.Json.Serialization;

namespace PlaywrightHelpers.Dtos;

/// <summary>
/// DTO que representa a resposta da API /api/books com paginação.
/// </summary>
public class BooksResponseDto
{
    /// <summary>
    /// Lista de livros retornados na resposta.
    /// </summary>
    [JsonPropertyName("data")]
    public List<BookDto>? Data { get; set; }

    /// <summary>
    /// Total de livros disponíveis.
    /// </summary>
    [JsonPropertyName("total")]
    public int? Total { get; set; }

    /// <summary>
    /// Número da página atual.
    /// </summary>
    [JsonPropertyName("page")]
    public int? Page { get; set; }

    /// <summary>
    /// Limite de itens por página.
    /// </summary>
    [JsonPropertyName("limit")]
    public int? Limit { get; set; }

    /// <summary>
    /// Indica se há mais páginas disponíveis.
    /// </summary>
    [JsonPropertyName("hasMore")]
    public bool? HasMore { get; set; }
}

