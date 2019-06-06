using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Web;
using Blogroll.Common.Media;
using Optional;

namespace Blogroll.Web.Common.Media
{
    /// <summary>
    /// Can print an object into a JSON-encoded string.
    /// For now, it recognizes string, double, and object types.
    /// </summary>
    public sealed class JsonMedia: IMedia
    {
        public JsonMedia(): this(new JsonTerms())
        {
        }

        public JsonMedia(JsonTerms jsonTerms): this(jsonTerms, Option.None<IEnumerable<string>>())
        {
        }

        public JsonMedia(JsonTerms jsonTerms, Option<IEnumerable<string>> attribs)
        {
            _jsonTerms = jsonTerms;
            _attribs = attribs;
        }

        private readonly JsonTerms _jsonTerms;
        private Option<IEnumerable<string>> _attribs;
        private const string Q = @"""";
        private const string Lb = "{";
        private const string Rb = "}";

        private string Escaped(string value) => HttpUtility.JavaScriptStringEncode(value);

        public IMedia With(string key, string value) => new JsonMedia(_jsonTerms.With(key, value), _attribs);

        public IMedia With(string key, double value) => new JsonMedia(_jsonTerms.With(key, value), _attribs);

        public IMedia With(string key, object value) => new JsonMedia(_jsonTerms.With(key, value), _attribs);

        public byte[] Bytes() => new UTF8Encoding(false).GetBytes(ToString());

        public override string ToString()
        {
            var isConstrained = _attribs.HasValue;
            var constrainedAttributes = _attribs.ValueOr(new List<string>()).ToList();
            bool Matches(string t) => !isConstrained || constrainedAttributes.Contains(t);
            string ConvertedStrings(KeyValuePair<string, string> kvp) => $@"{Q}{kvp.Key}{Q}:{Q}{Escaped(kvp.Value)}{Q}";
            string ConvertedNumbers(KeyValuePair<string, double> kvp) => $@"{Q}{kvp.Key}{Q}:{kvp.Value.ToString(CultureInfo.InvariantCulture)}";
            string ConvertedObjects(KeyValuePair<string, object> kvp) => kvp.Value is JsonString jsonEncodedAlready 
                ? $@"{Q}{kvp.Key}{Q}:{jsonEncodedAlready}"
                : $@"{Q}{kvp.Key}{Q}:{Q}{kvp.Value}{Q}";
            var converted = _jsonTerms.Strings().Where(kvp => Matches(kvp.Key)).Select(ConvertedStrings)
                .Concat(_jsonTerms.Numbers().Where(kvp => Matches(kvp.Key)).Select(ConvertedNumbers))
                .Concat(_jsonTerms.Objects().Where(kvp => Matches(kvp.Key)).Select(ConvertedObjects));
            return $@"{Lb}{string.Join(",", converted)}{Rb}";
        }
    }
}
