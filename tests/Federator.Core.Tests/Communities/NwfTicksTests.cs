using System.IO;
using System.Linq;
using Federator.Core.Communities;
using NUnit.Framework;

namespace Federator.Core.Tests
{
    /// <summary>
    /// F232, Bader's round of 19 Oct 2026, fix 2. The NWF page's ticks: nothing ticked at the start, a community or a package
    /// ticking every NWF under it, one NWF ticked alone, and a run refused while none is.
    /// </summary>
    [TestFixture]
    public class NwfTicksTests
    {
        private string root;
        private string a;
        private string b;
        private string c;

        [SetUp]
        public void MakeRoot()
        {
            root = TempFolder.Make("nwfticks");
            a = Make("C06", "NWF", "1104-PAR-1B06BC-ZZZ-BM-MOD-000001.nwf");
            b = Make("C06", "NWF", "1104-PAR-1B06BS-ZZZ-BM-MOD-000001.nwf");
            c = Make("C07", "Infra", "NWF", "1104-PAR-1C07BC-ZZZ-BM-MOD-000001.nwf");
        }

        [TearDown]
        public void RemoveRoot()
        {
            TempFolder.Remove(root);
        }

        private string Make(params string[] parts)
        {
            string path = Path.Combine(new[] { root, "Report" }.Concat(parts).ToArray());
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            File.WriteAllText(path, "nwf");
            return path;
        }

        private NwfTicks Ticks()
        {
            return new NwfTicks(PackageTree.ReadNwfs(Path.Combine(root, "Report")));
        }

        [Test]
        public void NothingIsTickedAtTheStartAndARunIsRefused()
        {
            NwfTicks ticks = Ticks();

            Assert.That(ticks.Ticked(), Is.Empty);
            Assert.That(ticks.WhyNoRun(), Is.EqualTo(PackageWords.NoNwfTicked));
        }

        [Test]
        public void TickingACommunityTicksEveryNwfUnderIt()
        {
            NwfTicks ticks = Ticks();
            Community c06 = ticks.Tree.Communities.First(x => x.Name == "C06");

            ticks.TickCommunity(c06, true);

            Assert.That(ticks.Ticked().ToArray(), Is.EqualTo(new[] { a, b }));
            Assert.That(ticks.StateOf(c06), Is.True);
            Assert.That(ticks.WhyNoRun(), Is.Null);
        }

        [Test]
        public void TickingAPackageTicksItsNwfsAndOneNwfIsTickedAlone()
        {
            NwfTicks ticks = Ticks();
            Package infra = ticks.Tree.Find(Path.Combine("C07", "Infra"));

            ticks.TickPackage(infra, true);
            ticks.Tick(a, true);

            Assert.That(ticks.Ticked().ToArray(), Is.EqualTo(new[] { a, c }), "the tree's order");
            Assert.That(ticks.StateOf(ticks.Tree.Find("C06")), Is.Null, "one of its two ticked");
            Assert.That(ticks.PackagesTicked().Select(p => p.RelativePath).ToArray(), Is.EqualTo(new[] { "C06", Path.Combine("C07", "Infra") }));

            ticks.Tick(a, false);

            Assert.That(ticks.StateOf(ticks.Tree.Find("C06")), Is.False);
        }

        [Test]
        public void APathTheTreeDoesNotHoldIsNeverTicked()
        {
            NwfTicks ticks = Ticks();

            ticks.Tick(Path.Combine(root, "elsewhere.nwf"), true);

            Assert.That(ticks.Ticked(), Is.Empty);
            Assert.That(ticks.WhyNoRun(), Is.EqualTo(PackageWords.NoNwfTicked));
        }

        [Test]
        public void TickAllAndClearAllSetEveryNwf()
        {
            NwfTicks ticks = Ticks();

            ticks.TickAll();
            Assert.That(ticks.Ticked().Count, Is.EqualTo(3));

            ticks.ClearAll();
            Assert.That(ticks.Ticked(), Is.Empty);
        }

        [Test]
        public void ATreeNotReadRefusesWithItsOwnReason()
        {
            NwfTicks ticks = new NwfTicks(PackageTree.ReadNwfs(Path.Combine(root, "Gone")));

            Assert.That(ticks.WhyNoRun(), Does.StartWith("The Report folder is not there"));
        }

        [Test]
        public void TheBlockNamesEveryNwfTickedAndLeftOutAndTheSummaryCountsThem()
        {
            NwfTicks ticks = Ticks();
            ticks.Tick(b, true);

            string[] lines = ticks.Lines().ToArray();

            Assert.That(lines.Length, Is.EqualTo(6));
            Assert.That(lines[0], Is.EqualTo("report      : " + Path.Combine(root, "Report")));
            Assert.That(lines[1], Is.EqualTo("left out    C06  " + a));
            Assert.That(lines[2], Is.EqualTo("ticked      C06  " + b));
            Assert.That(lines[4], Is.EqualTo("ticked      : 1 NWF file"));
            Assert.That(lines[5], Is.EqualTo("left out    : 2 NWF files"));
            Assert.That(PackageWords.NwfSummary(ticks.Tree, ticks), Is.EqualTo("2 communities, 2 packages, 3 NWF files. 1 NWF file ticked."));
        }
    }
}
