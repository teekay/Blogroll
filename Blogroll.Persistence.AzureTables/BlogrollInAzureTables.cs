using Azure;
using Azure.Data.Tables;
using Blogroll.Common.Links;
using Blogroll.Common.Persistence;
using Blogroll.Web.Common;
using Optional;

namespace Blogroll.Persistence.AzureTables
{
    public class BlogrollInAzureTables: IPersistedBlogroll
    {
        public BlogrollInAzureTables(TableClient client, IPersistableBlogroll blogroll, IReadsContent reader)
        {
            _client = client;
            _blogroll = blogroll;
            _reader = reader;
        }

        private void InitOnce()
        {
            if (_initialized)
            {
                return;
            }

            _client.CreateIfNotExists();
            var x = _client.Query<LinkEntity>(filter: $"PartitionKey eq 'general'");
            foreach (var link in x)
            {
                _blogroll.Add(BlogRollItemFromEntity(link));
            }

            _initialized = true;
        }

        private readonly TableClient _client;
        private readonly IPersistableBlogroll _blogroll;
        private readonly IReadsContent _reader;
        private bool _initialized;

        private IPersistableBlogroll Source()
        {
            InitOnce();
            return _blogroll;
        }

        private BlogRollItem BlogRollItemFromEntity(LinkEntity link) => new BlogRollItem(
            new Link(
                link.Name,
                link.Url,
                link.FeedUrl,
                new ReadsFeedWithFeedReader()
            ), link.Position);

        public void Add(ILink link) => Source().Add(link);

        public void Remove(ILink link) => Source().Remove(link);

        public void Move(int oldPosition, int newPosition) => Source().Move(oldPosition, newPosition);

        public int PositionOf(ILink link) => Source().PositionOf(link);

        public ILink Find(int position) => Source().Find(position);

        public Task<string> PrintedTo(IBlogRollMediaSource printer) => Source().PrintedTo(printer);

        public void Save() => Source().PersistTo(new PersistingInAzureTables(_client, this));

        public void Dispose()
        {
        }

        private class PersistingInAzureTables : IPersisting
        {
            public PersistingInAzureTables(TableClient client, BlogrollInAzureTables parent)
            {
                _client = client;
                _parent = parent;
            }

            private readonly TableClient _client;
            private readonly BlogrollInAzureTables _parent;
            private int _index = 1;


            public Option<object> MaybeFound(ILink link)
            {
                var page = _client.Query<LinkEntity>(ent => ent.Url == link.Id());
                var items = page.ToArray();

                return items.Length == 1 
                    ? ((object)(_parent.BlogRollItemFromEntity(items[0]))).Some() 
                    : Option.None<object>();
            }

            public void Update(object existing, ILink updatedLink)
            {
                new AzureTableBlogrollItem(_client, updatedLink, _index, _parent._reader).Save();
                _index++;
            }

            public void Insert(ILink link)
            {
                new AzureTableBlogrollItem(_client, link, _index, _parent._reader).Save();
                _index++;
            }

            public void Delete(ILink link) => new AzureTableBlogrollItem(_client, link, _parent._reader).Delete();
        }
    }
}