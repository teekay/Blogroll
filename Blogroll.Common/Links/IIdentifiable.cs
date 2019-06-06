namespace Blogroll.Common.Links
{
    /// <summary>
    /// Contract for entities that have a unique (string) identity.
    /// </summary>
    public interface IIdentifiable
    {
        string Id();
    }
}