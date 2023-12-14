namespace ModularApi.Middleware
{
    public class UserScopeMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<UserScopeMiddleware> _logger;

        public UserScopeMiddleware(RequestDelegate next, ILogger<UserScopeMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task Invoke(HttpContext context) 
        {
            if (context.Request.Method == "GET")
            {
                using (_logger.BeginScope($"Scoping for all GET requests"))
                {
                    await _next(context);
                }
            }
            else 
            {
                await _next(context);
            }
        }
    }
}
