using System.Threading.Tasks;

namespace Blogroll.Common.Links
{
    /// <summary>
    /// A contract for RSS feed discovery.
    /// </summary>
    public interface IFeedDiscovery
    {
        Task<(string Title, string Url)> DiscoveredFeed(string url);
    }
}