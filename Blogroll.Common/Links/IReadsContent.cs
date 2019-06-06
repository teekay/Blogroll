using System.Collections.Generic;
using System.Threading.Tasks;

namespace Blogroll.Common.Links
{
    /// <summary>
    /// A contract for fetching content from RSS feeds
    /// </summary>
    public interface IReadsContent
    {
        Task<ICollection<Snippet>> Content(string url);
    }
}
