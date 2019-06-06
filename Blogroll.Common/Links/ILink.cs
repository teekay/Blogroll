using System;
using System.Threading.Tasks;
using Blogroll.Common.Media;

namespace Blogroll.Common.Links
{
    /// <summary>
    /// Contract for links that can be read and saved.
    /// </summary>
    public interface ILink: IIdentifiable, IPrintable, INullQueryable
    {
        /// <summary>
        /// Inform whether the instance can retrieve content from the link's RSS feed.
        /// </summary>
        bool CanRead();

        /// <summary>
        /// May return the latest content from the link's RSS feed.
        /// The assumption is that the implementing method will be async.
        /// </summary>
        Task<Snippet> LatestContent();

        /// <summary>
        /// Will provide its properties to the provided action that presumably performs a persistence operation.
        /// </summary>
        /// <param name="saveImpl"></param>
        void Save(Action<string, string, string> saveImpl);
    }
}