using System.Net.Http.Json;
using System.Text.Json;
using LibraryApi.Models;

namespace LibraryApi.Services
{
    using Microsoft.Extensions.Caching.Memory;

    public class OpenLibraryService : IOpenLibraryService
    {
        private readonly HttpClient _http;
        private readonly IMemoryCache _cache;
        private readonly JsonSerializerOptions _jsonOptions = new() { PropertyNameCaseInsensitive = true };

        public OpenLibraryService(HttpClient http, IMemoryCache cache)
        {
            _http = http;
            _cache = cache;
        }

        public async Task<SearchResult?> SearchAsync(string query, int page = 1)
        {
            var url = $"search.json?q={Uri.EscapeDataString(query)}&page={page}";
            var res = await _http.GetStreamAsync(url);
            return await JsonSerializer.DeserializeAsync<SearchResult>(res, _jsonOptions);
        }

        public async Task<BookData?> GetBookByOLIDAsync(string olid)
        {
            // Use the Books API
            var url = $"api/books?bibkeys=OLID:{Uri.EscapeDataString(olid)}&format=json&jscmd=data";
            var stream = await _http.GetStreamAsync(url);
            var doc = await JsonSerializer.DeserializeAsync<Dictionary<string, BookData>>(stream, _jsonOptions);
            if (doc != null && doc.TryGetValue($"OLID:{olid}", out var data)) return data;
            // fallback: try the edition resource /books/{olid}.json which often contains description and ocaid
            try
            {
                var edUrl = $"books/{Uri.EscapeDataString(olid)}.json";
                var str = await _http.GetStringAsync(edUrl);
                using var jd = JsonDocument.Parse(str);
                var root = jd.RootElement;
                var bd = new BookData();
                if (root.TryGetProperty("title", out var t)) bd.Title = t.GetString();
                if (root.TryGetProperty("description", out var desc)) bd.Description = desc;
                if (root.TryGetProperty("number_of_pages", out var pages) && pages.ValueKind == JsonValueKind.Number)
                    bd.NumberOfPages = pages.GetInt32();
                if (root.TryGetProperty("cover", out var cover) && cover.ValueKind == JsonValueKind.Object)
                {
                    try { bd.Cover = JsonSerializer.Deserialize<CoverInfo>(cover.GetRawText(), _jsonOptions); } catch { }
                }
                return bd;
            }
            catch
            {
                return null;
            }
        }

        public async Task<JsonDocument?> GetVolumeInfoAsync(string olid)
        {
            if (string.IsNullOrEmpty(olid)) return null;
            var cacheKey = $"vol:{olid}";
            if (_cache.TryGetValue<JsonDocument>(cacheKey, out var cached)) return cached;
            try
            {
                var url = $"api/volumes/brief/json/OLID:{Uri.EscapeDataString(olid)}";
                var str = await _http.GetStringAsync(url);
                var doc = JsonDocument.Parse(str);
                _cache.Set(cacheKey, doc, TimeSpan.FromHours(1));
                return doc;
            }
            catch
            {
                return null;
            }
        }

        public async Task<string?> GetDownloadLinkForOlidAsync(string olid)
        {
            if (string.IsNullOrEmpty(olid)) return null;
            var cacheKey = $"dl:{olid}";
            if (_cache.TryGetValue<string>(cacheKey, out var cached)) return cached;

            // Try volumes brief API to find preview_url or ocaid (Internet Archive)
            var vol = await GetVolumeInfoAsync(olid);
            if (vol == null) return null;

            try
            {
                if (vol.RootElement.TryGetProperty("records", out var records))
                {
                    foreach (var recProp in records.EnumerateObject())
                    {
                        var rec = recProp.Value;
                        if (rec.TryGetProperty("preview_url", out var preview))
                        {
                            var url = preview.GetString();
                            if (!string.IsNullOrEmpty(url))
                            {
                                _cache.Set(cacheKey, url, TimeSpan.FromHours(6));
                                return url;
                            }
                        }
                        if (rec.TryGetProperty("ocaid", out var ocaidEl))
                        {
                            var ocaid = ocaidEl.GetString();
                            if (!string.IsNullOrEmpty(ocaid))
                            {
                                var candidate = $"https://archive.org/download/{ocaid}/{ocaid}.pdf";
                                _cache.Set(cacheKey, candidate, TimeSpan.FromHours(6));
                                return candidate;
                            }
                        }
                    }
                }
            }
            catch { }

            // fallback: query edition resource for ocaid or ia identifier
            try
            {
                var edUrl = $"books/{Uri.EscapeDataString(olid)}.json";
                var str = await _http.GetStringAsync(edUrl);
                using var jd = JsonDocument.Parse(str);
                var root = jd.RootElement;
                if (root.TryGetProperty("ocaid", out var oca) && oca.ValueKind == JsonValueKind.String)
                {
                    var ocaid = oca.GetString();
                    if (!string.IsNullOrEmpty(ocaid))
                    {
                        var candidate = $"https://archive.org/download/{ocaid}/{ocaid}.pdf";
                        _cache.Set(cacheKey, candidate, TimeSpan.FromHours(6));
                        return candidate;
                    }
                }
                // sometimes source_records or identifiers.ia contain archive ids
                if (root.TryGetProperty("identifiers", out var ids) && ids.ValueKind == JsonValueKind.Object)
                {
                    if (ids.TryGetProperty("ia", out var iaArr) && iaArr.ValueKind == JsonValueKind.Array && iaArr.GetArrayLength() > 0)
                    {
                        var iaid = iaArr[0].GetString();
                        if (!string.IsNullOrEmpty(iaid))
                        {
                            var candidate = $"https://archive.org/download/{iaid}/{iaid}.pdf";
                            _cache.Set(cacheKey, candidate, TimeSpan.FromHours(6));
                            return candidate;
                        }
                    }
                }
            }
            catch { }

            return null;
        }

        public async Task<System.IO.Stream?> GetDownloadStreamAsync(string olid)
        {
            var link = await GetDownloadLinkForOlidAsync(olid);
            if (string.IsNullOrEmpty(link)) return null;
            try
            {
                // request as stream; do not buffer fully in memory
                var resp = await _http.GetAsync(link, HttpCompletionOption.ResponseHeadersRead);
                resp.EnsureSuccessStatusCode();
                var stream = await resp.Content.ReadAsStreamAsync();
                return stream;
            }
            catch
            {
                return null;
            }
        }

        public string GetCoverUrlByOlid(string olid, string size = "M")
        {
            // sizes: S, M, L
            return $"https://covers.openlibrary.org/b/olid/{Uri.EscapeDataString(olid)}-{size}.jpg";
        }

        public async Task<string?> GetAuthorDetailsAsync(string authorKey)
        {
            // authorKey expected like "/authors/OL1A" or "OL1A"
            var cleaned = authorKey?.Replace("/authors/", "");
            if (string.IsNullOrEmpty(cleaned)) return null;
            var url = $"authors/{cleaned}.json";
            try
            {
                return await _http.GetStringAsync(url);
            }
            catch
            {
                return null;
            }
        }
    }
}
