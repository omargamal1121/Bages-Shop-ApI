namespace Bags_Shop_API.Middleware
{
	public class UserKeyMiddleware
	{
		private readonly RequestDelegate _next;
		private readonly ILogger<UserKeyMiddleware> _logger;
		public UserKeyMiddleware(RequestDelegate next,ILogger<UserKeyMiddleware> logger)
		{
			_next = next;
			_logger = logger;

        }
        public async Task InvokeAsync(HttpContext context)
        {
            string? userKey = context.Request.Cookies["UserKey"];

            if (string.IsNullOrEmpty(userKey))
            {
                userKey = Guid.NewGuid().ToString();

                context.Response.Cookies.Append(
                    "UserKey",
                    userKey,
                    new CookieOptions
                    {
                        HttpOnly = true,
                        Expires = DateTimeOffset.UtcNow.AddMonths(6),
                        Secure = true,                 
                        SameSite = SameSiteMode.None,

                    });
            }

            context.Items["UserKey"] = userKey;

            await _next(context);
        }


    }
}
