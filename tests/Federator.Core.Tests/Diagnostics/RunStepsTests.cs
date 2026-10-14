using System;
using System.Collections.Generic;
using Federator.Core.Diagnostics;
using NUnit.Framework;

namespace Federator.Core.Tests
{
    /// <summary>
    /// The step list is the only place a step name is written down. If two callers can
    /// spell the same work two ways, one step reads as two in the timing block and
    /// neither total is the truth, which is the whole reason the list exists.
    /// </summary>
    [TestFixture]
    public class RunStepsTests
    {
        [Test]
        public void TheNineteenStepsAreThereInTheOrderAGroupMeetsThem()
        {
            IList<string> all = RunSteps.All;

            Assert.That(all.Count, Is.EqualTo(19));

            // F182, Bader's answer B to Q175: the viewpoints, then the NWF saved and the NWD published,
            // then the pictures and the reports that link them. VIEWS is F85's, built after the clash step
            // and before the NWF is saved the second time. NWD and CONFIRM follow at low memory, because
            // Navisworks's memory grows 3.3 MB for each picture and the NWD publish hung at 26 GB with a 0
            // byte file. PICTURES is F149's, every picture taken once every test has run, each picture's
            // IMAGES visit nesting inside it, then RENUMBER, FR-077, which checks every picture carries its
            // row's number. WORKBOOK, HTML and XML link the jpgs, so they come after. COVERAGE is F127's
            // add-in half and reads the clash workbook back. INTERNAL follows it, Bader's answer B to Q146.
            // Nineteen steps.
            string[] expected =
            {
                "DECIDE", "APPEND", "NWF SAVE", "UNITS", "SETS", "TESTS CREATE", "TESTS RUN",
                "HARVEST", "VIEWS", "NWD", "CONFIRM", "PICTURES", "IMAGES", "RENUMBER",
                "WORKBOOK", "HTML", "XML", "COVERAGE", "INTERNAL"
            };

            for (int i = 0; i < expected.Length; i++)
            {
                Assert.That(all[i], Is.EqualTo(expected[i]), "step " + i);
            }
        }

        [Test]
        public void EveryNameIsDistinct()
        {
            List<string> seen = new List<string>();

            foreach (string name in RunSteps.All)
            {
                Assert.That(seen, Does.Not.Contain(name), name + " is on the list twice");
                seen.Add(name);
            }
        }

        [Test]
        public void EveryConstantIsOnTheList()
        {
            foreach (string name in new[]
            {
                RunSteps.Decide, RunSteps.Append, RunSteps.NwfSave, RunSteps.Units,
                RunSteps.Sets, RunSteps.TestsCreate, RunSteps.TestsRun, RunSteps.Harvest,
                RunSteps.Pictures, RunSteps.Images, RunSteps.Renumber, RunSteps.Views, RunSteps.Workbook, RunSteps.Html,
                RunSteps.Xml, RunSteps.Coverage, RunSteps.InternalWorkbook, RunSteps.Nwd, RunSteps.Confirm
            })
            {
                Assert.That(RunSteps.IsAStep(name), Is.True, name);
            }
        }

        /// <summary>
        /// The break. Anything not on the list is not a step, whatever it looks like.
        /// </summary>
        [Test]
        public void ANameThatIsNotOnTheListIsNotAStep()
        {
            Assert.That(RunSteps.IsAStep("TESTS  RUN"), Is.False);
            Assert.That(RunSteps.IsAStep("tests run"), Is.False);
            Assert.That(RunSteps.IsAStep("CLASH"), Is.False);
            Assert.That(RunSteps.IsAStep(null), Is.False);
            Assert.That(RunSteps.IsAStep(string.Empty), Is.False);
            Assert.That(RunSteps.OrderOf("CLASH"), Is.EqualTo(-1));
        }

