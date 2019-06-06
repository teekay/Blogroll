using System.Collections.Generic;
using Blogroll.Common.Links;
using Blogroll.Common.Media;
using Blogroll.Web.Common.Media;
using Optional;

namespace Blogroll.Web.Common.BlogRollPrinters
{
    /// <summary>
    /// Specialized class that helps IBlogRoll to print itself into a JSON-encoded string.
    /// </summary>
    public sealed class PrintsToJson : IBlogRollMediaSource
    {
        public PrintsToJson(bool withSnippet)
        {
            _withSnippet = withSnippet;
        }

        private readonly bool _withSnippet;
        private IEnumerable<string> ContainingTemplateContents() =>
            _withSnippet ? new List<string> { "Link", "Snippet" } : new List<string> { "Link" };

        public IMedia MasterTemplate() => new TextMedia("[{{Links}}]");
        public IMedia ContainingTemplate() => new JsonTypedMedia(new JsonTerms(),
            ContainingTemplateContents().Some());
        public IMedia LinkTemplate() => new JsonMedia(new JsonTerms(),
            ((IEnumerable<string>)new List<string> { "Name", "Url", "FeedUrl" }).Some());
        public IMedia SnippetTemplate() => new JsonMedia();
        public string Separator() => ",";
    }
}
