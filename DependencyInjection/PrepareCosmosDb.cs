using Microsoft.Azure.Cosmos;
using ModularApi.Models.Data;

namespace ModularApi.DependencyInjection
{
    public static class PrepareCosmosDb
    {
        public static async Task<CosmosRepository> CosmosService(IConfigurationSection configurationSection)
        {
            string databaseName = configurationSection.GetSection("DbName").Value;
            string containerName = configurationSection.GetSection("ContainerName").Value;
            string account = configurationSection.GetSection("Account").Value;
            string key = configurationSection.GetSection("Key").Value;
            CosmosClient client = new CosmosClient(account, key);
            CosmosRepository cosmosDbService = new CosmosRepository(client, databaseName, containerName);
            DatabaseResponse database = await client.CreateDatabaseIfNotExistsAsync(databaseName);
            await database.Database.CreateContainerIfNotExistsAsync(containerName, "/id");

            return cosmosDbService;
        }
    }
}
