namespace Blogroll.Common.Media
{
    /// <summary>
    /// A contract for objects that can print themselves.
    /// </summary>
    public interface IPrintable
    {
        /// <summary>
        /// Returns an IMedia instance into which the object has printed itself.
        /// </summary>
        IMedia PrePrinted(IMedia media);

        /// <summary>
        /// Returns the rendered string of an IMedia instance into which the object has printed itself.
        /// </summary>
        /// <param name="media"></param>
        /// <returns></returns>
        string PrintedTo(IMedia media);
    }
}
