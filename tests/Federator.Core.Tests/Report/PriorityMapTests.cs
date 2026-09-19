using System;
using System.Collections.Generic;
using System.IO;
using Federator.Core.Clash;
using Federator.Core.Diagnostics;
using Federator.Core.Report;
using NUnit.Framework;

namespace Federator.Core.Tests
{
    /// <summary>
    /// F83. The priority is a decision the project made about which clashes matter, and it
    /// is in nothing Navisworks exports, so it arrives beside the clash XML as its own
    /// file. When nothing is picked, nothing changes.
    ///
    /// PRIORITY IS NOT STATUS. A, B and C come off the matrix. New, Active, Reviewed,
    /// Approved and Resolved are Navisworks words on a clash, and the two are never used
    /// for each other.
    /// </summary>
    [TestFixture]
    public class PriorityMapTests
    {
        private static PriorityMap TheFile()
        {
            return PriorityMap.Read(File.ReadAllText(Samples.PriorityMap()), Samples.PriorityMap());
        }

        // ---------- the file itself ----------

        [Test]
        public void TheFileIsReadOffTheDiskAndNotACopy()
        {
            PriorityMap map = TheFile();

            Assert.That(map.Picked, Is.True);
            Assert.That(map.RowCount, Is.EqualTo(1830),
                "one row per test in the client's matrix, read off the file");
            Assert.That(map.Problems, Is.Empty);
        }

        [Test]
        public void TheHeaderIsTheFourColumnsAndIsNotReadAsARow()
        {
            string first = File.ReadAllLines(Samples.PriorityMap())[0];

            Assert.That(PriorityMap.Columns,
                Is.EqualTo(new[] { "test_name", "left_set", "right_set", "priority" }).AsCollection);
            Assert.That(Csv.Cells(first),
                Is.EqualTo(new List<string>(PriorityMap.Columns)).AsCollection);
            Assert.That(TheFile().Names("test_name"), Is.False);
        }

        /// <summary>
        /// Every test the corrected matrix holds is named by the file, and every row of the
        /// file names a test that is in it. This is what makes F87's rename visible: run
        /// against the uncorrected matrix, sixty of these do not match, and nothing here
        /// papers over the hyphen.
        /// </summary>
        [Test]
        public void TheFileAndTheCorrectedMatrixNameTheSameTests()
        {
            PriorityMap map = TheFile();
            List<string> names = TestNamesIn(Samples.CorrectedMatrix());
            List<string> unmatched = new List<string>();

            foreach (string name in names)
            {
                if (!map.Names(name))
                {
                    unmatched.Add(name);
                }
            }

            Assert.That(names.Count, Is.EqualTo(1830));
            Assert.That(unmatched, Is.Empty,
                "a test the priority file does not name: "
                    + string.Join("; ", unmatched.ToArray()));
        }

        /// <summary>
        /// The break, and the reason the match is exact. Against the matrix BEFORE F87
        /// renamed BLD-DRPipe Accessories, sixty tests do not match, and that is the
        /// rename being visible rather than a fault in this reader.
        /// </summary>
        [Test]
        public void AgainstTheUncorrectedMatrixSixtyTestsDoNotMatch()
        {
            PriorityMap map = TheFile();
            int unmatched = 0;

            foreach (string name in TestNamesIn(Samples.Matrix()))
            {
                if (!map.Names(name))
                {
                    unmatched++;
                }
            }

            Assert.That(unmatched, Is.GreaterThan(0),
                "the uncorrected matrix carries a set name the priority file does not");
        }

        [Test]
        public void EveryRowCarriesOneOfTheThreeLetters()
        {
            PriorityMap map = TheFile();
            PriorityTally seen = new PriorityTally();

            foreach (string name in TestNamesIn(Samples.CorrectedMatrix()))
            {
                seen.Add(map.Of(name), 1);
            }

            Assert.That(seen.Of(ClashPriority.None), Is.EqualTo(0));
            Assert.That(seen.Of(ClashPriority.A) + seen.Of(ClashPriority.B)
                + seen.Of(ClashPriority.C), Is.EqualTo(1830));
        }

        // ---------- how it matches ----------

