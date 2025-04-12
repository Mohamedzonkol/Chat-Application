using Microsoft.AspNetCore.Mvc;

namespace API.Dto
{
    public class RegisterRequest
    {
        [FromForm]
        public string UserName { get; set; }
        [FromForm]
        public string FullName { get; set; }
        [FromForm]
        public string Email { get; set; }
        [FromForm]
        public string Password { get; set; }
        [FromForm]
        public IFormFile? ProfileImage { get; set; }
    }
}
