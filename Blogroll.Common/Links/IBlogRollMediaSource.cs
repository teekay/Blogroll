using Blogroll.Common.Media;

namespace Blogroll.Common.Links
{
    /// <summary>
    /// A contract that specifies methods useful for printing an IBlogRoll instance.
    /// </summary>
    public interface IBlogRollMediaSource
    {
        /// <summary>
        /// Returns the outermost template.
        /// </summary>
        IMedia MasterTemplate();

        /// <summary>
        /// Returns the template hosting the link and, presumably, the latest content from the link.
        /// </summary>
        IMedia ContainingTemplate();

        /// <summary>
        /// Returns the template that the link can print itself to.
        /// </summary>
        IMedia LinkTemplate();

        /// <summary>
        /// Returns the template for the latest content from the link.
        /// </summary>
        IMedia SnippetTemplate();

        /// <summary>
        /// Returns a separator, if any, to use when printing multiple links.
        /// </summary>
        /// <returns></returns>
        string Separator();
    }
}