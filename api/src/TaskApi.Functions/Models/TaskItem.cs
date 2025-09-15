using System;

namespace TaskApi.Functions.Models
{
    public enum TaskStatus { Pending = 0, InProgress = 1, Done = 2 }

    public class TaskItem
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid UserId { get; set; }
        public string Title { get; set; } = "";
        public string Description { get; set; } = "";
        public DateTime? DueDate { get; set; }
        public TaskStatus Status { get; set; } = TaskStatus.Pending;
        public string CreatedBy { get; set; } = "";
        public string AssignedTo { get; set; } = "";
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Navigation
        public User? User { get; set; }
    }
}
