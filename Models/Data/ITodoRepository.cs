using ModularApi.Models.Data.Entities;

namespace ModularApi.Models.Data
{
    public interface ITodoRepository
    {
        public Task<Todo> GetTodoById(string uid);
        public Task<Todo> GetTodoByName(string name);
        public Task<Todo[]> GetAllTodos();
        public Task<Todo> AddTodo(Todo t);
        public Task<Todo> DeleteTodo(Todo t);
        public Task<bool> Commit();
    }
}
