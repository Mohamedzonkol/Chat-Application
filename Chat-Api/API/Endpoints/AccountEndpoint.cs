using API.Comman;
using API.Dto;
using API.Models;
using API.Services;
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
                async (HttpContext context, UserManager<AppUser> userManager, [FromForm] RegisterRequest request) =>
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

            return group;
        }
    }
}
