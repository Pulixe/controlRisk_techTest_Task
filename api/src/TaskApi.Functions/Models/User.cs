using System;
using System.Collections.Generic;

namespace TaskApi.Functions.Models
{
    public class User
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Sub { get; set; } = string.Empty; // OIDC subject (unique per provider/tenant)
        public string? Name { get; set; }
        public string? Email { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? LastLogin { get; set; }

        public ICollection<TaskItem> Tasks { get; set; } = new List<TaskItem>();
    }
}

