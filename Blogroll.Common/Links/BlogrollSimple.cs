using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Blogroll.Common.Media;
using Blogroll.Common.Persistence;

namespace Blogroll.Common.Links
{
    /// <summary>
    /// Blogroll encapsulates zero or more links. The client can  request to
    /// add / remove links and change their positions.
    /// This implementation discovers that we can get rid of BlogRollItem and simply add links at specified indices.
    /// </summary>
    public sealed class BlogrollSimple : IPersistableBlogroll
    {
        public BlogrollSimple() : this(new List<ILink>())
        {
        }

        public BlogrollSimple(IEnumerable<ILink> links)
        {
            Links = links.ToList();
        }

        private List<ILink> Links { get; }

        private readonly List<ILink> _deletedLinks = new List<ILink>();

        /// <summary>
        /// Add or update a link
        /// </summary>
        public void Add(ILink link)
        {
            if (Updated(link)) return;
            Links.Add(link);
        }

        private bool Updated(ILink link)
        {
            var existing = Links.FirstOrDefault(x => x.Equals(link));
            if (existing == null) return false;
            Update(existing, link);
            return true;
        }

        private void Update(ILink existing, ILink updated)
        {
            var position = Links.IndexOf(existing);
            Links.RemoveAt(position);
            Links.Insert(position, updated);
        }

        /// <summary>
        /// Remove a link
        /// </summary>
        public void Remove(ILink link)
        {
            var toRemove = Links.FirstOrDefault(x => x.Equals(link));
            if (toRemove == null) return;
            Links.Remove(toRemove);
            _deletedLinks.Add(toRemove);
        }

        /// <summary>
        /// Move a link identified by a position to a new position. Index starts at 1.
        /// </summary>
        /// <param name="oldPosition">Existing position</param>
        /// <param name="newPosition">New position</param>
        public void Move(int oldPosition, int newPosition)
        {
            var targetLink = Links.ElementAt(SafeIndexQuery(oldPosition));
            if (targetLink == null) return;
            Links.RemoveAt(oldPosition - 1);
            Links.Insert(SafeIndexInsert(newPosition), targetLink);
        }

        private int SafeIndexQuery(int pos) => Math.Min(Links.Count - 1, Math.Max(0, pos - 1));
        private int SafeIndexInsert(int pos) => Math.Min(Links.Count, Math.Max(0, pos - 1));

        /// <summary>
        /// Returns an index of a link. Index starts at 1.
        /// </summary>
        public int PositionOf(ILink link) => Links.IndexOf(link) + 1;

        /// <summary>
        /// Returns a link at a position or an empty link (instead of null).
        /// </summary>
        public ILink Find(int position) => position < 1 || position > Links.Count 
            ? Link.Empty() 
            : Links[position - 1];

        /// <summary>
        /// Prints itself into media provided by the printer.
        /// </summary>
        public async Task<string> PrintedTo(IBlogRollMediaSource printer)
        {
            async Task<string> EachLinkPrinted(ILink link)
            {
                return printer.ContainingTemplate()
                    .With("Link", link.PrintedTo(printer.LinkTemplate()))
                    .With("Snippet", (await link.LatestContent()).PrintedTo(printer.SnippetTemplate()))
                    .ToString();
            }

            var items = Enumerable.Range(0, Links.Count).Select(x => new LinkWithPosition(Links[x], x + 1));
            return printer.MasterTemplate()
                .With("Links", string.Join(printer.Separator(),
                    await Task.WhenAll(items.Select(async item => await EachLinkPrinted(item)))))
                .ToString();
        }

        /// <summary>
        /// Persists its contents into the provided data source.
        /// </summary>
        /// <param name="dataSource"></param>
        public void PersistTo(IPersisting dataSource)
        {
            void PersistItem(ILink item) =>
                dataSource.MaybeFound(item).Match(some => dataSource.Update(some, item), () => dataSource.Insert(item));
            _deletedLinks.ForEach(dataSource.Delete);
            Links.ForEach(PersistItem);
        }

        public override string ToString() => $"Blogroll with {Links.Count} links";

        private class LinkWithPosition : ILink
        {
            public LinkWithPosition(ILink link, int position)
            {
                _link = link;
                _position = position;
            }

            private readonly ILink _link;
            private readonly int _position;

            public string Id() => _link.Id();

            public IMedia PrePrinted(IMedia media) => _link.PrePrinted(media).With("Position", _position);

            public string PrintedTo(IMedia media) => PrePrinted(media).ToString();

            public bool AmEmpty() => _link.AmEmpty();

            public bool CanRead() => _link.CanRead();

            public async Task<Snippet> LatestContent() => await _link.LatestContent();

            public void Save(Action<string, string, string> saveImpl) => _link.Save(saveImpl);
        }
    }
}
