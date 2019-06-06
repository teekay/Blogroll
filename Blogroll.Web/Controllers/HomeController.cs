using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Blogroll.Common.Links;
using Blogroll.Common.Persistence;
using Blogroll.Web.Common;
using Blogroll.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Blogroll.Web.Controllers
{
    [Authorize]
    public sealed class HomeController : Controller
    {
        public HomeController(IPersistedBlogroll blogroll, ResolvedContentRoot contentRoot)
        {
            _blogRoll = blogroll;
            _contentRoot = contentRoot;
        }

        private readonly IPersistedBlogroll _blogRoll;
        private readonly ResolvedContentRoot _contentRoot;

        [HttpGet]
        public IActionResult Index()
        {
            return View(new ViewModel(_contentRoot, _blogRoll, Link.Empty()));
        }

        [HttpPost]
        public async Task<IActionResult> Add()
        {
            await AddOrUpdateLink(new FindsFeeds());
            return Redirect("/");
        }

        private async Task AddOrUpdateLink(IFeedDiscovery discovery)
        {
            _blogRoll.Add(
                await new LinkFromRequest(
                        HttpContext.Request.Form
                            .Select(kvp => new KeyValuePair<string, string>(kvp.Key, kvp.Value.ToString())),
                        discovery, new ReadsFeedWithFeedReader())
                    .Link());
            _blogRoll.Save();
        }

        [HttpGet]
        public IActionResult Edit(int? id)
        {
            return id == null
                ? NotFound()
                : MaybeEdit((int) id);
        }

        private IActionResult MaybeEdit(int id)
        {
            ILink maybe = _blogRoll.Find(id);
            return maybe.AmEmpty()
                ? (IActionResult) NotFound()
                : View(new ViewModel(_contentRoot, _blogRoll, maybe));
        }

        [HttpPost]
        public async Task<IActionResult> Update()
        {
            await AddOrUpdateLink(new DoesNotFindFeed()); // FeedUrl could be removed on purpose, let's not re-add it
            return Redirect("/");
        }

        [HttpPost]
        public IActionResult Remove(int? id)
        {
            return id == null
                ? NotFound()
                : MaybeRemove((int)id);
        }

        private IActionResult MaybeRemove(int id)
        {
            var maybe = _blogRoll.Find(id);
            if (maybe.AmEmpty()) return NotFound();
            _blogRoll.Remove(maybe);
            _blogRoll.Save();
            return Redirect("/");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
