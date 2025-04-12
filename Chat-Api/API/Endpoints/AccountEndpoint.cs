using API.Comman;
using API.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace API.Endpoints
{
    public static class AccountEndpoint
    {
        public static RouteGroupBuilder MapAccountEndpoints(this WebApplication app)
        {
            var group = app.MapGroup("/api/account").WithTags("Account");

            group.MapPost("/register",
                async (HttpContext context, UserManager<AppUser> userManager, [FromForm] string userName, [FromForm] string fullName,
                    [FromForm] string email, [FromForm] string password) =>
                {
                    var userFromdb = await userManager.FindByEmailAsync(email);
                    if (userFromdb != null)
                    {
                        return Results.BadRequest(Response<string>.Failure("User already exists"));
                    }

                    var user = new AppUser
                    {
                        Email = email,
                        FullName = fullName,
                        UserName = userName
                    };
                    var result = await userManager.CreateAsync(user, password);
                    if (result.Succeeded)
                    {
                        return Results.Ok(Response<string>.Success("", "User created successfully"));
                    }

                    return Results.BadRequest(Response<string>.Failure("User creation failed",
                        result.Errors.FirstOrDefault()?.Description));
                }).DisableAntiforgery();
            return group;
        }
    }
}
