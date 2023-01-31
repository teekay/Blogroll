using Blogroll.Common.Links;
using Blogroll.Common.Persistence;
using MySql.Data.MySqlClient;
using Optional;

namespace Blogroll.Persistence.MySQL
{
    public class BlogrollInMysql : IPersistedBlogroll
    {
        public BlogrollInMysql(IPersistableBlogroll blogroll, MySqlConnection connection, IReadsContent reader) 
        {
            Connection = connection;
            _source = blogroll;
            _reader = reader;

            Connection.Open();
        }

        private bool _initialized;
        private readonly IPersistableBlogroll _source;
        private readonly IReadsContent _reader;

        private MySqlConnection Connection { get; }

        private IPersistableBlogroll Source()
        {
            if (!_initialized) InitOnce();
            return _source;
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
            _source.PersistTo(new PersistingInMySQL(Connection, this));
        }

        public void Dispose() => Connection.Dispose();

        public override string ToString() => $"Blogroll in MySQL with {_source}";

        private void InitOnce()
        {
            if (_initialized) return;
            using var createSchemaCommand = Connection.CreateCommand();
            createSchemaCommand.CommandText = @"CREATE TABLE IF NOT EXISTS `Links`(`Url` varchar(768) NOT NULL PRIMARY KEY, `Name` varchar(768), `FeedUrl` varchar(768), `Position` INTEGER)";
            createSchemaCommand.ExecuteNonQuery();

            using var readAllLinksCommand = Connection.CreateCommand();
            readAllLinksCommand.CommandText = @"SELECT * FROM Links ORDER BY Position";
            using var reader = readAllLinksCommand.ExecuteReader();
            while (reader.Read())
            {
                _source.Add(BlogRollItemFromReader(reader));
            }
            _initialized = true;
        }

        private BlogRollItem BlogRollItemFromReader(MySqlDataReader reader) => new(
            new Link(reader["Name"].ToString(),
                reader["Url"].ToString(),
                reader["FeedUrl"].ToString(),
                _reader),
            int.Parse(reader["Position"].ToString() ?? "0"));

        private class PersistingInMySQL : IPersisting
        {
            public PersistingInMySQL(MySqlConnection connection, BlogrollInMysql parent)
            {
                _connection = connection;
                _parent = parent;
            }

            private readonly MySqlConnection _connection;
            private readonly BlogrollInMysql _parent;
            private int _index = 1;

            public Option<object> MaybeFound(ILink link)
            {
                using var cmd = _connection.CreateCommand();
                cmd.CommandText = @"SELECT * FROM Links WHERE Url=@Url";
                cmd.Parameters.AddWithValue("@Url", link.Id());
                using var reader = cmd.ExecuteReader();
                var found = reader.Read();
                return found
                    ? ((object)_parent.BlogRollItemFromReader(reader)).Some()
                    : Option.None<object>();
            }

            public void Update(object existing, ILink updatedLink)
            {
                new MysqlBlogRollItem(_connection, updatedLink, _index, _parent._reader).Save();
                _index += 1;
            }

            public void Insert(ILink link)
            {
                new MysqlBlogRollItem(_connection, link, _index, _parent._reader).Save();
                _index += 1;
            }

            public void Delete(ILink link) => new MysqlBlogRollItem(_connection, link, _parent._reader).Delete();
        }
    }
}