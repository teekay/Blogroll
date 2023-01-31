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
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;


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
            if (!_supportedExtensions.Contains(ext))
            {
                return new HttpResponseMessage(HttpStatusCode.BadRequest)
                {
                    Content = new StringContent($"Unsupported format: {ext}")
                };
            }

            var printed = await Printed(context, ext);
            var contentType = _extToContentType[ext];

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(printed, Encoding.UTF8, contentType)
            };
        }

        [FunctionName("blogroll-cache")]
        public async Task Cache(
            [TimerTrigger("0 0 * * * *")] TimerInfo timer,
            ExecutionContext context,
            ILogger log)
        {
            log.Log(LogLevel.Information, "Cache the blogroll to Blob storage");

            var storageConnectionString = Environment.GetEnvironmentVariable("AzureWebJobsStorage");
            var storageAccount = CloudStorageAccount.Parse(storageConnectionString);
            var cloudBlobClient = storageAccount.CreateCloudBlobClient();
            var cloudBlobContainer = cloudBlobClient.GetContainerReference("links");

            foreach (var supportedExtension in _supportedExtensions)
            {
                var output = await Printed(context, supportedExtension);
                var fname = $"links.{supportedExtension}";
                var cloudBlockBlob = cloudBlobContainer.GetBlockBlobReference(fname);
                log.Log(LogLevel.Information, $"Uploading to Blob storage as blob:\n\t {cloudBlockBlob.Uri}\n");
                await cloudBlockBlob.UploadTextAsync(output);
            }
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

            var blogroll = Blogroll();
            blogroll.Add(new Link(link.Name, link.Url, link.FeedUrl));
            blogroll.Save();

            return new HttpResponseMessage(HttpStatusCode.Accepted);
        }

        private readonly List<string> _supportedExtensions = new() { "json", "txt", "html" };

        private readonly Dictionary<string, string> _extToContentType = new()
        {
            {"txt", "text/plain"},
            {"html", "text/html"},
            {"json", "application/json"}
        };

        private BlogrollInAzureTables Blogroll()
        {
            var storageConnectionString = Environment.GetEnvironmentVariable("AzureWebJobsStorage");
            var tableClient = new TableClient(storageConnectionString, @"links");
            return new BlogrollInAzureTables(tableClient, new BlogRoll(), new ReadsFeedWithFeedReader());
        }

        private async Task<string> Printed(ExecutionContext context, string ext)
        {
            var templateRootPath = Path.Join(context.FunctionAppDirectory, "Views");
            var blogroll = Blogroll();
            var printed = await blogroll.PrintedTo(Printer(ext, templateRootPath));
            return printed;
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

        private record LinkRequest(string Name, string Url, string FeedUrl);
    }
}
