using System;
using System.Collections.Generic;
using Federator.Core.Health;
using NUnit.Framework;

namespace Federator.Core.Tests.Health
{
    /// <summary>
    /// The models not on the same shared coordinates, Bader's answer to Q99 and Q100 on 2026-10-04,
    /// as his order of 2026-10-14 changed it, F159. The clash is never skipped for such a model. The
    /// check says a NOTICE, in the run log and in the run's list for the modellers, which models, and
    /// that the clash results may be wrong for them. The numbers are the real 1B06K1 of the C06 run of
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

        // ---------- the NOTICE ----------

        [Test]
        public void TheNoticeNamesTheGroupTheReferenceTheModelsAndSaysTheResultsMayBeWrong()
        {
            OffCoordinates off = The1B06K1();
            IList<string> notice = off.Notice("1B06K1");

            Assert.That(notice[0], Is.EqualTo(
                "1B06K1: 1 model is not on the same shared coordinates, measured from the reference model"
                + " AR  1104-PAR-1B06K1-ZZZ-AR-MOD-000001.nwc. The clash results may be wrong for these models."
                + " The clash was run and nothing was skipped."));
            Assert.That(notice[1], Is.EqualTo("   " + off.Models[0]));
            Assert.That(notice.Count, Is.EqualTo(2));
        }

        [Test]
        public void TheNoticeWordsAreBadersAndOneConstant()
        {
            Assert.That(OffCoordinates.ResultsMayBeWrong, Is.EqualTo("The clash results may be wrong for these models."));
        }

        [Test]
        public void ThereIsNoNoticeWhereNoModelIsOff()
        {
            Assert.That(None().Notice("1B06WL"), Is.Empty);
            Assert.That(NotPlaced().Notice("1A04PK"), Is.Empty, "a placement not read is not called off");
        }

        [Test]
        public void AModelOnInternalIsInTheNoticeWithItsOwnWords()
        {
            IList<string> notice = OnInternal().Notice("1A02MM");

            Assert.That(notice[0], Does.StartWith("1A02MM: 1 model is not on the same shared coordinates"));
            Assert.That(Joined(notice), Does.Contain("ST  1104-PAR-1A02MM-ZZZ-ST-MOD-000001.nwc"));
            Assert.That(Joined(notice), Does.Contain("Revit's own origin"));
        }

        [Test]
        public void TwoModelsReadAsModelsAndAreEachOnItsOwnLine()
        {
            OffCoordinates off = AlignmentCheck.NotOnTheSameCoordinates(
                new List<ModelPlacement>
                {
                    new ModelPlacement("x-AR.nwc", "AR", "Site", 0.0, 0.0, 0.0),
                    new ModelPlacement("x-ST.nwc", "ST", "Internal", 0.0, 0.0, 0.0),
                    new ModelPlacement("x-EL.nwc", "EL", string.Empty, 0.0, 0.0, 0.0)
                },
                AlignmentCheck.DefaultFarModelMillimetres);

            IList<string> notice = off.Notice("1A02MM");

            Assert.That(notice[0], Does.StartWith("1A02MM: 2 models are not on the same shared coordinates"));
            Assert.That(notice.Count, Is.EqualTo(3));
        }

        // ---------- the RESULT block ----------

        [Test]
        public void TheResultSaysNothingWasSkippedAndListsEachGroupHoldingAnOffModel()
        {
            OffCoordinatesAcrossTheRun run = new OffCoordinatesAcrossTheRun(Started, 2);
            run.Add("1B06K1", The1B06K1(), true);
            run.Add("1B06WL", None(), true);

            string result = Joined(run.ResultLines());

            Assert.That(result, Does.StartWith(
                "clash skipped  : none, every clash is run whatever the coordinates are. 1 group holds a model not on"
                + " the same shared coordinates, so the clash results may be wrong for it"));
            Assert.That(result, Does.Contain("coordinates    : 2 of 2 groups judged"));
            Assert.That(result, Does.Contain(
                "      1B06K1     1 model(s) not on the same shared coordinates, the clash results may be wrong"));
            Assert.That(result, Does.Not.Contain("1B06WL"), "a group whose models are all in place is not listed");
        }

        [Test]
        public void WithNothingOffTheResultSaysNoClashWasSkippedAndHowManyWereJudged()
        {
            OffCoordinatesAcrossTheRun run = new OffCoordinatesAcrossTheRun(Started, 1);
            run.Add("1B06WL", None(), true);

            Assert.That(Joined(run.ResultLines()), Is.EqualTo(
                "clash skipped  : none, every clash is run whatever the coordinates are\n"
                + "coordinates    : 1 of 1 group judged"));
        }

