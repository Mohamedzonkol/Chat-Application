namespace API.Dto
{
    public class OnlineUserDto
    {
        public string? Id { get; set; }
        public string? ConnectionId { get; set; }
        public string? UserName { get; set; }
        public string? FullName { get; set; }
        public string? Image { get; set; }
        public bool IsOnline { get; set; }
        public int UnreadCount { get; set; }
    }
}
