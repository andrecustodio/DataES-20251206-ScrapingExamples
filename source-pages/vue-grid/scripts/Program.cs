using System.Text.Json;
using System.Text.Json.Serialization;

// Lista de livros brasileiros famosos para gerar dados de exemplo
var books = new[]
{
    new { Title = "Dom Casmurro", Author = "Machado de Assis", Year = "1899", Pages = 256, Subjects = new[] { "Fiction", "Brazilian Literature" } },
    new { Title = "Memórias Póstumas de Brás Cubas", Author = "Machado de Assis", Year = "1881", Pages = 240, Subjects = new[] { "Fiction", "Brazilian Literature" } },
    new { Title = "Quincas Borba", Author = "Machado de Assis", Year = "1891", Pages = 288, Subjects = new[] { "Fiction", "Brazilian Literature" } },
    new { Title = "O Alienista", Author = "Machado de Assis", Year = "1882", Pages = 96, Subjects = new[] { "Fiction", "Brazilian Literature" } },
    new { Title = "A Cartomante", Author = "Machado de Assis", Year = "1884", Pages = 64, Subjects = new[] { "Fiction", "Brazilian Literature" } },
    new { Title = "O Primo Basílio", Author = "Eça de Queirós", Year = "1878", Pages = 400, Subjects = new[] { "Fiction", "Portuguese Literature" } },
    new { Title = "Os Maias", Author = "Eça de Queirós", Year = "1888", Pages = 600, Subjects = new[] { "Fiction", "Portuguese Literature" } },
    new { Title = "A Cidade e as Serras", Author = "Eça de Queirós", Year = "1901", Pages = 320, Subjects = new[] { "Fiction", "Portuguese Literature" } },
    new { Title = "O Cortiço", Author = "Aluísio Azevedo", Year = "1890", Pages = 256, Subjects = new[] { "Fiction", "Brazilian Literature" } },
    new { Title = "O Mulato", Author = "Aluísio Azevedo", Year = "1881", Pages = 280, Subjects = new[] { "Fiction", "Brazilian Literature" } },
    new { Title = "O Guarani", Author = "José de Alencar", Year = "1857", Pages = 400, Subjects = new[] { "Fiction", "Brazilian Literature", "Romance" } },
    new { Title = "Iracema", Author = "José de Alencar", Year = "1865", Pages = 200, Subjects = new[] { "Fiction", "Brazilian Literature", "Romance" } },
    new { Title = "Senhora", Author = "José de Alencar", Year = "1875", Pages = 300, Subjects = new[] { "Fiction", "Brazilian Literature", "Romance" } },
    new { Title = "O Tempo e o Vento", Author = "Erico Verissimo", Year = "1949", Pages = 800, Subjects = new[] { "Fiction", "Brazilian Literature", "Historical Fiction" } },
    new { Title = "Claro Enigma", Author = "Carlos Drummond de Andrade", Year = "1951", Pages = 150, Subjects = new[] { "Poetry", "Brazilian Literature" } },
    new { Title = "Grande Sertão: Veredas", Author = "João Guimarães Rosa", Year = "1956", Pages = 600, Subjects = new[] { "Fiction", "Brazilian Literature" } },
    new { Title = "Sagarana", Author = "João Guimarães Rosa", Year = "1946", Pages = 300, Subjects = new[] { "Fiction", "Brazilian Literature" } },
    new { Title = "Vidas Secas", Author = "Graciliano Ramos", Year = "1938", Pages = 200, Subjects = new[] { "Fiction", "Brazilian Literature" } },
    new { Title = "São Bernardo", Author = "Graciliano Ramos", Year = "1934", Pages = 250, Subjects = new[] { "Fiction", "Brazilian Literature" } },
    new { Title = "Angústia", Author = "Graciliano Ramos", Year = "1936", Pages = 220, Subjects = new[] { "Fiction", "Brazilian Literature" } },
    new { Title = "Macunaíma", Author = "Mário de Andrade", Year = "1928", Pages = 180, Subjects = new[] { "Fiction", "Brazilian Literature" } },
    new { Title = "Amar, Verbo Intransitivo", Author = "Mário de Andrade", Year = "1927", Pages = 200, Subjects = new[] { "Fiction", "Brazilian Literature" } },
    new { Title = "O Quinze", Author = "Rachel de Queiroz", Year = "1930", Pages = 250, Subjects = new[] { "Fiction", "Brazilian Literature" } },
    new { Title = "Dona Flor e Seus Dois Maridos", Author = "Jorge Amado", Year = "1966", Pages = 500, Subjects = new[] { "Fiction", "Brazilian Literature" } },
    new { Title = "Capitães da Areia", Author = "Jorge Amado", Year = "1937", Pages = 350, Subjects = new[] { "Fiction", "Brazilian Literature" } },
    new { Title = "Gabriela, Cravo e Canela", Author = "Jorge Amado", Year = "1958", Pages = 400, Subjects = new[] { "Fiction", "Brazilian Literature" } },
    new { Title = "A Hora da Estrela", Author = "Clarice Lispector", Year = "1977", Pages = 120, Subjects = new[] { "Fiction", "Brazilian Literature" } },
    new { Title = "A Paixão Segundo G.H.", Author = "Clarice Lispector", Year = "1964", Pages = 200, Subjects = new[] { "Fiction", "Brazilian Literature" } },
    new { Title = "O Lustre", Author = "Clarice Lispector", Year = "1946", Pages = 250, Subjects = new[] { "Fiction", "Brazilian Literature" } },
    new { Title = "A Maçã no Escuro", Author = "Clarice Lispector", Year = "1961", Pages = 300, Subjects = new[] { "Fiction", "Brazilian Literature" } },
    new { Title = "O Auto da Compadecida", Author = "Ariano Suassuna", Year = "1955", Pages = 150, Subjects = new[] { "Drama", "Brazilian Literature" } },
    new { Title = "A Pedra do Reino", Author = "Ariano Suassuna", Year = "1971", Pages = 600, Subjects = new[] { "Fiction", "Brazilian Literature" } },
    new { Title = "O Sertanejo", Author = "José de Alencar", Year = "1875", Pages = 350, Subjects = new[] { "Fiction", "Brazilian Literature" } },
    new { Title = "Lucíola", Author = "José de Alencar", Year = "1862", Pages = 280, Subjects = new[] { "Fiction", "Brazilian Literature", "Romance" } },
    new { Title = "A Moreninha", Author = "Joaquim Manuel de Macedo", Year = "1844", Pages = 200, Subjects = new[] { "Fiction", "Brazilian Literature", "Romance" } },
    new { Title = "O Seminarista", Author = "Bernardo Guimarães", Year = "1872", Pages = 250, Subjects = new[] { "Fiction", "Brazilian Literature" } },
    new { Title = "A Escrava Isaura", Author = "Bernardo Guimarães", Year = "1875", Pages = 300, Subjects = new[] { "Fiction", "Brazilian Literature" } },
    new { Title = "O Triste Fim de Policarpo Quaresma", Author = "Lima Barreto", Year = "1915", Pages = 350, Subjects = new[] { "Fiction", "Brazilian Literature" } },
    new { Title = "Clara dos Anjos", Author = "Lima Barreto", Year = "1948", Pages = 200, Subjects = new[] { "Fiction", "Brazilian Literature" } },
    new { Title = "Recordações do Escrivão Isaías Caminha", Author = "Lima Barreto", Year = "1909", Pages = 280, Subjects = new[] { "Fiction", "Brazilian Literature" } },
    new { Title = "O Ateneu", Author = "Raul Pompéia", Year = "1888", Pages = 250, Subjects = new[] { "Fiction", "Brazilian Literature" } },
    new { Title = "O Mulato", Author = "Aluísio Azevedo", Year = "1881", Pages = 280, Subjects = new[] { "Fiction", "Brazilian Literature" } },
    new { Title = "Casa de Pensão", Author = "Aluísio Azevedo", Year = "1884", Pages = 320, Subjects = new[] { "Fiction", "Brazilian Literature" } },
    new { Title = "O Homem", Author = "Aluísio Azevedo", Year = "1887", Pages = 200, Subjects = new[] { "Fiction", "Brazilian Literature" } },
    new { Title = "A Normalista", Author = "Adolfo Caminha", Year = "1893", Pages = 250, Subjects = new[] { "Fiction", "Brazilian Literature" } },
    new { Title = "Bom-Crioulo", Author = "Adolfo Caminha", Year = "1895", Pages = 200, Subjects = new[] { "Fiction", "Brazilian Literature" } },
    new { Title = "O Missionário", Author = "Inglês de Sousa", Year = "1888", Pages = 400, Subjects = new[] { "Fiction", "Brazilian Literature" } },
    new { Title = "O Guarani", Author = "José de Alencar", Year = "1857", Pages = 400, Subjects = new[] { "Fiction", "Brazilian Literature", "Romance" } },
    new { Title = "Iracema", Author = "José de Alencar", Year = "1865", Pages = 200, Subjects = new[] { "Fiction", "Brazilian Literature", "Romance" } },
    new { Title = "Senhora", Author = "José de Alencar", Year = "1875", Pages = 300, Subjects = new[] { "Fiction", "Brazilian Literature", "Romance" } },
    new { Title = "O Sertanejo", Author = "José de Alencar", Year = "1875", Pages = 350, Subjects = new[] { "Fiction", "Brazilian Literature" } },
    new { Title = "Lucíola", Author = "José de Alencar", Year = "1862", Pages = 280, Subjects = new[] { "Fiction", "Brazilian Literature", "Romance" } },
    new { Title = "A Moreninha", Author = "Joaquim Manuel de Macedo", Year = "1844", Pages = 200, Subjects = new[] { "Fiction", "Brazilian Literature", "Romance" } },
    new { Title = "O Seminarista", Author = "Bernardo Guimarães", Year = "1872", Pages = 250, Subjects = new[] { "Fiction", "Brazilian Literature" } },
    new { Title = "A Escrava Isaura", Author = "Bernardo Guimarães", Year = "1875", Pages = 300, Subjects = new[] { "Fiction", "Brazilian Literature" } },
    new { Title = "O Triste Fim de Policarpo Quaresma", Author = "Lima Barreto", Year = "1915", Pages = 350, Subjects = new[] { "Fiction", "Brazilian Literature" } },
    new { Title = "Clara dos Anjos", Author = "Lima Barreto", Year = "1948", Pages = 200, Subjects = new[] { "Fiction", "Brazilian Literature" } },
    new { Title = "Recordações do Escrivão Isaías Caminha", Author = "Lima Barreto", Year = "1909", Pages = 280, Subjects = new[] { "Fiction", "Brazilian Literature" } },
    new { Title = "O Ateneu", Author = "Raul Pompéia", Year = "1888", Pages = 250, Subjects = new[] { "Fiction", "Brazilian Literature" } },
    new { Title = "O Mulato", Author = "Aluísio Azevedo", Year = "1881", Pages = 280, Subjects = new[] { "Fiction", "Brazilian Literature" } },
    new { Title = "Casa de Pensão", Author = "Aluísio Azevedo", Year = "1884", Pages = 320, Subjects = new[] { "Fiction", "Brazilian Literature" } },
    new { Title = "O Homem", Author = "Aluísio Azevedo", Year = "1887", Pages = 200, Subjects = new[] { "Fiction", "Brazilian Literature" } },
    new { Title = "A Normalista", Author = "Adolfo Caminha", Year = "1893", Pages = 250, Subjects = new[] { "Fiction", "Brazilian Literature" } },
    new { Title = "Bom-Crioulo", Author = "Adolfo Caminha", Year = "1895", Pages = 200, Subjects = new[] { "Fiction", "Brazilian Literature" } },
    new { Title = "O Missionário", Author = "Inglês de Sousa", Year = "1888", Pages = 400, Subjects = new[] { "Fiction", "Brazilian Literature" } }
};

