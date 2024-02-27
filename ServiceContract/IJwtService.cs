using OrdersMinimalAPI.EfCore;
using OrdersMinimalAPI.Response;
using System.Security.Claims;

namespace OrdersMinimalAPI.ServiceContract
{
    public interface IJwtService
    {
        AuthenticationResponse CreateJwtToken(ApplicationUser user);
        ClaimsPrincipal? GetPrincipalFromJwtToken(string? token);
    }
}
