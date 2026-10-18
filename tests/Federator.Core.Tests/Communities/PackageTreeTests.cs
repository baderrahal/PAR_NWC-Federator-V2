using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Federator.Core.Communities;
using Federator.Core.Rerun;
using NUnit.Framework;

namespace Federator.Core.Tests
{
    /// <summary>
    /// F208, Bader's message of 17 Oct 2026. The communities and packages under one parent, read off folders made on the
    /// disk, in the local test layout and the ACC one, with names holding spaces, the word and and an ampersand.
    /// </summary>
    [TestFixture]
    public class PackageTreeTests
    {
        private string root;

        [SetUp]
        public void MakeRoot()
        {
            root = TempFolder.Make("packages");
        }

        [TearDown]
        public void RemoveRoot()
        {
            TempFolder.Remove(root);
        }

        private string File(params string[] parts)
        {
            string path = Path.Combine(new[] { root }.Concat(parts).ToArray());
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            System.IO.File.WriteAllText(path, "nwc");
            return path;
        }

        private string Parent
        {
            get { return Path.Combine(root, "Parent"); }
        }

        private static string Rel(params string[] parts)
        {
            return string.Join(Path.DirectorySeparatorChar.ToString(), parts);
        }

        [Test]
        public void TheFlatLayoutMakesEachCommunityItsOwnPackage()
        {
            File("Parent", "C06", "1104-PAR-1B06BC-ZZZ-AR-MOD-000001.nwc");
            File("Parent", "C06", "1104-PAR-1B06BC-ZZZ-EL-MOD-000001.nwc");
            File("Parent", "C07", "1104-PAR-1B06BC-ZZZ-ME-MOD-000001.nwc");

            PackageTree tree = PackageTree.Read(Parent, ".nwc");

            Assert.That(tree.WasRead, Is.True);
            Assert.That(tree.Communities.Count, Is.EqualTo(2));
            Assert.That(tree.Communities[0].Name, Is.EqualTo("C06"));
            Assert.That(tree.Communities[1].Name, Is.EqualTo("C07"));
            IList<Package> packages = tree.Packages();
            Assert.That(packages.Count, Is.EqualTo(2));
            Assert.That(packages[0].RelativePath, Is.EqualTo("C06"));
            Assert.That(packages[0].IsTheCommunityItself, Is.True);
            Assert.That(packages[0].Name, Is.EqualTo("C06 itself"));
            Assert.That(packages[0].Files.Count, Is.EqualTo(2));
            Assert.That(packages[1].Files.Count, Is.EqualTo(1));
        }

        [Test]
        public void TheAccLayoutPutsTwoPackagesSideBySideUnderTheCommunity()
        {
            File("Parent", "C06", "Infrastructure and Landscape", "1104-PAR-1B06BC-ZZZ-AR-MOD-000001.nwc");
            File("Parent", "C06", "Infrastructure", "1104-PAR-1B06BC-ZZZ-EL-MOD-000001.nwc");
            File("Parent", "C06", "Infrastructure", "1104-PAR-1B06BC-ZZZ-ME-MOD-000001.nwc");

            PackageTree tree = PackageTree.Read(Parent, ".nwc");

            Assert.That(tree.Communities.Count, Is.EqualTo(1));
            IList<Package> packages = tree.Packages();
            Assert.That(packages.Count, Is.EqualTo(2));
            Assert.That(packages[0].RelativePath, Is.EqualTo(Rel("C06", "Infrastructure")));
            Assert.That(packages[0].Files.Count, Is.EqualTo(2));
            Assert.That(packages[1].RelativePath, Is.EqualTo(Rel("C06", "Infrastructure and Landscape")));
            Assert.That(packages[1].Name, Is.EqualTo("Infrastructure and Landscape"));
            Assert.That(packages[1].Files.Count, Is.EqualTo(1));
            Assert.That(packages[1].Community, Is.EqualTo("C06"));
        }

        [Test]
        public void AFolderHoldingFilesAndSubfoldersIsOnePackageAndEachSubfolderAnotherNeverMixed()
        {
            string own = File("Parent", "C06", "Roads", "1104-PAR-1B06BC-ZZZ-AR-MOD-000001.nwc");
            string below = File("Parent", "C06", "Roads", "Lighting", "1104-PAR-1B06BC-ZZZ-EL-MOD-000001.nwc");

            PackageTree tree = PackageTree.Read(Parent, ".nwc");

            IList<Package> packages = tree.Packages();
            Assert.That(packages.Count, Is.EqualTo(2));
            Assert.That(packages[0].RelativePath, Is.EqualTo(Rel("C06", "Roads")));
            Assert.That(packages[0].Files.Count, Is.EqualTo(1));
            Assert.That(packages[0].Files[0], Is.EqualTo(own));
            Assert.That(packages[1].RelativePath, Is.EqualTo(Rel("C06", "Roads", "Lighting")));
            Assert.That(packages[1].Files.Count, Is.EqualTo(1));
            Assert.That(packages[1].Files[0], Is.EqualTo(below));
        }

