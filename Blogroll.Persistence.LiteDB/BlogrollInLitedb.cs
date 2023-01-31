using System.Linq;
using System.Threading.Tasks;
using Blogroll.Common.Links;
using Blogroll.Common.Persistence;
using LiteDB;
using Optional;

namespace Blogroll.Persistence.LiteDB
{
    /// <summary>
    /// Encapsulates the provided IPersistableBlogroll instance,
    /// proxies its operations, and provides persistence functionality in LiteDB.
    /// </summary>
    public sealed class BlogrollInLitedb : IPersistedBlogroll
    {
        public BlogrollInLitedb(IPersistableBlogroll blogroll, string pathToDatabase, IReadsContent reader)
        {
            _source = blogroll;
            _pathToDatabase = pathToDatabase;
            _reader = reader;
        }

        private readonly IPersistableBlogroll _source;
        private readonly string _pathToDatabase;
        private readonly IReadsContent _reader;
        private bool _initialized;

        private void InitOnce()
        {
            if (_initialized) return;
            using var db = new LiteDatabase(_pathToDatabase);
            var col = Collection(db);
            col.EnsureIndex("Url", true);
            var ordered = col.FindAll().OrderBy(x => x["Position"]).ToList();
            var indexes = Enumerable.Range(0, ordered.Count);
            indexes.Select(index => (BlogRollItem)new LiteDbBlogRollItem(ordered[index], _reader)).ToList().ForEach(_source.Add);
            _initialized = true;
        }

        private LiteCollection<BsonDocument> Collection(LiteDatabase db) => db.GetCollection("BlogRollItemInLitedb");

        private IPersistableBlogroll Source()
        {
            if (!_initialized) InitOnce();
            return _source;
        }

        public void Add(ILink link) => Source().Add(link);
        public void Remove(ILink link) => Source().Remove(link);
        public void Move(int oldPosition, int newPosition) => Source().Move(oldPosition, newPosition);
        public int PositionOf(ILink link) => Source().PositionOf(link);
        public async Task<string> PrintedTo(IBlogRollMediaSource printer) => await Source().PrintedTo(printer);
        public ILink Find(int position) => Source().Find(position);

        public void Save()
        {
            using var db = new LiteDatabase(_pathToDatabase);
            Source().PersistTo(new PersistingInLiteDb(Collection(db), this));
        }

        public void Dispose() { }

        private class PersistingInLiteDb : IPersisting
        {
            public PersistingInLiteDb(LiteCollection<BsonDocument> collection, BlogrollInLitedb parent)
            {
                _collection = collection;
                _parent = parent;
            }

            private readonly LiteCollection<BsonDocument> _collection;
            private readonly BlogrollInLitedb _parent;
            private int _index = 1;

            public Option<object> MaybeFound(ILink link) => 
                ((object) _collection.FindOne(x => x["Url"].Equals(link.Id())))?.Some() ?? Option.None<object>();

            public void Update(object existing, ILink updatedLink)
            {
                _collection.Update(new LiteDbBlogRollItem((BsonDocument) existing, updatedLink, _index, _parent._reader));
                _index += 1;
            }

            public void Insert(ILink link)
            {
                _collection.Insert(new LiteDbBlogRollItem(link, _index, _parent._reader));
                _index += 1;
            }

            public void Delete(ILink link) => 
                _collection.FindOne(x => x["Url"].Equals(link.Id()))
                    .Some()
                    .MatchSome(doc => _collection.Delete(doc["_id"]));
        }

        public override string ToString() => $"Blogroll in LiteDB with {Source()}";
    }
}