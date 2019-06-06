using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Blogroll.Common.Commons;
using Blogroll.Common.Links;

namespace Blogroll.Web.Common
{
    /// <summary>
    /// Encapsulates key-value pairs presumably coming from an Http request,
    /// and can generate an ILink instance from them.
    /// Will use the provided IFeedDiscovery to look-up the RSS feed associated with the link.
    /// </summary>
    public class LinkFromRequest
    {
        public LinkFromRequest(IEnumerable<KeyValuePair<string, string>> formInputs, 
            IFeedDiscovery feedDiscovery): this(formInputs, feedDiscovery, new DoesNotRead()) {}

        public LinkFromRequest(IEnumerable<KeyValuePair<string, string>> formInputs, 
            IFeedDiscovery feedDiscovery, IReadsContent reader)
        {
            _feedDiscovery = feedDiscovery;
            _reader = reader;
            var inputs = formInputs.ToList();
            _url = new SolidString(inputs.FirstOrDefault(x => x.Key.Equals("Url")).Value);
            _name = new SolidString(inputs.FirstOrDefault(x => x.Key.Equals("Name")).Value);
            _feedUrl = new SolidString(inputs.FirstOrDefault(x => x.Key.Equals("FeedUrl")).Value);
        }

        private readonly string _name;
        private readonly string _url;
        private readonly string _feedUrl;
        private readonly IFeedDiscovery _feedDiscovery;
        private readonly IReadsContent _reader;

        public async Task<ILink> Link()
        {
            var link = new Link(_name, _url, _feedUrl, _reader);
            return link.CanRead()
                ? link
                : await Discovered(link);
        }

        private async Task<ILink> Discovered(ILink link)
        {
            var (title, feedUrl) = await _feedDiscovery.DiscoveredFeed(_url);
            return string.IsNullOrEmpty(feedUrl)
                ? link
                : new Link((_name != string.Empty ? _name : title), _url, feedUrl, _reader);
        }
    }
}
