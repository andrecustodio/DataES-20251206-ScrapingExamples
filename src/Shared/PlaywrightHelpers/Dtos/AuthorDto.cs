using System.Text.Json.Serialization;

namespace PlaywrightHelpers.Dtos;

/// <summary>
/// DTO que representa um autor de livro na resposta da API.
/// </summary>
public class AuthorDto
{
    /// <summary>
    /// Chave única do autor.
    /// </summary>
    [JsonPropertyName("key")]
    public string? Key { get; set; }

    /// <summary>
    /// Nome do autor.
    /// </summary>
    [JsonPropertyName("name")]
    public string? Name { get; set; }
}

