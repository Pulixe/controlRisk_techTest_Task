using Microsoft.Azure.Functions.Worker;
using TaskApi.Functions.Models;
using TaskApi.Functions.Middleware;

namespace TaskApi.Functions.Extensions
{
    public static class FunctionContextUserExtensions
    {
        public static User? GetCurrentUser(this FunctionContext context)
        {
            if (context.Items.TryGetValue(JwtAuthenticationMiddleware.ContextUserKey, out var userObj) && userObj is User u)
            {
                return u;
            }
            return null;
        }
    }
}

