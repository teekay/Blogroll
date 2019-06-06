using Blogroll.Common.Links;
using LiteDB;

namespace Blogroll.Persistence.LiteDB
{
    /// <summary>
    /// Encapsulates a BsonDocument with stored properties of a BlogRollItem.
    /// Provides conversions between the two.
    /// </summary>
    internal sealed class LiteDbBlogRollItem
    {
        private LiteDbBlogRollItem(IReadsContent reader)
        {
            _reader = reader;
        }

        public LiteDbBlogRollItem(BsonDocument document, IReadsContent reader): this(reader)
        {
            _document = document;
        }

        public LiteDbBlogRollItem(BsonDocument document, int position, IReadsContent reader): this(reader)
        {
            _document = document;
            _position = position;
        }

        public LiteDbBlogRollItem(ILink link, int position, IReadsContent reader): this(reader)
        {
            _position = position;
            _document = new BsonDocument();
            ReadFromSource(link);
        }

        public LiteDbBlogRollItem(BsonDocument document, ILink link, int position, IReadsContent reader) : this(document, position, reader)
        {
            ReadFromSource(link);
        }

        private readonly BsonDocument _document;
        private readonly IReadsContent _reader;
        private readonly int _position;

        public static implicit operator BsonDocument(LiteDbBlogRollItem value) => value.Document();
        public static implicit operator BlogRollItem(LiteDbBlogRollItem value) => value.ToBlogRollItem();

        private BlogRollItem ToBlogRollItem() =>
            new BlogRollItem(new Link(_document["Name"], _document["Url"], _document["FeedUrl"], _reader),
                _document["Position"]);

        private BsonDocument Document() => _document;

        private void ReadFromSource(ILink source)
        {
            source.Save(ReadLink);
            _document["Position"] = _position;
        }

        private void ReadLink(string name, string url, string feedUrl)
        {
            _document["Name"] = name;
            _document["Url"] = url;
            _document["FeedUrl"] = feedUrl;
        }

        public override string ToString() => $"{_document["Url"]} - {_document["_id"]}";
    }
}