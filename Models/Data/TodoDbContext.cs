using Microsoft.EntityFrameworkCore;
using ModularApi.Models.Data.Entities;

namespace ModularApi.Models.Data
{
    public class TodoDbContext : DbContext
    {
        private readonly IConfiguration _config;

        public TodoDbContext(DbContextOptions<TodoDbContext> options, IConfiguration config)
            : base(options)
        {
            _config = config;
        }
        public DbSet<Todo>? Todos { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            try 
            {
                optionsBuilder.UseSqlServer(_config.GetConnectionString("DefaultConnection"));
            }catch (Exception ex)
            {
                optionsBuilder.UseSqlServer("Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=TodoApi;Integrated Security=True;Connect Timeout=30;");
            }
        }
    }
}
