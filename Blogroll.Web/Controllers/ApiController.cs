using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Blogroll.Common.Persistence;
using Blogroll.Web.Common;
using Blogroll.Web.Common.BlogRollPrinters;
using Blogroll.Web.Common.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Blogroll.Web.Controllers
{
    [Route("api/v1")]
    public class ApiController: ControllerBase
    {
        public ApiController(ResolvedContentRoot contentRoot, IPersistedBlogroll blogroll)
        {
            _contentRoot = contentRoot;
            _blogroll = blogroll;
        }

        private readonly ResolvedContentRoot _contentRoot;
        private readonly IPersistedBlogroll _blogroll;
        private const string DefaultContentTypeOut = "text/html";
        private ResolvedContentType ContentType() => new ResolvedContentType(_extToContentType,
            DefaultContentTypeOut,
            HttpContext?.Request.ContentType ?? string.Empty,
            HttpContext?.Request.Path.ToString().Split('.').Last() ?? string.Empty,
            HttpContext?.Request.Headers["Accept"].ToList() ?? new List<string>());

        private readonly Dictionary<string, string> _extToContentType = new Dictionary<string, string>
        {
            {"txt", "text/plain"},
            {"html", "text/html"},
            {"json", "application/json"}
        };

        [AllowAnonymous]
        [HttpGet]
        [Route(@"links")]
        [Route(@"links.json")]
        [Route(@"links.txt")]
        [Route(@"links.html")]
        public async Task<ActionResult<string>> Links() => await LinksOut(true);

        [AllowAnonymous]
        [HttpGet]
        [Route("links/simple")]
        [Route("links/simple.json")]
        [Route("links/simple.txt")]
        [Route("links/simple.html")]
        public async Task<ActionResult<string>> SimpleLinks() => await LinksOut(false);

        [Authorize]
        [HttpPost]
        [Route("link/move/{fromPosition}/{toPosition}")]
        public IActionResult Move(int fromPosition, int toPosition)
        {
            _blogroll.Move(fromPosition, toPosition);
            _blogroll.Save();
            return Accepted();
        }

        private async Task<ActionResult> LinksOut(bool withSnippet)
        {
            var ct = ContentType();
            return Content(
                await PrintedLinks(ct, withSnippet),
                ct.ContentType(), Encoding.UTF8);
        }

        private async Task<string> PrintedLinks(ResolvedContentType ct, bool withSnippet) =>
            string.IsNullOrEmpty(ct.ContentType()) || ct.ContentType().Contains("json")
                ? await _blogroll.PrintedTo(new PrintsToJson(withSnippet))
                : ct.ContentType().Contains("html")
                    ? await _blogroll.PrintedTo(
                        new PrintsToHtml(System.IO.File.ReadAllText($"{_contentRoot}/Views/Public/_blogroll.hbs"),
                        System.IO.File.ReadAllText($"{_contentRoot}/Views/Public/_link_container.hbs"),
                        System.IO.File.ReadAllText($"{_contentRoot}/Views/Public/_link.hbs"),
                        System.IO.File.ReadAllText($"{_contentRoot}/Views/Public/_snippet.hbs"),
                        "\n", withSnippet))
                    : await _blogroll.PrintedTo(
                        new PrintsToText(
                        "{{Links}}", "{{Link}}", 
                        System.IO.File.ReadAllText($"{_contentRoot}/Views/Public/_link.txt"), 
                        string.Empty, "\n"));
    }
}