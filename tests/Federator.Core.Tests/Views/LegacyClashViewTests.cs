using System.Collections.Generic;
using Federator.Core.Views;
using NUnit.Framework;

namespace Federator.Core.Tests
{
    /// <summary>
    /// F114, Q114 point 16: the per clash viewpoints of earlier runs are the tool's and go, and
    /// they carry no mark, measure-views section 8. One is known by a strict shape: an optional
    /// priority word, a pair of two known codes or UNKNOWN sorted as F85 sorted them, an optional
    /// Over 150mm, then a leaf of a test's name, two spaces, Clash and digits only, with no
    /// comment and no redline. Anything else is a person's. The names are sample data only, the
    /// first one the baseline's first viewpoint, baseline log line 431.
    /// </summary>
    [TestFixture]
    public class LegacyClashViewTests
    {
        private const string Test = "BLD-AR-Curtain Mullions-vs-BLD-AR-Walls";

        private static readonly string[] Codes = { "AR", "ST", "ME", "FF", "PL", "DR", "EL" };

        private static string TestOf(string[] folders, string leaf, bool isFolder = false, int comments = 0, int? redlines = 0)
        {
            return LegacyClashView.TestOf(
                folders, leaf, isFolder, comments, redlines, Codes, new List<string> { Test, "BLD-ME-Ducts-vs-BLD-ST-Columns" }, new ViewpointSettings());
        }

        [Test]
        public void TheBaselinesFirstViewpointIsAPerClashViewpointOfAnEarlierRun()
        {
            Assert.That(TestOf(new[] { "AR vs AR" }, Test + "  Clash1"), Is.EqualTo(Test));
        }

        [Test]
        public void ThePriorityAndSizeLayersAreReadToo()
        {
            Assert.That(TestOf(new[] { "A", "ME vs ST", "Over 150mm" }, "BLD-ME-Ducts-vs-BLD-ST-Columns  Clash204"),
                Is.EqualTo("BLD-ME-Ducts-vs-BLD-ST-Columns"));
            Assert.That(TestOf(new[] { "No priority", "AR vs AR" }, Test + "  Clash9"), Is.EqualTo(Test));
            Assert.That(TestOf(new[] { "EL vs UNKNOWN" }, Test + "  Clash9"), Is.EqualTo(Test), "UNKNOWN sorts after every code");
        }

        [Test]
        public void EveryOtherShapeIsAPersons()
        {
            Assert.That(TestOf(new[] { "AR vs AR" }, Test + " Clash1"), Is.Null, "one space");
            Assert.That(TestOf(new[] { "ST vs AR" }, Test + "  Clash1"), Is.Null, "a pair F85 never wrote, unsorted");
            Assert.That(TestOf(new[] { "A", "AR vs AR", "Over 150mm", "x" }, Test + "  Clash1"), Is.Null, "depth 4");
            Assert.That(TestOf(new string[0], Test + "  Clash1"), Is.Null, "at the root");
            Assert.That(TestOf(new[] { "AR vs AR" }, "Walls-vs-Doors  Clash1"), Is.Null, "a test neither the document nor the XML holds");
            Assert.That(TestOf(new[] { "AR vs AR" }, Test + "  Clash"), Is.Null, "Clash with no digits");
            Assert.That(TestOf(new[] { "AR vs AR" }, Test + "  Clash12a"), Is.Null, "not digits only");
            Assert.That(TestOf(new[] { "AR vs AR" }, Test + "  Clash1", comments: 1), Is.Null, "a comment makes it a person's");
            Assert.That(TestOf(new[] { "AR vs AR" }, Test + "  Clash1", redlines: 1), Is.Null, "a redline makes it a person's");
            Assert.That(TestOf(new[] { "AR vs AR" }, Test + "  Clash1", redlines: null), Is.Null, "redlines not read prove nothing");
            Assert.That(TestOf(new[] { "AR vs AR" }, Test + "  Clash1", isFolder: true), Is.Null, "a folder");
            Assert.That(TestOf(new[] { "XX vs YY" }, Test + "  Clash1"), Is.Null, "codes nobody knows");
            Assert.That(TestOf(new[] { "Architecture vs Structure" }, Test + "  Clash1"), Is.Null, "a team pair is this tool's new tree, not F85's");
            Assert.That(TestOf(new[] { "Over 150mm", "AR vs AR" }, Test + "  Clash1"), Is.Null, "the size folder above the pair");
        }
    }
}
