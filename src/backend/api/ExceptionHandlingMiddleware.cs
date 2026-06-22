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
            catch (Exception ex)
            {
                // Лог строго на русском перед двоеточием, как в контроллерах
                _logger.LogError(ex, "Глобальный перехватчик ошибок: Необработанное исключение при вызове {Method} {Path}", 
                    context.Request.Method, context.Request.Path);

                context.Response.ContentType = "application/json";
                context.Response.StatusCode = 500;

                var response = new 
                { 
                    message = "Внутренняя ошибка сервера. Подробности записаны в логи системы." 
                };
                
                await context.Response.WriteAsync(JsonSerializer.Serialize(response));
            }
        }
    }
}