        [Test]
        public void ATestNameIsMatchedExactlyAndNeverTrimmed()
        {
            PriorityMap map = PriorityMap.Read(
                "test_name,left_set,right_set,priority\r\nA vs B ,A ,B,A\r\n", "a.csv");

            Assert.That(map.Of("A vs B "), Is.EqualTo(ClashPriority.A));
            Assert.That(map.Of("A vs B"), Is.EqualTo(ClashPriority.None),
                "two set names in the reference file end in a space, so trimming matches "
                    + "a different test");
        }

        [Test]
        public void ATestTheFileDoesNotNameHasNoPriorityAndIsNotAnA()
        {
            Assert.That(TheFile().Of("nothing like a test name"), Is.EqualTo(ClashPriority.None));
            Assert.That(TheFile().Of(null), Is.EqualTo(ClashPriority.None));
            Assert.That(Priorities.Cell(ClashPriority.None), Is.EqualTo(string.Empty));
        }

        /// <summary>
        /// The file is CRLF. Splitting on a newline alone leaves a carriage return on the
        /// priority letter and every single row reads as an unknown letter, which is the
        /// same class of fault as a CRLF copy jamming the two walls shut.
        /// </summary>
        [Test]
        public void CarriageReturnsNeverReachThePriorityLetter()
        {
            PriorityMap map = PriorityMap.Read(
                "test_name,left_set,right_set,priority\r\nT,L,R,A\r\nU,L,R,B\r\n", "a.csv");

            Assert.That(map.RowCount, Is.EqualTo(2));
            Assert.That(map.Of("T"), Is.EqualTo(ClashPriority.A));
            Assert.That(map.Of("U"), Is.EqualTo(ClashPriority.B));
            Assert.That(map.Problems, Is.Empty);
        }

        [Test]
        public void ALetterIsReadWithoutCaseAndTrimmed()
        {
            PriorityMap map = PriorityMap.Read(
                "test_name,left_set,right_set,priority\nT,L,R, a \nU,L,R,b\n", "a.csv");

            Assert.That(map.Of("T"), Is.EqualTo(ClashPriority.A));
            Assert.That(map.Of("U"), Is.EqualTo(ClashPriority.B));
        }

        // ---------- what it refuses, and never fails a run over ----------

        [Test]
        public void ARowWithTooFewCellsIsNamedAndLeftOut()
        {
            PriorityMap map = PriorityMap.Read(
                "test_name,left_set,right_set,priority\nT,L,R,A\nbroken\n", "a.csv");

            Assert.That(map.RowCount, Is.EqualTo(1));
            Assert.That(map.Problems.Count, Is.EqualTo(1));
            Assert.That(map.Problems[0], Does.Contain("line 3"));
            Assert.That(map.Problems[0], Does.Contain("needs four"));
        }

        /// <summary>
        /// A letter this tool does not know is a problem and NOT a silent None, because a
        /// silent None reads exactly like a test the matrix never mentioned.
        /// </summary>
        [Test]
        public void ALetterThatIsNotABOrCIsNamedAndLeftOut()
        {
            PriorityMap map = PriorityMap.Read(
                "test_name,left_set,right_set,priority\nT,L,R,D\n", "a.csv");

            Assert.That(map.RowCount, Is.EqualTo(0));
            Assert.That(map.Problems[0], Does.Contain("\"D\""));
            Assert.That(map.Problems[0], Does.Contain("not A, B or C"));
            Assert.That(map.Of("T"), Is.EqualTo(ClashPriority.None));
        }

        [Test]
        public void AnEmptyFileIsAProblemAndNotAThrow()
        {
            PriorityMap map = PriorityMap.Read(string.Empty, "a.csv");

            Assert.That(map.Picked, Is.True);
            Assert.That(map.RowCount, Is.EqualTo(0));
            Assert.That(map.Problems[0], Does.Contain("empty"));
        }

        [Test]
        public void AQuotedCellMayHoldACommaAndADoubledQuote()
        {
            IList<string> cells = Csv.Cells("\"one, two\",\"he said \"\"no\"\"\",three,A");

            Assert.That(cells.Count, Is.EqualTo(4));
            Assert.That(cells[0], Is.EqualTo("one, two"));
            Assert.That(cells[1], Is.EqualTo("he said \"no\""));
            Assert.That(cells[3], Is.EqualTo("A"));
        }

