using Blogroll.Common.Links;
using MySql.Data.MySqlClient;

namespace Blogroll.Persistence.MySQL
{
    internal class MysqlBlogRollItem
    {
        public MysqlBlogRollItem(MySqlConnection db, ILink link, IReadsContent reader)
        {
            _db = db;
            _link = link;
            _reader = reader;
            ReadFromSource(link);
        }

        public MysqlBlogRollItem(MySqlConnection db, ILink link, int position, IReadsContent reader) : this(db, link, reader)
        {
            Position = position;
        }

        private readonly MySqlConnection _db;
        private readonly ILink _link;
        private readonly IReadsContent _reader;
        private string Name { get; set; } = string.Empty;
        private string Url { get; set; } = string.Empty;
        private string FeedUrl { get; set; } = string.Empty;
        private int Position { get; }

        private void ReadFromSource(ILink source) => source.Save(ReadLink);

        private void ReadLink(string name, string url, string feedUrl)
        {
            Name = name;
            Url = url;
            FeedUrl = feedUrl;
        }

        private BlogRollItem ToBlogRollItem() => new(new Link(Name, Url, FeedUrl, _reader), Position);

        private BlogRollItem BlogRollItemFromReader(MySqlDataReader reader) => new(
            new Link(reader["Name"].ToString(),
                reader["Url"].ToString(),
                reader["FeedUrl"].ToString(),
                _reader),
            int.Parse(reader["Position"].ToString() ?? "0"));

        public static implicit operator BlogRollItem(MysqlBlogRollItem value) => value.ToBlogRollItem();

        public void Save()
        {
            using var cmd = _db.CreateCommand();
            cmd.CommandText = @"SELECT * FROM Links WHERE Url=@Url";
            cmd.Parameters.AddWithValue("@Url", _link.Id());

            using var reader = cmd.ExecuteReader();
            var found = reader.Read();
            reader.Close();

            var action = found ? (Action<MySqlDataReader>)Update : Insert;
            action.Invoke(reader);
        }

        private void Insert(MySqlDataReader reader)
        {
            using var cmd = _db.CreateCommand();
            cmd.CommandText = @"INSERT INTO Links(Url, Name, FeedUrl, Position) Values(@Url, @Name, @FeedUrl, @Position)";
            cmd.Parameters.AddWithValue("@Url", Url);
            cmd.Parameters.AddWithValue("@Name", Name);
            cmd.Parameters.AddWithValue("@FeedUrl", FeedUrl);
            cmd.Parameters.AddWithValue("@Position", Position);
            cmd.Parameters.AddWithValue("@OriginalUrl", _link.Id());

            cmd.ExecuteNonQuery();
        }

        private void Update(MySqlDataReader reader)
        {
            var original = BlogRollItemFromReader(reader);
            using var cmd = _db.CreateCommand();
            cmd.CommandText = @"UPDATE Links SET Url=@Url, Name=@Name, FeedUrl=@FeedUrl, Position=@Position WHERE Url=@OriginalUrl";
            cmd.Parameters.AddWithValue("@Url", Url);
            cmd.Parameters.AddWithValue("@Name", Name);
            cmd.Parameters.AddWithValue("@FeedUrl", FeedUrl);
            cmd.Parameters.AddWithValue("@Position", Position);
            cmd.Parameters.AddWithValue("@OriginalUrl", original.Id());

            cmd.ExecuteNonQuery();
        }

        public void Delete()
        {
            using var cmd = _db.CreateCommand();
            cmd.CommandText = @"DELETE FROM Links WHERE Url=@Url";
            cmd.Parameters.AddWithValue("@Url", _link.Id());
            
            cmd.ExecuteNonQuery();
        }

        public override string ToString() => $"{Url} in MySQL";
    }
}
