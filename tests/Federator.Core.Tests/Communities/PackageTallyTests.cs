using System.Collections.Generic;
using System.IO;
using System.Linq;
using Federator.Core.Diagnostics;
using Federator.Core.Communities;
using NUnit.Framework;

namespace Federator.Core.Tests
{
    /// <summary>
    /// F208. RESULT names community and package on every group, with one total per package, one per community and one
    /// for the run, and a package that ran nothing is counted with why.
    /// </summary>
    [TestFixture]
    public class PackageTallyTests
    {
        private string root;
        private PackageTree tree;

        [SetUp]
        public void MakeTree()
        {
            root = TempFolder.Make("tally");
            Make("C06", "Infrastructure", "1104-PAR-1B06BC-ZZZ-AR-MOD-000001.nwc");
            Make("C06", "Infrastructure and Landscape", "1104-PAR-1B06G1-ZZZ-AR-MOD-000001.nwc");
            Make("C07", "1104-PAR-1B06BC-ZZZ-AR-MOD-000001.nwc");
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

        private static string Rel(params string[] parts)
        {
            return string.Join(Path.DirectorySeparatorChar.ToString(), parts);
        }

        [Test]
        public void EveryGroupIsNamedWithItsPackageAndEachPackageCommunityAndTheRunHasItsTotal()
        {
            PackageTally tally = new PackageTally();
            Package infra = tree.Find(Rel("C06", "Infrastructure"));
            Package landscape = tree.Find(Rel("C06", "Infrastructure and Landscape"));
            Package c07 = tree.Find("C07");
            tally.Add(infra, "1B06BC", GroupOutcome.Done, 120);
            tally.Add(landscape, "1B06G1", GroupOutcome.Failed, 0);
            tally.Add(c07, "1B06BC", GroupOutcome.Partial, 30);

            IList<string> lines = tally.Lines();

            Assert.That(lines, Has.Some.EqualTo("group          " + Rel("C06", "Infrastructure") + "  1B06BC  DONE  120 clashes"));
            Assert.That(lines, Has.Some.EqualTo("group          C07  1B06BC  PARTIAL  30 clashes"));
            Assert.That(lines, Has.Some.EqualTo("package        " + Rel("C06", "Infrastructure") + " : 1 group, 1 done, 0 partial, 0 failed, 120 clashes"));
            Assert.That(lines, Has.Some.EqualTo("community      C06 : 2 packages, 2 groups, 1 done, 0 partial, 1 failed, 120 clashes"));
            Assert.That(lines, Has.Some.EqualTo("community      C07 : 1 package, 1 group, 0 done, 1 partial, 0 failed, 30 clashes"));
            Assert.That(lines.Last(), Is.EqualTo("run            : 2 communities, 3 packages, 3 groups, 1 done, 1 partial, 1 failed, 150 clashes"));
        }

        [Test]
        public void APackageThatRanNoGroupIsCountedWithWhy()
        {
            PackageTally tally = new PackageTally();
            tally.Add(tree.Find("C07"), "1B06BC", GroupOutcome.Done, 3);
            tally.NotRun(tree.Find(Rel("C06", "Infrastructure")), "no NWF in its NWF folder");

            IList<string> lines = tally.Lines();

            Assert.That(lines, Has.Some.EqualTo("package        " + Rel("C06", "Infrastructure") + " : not run, no NWF in its NWF folder"));
            Assert.That(lines.Last(), Is.EqualTo("run            : 2 communities, 2 packages, 1 group, 1 done, 0 partial, 0 failed, 3 clashes"));
        }

        [Test]
        public void AClashCountNotTakenIsUnknownAndTheTotalSaysAtLeast()
        {
            PackageTally tally = new PackageTally();
            tally.Add(tree.Find("C07"), "1B06BC", GroupOutcome.Failed, -1);
            tally.Add(tree.Find(Rel("C06", "Infrastructure")), "1B06BC", GroupOutcome.Done, 5);

            IList<string> lines = tally.Lines();

            Assert.That(lines, Has.Some.EqualTo("group          C07  1B06BC  FAILED  clashes UNKNOWN"));
            Assert.That(lines.Last(), Does.EndWith("at least 5 clashes, a count was not taken"));
        }

        [Test]
        public void ARunOfNoPackageSaysSo()
        {
            Assert.That(new PackageTally().Lines().Single(), Is.EqualTo("packages       : none ran in this run"));
        }
    }
}
