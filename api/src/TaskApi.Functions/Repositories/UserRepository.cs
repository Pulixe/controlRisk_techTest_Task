using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TaskApi.Functions.Data;
using TaskApi.Functions.Models;

namespace TaskApi.Functions.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly AppDbContext _db;
        public UserRepository(AppDbContext db) => _db = db;

        public async Task<User?> GetBySubAsync(string sub)
        {
            // Return a tracked entity to avoid duplicate tracking conflicts on update
            return await _db.Users.FirstOrDefaultAsync(u => u.Sub == sub);
        }

        public async Task<User> AddAsync(User user)
        {
            _db.Users.Add(user);
            await _db.SaveChangesAsync();
            return user;
        }

        public async Task UpdateAsync(User user)
        {
            // If an instance with the same key is already tracked, update its values instead of attaching a new instance
            var local = _db.Users.Local.FirstOrDefault(e => e.Id == user.Id);
            if (local != null)
            {
                _db.Entry(local).CurrentValues.SetValues(user);
            }
            else
            {
                _db.Attach(user);
                _db.Entry(user).State = EntityState.Modified;
            }
            await _db.SaveChangesAsync();
        }

        public async Task<IEnumerable<User>> ListAsync(string? q = null, int skip = 0, int take = 50)
        {
            IQueryable<User> query = _db.Users.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(q))
            {
                var term = q.Trim();
                query = query.Where(u => (u.Name != null && u.Name.Contains(term)) || (u.Email != null && u.Email.Contains(term)));
            }

            query = query
                .OrderBy(u => u.Name)
                .ThenBy(u => u.Email)
                .Skip(skip)
                .Take(take);

            return await query.ToListAsync();
        }
    }
}
