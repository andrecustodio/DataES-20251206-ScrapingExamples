using System.Text.Json.Serialization;

namespace PlaywrightHelpers.Dtos;

/// <summary>
/// DTO que representa um livro na resposta da API.
/// </summary>
public class BookDto
{
    /// <summary>
    /// Chave única do livro.
    /// </summary>
    [JsonPropertyName("key")]
    public string? Key { get; set; }

    /// <summary>
    /// Título do livro.
    /// </summary>
    [JsonPropertyName("title")]
    public string? Title { get; set; }

    /// <summary>
    /// Lista de autores do livro.
    /// </summary>
    [JsonPropertyName("authors")]
    public List<AuthorDto>? Authors { get; set; }

    /// <summary>
    /// Lista de números ISBN do livro.
    /// </summary>
    [JsonPropertyName("isbn")]
    public List<string>? Isbn { get; set; }

    /// <summary>
    /// Data de publicação do livro.
    /// </summary>
    [JsonPropertyName("publish_date")]
    public string? PublishDate { get; set; }

    /// <summary>
    /// Número de páginas do livro.
    /// </summary>
    [JsonPropertyName("number_of_pages")]
    public int? NumberOfPages { get; set; }

    /// <summary>
    /// URLs das capas do livro em diferentes tamanhos.
    /// </summary>
    [JsonPropertyName("cover")]
    public CoverDto? Cover { get; set; }

    /// <summary>
    /// Lista de assuntos/temas do livro.
    /// </summary>
    [JsonPropertyName("subjects")]
    public List<string>? Subjects { get; set; }

    /// <summary>
    /// Descrição do livro.
    /// </summary>
    [JsonPropertyName("description")]
    public string? Description { get; set; }

    /// <summary>
    /// Primeira frase do livro.
    /// </summary>
    [JsonPropertyName("first_sentence")]
    public string? FirstSentence { get; set; }

    /// <summary>
    /// Código do idioma do livro (ex: "por" para português).
    /// </summary>
    [JsonPropertyName("language")]
    public string? Language { get; set; }
}

