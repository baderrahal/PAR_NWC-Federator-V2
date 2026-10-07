using System.Collections.Generic;
using Federator.Core.Clash;
using Federator.Core.Report;
using Federator.Core.Sets;
using NUnit.Framework;

namespace Federator.Core.Tests
{
    /// <summary>
    /// F132 attempts 12 and 13, Bader's answer A to Q142 of 2026-10-07. A mirror's clashes are
    /// merged into its kept test only where this run created both tests of the pair and every
    /// set of both from the picked XML. A test the NWF already held under
    /// either name, an old test renamed under Q136 A, a test renamed back after a change of
    /// roles, and every run with no XML merge nothing: each test keeps its own clashes under its
    /// own name, and the log says why and that a clash both find may be counted twice. Where one
    /// mirror of a kept test is not merged, none of its mirrors is. The five faults the breaker
    /// found on attempt 11 are here as cases that now keep their own clashes. The set and test
    /// names in here are sample data.
    /// </summary>
    [TestFixture]
    public class MirrorMergedWhereTests
    {
        private const string Ducts = MirrorRuleTests.Ducts;
        private const string Columns = MirrorRuleTests.Columns;
        private const string Walls = MirrorRuleTests.Walls;
        private const string Telecom = MirrorRuleTests.Telecom;
        private const string Telephone = MirrorRuleTests.Telephone;
        private const string Kept = MirrorRuleTests.DuctsVsColumns;
        private const string Swap = MirrorRuleTests.ColumnsVsDucts;
        private const string SwapMirror = Swap + " (mirror)";
        private const string TelecomVsWalls = MirrorRuleTests.TelecomVsWalls;
        private const string TelephoneVsWalls = MirrorRuleTests.TelephoneVsWalls;
        private const string WallsVsTelephone = "BLD-AR-Walls-vs-BLD-EL-Telephone Devices";

        private static ClashTestPlan TheXmlPlan()
        {
            return MirrorRuleTests.Plan(
                MirrorRuleTests.Test(Kept, Ducts, Columns),
                MirrorRuleTests.Test(Swap, Columns, Ducts));
        }

        /// <summary>The rule of the XML holding Ducts against Columns and its swap, over that document and that sets build.</summary>
        private static MirrorRule Over(ClashTestPlan saved, SetBuildOutcome built)
        {
            return MirrorRuleTests.RuleBuilt(TheXmlPlan().Buildable, PriorityMap.NothingPicked(), saved, built);
        }

        private static ClashTestPlan Holding(params SavedClashTest[] saved)
        {
            return ClashTestPlan.FromDocument(new List<SavedClashTest>(saved), "m");
        }

        private static int Merges(MirrorRule rule)
        {
            return MirrorMerge.Of(rule).Count;
        }

        /// <summary>The line of a test kept whose mirrors are not merged, and why.</summary>
        private static string NotMerged(string kept, string mirrors, string because)
        {
            return MirrorRule.Prefix + "   " + kept + (mirrors.Contains(" and ") ? " and its mirrors " : " and its mirror ")
                + mirrors + " keep their own clashes under their own names, not merged, because " + because
                + ", and a clash both find may be counted twice";
        }

        /// <summary>A set already in the document, its question read off it as the drift check reads it, that many conditions.</summary>
        private static void Present(SetBuildOutcome built, string path, string asked)
        {
            built.AddAlreadyPresent(path, path.Substring(path.LastIndexOf('/') + 1), 1, 5).Asked = asked;
        }

        private const string Element = "LcRevitData_Element";
        private const string Category = "LcRevitPropertyElementCategory";

        /// <summary>What the drift check reads off a set asking Category equals that value, as SetDrift compares it.</summary>
        private static SetDrift DriftOf(string path, string askedValue, string wantedValue)
        {
            ReadCondition asked = new ReadCondition(Element, Category, "equals", askedValue);
            ReadCondition wanted = new ReadCondition(Element, Category, "equals", wantedValue);

            return SetDrift.Compare(
                path, new List<ReadCondition> { asked }, new List<string> { wanted.Key() }, new List<string> { wanted.Describe() });
        }

