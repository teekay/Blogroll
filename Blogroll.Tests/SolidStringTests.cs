using Blogroll.Common.Commons;
using NUnit.Framework;

namespace Blogroll.Tests
{
    [TestFixture]
    internal class SolidStringTests
    {
        [Test]
        public void OneSolidEqualsAnother()
        {
            var ss1 = new SolidString("one");
            var ss2 = new SolidString("one");
            Assert.IsTrue(ss1.Equals(ss2));
            Assert.IsTrue(ss1.Equals((object)ss2));
            Assert.IsTrue(ss1 == ss2);
            Assert.AreEqual(ss1, ss2);
        }

        [Test]
        public void OneSolidEqualsSameString()
        {
            var ss1 = new SolidString("one");
            var ss2 = "one";
            Assert.IsTrue(ss1.Equals(ss2));
            // ReSharper disable once SuspiciousTypeConversion.Global
            Assert.IsTrue(ss1.Equals((object)ss2));
            Assert.IsTrue(ss1 == ss2);
            Assert.AreEqual(ss1, ss2);
        }

        [Test]
        public void OneSolidDiffersFromAnother()
        {
            var ss1 = new SolidString("one");
            var ss2 = new SolidString("two");
            Assert.IsFalse(ss1.Equals(ss2));
            Assert.IsFalse(ss1.Equals((object)ss2));
            Assert.IsTrue(ss1 != ss2);
            Assert.AreNotEqual(ss1, ss2);
        }

        [Test]
        public void OneSolidDiffersFromSameString()
        {
            var ss1 = new SolidString("one");
            var ss2 = "two";
            Assert.IsFalse(ss1.Equals(ss2));
            // ReSharper disable once SuspiciousTypeConversion.Global
            Assert.IsFalse(ss1.Equals((object)ss2));
            Assert.IsTrue(ss1 != ss2);
            Assert.AreNotEqual(ss1, ss2);
        }

        [Test]
        public void IsNeverNull()
        {
            var ss1 = new SolidString(null);
            var ss2 = new SolidString(string.Empty);
            var s3 = string.Empty;
            Assert.AreEqual(ss1, ss2);
            Assert.AreEqual(ss1, s3);
            Assert.AreEqual(ss2, s3);
        }

    }
}
