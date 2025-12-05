/// <summary>
/// Classe para representar informações de um livro.
/// </summary>
class BookInfo
{
    public string Key { get; set; } = "";
    public string Title { get; set; } = "";
    public List<string> Authors { get; set; } = new();
    public string Isbn { get; set; } = "";
}

