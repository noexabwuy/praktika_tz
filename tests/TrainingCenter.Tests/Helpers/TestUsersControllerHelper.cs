using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using api.Controllers;
using api.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace TrainingCenter.Tests.Helpers
{
    public static class TestUsersControllerHelper
    {
        public static async Task<IActionResult> GetUsers(
            ApplicationDbContext context,
            string token,
            string? roleFilter = null)
        {
            var controller = new UsersController(context);

            var role = "Admin";
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Role, role),
                new Claim(ClaimTypes.NameIdentifier, "some-user-id")
            };
            var identity = new ClaimsIdentity(claims, "TestAuth");
            var principal = new ClaimsPrincipal(identity);

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = principal }
            };

            return await controller.GetUsers(roleFilter);
        }
    }
}