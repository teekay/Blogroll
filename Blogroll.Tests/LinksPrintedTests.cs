using Blogroll.Common.Links;
using Blogroll.Common.Media;
using Blogroll.Web.Common.Media;
using NUnit.Framework;

namespace Blogroll.Tests
{
    [TestFixture]
    internal class LinksPrintedTests
    {
        [Test]
        public void LinkPrintedToText()
        {
            var printed = new Link("The blog", "https://blog.example.com", "https://blog.example.com/rss")
                .PrintedTo(new TextMedia("{{Name}} - {{Url}}, {{FeedUrl}}"));
            Assert.AreEqual("The blog - https://blog.example.com, https://blog.example.com/rss", printed);
        }

        [Test]
        public void BlogRollItemPrintedToText()
        {
            var printed = new BlogRollItem(new Link("The blog", "https://blog.example.com", "https://blog.example.com/rss"), 1)
                .PrintedTo(new TextMedia("{{Position}}: {{Name}} - {{Url}}, {{FeedUrl}}"));
            Assert.AreEqual("1: The blog - https://blog.example.com, https://blog.example.com/rss", printed);
        }

        [Test]
        public void LinkPrintedToHtml()
        {
            var printed = new Link("The blog", "https://blog.example.com", "https://blog.example.com/rss")
                .PrintedTo(new HtmlMedia("{{Name}} - {{Url}}, {{FeedUrl}}"));
            Assert.AreEqual("The blog - https://blog.example.com, https://blog.example.com/rss", printed);
        }

        [Test]
        public void BlogRollItemPrintedToHtml()
        {
            var printed = new BlogRollItem(new Link("The blog", "https://blog.example.com", "https://blog.example.com/rss"), 1)
                .PrintedTo(new HtmlMedia("{{Position}}: {{Name}} - {{Url}}, {{FeedUrl}}"));
            Assert.AreEqual("1: The blog - https://blog.example.com, https://blog.example.com/rss", printed);
        }

    }
}
