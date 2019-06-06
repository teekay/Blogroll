using Blogroll.Common.Links;
using NUnit.Framework;

namespace Blogroll.Tests
{
    [TestFixture]
    internal class LinkTests
    {
        [Test]
        public void LinkToLinkEqualityTest()
        {
            var link1 = new Link("https://tomaskohl.com/tango");
            var link2 = new Link("https://tomaskohl.com/tango");
            Assert.IsTrue(link1.Equals(link2));
            // ReSharper disable once PossibleUnintendedReferenceComparison
            Assert.IsTrue(link1 == link2);
        }

        [Test]
        public void LinkToBlogrollItemEqualityTest()
        {
            ILink link1 = new Link("https://tomaskohl.com/tango");
            BlogRollItem link2 = new BlogRollItem(new Link("https://tomaskohl.com/tango"), 1);
            Assert.IsTrue(link1.Equals(link2));
            // ReSharper disable once PossibleUnintendedReferenceComparison
            Assert.IsFalse(link1 == link2);
        }
    }
}
