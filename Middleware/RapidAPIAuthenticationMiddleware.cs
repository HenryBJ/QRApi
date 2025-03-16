using System.Security.Claims;

namespace QRApi.Middleware;

public class RapidAPIAuthenticationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly string _rapidAPISecret;

    public RapidAPIAuthenticationMiddleware(RequestDelegate next, IConfiguration configuration)
    {
        _next = next;
        _rapidAPISecret = configuration["RapidAPISecret"]; // Obtiene el secreto de los secrets
    }

    public async Task Invoke(HttpContext context)
    {
        if (context.Request.Headers.TryGetValue("X-RapidAPI-Proxy-Secret", out var secretValue))
        {
            if (secretValue == _rapidAPISecret) // Si el header coincide con el secreto
            {
                // Simula un usuario autenticado con permisos completos
                var claims = new[]
                {
                    new Claim(ClaimTypes.Name, "RapidAPIUser"),
                    new Claim("qr-use-icon", "true"),
                    new Claim("qr-use-colors", "true")
                };

                var identity = new ClaimsIdentity(claims, "RapidAPI");
                var principal = new ClaimsPrincipal(identity);
                context.User = principal;
            }
        }

        await _next(context);
    }
}
