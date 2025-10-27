using LibraryApi.Data;
using LibraryApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LibraryApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly AppDbContext _db;
        public UsersController(AppDbContext db) { _db = db; }

        [HttpPost]
        public async Task<IActionResult> Register([FromBody] User user)
        {
            if (string.IsNullOrWhiteSpace(user.Username)) return BadRequest("username required");
            _db.Users.Add(user);
            await _db.SaveChangesAsync();
            return CreatedAtAction(nameof(Get), new { id = user.Id }, user);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var u = await _db.Users.FindAsync(id);
            if (u == null) return NotFound();
            return Ok(u);
        }

        [HttpGet]
        public async Task<IActionResult> List() => Ok(await _db.Users.ToListAsync());

        [HttpPost("{id}/wishlist")]
        public async Task<IActionResult> AddWishlist(int id, [FromBody] WishlistItem item)
        {
            var user = await _db.Users.FindAsync(id);
            if (user == null) return NotFound("user not found");
            item.UserId = id;
            _db.WishlistItems.Add(item);
            await _db.SaveChangesAsync();
            return CreatedAtAction(nameof(GetWishlist), new { id = id }, item);
        }

        [HttpGet("{id}/wishlist")]
        public async Task<IActionResult> GetWishlist(int id)
        {
            var list = await _db.WishlistItems.Where(w => w.UserId == id).ToListAsync();
            return Ok(list);
        }

        [HttpGet("{id}/searchHistory")]
        public async Task<IActionResult> GetSearchHistory(int id)
        {
            var list = await _db.SearchHistories.Where(s => s.UserId == id).OrderByDescending(s => s.Timestamp).Take(50).ToListAsync();
            return Ok(list);
        }

        [HttpGet("ranking")]
        public async Task<IActionResult> Ranking()
        {
            // Ranking by number of loans per borrower name
            var ranks = await _db.BookLoans
                .GroupBy(l => l.Borrower)
                .Select(g => new { borrower = g.Key, loans = g.Count() })
                .OrderByDescending(x => x.loans)
                .Take(50)
                .ToListAsync();
            return Ok(ranks);
        }
    }
}