        /// <summary>
        /// The breaker's first finding at c5d8aa8. One tally lived on the window's log and its groups
        /// were never cleared, so the second run of a window listed 1A02MM twice. Each run has its own,
        /// made when the run starts.
        /// </summary>
        [Test]
        public void EachRunKeepsItsOwnGroups()
        {
            OffCoordinatesAcrossTheRun first = new OffCoordinatesAcrossTheRun(Started, 1);
            first.Add("1A02MM", The1B06K1(), true);

            OffCoordinatesAcrossTheRun second = new OffCoordinatesAcrossTheRun(Started.AddHours(1), 1);
            second.Add("1A02MM", The1B06K1(), true);

            string secondResult = Joined(second.ResultLines());

            Assert.That(Times(secondResult, "1A02MM"), Is.EqualTo(1));
            Assert.That(Joined(second.ForModellers()), Does.StartWith(
                "Models not on the same shared coordinates, from the run started 2026-10-05 10:30"));
            Assert.That(Times(Joined(second.ForModellers()), "1A02MM:"), Is.EqualTo(1));
        }

        /// <summary>
        /// One fixed list name meant a smaller run, or the open file run of one group, wrote over the
        /// list of the full run. Each run's list is its own file, named for the second its run started.
        /// </summary>
        [Test]
        public void EachRunsListIsItsOwnFileNamedForItsStart()
        {
            OffCoordinatesAcrossTheRun full = new OffCoordinatesAcrossTheRun(new DateTime(2026, 10, 5, 9, 30, 0), 46);
            OffCoordinatesAcrossTheRun one = new OffCoordinatesAcrossTheRun(new DateTime(2026, 10, 5, 11, 2, 7), 1);

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
            OffCoordinatesAcrossTheRun run = new OffCoordinatesAcrossTheRun(Started, 46);
            run.Add("1B06WL", None(), true);
            run.Add("1B06WM", None(), true);
            run.Add("1B06WO", None(), true);

            Assert.That(Joined(run.ResultLines()), Does.Contain(
                "coordinates    : 3 of 46 groups judged, 43 not reached, the run stopped or the group ended before its"
                + " models were read"));

            string list = Joined(run.ForModellers());
            Assert.That(list, Does.Not.Contain("No model in this run was found off"));
            Assert.That(list, Does.Contain(
                "The check judged 3 of 46 groups of this run. 43 were not reached, the run stopped or the group ended"
                + " before its models were read."));
            Assert.That(list, Does.Contain("No group is listed, and that is not a clean bill for the groups and models above that were not judged."));
        }

        [Test]
        public void AGroupWhosePlacementsCannotBeReadIsCountedAndNeverReadsAsClean()
        {
            OffCoordinatesAcrossTheRun run = new OffCoordinatesAcrossTheRun(Started, 1);
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
            OffCoordinatesAcrossTheRun run = new OffCoordinatesAcrossTheRun(Started, 1);
            run.Add("1A04PK", null, true);

            Assert.That(Joined(run.ResultLines()), Does.Contain(
                "coordinates    : 0 of 1 group judged, 1 whose models could not be read"));
            Assert.That(Joined(run.ForModellers()), Does.Not.Contain("No model in this run was found off"));
        }

        /// <summary>The one list that may say nothing was found: every group judged and every model judged.</summary>
        [Test]
        public void EveryGroupAndModelJudgedAndNothingOffIsTheOneCleanList()
        {
            OffCoordinatesAcrossTheRun run = new OffCoordinatesAcrossTheRun(Started, 2);
            run.Add("1B06WL", None(), true);
            run.Add("1B06WM", None(), true);

            IList<string> list = run.ForModellers();

            Assert.That(list[list.Count - 1], Is.EqualTo(
                "No model in this run was found off its group's shared coordinates, so no group is listed."));
        }

        // ---------- the one list for the run ----------

