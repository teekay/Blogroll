using Blogroll.Common.Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Blogroll.Common.Links
{
    /// <summary>
    /// Blogroll encapsulates zero or more links. The client can  request to
    /// add / remove links and change their positions.
    /// </summary>
    public sealed class BlogRoll : IPersistableBlogroll
    {
        public BlogRoll(): this(new List<BlogRollItem>())
        {
        }

        public BlogRoll(IEnumerable<BlogRollItem> links)
        {
            Links = links.ToList();
        }

        public BlogRoll(IEnumerable<ILink> links)
        {
            var list = links.ToList();
            Links = Enumerable.Range(0, list.Count)
                .Select(pos => new BlogRollItem(list[pos], pos))
                .ToList();
        }

        private List<BlogRollItem> Links { get; }

        private readonly List<BlogRollItem> _deletedLinks = new List<BlogRollItem>();

        /// <summary>
        /// Add or update a link
        /// </summary>
        public void Add(ILink link)
        {
            if (Updated(link)) return;
            Links.Add(new BlogRollItem(link, Links.Count + 1));
        }

        private bool Updated(ILink link)
        {
            var existing = Links.FirstOrDefault(x => x.Equals(link));
            if (existing == null) return false;
            Update(existing, link);
            return true;
        }

        private void Update(BlogRollItem existing, ILink link)
        {
            var updated = new BlogRollItem(link, existing.Position);
            Links.Remove(existing);
            Links.Add(updated);
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
            Reindex();
        }

        /// <summary>
        /// Move a link identified by a position to a new position. Index starts at 1.
        /// </summary>
        /// <param name="oldPosition">Existing position</param>
        /// <param name="newPosition">New position</param>
        public void Move(int oldPosition, int newPosition) => new Repositioned(Links, oldPosition, newPosition).Move();

        /// <summary>
        /// Returns an index of a link. Index starts at 1.
        /// </summary>
        public int PositionOf(ILink link)
        {
            Reindex();
            return Links.FirstOrDefault(bri => bri.Equals(link))?.Position ?? 0;
        }

        /// <summary>
        /// Returns a link at a position or an empty link (instead of null).
        /// </summary>
        public ILink Find(int position) => Links.FirstOrDefault(link => link.Position == position) ?? Link.Empty();

        private void Reindex()
        {
            var ordered = Links.OrderBy(x => x.Position).ToList();
            Enumerable.Range(0, ordered.Count)
                .ToList()
                .ForEach(index => ordered[index].Move(index + 1));
        }

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
            Reindex();
            return printer.MasterTemplate()
                .With("Links", string.Join(printer.Separator(), 
                    await Task.WhenAll(Links.OrderBy(link => link.Position)
                        .Select(async item => await EachLinkPrinted(item)))))
                .ToString();
        }

        /// <summary>
        /// Persists its contents into the provided data source.
        /// </summary>
        /// <param name="dataSource"></param>
        public void PersistTo(IPersisting dataSource)
        {
            void PersistItem(BlogRollItem item) => 
                dataSource.MaybeFound(item).Match(some => dataSource.Update(some, item), () => dataSource.Insert(item));
            Reindex();
            _deletedLinks.ForEach(dataSource.Delete);
            Links.OrderBy(link => link.Position).ToList().ForEach(PersistItem);
        }

        public override string ToString() => $"Blogroll with {Links.Count} links";

        private class Repositioned
        {
            public Repositioned(IEnumerable<BlogRollItem> source, int oldPosition, int newPosition)
            {
                _source = source.ToList();
                _oldPosition = oldPosition;
                _newPosition = newPosition;
            }

            private readonly List<BlogRollItem> _source;
            private readonly int _oldPosition;
            private readonly int _newPosition;

            private BlogRollItem Find(int position) => _source.FirstOrDefault(link => link.Position == position) ?? BlogRollItem.Empty();

            public void Move()
            {
                var maybe = Find(_oldPosition);
                if (maybe.AmEmpty() || _oldPosition == _newPosition) return;
                var movingDown = _oldPosition < _newPosition;
                Func<BlogRollItem, bool> findAffectedNeighbor = movingDown ? WhenMovingDown : (Func<BlogRollItem, bool>)WhenMovingUp;
                Action<BlogRollItem> pushNeighbors = movingDown ? PushUp : (Action<BlogRollItem>)PushDown;
                _source.Where(findAffectedNeighbor).ToList().ForEach(pushNeighbors);
                maybe.Move(_newPosition);
            }

            private bool WhenMovingDown(BlogRollItem x) => x.Position <= _newPosition;
            private bool WhenMovingUp(BlogRollItem x) => x.Position >= _newPosition;
            private void PushUp(BlogRollItem x) => x.Move(x.Position - 1);
            private void PushDown(BlogRollItem x) => x.Move(x.Position + 1);
        }
    }
}