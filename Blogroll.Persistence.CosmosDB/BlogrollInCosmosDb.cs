using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Blogroll.Common.Links;
using Blogroll.Common.Persistence;
using Microsoft.Azure.Cosmos;

namespace Blogroll.Persistence.CosmosDB
{
    public class BlogrollInCosmosDb: IPersistedBlogroll
    {
        public BlogrollInCosmosDb(IPersistableBlogroll blogroll, CosmosDbClient client)
        {
            _blogroll = blogroll;
            _client = client;
        }

        private readonly IPersistableBlogroll _blogroll;
        private readonly CosmosDbClient _client;
        private bool _initialized;

        public async Task Init()
        {
            if (_initialized)
            {
                return;
            }

            if (_client.Container == null)
            {
                throw new InvalidOperationException("Client is not initialized");
            }

            var query = new QueryDefinition(
                query: "SELECT * FROM links p WHERE p.partitionKey = @key"
            ).WithParameter("@key", @"general");
            using FeedIterator<LinkDto> feed = _client.Container.GetItemQueryIterator<LinkDto>(
                queryDefinition: query
            );
            while (feed.HasMoreResults)
            {
                var response = await feed.ReadNextAsync();
                foreach (var link in response)
                {
                    _blogroll.Add(new BlogRollItem(new Link(link.Name, link.Url, link.FeedUrl), link.Position));
                }
            }
            _initialized = true;
        }

        public void Add(ILink link)
        {
            _blogroll.Add(link);
        }

        public void Remove(ILink link)
        {
            _blogroll.Remove(link);
        }

        public void Move(int oldPosition, int newPosition)
        {
            _blogroll.Move(oldPosition, newPosition);
        }

        public int PositionOf(ILink link)
        {
            return _blogroll.PositionOf(link);
        }

        public ILink Find(int position)
        {
            return _blogroll.Find(position);
        }

        public Task<string> PrintedTo(IBlogRollMediaSource printer)
        {
            return _blogroll.PrintedTo(printer);
        }

        public void Save()
        {
            throw new NotImplementedException();
        }

        private class LinkDto
        {
            public string id { get; set; }
            public string Url { get; set; }
            public string FeedUrl { get; set; }
            public string Name { get; set; }
            public int Position { get; set; }
        }
    }
}
