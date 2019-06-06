using System.IO;
using Blogroll.Common.Links;
using Blogroll.Common.Persistence;
using Blogroll.Persistence.LiteDB;
using Blogroll.Persistence.SQLite;
using Blogroll.Web.Common;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Blogroll.Web
{
    public class Startup
    {
        public Startup(IConfiguration configuration, IHostingEnvironment env)
        {
            Configuration = configuration;
            Environment = env;
        }

        public IConfiguration Configuration { get; }
        public IHostingEnvironment Environment { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            var contentRoot = new ResolvedContentRoot(Environment);
            services.AddSingleton<IAuthenticating>(s => new SingleUserAuth(Configuration["Admin:Password"]));
            services.AddSingleton<ResolvedContentRoot>(s => contentRoot);
            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });
            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(options =>
                {
                    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
                });
            var preferredDbStorage = Configuration["Data:Engine"]?.ToLowerInvariant() ?? "sqlite";
            var dataFolder = preferredDbStorage == "sqlite" ? "SQLite" : "LiteDb";
            var maybeStoragePath = Configuration["Data:Storage"] ?? string.Empty;
            var storagePath = Directory.Exists(maybeStoragePath)
                ? maybeStoragePath
                : $"{contentRoot}/Data/{dataFolder}";
            services.AddTransient<IPersistedBlogroll>(x => preferredDbStorage == "litedb"
            ? (IPersistedBlogroll) new BlogrollInLitedb(
                new BlogrollSimple(), $"{storagePath}/blogroll.litedb", new ReadsFeedWithFeedReader())
            : new BlogrollInSqlite(
                new BlogrollSimple(), $"{storagePath}/blogroll.sqlite", new ReadsFeedWithFeedReader()));
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseStaticFiles();
            app.UseCookiePolicy();
            app.UseAuthentication();
            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
