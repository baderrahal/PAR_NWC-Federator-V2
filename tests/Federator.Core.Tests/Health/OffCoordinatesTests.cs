using System;
using System.Collections.Generic;
using Federator.Core.Health;
using NUnit.Framework;

namespace Federator.Core.Tests.Health
{
    /// <summary>
    /// Bader's answer to Q99 and Q100 on 2026-10-04. A group holding a model not on the same
    /// shared coordinates as its reference model skips its clash and nothing else: its NWF
    /// and NWD are written with every model, set and test, no test is run, and a note says
    /// why, beside the NWD and in the Clash Report folder, with one line per model. The run
    /// writes one list of them for the modellers, and the RESULT block lists the groups.
    /// The numbers are the real 1B06K1 of the C06 run of 2026-10-01, log line 1834.
    /// </summary>
    [TestFixture]
    public class OffCoordinatesTests
    {
        private static string Joined(IList<string> lines)
        {
            return string.Join("\n", new List<string>(lines).ToArray());
        }

        private static OffCoordinates The1B06K1()
        {
            return AlignmentCheck.NotOnTheSameCoordinates(
                new List<ModelPlacement>
                {
                    new ModelPlacement("1104-PAR-1B06K1-ZZZ-AR-MOD-000001.nwc", "AR", "COMMUNITY 4A", 0.0, 0.0, 0.0),
                    new ModelPlacement("1104-PAR-1B06K1-ZZZ-ST-MOD-000001.nwc", "ST", "COMMUNITY 4A",
                        -658144882.33, -2746014844.6, -683828.41)
                },
                AlignmentCheck.DefaultFarModelMillimetres);
        }

        private static OffCoordinates None()
        {
            return AlignmentCheck.NotOnTheSameCoordinates(
                new List<ModelPlacement>
                {
                    new ModelPlacement("a-AR.nwc", "AR", "Site", 0.0, 0.0, 0.0),
                    new ModelPlacement("a-ST.nwc", "ST", "Site", 0.0, 0.0, 10.0)
                },
                AlignmentCheck.DefaultFarModelMillimetres);
        }

        // ---------- the group's reason and its note ----------

        [Test]
        public void TheReasonIsBadersWords()
        {
            Assert.That(OffCoordinates.ClashSkippedReason, Is.EqualTo("clash skipped, models not on the same shared coordinates"));
        }

        /// <summary>
        /// The note goes beside the NWD and into the Clash Report folder, so whoever looks for
        /// the report finds the reason, and it carries the same lines as the log.
        /// </summary>
        [Test]
        public void TheNoteSaysWhatWasDoneWhatWasNotAndCarriesTheSameLinesAsTheLog()
        {
            OffCoordinates off = The1B06K1();
            string note = Joined(off.Note("1B06K1"));

            Assert.That(note, Does.StartWith("1B06K1: clash skipped, models not on the same shared coordinates"));
            Assert.That(note, Does.Contain(
                "The NWF and the NWD of this group hold every model, set and clash test. No clash test was run and no"
                + " clash report or viewpoint was made, because these models are not on the same shared coordinates as"
                + " the reference model AR  1104-PAR-1B06K1-ZZZ-AR-MOD-000001.nwc:"));
            Assert.That(note, Does.Contain("\n   " + off.Models[0]));
            Assert.That(note, Does.Contain(
                "Once these models are exported again on the project's shared coordinates, the next run clashes the"
                + " group with the tests already saved in its NWF."));
        }

        [Test]
        public void TheNoteIsNamedAfterTheFileItSitsBeside()
        {
            Assert.That(OffCoordinates.NoteEnding, Is.EqualTo(" clash skipped.txt"));
            Assert.That(OffCoordinates.ListName, Is.EqualTo("Models not on the same shared coordinates.txt"));
        }

        // ---------- the RESULT block ----------

        [Test]
        public void WithTheRuleOnTheResultListsEachSkippedGroup()
        {
            OffCoordinatesAcrossTheRun run = new OffCoordinatesAcrossTheRun();
            run.SkipsTheClash = true;
            run.Add("1B06K1", The1B06K1());
            run.Add("1B06WL", None());

            string result = Joined(run.ResultLines());

            Assert.That(result, Does.StartWith(
                "clash skipped  : 1 group, models not on the same shared coordinates. Each wrote its NWF and its NWD"
                + " with its clash tests in them and ran none"));
            Assert.That(result, Does.Contain("      1B06K1     1 model(s) not on the same shared coordinates"));
            Assert.That(result, Does.Not.Contain("1B06WL"), "a group whose models are all in place is not listed");
            Assert.That(run.Groups, Is.EqualTo(1));
        }