var random = new Random();
var generatedBooks = new List<Book>();

for (int i = 0; i < books.Length; i++)
{
    var book = books[i];
    var workKey = $"/works/OL{100000 + i}W";
    var authorKey = $"/authors/OL{50000 + i}A";
    
    // Gerar ISBN aleatório
    string isbn10 = $"{random.NextInt64(1000000000, 9999999999)}";
    string isbn13 = $"978{random.NextInt64(1000000000, 9999999999)}";
    
    var generatedBook = new Book
    {
        Key = workKey,
        Title = book.Title,
        Authors = new[]
        {
            new Author
            {
                Key = authorKey,
                Name = book.Author
            }
        },
        Isbn = new[] { isbn10, isbn13 },
        PublishDate = book.Year,
        NumberOfPages = book.Pages,
        Cover = new Cover
        {
            Small = $"https://covers.openlibrary.org/b/id/{10000 + i}-S.jpg",
            Medium = $"https://covers.openlibrary.org/b/id/{10000 + i}-M.jpg",
            Large = $"https://covers.openlibrary.org/b/id/{10000 + i}-L.jpg"
        },
        Subjects = book.Subjects,
        Description = $"Descrição do livro '{book.Title}' de {book.Author}. Uma obra clássica da literatura brasileira publicada em {book.Year}.",
        FirstSentence = $"Primeira frase do livro '{book.Title}' de {book.Author}.",
        Language = "por"
    };
    
    generatedBooks.Add(generatedBook);
}

