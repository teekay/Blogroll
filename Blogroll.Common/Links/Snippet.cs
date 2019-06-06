using Blogroll.Common.Media;

namespace Blogroll.Common.Links
{
    /// <summary>
    /// Represents a bit of content read from an ILink.
    /// Keys recognized for printing: Link, Title, Content.
    /// </summary>
    public sealed class Snippet: IPrintable, INullQueryable
    {
        public Snippet(string permalink, string title, string content)
        {
            Permalink = permalink;
            Title = title;
            Content = content;
        }

        public string Permalink { get; }
        public string Title { get; }
        public string Content { get; }

        public static Snippet Empty() => new Snippet(string.Empty, string.Empty, string.Empty);

        public IMedia PrePrinted(IMedia media) =>
            media.With("Link", Permalink)
                .With("Title", Title)
                .With("Content", Content);

        /// <summary>
        /// Prints itself into the provided template.
        /// Available properties:
        /// - Link (URL of the article)
        /// - Title (Title of the article)
        /// - Content (snippet of the article's contents)
        /// </summary>
        /// <param name="media"></param>
        /// <returns></returns>
        public string PrintedTo(IMedia media) => PrePrinted(media).ToString();

        public bool AmEmpty() => string.IsNullOrEmpty(Permalink);
    }
}