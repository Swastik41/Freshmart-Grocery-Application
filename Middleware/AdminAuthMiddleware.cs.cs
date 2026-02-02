using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace FreshMart.Middleware
{
    public class AdminAuthMiddleware
    {
        private readonly RequestDelegate _next;

        public AdminAuthMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var path = context.Request.Path.Value?.ToLower();

            // Protect admin routes except login
            if (path != null && path.StartsWith("/admin") && !path.Contains("login"))
            {
                var adminLogged = context.Session.GetString("AdminLoggedIn");

                if (adminLogged != "true")
                {
                    context.Response.Redirect("/Admin/Login");
                    return;
                }
            }

            await _next(context);
        }
    }
}