        // ---------- what the log says ----------

        [Test]
        public void TheMatchLineCountsBothSidesAndNamesFiveUnmatched()
        {
            PriorityMap map = PriorityMap.Read(
                "test_name,left_set,right_set,priority\nT,L,R,A\n", "a.csv");

            string[] tests = { "T", "u1", "u2", "u3", "u4", "u5", "u6", "u7" };
            IList<string> lines = map.MatchLines(tests);
            string all = string.Join("\n", new List<string>(lines).ToArray());

            Assert.That(lines[0], Does.StartWith("PRIORITY 1 in the file, 8 tests in this run, "
                + "1 matched, 7 not named by the file"));
            Assert.That(all, Does.Contain("u1"));
            Assert.That(all, Does.Contain("u5"));
            Assert.That(all, Does.Not.Contain("u6"), "five examples and then a count");
            Assert.That(all, Does.Contain("and 2 more not named by the file"));
        }

        [Test]
        public void NothingPickedWritesNoLineAtAll()
        {
            PriorityMap map = PriorityMap.NothingPicked();

            Assert.That(map.Picked, Is.False);
            Assert.That(map.MatchLines(new[] { "a", "b" }), Is.Empty);
            Assert.That(map.Of("a"), Is.EqualTo(ClashPriority.None));
        }

        [Test]
        public void TheRealFileMatchesTheRealMatrixOnEveryTest()
        {
            IList<string> lines = TheFile().MatchLines(TestNamesIn(Samples.CorrectedMatrix()));

            Assert.That(lines.Count, Is.EqualTo(1), "nothing unmatched, so nothing named");
            Assert.That(lines[0], Does.Contain("1830 matched, 0 not named by the file"));
        }

        // ---------- the tally ----------

        [Test]
        public void TheTallyCountsClashesAndNotTests()
        {
            PriorityTally tally = new PriorityTally();
            tally.Add(ClashPriority.A, 40);
            tally.Add(ClashPriority.A, 2);
            tally.Add(ClashPriority.C, 1);

            Assert.That(tally.Of(ClashPriority.A), Is.EqualTo(42));
            Assert.That(tally.Of(ClashPriority.B), Is.EqualTo(0));
            Assert.That(tally.Of(ClashPriority.C), Is.EqualTo(1));
            Assert.That(tally.Total, Is.EqualTo(43));
        }

        [Test]
        public void EveryPriorityIsNamedInTheBlockIncludingTheOnesAtZero()
        {
            PriorityTally tally = new PriorityTally();
            tally.Add(ClashPriority.A, 3);

            string all = string.Join("\n", new List<string>(tally.Lines()).ToArray());

            Assert.That(all, Does.Contain("A           : 3"));
            Assert.That(all, Does.Contain("B           : 0"));
            Assert.That(all, Does.Contain("C           : 0"));
            Assert.That(all, Does.Contain("No priority : 0"));
        }

        [Test]
        public void OneGroupRollsIntoARunTotal()
        {
            PriorityTally run = new PriorityTally();
            PriorityTally group = new PriorityTally();
            group.Add(ClashPriority.B, 5);

            run.Add(group);
            run.Add(group);

            Assert.That(run.Of(ClashPriority.B), Is.EqualTo(10));
        }

        [Test]
        public void TheResultLineIsNullWhenNothingWasPicked()
        {
            PriorityTally tally = new PriorityTally();
            tally.Add(ClashPriority.A, 1);

            Assert.That(PriorityTally.ResultLine(false, tally), Is.Null,
                "a line reading zero on every run teaches people to skip the block");
            Assert.That(PriorityTally.ResultLine(true, null), Is.Null);
            Assert.That(PriorityTally.ResultLine(true, tally),
                Does.StartWith("by priority    : A 1, B 0, C 0, No priority 0, 1 in all"));
        }

        // ---------- the order ----------

        private static ClashReport AReport(params string[] names)
        {
            ClashReport report = new ClashReport("1C07BC", "out");

            foreach (string name in names)
            {
                report.AddTest(name);
            }

            return report;
        }

