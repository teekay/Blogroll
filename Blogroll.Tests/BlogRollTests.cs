using System.Threading.Tasks;
using Blogroll.Common.Links;
using Blogroll.Web.Common.BlogRollPrinters;
using NUnit.Framework;

namespace Blogroll.Tests
{
    [TestFixture]
    internal class BlogRollTests
    {
        [Test]
        public async Task LinksArePositionedOnTheFiFoBasis()
        {
            var link1 = new Link("Test 1", "https://blog.example.com/1");
            var link2 = new Link("Test 2", "https://blog.example.com/2");
            var roll = new BlogRoll();
            roll.Add(link1);
            roll.Add(link2);
            var printed = await roll.PrintedTo(new PrintsToText("{{Links}}", 
                "{{Link}}",
                "{{Position}} - {{Name}}",
                string.Empty, ","));
            Assert.AreEqual("1 - Test 1,2 - Test 2", printed);
        }

        [Test]
        public async Task GuaranteesUniquePositionsWhenPrinting()
        {
            var link1 = new BlogRollItem(new Link("Test 1", "https://blog.example.com/1"), 1);
            var link2 = new BlogRollItem(new Link("Test 2", "https://blog.example.com/2"), 1);
            var roll = new BlogRoll();
            roll.Add(link1);
            roll.Add(link2);
            var printed = await roll.PrintedTo(new PrintsToText("{{Links}}", 
                "{{Link}}",
                "{{Position}} - {{Name}}",
                string.Empty, ","));
            Assert.AreEqual("1 - Test 1,2 - Test 2", printed);
        }

        [Test]
        public async Task CanMoveLinks()
        {
            var link1 = new BlogRollItem(new Link("Test 1", "https://blog.example.com/1"), 1);
            var link2 = new BlogRollItem(new Link("Test 2", "https://blog.example.com/2"), 1);
            var roll = new BlogRoll();
            roll.Add(link1);
            roll.Add(link2);
            roll.Move(1, 2);
            var printed = await roll.PrintedTo(new PrintsToText("{{Links}}",
                "{{Link}}",
                "{{Position}} - {{Name}}",
                string.Empty, ","));
            Assert.AreEqual("1 - Test 2,2 - Test 1", printed);
        }

        [Test]
        public async Task CanRemoveLinks()
        {
            var link1 = new BlogRollItem(new Link("Test 1", "https://blog.example.com/1"), 1);
            var link2 = new BlogRollItem(new Link("Test 2", "https://blog.example.com/2"), 1);
            var link3 = new BlogRollItem(new Link("Test 3", "https://blog.example.com/3"), 1);
            var roll = new BlogRoll();
            roll.Add(link1);
            roll.Add(link2);
            roll.Add(link3);
            Assert.AreEqual("1 - Test 1,2 - Test 2,3 - Test 3", 
                await roll.PrintedTo(new PrintsToText("{{Links}}",
                "{{Link}}",
                "{{Position}} - {{Name}}",
                string.Empty, ",")));
            roll.Remove(link2);
            Assert.AreEqual("1 - Test 1,2 - Test 3",
                await roll.PrintedTo(new PrintsToText("{{Links}}",
                    "{{Link}}",
                    "{{Position}} - {{Name}}",
                    string.Empty, ",")));
        }

        [Test]
        public void FindsByPosition()
        {
            var link1 = new Link("Test 1", "https://blog.example.com/1");
            var link2 = new Link("Test 2", "https://blog.example.com/2");
            var link3 = new Link("Test 3", "https://blog.example.com/3");
            var roll = new BlogRoll();
            roll.Add(link1);
            roll.Add(link2);
            roll.Add(link3);
            Assert.AreEqual(roll.Find(2),  link2);
            Assert.NotNull(roll.Find(66));
            Assert.IsTrue(roll.Find(66).AmEmpty());
        }

        [Test]
        public void FindsPositionOfLink()
        {
            var link1 = new Link("Test 1", "https://blog.example.com/1");
            var link2 = new Link("Test 2", "https://blog.example.com/2");
            var link3 = new Link("Test 3", "https://blog.example.com/3");
            var roll = new BlogRoll();
            roll.Add(link1);
            roll.Add(link2);
            roll.Add(link3);
            Assert.AreEqual(2, roll.PositionOf(link2));
        }
    }
}