using System.Collections.Generic;
using Blogroll.Common.Media;
using Optional;

namespace Blogroll.Web.Common.Media
{
    /// <summary>
    /// Acts as a proxy for JsonMedia, assuming that the values passed to With(string key, string value)
    /// are already JSON-encoded strings, and passing them to JsonMedia as objects of the JsonString class.
    /// Therefore, generated JSON is treated as such and not stringified any further.
    /// </summary>
    public sealed class JsonTypedMedia: IMedia
    {
        public JsonTypedMedia() : this(new JsonTerms())
        {
        }

        public JsonTypedMedia(JsonTerms jsonTerms) : this(jsonTerms, Option.None<IEnumerable<string>>())
        {
        }

        public JsonTypedMedia(JsonTerms jsonTerms, Option<IEnumerable<string>> attribs)
        {
            _jsonMedia = new JsonMedia(jsonTerms, attribs);
        }

        private JsonMedia _jsonMedia;

        public IMedia With(string key, string value)
        {
            _jsonMedia = (JsonMedia) _jsonMedia.With(key, new JsonString(value));
            return this;
        }

        public IMedia With(string key, double value)
        {
            _jsonMedia = (JsonMedia) _jsonMedia.With(key, value);
            return this;
        }

        public IMedia With(string key, object value)
        {
            _jsonMedia = (JsonMedia)_jsonMedia.With(key, value);
            return this;
        }

        public byte[] Bytes() => _jsonMedia.Bytes();

        public override string ToString() => _jsonMedia.ToString();
    }
}