// Salvar em arquivos JSON (10 livros por arquivo)
var outputDir = Path.Combine(Directory.GetCurrentDirectory(), "..", "data");
Directory.CreateDirectory(outputDir);

var booksPerFile = 10;
for (int i = 0; i < generatedBooks.Count; i += booksPerFile)
{
    var batch = generatedBooks.Skip(i).Take(booksPerFile).ToArray();
    var fileName = Path.Combine(outputDir, $"books_{i / booksPerFile + 1}.json");
    
    var options = new JsonSerializerOptions
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };
    
    var json = JsonSerializer.Serialize(batch, options);
    File.WriteAllText(fileName, json);
    
    Console.WriteLine($"Arquivo gerado: {fileName} ({batch.Length} livros)");
}

Console.WriteLine($"\nTotal de livros gerados: {generatedBooks.Count}");
Console.WriteLine($"Arquivos criados em: {outputDir}");

// Classes para serialização JSON
public class Book
{
    [JsonPropertyName("key")]
    public string Key { get; set; } = "";
    
    [JsonPropertyName("title")]
    public string Title { get; set; } = "";
    
    [JsonPropertyName("authors")]
    public Author[] Authors { get; set; } = Array.Empty<Author>();
    
    [JsonPropertyName("isbn")]
    public string[] Isbn { get; set; } = Array.Empty<string>();
    
    [JsonPropertyName("publish_date")]
    public string? PublishDate { get; set; }
    
    [JsonPropertyName("number_of_pages")]
    public int? NumberOfPages { get; set; }
    
    [JsonPropertyName("cover")]
    public Cover? Cover { get; set; }
    
    [JsonPropertyName("subjects")]
    public string[]? Subjects { get; set; }
    
    [JsonPropertyName("description")]
    public string? Description { get; set; }
    
    [JsonPropertyName("first_sentence")]
    public string? FirstSentence { get; set; }
    
    [JsonPropertyName("language")]
    public string? Language { get; set; }
}

public class Author
{
    [JsonPropertyName("key")]
    public string Key { get; set; } = "";
    
    [JsonPropertyName("name")]
    public string Name { get; set; } = "";
}

public class Cover
{
    [JsonPropertyName("small")]
    public string? Small { get; set; }
    
    [JsonPropertyName("medium")]
    public string? Medium { get; set; }
    
    [JsonPropertyName("large")]
    public string? Large { get; set; }
}
