using System.Net.Http;
using System.Text.Json;

namespace BookLibrary;

/// <summary>
/// Безплатен публичен API за каталог на книги – Open Library (https://openlibrary.org).
/// Не изисква ключ или регистрация.
/// </summary>
public static class OpenLibraryClient
{
    private static readonly HttpClient Http = CreateClient();

    private static HttpClient CreateClient()
    {
        var c = new HttpClient { Timeout = TimeSpan.FromSeconds(15) };
        c.DefaultRequestHeaders.UserAgent.ParseAdd("BookLibraryStudentApp/1.0");
        return c;
    }

    public static async Task<List<CatalogItem>> SearchAsync(string query)
    {
        var url = "https://openlibrary.org/search.json?q=" + Uri.EscapeDataString(query) +
                  "&fields=title,author_name,first_publish_year&limit=25";

        using var response = await Http.GetAsync(url);
        response.EnsureSuccessStatusCode();
        using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());

        var result = new List<CatalogItem>();
        if (!doc.RootElement.TryGetProperty("docs", out var docs)) return result;

        foreach (var d in docs.EnumerateArray())
        {
            if (!d.TryGetProperty("title", out var t) || string.IsNullOrWhiteSpace(t.GetString()))
                continue;

            string author = "";
            if (d.TryGetProperty("author_name", out var an) && an.ValueKind == JsonValueKind.Array
                && an.GetArrayLength() > 0)
                author = an[0].GetString() ?? "";

            int year = 0;
            if (d.TryGetProperty("first_publish_year", out var y) && y.ValueKind == JsonValueKind.Number)
                year = y.GetInt32();

            result.Add(new CatalogItem(t.GetString()!, author, year));
        }
        return result;
    }
}
