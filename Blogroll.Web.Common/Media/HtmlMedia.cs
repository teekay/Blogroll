using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Blogroll.Common.Media;
using HandlebarsDotNet;
using Optional;

namespace Blogroll.Web.Common.Media
{
    /// <summary>
    /// Prints to HTML using Handlebars.
    /// </summary>
    public sealed class HtmlMedia: IMedia
    {
        public HtmlMedia() : this("")
        {
        }

        public HtmlMedia(string template) : this(template, Option.None<IEnumerable<string>>())
        {
        }

        public HtmlMedia(string template, Option<IEnumerable<string>> acceptedKeys) : this(template, new Dictionary<string, string>(), acceptedKeys)
        {
        }

        public HtmlMedia(string template, IDictionary<string, string> terms, Option<IEnumerable<string>> acceptedKeys)
        {
            _template = template;
            _terms = terms;
            _acceptedKeys = acceptedKeys.ValueOr(new List<string>()).ToList();
            _isConstrained = acceptedKeys.HasValue;
        }

        public HtmlMedia(string template, IDictionary<string, string> terms, IEnumerable<string> acceptedKeys)
        {
            _template = template;
            _terms = terms;
            _acceptedKeys = acceptedKeys?.ToList() ?? throw new ArgumentNullException(nameof(acceptedKeys));
            _isConstrained = true;
        }

        private readonly string _template;
        private readonly IDictionary<string, string> _terms;
        private readonly bool _isConstrained;
        private readonly List<string> _acceptedKeys;

        private string ValueOrEmpty(string key, string value) => !_isConstrained || _acceptedKeys.Contains(key)
            ? value
            : string.Empty;

        private IMedia Copied() => _isConstrained
            ? new HtmlMedia(_template, _terms, _acceptedKeys)
            : new HtmlMedia(_template, _terms, Option.None<IEnumerable<string>>());

        public IMedia With(string key, string value)
        {
            _terms[key] = ValueOrEmpty(key, value);
            return Copied();
        }

        public IMedia With(string key, double value)
        {
            _terms[key] = ValueOrEmpty(key, value.ToString(CultureInfo.InvariantCulture));
            return Copied();
        }

        public IMedia With(string key, object value)
        {
            _terms[key] = ValueOrEmpty(key, value?.ToString() ?? string.Empty);
            return Copied();
        }

        public byte[] Bytes() => new UTF8Encoding(false).GetBytes(ToString());

        public override string ToString() => Handlebars.Compile(_template).Invoke(_terms);
    }
}
