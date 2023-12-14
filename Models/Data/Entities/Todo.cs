using System.ComponentModel.DataAnnotations;

namespace ModularApi.Models.Data.Entities
{
    public class Todo
    {
        [Key]
        public string id { get; set; } = Guid.NewGuid().ToString();

        [Required]
        public string Name { get; set; }

        [Required]
        public string Description { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public bool IsDone { get; set; } = false;
    }
}