        /// <summary>One short file for the run, which Bader forwards to the modellers, with a NOTICE a group.</summary>
        [Test]
        public void TheListForTheModellersCarriesANoticeForEachGroupWithItsReferenceAndItsModels()
        {
            OffCoordinatesAcrossTheRun run = new OffCoordinatesAcrossTheRun(Started, 1);
            run.Add("1B06K1", The1B06K1(), true);

            string list = Joined(run.ForModellers());

            Assert.That(list, Does.StartWith("Models not on the same shared coordinates, from the run started 2026-10-05 09:30"));
            Assert.That(list, Does.Contain("The check judged 1 of 1 group of this run."));
            Assert.That(list, Does.Contain(
                "NOTICE. Every clash was run and none was skipped. The clash results may be wrong for these models."
                + " Export them again on the project's shared coordinates and run again."));
            Assert.That(list, Does.Contain(
                "NOTICE. 1B06K1: 1 model is not on the same shared coordinates, measured from the reference model"
                + " AR  1104-PAR-1B06K1-ZZZ-AR-MOD-000001.nwc. The clash results may be wrong for these models."
                + " The clash was run and nothing was skipped."));
            Assert.That(list, Does.Contain("\n   " + The1B06K1().Models[0]));
            Assert.That(list, Does.Not.Contain("clash was skipped."), "nothing is skipped and the list never says it was");
        }

        /// <summary>
        /// A group with no clash test to run has no clash results, so the list says there are none to
        /// be wrong, and still names its models.
        /// </summary>
        [Test]
        public void AGroupWithNoClashTestToRunStillNamesItsModelsAndSaysThereAreNoResults()
        {
            OffCoordinatesAcrossTheRun run = new OffCoordinatesAcrossTheRun(Started, 1);
            run.Add("1A02MM", The1B06K1(), false);

            string list = Joined(run.ForModellers());

            Assert.That(list, Does.Contain("NOTICE. 1A02MM: 1 model is not on the same shared coordinates"));
            Assert.That(list, Does.Contain("No clash test was to run in this group this run, so there are no results of it to be wrong."));
        }

        // ---------- the ALIGNMENT run line ----------

        [Test]
        public void TheAlignmentRunLineSaysNoClashWasSkippedAndNamesTheGroupsWithOffModels()
        {
            OffCoordinatesAcrossTheRun run = new OffCoordinatesAcrossTheRun(Started, 2);
            run.Add("1B06K1", The1B06K1(), true);
            run.Add("1B06WL", None(), true);

            string line = run.AlignmentRunLine(3);

            Assert.That(line, Is.EqualTo(
                "ALIGNMENT across the run: 3 model(s) sit somewhere their group's reference model does not. Nothing was"
                + " changed in any model, and no clash was skipped. 1 group held models not on the same shared"
                + " coordinates, so the clash results may be wrong for them, which RESULT lists."));
            Assert.That(line, Does.Not.Contain("every group ran"));

            OffCoordinatesAcrossTheRun clean = new OffCoordinatesAcrossTheRun(Started, 1);
            clean.Add("1B06WL", None(), true);
            Assert.That(clean.AlignmentRunLine(0), Is.EqualTo(
                "ALIGNMENT across the run: 0 model(s) sit somewhere their group's reference model does not"));
            Assert.That(clean.AlignmentRunLine(2), Is.EqualTo(
                "ALIGNMENT across the run: 2 model(s) sit somewhere their group's reference model does not. Nothing was"
                + " changed in any model."));
        }

        // ---------- F137, FR-195, Q111 B: a model naming no site is not on the same coordinates ----------

        private const string ArFile = "1104-PAR-1A02MM-ZZZ-AR-MOD-000001.nwc";
        private const string StFile = "1104-PAR-1A02MM-ZZZ-ST-MOD-000001.nwc";

        private static IList<ModelPlacement> TheStNamesNoSite()
        {
            return new List<ModelPlacement>
            {
                new ModelPlacement(ArFile, "AR", "A site", 0.0, 0.0, 0.0),
                new ModelPlacement(StFile, "ST", string.Empty, 0.0, 0.0, 0.0)
            };
        }

        /// <summary>
        /// Bader's answer to Q111, B. A model whose revit_ProjectLocation names no site is not on the
        /// same coordinates, so it is in the group's list and its line says no site is named.
        /// </summary>
        [Test]
        public void AModelNamingNoSiteIsNotOnTheSameCoordinatesAndItsLineSaysSo()
        {
            OffCoordinates off = AlignmentCheck.NotOnTheSameCoordinates(
                TheStNamesNoSite(), AlignmentCheck.DefaultFarModelMillimetres);

            Assert.That(off.Any, Is.True);
            Assert.That(off.Models.Count, Is.EqualTo(1));
            Assert.That(off.Models[0], Does.StartWith("ST  " + StFile));
            Assert.That(off.Models[0], Does.Contain("shared site none named"));
            Assert.That(off.Models[0], Does.Contain("the model names no shared site at all"));
        }