        [Test]
        public void TheDepthIsNotFixed()
        {
            File("Parent", "C06", "a", "b", "c", "d", "1104-PAR-1B06BC-ZZZ-AR-MOD-000001.nwc");

            PackageTree tree = PackageTree.Read(Parent, ".nwc");

            Assert.That(tree.Packages().Count, Is.EqualTo(1));
            Assert.That(tree.Packages()[0].RelativePath, Is.EqualTo(Rel("C06", "a", "b", "c", "d")));
        }

        [Test]
        public void AnAmpersandInAFolderNameReadsLikeAnyOther()
        {
            File("Parent", "C07", "Roads & Bridges", "1104-PAR-1B06BC-ZZZ-AR-MOD-000001.nwc");

            PackageTree tree = PackageTree.Read(Parent, ".nwc");

            Assert.That(tree.Packages().Count, Is.EqualTo(1));
            Assert.That(tree.Packages()[0].RelativePath, Is.EqualTo(Rel("C07", "Roads & Bridges")));
            Assert.That(tree.Find(Rel("c07", "roads & bridges")), Is.Not.Null, "found again without case");
        }

        [Test]
        public void OnlyFilesOfTheKindCountAndTheExtensionIsReadCaseBlind()
        {
            File("Parent", "C06", "1104-PAR-1B06BC-ZZZ-AR-MOD-000001.NWC");
            File("Parent", "C06", "1104-PAR-1B06BC-ZZZ-AR-MOD-000002.nwc.bak");
            File("Parent", "C06", "1104-PAR-1B06BC-ZZZ-AR-MOD-000003.nwd");
            File("Parent", "C06", "Only other kinds", "notes.txt");

            PackageTree tree = PackageTree.Read(Parent, ".nwc");

            Assert.That(tree.Packages().Count, Is.EqualTo(1));
            Assert.That(tree.Packages()[0].Files.Count, Is.EqualTo(1));
            Assert.That(Path.GetFileName(tree.Packages()[0].Files[0]), Is.EqualTo("1104-PAR-1B06BC-ZZZ-AR-MOD-000001.NWC"));
        }

        [Test]
        public void FilesInTheParentItselfAreOnePackageNamedAfterIt()
        {
            File("Parent", "1104-PAR-1B06BC-ZZZ-AR-MOD-000001.nwc");
            File("Parent", "1104-PAR-1B06BC-ZZZ-ST-MOD-000001.nwc");
            File("Parent", "C06", "1104-PAR-1B06BC-ZZZ-EL-MOD-000001.nwc");

            PackageTree tree = PackageTree.Read(Parent, ".nwc");
            Package itself = tree.Find("Parent");

            Assert.That(tree.Packages().Count, Is.EqualTo(2));
            Assert.That(tree.LeftOut, Is.Empty);
            Assert.That(itself, Is.Not.Null);
            Assert.That(itself.Community, Is.EqualTo("Parent"));
            Assert.That(itself.FullPath, Is.EqualTo(Parent));
            Assert.That(itself.Files.Count, Is.EqualTo(2));
            Assert.That(itself.Name, Is.EqualTo("Parent itself"));
            Assert.That(tree.Communities.Select(c => c.Name).ToArray(), Is.EqualTo(new[] { "C06", "Parent" }), "communities in name order");
        }

        [Test]
        public void AParentHoldingOnlyItsOwnFilesIsOneCommunityOfOnePackage()
        {
            File("Parent", "1104-PAR-1B06BC-ZZZ-AR-MOD-000001.nwc");

            PackageTree tree = PackageTree.Read(Parent, ".nwc");

            Assert.That(tree.Communities.Count, Is.EqualTo(1));
            Assert.That(tree.Packages().Single().RelativePath, Is.EqualTo("Parent"));
        }

        [Test]
        public void TheParentsOwnFilesAreLeftOutWhereAFolderUnderItHasItsName()
        {
            File("Parent", "1104-PAR-1B06BC-ZZZ-AR-MOD-000001.nwc");
            File("Parent", "parent", "1104-PAR-1B06BC-ZZZ-EL-MOD-000001.nwc");

            PackageTree tree = PackageTree.Read(Parent, ".nwc");

            Assert.That(tree.Packages().Count, Is.EqualTo(1));
            Assert.That(tree.Packages()[0].FullPath, Is.EqualTo(Path.Combine(Parent, "parent")));
            Assert.That(tree.LeftOut.Count, Is.EqualTo(1));
            Assert.That(tree.LeftOut[0], Is.EqualTo("1 NWC file sit in the parent folder itself, which would be the package Parent, "
                + "and a folder under it carries that name too, so they are in no package"));
        }

