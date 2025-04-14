using System.Security.Claims;

namespace API.Extensions
{
    public static class ClaimExtensions
    {
        public static string GetUserName(this ClaimsPrincipal user)
        {
            return user.FindFirstValue(ClaimTypes.Name) ?? throw new Exception("Cannot Get user");
        }
        public static Guid GetUserId(this ClaimsPrincipal user)
        {
            var id = user.FindFirstValue(ClaimTypes.NameIdentifier);
            if (id == null) throw new Exception("Cannot Get user");
            return Guid.Parse(id);
        }
    }
}
