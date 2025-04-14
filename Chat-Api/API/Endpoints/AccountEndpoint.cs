using API.Comman;
using API.Dto;
using API.Models;
using API.Services;
using Microsoft.AspNetCore.Identity;

namespace API.Endpoints
{
    public static class AccountEndpoint
    {
        public static RouteGroupBuilder MapAccountEndpoints(this WebApplication app)
        {
            var group = app.MapGroup("/api/account").WithTags("Account");

            group.MapPost("/register",
                async (HttpContext context, UserManager<AppUser> userManager, RegisterRequest request) =>
                {
                    var userFromDb = await userManager.FindByEmailAsync(request.Email);
                    if (userFromDb != null)
                    {
                        return Results.BadRequest(Response<string>.Failure("User already exists"));
                    }

                    var picture = await FileUploadService.UploadFile(request.ProfileImage);
                    picture = $"{context.Request.Scheme}://{context.Request.Host}/uploads/{picture}";

                    var user = new AppUser
                    {
                        Email = request.Email,
                        FullName = request.FullName,
                        UserName = request.UserName,
                        ProfileImage = picture
                    };

                    var result = await userManager.CreateAsync(user, request.Password);
                    if (result.Succeeded)
                    {
                        return Results.Ok(Response<string>.Success("", "User created successfully"));
                    }

                    return Results.BadRequest(Response<string>.Failure("User creation failed",
                        result.Errors.FirstOrDefault()?.Description));
                }).DisableAntiforgery();

            group.MapPost("/login",
                async (UserManager<AppUser> userManager, TokenServices tokenServices, LoginDto request) =>
            {
                var user = await userManager.FindByEmailAsync(request.Email);
                if (user == null)
                {
                    return Results.BadRequest(Response<string>.Failure("User not found"));
                }
                var result = await userManager.CheckPasswordAsync(user, request.Password);
                if (!result)
                {
                    return Results.BadRequest(Response<string>.Failure("Invalid password"));
                }

                var token = tokenServices.GenerateToken(user.Id, user.UserName);
                return Results.Ok(Response<string>.Success(token, "Login successful"));
            });
            return group;
        }
    }
}
