using TaskApi.Functions.Models;

namespace TaskApi.Functions.Factories
{
    public interface ITaskFactory
    {
        TaskItem Create(string title, string description, DateTime? dueDate, string createdBy, string? assignedTo = null);
    }

    public class TaskFactory : ITaskFactory
    {
        public TaskItem Create(string title, string description, DateTime? dueDate, string createdBy, string? assignedTo = null)
        {
            return new TaskItem
            {
                Title = title,
                Description = description,
                DueDate = dueDate,
                CreatedBy = createdBy,
                AssignedTo = assignedTo ?? "",
                Status = Models.TaskStatus.Pending,
            };
        }
    }
}
