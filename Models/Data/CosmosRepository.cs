using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;
using ModularApi.Models.Data.Entities;

namespace ModularApi.Models.Data
{
    public class CosmosRepository : ITodoRepository
    {
        private readonly Container _container;

        public CosmosRepository(CosmosClient client, string databaseName, string containerName)
        {
            _container = client.GetContainer(databaseName, containerName);
        }

        public async Task<Todo[]> GetAllTodos()
        {
            FeedIterator<Todo> todos = _container.GetItemQueryIterator<Todo>(new QueryDefinition("SELECT * FROM Todos"));
            List<Todo> todoList = new List<Todo>();
            while (todos.HasMoreResults)
            {
                todoList.AddRange(await todos.ReadNextAsync());
            }
            return todoList.ToArray();
        }

        public async Task<Todo> GetTodoById(string uid)
        {
            try
            {
                ItemResponse<Todo> response = await _container.ReadItemAsync<Todo>(uid.ToString(), new PartitionKey(uid));
                return response.Resource;
            }
            catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return null;
            }
        }

        public async Task<Todo> GetTodoByName(string name)
        {
            try
            {
                var queryable = _container.GetItemLinqQueryable<Todo>();
                var iterator = queryable.Where(t => t.Name == name).ToFeedIterator();
                List<Todo> todoList = new List<Todo>();
                todoList.AddRange(await iterator.ReadNextAsync());
                return todoList.FirstOrDefault();
            }
            catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return null;
            }
        }

        public async Task<Todo> AddTodo(Todo t)
        {
            ItemResponse<Todo> response = await _container.CreateItemAsync(t, new PartitionKey(t.id));
            return response.Resource;
        }

        public async Task<Todo> UpdateTodo(string id, Todo t)
        {
            return await _container.ReplaceItemAsync<Todo>(t, id, new PartitionKey(id));
        }

        public async Task<Todo> DeleteTodo(Todo t)
        {
            ItemResponse<Todo> response = await _container.DeleteItemAsync<Todo>(t.id, new PartitionKey(t.id));
            return response.Resource;
        }

        public async Task<bool> DeleteTodoById(Todo t)
        {
            try
            {
                await _container.DeleteItemAsync<Todo>(t.id, new PartitionKey(t.id));
                return true;
            }
            catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return false;
            }
        }

        public Task<bool> Commit()
        {
            return Task.FromResult(true);
        }
    }
}
