namespace API.Models
{
    public class Messages
    {
        public int Id { get; set; }
        public int? SenderId { get; set; }
        public int? ReceiverId { get; set; }
        public string? Content { get; set; }
        public DateTime? CreatedAt { get; set; }
        public bool? IsRead { get; set; }
        public AppUser Sender { get; set; }
        public AppUser Receiver { get; set; }
    }
}
