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
        public void FilesInTheParentItselfAreLeftOutAndSaid()
        {
            File("Parent", "1104-PAR-1B06BC-ZZZ-AR-MOD-000001.nwc");
            File("Parent", "C06", "1104-PAR-1B06BC-ZZZ-EL-MOD-000001.nwc");

            PackageTree tree = PackageTree.Read(Parent, ".nwc");

            Assert.That(tree.Packages().Count, Is.EqualTo(1));
            Assert.That(tree.LeftOut.Count, Is.EqualTo(1));
            Assert.That(tree.LeftOut[0], Does.Contain("1 NWC file sit in the parent folder itself"));
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
