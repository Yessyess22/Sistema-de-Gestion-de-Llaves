using System;
using System.ComponentModel.DataAnnotations;

namespace SistemaGestionLlaves.Models
{
    public class Auditoria
    {
        public int Id { get; set; }
        public string? UserId { get; set; }
        public string? Type { get; set; } // Create, Update, Delete
        public string? TableName { get; set; }
        public DateTime DateTime { get; set; }
        public string? OldValues { get; set; } // JSON
        public string? NewValues { get; set; } // JSON
        public string? AffectedColumns { get; set; }
        public string? PrimaryKey { get; set; }
    }
}
