using Blogroll.Common.Links;

namespace Blogroll.Common.Persistence
{
    /// <summary>
    /// A contract that enriches an IBlogRoll instance to be able to persist itself.
    /// To be used on IBlogRoll implementations that do not rely on a specific data backend.
    /// </summary>
    public interface IPersistableBlogroll: IBlogRoll
    {
        /// <summary>
        /// Persist the contents of a blogroll into a persistence layer
        /// exposed by a data source.
        /// </summary>
        void PersistTo(IPersisting dataSource);
    }
}