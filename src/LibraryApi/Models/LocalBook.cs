namespace LibraryApi.Models
{
    public class LocalBook
    {
        public int Id { get; set; }
        public string? Title { get; set; }
        public string? Authors { get; set; }
        public string? OpenLibraryId { get; set; }
        public string? CoverUrl { get; set; }
        public bool IsAvailable { get; set; } = true;
    }

    public class BookLoan
    {
        public int Id { get; set; }
        public int LocalBookId { get; set; }
        public string Borrower { get; set; } = string.Empty;
        public DateTime LoanDate { get; set; } = DateTime.UtcNow;
        public DateTime? ReturnDate { get; set; }
    }
}