        /// <summary>The notice and the modellers' list carry the same line, so both say no site is named.</summary>
        [Test]
        public void TheNoticeAndTheListSayNoSiteIsNamed()
        {
            OffCoordinates off = AlignmentCheck.NotOnTheSameCoordinates(
                TheStNamesNoSite(), AlignmentCheck.DefaultFarModelMillimetres);

            Assert.That(Joined(off.Notice("1A02MM")), Does.Contain("the model names no shared site at all"));

            OffCoordinatesAcrossTheRun run = new OffCoordinatesAcrossTheRun(Started, 1);
            run.Add("1A02MM", off, true);

            Assert.That(Joined(run.ForModellers()), Does.Contain("the model names no shared site at all"));
        }

        /// <summary>
        /// F176, Bader's answer to Q172. A model naming no site is a NOTICE and fails no group, and the
        /// ALIGNMENT block names the model under a NOTICE and says nothing is skipped.
        /// </summary>
        [Test]
        public void ANoSiteModelFailsNoGroupAndTheBlockSaysANoticeAndSkipsNothing()
        {
            Assert.That(
                AlignmentCheck.NotOnTheSameCoordinates(TheStNamesNoSite(), AlignmentCheck.DefaultFarModelMillimetres).NoSite.Count,
                Is.EqualTo(1));

            string block = Joined(AlignmentCheck.Lines(TheStNamesNoSite(), AlignmentCheck.DefaultFarModelMillimetres));

            Assert.That(block, Does.Contain("NOTICE. 1 model(s) are not on the same shared coordinates"));
            Assert.That(block, Does.Contain("ST  " + StFile));
            Assert.That(block, Does.Not.Contain("CLASH SKIPPED"));
            Assert.That(block, Does.Not.Contain("THIS GROUP IS FAILED"));
        }

        /// <summary>A site that could not be read is not a model naming none, so it is not on any list.</summary>
        [Test]
        public void ASiteThatCouldNotBeReadIsNotAModelNamingNoSite()
        {
            IList<ModelPlacement> models = new List<ModelPlacement>
            {
                new ModelPlacement(ArFile, "AR", "A site", 0.0, 0.0, 0.0),
                new ModelPlacement(StFile, "ST", ModelPlacement.SiteNotRead, 0.0, 0.0, 0.0)
            };

            OffCoordinates off = AlignmentCheck.NotOnTheSameCoordinates(models, AlignmentCheck.DefaultFarModelMillimetres);

            Assert.That(off.Any, Is.False);
            Assert.That(off.Notice("1A02MM"), Is.Empty);
        }

        /// <summary>
        /// The reference itself may name no site. It is named as the reference and as naming none,
        /// and the model beside it on a site is not listed for its distance.
        /// </summary>
        [Test]
        public void AReferenceNamingNoSiteIsNamedAsBoth()
        {
            IList<ModelPlacement> models = new List<ModelPlacement>
            {
                new ModelPlacement(ArFile, "AR", string.Empty, 0.0, 0.0, 0.0),
                new ModelPlacement(StFile, "ST", "A site", 0.0, 0.0, 0.0)
            };

            OffCoordinates off = AlignmentCheck.NotOnTheSameCoordinates(models, AlignmentCheck.DefaultFarModelMillimetres);

            Assert.That(off.Models.Count, Is.EqualTo(1));
            Assert.That(off.Models[0], Does.StartWith("AR  " + ArFile));
            Assert.That(off.Models[0], Does.Contain("the reference model itself"));
            Assert.That(off.Models[0], Does.Contain("the model names no shared site at all"));
        }

        /// <summary>One on Internal and one naming no site are both listed, each by its own words.</summary>
        [Test]
        public void ABothKindsAreListedByTheirOwnWords()
        {
            IList<ModelPlacement> models = new List<ModelPlacement>
            {
                new ModelPlacement(ArFile, "AR", "A site", 0.0, 0.0, 0.0),
                new ModelPlacement(StFile, "ST", string.Empty, 0.0, 0.0, 0.0),
                new ModelPlacement("1104-PAR-1A02MM-ZZZ-EL-MOD-000001.nwc", "EL", "Internal", 0.0, 0.0, 0.0)
            };

            OffCoordinates off = AlignmentCheck.NotOnTheSameCoordinates(models, AlignmentCheck.DefaultFarModelMillimetres);

            Assert.That(off.Models.Count, Is.EqualTo(2));
            Assert.That(off.Models[0], Does.Contain("the model names no shared site at all"));
            Assert.That(off.Models[0], Does.Not.Contain("Revit's own origin"));
            Assert.That(off.Models[1], Does.Contain("Revit's own origin"));
            Assert.That(off.Models[1], Does.Not.Contain("names no shared site"));
        }
    }
}
