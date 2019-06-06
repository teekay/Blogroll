using System.Collections.Generic;
using System.Threading.Tasks;
using Blogroll.Common.Media;
using Blogroll.Web.Common;
using NUnit.Framework;

namespace Blogroll.Tests
{
    [TestFixture]
    internal class LinkFromRequestTests
    {
        [Test]
        public async Task WontFindFeed()
        {
            var link = await new LinkFromRequest(new[]
                {
                    new KeyValuePair<string, string>("Url", "https://blog.example.com"),
                    new KeyValuePair<string, string>("Name", "Some blog"),
                }, new DummyFeedDiscovery(string.Empty, string.Empty, false))
                .Link();
            Assert.IsFalse(link.CanRead());
        }

        [Test]
        public async Task WillFindFeed()
        {
            var link = await new LinkFromRequest(new[]
                {
                    new KeyValuePair<string, string>("Url", "https://blog.example.com"),
                    new KeyValuePair<string, string>("Name", "Some blog"),
                }, new DummyFeedDiscovery("Some blog", "https://blog.example.com/feed.rss", true))
                .Link();
            Assert.IsTrue(link.CanRead());
        }

        [Test]
        public async Task WillRespectTheEnteredFeedUrl()
        {
            var link = await new LinkFromRequest(new[]
                {
                    new KeyValuePair<string, string>("Url", "https://blog.example.com"),
                    new KeyValuePair<string, string>("Name", "Some blog"),
                    new KeyValuePair<string, string>("FeedUrl", "https://blog.example.com/syndicated"),
                }, new DummyFeedDiscovery("Some blog", "https://blog.example.com/feed.rss", true))
                .Link();
            Assert.IsTrue(link.CanRead());
            Assert.AreEqual("https://blog.example.com/syndicated", link.PrintedTo(new TextMedia("{{FeedUrl}}")));
        }

        [Test]
        public async Task WillKeepTheProvidedName()
        {
            var link = await new LinkFromRequest(new[]
                {
                    new KeyValuePair<string, string>("Url", "https://blog.example.com"),
                    new KeyValuePair<string, string>("Name", "Cool blog"),
                }, new DummyFeedDiscovery("Cool blog (feed)", "https://blog.example.com/feed.rss", true))
                .Link();
            Assert.AreEqual("Cool blog", link.PrintedTo(new TextMedia("{{Name}}")));
        }

        [Test]
        public async Task WillSupplyNameFromFeed()
        {
            var link = await new LinkFromRequest(new[]
                {
                    new KeyValuePair<string, string>("Url", "https://blog.example.com"),
                    new KeyValuePair<string, string>("Name", string.Empty),
                }, new DummyFeedDiscovery("Cool blog (feed)", "https://blog.example.com/feed.rss", true))
                .Link();
            Assert.AreEqual("Cool blog (feed)", link.PrintedTo(new TextMedia("{{Name}}")));
        }

    }
}
