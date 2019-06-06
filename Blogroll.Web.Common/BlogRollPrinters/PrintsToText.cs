using Blogroll.Common.Links;
using Blogroll.Common.Media;

namespace Blogroll.Web.Common.BlogRollPrinters
{
    /// <summary>
    /// Specialized class that helps IBlogRoll to print itself into (presumably) plain text.
    /// </summary>
    public sealed class PrintsToText : IBlogRollMediaSource
    {
        public PrintsToText(string masterTemplate, string containingTemplate, string linkTemplate, string snippetTemplate, string separator)
        {
            _masterTemplate = masterTemplate;
            _containingTemplate = containingTemplate;
            _linkTemplate = linkTemplate;
            _snippetTemplate = snippetTemplate;
            _separator = separator;
        }

        private readonly string _masterTemplate;
        private readonly string _containingTemplate;
        private readonly string _linkTemplate;
        private readonly string _snippetTemplate;
        private readonly string _separator;

        public IMedia MasterTemplate() => new TextMedia(_masterTemplate);
        public IMedia ContainingTemplate() => new TextMedia(_containingTemplate);
        public IMedia LinkTemplate() => new TextMedia(_linkTemplate);
        public IMedia SnippetTemplate() => new TextMedia(_snippetTemplate);
        public string Separator() => _separator;
    }
}
