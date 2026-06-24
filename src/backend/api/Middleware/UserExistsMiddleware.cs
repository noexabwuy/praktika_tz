using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using api.Data;

namespace api.Middleware
{
    /// <summary>
    /// Middleware для проверки существования пользователя в БД
    /// </summary>
    public class UserExistsMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<UserExistsMiddleware> _logger;

        public UserExistsMiddleware(RequestDelegate next, ILogger<UserExistsMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context, ApplicationDbContext dbContext)
        {
            // Проверяем, авторизован ли пользователь
            if (context.User.Identity?.IsAuthenticated == true)
            {
                var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim != null && Guid.TryParse(userIdClaim.Value, out var userId))
                {
                    // Проверяем, существует ли пользователь в БД
                    var userExists = await dbContext.Users.AnyAsync(u => u.Id == userId);

                    if (!userExists)
                    {
                        _logger.LogWarning($"Пользователь {userId} не найден в БД, но токен активен");

                        // Возвращаем 401 и просим выйти
                        context.Response.StatusCode = 401;
                        await context.Response.WriteAsJsonAsync(new
                        {
                            message = "Пользователь не найден. Пожалуйста, выйдите и войдите заново.",
                            code = "USER_NOT_FOUND"
                        });
                        return;
                    }
                }
            }

            await _next(context);
        }
    }

    // Метод расширения для удобного добавления в Program.cs
    public static class UserExistsMiddlewareExtensions
    {
        public static IApplicationBuilder UseUserExistsMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<UserExistsMiddleware>();
        }
    }
}