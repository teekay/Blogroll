using Microsoft.AspNetCore.Hosting;

namespace Blogroll.Web.Common
{
    public sealed class ResolvedContentRoot
    {
        public ResolvedContentRoot(IWebHostEnvironment env)
        {
            _env = env;
        }

        private readonly IWebHostEnvironment _env;

        public string ContentRoot() => _env.ContentRootPath;

        public override string ToString() => ContentRoot();
    }
}
