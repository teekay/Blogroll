using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Blogroll.Common.Commons;
using Blogroll.Common.Media;

namespace Blogroll.Common.Links
{
    /// <summary>
    /// Implementation of an ILink. Encapsulates a URL that can be named,
    /// and can contain the URL of its corresponding RSS feed.
    /// The Id() method returns the URL.
    /// Keys recognized for printing are: Name, Url, FeedUrl.
    /// </summary>
    public sealed class Link : ILink, IEquatable<ILink>
    {
        public Link(string url) : this(url, new DoesNotRead()) {}

        public Link(string name, string url) : this(name, url, "", new DoesNotRead()) {}

        public Link(string url, IReadsContent reader) : this("", url, "", reader) {}

        public Link(string name, string url, string feedUrl): this(name, url, feedUrl, new DoesNotRead()) {}

        public Link(string name, string url, string feedUrl, IReadsContent reader)
        {
            _reader = reader;
            Name = new SolidString(name);
            Url = new SolidString(url);
            FeedUrl = new SolidString(feedUrl);
        }

        private readonly IReadsContent _reader;

        public static ILink Empty() => new Link(string.Empty, new DoesNotRead());

        private string Name { get; }

        private string Url { get; }

        private string FeedUrl { get; }

        public string Id() => Url;

        public IMedia PrePrinted(IMedia media) => media.With(nameof(Url), Url)
            .With(nameof(Name), Name)
            .With(nameof(FeedUrl), FeedUrl);

        public string PrintedTo(IMedia media) =>
            PrePrinted(media).ToString();

        public bool CanRead() => !string.IsNullOrEmpty(FeedUrl);

        public async Task<Snippet> LatestContent()
        {
            try
            {
                var latest = (await _reader.Content(FeedUrl == string.Empty
                    ? Url
                    : FeedUrl)).FirstOrDefault();
                return latest ?? Snippet.Empty();
            }
            catch (Exception e)
            {
                Trace.WriteLine($"Could not fetch the feed: {e.Message}");
                return Snippet.Empty();
            }
        }

        public bool AmEmpty() => string.IsNullOrEmpty(Url);

        public void Save(Action<string, string, string> saveImpl) => saveImpl(Name, Url, FeedUrl);

        public bool Equals(ILink other) => other != null && string.Equals(Id(), other.Id());

        public override bool Equals(object obj) => 
            obj != null && (
                ReferenceEquals(this, obj) || 
                obj is ILink other && Equals(other));

        public static bool operator ==(Link one, Link two) => one?.Equals(two) ?? false;

        public static bool operator !=(Link one, Link two) => !(one == two);

        public override int GetHashCode() => Id().GetHashCode();

        public override string ToString() => $"{Name} at {FeedUrl}";
    }
}