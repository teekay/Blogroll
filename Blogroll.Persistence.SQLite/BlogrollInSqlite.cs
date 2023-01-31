using System.Data.SQLite;
using System.Threading.Tasks;
using Blogroll.Common.Links;
using Blogroll.Common.Persistence;
using Optional;

namespace Blogroll.Persistence.SQLite
{
    /// <summary>
    /// Encapsulates the provided IPersistableBlogroll instance,
    /// proxies its operations, and provides persistence functionality in SQLite.
    /// </summary>
    public sealed class BlogrollInSqlite : IPersistedBlogroll
    {
        public BlogrollInSqlite(IPersistableBlogroll blogroll, string pathToDatabase, IReadsContent reader)
        {
            _source = blogroll;
            _pathToDatabase = pathToDatabase;
            _reader = reader;
        }

        private readonly IPersistableBlogroll _source;
        private readonly string _pathToDatabase;
        private readonly IReadsContent _reader;
        private bool _initialized;

        private SQLiteConnection Db()
        {
            var db = new SQLiteConnection($"Data Source={_pathToDatabase}; Version=3; FailIfMissing=False");
            db.Open();
            return db;
        }

        private IPersistableBlogroll Source()
        {
            if (!_initialized) InitOnce();
            return _source;
        }

        private void InitOnce()
        {
            if (_initialized) return;
            using var db = Db();
            using var cmdCreateTable = new PreparedQuery(db,
                "CREATE TABLE IF NOT EXISTS Links(Url TEXT NOT NULL PRIMARY KEY, Name TEXT, FeedUrl TEXT, Position INTEGER)").Command();
            cmdCreateTable.ExecuteNonQuery();
            using var cmd = new PreparedQuery(db, "SELECT * FROM Links ORDER BY Position").Command();
            var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                _source.Add(BlogRollItemFromReader(reader));
            }
            _initialized = true;
        }

        public void Add(ILink link) => Source().Add(link);
        public void Remove(ILink link) => Source().Remove(link);
        public void Move(int oldPosition, int newPosition) => Source().Move(oldPosition, newPosition);
        public int PositionOf(ILink link) => Source().PositionOf(link);
        public ILink Find(int position) => Source().Find(position);
        public Task<string> PrintedTo(IBlogRollMediaSource printer) => Source().PrintedTo(printer);

        public void Save()
        {
            InitOnce();
            using var db = Db();
            _source.PersistTo(new PersistingInSqlite(db, this));
        }

        public void Dispose() { }

        private BlogRollItem BlogRollItemFromReader(SQLiteDataReader reader) => new BlogRollItem(
            new Link(reader["Name"].ToString(),
                reader["Url"].ToString(),
                reader["FeedUrl"].ToString(), 
                _reader),
            int.Parse(reader["Position"].ToString()));

        private class PersistingInSqlite : IPersisting
        {
            public PersistingInSqlite(SQLiteConnection connection, BlogrollInSqlite parent)
            {
                _connection = connection;
                _parent = parent;
            }

            private readonly SQLiteConnection _connection;
            private readonly BlogrollInSqlite _parent;
            private int _index = 1;

            public Option<object> MaybeFound(ILink link)
            {
                using var cmd = new PreparedQuery(_connection, "SELECT * FROM Links WHERE Url=@Url", ("@Url", link.Id())).Command();
                using var reader = cmd.ExecuteReader();
                var found = reader.Read();
                return found 
                    ? ((object)_parent.BlogRollItemFromReader(reader)).Some()
                    : Option.None<object>();
            }

            public void Update(object existing, ILink updatedLink)
            {
                new SqliteBlogRollItem(_connection, updatedLink, _index, _parent._reader).Save();
                _index += 1;
            }

            public void Insert(ILink link)
            {
                new SqliteBlogRollItem(_connection, link, _index, _parent._reader).Save();
                _index += 1;
            }

            public void Delete(ILink link) => new SqliteBlogRollItem(_connection, link, _parent._reader).Delete();
        }

        public override string ToString() => $"Blogroll in SQLite with {_source}";
    }
}