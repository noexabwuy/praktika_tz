using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Text.Json;
using System.Threading.Tasks;

namespace api.Middleware
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;

        public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
{
    try
    {
        await _next(context);
    }
    // 1. Обработка ошибок некорректного HTTP-запроса (400 Bad Request)
    catch (Microsoft.AspNetCore.Http.BadHttpRequestException ex)
    {
        _logger.LogWarning(ex, "Глобальный перехватчик ошибок: Некорректный запрос при вызове {Method} {Path}", 
            context.Request.Method, context.Request.Path);

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = StatusCodes.Status400BadRequest;

        var response = new 
        { 
            message = "Некорректный запрос. Проверьте правильность передаваемых данных." 
        };
        
        await context.Response.WriteAsync(JsonSerializer.Serialize(response));
    }
    // 2. Обработка ошибок доступа / неавторизованного действия (401 Unauthorized)
    catch (UnauthorizedAccessException ex)
    {
        _logger.LogWarning(ex, "Глобальный перехватчик ошибок: Отказ в доступе при вызове {Method} {Path}", 
            context.Request.Method, context.Request.Path);

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = StatusCodes.Status401Unauthorized;

        var response = new 
        { 
            message = "Доступ запрещен. Необходима авторизация или у вас недостаточно прав." 
        };
        
        await context.Response.WriteAsync(JsonSerializer.Serialize(response));
    }
    // 3. Общий перехватчик для всех остальных непредвиденных ошибок (500 Internal Server Error)
    catch (Exception ex)
    {
        _logger.LogError(ex, "Глобальный перехватчик ошибок: Необработанное исключение при вызове {Method} {Path}", 
            context.Request.Method, context.Request.Path);

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = StatusCodes.Status500InternalServerError;

        var response = new 
        { 
            message = "Внутренняя ошибка сервера. Подробности записаны в логи системы." 
        };
        
        await context.Response.WriteAsync(JsonSerializer.Serialize(response));
    }
}
    }
}