        // A First run over a new NWF, RunPath.Label: both tests are created from the XML and
        // both sets are built from it, so the mirror is merged.
        [Test]
        public void BothTestsCreatedFromTheXmlOverSetsBuiltFromItAreMerged()
        {
            MirrorRule rule = Over(MirrorRuleTests.NothingSaved(), MirrorRuleTests.Built(Ducts, Columns));

            Assert.That(rule.Pairs.Count, Is.EqualTo(1));
            Assert.That(Merges(rule), Is.EqualTo(1));
            Assert.That(string.Join("\n", new List<string>(rule.Lines()).ToArray()), Does.Not.Contain("not merged"));
        }

        // The breaker's finding 1 on attempt 11. With no XML a saved mirror paired by its sides
        // alone, at another tolerance than its kept test here, and its clashes were merged at
        // settings no XML gave. Now a run with no XML merges nothing, whatever the settings, the
        // pair is still found and named, and its line says why.
        [Test]
        public void ARunWithNoXmlMergesNothingWhateverTheMirrorsSettings()
        {
            foreach (double tolerance in new[] { 0.025, 0.05 })
            {
                ClashTestPlan saved = Holding(
                    MirrorRuleTests.Saved(Kept, Ducts, Columns, 0),
                    new SavedClashTest(SwapMirror, 1, tolerance, true, false, 1, Columns, false, 1, Ducts, new[] { 1 }));
                MirrorRule rule = MirrorRule.Of(
                    saved.Buildable, PriorityMap.NothingPicked(), null, new MirrorSettings(), null, null);

                Assert.That(rule.Pairs.Count, Is.EqualTo(1), "found by its sides, " + tolerance);
                Assert.That(Merges(rule), Is.EqualTo(0), "no clash of " + SwapMirror + " goes under " + Kept + ", " + tolerance);
                Assert.That(rule.Lines()[0], Does.EndWith(". No XML was picked, so no pair is merged: both tests of each "
                    + "pair are run as they are saved and each keeps its own clashes under its own name, and a clash both "
                    + "find may be counted twice"));
            }
        }

        // The breaker's finding 2 on attempt 11. A pair is judged on the picked XML's sets, and a
        // set already in the NWF keeps the question it was built with. Where the drift check
        // found a set of either test asking another question than the XML, nothing is merged,
        // for the same two sets swapped and for two sets of one rule list alike.
        [Test]
        public void ASetTheDriftCheckFoundDriftedMergesNothing()
        {
            SetBuildOutcome swapped = MirrorRuleTests.Built(Ducts);
            Present(swapped, Columns, "Category equals Old Columns");
            swapped.AddDrift(DriftOf(Columns, "Old Columns", "Structural Columns"), false);

            MirrorRule rule = Over(MirrorRuleTests.NothingSaved(), swapped);

            Assert.That(Merges(rule), Is.EqualTo(0));
            Assert.That(rule.Lines(), Does.Contain(NotMerged(Kept, SwapMirror, "the set " + Columns + " was found by the "
                + "SET DRIFT check asking another question than the picked XML")));

            SetBuildOutcome byRuleList = MirrorRuleTests.Built(Telecom, Walls);
            Present(byRuleList, Telephone, "Category equals Telephones");
            byRuleList.AddDrift(DriftOf(Telephone, "Telephones", "Telephone Devices"), true);

            MirrorRule alike = MirrorRuleTests.RuleBuilt(
                MirrorRuleTests.Plan(
                    MirrorRuleTests.Test(TelecomVsWalls, Telecom, Walls),
                    MirrorRuleTests.Test(TelephoneVsWalls, Telephone, Walls)).Buildable,
                PriorityMap.NothingPicked(),
                MirrorRuleTests.NothingSaved(),
                byRuleList);

            Assert.That(alike.Pairs.Count, Is.EqualTo(1));
            Assert.That(Merges(alike), Is.EqualTo(0), "drifted and rebuilt is still drifted");
        }

