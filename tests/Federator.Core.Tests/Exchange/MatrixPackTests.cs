using System;
using System.Collections.Generic;
using System.IO;
using Federator.Core.Clash;
using Federator.Core.Exchange;
using NUnit.Framework;

namespace Federator.Core.Tests
{
    /// <summary>
    /// F154, Bader's decision of 2026-10-13, Q162-4: the priority CSV and the by design CSV are built into the tool
    /// in the matrix pack, read through PriorityMap.Read and ByDesignPairs.Read with no change to either rule. The
    /// packs are found by listing the resource names under one prefix, so no code names a project's file.
    /// </summary>
    [TestFixture]
    public class MatrixPackTests
    {
        private const string Pack = "1104-PAR";

        private static MatrixPack TheBuiltInPack()
        {
            string why;
            MatrixPack pack = MatrixPack.TheOne(MatrixPack.All(), out why);
            Assert.That(pack, Is.Not.Null, why);
            return pack;
        }

        /// <summary>One pack is built in today, named by its folder, and it holds the two CSVs under their own names.</summary>
        [Test]
        public void OnePackIsBuiltInAndItHoldsTheTwoCsvs()
        {
            IList<MatrixPack> packs = MatrixPack.All();

            Assert.That(packs.Count, Is.EqualTo(1));
            Assert.That(packs[0].Name, Is.EqualTo(Pack), "the folder under src\\Federator.Core\\Packs");
            Assert.That(packs[0].Holds(MatrixPack.PriorityFile), Is.True);
            Assert.That(packs[0].Holds(MatrixPack.ByDesignFile), Is.True);
        }

        /// <summary>
        /// The embedded priority file is the sample Bader committed, byte for byte, so the copy in the pack cannot drift from
        /// the file read off the client's matrix. The samples are read and never written.
        /// </summary>
        [Test]
        public void TheEmbeddedPriorityFileIsTheSampleByteForByte()
        {
            MatrixPack pack = TheBuiltInPack();

            Assert.That(pack.ReadText(MatrixPack.PriorityFile),
                Is.EqualTo(File.ReadAllText(Path.Combine(Samples.Folder(), "clash-priority-map.csv"))));
        }

        /// <summary>
        /// F247, Bader's answer Q230 of 20 Oct 2026: the pack's by design pairs are its own file and no longer a copy of the
        /// sample, since he removed the 17 rows of AR against AR and ST against ST and kept the other 38. The sample stays as
        /// the evidence it is, and this holds the pack to it: the pack's rows are the sample's rows in their order, less
        /// exactly the rows whose two sets carry one set code the pack's rules leave out, and nothing else.
        /// </summary>
        [Test]
        public void TheEmbeddedByDesignPairsAreTheSampleLessThePairsOfOneLeftOutCode()
        {
            MatrixPack pack = TheBuiltInPack();
            PackRules rules = pack.Rules();
            List<string> known = new List<string>(MatrixCorrections.ReadPack(pack).Teams.KnownCodes(null));
            List<string> sample = new List<string>(File.ReadAllText(Path.Combine(Samples.Folder(), "by-design-pairs.csv"))
                .Replace("\r\n", "\n").TrimEnd('\n').Split('\n'));
            List<string> expected = new List<string>();
            int removed = 0;

            Assert.That(rules.SameCodeLeftOut, Is.EqualTo(new[] { "AR", "ST" }));

            foreach (string row in sample)
            {
                string[] cells = row.Split(',');
                string left = Federator.Core.Teams.CodeOf.Set(cells[0], known, '-');
                bool leftOut = left.Length > 0 && rules.SameCodeLeftOut.Contains(left)
                    && left == Federator.Core.Teams.CodeOf.Set(cells[1], known, '-');

                if (leftOut)
                {
                    removed++;
                    continue;
                }

                expected.Add(row);
            }

            List<string> embedded = new List<string>(pack.ReadText(MatrixPack.ByDesignFile).Replace("\r\n", "\n").TrimEnd('\n').Split('\n'));

            Assert.That(removed, Is.EqualTo(17), "the 17 rows of AR against AR and ST against ST");
            Assert.That(embedded.Count, Is.EqualTo(expected.Count));

            for (int i = 0; i < expected.Count; i++)
            {
                Assert.That(embedded[i], Is.EqualTo(expected[i]), "row " + (i + 1));
            }
        }

