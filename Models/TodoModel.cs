using System.ComponentModel.DataAnnotations;

namespace ModularApi.Models
{
    public class TodoModel
    {
        [Required]
        public string Name { get; set; }

        [Required]
        public string Description { get; set; }

        public bool IsDone { get; set; } = false;
    }
}
