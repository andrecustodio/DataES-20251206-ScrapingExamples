using System.Text.Json.Serialization;

namespace PlaywrightHelpers.Dtos;

/// <summary>
/// DTO que representa as URLs de capa de um livro em diferentes tamanhos.
/// </summary>
public class CoverDto
{
    /// <summary>
    /// URL da capa em tamanho pequeno.
    /// </summary>
    [JsonPropertyName("small")]
    public string? Small { get; set; }

    /// <summary>
    /// URL da capa em tamanho médio.
    /// </summary>
    [JsonPropertyName("medium")]
    public string? Medium { get; set; }

    /// <summary>
    /// URL da capa em tamanho grande.
    /// </summary>
    [JsonPropertyName("large")]
    public string? Large { get; set; }
}