        /// <summary>F247. The pack's rules read, naming AR and ST as the codes whose tests against themselves are left out.</summary>
        [Test]
        public void ThePacksRulesRead()
        {
            PackRules rules = TheBuiltInPack().Rules();

            Assert.That(rules.IsRead, Is.True, rules.Unread);
            Assert.That(rules.SameCodeLeftOut, Is.EqualTo(new[] { "AR", "ST" }));
            Assert.That(rules.Line(), Is.EqualTo("PACK    the pack's rules read off the built in matrix pack 1104-PAR, pack-rules.txt, a test of one"
                + " set code is left out for AR and ST alone, and every test of one set code of another code runs, Bader's answer to Q230"));
        }

        /// <summary>
        /// THE TEST THE BRIEF ASKS FOR: the embedded priority file holds one row per test of the built in matrix, every
        /// test named once with a letter and no row naming a test the matrix does not hold. The matrix is the corrected
        /// matrix in exchange, which F155 builds into the same pack.
        /// </summary>
        [Test]
        public void TheEmbeddedPriorityFileHoldsOneRowPerTestOfTheBuiltInMatrix()
        {
            PriorityMap map = TheBuiltInPack().Priorities();
            ExchangeDocument matrix = MatrixCorrections.ReadPicked(
                Path.Combine(Samples.ExchangeFolder(), "1104-PAR_CLASH_AllInOne_25mm_FIXED.xml"));

            Assert.That(map.Picked, Is.True);
            Assert.That(map.Problems, Is.Empty);
            Assert.That(matrix.Tests.Count, Is.EqualTo(1830));
            Assert.That(map.RowCount, Is.EqualTo(matrix.Tests.Count), "one row per test");

            HashSet<string> names = new HashSet<string>(StringComparer.Ordinal);

            foreach (ClashTestDefinition test in matrix.Tests)
            {
                names.Add(test.Name);
                Assert.That(map.Of(test.Name), Is.Not.EqualTo(ClashPriority.None), test.Name + " has no row");
            }

            Assert.That(names.Count, Is.EqualTo(map.RowCount), "no row names a test the matrix does not hold");
        }

        /// <summary>The by design pairs of the pack read with no problem, the 38 rows Bader kept, Q230.</summary>
        [Test]
        public void TheEmbeddedByDesignPairsRead()
        {
            ByDesignPairs pairs = TheBuiltInPack().ByDesign();

            Assert.That(pairs.Picked, Is.True);
            Assert.That(pairs.Problems, Is.Empty);
            Assert.That(pairs.Count, Is.EqualTo(38));
        }

        /// <summary>
        /// The packs are read off resource names alone. Two folders are two packs, a name outside the prefix or with
        /// no file is no pack, either separator reads, and the one pack a run uses is refused with its reason where
        /// there is none or more than one, since a run chooses by the project code, PackChoice, F166, Q164.
        /// </summary>
        [Test]
        public void PacksAreFoundByTheirResourceNamesAndTwoAreNotChosenBetween()
        {
            IList<MatrixPack> two = MatrixPack.All(new[]
            {
                MatrixPack.Prefix + "\\A\\" + MatrixPack.PriorityFile,
                MatrixPack.Prefix + "/B/" + MatrixPack.ByDesignFile,
                MatrixPack.Prefix + "\\A\\" + MatrixPack.ByDesignFile,
                "Federator.Core.Exchange.revit-categories.txt",
                MatrixPack.Prefix + "\\C",
                MatrixPack.Prefix + "X\\D\\f.csv",
            }, typeof(MatrixPack).Assembly);

            Assert.That(two.Count, Is.EqualTo(2));
            Assert.That(two[0].Name, Is.EqualTo("A"));
            Assert.That(two[0].Files.Count, Is.EqualTo(2));
            Assert.That(two[1].Name, Is.EqualTo("B"));

            string why;
            Assert.That(MatrixPack.TheOne(two, out why), Is.Null);
            Assert.That(why, Is.EqualTo("2 matrix packs are built into this tool, A, B, and a run chooses one by the project code of its NWC files, PackChoice"));
            Assert.That(MatrixPack.TheOne(new List<MatrixPack>(), out why), Is.Null);
            Assert.That(why, Is.EqualTo("no matrix pack is built into this tool"));
            Assert.That(MatrixPack.TheOne(new List<MatrixPack> { two[1] }, out why), Is.SameAs(two[1]));
            Assert.That(why, Is.Null);
        }

        /// <summary>A file the pack does not hold is refused by name, never read as empty.</summary>
        [Test]
        public void AFileThePackDoesNotHoldIsRefused()
        {
            Assert.Throws<FileNotFoundException>(() => TheBuiltInPack().ReadText("not-there.csv"));
        }

