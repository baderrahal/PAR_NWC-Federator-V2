using System;
using System.Collections.Generic;
using Federator.Core.Health;
using NUnit.Framework;

namespace Federator.Core.Tests.Health
{
    /// <summary>
    /// Bader's answer to Q99 and Q100 on 2026-10-04. A group holding a model not on the same
    /// shared coordinates as its reference model skips its clash and nothing else, where the
    /// run would have run a clash test in it. A note says why, beside the NWD and in the
    /// Clash Report folder, with one line per model, and says only what was checked. Each run
    /// writes its own list of them for the modellers, and the RESULT block lists the groups
    /// and how many the rule judged. The numbers are the real 1B06K1 of the C06 run of
    /// 2026-10-01, log line 1834.
    /// </summary>
    [TestFixture]
    public class OffCoordinatesTests
    {
        private static readonly DateTime Started = new DateTime(2026, 10, 5, 9, 30, 0);

        private static string Joined(IList<string> lines)
        {
            return string.Join("\n", new List<string>(lines).ToArray());
        }

        private static int Times(string text, string part)
        {
            int count = 0;
            int at = text.IndexOf(part, StringComparison.Ordinal);

            while (at >= 0)
            {
                count++;
                at = text.IndexOf(part, at + part.Length, StringComparison.Ordinal);
            }

            return count;
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

        /// <summary>A group whose placements could not be read, every one of them NaN, its sites read.</summary>
        private static OffCoordinates NotPlaced()
        {
            return AlignmentCheck.NotOnTheSameCoordinates(
                new List<ModelPlacement>
                {
                    new ModelPlacement("b-AR.nwc", "AR", "Site", ModelPlacement.NotRead, ModelPlacement.NotRead, ModelPlacement.NotRead),
                    new ModelPlacement("b-ST.nwc", "ST", "Site", ModelPlacement.NotRead, ModelPlacement.NotRead, ModelPlacement.NotRead)
                },
                AlignmentCheck.DefaultFarModelMillimetres);
        }

        /// <summary>A group whose ST sits in place but whose site read threw, so it could be on Internal.</summary>
        private static OffCoordinates OneSiteNotRead()
        {
            return AlignmentCheck.NotOnTheSameCoordinates(
                new List<ModelPlacement>
                {
                    new ModelPlacement("c-AR.nwc", "AR", "Site", 0.0, 0.0, 0.0),
                    new ModelPlacement("c-ST.nwc", "ST", ModelPlacement.SiteNotRead, 0.0, 0.0, 10.0)
                },
                AlignmentCheck.DefaultFarModelMillimetres);
        }

        private const bool RuleOn = true;
        private const bool RuleOff = false;
        private const bool ATestRuns = true;
        private const bool NothingToRun = false;

        /// <summary>The real 1A02MM's ST of 5q, on Internal and 255 mm above the AR, under a metre.</summary>
        private static OffCoordinates OnInternal()
        {
            return AlignmentCheck.NotOnTheSameCoordinates(
                new List<ModelPlacement>
                {
                    new ModelPlacement("1104-PAR-1A02MM-ZZZ-AR-MOD-000001.nwc", "AR", "SWLS-02-SharedCoordinate", 0.0, -13419.0, -509.0),
                    new ModelPlacement("1104-PAR-1A02MM-ZZZ-ST-MOD-000001.nwc", "ST", "Internal", 0.0, -13419.0, -254.0)
                },
                AlignmentCheck.DefaultFarModelMillimetres);
        }

        private static NwfAndNwd BothOnDisk()
        {
            return new NwfAndNwd(@"C:\out\1B06K1.nwf", true, @"C:\out\1B06K1.nwd", true, true);
        }

        // ---------- the group's reason and its note ----------

        [Test]
        public void TheReasonIsBadersWords()
        {
            Assert.That(OffCoordinates.ClashSkippedReason, Is.EqualTo("clash skipped, models not on the same shared coordinates"));
        }

        /// <summary>
        /// The note goes beside the NWD and into the Clash Report folder, so whoever looks for
        /// the report finds the reason, and it carries the same lines as the log. It says only
        /// what was checked: the tests in the words of the CLASH line, F77, and the NWF and the
        /// NWD off the disk. It once said both held every model, set and clash test, which F77
        /// makes false whenever a side finds nothing.
        /// </summary>
        [Test]
        public void TheNoteSaysOnlyWhatWasChecked()
        {
            OffCoordinates off = The1B06K1();
            string note = Joined(off.Note("1B06K1", BothOnDisk(), 0, new List<string>()));

            Assert.That(note, Does.StartWith("1B06K1: clash skipped, models not on the same shared coordinates"));
            Assert.That(note, Does.Contain(
                "No clash test was run and no clash report or viewpoint was made, because these models are not on the"
                + " same shared coordinates, measured from the reference model AR  1104-PAR-1B06K1-ZZZ-AR-MOD-000001.nwc:"));
            Assert.That(note, Does.Contain("\n   " + off.Models[0]));
            Assert.That(note, Does.Contain("The tests whose sides both find something are created and none is run."));
            Assert.That(note, Does.Contain(@"The NWF is on disk at C:\out\1B06K1.nwf, read after the NWD was published."));
            Assert.That(note, Does.Contain(@"The NWD was published by this run at C:\out\1B06K1.nwd."));
            Assert.That(note, Does.Not.Contain("every model, set and clash test"));
            Assert.That(note, Does.Contain(
                "Once these models are exported again on the project's shared coordinates, the next run clashes the"
                + " group with the tests already saved in its NWF."));
        }

        /// <summary>A FAILED group's note says which file was not written, read off the disk and never assumed.</summary>
        [Test]
        public void AFailedGroupsNoteSaysWhichFileWasNotWritten()
        {
            OffCoordinates off = The1B06K1();

            string noNwd = Joined(off.Note(
                "1B06K1", new NwfAndNwd(@"C:\out\1B06K1.nwf", true, @"C:\out\1B06K1.nwd", false, false), 0, new List<string>()));
            Assert.That(noNwd, Does.Contain(@"The NWD was NOT written. It is not on disk at C:\out\1B06K1.nwd."));
            Assert.That(noNwd, Does.Not.Contain("published by this run"));

            string noNwf = Joined(off.Note(
                "1B06K1", new NwfAndNwd(@"C:\out\1B06K1.nwf", false, @"C:\out\1B06K1.nwd", true, true), 0, new List<string>()));
            Assert.That(noNwf, Does.Contain(@"The NWF was NOT written. It is not on disk at C:\out\1B06K1.nwf."));
            Assert.That(noNwf, Does.Not.Contain("The NWF is on disk"));

            string stale = Joined(off.Note(
                "1B06K1", new NwfAndNwd(@"C:\out\1B06K1.nwf", true, @"C:\out\1B06K1.nwd", true, false), 0, new List<string>()));
            Assert.That(stale, Does.Contain(
                @"The NWD was NOT written by this run. The file at C:\out\1B06K1.nwd is from an earlier run, because the"
                + " publish did not report success."));
        }

        /// <summary>
        /// The tests already saved in the NWF keep their results, F72c, and a skipped group
        /// runs none of them, so the panel in the NWD shows an earlier run's clashes. The note
        /// says so whichever way the count went.
        /// </summary>
        [Test]
        public void TheNoteSaysTheTestsAlreadyInTheNwfKeepTheResultsOfAnEarlierRun()
        {
            OffCoordinates off = The1B06K1();
            const string Keeps = "Any clash test already saved in the NWF keeps the results of an earlier run, because this run ran none.";

            string weekly = Joined(off.Note("1B06K1", BothOnDisk(), 40, new List<string>()));
            Assert.That(weekly, Does.Contain(Keeps + " 40 of this run's tests were already there."));

            string first = Joined(off.Note("1B06K1", BothOnDisk(), 0, new List<string>()));
            Assert.That(first, Does.Contain(Keeps));
            Assert.That(first, Does.Not.Contain("were already there"));

            string unknown = Joined(off.Note("1B06K1", BothOnDisk(), -1, new List<string>()));
            Assert.That(unknown, Does.Contain(Keeps + " How many of this run's tests were already there is UNKNOWN."));
        }

        /// <summary>
        /// Words and never deletion. A workbook, a page or an XML an earlier run wrote under
        /// the name this run would have written stays where it is and reads as current, so
        /// the note names each one the engine found, and says when none was found.
        /// </summary>
        [Test]
        public void TheNoteNamesTheReportsOfAnEarlierRunThatStillStand()
        {
            OffCoordinates off = The1B06K1();
            string found = @"   C:\out\Clash Reports\1B06K1.xlsx  last written 2026-09-28 14:02, 1,234 bytes";

            string with = Joined(off.Note("1B06K1", BothOnDisk(), 0, new List<string> { found }));
            Assert.That(with, Does.Contain(
                "These report files are at the names this run would have written. Each is from an earlier run and was"
                + " not written by this run:\n" + found));

            string without = Joined(off.Note("1B06K1", BothOnDisk(), 0, new List<string>()));
            Assert.That(without, Does.Contain(
                "No report file is at the names this run would have written, so no report of an earlier run stands"
                + " beside this note."));
        }

        [Test]
        public void TheNoteIsNamedAfterTheFileItSitsBeside()
        {
            Assert.That(OffCoordinates.NoteEnding, Is.EqualTo(" clash skipped.txt"));
        }

        // ---------- which group skips its clash ----------

        /// <summary>
        /// The skip applies only where this run would have run a clash test. A group with
        /// nothing to clash, no XML and no test saved, an XML of sets alone, or one
        /// discipline, is judged as before, the breaker's second finding at c5d8aa8.
        /// </summary>
        [Test]
        public void TheClashIsSkippedOnlyWhereTheRuleIsOnATestWouldRunAndAModelIsOff()
        {
            Assert.That(The1B06K1().SkipsTheClash(true, true), Is.True);
            Assert.That(The1B06K1().SkipsTheClash(true, false), Is.False, "nothing to clash, so nothing is skipped");
            Assert.That(The1B06K1().SkipsTheClash(false, true), Is.False, "the rule is off");
            Assert.That(None().SkipsTheClash(true, true), Is.False, "every model in place");
        }

        // ---------- the note of an earlier run ----------

        /// <summary>
        /// An earlier run's note goes only when this run judged every model of the group and
        /// did not skip its clash. A model whose placement or site is UNKNOWN, or a group with
        /// no model read, keeps it, because an unknown is not a pass. Null is the note going,
        /// and anything else is why it stays.
        /// </summary>
        [Test]
        public void AnEarlierNoteGoesOnlyWhenEveryModelWasJudgedAndTheClashWasNotSkipped()
        {
            Assert.That(OffCoordinates.EarlierNoteKeptBecause(None(), RuleOn, ATestRuns), Is.Null);
            Assert.That(OffCoordinates.EarlierNoteKeptBecause(The1B06K1(), RuleOn, ATestRuns), Is.Not.Null, "this run skipped the clash");
            Assert.That(OffCoordinates.EarlierNoteKeptBecause(The1B06K1(), RuleOff, ATestRuns), Is.Null, "the rule was off and every model was judged");
            Assert.That(OffCoordinates.EarlierNoteKeptBecause(NotPlaced(), RuleOn, ATestRuns), Is.Not.Null, "no placement read");
            Assert.That(OffCoordinates.EarlierNoteKeptBecause(OneSiteNotRead(), RuleOn, ATestRuns), Is.Not.Null, "a site not read could be Internal");
            Assert.That(
                OffCoordinates.EarlierNoteKeptBecause(
                    AlignmentCheck.NotOnTheSameCoordinates(new List<ModelPlacement>(), AlignmentCheck.DefaultFarModelMillimetres),
                    RuleOn,
                    ATestRuns),
                Is.Not.Null,
                "no model read at all");
            Assert.That(
                OffCoordinates.EarlierNoteKeptBecause(null, RuleOn, ATestRuns),
                Is.EqualTo("this run did not judge every model of the group, so the note may still be true"),
                "the read threw");
        }

        /// <summary>
        /// F112 attempt 3. A group whose model is still on Internal, or still far, and in which
        /// this run ran no clash test, keeps the note an earlier run left, because what it says
        /// is still so. Attempt 2 removed it whenever every model was judged and the clash was
        /// not skipped, which deleted a true note in a group with nothing to clash.
        /// </summary>
        [Test]
        public void AnEarlierNoteStaysWhileAModelIsStillOffAndNoClashTestRan()
        {
            const string StillOff = "a model of the group is still not on the same shared coordinates and this run ran"
                + " no clash test in it, so the note may still be true";

            foreach (bool rule in new[] { RuleOn, RuleOff })
            {
                Assert.That(OffCoordinates.EarlierNoteKeptBecause(OnInternal(), rule, NothingToRun), Is.EqualTo(StillOff), "rule " + rule);
                Assert.That(OffCoordinates.EarlierNoteKeptBecause(The1B06K1(), rule, NothingToRun), Is.EqualTo(StillOff), "rule " + rule);
                Assert.That(OffCoordinates.EarlierNoteKeptBecause(None(), rule, NothingToRun), Is.Null, "every model in place, rule " + rule);
            }

            Assert.That(OffCoordinates.EarlierNoteKeptBecause(OnInternal(), RuleOff, ATestRuns), Is.Null, "the rule was off and the group was clashed");
        }

        // ---------- the RESULT block ----------

        [Test]
        public void WithTheRuleOnTheResultListsEachSkippedGroup()
        {
            OffCoordinatesAcrossTheRun run = new OffCoordinatesAcrossTheRun(true, Started, 2);
            run.Add("1B06K1", The1B06K1(), true);
            run.Add("1B06WL", None(), true);

            string result = Joined(run.ResultLines());

            Assert.That(result, Does.StartWith(
                "clash skipped  : 1 group, models not on the same shared coordinates. In each the tests whose sides"
                + " both find something are created and none is run"));
            Assert.That(result, Does.Contain("coordinates    : 2 of 2 groups judged"));
            Assert.That(result, Does.Contain("      1B06K1     1 model(s) not on the same shared coordinates, clash skipped"));
            Assert.That(result, Does.Not.Contain("1B06WL"), "a group whose models are all in place is not listed");
            Assert.That(result, Does.Not.Contain("wrote its NWF"), "the files written list says what was written");
        }

        [Test]
        public void WithTheRuleOnAndNothingOffTheResultSaysNoughtAndHowManyWereJudged()
        {
            OffCoordinatesAcrossTheRun run = new OffCoordinatesAcrossTheRun(true, Started, 1);
            run.Add("1B06WL", None(), true);

            Assert.That(Joined(run.ResultLines()), Is.EqualTo(
                "clash skipped  : 0 groups, models not on the same shared coordinates\n"
                + "coordinates    : 1 of 1 group judged"));
        }

        /// <summary>
        /// With the rule off the groups are clashed as before, and the RESULT block says the
        /// rule was off, so a run with it off cannot be read as a run where nothing was off.
        /// </summary>
        [Test]
        public void WithTheRuleOffTheResultSaysSoAndStillNamesTheGroups()
        {
            OffCoordinatesAcrossTheRun run = new OffCoordinatesAcrossTheRun(false, Started, 1);
            run.Add("1B06K1", The1B06K1(), true);

            string result = Joined(run.ResultLines());

            Assert.That(result, Does.StartWith(
                "clash skipped  : none, the rule that skips it was off for this run, so 1 group was clashed with a model"
                + " not on the same shared coordinates"));
            Assert.That(result, Does.Contain("      1B06K1     1 model(s) not on the same shared coordinates, clashed, the rule was off"));
        }

        /// <summary>
        /// The breaker's first finding at c5d8aa8. One tally lived on the window's log, its
        /// rule state overwritten each run and its groups never cleared, so the second run of
        /// a window, with the box unticked, listed 1A02MM twice and called both clashed. Each
        /// run now has its own, made when the run starts with the rule state of that run.
        /// </summary>
        [Test]
        public void EachRunKeepsItsOwnGroupsAndItsOwnRuleState()
        {
            OffCoordinatesAcrossTheRun first = new OffCoordinatesAcrossTheRun(true, Started, 1);
            first.Add("1A02MM", The1B06K1(), true);

            OffCoordinatesAcrossTheRun second = new OffCoordinatesAcrossTheRun(false, Started.AddHours(1), 1);
            second.Add("1A02MM", The1B06K1(), true);

            string firstResult = Joined(first.ResultLines());
            string secondResult = Joined(second.ResultLines());

            Assert.That(firstResult, Does.StartWith("clash skipped  : 1 group,"));
            Assert.That(secondResult, Does.StartWith("clash skipped  : none, the rule that skips it was off for this run, so 1 group was clashed"));
            Assert.That(Times(secondResult, "1A02MM"), Is.EqualTo(1));
            Assert.That(Joined(second.ForModellers()), Does.StartWith(
                "Models not on the same shared coordinates, from the run started 2026-10-05 10:30"));
            Assert.That(Times(Joined(second.ForModellers()), "\n1A02MM"), Is.EqualTo(1));
        }

        /// <summary>
        /// One fixed list name meant a smaller run, or the open file run of one group, wrote
        /// over the list of the full run with its own few groups. Each run's list is now its
        /// own file, named for the second its run started, so every run's list is kept.
        /// </summary>
        [Test]
        public void EachRunsListIsItsOwnFileNamedForItsStart()
        {
            OffCoordinatesAcrossTheRun full = new OffCoordinatesAcrossTheRun(true, new DateTime(2026, 10, 5, 9, 30, 0), 46);
            OffCoordinatesAcrossTheRun one = new OffCoordinatesAcrossTheRun(true, new DateTime(2026, 10, 5, 11, 2, 7), 1);

            Assert.That(full.ListName, Is.EqualTo("Models not on the same shared coordinates, run 2026-10-05 093000.txt"));
            Assert.That(one.ListName, Is.EqualTo("Models not on the same shared coordinates, run 2026-10-05 110207.txt"));
        }

        // ---------- a zero that reads as clean ----------

        /// <summary>
        /// The breaker's fourth finding at c5d8aa8: a run stopped after 3 groups of 46 said 0
        /// groups and no model found off, which reads as 46 clean groups.
        /// </summary>
        [Test]
        public void ARunStoppedAfterThreeGroupsNeverReadsAsClean()
        {
            OffCoordinatesAcrossTheRun run = new OffCoordinatesAcrossTheRun(true, Started, 46);
            run.Add("1B06WL", None(), true);
            run.Add("1B06WM", None(), true);
            run.Add("1B06WO", None(), true);

            Assert.That(Joined(run.ResultLines()), Does.Contain(
                "coordinates    : 3 of 46 groups judged, 43 not reached, the run stopped or the group ended before its"
                + " models were read"));

            string list = Joined(run.ForModellers());
            Assert.That(list, Does.Not.Contain("No model in this run was found off"));
            Assert.That(list, Does.Contain(
                "The rule judged 3 of 46 groups of this run. 43 were not reached, the run stopped or the group ended"
                + " before its models were read."));
            Assert.That(list, Does.Contain("No group is listed, and that is not a clean bill for the groups and models above that were not judged."));
        }

        [Test]
        public void AGroupWhosePlacementsCannotBeReadIsCountedAndNeverReadsAsClean()
        {
            OffCoordinatesAcrossTheRun run = new OffCoordinatesAcrossTheRun(true, Started, 1);
            run.Add("1A04PK", NotPlaced(), true);

            Assert.That(Joined(run.ResultLines()), Does.Contain(
                "coordinates    : 1 of 1 group judged, 2 model(s) in the groups judged not judged, a placement or a site UNKNOWN"));

            string list = Joined(run.ForModellers());
            Assert.That(list, Does.Not.Contain("No model in this run was found off"));
            Assert.That(list, Does.Contain(
                "2 model(s) in the groups judged could not be judged, a placement or a site UNKNOWN, and are not listed."));
        }

        [Test]
        public void AGroupWhoseAlignmentReadThrewIsNotJudged()
        {
            OffCoordinatesAcrossTheRun run = new OffCoordinatesAcrossTheRun(true, Started, 1);
            run.Add("1A04PK", null, true);

            Assert.That(Joined(run.ResultLines()), Does.Contain(
                "coordinates    : 0 of 1 group judged, 1 whose models could not be read"));
            Assert.That(Joined(run.ForModellers()), Does.Not.Contain("No model in this run was found off"));
        }

        /// <summary>The one list that may say nothing was found: every group judged and every model judged.</summary>
        [Test]
        public void EveryGroupAndModelJudgedAndNothingOffIsTheOneCleanList()
        {
            OffCoordinatesAcrossTheRun run = new OffCoordinatesAcrossTheRun(true, Started, 2);
            run.Add("1B06WL", None(), true);
            run.Add("1B06WM", None(), true);

            IList<string> list = run.ForModellers();

            Assert.That(list[list.Count - 1], Is.EqualTo(
                "No model in this run was found off its group's shared coordinates, so no group is listed."));
        }

        // ---------- a group with nothing to clash ----------

        /// <summary>
        /// The breaker's second finding at c5d8aa8: a group where no clash test would run is
        /// judged as before. It is named with what was true of it and never called skipped.
        /// </summary>
        [Test]
        public void AGroupWithNothingToClashIsNotSkippedAndSaysSo()
        {
            OffCoordinatesAcrossTheRun run = new OffCoordinatesAcrossTheRun(true, Started, 1);
            run.Add("1A02MM", The1B06K1(), false);

            string result = Joined(run.ResultLines());
            Assert.That(result, Does.StartWith("clash skipped  : 0 groups, models not on the same shared coordinates"));
            Assert.That(result, Does.Contain(
                "      1A02MM     1 model(s) not on the same shared coordinates, no clash test to run, so nothing was skipped"));

            string list = Joined(run.ForModellers());
            Assert.That(list, Does.Contain("   no clash test was to run in it this run, so nothing was skipped."));
            Assert.That(list, Does.Not.Contain("its clash was skipped"));
        }

        // ---------- the one list for the run ----------

        /// <summary>One short file for the run, which Bader forwards to the modellers.</summary>
        [Test]
        public void TheListForTheModellersNamesEachGroupItsReferenceAndItsModels()
        {
            OffCoordinatesAcrossTheRun run = new OffCoordinatesAcrossTheRun(true, Started, 1);
            run.Add("1B06K1", The1B06K1(), true);

            string list = Joined(run.ForModellers());

            Assert.That(list, Does.StartWith("Models not on the same shared coordinates, from the run started 2026-10-05 09:30"));
            Assert.That(list, Does.Contain("The rule judged 1 of 1 group of this run."));
            Assert.That(list, Does.Contain(
                "The rule that skips the clash was on for this run. The next run clashes each skipped group once its"
                + " models are exported again on the project's shared coordinates."));
            Assert.That(list, Does.Contain("\n1B06K1, measured from the reference model AR  1104-PAR-1B06K1-ZZZ-AR-MOD-000001.nwc\n"));
            Assert.That(list, Does.Contain(
                "   its clash was skipped. The tests whose sides both find something are created and none is run."));
            Assert.That(list, Does.Contain("\n   " + The1B06K1().Models[0]));
            Assert.That(list, Does.Not.Contain("were written with every model"));
        }

        /// <summary>
        /// The rule state is given when the run starts and cannot be left unset, so the list
        /// never has to guess it. ForModellers read an unset state as on until c5d8aa8.
        /// </summary>
        [Test]
        public void WithTheRuleOffTheListSaysTheGroupsWereClashed()
        {
            OffCoordinatesAcrossTheRun run = new OffCoordinatesAcrossTheRun(false, Started, 1);
            run.Add("1B06K1", The1B06K1(), true);

            string list = Joined(run.ForModellers());

            Assert.That(list, Does.Contain(
                "The rule that skips the clash was off for this run, so a group below that ran a clash test was"
                + " clashed, and its clashes with the other disciplines cannot be trusted until these models are"
                + " exported again on the project's shared coordinates."));
            Assert.That(list, Does.Contain("   it was clashed, the rule being off."));
            Assert.That(list, Does.Not.Contain("its clash was skipped"));
        }

        // ---------- the ALIGNMENT run line ----------

        /// <summary>
        /// The run line ended "Nothing was changed and every group ran." while RESULT, a few
        /// lines on, listed groups whose clash was skipped. It now says what the rule did.
        /// </summary>
        [Test]
        public void TheAlignmentRunLineNamesTheSkippedGroupsAndNeverSaysEveryGroupRan()
        {
            OffCoordinatesAcrossTheRun run = new OffCoordinatesAcrossTheRun(true, Started, 2);
            run.Add("1B06K1", The1B06K1(), true);
            run.Add("1B06WL", None(), true);

            string line = run.AlignmentRunLine(3);

            Assert.That(line, Is.EqualTo(
                "ALIGNMENT across the run: 3 model(s) sit somewhere their group's reference model does not. Nothing was"
                + " changed in any model, and the clash of 1 group was skipped, models not on the same shared"
                + " coordinates, which RESULT lists."));
            Assert.That(line, Does.Not.Contain("every group ran"));

            OffCoordinatesAcrossTheRun clean = new OffCoordinatesAcrossTheRun(true, Started, 1);
            clean.Add("1B06WL", None(), true);
            Assert.That(clean.AlignmentRunLine(0), Is.EqualTo(
                "ALIGNMENT across the run: 0 model(s) sit somewhere their group's reference model does not"));
            Assert.That(clean.AlignmentRunLine(2), Is.EqualTo(
                "ALIGNMENT across the run: 2 model(s) sit somewhere their group's reference model does not. Nothing was"
                + " changed in any model."));
        }
    }
}
