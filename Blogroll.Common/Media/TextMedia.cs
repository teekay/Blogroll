using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Blogroll.Common.Media
{
    /// <summary>
    /// Simple media printing to plain text and using a simple Handlebars-like notation
    /// for template keys like this: {{Key}}
    /// </summary>
    public sealed class TextMedia: IMedia
    {
        public TextMedia(): this("")
        {
        }

        public TextMedia(string template) : this(template, new Dictionary<string, string>())
        {
        }

        public TextMedia(string template, IDictionary<string, string> terms)
        {
            _template = template;
            _terms = terms;
        }

        private readonly IDictionary<string, string> _terms;
        private readonly string _template;

        public IMedia With(string key, string value)
        {
            _terms[key] = value;
            return new TextMedia(_template, _terms);
        }

        public IMedia With(string key, double value)
        {
            _terms[key] = value.ToString(CultureInfo.InvariantCulture);
            return new TextMedia(_template, _terms);
        }

        public IMedia With(string key, object value)
        {
            _terms[key] = value?.ToString() ?? string.Empty;
            return new TextMedia(_template, _terms);
        }

        public byte[] Bytes() => new UTF8Encoding(false).GetBytes(ToString());

        private string TemplateKey(string key) => new StringBuilder("{{").Append(key).Append("}}").ToString();

        public override string ToString() => 
            _terms.Aggregate(_template, (current, kvp) => current.Replace(TemplateKey(kvp.Key), kvp.Value));
    }
}