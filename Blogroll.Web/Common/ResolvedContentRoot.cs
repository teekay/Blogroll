using Microsoft.AspNetCore.Hosting;

namespace Blogroll.Web.Common
{
    public sealed class ResolvedContentRoot
    {
        public ResolvedContentRoot(IHostingEnvironment env)
        {
            _env = env;
        }

        private readonly IHostingEnvironment _env;

        public string ContentRoot() => _env.ContentRootPath;

        public override string ToString() => ContentRoot();
    }
}
