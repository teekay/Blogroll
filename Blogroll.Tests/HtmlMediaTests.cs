using Blogroll.Web.Common.Media;
using NUnit.Framework;

namespace Blogroll.Tests
{
    [TestFixture]
    internal class HtmlMediaTests
    {
        [Test]
        public void UnconstrainedPrintsEverythingOneParam()
        {
            var printed = new HtmlMedia("{{Name}}").With("Name", "Clarissa").ToString();
            Assert.AreEqual("Clarissa", printed);
        }

        [Test]
        public void UnconstrainedPrintsEverythingTwoParams()
        {
            var printed = new HtmlMedia("{{Name}}: {{Gender}}")
                .With("Name", "Clarissa")
                .With("Gender", "Non-conforming")
                .ToString();
            Assert.AreEqual("Clarissa: Non-conforming", printed);
        }

    }
}
