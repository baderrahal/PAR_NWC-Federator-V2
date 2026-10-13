using System;
using System.Collections.Generic;
using System.IO;
using Federator.Core.Coverage;
using Federator.Core.Health;
using Federator.Core.Report;
using NUnit.Framework;

namespace Federator.Core.Tests
{
    /// <summary>
    /// The COVERAGE block of one group, F127's add-in half, and the codes it judges a side by. The block is
    /// read off the same CoverageSheetData the Coverage sheet is written from, so the two cannot say two
    /// things of one test. Each test breaks one thing and asserts the block names it.
    /// </summary>
    [TestFixture]
    public class CoverageBlockTests
    {
        private string folder;

        [SetUp]
        public void MakeFolder()
        {
            folder = TempFolder.Make("FederatorCoverageBlock");
        }

        [TearDown]
        public void RemoveFolder()
        {
            TempFolder.Remove(folder);
        }

        private static TestCoverage Ran(int position, string name, int clashes)
        {
            return new TestCoverage(position, name, "a/BLD-AR-Walls", "a/BLD-AR-Floors", 2, 2, TestPresence.CreatedThisRun,
                true, clashes, clashes > 0 ? CoverageReason.HasClashes : CoverageReason.RanAndFoundNone, string.Empty);
        }

        private static TestCoverage KeptOut(int position, string name)
        {
            return new TestCoverage(position, name, "a/BLD-AR-Stairs", "a/BLD-AR-Floors", 0, 2, TestPresence.NotInDocument,
                false, -1, CoverageReason.SideFoundNothing, "left \"a/BLD-AR-Stairs\" 0 items, code AR");
        }

        private static CoverageSheetData Data(IList<TestCoverage> tests, CountCheck check)
        {
            return new CoverageSheetData(@"C:\picked\clash.xml", "ab12", null, null, tests, check, null, null);
        }

        private static string Joined(IList<string> lines)
        {
            return string.Join("\n", new List<string>(lines).ToArray());
        }

        [Test]
        public void TheBlockCountsEveryReasonAndNamesTheFileItsBytesAndTheCounts()
        {
            IList<TestCoverage> tests = new List<TestCoverage> { Ran(1, "T1", 2), Ran(2, "T2", 0), KeptOut(3, "T3") };
            IList<string> lines = CoverageBlock.Lines("100000", Data(tests, null), new CoverageSettings());

            Assert.That(lines[0], Is.EqualTo(@"the tests        : 3 in the clash matrix, C:\picked\clash.xml, sha256 ab12, corrected by nothing"));
            Assert.That(lines[1], Is.EqualTo("counted          : 3 tests in the clash matrix: 2 created this run, 0 already there, 1 not created, 2 run, 1 with clashes, 1 without"));
            Assert.That(lines, Does.Contain("         1  has clashes"));
            Assert.That(lines, Does.Contain("         1  ran and found no clashes"));
            Assert.That(lines, Does.Contain("         1  " + CoverageWords.For(CoverageReason.SideFoundNothing)));
            Assert.That(lines, Does.Contain("    " + CoverageWords.For(CoverageReason.SideFoundNothing) + "  T3  left \"a/BLD-AR-Stairs\" 0 items, code AR"));
            Assert.That(lines[lines.Count - 1], Is.EqualTo(CoverageBlock.KeepsItsResult));
        }

        /// <summary>The saved tests with no clash matrix read are named as that, never as a clash matrix.</summary>
        [Test]
        public void TheSavedTestsAreNamedAsTheSavedTests()
        {
            CoverageSheetData data = new CoverageSheetData(null, null, null, null, new List<TestCoverage> { Ran(1, "T1", 0) }, null, null, null);

            Assert.That(CoverageBlock.Lines("g", data, new CoverageSettings())[0],
                Is.EqualTo("the tests        : 1 the tests saved in the document"));
        }

