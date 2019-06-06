using Blogroll.Common.Links;

namespace Blogroll.Common.Persistence
{
    /// <summary>
    /// A contract that enriches an IBlogRoll instance to be able to persist itself.
    /// To be used on IBlogRoll implementations that encapsulate a specific data backend.
    /// </summary>
    public interface IPersistedBlogroll: IBlogRoll
    {
        void Save();
    }
}