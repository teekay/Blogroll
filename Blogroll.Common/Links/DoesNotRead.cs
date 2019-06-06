using System.Collections.Generic;
using System.Threading.Tasks;

namespace Blogroll.Common.Links
{
    /// <summary>
    /// A dummy implementation of IReadsContent that cannot read.
    /// </summary>
    public sealed class DoesNotRead : IReadsContent
    {
#pragma warning disable 1998
        public async Task<ICollection<Snippet>> Content(string url)
#pragma warning restore 1998
        {
            return new List<Snippet>();
        }
    }
}
