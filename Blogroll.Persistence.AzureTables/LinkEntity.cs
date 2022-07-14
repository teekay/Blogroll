using Azure;
using Azure.Data.Tables;

namespace Blogroll.Persistence.AzureTables
{
    internal class LinkEntity : ITableEntity
    {
        public string Url { get; set; }
        public string FeedUrl { get; set; }
        public string Name { get; set; }
        public int Position { get; set; }
        public string PartitionKey { get; set; }
        public string RowKey { get; set; }
        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }
    }
}
