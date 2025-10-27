using LibraryApi.Data;
using LibraryApi.Models;
using LibraryApi.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LibraryApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BooksController : ControllerBase
    {
        private readonly IOpenLibraryService _open;
        private readonly AppDbContext _db;

        public BooksController(IOpenLibraryService open, AppDbContext db)
        {
            _open = open;
            _db = db;
        }

        // GET api/books/search?q=harry
        [HttpGet("search")]
        public async Task<IActionResult> Search([FromQuery] string q, [FromQuery] int page = 1)
        {
            if (string.IsNullOrWhiteSpace(q)) return BadRequest("q required");
            // record search history when X-User-Id header is present
            if (Request.Headers.TryGetValue("X-User-Id", out var userIdHeader) && int.TryParse(userIdHeader.FirstOrDefault(), out var uid))
            {
                try
                {
                    _db.SearchHistories.Add(new Models.SearchHistory { UserId = uid, Query = q });
                    await _db.SaveChangesAsync();
                }
                catch { /* ignore failures recording history */ }
            }

            var res = await _open.SearchAsync(q, page);
            string? amazon = null;
            if (res == null || res.NumFound == 0)
            {
                amazon = $"https://www.amazon.com/s?k={Uri.EscapeDataString(q)}";
                return Ok(new { items = Array.Empty<object>(), amazonLink = amazon });
            }

            // map docs to a lighter DTO for the client, including a coverUrl when possible
            var itemsEnumerable = res.Docs?.Select(d =>
            {
                var olid = d.CoverEditionKey ?? (d.EditionKey?.FirstOrDefault()) ?? d.Key?.Split('/').Last();
                var cover = string.IsNullOrEmpty(olid) ? null : _open.GetCoverUrlByOlid(olid, "M");
                return new
                {
                    title = d.Title,
                    authors = d.AuthorName,
                    olid,
                    coverUrl = cover
                };
            });
            var items = itemsEnumerable != null ? itemsEnumerable.ToArray() : Array.Empty<object>();

            return Ok(new { items, amazonLink = amazon });
        }

        // GET api/books/olid/OL123M
        [HttpGet("olid/{olid}")]
        public async Task<IActionResult> GetByOlid(string olid)
        {
            var data = await _open.GetBookByOLIDAsync(olid);
            if (data == null) return NotFound();
            return Ok(data);
        }

        // GET api/books/cover/OL123M?size=L
        [HttpGet("cover/{olid}")]
        public IActionResult Cover(string olid, [FromQuery] string size = "M")
        {
            var url = _open.GetCoverUrlByOlid(olid, size);
            return Redirect(url);
        }

        // GET api/books/free?q=...
        [HttpGet("free")]
        public async Task<IActionResult> Free([FromQuery] string q)
        {
            if (string.IsNullOrWhiteSpace(q)) return BadRequest("q required");
            var search = await _open.SearchAsync(q, 1);
            if (search == null || search.Docs == null) return Ok(new { items = Array.Empty<object>() });
            var list = new List<object>();
            // limit to first 10 to avoid many requests
            foreach (var d in search.Docs.Take(10))
            {
                var olid = d.CoverEditionKey ?? (d.EditionKey?.FirstOrDefault()) ?? d.Key?.Split('/').Last();
                if (string.IsNullOrEmpty(olid)) continue;
                var book = await _open.GetBookByOLIDAsync(olid);
                var download = await _open.GetDownloadLinkForOlidAsync(olid);
                string? desc = null;
                if (book?.Description.HasValue == true)
                {
                    var el = book.Description.Value;
                    if (el.ValueKind == System.Text.Json.JsonValueKind.String)
                        desc = el.GetString();
                    else if (el.ValueKind == System.Text.Json.JsonValueKind.Object && el.TryGetProperty("value", out var v))
                        desc = v.GetString();
                }
                if (!string.IsNullOrEmpty(download))
                {
                    list.Add(new {
                        title = d.Title,
                        authors = d.AuthorName,
                        olid,
                        cover = _open.GetCoverUrlByOlid(olid,"M"),
                        description = desc,
                        downloadUrl = download
                    });
                }
            }
            return Ok(new { items = list });
        }

        // GET api/books/download/{olid}
        [HttpGet("download/{olid}")]
        public async Task<IActionResult> Download(string olid)
        {
            var link = await _open.GetDownloadLinkForOlidAsync(olid);
            if (link == null) return NotFound();
            return Redirect(link);
        }

        // GET api/books/proxydownload/{olid}
        [HttpGet("proxydownload/{olid}")]
        public async Task<IActionResult> ProxyDownload(string olid)
        {
            var stream = await _open.GetDownloadStreamAsync(olid);
            if (stream == null) return NotFound();
            // Try to set content type based on file extension in link if available
            var link = await _open.GetDownloadLinkForOlidAsync(olid);
            var contentType = "application/octet-stream";
            var filename = olid + ".dat";
            if (!string.IsNullOrEmpty(link))
            {
                if (link.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase)) contentType = "application/pdf";
                var seg = link.Split('/').LastOrDefault();
                if (!string.IsNullOrEmpty(seg)) filename = seg;
            }
            return File(stream, contentType, fileDownloadName: filename);
        }

        // Local library: add a book to local collection
        [HttpPost("local")]
        public async Task<IActionResult> AddLocal([FromBody] LocalBook book)
        {
            _db.LocalBooks.Add(book);
            await _db.SaveChangesAsync();
            return CreatedAtAction(nameof(GetLocal), new { id = book.Id }, book);
        }

        [HttpGet("local/{id}")]
        public async Task<IActionResult> GetLocal(int id)
        {
            var b = await _db.LocalBooks.FindAsync(id);
            if (b == null) return NotFound();
            return Ok(b);
        }

        [HttpGet("local")]
        public async Task<IActionResult> ListLocal()
        {
            var list = await _db.LocalBooks.ToListAsync();
            return Ok(list);
        }

        // Lend a local book
        [HttpPost("local/{id}/lend")]
        public async Task<IActionResult> LendLocal(int id, [FromBody] string borrower)
        {
            var book = await _db.LocalBooks.FindAsync(id);
            if (book == null) return NotFound();
            if (!book.IsAvailable) return BadRequest("Book not available");
            book.IsAvailable = false;
            var loan = new BookLoan { LocalBookId = id, Borrower = borrower };
            _db.BookLoans.Add(loan);
            await _db.SaveChangesAsync();
            return Ok(loan);
        }

        [HttpPost("local/{id}/return")]
        public async Task<IActionResult> ReturnLocal(int id)
        {
            var book = await _db.LocalBooks.FindAsync(id);
            if (book == null) return NotFound();
            var loan = await _db.BookLoans.Where(l => l.LocalBookId == id && l.ReturnDate == null).OrderByDescending(l => l.LoanDate).FirstOrDefaultAsync();
            if (loan == null) return BadRequest("No active loan");
            loan.ReturnDate = DateTime.UtcNow;
            book.IsAvailable = true;
            await _db.SaveChangesAsync();
            return Ok(loan);
        }
    }
}
