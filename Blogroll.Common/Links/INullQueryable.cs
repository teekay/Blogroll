namespace Blogroll.Common.Links
{
    /// <summary>
    /// Contract to let an object declare whether it's essentially empty (this close to null)
    /// or not.
    /// </summary>
    public interface INullQueryable
    {
        bool AmEmpty();
    }
}
