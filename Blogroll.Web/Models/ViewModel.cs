using System.IO;
using System.Threading.Tasks;
using Blogroll.Common.Links;
using Blogroll.Web.Common;
using Blogroll.Web.Common.BlogRollPrinters;
using Blogroll.Web.Common.Common;

namespace Blogroll.Web.Models
{
    /// <summary>
    /// Viewmodel for the Razor views. Encapsulates a blogroll and the selected link, which can be empty but not null.
    /// Since we do not use public properties, the viewmodel proxies all print requests.
    /// </summary>
    public sealed class ViewModel
    {
        public ViewModel(ResolvedContentRoot contentRoot, IBlogRoll blogroll, ILink link)
        {
            _contentRoot = contentRoot;
            _blogroll = blogroll;
            _link = link;
        }

        private readonly ResolvedContentRoot _contentRoot;
        private readonly IBlogRoll _blogroll;
        private readonly ILink _link;

        public async Task<string> Links() => await _blogroll.PrintedTo(new PrintsToHtml("{{{Links}}}",
            File.ReadAllText($"{_contentRoot}/Views/Manage/_link.hbs"),
            File.ReadAllText($"{_contentRoot}/Views/Manage/_linkName.hbs"),
            File.ReadAllText($"{_contentRoot}/Views/Manage/_snippet.hbs"),
            string.Empty, true));

        public string Id() => _blogroll.PositionOf(_link).ToString();

        public string Form(string templatePath) => new FormViewModel($"{_contentRoot}/{templatePath}", _link).ToString();
    }
}