        private string Report
        {
            get { return Path.Combine(root, "Report"); }
        }

        private string Nwf(params string[] parts)
        {
            return File(new[] { "Report" }.Concat(parts).ToArray());
        }

        [Test]
        public void TheNwfTreeIsReadOffTheReportFolderAPackageBeingTheFolderOverItsNwfFolder()
        {
            string a = Nwf("C06", "NWF", "1104-PAR-1B06BC-ZZZ-BM-MOD-000001.nwf");
            string b = Nwf("C06", "NWF", "1104-PAR-1B06BS-ZZZ-BM-MOD-000001.nwf");
            string c = Nwf("C07", "Infrastructure and Landscape", "NWF", "1104-PAR-1C07BC-ZZZ-BM-MOD-000001.nwf");
            Nwf("C06", "NWD", "1104-PAR-1B06BC-ZZZ-BM-MOD-000001.nwd");
            Nwf("C06", "Log", "run.log");

            PackageTree tree = PackageTree.ReadNwfs(Report);

            Assert.That(tree.WasRead, Is.True);
            Assert.That(tree.Extension, Is.EqualTo(".nwf"));
            Assert.That(tree.Packages().Select(p => p.RelativePath).ToArray(),
                Is.EqualTo(new[] { "C06", Rel("C07", "Infrastructure and Landscape") }));
            Assert.That(tree.Find("C06").Files.ToArray(), Is.EqualTo(new[] { a, b }));
            Assert.That(tree.Find(Rel("C07", "Infrastructure and Landscape")).Files.ToArray(), Is.EqualTo(new[] { c }));
            Assert.That(tree.Find("C06").Community, Is.EqualTo("C06"));
            Assert.That(tree.LeftOut, Is.Empty);
        }

        [Test]
        public void TheNwfTreeFindsTheNwfsPackageNwfsFinds()
        {
            Nwf("C06", "Infra", "NWF", "1104-PAR-1B06BC-ZZZ-BM-MOD-000001.nwf");

            Package package = PackageTree.ReadNwfs(Report).Packages().Single();
            IList<PackageNwf> found = PackageNwfs.For(Report, package, NwfPickDisk.Real);

            Assert.That(found.Select(n => n.NwfPath).ToArray(), Is.EqualTo(package.Files.ToArray()));
        }

        [Test]
        public void AnNwdInAnNwfFolderIsNoNwfAndACommunityWithNoNwfIsLeftOutAndSaid()
        {
            Nwf("C06", "NWF", "1104-PAR-1B06BC-ZZZ-BM-MOD-000001.nwf");
            Nwf("C07", "NWF", "1104-PAR-1C07BC-ZZZ-BM-MOD-000001.nwd");

            PackageTree tree = PackageTree.ReadNwfs(Report);

            Assert.That(tree.Packages().Count, Is.EqualTo(1));
            Assert.That(tree.LeftOut.Single(), Is.EqualTo("community C07 holds no NWF file in any folder below it, so it has no package"));
        }

        [Test]
        public void ANwfTreeOfNoReportFolderSaysWhy()
        {
            Assert.That(PackageTree.ReadNwfs("  ").WhyNoTree, Is.EqualTo(PackageWords.PickTheReport));
            Assert.That(PackageTree.ReadNwfs(Report).WhyNoTree, Is.EqualTo("The Report folder is not there: " + Report));
            Assert.That(PackageWords.NwfSummary(null, null), Is.EqualTo(PackageWords.NwfNotReadYet));
        }

        [Test]
        public void ACommunityWithNoFileOfTheKindIsLeftOutAndSaid()
        {
            File("Parent", "C06", "1104-PAR-1B06BC-ZZZ-AR-MOD-000001.nwc");
            File("Parent", "Empty One", "Sub", "readme.txt");

            PackageTree tree = PackageTree.Read(Parent, ".nwc");

            Assert.That(tree.Communities.Count, Is.EqualTo(1));
            Assert.That(tree.LeftOut.Count, Is.EqualTo(1));
            Assert.That(tree.LeftOut[0], Is.EqualTo("community Empty One holds no NWC file in any folder below it, so it has no package"));
        }

