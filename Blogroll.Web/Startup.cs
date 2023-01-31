using System.IO;
using Azure.Data.Tables;
using Blogroll.Common.Links;
using Blogroll.Common.Persistence;
using Blogroll.Persistence.AzureTables;
using Blogroll.Persistence.LiteDB;
using Blogroll.Persistence.SQLite;
using Blogroll.Web.Common;
using Blogroll.Web.Common.Common;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Blogroll.Web
{
    public class Startup
    {
        public Startup(IConfiguration configuration, IWebHostEnvironment env)
        {
            Configuration = configuration;
            Environment = env;
        }

        public IConfiguration Configuration { get; }
        public IWebHostEnvironment Environment { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            var contentRoot = new ResolvedContentRoot(Environment);
            services.AddSingleton<IAuthenticating>(s => new SingleUserAuth(Configuration["Admin:Password"]));
            services.AddSingleton<ResolvedContentRoot>(s => contentRoot);
            services.AddAuthorization();
            services.AddControllers();
            services.AddMvc();
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
            services.AddTransient<IPersistedBlogroll>(x => PersistedBlogroll(preferredDbStorage, storagePath));
            //services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_3_0);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseRouting();
            app.UseStaticFiles();
            app.UseCookiePolicy();
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }

        private IPersistedBlogroll PersistedBlogroll(string dbApi, string storagePath)
        {
            return dbApi switch
            {
                "litedb" => new BlogrollInLitedb(
                    new BlogrollSimple(), $"{storagePath}/blogroll.litedb", new ReadsFeedWithFeedReader()),
                "sqlite" => new BlogrollInSqlite(
                    new BlogrollSimple(), $"{storagePath}/blogroll.sqlite", new ReadsFeedWithFeedReader()),
                "azuretables" => new BlogrollInAzureTables(
                    new TableClient(Configuration["AzureWebJobsStorage"], @"links"), new BlogRoll(), new ReadsFeedWithFeedReader()),
                _ => throw new System.Exception($"Unsupported DB API: {dbApi}")
            };
        }
    }
}
