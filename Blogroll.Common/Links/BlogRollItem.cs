using System;
using System.Threading.Tasks;
using Blogroll.Common.Media;

namespace Blogroll.Common.Links
{
    /// <summary>
    /// Encapsulates an ILink, and adds a Position to it.
    /// Key recognized for printing is: Position.
    /// </summary>
    public sealed class BlogRollItem: ILink, IEquatable<ILink>, IComparable<BlogRollItem>
    {
        public BlogRollItem(ILink link, int position)
        {
            _link = link;
            Position = position;
        }

        public static BlogRollItem Empty() => new BlogRollItem(Link.Empty(), 0);

        private readonly ILink _link;

        internal int Position { get; private set; }

        public bool CanRead() => _link.CanRead();

        public async Task<Snippet> LatestContent() => await _link.LatestContent();

        public void Move(int pos) => Position = pos;

        public IMedia PrePrinted(IMedia media) => _link.PrePrinted(media).With(nameof(Position), Position);

        public string PrintedTo(IMedia media) => PrePrinted(media).ToString();

        public bool AmEmpty() => _link.AmEmpty();

        public string Id() => _link.Id();

        public bool Equals(ILink other) => other != null && string.Equals(Id(), other.Id());

        public override bool Equals(object obj) =>
            obj != null && (
                ReferenceEquals(this, obj) ||
                obj is ILink other && Equals(other));

        public override int GetHashCode() => Id().GetHashCode();

        public int CompareTo(BlogRollItem other) =>
            ReferenceEquals(this, other) 
                ? 0
                : ReferenceEquals(null, other) 
                    ? 1 
                    : Position.CompareTo(other.Position);

        public void Save(Action<string, string, string> saveImpl) => _link.Save(saveImpl);

        public void Save(Action<int> saveImpl) => saveImpl(Position);

        public override string ToString() => $"{Position}: {_link}";
    }
}