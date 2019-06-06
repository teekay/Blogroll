using System.Collections.Generic;
using Blogroll.Common.Links;
using Blogroll.Common.Media;
using Blogroll.Web.Common.Media;
using Optional;

namespace Blogroll.Web.Common.BlogRollPrinters
{
    /// <summary>
    /// Specialized class that helps IBlogRoll to print itself into (presumably) HTML.
    /// </summary>
    public sealed class PrintsToHtml : IBlogRollMediaSource
    {
        public PrintsToHtml(string masterTemplate, string containingTemplate,
            string linkTemplate, string snippetTemplate, string separator, bool withSnippet)
        {
            _masterTemplate = masterTemplate;
            _containingTemplate = containingTemplate;
            _linkTemplate = linkTemplate;
            _snippetTemplate = snippetTemplate;
            _separator = separator;
            _withSnippet = withSnippet;
        }

        private readonly string _masterTemplate;
        private readonly string _containingTemplate;
        private readonly string _linkTemplate;
        private readonly string _snippetTemplate;
        private readonly string _separator;
        private readonly bool _withSnippet;

        private IEnumerable<string> ContainingTemplateContents() =>
            _withSnippet ? new List<string> { "Link", "Snippet" } : new List<string> { "Link" };

        public IMedia MasterTemplate() => new HtmlMedia(_masterTemplate);
        public IMedia ContainingTemplate() => _withSnippet
            ? new HtmlMedia(_containingTemplate)
            : new HtmlMedia(_containingTemplate, ContainingTemplateContents().Some());
        public IMedia LinkTemplate() => new HtmlMedia(_linkTemplate);
        public IMedia SnippetTemplate() => new HtmlMedia(_snippetTemplate);
        public string Separator() => _separator;
    }
}
