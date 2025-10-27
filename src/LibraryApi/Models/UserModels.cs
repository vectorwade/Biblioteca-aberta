using System.ComponentModel.DataAnnotations;

namespace LibraryApi.Models
{
    public class User
    {
        public int Id { get; set; }
        [Required]
        public string Username { get; set; } = string.Empty;
        public string? DisplayName { get; set; }
        public string? Email { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    public class WishlistItem
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string? OpenLibraryId { get; set; }
        public string? Title { get; set; }
        public DateTime AddedAt { get; set; } = DateTime.UtcNow;
    }

    public class SearchHistory
    {
        public int Id { get; set; }
        public int? UserId { get; set; }
        public string Query { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}