        // The breaker's finding 1 and the reviewer's row F132-R2 on attempt 12. The drift check
        // reads four strings per condition, the category, the property, contains or equals,
        // and the value, and not a set's flags, its negation, its Or group, its ignore bits or
        // its search's base selection, so a present set changed only in those read as not
        // drifted and the pair merged on UNKNOWN. Now a set already in the document merges
        // nothing whatever the drift check read of it, read and not drifted, not read, or
        // never asked, and so do a set that failed to build and a set the build never reached.
        // A merge rests only on sets this run created.
        [Test]
        public void ASetAlreadyInTheDocumentMergesNothingWhateverTheDriftCheckRead()
        {
            const string present = " was already in the document and not created by this run, so whether it asks what the "
                + "picked XML asks is UNKNOWN, the set drift check reading each condition's values and not a set's flags "
                + "or search";

            SetBuildOutcome read = MirrorRuleTests.Built(Ducts);
            Present(read, Columns, "Category equals Structural Columns");

            SetBuildOutcome notRead = MirrorRuleTests.Built(Ducts);
            Present(notRead, Columns, SetDrift.Compare(Columns, null, new List<string>(), new List<string>()).AskedNow());

            SetBuildOutcome neverAsked = MirrorRuleTests.Built(Ducts);
            neverAsked.AddAlreadyPresent(Columns, "BLD-ST-Columns", 1, 5);

            SetBuildOutcome failed = MirrorRuleTests.Built(Ducts);
            failed.AddFailed(Columns, "BLD-ST-Columns", 1, "the API threw");

            MirrorRule notDrifted = Over(MirrorRuleTests.NothingSaved(), read);
            MirrorRule unread = Over(MirrorRuleTests.NothingSaved(), notRead);
            MirrorRule unasked = Over(MirrorRuleTests.NothingSaved(), neverAsked);
            MirrorRule missing = Over(MirrorRuleTests.NothingSaved(), MirrorRuleTests.Built(Ducts));
            MirrorRule broken = Over(MirrorRuleTests.NothingSaved(), failed);

            Assert.That(Merges(notDrifted), Is.EqualTo(0), "read and not drifted by value is still not created this run");
            Assert.That(notDrifted.Lines(), Does.Contain(NotMerged(Kept, SwapMirror, "the set " + Columns + present)));
            Assert.That(Merges(unread), Is.EqualTo(0));
            Assert.That(unread.Lines(), Does.Contain(NotMerged(Kept, SwapMirror, "the set " + Columns + present)));
            Assert.That(Merges(unasked), Is.EqualTo(0));
            Assert.That(unasked.Lines(), Does.Contain(NotMerged(Kept, SwapMirror, "the set " + Columns + present)));
            Assert.That(Merges(missing), Is.EqualTo(0));
            Assert.That(missing.Lines(), Does.Contain(NotMerged(Kept, SwapMirror, "the set " + Columns + " was neither "
                + "built nor found by this run's sets build, so what it asks is UNKNOWN")));
            Assert.That(Merges(broken), Is.EqualTo(0));
            Assert.That(broken.Lines(), Does.Contain(NotMerged(Kept, SwapMirror, "the set " + Columns + " failed to "
                + "build, so what it asks is UNKNOWN")));
        }

        // The reviewer's fail open default on attempt 12. A pair built and never judged merged.
        // Now a pair merges only once JudgeTheMerges has said so, and a path that skips the
        // judge merges nothing.
        [Test]
        public void APairMergesOnlyOnceJudged()
        {
            IList<PlannedClashTest> tests = TheXmlPlan().Buildable;
            MirrorPair fresh = new MirrorPair(
                tests[0], ClashPriority.None, tests[1], ClashPriority.None, MirrorKind.Swapped, null, SwapMirror, true);

            Assert.That(fresh.Merges, Is.False);
            Assert.That(Over(MirrorRuleTests.NothingSaved(), MirrorRuleTests.Built(Ducts, Columns)).Pairs[0].Merges, Is.True);
        }

        // The reviewer's finding 3 on attempt 12. The document holds a test under the name the
        // mirror runs under, so the mirror is found by that name and run as it is saved, and
        // the pair line and the coverage words said it was created. Now they say what is done.
        [Test]
        public void AMirrorHeldUnderItsRunNameIsSaidRunAsItIsSaved()
        {
            MirrorRule rule = Over(Holding(MirrorRuleTests.Saved(SwapMirror, Columns, Ducts, 0)), MirrorRuleTests.Built(Ducts, Columns));
            string pairLine = PairLine(rule);

            Assert.That(Merges(rule), Is.EqualTo(0));
            Assert.That(pairLine, Does.Contain(Swap + " is its mirror, the same two sets swapped, run as it is saved, the "
                + "document already holding a test named " + SwapMirror));
            Assert.That(pairLine, Does.Not.Contain("created"));
            Assert.That(rule.CoverageNames()[1].Value, Is.EqualTo("a mirror of " + Kept + ", the same two sets swapped, run as "
                + "it is saved, the document already holding a test named " + SwapMirror + ", its clashes kept under its own "
                + "name"));
        }

