using System.Text.Json.Serialization;
using System.Text.Json;

namespace LibraryApi.Models
{
    // Minimal DTOs for OpenLibrary responses we use
    public class SearchResult
    {
        [JsonPropertyName("numFound")] public int NumFound { get; set; }
        [JsonPropertyName("docs")] public Doc[]? Docs { get; set; }
    }

    public class Doc
    {
        [JsonPropertyName("title")] public string? Title { get; set; }
        [JsonPropertyName("author_name")] public string[]? AuthorName { get; set; }
        [JsonPropertyName("cover_edition_key")] public string? CoverEditionKey { get; set; }
        [JsonPropertyName("edition_key")] public string[]? EditionKey { get; set; }
        [JsonPropertyName("key")] public string? Key { get; set; }
        [JsonPropertyName("first_publish_year")] public int? FirstPublishYear { get; set; }
    }

    // For /api/books?bibkeys=OLID:OL...&format=json&jscmd=data we return a dictionary; we'll map selectively in service
    public class BookData
    {
        [JsonPropertyName("title")] public string? Title { get; set; }
        [JsonPropertyName("authors")] public AuthorRef[]? Authors { get; set; }
        [JsonPropertyName("publish_date")] public string? PublishDate { get; set; }
        [JsonPropertyName("number_of_pages")] public int? NumberOfPages { get; set; }
        [JsonPropertyName("cover")] public CoverInfo? Cover { get; set; }
        [JsonPropertyName("description")] public JsonElement? Description { get; set; }
        [JsonPropertyName("excerpts")] public object? Excerpts { get; set; }
        [JsonPropertyName("ebooks")] public object? Ebooks { get; set; }
    }

    public class AuthorRef
    {
        [JsonPropertyName("name")] public string? Name { get; set; }
        [JsonPropertyName("url")] public string? Url { get; set; }
    }

    public class CoverInfo
    {
        [JsonPropertyName("small")] public string? Small { get; set; }
        [JsonPropertyName("medium")] public string? Medium { get; set; }
        [JsonPropertyName("large")] public string? Large { get; set; }
    }
}
