using ModularApi.Models.Data.Entities;

namespace ModularApi.Models.Data
{
    public class InMemoryRepository : ITodoRepository
    {
        private static List<Todo> _todoList = new List<Todo>() {
            new Todo {
                id = "e55cfd0f-ab76-422f-b345-9de48d2dd6e9",
                Name = "Test name",
                Description = "Some description",
                CreatedDate =  DateTime.Now,
                IsDone = false }
        };

        public Task<Todo> GetTodoById(string uid)
        {
            var todo = _todoList.Find(t => t.id == uid);
            return Task.FromResult(todo);
        }

        public Task<Todo> GetTodoByName(string name)
        {
            var todo = _todoList.Find(t => t.Name == name);
            return Task.FromResult(todo);
        }

        public Task<Todo[]> GetAllTodos()
        {
            return Task.FromResult(_todoList.ToArray());
        }

        public Task<Todo> AddTodo(Todo t)
        {
            if (t == null) throw new ArgumentNullException(nameof(t));
            if (string.IsNullOrEmpty(t.id))
            {
                t.id = Guid.NewGuid().ToString();
            }

            _todoList.Add(t);
            return Task.FromResult(t);

        }

        public Task<Todo> DeleteTodo(Todo t)
        {
            if (t == null) throw new ArgumentNullException(nameof(t));
            var todo = _todoList.Find(todoItem => todoItem.id == t.id);
            if (todo != null)
            {
                _todoList.Remove(todo);
            }
            return Task.FromResult(t);
        }

        public Task<bool> Commit()
        {
            return Task.FromResult(true);
        }
    }
}