        /// <summary>The examples stop at the setting and the rest are counted, never dropped in silence.</summary>
        [Test]
        public void TheExamplesStopAtTheSettingAndTheRestAreCounted()
        {
            List<TestCoverage> tests = new List<TestCoverage>();

            for (int i = 1; i <= 4; i++)
            {
                tests.Add(KeptOut(i, "K" + i));
            }

            CoverageSettings settings = new CoverageSettings();
            settings.ExamplesPerReason = 2;
            string block = Joined(CoverageBlock.Lines("g", Data(tests, null), settings));

            Assert.That(block, Does.Contain("  K1  "));
            Assert.That(block, Does.Contain("  K2  "));
            Assert.That(block, Does.Not.Contain("  K3  "));
            Assert.That(block, Does.Contain("    and 2 more of this reason, every one on the Coverage sheet of the internal workbook and in the .tsv"));
        }

        /// <summary>No check taken reads UNKNOWN and never nought FAILED.</summary>
        [Test]
        public void NoCheckReadsUnknownAndNeverNoughtFailed()
        {
            string block = Joined(CoverageBlock.Lines("g", Data(new List<TestCoverage> { Ran(1, "T1", 2) }, null), new CoverageSettings()));

            Assert.That(block, Does.Contain("workbook against Clash Detective : UNKNOWN, the count check was not taken"));
            Assert.That(block, Does.Not.Contain("0 FAILED"));
        }

        private WorkbookTests Workbook(string name, int rows)
        {
            ClashReport report = new ClashReport("g", "1104-PAR-100000-ZZZ-BM-RPT-000001");
            report.DocumentUnits = "m";
            WorkbookTestsTests.AddTest(report, name, rows, 0);
            return WorkbookTestsTests.ReadBack(WorkbookTestsTests.Write(report, folder));
        }

        /// <summary>A count that differs is its COVERAGE FAILED line in the block, the four numbers on it.</summary>
        [Test]
        public void ACountThatDiffersIsAFailedLineInTheBlock()
        {
            IList<TestCoverage> tests = new List<TestCoverage> { Ran(1, "T1", 2) };
            CountCheck check = CountCheck.Judge(
                tests, new List<DocumentTestCount> { new DocumentTestCount("T1", "Tests", 3, 3) }, Workbook("T1", 2), null, -1);
            IList<string> lines = CoverageBlock.Lines("100000", Data(tests, check), new CoverageSettings());
            string block = Joined(lines);

            Assert.That(block, Does.Contain("workbook against Clash Detective : 1 compared, 0 agree, 1 FAILED, 0 not compared, 0 held by neither side"));
            Assert.That(lines, Does.Contain(check.FailedLines("100000")[0]));
            Assert.That(check.FailedLines("100000")[0], Does.StartWith(CountCheck.FailedPrefix));
        }

        /// <summary>The good file: the counts agree and no FAILED line is written.</summary>
        [Test]
        public void CountsThatAgreeWriteNoFailedLine()
        {
            IList<TestCoverage> tests = new List<TestCoverage> { Ran(1, "T1", 2) };
            CountCheck check = CountCheck.Judge(
                tests, new List<DocumentTestCount> { new DocumentTestCount("T1", "Tests", 2, 2) }, Workbook("T1", 2), null, -1);
            string block = Joined(CoverageBlock.Lines("100000", Data(tests, check), new CoverageSettings()));

            Assert.That(block, Does.Contain("workbook against Clash Detective : 1 compared, 1 agree, 0 FAILED, 0 not compared, 0 held by neither side"));
            Assert.That(block, Does.Not.Contain(CountCheck.FailedPrefix));
        }