        [Test]
        public void WithTheRuleOnAndNothingSkippedTheResultSaysNought()
        {
            OffCoordinatesAcrossTheRun run = new OffCoordinatesAcrossTheRun();
            run.SkipsTheClash = true;
            run.Add("1B06WL", None());

            Assert.That(Joined(run.ResultLines()), Is.EqualTo("clash skipped  : 0 groups, models not on the same shared coordinates"));
        }

        /// <summary>
        /// With the rule off the groups are clashed as before, and the RESULT block says the
        /// rule was off, so a run with it off cannot be read as a run where nothing was off.
        /// </summary>
        [Test]
        public void WithTheRuleOffTheResultSaysSoAndStillNamesTheGroups()
        {
            OffCoordinatesAcrossTheRun run = new OffCoordinatesAcrossTheRun();
            run.SkipsTheClash = false;
            run.Add("1B06K1", The1B06K1());

            string result = Joined(run.ResultLines());

            Assert.That(result, Does.StartWith(
                "clash skipped  : none, the rule that skips it was off for this run, so 1 group was clashed with a model"
                + " not on the same shared coordinates"));
            Assert.That(result, Does.Contain("      1B06K1     1 model(s) not on the same shared coordinates"));
        }

        [Test]
        public void ARunTheEngineNeverToldSaysNothingRatherThanGuessing()
        {
            Assert.That(new OffCoordinatesAcrossTheRun().ResultLines(), Is.Empty);
        }

        // ---------- the one list for the run ----------

        /// <summary>One short file for the run, which Bader forwards to the modellers.</summary>
        [Test]
        public void TheListForTheModellersNamesEachGroupItsReferenceAndItsModels()
        {
            OffCoordinatesAcrossTheRun run = new OffCoordinatesAcrossTheRun();
            run.SkipsTheClash = true;
            run.Add("1B06K1", The1B06K1());

            string list = Joined(run.ForModellers(new DateTime(2026, 10, 5, 9, 30, 0)));

            Assert.That(list, Does.StartWith("Models not on the same shared coordinates, from the run started 2026-10-05 09:30"));
            Assert.That(list, Does.Contain(
                "The clash of each group below was skipped. Its NWF and its NWD were written with every model, set and"
                + " clash test in them, and the next run clashes it once these models are exported again on the"
                + " project's shared coordinates."));
            Assert.That(list, Does.Contain("\n1B06K1, measured from the reference model AR  1104-PAR-1B06K1-ZZZ-AR-MOD-000001.nwc\n"));
            Assert.That(list, Does.Contain("\n   " + The1B06K1().Models[0]));
        }

        [Test]
        public void WithTheRuleOffTheListSaysTheGroupsWereClashed()
        {
            OffCoordinatesAcrossTheRun run = new OffCoordinatesAcrossTheRun();
            run.SkipsTheClash = false;
            run.Add("1B06K1", The1B06K1());

            Assert.That(Joined(run.ForModellers(new DateTime(2026, 10, 5, 9, 30, 0))), Does.Contain(
                "The rule that skips the clash was off for this run, so each group below was clashed, and its clashes"
                + " with the other disciplines cannot be trusted until these models are exported again on the"
                + " project's shared coordinates."));
        }

        /// <summary>Written every run, so a list from an earlier run never stands as this run's.</summary>
        [Test]
        public void WithNothingOffTheListSaysSo()
        {
            OffCoordinatesAcrossTheRun run = new OffCoordinatesAcrossTheRun();
            run.SkipsTheClash = true;
            run.Add("1B06WL", None());

            IList<string> list = run.ForModellers(new DateTime(2026, 10, 5, 9, 30, 0));

            Assert.That(list.Count, Is.EqualTo(2));
            Assert.That(list[1], Is.EqualTo("No model in this run was found off its group's shared coordinates, so no group is listed."));
        }
    }
}
