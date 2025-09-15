using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TaskApi.Functions.Data;
using TaskApi.Functions.Models;

namespace TaskApi.Functions.Repositories
{
    public class TaskRepository : ITaskRepository
    {
        private readonly AppDbContext _db;
        public TaskRepository(AppDbContext db) => _db = db;

        public async Task CreateAsync(TaskItem item)
        {
            _db.Tasks.Add(item);
            await _db.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var t = await _db.Tasks.FindAsync(id);
            if (t == null) return;
            _db.Tasks.Remove(t);
            await _db.SaveChangesAsync();
        }

        public async Task<TaskItem?> GetAsync(Guid id)
        {
            return await _db.Tasks.AsNoTracking().FirstOrDefaultAsync(t => t.Id == id);
        }

        public async Task<IEnumerable<TaskItem>> ListAsync(
            string? status = null,
            string? assignedTo = null,
            string? createdBy = null,
            string? sortBy = null,
            bool desc = false,
            int skip = 0,
            int take = 50,
            Guid? userId = null,
            string? title = null,
            string? q = null,
            DateTime? dueFrom = null,
            DateTime? dueTo = null)
        {
            IQueryable<TaskItem> query = _db.Tasks.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<Models.TaskStatus>(status, true, out var st))
                query = query.Where(t => t.Status == st);

            if (!string.IsNullOrWhiteSpace(assignedTo))
                query = query.Where(t => t.AssignedTo == assignedTo);

            if (!string.IsNullOrWhiteSpace(createdBy))
                query = query.Where(t => t.CreatedBy == createdBy);

            if (userId.HasValue)
                query = query.Where(t => t.UserId == userId.Value);

            // Title contains filter
            if (!string.IsNullOrWhiteSpace(title))
            {
                var term = title.Trim();
                query = query.Where(t => EF.Functions.Like(t.Title, $"%{term}%"));
            }

            // Free text over title and description
            if (!string.IsNullOrWhiteSpace(q))
            {
                var term = q!.Trim();
                query = query.Where(t => EF.Functions.Like(t.Title, $"%{term}%") || EF.Functions.Like(t.Description, $"%{term}%"));
            }

            // Due date range
            if (dueFrom.HasValue)
                query = query.Where(t => t.DueDate >= dueFrom.Value);
            if (dueTo.HasValue)
                query = query.Where(t => t.DueDate < dueTo.Value);

            // sorting
            query = sortBy?.ToLowerInvariant() switch
            {
                "duedate" => desc ? query.OrderByDescending(t => t.DueDate) : query.OrderBy(t => t.DueDate),
                "createdat" => desc ? query.OrderByDescending(t => t.CreatedAt) : query.OrderBy(t => t.CreatedAt),
                "title" => desc ? query.OrderByDescending(t => t.Title) : query.OrderBy(t => t.Title),
                _ => query.OrderBy(t => t.CreatedAt)
            };

            return await query.Skip(skip).Take(take).ToListAsync();
        }

        public async Task UpdateAsync(TaskItem item)
        {
            var existing = await _db.Tasks.FindAsync(item.Id);
            if (existing == null) throw new InvalidOperationException("Task not found");
            existing.Title = item.Title;
            existing.Description = item.Description;
            existing.DueDate = item.DueDate;
            existing.Status = item.Status;
            existing.AssignedTo = item.AssignedTo;
            existing.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
        }
    }
}
