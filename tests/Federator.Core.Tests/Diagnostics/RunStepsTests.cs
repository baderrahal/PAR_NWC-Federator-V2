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
        public void TheFifteenStepsAreThereInTheOrderAGroupMeetsThem()
        {
            IList<string> all = RunSteps.All;

            Assert.That(all.Count, Is.EqualTo(15));

            // VIEWS is F85's, between IMAGES and WORKBOOK, because building the viewpoints
            // runs after the clash step and before the second NWF save. It was untimed
            // before, so the seconds it cost came off no total.
            string[] expected =
            {
                "DECIDE", "APPEND", "NWF SAVE", "UNITS", "SETS", "TESTS CREATE", "TESTS RUN",
                "HARVEST", "IMAGES", "VIEWS", "WORKBOOK", "HTML", "XML", "NWD", "CONFIRM"
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
                RunSteps.Images, RunSteps.Views, RunSteps.Workbook, RunSteps.Html, RunSteps.Xml,
                RunSteps.Nwd, RunSteps.Confirm
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
            Assert.That(RunSteps.OrderOf(RunSteps.Confirm), Is.EqualTo(14));
            Assert.That(
                RunSteps.OrderOf(RunSteps.Views),
                Is.GreaterThan(RunSteps.OrderOf(RunSteps.Harvest)),
                "the viewpoints are built after the clash step");
            Assert.That(
                RunSteps.OrderOf(RunSteps.Views),
                Is.LessThan(RunSteps.OrderOf(RunSteps.Nwd)),
                "and before the NWD is published");
            Assert.That(
                RunSteps.OrderOf(RunSteps.TestsRun),
                Is.GreaterThan(RunSteps.OrderOf(RunSteps.TestsCreate)));
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