        [Test]
        public void ALinkIsNamedAndNeverFollowed()
        {
            File("Parent", "C06", "1104-PAR-1B06BC-ZZZ-AR-MOD-000001.nwc");
            string linked = Path.Combine(Parent, "C06", "Looped");
            File("Parent", "C06", "Looped", "1104-PAR-1B06BC-ZZZ-EL-MOD-000001.nwc");

            PackageTree tree = PackageTree.Read(
                Parent, ".nwc", NwfPickDisk.Real, folder => string.Equals(folder, linked, StringComparison.OrdinalIgnoreCase));

            Assert.That(tree.Packages().Count, Is.EqualTo(1));
            Assert.That(tree.Packages()[0].RelativePath, Is.EqualTo("C06"));
            Assert.That(tree.LeftOut.Single(), Is.EqualTo(Rel("C06", "Looped") + " is a link or a junction and was not followed"));
        }

        [Test]
        public void AFolderThatWillNotReadIsNamedAndTheRestAreRead()
        {
            File("Parent", "C06", "Good", "1104-PAR-1B06BC-ZZZ-AR-MOD-000001.nwc");
            string bad = Path.Combine(Parent, "C06", "Locked");
            File("Parent", "C06", "Locked", "1104-PAR-1B06BC-ZZZ-EL-MOD-000001.nwc");
            NwfPickDisk real = NwfPickDisk.Real;
            NwfPickDisk disk = new NwfPickDisk(
                real.IsFile,
                real.IsFolder,
                folder =>
                {
                    if (string.Equals(folder, bad, StringComparison.OrdinalIgnoreCase))
                    {
                        throw new UnauthorizedAccessException("Access to the path is denied.");
                    }

                    return real.FilesIn(folder);
                },
                real.FoldersIn);

            PackageTree tree = PackageTree.Read(Parent, ".nwc", disk, folder => false);

            Assert.That(tree.Packages().Count, Is.EqualTo(1));
            Assert.That(tree.Packages()[0].RelativePath, Is.EqualTo(Rel("C06", "Good")));
            Assert.That(tree.LeftOut.Single(), Does.StartWith(Rel("C06", "Locked") + " could not be read, Access to the path is denied."));
        }

        [Test]
        public void NoParentAndAParentNotThereReadNoTreeAndSayWhy()
        {
            PackageTree none = PackageTree.Read("  ", ".nwc");
            Assert.That(none.WasRead, Is.False);
            Assert.That(none.WhyNoTree, Is.EqualTo(PackageWords.PickTheParent));

            string gone = Path.Combine(root, "gone");
            PackageTree missing = PackageTree.Read(gone, ".nwc");
            Assert.That(missing.WasRead, Is.False);
            Assert.That(missing.WhyNoTree, Does.Contain(gone));
        }

        [Test]
        public void AParentThatWillNotReadKeepsWhatTheDiskSaidOutOfTheLabel()
        {
            Directory.CreateDirectory(Parent);
            NwfPickDisk real = NwfPickDisk.Real;
            NwfPickDisk disk = new NwfPickDisk(
                real.IsFile,
                real.IsFolder,
                folder => { throw new IOException("The device is not ready."); },
                real.FoldersIn);

            PackageTree tree = PackageTree.Read(Parent, ".nwc", disk, folder => false);

            Assert.That(tree.WasRead, Is.False);
            Assert.That(tree.WhyNoTree, Does.Contain("The device is not ready."));
            Assert.That(tree.WhyNoTreeForALabel, Does.Not.Contain("The device is not ready."));
            Assert.That(PackageWords.Summary(tree, null), Is.EqualTo(PackageWords.TreeWouldNotReadForALabel));
        }

        [Test]
        public void TheLinesNameEveryCommunityAndPackageWithItsPathAndFiles()
        {
            File("Parent", "C06", "Infrastructure and Landscape", "1104-PAR-1B06BC-ZZZ-AR-MOD-000001.nwc");
            File("Parent", "C06", "Infrastructure", "1104-PAR-1B06BC-ZZZ-EL-MOD-000001.nwc");
            File("Parent", "C07", "1104-PAR-1B06BC-ZZZ-ME-MOD-000001.nwc");

            IList<string> lines = PackageTree.Read(Parent, ".nwc").Lines();

            Assert.That(lines, Has.Some.EqualTo("found       : 2 communities, 3 packages, 3 NWC files"));
            Assert.That(lines, Has.Some.EqualTo("community   C06  2 packages, 2 NWC files"));
            // F216, Bader's F-R4: every package line carries its full path as well as its path from the parent.
            Assert.That(lines, Has.Some.EqualTo("   package  " + Rel("C06", "Infrastructure and Landscape") + "  1 NWC file, at "
                + Path.Combine(Parent, "C06", "Infrastructure and Landscape")));
            Assert.That(lines, Has.Some.EqualTo("   package  C07  1 NWC file, at " + Path.Combine(Parent, "C07")));
            Assert.That(lines, Has.Some.EqualTo("left out    : nothing, every file of the kind is in a package"));
        }
    }
}
