using Microsoft.Data.Sqlite;

namespace BookLibrary;

/// <summary>Достъп до SQLite базата (library.db до .exe файла).</summary>
public static class Db
{
    public static readonly string DbPath =
        System.IO.Path.Combine(AppContext.BaseDirectory, "library.db");

    private const string Schema = """
        CREATE TABLE IF NOT EXISTS Authors (
            Id      INTEGER PRIMARY KEY AUTOINCREMENT,
            Name    TEXT NOT NULL UNIQUE,
            Country TEXT NOT NULL DEFAULT ''
        );
        CREATE TABLE IF NOT EXISTS Books (
            Id       INTEGER PRIMARY KEY AUTOINCREMENT,
            Title    TEXT NOT NULL,
            AuthorId INTEGER NOT NULL,
            Year     INTEGER NOT NULL,
            FOREIGN KEY (AuthorId) REFERENCES Authors(Id) ON DELETE RESTRICT
        );
        """;

    private static SqliteConnection Open()
    {
        var c = new SqliteConnection($"Data Source={DbPath};Foreign Keys=True");
        c.Open();
        return c;
    }

    private static int Exec(string sql, params (string Name, object? Value)[] p)
    {
        using var c = Open();
        using var cmd = c.CreateCommand();
        cmd.CommandText = sql;
        foreach (var (n, v) in p) cmd.Parameters.AddWithValue(n, v ?? DBNull.Value);
        return cmd.ExecuteNonQuery();
    }

    public static void Init()
    {
        Exec(Schema);

        using var c = Open();
        using var cmd = c.CreateCommand();
        cmd.CommandText = "SELECT COUNT(*) FROM Authors";
        if ((long)cmd.ExecuteScalar()! > 0) return;

        // Примерни данни при първо стартиране
        int vazov = AddAuthor(new Author { Name = "Иван Вазов", Country = "България" });
        int yovkov = AddAuthor(new Author { Name = "Йордан Йовков", Country = "България" });
        AddBook(new Book { Title = "Под игото", AuthorId = vazov, Year = 1894 });
        AddBook(new Book { Title = "Старопланински легенди", AuthorId = yovkov, Year = 1927 });
    }

    // ---------- Автори ----------
    public static List<Author> GetAuthors()
    {
        var list = new List<Author>();
        using var c = Open();
        using var cmd = c.CreateCommand();
        cmd.CommandText = "SELECT Id, Name, Country FROM Authors ORDER BY Name";
        using var r = cmd.ExecuteReader();
        while (r.Read())
            list.Add(new Author { Id = r.GetInt32(0), Name = r.GetString(1), Country = r.GetString(2) });
        return list;
    }

    public static int AddAuthor(Author a)
    {
        using var c = Open();
        using var cmd = c.CreateCommand();
        cmd.CommandText = "INSERT INTO Authors (Name, Country) VALUES ($n, $c); SELECT last_insert_rowid();";
        cmd.Parameters.AddWithValue("$n", a.Name);
        cmd.Parameters.AddWithValue("$c", a.Country);
        return (int)(long)cmd.ExecuteScalar()!;
    }

    public static void UpdateAuthor(Author a) =>
        Exec("UPDATE Authors SET Name = $n, Country = $c WHERE Id = $id",
            ("$n", a.Name), ("$c", a.Country), ("$id", a.Id));

    public static void DeleteAuthor(int id) =>
        Exec("DELETE FROM Authors WHERE Id = $id", ("$id", id));

    /// <summary>Връща Id на автор с това име; ако няма такъв, го създава.</summary>
    public static int GetOrCreateAuthor(string name)
    {
        name = name.Trim();
        var existing = GetAuthors().FirstOrDefault(a =>
            string.Equals(a.Name, name, StringComparison.CurrentCultureIgnoreCase));
        return existing?.Id ?? AddAuthor(new Author { Name = name });
    }

    // ---------- Книги ----------
    /// <summary>
    /// Търсенето по заглавие е в C#, защото SQLite LIKE не е нечувствителен
    /// към регистъра за кирилица.
    /// </summary>
    public static List<Book> GetBooks(string titleSearch = "")
    {
        var list = new List<Book>();
        using var c = Open();
        using var cmd = c.CreateCommand();
        cmd.CommandText = """
            SELECT b.Id, b.Title, b.AuthorId, a.Name, b.Year
            FROM Books b JOIN Authors a ON a.Id = b.AuthorId
            ORDER BY b.Title
            """;
        using var r = cmd.ExecuteReader();
        while (r.Read())
        {
            var b = new Book
            {
                Id = r.GetInt32(0), Title = r.GetString(1), AuthorId = r.GetInt32(2),
                AuthorName = r.GetString(3), Year = r.GetInt32(4)
            };
            if (string.IsNullOrWhiteSpace(titleSearch) ||
                b.Title.Contains(titleSearch, StringComparison.CurrentCultureIgnoreCase))
                list.Add(b);
        }
        return list;
    }

    public static void AddBook(Book b) =>
        Exec("INSERT INTO Books (Title, AuthorId, Year) VALUES ($t, $a, $y)",
            ("$t", b.Title), ("$a", b.AuthorId), ("$y", b.Year));

    public static void UpdateBook(Book b) =>
        Exec("UPDATE Books SET Title = $t, AuthorId = $a, Year = $y WHERE Id = $id",
            ("$t", b.Title), ("$a", b.AuthorId), ("$y", b.Year), ("$id", b.Id));

    public static void DeleteBook(int id) =>
        Exec("DELETE FROM Books WHERE Id = $id", ("$id", id));
}
