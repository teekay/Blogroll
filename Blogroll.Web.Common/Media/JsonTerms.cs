using System.Collections.Generic;

namespace Blogroll.Web.Common.Media
{
    /// <summary>
    /// Encapsulated a dictionary of keys, presumably corresponding to object properties, and their values.
    /// Coule be merged into JsonMedia itself as it seemes to somehow do the same thing except the printing.
    /// </summary>
    public sealed class JsonTerms
    {
        public JsonTerms(): this(new Dictionary<string, string>(),
                                 new Dictionary<string, double>(),
                                 new Dictionary<string, object>())
        {
        }

        public JsonTerms(IDictionary<string, string> stringTerms,
            IDictionary<string, double> numericTerms,
            IDictionary<string, object> genericTerms) // hmm, generic...
        {
            _stringTerms = stringTerms;
            _numericTerms = numericTerms;
            _genericTerms = genericTerms;
        }

        private readonly IDictionary<string, string> _stringTerms;
        private readonly IDictionary<string, double> _numericTerms;
        private readonly IDictionary<string, object> _genericTerms;

        public JsonTerms With(string key, string value)
        {
            _stringTerms[key] = value;
            return new JsonTerms(_stringTerms, _numericTerms, _genericTerms);
        }

        public JsonTerms With(string key, double value)
        {
            _numericTerms[key] = value;
            return new JsonTerms(_stringTerms, _numericTerms, _genericTerms);
        }

        public JsonTerms With(string key, object value)
        {
            _genericTerms[key] = value;
            return new JsonTerms(_stringTerms, _numericTerms, _genericTerms);
        }

        public IDictionary<string, string> Strings() => _stringTerms;
        public IDictionary<string, double> Numbers() => _numericTerms;
        public IDictionary<string, object> Objects() => _genericTerms;
    }
}