        // Q136 A, the same words. An old test renamed to the name the mirror runs under is run
        // as it is saved under that name, not created.
        [Test]
        public void AMirrorAnOldTestIsRenamedToIsSaidRunAsItIsSaved()
        {
            MirrorRule rule = Over(Holding(MirrorRuleTests.Saved(Swap, Columns, Ducts, 0)), MirrorRuleTests.Built(Ducts, Columns));

            Assert.That(rule.Renames.Planned.Count, Is.EqualTo(1));
            Assert.That(PairLine(rule), Does.Contain("the same two sets swapped, run as it is saved, a test the document "
                + "holds renamed " + SwapMirror));
            Assert.That(PairLine(rule), Does.Not.Contain("created"));
        }

        // The reviewer's and the breaker's finding on attempt 12 that the count line and the
        // coverage sheet say merged at plan time, before MirrorMerge.AddTo has run and can
        // still merge nothing. Now they say to be merged once both tests run.
        [Test]
        public void TheWordsOfAMergeSayItWaitsOnTheRun()
        {
            MirrorRule rule = Over(MirrorRuleTests.NothingSaved(), MirrorRuleTests.Built(Ducts, Columns));
            IList<KeyValuePair<string, string>> names = rule.CoverageNames();

            Assert.That(Merges(rule), Is.EqualTo(1));
            Assert.That(rule.Lines()[0], Does.Contain("Both tests of each pair are run. 1 mirror is to be merged by the pair "
                + "of items into the one kept once both tests of the pair run, the higher priority"));
            Assert.That(rule.Lines()[0], Does.EndWith("since a mirror is merged only where this run created both tests of "
                + "the pair and every set of both from the picked XML"));
            Assert.That(names[0].Value, Is.EqualTo("kept of a mirrored pair, the clashes only its mirror " + SwapMirror
                + " finds are to be added to it once both run"));
            Assert.That(names[1].Value, Does.EndWith(", its clashes to be merged into " + Kept + "'s once both run"));
        }

        /// <summary>The MIRROR line naming the pair of the XML holding Ducts against Columns and its swap.</summary>
        private static string PairLine(MirrorRule rule)
        {
            foreach (string line in rule.Lines())
            {
                if (line.StartsWith(MirrorRule.Prefix + "   " + Kept + " is kept, ", System.StringComparison.Ordinal))
                {
                    return line;
                }
            }

            return "no pair line";
        }

        // The breaker's finding 3 on attempt 11. A test the NWF already holds under the kept
        // test's name was run as a test of the pair where it carried every setting the DRIFT
        // block reads, though an ignore rule a person added is read by none. Now a test this run
        // did not create is never merged, whatever it carries, and its line says why.
        [Test]
        public void ATestTheDocumentHoldsUnderTheKeptTestsNameMergesNothing()
        {
            MirrorRule rule = Over(Holding(MirrorRuleTests.Saved(Kept, Ducts, Columns, 0)), MirrorRuleTests.Built(Ducts, Columns));

            Assert.That(rule.Pairs.Count, Is.EqualTo(1), "both still run");
            Assert.That(rule.Pairs[0].MirrorName, Is.EqualTo(SwapMirror));
            Assert.That(Merges(rule), Is.EqualTo(0));
            Assert.That(rule.Lines(), Does.Contain(NotMerged(Kept, SwapMirror, "the document already holds a test named "
                + Kept)));
        }

        // The breaker's finding 4 on attempt 11. The XML names the mirror with the ending
        // already, and the NWF holds a test of that name with the kept test's two sets in the
        // kept test's order, a duplicate. It was run as the mirror, so the clashes only a true
        // swap finds were never found. Now it keeps its own clashes under its own name.
        [Test]
        public void ADuplicateUnderTheMirrorsOwnXmlNameMergesNothing()
        {
            MirrorRule rule = MirrorRuleTests.RuleBuilt(
                MirrorRuleTests.Plan(
                    MirrorRuleTests.Test(Kept, Ducts, Columns),
                    MirrorRuleTests.Test(SwapMirror, Columns, Ducts)).Buildable,
                PriorityMap.NothingPicked(),
                Holding(MirrorRuleTests.Saved(SwapMirror, Ducts, Columns, 0)),
                MirrorRuleTests.Built(Ducts, Columns));

            Assert.That(Merges(rule), Is.EqualTo(0));
            Assert.That(rule.Lines(), Does.Contain(NotMerged(Kept, SwapMirror, "the document already holds a test named "
                + SwapMirror)));
        }