        // ---------- F155, the clash matrix built in ----------

        private const string Matrix = "1104-PAR_CLASH_AllInOne_25mm_FIXED";

        /// <summary>
        /// The pack holds the corrected matrix of exchange with its team map and its list of corrections, each byte for
        /// byte the file in exchange, so the copy cannot drift from what the rule proves the exchange file to be.
        /// </summary>
        [Test]
        public void ThePackHoldsTheCorrectedMatrixAndItsTwoListsByteForByte()
        {
            MatrixPack pack = TheBuiltInPack();
            string whyNone;

            Assert.That(pack.Files.Count, Is.EqualTo(7), "the two CSVs, the clash XML, its two lists, the project code and the pack's rules");
            Assert.That(pack.XmlFile(out whyNone), Is.EqualTo(Matrix + ".xml"), whyNone);

            foreach (string file in new[] { Matrix + ".xml", Matrix + ".teams.txt", Matrix + ".corrections.txt" })
            {
                Assert.That(pack.Sha256(file),
                    Is.EqualTo(Federator.Core.Diagnostics.FileFingerprint.Sha256(Path.Combine(Samples.ExchangeFolder(), file))), file);
            }
        }

        /// <summary>
        /// THE SAME 1830 TESTS AS THE RUN OF 2026-10-13. The pack read through ReadPack, the existing ExchangeReader,
        /// TeamMap and MatrixCorrections, gives the sets and the tests the same files on the disk give through
        /// ReadPicked, in the same order and with the same corrections and teams, the lines naming the pack in place of
        /// the path. The document names its pack and its sha256 is the file's.
        /// </summary>
        [Test]
        public void ThePackReadsAsTheSameFilesOnTheDiskRead()
        {
            ExchangeDocument built = MatrixCorrections.ReadPack(TheBuiltInPack());
            ExchangeDocument onDisk = MatrixCorrections.ReadPicked(Path.Combine(Samples.ExchangeFolder(), Matrix + ".xml"));

            Assert.That(built.Tests.Count, Is.EqualTo(1830));
            Assert.That(built.Sets.Count, Is.EqualTo(onDisk.Sets.Count));
            Assert.That(built.Tests.Count, Is.EqualTo(onDisk.Tests.Count));

            for (int i = 0; i < onDisk.Tests.Count; i++)
            {
                Assert.That(built.Tests[i].Name, Is.EqualTo(onDisk.Tests[i].Name), "test " + i);
            }

            for (int i = 0; i < onDisk.Sets.Count; i++)
            {
                Assert.That(built.Sets[i].Path, Is.EqualTo(onDisk.Sets[i].Path), "set " + i);
            }

            Assert.That(built.Corrections.Count, Is.EqualTo(onDisk.Corrections.Count));
            Assert.That(built.Corrections[0], Does.Contain("the built in matrix pack " + Pack));

            for (int i = 1; i < onDisk.Corrections.Count; i++)
            {
                Assert.That(built.Corrections[i], Is.EqualTo(onDisk.Corrections[i]), "correction line " + i);
            }

            Assert.That(built.Teams.IsRead, Is.True);
            Assert.That(built.Teams.Teams.Count, Is.EqualTo(onDisk.Teams.Teams.Count));
            Assert.That(built.Pack.Name, Is.EqualTo(Pack));
            Assert.That(built.SourcePath, Is.EqualTo("the built in matrix pack " + Pack + ", " + Matrix + ".xml"));
            Assert.That(built.Sha256(), Is.EqualTo(onDisk.Sha256()));
        }

