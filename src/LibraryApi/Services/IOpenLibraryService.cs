using LibraryApi.Models;
using System.Text.Json;

namespace LibraryApi.Services
{
    public interface IOpenLibraryService
    {
        Task<SearchResult?> SearchAsync(string query, int page = 1);
        Task<BookData?> GetBookByOLIDAsync(string olid);
        string GetCoverUrlByOlid(string olid, string size = "M");
        Task<string?> GetAuthorDetailsAsync(string authorKey);
        Task<JsonDocument?> GetVolumeInfoAsync(string olid);
        Task<string?> GetDownloadLinkForOlidAsync(string olid);
        Task<System.IO.Stream?> GetDownloadStreamAsync(string olid);
    }
}
