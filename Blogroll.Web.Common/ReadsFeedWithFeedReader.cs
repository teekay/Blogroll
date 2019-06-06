using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Blogroll.Common.Links;
using CodeHollow.FeedReader;

namespace Blogroll.Web.Common
{
    public sealed class ReadsFeedWithFeedReader: IReadsContent
    {
        public async Task<ICollection<Snippet>> Content(string url)
        {
            var feed = await FeedReader.ReadAsync(url);
            return feed.Items.ToList().Select(item => new Snippet(item.Link, item.Title, item.Description)).ToList();
        }
    }
}