﻿namespace API.Dto
{
    public class RegisterRequest
    {
        public string UserName { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public IFormFile? ProfileImage { get; set; }
    }
}
