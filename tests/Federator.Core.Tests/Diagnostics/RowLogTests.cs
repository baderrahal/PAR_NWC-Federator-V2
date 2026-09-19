using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Federator.Core.Diagnostics;
using NUnit.Framework;

namespace Federator.Core.Tests
{
    /// <summary>
    /// The machine readable log as it lands on the disk, read back as a file.
    ///
    /// Read off the disk and never off the object that wrote it, because the whole point
    /// of both logs is that what is on the disk is what happened.
    /// </summary>
    [TestFixture]
    public class RowLogTests
    {
        private string folder;

        [SetUp]
        public void MakeFolder()
        {
            folder = TempFolder.Make("FederatorRowLog");
        }

        [TearDown]
        public void RemoveFolder()
        {
            TempFolder.Remove(folder);
        }

        private RunLog Start()
        {
            return RunLog.Start(folder, new DateTime(2026, 9, 19, 9, 14, 22));
        }

        private static IList<string[]> ReadRows(RunLog log)
        {
            List<string[]> rows = new List<string[]>();

            using (FileStream stream = new FileStream(
                       log.RowLogPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            using (StreamReader reader = new StreamReader(stream, Encoding.UTF8))
            {
                string line = reader.ReadLine();

                while (line != null)
                {
                    if (line.Length > 0)
                    {
                        rows.Add(line.Split('\t'));
                    }

                    line = reader.ReadLine();
                }
            }

            return rows;
        }

        [Test]
        public void TheRowFileSitsBesideTheTextLogAndStartsWithTheHeader()
        {
            using (RunLog log = Start())
            {
                Assert.That(log.RowLogPath, Is.Not.Null);
                Assert.That(File.Exists(log.RowLogPath), Is.True);
                Assert.That(
                    Path.GetFileNameWithoutExtension(log.RowLogPath),
                    Is.EqualTo(Path.GetFileNameWithoutExtension(log.Path)));
                Assert.That(Path.GetExtension(log.RowLogPath), Is.EqualTo(EventRow.FileNameExtension));

                IList<string[]> rows = ReadRows(log);

                Assert.That(rows.Count, Is.GreaterThanOrEqualTo(1));
                Assert.That(string.Join("\t", rows[0]), Is.EqualTo(EventRow.Header()));
            }
        }

        [Test]
        public void TheTextLogSaysWhereTheRowFileIs()
        {
            using (RunLog log = Start())
            {
                Assert.That(log.ReadAll(), Does.Contain("ROWS     the machine readable log is"));
                Assert.That(log.ReadAll(), Does.Contain(EventRow.FileNameExtension));
            }
        }

        /// <summary>
        /// One writer, so the two cannot drift. A step writes its line and its row in the
        /// same call, and neither can happen without the other.
        /// </summary>
        [Test]
        public void AStepWritesItsLineAndItsRowTogether()
        {
            using (RunLog log = Start())
            {
                log.GroupStarted("1B06PH", new List<string>());

                using (RunStep step = log.Step(RunSteps.Nwd))
                {
                    step.Changed("published");
                }

                IList<string[]> rows = ReadRows(log);
                string[] started = null;
                string[] finished = null;

                foreach (string[] row in rows)
                {
                    if (row[4] == "step started" && row[5] == RunSteps.Nwd) { started = row; }
                    if (row[4] == "step finished" && row[5] == RunSteps.Nwd) { finished = row; }
                }

                Assert.That(started, Is.Not.Null, "no row for the step starting");
                Assert.That(finished, Is.Not.Null, "no row for the step finishing");
                Assert.That(started[2], Is.EqualTo("1B06PH"));
                Assert.That(finished[7], Is.EqualTo("published"));

                // And the text log said the same thing.
                Assert.That(log.ReadAll(), Does.Contain("STEP     NWD"));
            }
        }

        [Test]
        public void EveryRowHasTheSameEightFieldsTheHeaderHas()
        {
            using (RunLog log = Start())
            {
                log.GroupStarted("1B06PH", new List<string>());

                using (RunStep step = log.Step(RunSteps.TestsRun))
                {
                    step.Changed("a name with a\ttab and a\nnewline in it");
                }

                log.GroupFinished("1B06PH", GroupOutcome.Done, 3.0, null, "First run");

                foreach (string[] row in ReadRows(log))
                {
                    Assert.That(row.Length, Is.EqualTo(EventRow.FieldCount),
                        string.Join(" | ", row));
                }
            }
        }

        [Test]
        public void ARowCarriesTheStepItHappenedInside()
        {
            using (RunLog log = Start())
            {
                log.GroupStarted("1B06PH", new List<string>());

                using (RunStep step = log.Step(RunSteps.Harvest))
                {
                    log.Numbered("anything", "counted", "something", "7", null);
                    step.Changed("done");
                }

                foreach (string[] row in ReadRows(log))
                {
                    if (row[4] == "counted")
                    {
                        Assert.That(row[3], Is.EqualTo(RunSteps.Harvest));
                        Assert.That(row[6], Is.EqualTo("7"));
                        return;
                    }
                }

                Assert.Fail("the row was never written");
            }
        }

        /// <summary>
        /// A sentence with no number in it writes no row. A row whose number column is
        /// empty is noise in a file whose whole purpose is numbers.
        /// </summary>
        [Test]
        public void ASentenceWithNoNumberWritesNoRow()
        {
            using (RunLog log = Start())
            {
                int before = ReadRows(log).Count;
                log.Line("RUN      the Clash step picked nothing");

                Assert.That(ReadRows(log).Count, Is.EqualTo(before));
                Assert.That(log.ReadAll(), Does.Contain("the Clash step picked nothing"));
            }
        }

        /// <summary>
        /// Line by line and flushed, exactly like the text log, so a run that dies mid
        /// group leaves both files whole up to that moment.
        /// </summary>
        [Test]
        public void EveryRowIsOnTheDiskBeforeTheCallReturns()
        {
            using (RunLog log = Start())
            {
                log.Numbered("one", "counted", "first", "1", null);

                Assert.That(ReadRows(log).Count, Is.EqualTo(2), "the header and one row");

                log.Numbered("two", "counted", "second", "2", null);

                Assert.That(ReadRows(log).Count, Is.EqualTo(3));
            }
        }

        /// <summary>
        /// A log that fell back to another folder takes its row file with it. The two
        /// always sit beside each other, wherever they landed.
        /// </summary>
        [Test]
        public void ALogThatFellBackTakesItsRowFileWithIt()
        {
            using (RunLog log = RunLog.StartOrDisabled(null, new DateTime(2026, 9, 19, 9, 14, 22), 5))
            {
                Assert.That(log.IsWritingToDisk, Is.True, "the fallback folder is what this is for");
                Assert.That(() => log.Numbered("a line", "counted", "thing", "1", null), Throws.Nothing);

                Assert.That(
                    Path.GetDirectoryName(log.RowLogPath),
                    Is.EqualTo(Path.GetDirectoryName(log.Path)));
                Assert.That(File.Exists(log.RowLogPath), Is.True);
            }
        }

        /// <summary>
        /// The break. A row file that cannot be opened leaves the text log untouched and
        /// says why in it. The second log is a convenience and the first one is the
        /// record, so nothing here is allowed to stop a run.
        /// </summary>
        [Test]
        public void ARowFileThatCannotBeOpenedSaysSoAndChangesNothingElse()
        {
            // A folder that cannot exist, because a file is already sitting where it
            // would go. No file system makes a folder under a file.
            string underAFile = Path.Combine(folder, "notafolder");
            File.WriteAllText(underAFile, "this is a file");

            RowLog rows = RowLog.StartBeside(Path.Combine(underAFile, "run.log"));

            Assert.That(rows.IsWritingToDisk, Is.False);
            Assert.That(rows.Path, Is.Null);
            Assert.That(rows.WhyNot, Is.Not.Null.And.Not.Empty);
            Assert.That(rows.WhereItIs(), Does.Contain("no machine readable log"));
            Assert.That(rows.WhereItIs(), Does.Contain("The text log is unaffected"));

            Assert.That(
                () => rows.Write(new EventRow("09:00:00.000", 0.0, null, null, "x", null, "1", null)),
                Throws.Nothing);

            rows.Dispose();
        }

        [Test]
        public void TheTimingBlockPutsItsNumbersInTheRowFileToo()
        {
            using (RunLog log = Start())
            {
                log.GroupStarted("1B06PH", new List<string>());

                using (RunStep step = log.Step(RunSteps.Nwd))
                {
                    step.Changed("published");
                }

                log.GroupFinished("1B06PH", GroupOutcome.Done, 10.0, null, "First run");

                bool found = false;

                foreach (string[] row in ReadRows(log))
                {
                    if (row[4] == "timing" && row[5] == RunSteps.Nwd)
                    {
                        found = true;
                    }
                }

                Assert.That(found, Is.True, "the timing block wrote no row");
            }
        }
    }
}
