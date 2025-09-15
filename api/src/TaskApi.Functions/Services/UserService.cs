using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Threading.Tasks;
using TaskApi.Functions.Models;
using TaskApi.Functions.Repositories;
using TaskApi.Functions.Factories;
using Microsoft.Extensions.Logging;

namespace TaskApi.Functions.Services
{
    public interface IUserService
    {
        Task<User> EnsureUserAsync(JwtSecurityToken token);
    }

    public class UserService : IUserService
    {
        private readonly IUserRepository _users;
        private readonly IUserFactory _factory;
        private readonly ILogger<UserService> _logger;

        public UserService(IUserRepository users, IUserFactory factory, ILogger<UserService> logger)
        {
            _users = users;
            _factory = factory;
            _logger = logger;
        }

        public async Task<User> EnsureUserAsync(JwtSecurityToken token)
        {
            var sub = token.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sub)?.Value
                      ?? token.Subject ?? string.Empty;

            // Prefer standard email claims, then common provider fallbacks
            string claimSource = "none";
            string email = token.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Email || c.Type == "email")?.Value ?? string.Empty;
            if (!string.IsNullOrWhiteSpace(email))
            {
                claimSource = "email";
            }
            else
            {
                var emailsArray = token.Claims.Where(c => c.Type == "emails").Select(c => c.Value).FirstOrDefault();
                if (!string.IsNullOrWhiteSpace(emailsArray))
                {
                    email = emailsArray;
                    claimSource = "emails[]";
                }
                else
                {
                    var alt = token.Claims.FirstOrDefault(c => c.Type == "preferred_username" || c.Type == "upn" || c.Type == "unique_name")?.Value;
                    if (!string.IsNullOrWhiteSpace(alt))
                    {
                        email = alt!;
                        claimSource = "preferred_username/upn/unique_name";
                    }
                    else
                    {
                        email = string.Empty;
                    }
                }
            }
            
            var name = token.Claims.FirstOrDefault(c => c.Type == "name" || c.Type == JwtRegisteredClaimNames.Name)?.Value ?? string.Empty;

            if (string.IsNullOrWhiteSpace(sub))
                throw new System.Security.Authentication.AuthenticationException("Token missing 'sub' claim");

            _logger.LogInformation("EnsureUserAsync: sub={sub}, name={name}, emailCandidate={email}, source={source}", sub, name, email, claimSource);

            var existing = await _users.GetBySubAsync(sub);
            if (existing == null)
            {
                var user = _factory.Create(sub, name, email);
                user.LastLogin = System.DateTime.UtcNow;
                _logger.LogInformation("Creating new user: sub={sub}, name={name}, email={email}", sub, user.Name, user.Email);
                return await _users.AddAsync(user);
            }
            else
            {
                existing.LastLogin = System.DateTime.UtcNow;
                // Optionally keep latest name/email
                if (!string.IsNullOrWhiteSpace(name)) existing.Name = name;
                if (!string.IsNullOrWhiteSpace(email))
                {
                    existing.Email = email;
                }
                _logger.LogInformation("Updating user: id={id}, sub={sub}, name={name}, email={email}", existing.Id, existing.Sub, existing.Name, existing.Email);
                await _users.UpdateAsync(existing);
                return existing;
            }
        }
    }
}
