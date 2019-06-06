using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Blogroll.Common.Persistence;

namespace Blogroll.Common.Links
{
    /// <summary>
    /// Alternative implementation of IPersistableBlogroll using a LinkedList&lt;T&gt;
    /// as its underlying collection.
    /// </summary>
    public sealed class BlogRollLinked: IPersistableBlogroll
    {
        public BlogRollLinked(): this(new LinkedList<ILink>())
        {
        }

        public BlogRollLinked(IEnumerable<ILink> links): this(new LinkedList<ILink>(links))
        {
        }

        public BlogRollLinked(LinkedList<ILink> links)
        {
            Links = links;
        }

        private LinkedList<ILink> Links { get; }

        private List<BlogRollItem> Wrapped()
        {
            var ordered = Links.ToList();
            return Enumerable.Range(0, ordered.Count).Select(pos => new BlogRollItem(ordered[pos], pos + 1)).ToList();
        }

        public void Add(ILink link) => Links.AddLast(link);
        public void Remove(ILink link) => Links.Remove(link);

        public void Move(int oldPosition, int newPosition)
        {
            var maybe = Find(oldPosition);
            var target = Find(newPosition);
            var realTarget = Links.Find(target);
            if (maybe == null || target == null || realTarget == null) return;
            Links.Remove(maybe);
            Links.AddAfter(realTarget, maybe);
        }

        public int PositionOf(ILink link)
        {
            var target = Links.Find(link);
            if (target == null) return 0;
            var pos = 1; // this ugliness illustrates that LinkedList<T> is not the appropriate data type for this
            do
            {
                target = target.Previous;
                if (target != null) pos += 1;
            } while (target != null);
            return pos;
        }

        public ILink Find(int position) => Links.ElementAt(position - 1) ?? Link.Empty();

        public async Task<string> PrintedTo(IBlogRollMediaSource printer)
        {
            async Task<string> EachLinkPrinted(ILink link)
            {
                return printer.ContainingTemplate()
                    .With("Link", link.PrintedTo(printer.LinkTemplate()))
                    .With("Snippet", (await link.LatestContent()).PrintedTo(printer.SnippetTemplate()))
                    .ToString();
            }

            var wrapped = Wrapped();
            return printer.MasterTemplate()
                .With("Links", string.Join(printer.Separator(),
                    await Task.WhenAll(wrapped.Select(async item => await EachLinkPrinted(item)))))
                .ToString();
        }

        public void PersistTo(IPersisting dataSource)
        {
            void PersistItem(BlogRollItem item) => 
                dataSource.MaybeFound(item).Match(some => dataSource.Update(some, item), () => dataSource.Insert(item));
            Wrapped().ForEach(PersistItem);
        }
    }
}