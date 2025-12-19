using Hangfire.Annotations;
using Hangfire.Dashboard;
using System.Net.Http.Headers;
using System.Text;

namespace Bags_Shop_API.Middleware
{
    public class HangfireDashboardAuthorizationFilter : IDashboardAuthorizationFilter
    {
        private readonly string _dashboardPassword;

        public HangfireDashboardAuthorizationFilter(IConfiguration configuration)
        {
            _dashboardPassword = configuration["HangfireSettings:DashboardPassword"] ?? "DefaultPassword";
        }

        public bool Authorize([NotNull] DashboardContext context)
        {
            var httpContext = context.GetHttpContext();

            string header = httpContext.Request.Headers["Authorization"];

            if (!string.IsNullOrWhiteSpace(header))
            {
                var authHeader = AuthenticationHeaderValue.Parse(header);

                if ("Basic".Equals(authHeader.Scheme, StringComparison.OrdinalIgnoreCase))
                {
                    var parameter = Encoding.UTF8.GetString(Convert.FromBase64String(authHeader.Parameter));
                    var parts = parameter.Split(':');

                    if (parts.Length == 2)
                    {
                        var userName = parts[0];
                        var password = parts[1];

                        // Allowing any username, just checking the password
                        if (password == _dashboardPassword)
                        {
                            return true;
                        }
                    }
                }
            }

            // If not authorized, trigger the browser prompt
            httpContext.Response.StatusCode = 401;
            httpContext.Response.Headers.Append("WWW-Authenticate", "Basic realm=\"Hangfire Dashboard\"");
            return false;
        }
    }
}
