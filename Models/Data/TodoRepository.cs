using Microsoft.EntityFrameworkCore;
using ModularApi.Models.Data.Entities;

namespace ModularApi.Models.Data
{
    public class TodoRepository : ITodoRepository
    {
        private readonly TodoDbContext _dbContext;
        private readonly ILogger _factoryLogger;

        public TodoRepository(TodoDbContext dbContext, ILoggerFactory loggerFactory)
        {
            _dbContext = dbContext;
            _factoryLogger = loggerFactory.CreateLogger("RepositoryFactoryLogger");
        }

        public async Task<Todo> GetTodoById(string uid)
        {
            _factoryLogger.LogInformation($"(F) Getting todo with id {uid}");
            return await _dbContext.Todos.Where(t => t.id == uid).FirstOrDefaultAsync();
        }

        public async Task<Todo> GetTodoByName(string name)
        {
            _factoryLogger.LogInformation($"(F) Getting todo with name {name}");
            return await _dbContext.Todos.Where(t => t.Name == name).FirstOrDefaultAsync();
        }

        public async Task<Todo[]> GetAllTodos()
        {
            _factoryLogger.LogInformation($"(F) Getting all todos");
            return await _dbContext.Todos.ToArrayAsync();
        }

        public async Task<Todo> AddTodo(Todo t)
        {
            _factoryLogger.LogInformation($"(F) Adding todo with name {t.Name}");
            await _dbContext.Todos.AddAsync(t);
            return t;
        }

        public async Task<Todo> DeleteTodo(Todo t)
        {
            _factoryLogger.LogInformation($"(F) Deleting todo with id {t.id}");
            _dbContext.Todos.Remove(t);
            return t;
        }

        public async Task<bool> Commit()
        {
            _factoryLogger.LogInformation($"(F) Saving changes");
            return (await _dbContext.SaveChangesAsync()) > 0;
        }
    }
}
