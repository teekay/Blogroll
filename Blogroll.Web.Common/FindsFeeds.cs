using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Blogroll.Common.Links;
using CodeHollow.FeedReader;

namespace Blogroll.Web.Common
{
    /// <summary>
    /// Performs a discovery of a RSS feed.
    /// </summary>
    public sealed class FindsFeeds : IFeedDiscovery
    {
        private readonly (string, string) _nothing = (string.Empty, string.Empty);

        public async Task<(string Title, string Url)> DiscoveredFeed(string url)
        {
            try
            {
                var feed = (await FeedReader.GetFeedUrlsFromUrlAsync(url)).FirstOrDefault();
                return feed == null
                    ? _nothing
                    : (feed.Title, feed.Url);
            }
            catch (Exception e)
            {
                Trace.WriteLine($"Could not discover feed: {e.Message}");
                return _nothing;
            }
        }
    }
}