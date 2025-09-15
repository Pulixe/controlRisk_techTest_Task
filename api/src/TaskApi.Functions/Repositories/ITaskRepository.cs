using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TaskApi.Functions.Models;

namespace TaskApi.Functions.Repositories
{
    public interface ITaskRepository
    {
        Task<TaskItem?> GetAsync(Guid id);
        Task<IEnumerable<TaskItem>> ListAsync(
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
            DateTime? dueTo = null);
        Task CreateAsync(TaskItem item);
        Task UpdateAsync(TaskItem item);
        Task DeleteAsync(Guid id);
    }
}