        [Test]
        public void TheOrderIsThePositionOnTheList()
        {
            Assert.That(RunSteps.OrderOf(RunSteps.Decide), Is.EqualTo(0));
            Assert.That(RunSteps.OrderOf(RunSteps.InternalWorkbook), Is.EqualTo(18));
            Assert.That(
                RunSteps.OrderOf(RunSteps.Views),
                Is.GreaterThan(RunSteps.OrderOf(RunSteps.Harvest)),
                "the viewpoints are built after the clash step");
            Assert.That(
                RunSteps.OrderOf(RunSteps.Pictures),
                Is.GreaterThan(RunSteps.OrderOf(RunSteps.Harvest)),
                "the pictures are taken once every test's rows are read, F149");
            Assert.That(
                RunSteps.OrderOf(RunSteps.Images),
                Is.GreaterThan(RunSteps.OrderOf(RunSteps.Pictures)),
                "and each picture's visit sits inside the picture pass");
            Assert.That(
                RunSteps.OrderOf(RunSteps.Renumber),
                Is.GreaterThan(RunSteps.OrderOf(RunSteps.Images)),
                "the pictures are renamed once every one has been written");
            Assert.That(
                RunSteps.OrderOf(RunSteps.Renumber),
                Is.LessThan(RunSteps.OrderOf(RunSteps.Workbook)),
                "and before the reports that link them are written");
            Assert.That(
                RunSteps.OrderOf(RunSteps.Views),
                Is.LessThan(RunSteps.OrderOf(RunSteps.Nwd)),
                "the viewpoints are built before the NWD is published");
            Assert.That(
                RunSteps.OrderOf(RunSteps.TestsRun),
                Is.GreaterThan(RunSteps.OrderOf(RunSteps.TestsCreate)));
        }

        /// <summary>
        /// F182, Bader's answer B to Q175. Navisworks's private memory grows with every picture, 26 GB on C04,
        /// and the NWD publish hung there with a 0 byte file. So the NWF is saved and the NWD published BEFORE
        /// the pictures, at low memory, and a picture step that stops or dies leaves both written.
        /// </summary>
        [Test]
        public void TheNwfIsSavedAndTheNwdPublishedBeforeAnyPictureIsTaken()
        {
            int pictures = RunSteps.OrderOf(RunSteps.Pictures);

            Assert.That(RunSteps.OrderOf(RunSteps.NwfSave), Is.LessThan(pictures));
            Assert.That(RunSteps.OrderOf(RunSteps.Views), Is.LessThan(pictures), "the viewpoints need no picture");
            Assert.That(RunSteps.OrderOf(RunSteps.Nwd), Is.LessThan(pictures));
            Assert.That(RunSteps.OrderOf(RunSteps.Confirm), Is.LessThan(pictures), "the NWF is confirmed whole before them too");
            Assert.That(RunSteps.OrderOf(RunSteps.Images), Is.GreaterThan(RunSteps.OrderOf(RunSteps.Nwd)));
        }

        [Test]
        public void EveryReportThatLinksAPictureIsWrittenAfterThePictures()
        {
            int pictures = RunSteps.OrderOf(RunSteps.Pictures);

            Assert.That(RunSteps.OrderOf(RunSteps.Workbook), Is.GreaterThan(pictures), "the Excel links the jpgs");
            Assert.That(RunSteps.OrderOf(RunSteps.Html), Is.GreaterThan(pictures), "the HTML links the jpgs");
            Assert.That(RunSteps.OrderOf(RunSteps.Xml), Is.GreaterThan(pictures));
            Assert.That(RunSteps.OrderOf(RunSteps.Coverage), Is.GreaterThan(RunSteps.OrderOf(RunSteps.Workbook)), "it reads the workbook back");
            Assert.That(RunSteps.OrderOf(RunSteps.InternalWorkbook), Is.GreaterThan(RunSteps.OrderOf(RunSteps.Coverage)));
        }

        /// <summary>
        /// Read off the longest name and never typed, so adding a longer step cannot
        /// leave the block ragged without anything saying so.
        /// </summary>
        [Test]
        public void TheColumnIsAsWideAsTheLongestName()
        {
            int longest = 0;

            foreach (string name in RunSteps.All)
            {
                if (name.Length > longest)
                {
                    longest = name.Length;
                }
            }

            Assert.That(RunSteps.NameWidth, Is.EqualTo(longest));
            Assert.That(RunSteps.NameWidth, Is.EqualTo("TESTS CREATE".Length));

            foreach (string name in RunSteps.All)
            {
                Assert.That(RunSteps.Padded(name).Length, Is.EqualTo(RunSteps.NameWidth), name);
                Assert.That(RunSteps.Padded(name), Does.StartWith(name), name);
            }
        }

        [Test]
        public void TheRefusalNamesTheNameAndEveryStepThereIs()
        {
            string said = RunSteps.NotAStep("CLASH");

            Assert.That(said, Does.Contain("\"CLASH\""));
            Assert.That(said, Does.Contain(RunSteps.Decide));
            Assert.That(said, Does.Contain(RunSteps.Confirm));
        }

        [Test]
        public void TheListHandedOutCannotBeAddedTo()
        {
            IList<string> all = RunSteps.All;

            Assert.That(all.IsReadOnly, Is.True);
            Assert.That(() => all.Add("SOMETHING ELSE"), Throws.TypeOf<NotSupportedException>());
        }
    }
}