        /// <summary>
        /// A pack is read off its resource names, so a pack with no clash XML, or two, has no matrix and says why,
        /// ReadPack refuses it, and TheOneWithAMatrix hands back no pack, so the window and every run read no matrix and
        /// run the tests saved in each NWF, the reviewer's finding on F155.
        /// </summary>
        [Test]
        public void APackWithNoClashXmlOrTwoHasNoMatrixAndSaysWhy()
        {
            IList<MatrixPack> packs = MatrixPack.All(new[]
            {
                MatrixPack.Prefix + "\\None\\" + MatrixPack.PriorityFile,
                MatrixPack.Prefix + "\\Two\\a.xml",
                MatrixPack.Prefix + "\\Two\\b.XML",
            }, typeof(MatrixPack).Assembly);
            string whyNone;

            Assert.That(packs[0].XmlFile(out whyNone), Is.Null);
            Assert.That(whyNone, Is.EqualTo("the matrix pack None holds no clash XML"));
            Assert.That(packs[1].XmlFile(out whyNone), Is.Null);
            Assert.That(whyNone, Does.StartWith("the matrix pack Two holds 2 clash XML files, a.xml, b.XML"));
            Assert.Throws<InvalidDataException>(() => MatrixCorrections.ReadPack(packs[0]));
            Assert.Throws<InvalidDataException>(() => MatrixCorrections.ReadPack(packs[1]));

            Assert.That(MatrixPack.TheOneWithAMatrix(new[] { packs[0] }, out whyNone), Is.Null);
            Assert.That(whyNone, Is.EqualTo("the matrix pack None holds no clash XML"));
            Assert.That(MatrixPack.TheOneWithAMatrix(new[] { packs[1] }, out whyNone), Is.Null);
            Assert.That(whyNone, Does.StartWith("the matrix pack Two holds 2 clash XML files"));
            Assert.That(MatrixPack.TheOneWithAMatrix(packs, out whyNone), Is.Null);
            Assert.That(whyNone, Does.StartWith("2 matrix packs are built into this tool"));
            Assert.That(MatrixPack.TheOneWithAMatrix(new MatrixPack[0], out whyNone), Is.Null);
            Assert.That(whyNone, Is.EqualTo("no matrix pack is built into this tool"));
            Assert.That(MatrixPack.TheOneWithAMatrix(MatrixPack.All(), out whyNone).Name, Is.EqualTo(Pack));
            Assert.That(whyNone, Is.Null);
        }

        /// <summary>
        /// The tests of the pack ask the same questions as the tests of the file on the disk: each side's set, the
        /// test type, the tolerance and its units and merge composites, test by test, so the built in matrix is the
        /// corrected exchange file and not merely as many tests of the same names.
        /// </summary>
        [Test]
        public void EachTestOfThePackAsksWhatTheFileOnTheDiskAsks()
        {
            ExchangeDocument built = MatrixCorrections.ReadPack(TheBuiltInPack());
            ExchangeDocument onDisk = MatrixCorrections.ReadPicked(Path.Combine(Samples.ExchangeFolder(), Matrix + ".xml"));

            for (int i = 0; i < onDisk.Tests.Count; i++)
            {
                ClashTestDefinition a = built.Tests[i];
                ClashTestDefinition b = onDisk.Tests[i];
                string at = "test " + i + ", " + b.Name;

                Assert.That(a.Left.Locator, Is.EqualTo(b.Left.Locator), at);
                Assert.That(a.Right.Locator, Is.EqualTo(b.Right.Locator), at);
                Assert.That(a.TestType, Is.EqualTo(b.TestType), at);

                Assert.That(a.MergeComposites, Is.EqualTo(b.MergeComposites), at);
            }
        }

        /// <summary>
        /// NOTHING IN THE CODE NAMES ANY ONE PROJECT'S FILE, Bader's rule kept by F155: no code line of a .cs or .xaml
        /// file of src names the pack, so neither its folder nor its matrix, which the code finds by the resource prefix
        /// alone. A comment may, a line starting // or the part of a line after " // ", since a comment is not code,
        /// and the pack's own files may, since the folder is the project's data.
        /// </summary>
        [Test]
        public void NoCodeLineOfSrcNamesThePackOrItsMatrix()
        {
            string src = Path.Combine(Samples.Repo(), "src");
            int read = 0;

            foreach (string file in Directory.GetFiles(src, "*.*", SearchOption.AllDirectories))
            {
                bool code = file.EndsWith(".cs", StringComparison.OrdinalIgnoreCase)
                    || file.EndsWith(".xaml", StringComparison.OrdinalIgnoreCase);

                if (!code
                    || file.IndexOf(Path.DirectorySeparatorChar + "obj" + Path.DirectorySeparatorChar, StringComparison.Ordinal) >= 0
                    || file.IndexOf(Path.DirectorySeparatorChar + "bin" + Path.DirectorySeparatorChar, StringComparison.Ordinal) >= 0)
                {
                    continue;
                }

                string[] lines = File.ReadAllLines(file);
                read++;

                for (int i = 0; i < lines.Length; i++)
                {
                    string line = lines[i].Trim();

                    if (line.StartsWith("//", StringComparison.Ordinal))
                    {
                        continue;
                    }

                    int comment = line.IndexOf(" // ", StringComparison.Ordinal);

                    if (comment >= 0)
                    {
                        line = line.Substring(0, comment);
                    }

                    Assert.That(line, Does.Not.Contain(Pack), file + " line " + (i + 1));
                }
            }

            Assert.That(read, Is.GreaterThan(100), "the files of src were read");
        }
    }
}