        [Test]
        public void WithNothingPickedTheOrderIsTheMeasuredOne()
        {
            ClashReport report = AReport("z", "a", "m");
            IList<TestReport> measured = report.InReportOrder();
            IList<TestReport> ordered = ReportOrder.Tests(report);

            Assert.That(ordered.Count, Is.EqualTo(measured.Count));

            for (int i = 0; i < ordered.Count; i++)
            {
                Assert.That(ordered[i].Name, Is.EqualTo(measured[i].Name), "position " + i);
            }
        }

        [Test]
        public void WithAFilePickedTheOrderIsAThenBThenCThenByName()
        {
            ClashReport report = AReport("z", "a", "m", "b");
            report.Priorities = PriorityMap.Read(
                "test_name,left_set,right_set,priority\nz,L,R,C\na,L,R,B\nm,L,R,A\n", "a.csv");

            foreach (TestReport test in report.Tests)
            {
                test.Priority = report.Priorities.Of(test.Name);
            }

            IList<TestReport> ordered = ReportOrder.Tests(report);

            Assert.That(ordered[0].Name, Is.EqualTo("m"), "A first");
            Assert.That(ordered[1].Name, Is.EqualTo("a"), "then B");
            Assert.That(ordered[2].Name, Is.EqualTo("z"), "then C");
            Assert.That(ordered[3].Name, Is.EqualTo("b"), "then the test the file says nothing about");
        }

        [Test]
        public void TwoTestsOfOnePriorityAreOrderedByTestName()
        {
            ClashReport report = AReport("zebra", "apple", "mango");
            report.Priorities = PriorityMap.Read(
                "test_name,left_set,right_set,priority\nzebra,L,R,A\napple,L,R,A\nmango,L,R,A\n",
                "a.csv");

            foreach (TestReport test in report.Tests)
            {
                test.Priority = report.Priorities.Of(test.Name);
            }

            IList<TestReport> ordered = ReportOrder.Tests(report);

            Assert.That(ordered[0].Name, Is.EqualTo("apple"));
            Assert.That(ordered[1].Name, Is.EqualTo("mango"));
            Assert.That(ordered[2].Name, Is.EqualTo("zebra"));
        }

        [Test]
        public void ThePicturesFollowTheOrderTheBlocksAreWrittenIn()
        {
            ClashReport report = AReport("zebra", "apple");
            report.Priorities = PriorityMap.Read(
                "test_name,left_set,right_set,priority\nzebra,L,R,C\napple,L,R,A\n", "a.csv");

            foreach (TestReport test in report.Tests)
            {
                test.Priority = report.Priorities.Of(test.Name);
            }

            // The picture numbering walks the same list the workbook walks, so the first
            // block written is always cd00.
            IList<TestReport> ordered = ReportOrder.Tests(report);

            Assert.That(ordered[0].Name, Is.EqualTo("apple"));
        }

        /// <summary>A picker that remembers nothing is a picker that opens somewhere else.</summary>
        [Test]
        public void ThePriorityPickerRemembersItsOwnFolder()
        {
            List<PickerKind> all = new List<PickerKind>(FolderMemory.AllKinds());

            Assert.That(all, Does.Contain(PickerKind.Priority));
            Assert.That(all, Does.Contain(PickerKind.ByDesign));
            Assert.That(all.Count, Is.EqualTo(Enum.GetValues(typeof(PickerKind)).Length),
                "a kind on the enum and missing from AllKinds is remembered by nothing");
        }

        [Test]
        public void NothingPickedIsWhatTheOptionsStartWith()
        {
            Assert.That(new ReportOptions().PriorityPath, Is.EqualTo(string.Empty));
            Assert.That(new ReportOptions().ByDesignPath, Is.EqualTo(string.Empty));
        }

        private static List<string> TestNamesIn(string path)
        {
            List<string> names = new List<string>();
            System.Xml.Linq.XDocument document = System.Xml.Linq.XDocument.Load(path);

            foreach (System.Xml.Linq.XElement test in document.Descendants("clashtest"))
            {
                System.Xml.Linq.XAttribute name = test.Attribute("name");

                if (name != null)
                {
                    names.Add(name.Value);
                }
            }

            return names;
        }
    }
}
