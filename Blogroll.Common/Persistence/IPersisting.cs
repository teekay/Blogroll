using Blogroll.Common.Links;
using Optional;

namespace Blogroll.Common.Persistence
{
    /// <summary>
    /// Contract to fulfill by a concrete data storage implementation
    /// to enable persisting links.
    /// </summary>
    public interface IPersisting
    {
        /// <summary>
        /// Returns a link if found in the data store.
        /// </summary>
        /// <param name="link">ILink instance</param>
        /// <returns>An object from the data storage, to be used in the Update operation by the implementing party.</returns>
        Option<object> MaybeFound(ILink link);

        /// <summary>
        /// A method implementing an UPDATE operation in the data store.
        /// </summary>
        /// <param name="existing">An existing object representing the stored link, as returned by the MaybeFound method.</param>
        /// <param name="updatedLink">ILink instance that may contain updated information about the link.</param>
        void Update(object existing, ILink updatedLink);

        /// <summary>
        /// A method implementing an INSERT operation in the data store.
        /// </summary>
        /// <param name="link">An ILink instance to be persisted.</param>
        void Insert(ILink link);

        /// <summary>
        /// A method implementing a DELETE operation in the data store.
        /// </summary>
        /// <param name="link"></param>
        void Delete(ILink link);
    }
}