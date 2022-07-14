using System.Collections.Generic;
using System.Linq;

namespace Blogroll.Web.Common.Common
{
    /// <summary>
    /// Helps to decided the content type requested by the (Http) client.
    /// </summary>
    public sealed class ResolvedContentType
    {
        public ResolvedContentType(Dictionary<string, string> extToContentType, 
            string defaultContentTypeOut,
            string explicitContentType,
            string contentTypeFromPath,
            IList<string> acceptHeaders)
        {
            _extToContentType = extToContentType;
            _defaultContentTypeOut = defaultContentTypeOut;
            _explicitContentType = explicitContentType;
            _contentTypeFromPath = contentTypeFromPath;
            _acceptHeaders = acceptHeaders;
        }

        private readonly Dictionary<string, string> _extToContentType;
        private readonly string _defaultContentTypeOut;
        private readonly string _explicitContentType;
        private readonly string _contentTypeFromPath;
        private readonly IList<string> _acceptHeaders;
        private string _contentTypeIn;

        /// <summary>
        /// Provides the content type as requested by the (Http)  client.
        /// </summary>
        /// <returns></returns>
        public string ContentType() => _contentTypeIn ??= ContentTypeOrDefault(
            ContentTypeAcceptableByClient(), _defaultContentTypeOut);

        private string ContentTypeOrDefault(string contentType, string defaultValue) =>
            !string.IsNullOrEmpty(contentType)
                ? contentType
                : defaultValue;

        private string ContentTypeAcceptableByClient()
        {
            if (!string.IsNullOrEmpty(_explicitContentType)) return _explicitContentType;
            _extToContentType.TryGetValue(_contentTypeFromPath,
                out var maybeRequestedInPath);
            if (!string.IsNullOrEmpty(maybeRequestedInPath)) return maybeRequestedInPath;
            var acceptable = _acceptHeaders.ToList();
            var contentTypes = acceptable.SelectMany(header =>
                    header.Split(';')
                        .Where(separated => separated.Contains("/") && !separated.Contains("=")))
                .SelectMany(s => s.Split(','))
                .Select(s => s.ToLower())
                .ToList();
            return contentTypes.FirstOrDefault(ct => !string.IsNullOrEmpty(ContentTypeParsed(ct)));
        }

        private string ContentTypeParsed(string ct) => ct.Contains("json")
            ? "application/json"
            : ct.Contains("html")
                ? "text/html"
                : string.Empty;

        public override string ToString() => ContentType();
    }
}