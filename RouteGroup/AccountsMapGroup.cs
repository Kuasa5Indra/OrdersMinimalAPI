using Microsoft.AspNetCore.Identity;
using OrdersMinimalAPI.EfCore;
using OrdersMinimalAPI.Request;
using OrdersMinimalAPI.Response;
using OrdersMinimalAPI.Service;
using OrdersMinimalAPI.ServiceContract;
using System.Security.Claims;

namespace OrdersMinimalAPI.RouteGroup
{
    public static class AccountsMapGroup
    {
        public static RouteGroupBuilder AccountsAPI(this RouteGroupBuilder group)
        {
            group.MapPost("/register", async(
                RegisterAccountRequest request,
                UserManager<ApplicationUser> userManager,
                SignInManager<ApplicationUser> signInManager,
                IJwtService jwtService) =>
            {
                ApplicationUser user = new()
                {
                    Email = request.Email,
                    PhoneNumber = request.PhoneNumber,
                    UserName = request.UserName,
                    PersonName = request.PersonName
                };

                IdentityResult result = await userManager.CreateAsync(user, request.Password);

                if (result.Succeeded)
                {
                    await signInManager.SignInAsync(user, isPersistent: false);

                    var authenticationResponse = jwtService.CreateJwtToken(user);
                    user.RefreshToken = authenticationResponse.RefreshToken;
                    user.RefreshTokenExpiration = authenticationResponse.RefreshTokenExpiration;

                    await userManager.UpdateAsync(user);
                    return Results.Ok(authenticationResponse);
                }
                else
                {
                    string errorMessage = string.Join(", ", result.Errors.Select(e => e.Description));
                    return Results.Problem(errorMessage);
                }
            });

            group.MapPost("/login", async(
                LoginRequest request,
                UserManager<ApplicationUser> userManager,
                SignInManager<ApplicationUser> signInManager,
                IJwtService jwtService) =>
            {
                var result = await signInManager.PasswordSignInAsync(request.Username, request.Password, isPersistent: false, lockoutOnFailure: false);

                if (result.Succeeded)
                {
                    ApplicationUser? user = await userManager.FindByNameAsync(request.Username);

                    if (user == null)
                    {
                        return Results.NoContent();
                    }

                    var authenticationResponse = jwtService.CreateJwtToken(user);

                    user.RefreshToken = authenticationResponse.RefreshToken;
                    user.RefreshTokenExpiration = authenticationResponse.RefreshTokenExpiration;

                    await userManager.UpdateAsync(user);

                    return Results.Ok(authenticationResponse);
                }
                else
                {
                    return Results.Problem("Invalid username or password");
                }
            });

            group.MapGet("/logout", async(SignInManager<ApplicationUser> signInManager) =>
            {
                await signInManager.SignOutAsync();

                return Results.NoContent();
            });

            group.MapPost("/generate-new-token", async (
                TokenRequest request,
                UserManager<ApplicationUser> userManager,
                IJwtService jwtService) =>
            {
                if (request == null)
                {
                    return Results.BadRequest("Invalid client request");
                }

                string? token = request.Token;
                string? refreshToken = request.RefreshToken;

                ClaimsPrincipal? principal = jwtService.GetPrincipalFromJwtToken(token);

                if (principal == null)
                {
                    return Results.BadRequest("Invalid jwt access token");
                }

                string? id = principal.FindFirstValue(ClaimTypes.NameIdentifier);

                ApplicationUser? user = await userManager.FindByIdAsync(id);

                if (user == null || user.RefreshToken != refreshToken || user.RefreshTokenExpiration <= DateTime.UtcNow)
                {
                    return Results.BadRequest("Invalid Refresh Token");
                }

                AuthenticationResponse authenticationResponse = jwtService.CreateJwtToken(user);

                user.RefreshToken = authenticationResponse.RefreshToken;
                user.RefreshTokenExpiration = authenticationResponse.RefreshTokenExpiration;

                await userManager.UpdateAsync(user);

                return Results.Ok(authenticationResponse);
            });

            return group;
        }
    }
}