        // Q136 A. An old test saved under the XML's name of the mirror is renamed as the mirror,
        // its statuses kept. This run did not create it, so its clashes stay under its new name.
        [Test]
        public void AnOldTestRenamedAsTheMirrorMergesNothing()
        {
            MirrorRule rule = Over(Holding(MirrorRuleTests.Saved(Swap, Columns, Ducts, 0)), MirrorRuleTests.Built(Ducts, Columns));

            Assert.That(rule.Renames.Planned.Count, Is.EqualTo(1));
            Assert.That(rule.Renames.Planned[0].NewName, Is.EqualTo(SwapMirror));
            Assert.That(Merges(rule), Is.EqualTo(0));
            Assert.That(rule.Lines(), Does.Contain(NotMerged(Kept, SwapMirror, "the document already holds a test named "
                + Swap + " and a test the document holds is renamed " + SwapMirror)));
        }

        // A change of roles between two XML runs. The mirror an earlier run made is renamed
        // back to the name of the test the XML now keeps, its statuses kept. This run did not
        // create it, so nothing is merged into it.
        [Test]
        public void ATestRenamedBackAfterAChangeOfRolesMergesNothing()
        {
            MirrorRule rule = MirrorRuleTests.RuleBuilt(
                TheXmlPlan().Buildable,
                MirrorRuleTests.Priorities(Swap, "A"),
                Holding(MirrorRuleTests.Saved(SwapMirror, Columns, Ducts, 0)),
                MirrorRuleTests.Built(Ducts, Columns));

            Assert.That(rule.Pairs[0].Kept.Name, Is.EqualTo(Swap));
            Assert.That(rule.Renames.Planned.Count, Is.EqualTo(1));
            Assert.That(rule.Renames.Planned[0].NewName, Is.EqualTo(Swap));
            Assert.That(Merges(rule), Is.EqualTo(0));
            Assert.That(rule.Lines(), Does.Contain(NotMerged(Swap, Kept + " (mirror)", "a test the document holds is "
                + "renamed " + Swap)));
        }

        // The breaker's finding 5 on attempt 11, at the plan. A test kept over two mirrors, one
        // of them held in the NWF and so not merged. Merging the other would add a clash both
        // mirrors find and the kept test does not to the kept test, while the held mirror holds
        // it too. So where one mirror of a kept test is not merged, none of its mirrors is.
        [Test]
        public void WhereOneMirrorOfATestKeptIsHeldNoneOfItsMirrorsMerges()
        {
            MirrorRule rule = MirrorRuleTests.RuleBuilt(
                MirrorRuleTests.Plan(
                    MirrorRuleTests.Test(TelecomVsWalls, Telecom, Walls),
                    MirrorRuleTests.Test(TelephoneVsWalls, Telephone, Walls),
                    MirrorRuleTests.Test(WallsVsTelephone, Walls, Telephone)).Buildable,
                MirrorRuleTests.Priorities(TelecomVsWalls, "A"),
                Holding(MirrorRuleTests.Saved(WallsVsTelephone + " (mirror)", Walls, Telephone, 0)),
                MirrorRuleTests.Built(Telecom, Telephone, Walls));

            Assert.That(rule.Pairs.Count, Is.EqualTo(2));
            Assert.That(Merges(rule), Is.EqualTo(0));
            Assert.That(rule.Lines(), Does.Contain(NotMerged(
                TelecomVsWalls,
                TelephoneVsWalls + " (mirror) and " + WallsVsTelephone + " (mirror)",
                "the document already holds a test named " + WallsVsTelephone + " (mirror)")));
        }

        // An XML run is handed this run's sets build, so a set's drift is read before any merge.
        [Test]
        public void AnXmlRuleWithNoSetsBuildIsRefused()
        {
            Assert.Throws<System.ArgumentNullException>(() => MirrorRule.Of(
                TheXmlPlan().Buildable, PriorityMap.NothingPicked(), null, new MirrorSettings(),
                MirrorRuleTests.NothingSaved(), null));
        }
    }
}
