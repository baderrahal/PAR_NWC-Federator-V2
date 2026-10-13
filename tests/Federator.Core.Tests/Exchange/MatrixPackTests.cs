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
        /// The embedded files are the samples Bader committed, byte for byte, so the copy in the pack cannot drift from
        /// the file read off the client's matrix. The samples are read and never written.
        /// </summary>
        [Test]
        public void TheEmbeddedCsvsAreTheSamplesByteForByte()
        {
            MatrixPack pack = TheBuiltInPack();

            Assert.That(pack.ReadText(MatrixPack.PriorityFile),
                Is.EqualTo(File.ReadAllText(Path.Combine(Samples.Folder(), "clash-priority-map.csv"))));
            Assert.That(pack.ReadText(MatrixPack.ByDesignFile),
                Is.EqualTo(File.ReadAllText(Path.Combine(Samples.Folder(), "by-design-pairs.csv"))));
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

        /// <summary>The by design pairs of the pack read with no problem, the 55 rows of the sample.</summary>
        [Test]
        public void TheEmbeddedByDesignPairsRead()
        {
            ByDesignPairs pairs = TheBuiltInPack().ByDesign();

            Assert.That(pairs.Picked, Is.True);
            Assert.That(pairs.Problems, Is.Empty);
            Assert.That(pairs.Count, Is.EqualTo(55));
        }

        /// <summary>
        /// The packs are read off resource names alone. Two folders are two packs, a name outside the prefix or with
        /// no file is no pack, either separator reads, and the one pack a run uses is refused with its reason where
        /// there is none or more than one, since nothing chooses between packs yet, Q164.
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
            Assert.That(why, Is.EqualTo("2 matrix packs are built into this tool, A, B, and nothing chooses between them yet"));
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
    }
}
