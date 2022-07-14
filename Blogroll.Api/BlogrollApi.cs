using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Azure.Core.Diagnostics;
using Azure.Data.Tables;
using Blogroll.Common.Links;
using Blogroll.Persistence.AzureTables;
using Blogroll.Web.Common;
using Blogroll.Web.Common.BlogRollPrinters;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Blogroll.Api
{
    public class BlogrollApi
    {
        [FunctionName("blogroll")]
        public async Task<HttpResponseMessage> Get(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "links.{ext:alpha}")] HttpRequest req,
            ExecutionContext context,
            string ext,
            ILogger log)
        {
            var supportedFormats = new[] { "json", "txt", "html" };
            if (!supportedFormats.Contains(ext))
            {
                return new HttpResponseMessage(HttpStatusCode.BadRequest)
                {
                    Content = new StringContent($"Unsupported format: {ext}")
                };
            }

            var templateRootPath = Path.Join(context.FunctionAppDirectory, "Views");
            var storageConnectionString = Environment.GetEnvironmentVariable("AzureWebJobsStorage");
            var tableClient = new TableClient(storageConnectionString, @"links");
            var blogroll = new BlogrollInAzureTables(tableClient, new BlogRoll(), new ReadsFeedWithFeedReader());
            var printed = await blogroll.PrintedTo(Printer(ext, templateRootPath));
            var contentType = _extToContentType[ext];

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(printed, Encoding.UTF8, contentType)
            };
        }

        private IBlogRollMediaSource Printer(string ext, string rootPath)
        {
            return ext switch
            {
                "txt" => new PrintsToText(
                    "{{Links}}", 
                    "{{Link}}",
                    System.IO.File.ReadAllText(Path.Join(rootPath, "_link.txt")),
                    string.Empty, "\n"
                    ),
                "html" => new PrintsToHtml(
                    System.IO.File.ReadAllText(Path.Join(rootPath, "_blogroll.hbs")),
                    System.IO.File.ReadAllText(Path.Join(rootPath, "_link_container.hbs")),
                    System.IO.File.ReadAllText(Path.Join(rootPath, "_link.hbs")),
                    System.IO.File.ReadAllText(Path.Join(rootPath, "_snippet.hbs")),
                    "\n", true),
                "json" => new PrintsToJson(true),
                _ => throw new ArgumentOutOfRangeException(nameof(ext), ext, $"Unsupported extension: {ext}")
            };
        }

        [FunctionName("blogroll-manage")]
        public async Task<HttpResponseMessage> Post(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "links")] HttpRequest req,
            ExecutionContext context,
            ILogger log)
        {
            log.Log(LogLevel.Information, "Post a new link");
            var payload = await new StreamReader(req.Body).ReadToEndAsync();
            log.Log(LogLevel.Information, payload);
            var link = System.Text.Json.JsonSerializer.Deserialize<LinkRequest>(payload);
            if (link == null)
            {
                return new HttpResponseMessage(HttpStatusCode.BadRequest);
            }
            using AzureEventSourceListener listener = AzureEventSourceListener.CreateConsoleLogger();
            var storageConnectionString = Environment.GetEnvironmentVariable("AzureWebJobsStorage");
            var tableClient = new TableClient(storageConnectionString, @"links");
            var blogroll = new BlogrollInAzureTables(tableClient, new BlogRoll(), new ReadsFeedWithFeedReader());
            blogroll.Add(new Link(link.Name, link.Url, link.FeedUrl));
            blogroll.Save();

            return new HttpResponseMessage(HttpStatusCode.Accepted);
        }

        private readonly Dictionary<string, string> _extToContentType = new()
        {
            {"txt", "text/plain"},
            {"html", "text/html"},
            {"json", "application/json"}
        };

        private record LinkRequest(string Name, string Url, string FeedUrl);
    }
}
