#nullable enable
using System;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;

namespace Blogroll.Persistence.CosmosDB
{
    public class CosmosDbClient
    {
        public CosmosDbClient(string endpoint, string apiKey)
        {
            _endpoint = endpoint;
            _apiKey = apiKey;
        }

        private readonly string _endpoint;
        private readonly string _apiKey;
        public Container? Container { get; private set; }

        public async Task Setup()
        {
            using var client = new CosmosClient(
                accountEndpoint: _endpoint,
                authKeyOrResourceToken: _apiKey
            );
            var database = await client.CreateDatabaseIfNotExistsAsync(
                id: "blogroll"
            );
            var db = database.Database;

            Container = await db.CreateContainerIfNotExistsAsync(
                id: "links",
                partitionKeyPath: "/category",
                throughput: 400
            );
        }
    }
}
