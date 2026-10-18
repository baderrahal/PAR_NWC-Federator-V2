using System.Collections.Generic;
using System.IO;
using System.Linq;
using Federator.Core.Communities;
using NUnit.Framework;

namespace Federator.Core.Tests
{
    /// <summary>
    /// F208. Nothing is ticked at the start, a community ticks all its packages, a package is ticked alone, Tick all and
    /// Clear all, and Run is refused with a reason while none is ticked.
    /// </summary>
    [TestFixture]
    public class PackageTicksTests
    {
        private string root;
        private PackageTree tree;

        [SetUp]
        public void MakeTree()
        {
            root = TempFolder.Make("ticks");
            Make("C06", "Infrastructure and Landscape", "1104-PAR-1B06BC-ZZZ-AR-MOD-000001.nwc");
            Make("C06", "Infrastructure", "1104-PAR-1B06BC-ZZZ-EL-MOD-000001.nwc");
            Make("C06", "Infrastructure", "1104-PAR-1B06BC-ZZZ-ME-MOD-000001.nwc");
            Make("C07", "1104-PAR-1B06BC-ZZZ-ST-MOD-000001.nwc");
            Make("Dummy", "1104-PAR-1B06BC-ZZZ-AR-MOD-000002.nwc");
            tree = PackageTree.Read(root, ".nwc");
        }

        [TearDown]
        public void RemoveTree()
        {
            TempFolder.Remove(root);
        }

        private void Make(params string[] parts)
        {
            string path = Path.Combine(new[] { root }.Concat(parts).ToArray());
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            File.WriteAllText(path, "nwc");
        }

        private Community CommunityNamed(string name)
        {
            return tree.Communities.Single(c => c.Name == name);
        }

        [Test]
        public void NothingIsTickedAtTheStartAndRunIsRefusedWithAReason()
        {
            PackageTicks ticks = new PackageTicks(tree);

            Assert.That(ticks.Ticked(), Is.Empty);
            Assert.That(ticks.LeftOut().Count, Is.EqualTo(4));
            Assert.That(ticks.WhyNoRun(), Is.EqualTo(PackageWords.NothingTicked));
        }

        [Test]
        public void TickingACommunityTicksEveryOneOfItsPackagesAndNoOther()
        {
            PackageTicks ticks = new PackageTicks(tree);
            ticks.TickCommunity(CommunityNamed("C06"), true);

            Assert.That(ticks.Ticked().Select(p => p.Community).Distinct().Single(), Is.EqualTo("C06"));
            Assert.That(ticks.Ticked().Count, Is.EqualTo(2));
            Assert.That(ticks.StateOf(CommunityNamed("C06")), Is.True);
            Assert.That(ticks.StateOf(CommunityNamed("C07")), Is.False);
            Assert.That(ticks.WhyNoRun(), Is.Null);
        }

        [Test]
        public void APackageTickedAloneIsTheOnlyOneThatRunsAndItsCommunityReadsSome()
        {
            PackageTicks ticks = new PackageTicks(tree);
            Package infra = CommunityNamed("C06").Packages.Single(p => p.Name == "Infrastructure");
            ticks.Tick(infra, true);

            Assert.That(ticks.Ticked().Count, Is.EqualTo(1));
            Assert.That(ticks.Ticked()[0], Is.SameAs(infra));
            Assert.That(ticks.StateOf(CommunityNamed("C06")), Is.Null);
        }

        [Test]
        public void UntickingACommunityClearsItsPackages()
        {
            PackageTicks ticks = new PackageTicks(tree);
            ticks.TickAll();
            ticks.TickCommunity(CommunityNamed("C06"), false);

            Assert.That(ticks.Ticked().Select(p => p.RelativePath).ToArray(), Is.EqualTo(new[] { "C07", "Dummy" }));
        }

        [Test]
        public void TickAllTicksEveryPackageAndClearAllNone()
        {
            PackageTicks ticks = new PackageTicks(tree);
            ticks.TickAll();
            Assert.That(ticks.Ticked().Count, Is.EqualTo(4));

            ticks.ClearAll();
            Assert.That(ticks.Ticked(), Is.Empty);
            Assert.That(ticks.WhyNoRun(), Is.EqualTo(PackageWords.NothingTicked));
        }

        [Test]
        public void TheTickedRunInTheTreesOrderWhicheverWasTickedFirst()
        {
            PackageTicks ticks = new PackageTicks(tree);
            ticks.Tick(tree.Find("Dummy"), true);
            ticks.Tick(tree.Find("C07"), true);

            Assert.That(ticks.Ticked().Select(p => p.RelativePath).ToArray(), Is.EqualTo(new[] { "C07", "Dummy" }));
        }

        [Test]
        public void TheLinesNameEveryPackageTickedAndLeftOutWithItsPathAndTheCountOfEach()
        {
            PackageTicks ticks = new PackageTicks(tree);
            ticks.TickCommunity(CommunityNamed("C06"), true);

            IList<string> lines = ticks.Lines();
            string sep = Path.DirectorySeparatorChar.ToString();

            // F216, Bader's F-R4: every package line carries its full path as well as its path from the parent.
            Assert.That(lines, Has.Some.EqualTo("ticked      C06" + sep + "Infrastructure  2 NWC files, at "
                + Path.Combine(tree.Parent, "C06", "Infrastructure")));
            Assert.That(lines, Has.Some.EqualTo("ticked      C06" + sep + "Infrastructure and Landscape  1 NWC file, at "
                + Path.Combine(tree.Parent, "C06", "Infrastructure and Landscape")));
            Assert.That(lines, Has.Some.EqualTo("left out    C07  1 NWC file, at " + Path.Combine(tree.Parent, "C07")));
            Assert.That(lines, Has.Some.EqualTo("left out    Dummy  1 NWC file, at " + Path.Combine(tree.Parent, "Dummy")));
            Assert.That(lines, Has.Some.EqualTo("ticked      : 2 packages holding 3 NWC files"));
            Assert.That(lines, Has.Some.EqualTo("left out    : 2 packages holding 2 NWC files"));
        }

        [Test]
        public void AParentNotReadRefusesTheRunWithTheTreesReason()
        {
            PackageTicks ticks = new PackageTicks(PackageTree.Read(Path.Combine(root, "gone"), ".nwc"));

            Assert.That(ticks.WhyNoRun(), Does.StartWith("The parent folder is not there"));
        }

        [Test]
        public void APackageOfAnotherTreeIsNeverTicked()
        {
            string other = TempFolder.Make("other");

            try
            {
                Directory.CreateDirectory(Path.Combine(other, "C99"));
                File.WriteAllText(Path.Combine(other, "C99", "1104-PAR-1B99BC-ZZZ-AR-MOD-000001.nwc"), "nwc");
                Package stranger = PackageTree.Read(other, ".nwc").Packages()[0];

                PackageTicks ticks = new PackageTicks(tree);
                ticks.Tick(stranger, true);

                Assert.That(ticks.Ticked(), Is.Empty);
            }
            finally
            {
                TempFolder.Remove(other);
            }
        }
    }
}
