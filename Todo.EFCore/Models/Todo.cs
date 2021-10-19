using System.ComponentModel.DataAnnotations;

namespace EFCoreMigrationWeb.Models
{
    class Todo
    {
        public int Id { get; set; }
        [Required]
        public string? Title { get; set; }
        public bool IsComplete { get; set; }
        public int Priority { get; set; }
    }
}
