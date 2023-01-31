using Azure.Data.Tables;
using Blogroll.Common.Links;

namespace Blogroll.Persistence.AzureTables
{
    internal class AzureTableBlogrollItem
    {
        public AzureTableBlogrollItem(TableClient db, ILink link, IReadsContent reader)
        {
            _db = db;
            _link = link;
            _reader = reader;
            ReadFromSource(link);
        }

        public AzureTableBlogrollItem(TableClient db, ILink link, int position, IReadsContent reader) : this(db, link, reader)
        {
            Position = position;
        }

        private readonly TableClient _db;
        private readonly ILink _link;
        private readonly IReadsContent _reader;
        private string? Name { get; set; }
        private string? Url { get; set; }
        private string? FeedUrl { get; set; }
        private int Position { get; }

        private void ReadFromSource(ILink source) => source.Save(ReadLink);

        private void ReadLink(string name, string url, string feedUrl)
        {
            Name = name;
            Url = url;
            FeedUrl = feedUrl;
        }

        private BlogRollItem ToBlogRollItem() => new BlogRollItem(new Link(Name, Url, FeedUrl, _reader), Position);

        public static implicit operator BlogRollItem(AzureTableBlogrollItem value) => value.ToBlogRollItem();



        public void Save()
        {
            var entity = new LinkEntity
            {
                Name = Name,
                Url = Url,
                FeedUrl = FeedUrl,
                RowKey = Url.GetHashCode().ToString(),
                Position = Position,
                PartitionKey = @"general"
            };
            _db.UpsertEntity(entity);
        }

        public void Delete() => _db.DeleteEntity(@"general", Url);

        public override string ToString() => $"{Url} in Azure Table";
    }
}
