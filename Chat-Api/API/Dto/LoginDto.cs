using Microsoft.AspNetCore.Mvc;

namespace API.Dto
{
    public class LoginDto
    {
        [FromForm]
        public string Email { get; set; } = string.Empty;
        [FromForm]
        public string Password { get; set; } = string.Empty;
    }
}
