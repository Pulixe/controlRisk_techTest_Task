using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using TaskApi.Functions.Extensions;
using TaskApi.Functions.Repositories;

namespace TaskApi.Functions.Functions
{
    public class UsersFunction
    {
        private readonly IUserRepository _users;
        private readonly ILogger _logger;

        public UsersFunction(IUserRepository users, ILoggerFactory loggerFactory)
        {
            _users = users;
            _logger = loggerFactory.CreateLogger<UsersFunction>();
        }

        [Function("GetUsers")]
        public async Task<HttpResponseData> GetUsers(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "users")] HttpRequestData req,
            FunctionContext context)
        {
            try
            {
                var current = context.GetCurrentUser();
                if (current == null)
                {
                    var unauth = req.CreateResponse(HttpStatusCode.Unauthorized);
                    await unauth.WriteStringAsync("Unauthorized");
                    return unauth;
                }

                var query = System.Web.HttpUtility.ParseQueryString(req.Url.Query);
                string? q = query["q"];
                int skip = int.TryParse(query["skip"], out var s) ? s : 0;
                int take = int.TryParse(query["take"], out var t) ? t : 50;

                var list = await _users.ListAsync(q, skip, take);

                var resp = req.CreateResponse(HttpStatusCode.OK);
                await resp.WriteAsJsonAsync(list.Select(u => new { id = u.Id, name = u.Name, email = u.Email }));
                return resp;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error listing users");
                var r = req.CreateResponse(HttpStatusCode.InternalServerError);
                await r.WriteStringAsync("Internal server error");
                return r;
            }
        }
    }
}

