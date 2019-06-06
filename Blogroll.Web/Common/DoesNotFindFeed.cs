using System.Threading.Tasks;
using Blogroll.Common.Links;

namespace Blogroll.Web.Common
{
    /// <summary>
    /// While this looks like a mock / null class, it comes useful
    /// when we don't want a LinkFromRequest instance to (re)discover a RSS feed.
    /// </summary>
    internal sealed class DoesNotFindFeed: IFeedDiscovery
    {
#pragma warning disable 1998
        public async Task<(string Title, string Url)> DiscoveredFeed(string url)
#pragma warning restore 1998
        {
            return (string.Empty, string.Empty);
        }
    }
}
