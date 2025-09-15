using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TaskApi.Functions.Models;

namespace TaskApi.Functions.Repositories
{
    public interface IUserRepository
    {
        Task<User?> GetBySubAsync(string sub);
        Task<User> AddAsync(User user);
        Task UpdateAsync(User user);
        Task<IEnumerable<User>> ListAsync(string? q = null, int skip = 0, int take = 50);
    }
}
