using Blogroll.Common.Commons;

namespace Blogroll.Web.Common.Media
{
    /// <summary>
    /// Wraps a string and nothing more. Only used within JsonMedia and JsonTypedMedia.
    /// </summary>
    internal sealed class JsonString
    {
        public JsonString(string value)
        {
            _value = new SolidString(value);
        }

        private readonly string _value;

        public override string ToString() => _value;
    }
}