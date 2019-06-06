using System.Threading.Tasks;
using Blogroll.Common.Links;

namespace Blogroll.Tests
{
    internal class DummyFeedDiscovery: IFeedDiscovery
    {
        public DummyFeedDiscovery(string title, string url, bool beSuccessful)
        {
            _title = title;
            _url = url;
            _beSuccessful = beSuccessful;
        }

        private readonly string _title;
        private readonly string _url;
        private readonly bool _beSuccessful;

#pragma warning disable 1998
        public async Task<(string Title, string Url)> DiscoveredFeed(string url)
#pragma warning restore 1998
        {
            return _beSuccessful
                ? (_title, _url)
                : (string.Empty, string.Empty);
        }
    }
}
