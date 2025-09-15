using TaskApi.Functions.Models;

namespace TaskApi.Functions.Factories
{
    public interface IUserFactory
    {
        User Create(string sub, string name, string email);
    }

    public class UserFactory : IUserFactory
    {
        public User Create(string sub, string name, string email)
        {
            return new User
            {
                Sub = sub,
                Name = string.IsNullOrWhiteSpace(name) ? null : name,
                Email = string.IsNullOrWhiteSpace(email) ? null : email,
                CreatedAt = DateTime.UtcNow,
                LastLogin = null
            };
        }
    }
}

