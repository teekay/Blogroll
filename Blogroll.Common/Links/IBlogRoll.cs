using System.Threading.Tasks;

namespace Blogroll.Common.Links
{
    /// <summary>
    /// A contract for a blogroll that specifies a few collection-like methods
    /// without exposing the underlying collection.
    /// </summary>
    public interface IBlogRoll
    {
        /// <summary>
        /// Adds a link.
        /// </summary>
        void Add(ILink link);

        /// <summary>
        /// Removes a link.
        /// </summary>
        void Remove(ILink link);

        /// <summary>
        /// Moves a link from one position to another.
        /// </summary>
        void Move(int oldPosition, int newPosition);

        /// <summary>
        /// Returns an index of a link.
        /// </summary>
        int PositionOf(ILink link);

        /// <summary>
        /// Returns a link by its position.
        /// </summary>
        ILink Find(int position);

        /// <summary>
        /// Prints itself to the provided printer.
        /// The assumption is that the implementing method will be async.
        /// </summary>
        Task<string> PrintedTo(IBlogRollMediaSource printer);
    }
}