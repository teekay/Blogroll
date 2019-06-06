using System;
using System.Data.SQLite;
using Blogroll.Common.Links;

namespace Blogroll.Persistence.SQLite
{
    /// <summary>
    /// Encapsulates an ILink instance and can save it in a SQLite database.
    /// </summary>
    internal sealed class SqliteBlogRollItem
    {
        public SqliteBlogRollItem(SQLiteConnection db, ILink link, IReadsContent reader)
        {
            _db = db;
            _link = link;
            _reader = reader;
            ReadFromSource(link);
        }

        public SqliteBlogRollItem(SQLiteConnection db, ILink link, int position, IReadsContent reader) : this(db, link, reader)
        {
            Position = position;
        }

        private readonly SQLiteConnection _db;
        private readonly ILink _link;
        private readonly IReadsContent _reader;
        private string Name { get; set; }
        private string Url { get; set; }
        private string FeedUrl { get; set; }
        private int Position { get; }

        private void ReadFromSource(ILink source) => source.Save(ReadLink);

        private void ReadLink(string name, string url, string feedUrl)
        {
            Name = name;
            Url = url;
            FeedUrl = feedUrl;
        }

        private BlogRollItem ToBlogRollItem() => new BlogRollItem(new Link(Name, Url, FeedUrl, _reader), Position);

        private BlogRollItem BlogRollItemFromReader(SQLiteDataReader reader) => new BlogRollItem(
            new Link(reader["Name"].ToString(),
                reader["Url"].ToString(),
                reader["FeedUrl"].ToString(),
                _reader),
            int.Parse(reader["Position"].ToString()));

        public static implicit operator BlogRollItem(SqliteBlogRollItem value) => value.ToBlogRollItem();

        public void Save()
        {
            using var cmd = new PreparedQuery(_db, "SELECT * FROM Links WHERE Url=@Url", ("@Url", _link.Id())).Command();
            using var reader = cmd.ExecuteReader();
            var found = reader.Read();
            var action = found ? (Action<SQLiteDataReader>)Update : Insert;
            action.Invoke(reader);
        }

        private void Insert(SQLiteDataReader reader)
        {
            using var cmd = new PreparedQuery(_db,
                "INSERT INTO Links(Url, Name, FeedUrl, Position) Values(@Url, @Name, @FeedUrl, @Position)",
            ("@Url", Url), ("@Name", Name), ("@FeedUrl", FeedUrl), ("@Position", Position), ("@OriginalUrl", _link.Id())).Command();
            cmd.ExecuteNonQuery();
        }

        private void Update(SQLiteDataReader reader)
        {
            var original = BlogRollItemFromReader(reader);
            using var cmd = new PreparedQuery(_db,
                "UPDATE Links SET Url=@Url, Name=@Name, FeedUrl=@FeedUrl, Position=@Position WHERE Url=@OriginalUrl",
                ("@Url", Url), ("@Name", Name), ("@FeedUrl", FeedUrl), ("@Position", Position), ("@OriginalUrl", original.Id())).Command();
            cmd.ExecuteNonQuery();
        }

        public void Delete()
        {
            using var cmd = new PreparedQuery(_db, "DELETE FROM Links WHERE Url=@Url", ("@Url", _link.Id())).Command();
            cmd.ExecuteNonQuery();
        }

        public override string ToString() => $"{Url} in SQLite";
    }
}