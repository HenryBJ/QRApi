using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace QRApi.Middleware;

public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task Invoke(HttpContext context)
    {
        var logBuilder = new StringBuilder();

        // Obtener el Path y los Headers
        logBuilder.AppendLine($"📥 REQUEST: {context.Request.Method} {context.Request.Path}");

        foreach (var header in context.Request.Headers)
        {
            logBuilder.AppendLine($"📌 HEADER: {header.Key}: {header.Value}");
        }

        // Leer el Body del request
        context.Request.EnableBuffering(); // Permite leer el body sin consumirlo
        string body = "";
        using (var reader = new StreamReader(context.Request.Body, Encoding.UTF8, leaveOpen: true))
        {
            body = await reader.ReadToEndAsync();
            context.Request.Body.Position = 0; // Resetear la posición del stream
        }

        logBuilder.AppendLine($"📦 BODY: {body}");

        // Loggear la salida final en una sola línea
        _logger.LogInformation(logBuilder.ToString());

        // Llamar al siguiente middleware
        await _next(context);
    }
}
