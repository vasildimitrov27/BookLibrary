namespace BookLibrary;

public class Author
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Country { get; set; } = "";
}

public class Book
{
    public int Id { get; set; }
    public string Title { get; set; } = "";
    public int AuthorId { get; set; }
    public string AuthorName { get; set; } = "";
    public int Year { get; set; }
}

/// <summary>Резултат от търсене в Open Library.</summary>
public record CatalogItem(string Title, string Author, int Year);