        /// <summary>A test not compared is named with why, and a test Clash Detective holds that the file does not name is counted.</summary>
        [Test]
        public void ATestNotComparedIsNamedWithWhyAndATestNotInTheFileIsCounted()
        {
            IList<TestCoverage> tests = new List<TestCoverage> { Ran(1, "T1", 2) };
            CountCheck check = CountCheck.Judge(
                tests,
                new List<DocumentTestCount> { new DocumentTestCount("T1", "Tests", 2, 2), new DocumentTestCount("Old", "Tests", 1, 1) },
                null,
                "the clash was skipped",
                -1);
            string block = Joined(CoverageBlock.Lines("g", Data(tests, check), new CoverageSettings()));

            Assert.That(block, Does.Contain("workbook against Clash Detective : UNKNOWN, 0 compared, 0 agree, 0 FAILED, 1 not compared, 0 held by neither side"));
            Assert.That(block, Does.Contain("    not compared  T1  no workbook was read, the clash was skipped"));
            Assert.That(block, Does.Contain("not in the file  : 1 test in Clash Detective that the clash matrix does not name, not compared: Old"));
        }

        [Test]
        public void ACoverageNotTakenSaysWhyAndThatTheGroupKeepsItsResult()
        {
            IList<string> lines = CoverageBlock.NotTakenLines("no clash test was planned for this group");

            Assert.That(lines[0], Is.EqualTo("the tests        : UNKNOWN, no coverage was taken for this group, no clash test was planned for this group"));
            Assert.That(lines[1], Is.EqualTo(CoverageBlock.KeepsItsResult));
            Assert.That(CoverageBlock.NotTakenLines(null)[0], Does.EndWith("and why is UNKNOWN"));
            Assert.That(CoverageBlock.NotTakenLines("taking it threw IOException: a\r\nb")[0], Does.EndWith("taking it threw IOException: a b"),
                "one line whatever the message held");
        }

        /// <summary>Every test gets its .tsv row, the detail on one line whatever it held.</summary>
        [Test]
        public void TheRowTextIsOneLineWithPresenceRunAndReason()
        {
            TestCoverage kept = new TestCoverage(3, "T3", "a", "b", 0, 2, TestPresence.NotInDocument, false, -1,
                CoverageReason.SideFoundNothing, "left\r\nright");

            Assert.That(CoverageBlock.RowText(kept), Is.EqualTo(
                "position 3, not created, not run, " + CoverageWords.For(CoverageReason.SideFoundNothing) + ", left right"));
            Assert.That(CoverageBlock.RowText(Ran(1, "T1", 2)), Is.EqualTo("position 1, created this run, ran, has clashes"));
        }

        // ---------- the codes a side is judged by ----------

        [Test]
        public void TheGroupsCodesAreTheModelsCodesAndNullWhereTheModelsWereNotRead()
        {
            ICollection<string> codes = CoverageRule.GroupCodesOf(new List<ModelExport>
            {
                new ModelExport("a.nwc", "AR", 1, 1, 1, null),
                new ModelExport("b.nwc", "ME", 1, 1, 1, null),
                new ModelExport("c.nwc", string.Empty, 1, 1, 1, null),
                new ModelExport("d.nwc", "AR", 1, 1, 1, null)
            });

            Assert.That(codes, Is.EquivalentTo(new[] { "AR", "ME" }));
            Assert.That(CoverageRule.GroupCodesOf(null), Is.Null);
            Assert.That(CoverageRule.GroupCodesOf(new List<ModelExport>()), Is.Empty, "read and none gave a code, which For reads as not read");
        }

        [Test]
        public void TheRunsCodesAreEveryScannedGroupsAndTheGroupsOwn()
        {
            ICollection<string> codes = CoverageRule.RunCodesOf(
                new List<IEnumerable<string>> { new[] { "AR", "ST" }, new[] { "EL", string.Empty }, null },
                new List<string> { "ME" });

            Assert.That(codes, Is.EquivalentTo(new[] { "AR", "ST", "EL", "ME" }));
            Assert.That(CoverageRule.RunCodesOf(null, new List<string> { "AR" }), Is.EquivalentTo(new[] { "AR" }),
                "the open file run is the one group");
            Assert.That(CoverageRule.RunCodesOf(null, null), Is.Null);
        }
    }
}
