using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Federator.Core.Clash;
using Federator.Core.Exchange;
using NUnit.Framework;

namespace Federator.Core.Tests
{
    /// <summary>
    /// F132, FR-182, Bader's Q114 points 4 to 6 and 8. A test whose two sides are another
    /// test's two sets swapped finds the same clashes twice and doubles the workbook rows,
    /// the viewpoints and the time. One test of each pair is kept, the higher priority, A
    /// before B before C, and where equal the one first in the XML. The other is not
    /// created and not run. The set and test names in here are sample data and not
    /// settings, and nothing in the rule names any of them.
    /// </summary>
    [TestFixture]
    public class MirrorRuleTests
    {
        private const string Root = "lcop_selection_set_tree";
        internal const string Ducts = Root + "/Mechanical/BLD-ME-Ducts";
        internal const string Columns = Root + "/Structure/BLD-ST-Columns";
        private const string ArColumns = Root + "/Architecture/BLD-AR-Columns";
        internal const string Walls = Root + "/Architecture/BLD-AR-Walls";

        internal const string DuctsVsColumns = "BLD-ME-Ducts-vs-BLD-ST-Columns";
        internal const string ColumnsVsDucts = "BLD-ST-Columns-vs-BLD-ME-Ducts";

        // 25 mm and 75 mm written in feet, the two tolerances .claude\rules\core.md records.
        private const string Mm25 = "0.0820209974";
        private const string Mm75 = "0.2460629921";

        /// <summary>One clashtest written the way the reference file writes them.</summary>
        internal static string Test(
            string name, string left, string right, string tolerance = Mm25, string type = "hard_conservative")
        {
            return "<clashtest name=\"" + name + "\" test_type=\"" + type + "\" status=\"new\""
                + " tolerance=\"" + tolerance + "\" merge_composites=\"1\">"
                + "<left><clashselection selfintersect=\"0\" primtypes=\"1\"><locator>" + left
                + "</locator></clashselection></left>"
                + "<right><clashselection selfintersect=\"0\" primtypes=\"1\"><locator>" + right
                + "</locator></clashselection></right>"
                + "</clashtest>";
        }

        /// <summary>The plan of an XML holding those tests in that order, in a metric document.</summary>
        internal static ClashTestPlan Plan(params string[] tests)
        {
            string xml = "<exchange units=\"ft\"><batchtest name=\"b\" internal_name=\"b\">"
                + string.Concat(tests) + "</batchtest></exchange>";

            return ClashTestPlan.From(new ExchangeReader().ReadText(xml), "m");
        }

        /// <summary>A priority file, given as a test name then its letter, again and again.</summary>
        internal static PriorityMap Priorities(params string[] nameThenLetter)
        {
            StringBuilder text = new StringBuilder("test_name,left_set,right_set,priority\n");

            for (int i = 0; i + 1 < nameThenLetter.Length; i += 2)
            {
                text.Append(nameThenLetter[i]).Append(",left,right,").Append(nameThenLetter[i + 1]).Append('\n');
            }

            return PriorityMap.Read(text.ToString(), "priorities.csv");
        }

        internal static SavedClashTest Saved(string name, string left, string right, int address)
        {
            return new SavedClashTest(name, 1, 0.025, true, false, 1, left, false, 1, right, new[] { address });
        }

        private static string Text(IList<string> lines)
        {
            return string.Join("\n", new List<string>(lines).ToArray());
        }

        private static string LineNaming(MirrorRule rule, string name)
        {
            foreach (string line in rule.Lines())
            {
                if (line.Contains(name))
                {
                    return line;
                }
            }

            return null;
        }

        // ---------- what a pair is ----------

        [Test]
        public void DuctsAgainstColumnsWithColumnsAgainstDuctsKeepsOne()
        {
            ClashTestPlan plan = Plan(
                Test(DuctsVsColumns, Ducts, Columns),
                Test(ColumnsVsDucts, Columns, Ducts));

            MirrorRule rule = MirrorRule.Of(plan.Buildable, PriorityMap.NothingPicked());

            Assert.That(rule.Pairs.Count, Is.EqualTo(1));
            Assert.That(rule.Pairs[0].Kept.Name, Is.EqualTo(DuctsVsColumns));
            Assert.That(rule.Pairs[0].Mirror.Name, Is.EqualTo(ColumnsVsDucts));
        }

        // Bader's own example, as the picked XML holds it: the Ducts set against the AR
        // Columns and against the ST Columns. Two Columns sets of two disciplines are two
        // sets, so these are two tests over two pairs and not a mirror.
        [Test]
        public void TwoColumnsSetsOfTwoDisciplinesAreTwoSets()
        {
            ClashTestPlan plan = Plan(
                Test("BLD-ME-Ducts-vs-BLD-AR-Columns", Ducts, ArColumns),
                Test(ColumnsVsDucts, Columns, Ducts));

            Assert.That(MirrorRule.Of(plan.Buildable, PriorityMap.NothingPicked()).Pairs.Count, Is.EqualTo(0));
        }

        [Test]
        public void ASetNameEndingInASpaceIsNotTheSameSet()
        {
            ClashTestPlan plan = Plan(
                Test(DuctsVsColumns, Ducts, Columns),
                Test("BLD-ST-Columns -vs-BLD-ME-Ducts", Columns + " ", Ducts));

            Assert.That(plan.Buildable[1].Left.Locator, Is.EqualTo(Columns + " "),
                "the reader kept the space, so the rule is handed it");
            Assert.That(MirrorRule.Of(plan.Buildable, PriorityMap.NothingPicked()).Pairs.Count, Is.EqualTo(0));
        }

        [Test]
        public void ATestWithOneSetOnBothSidesIsNeverAMirror()
        {
            ClashTestPlan plan = Plan(
                Test("BLD-ME-Ducts-vs-BLD-ME-Ducts", Ducts, Ducts),
                Test("BLD-ME-Ducts-vs-BLD-ME-Ducts again", Ducts, Ducts));

            MirrorRule rule = MirrorRule.Of(plan.Buildable, PriorityMap.NothingPicked());

            Assert.That(rule.Pairs.Count, Is.EqualTo(0));
            Assert.That(plan.WithoutMirrors(rule).Buildable.Count, Is.EqualTo(2));
        }

        // One rule in one place. The two sets of a test either way round are the key the by
        // design pairs are found on, ByDesignPairs.KeyFor, and the mirror rule pairs on that
        // same key and not on a copy of it.
        [Test]
        public void TheMirrorRulePairsOnTheByDesignKey()
        {
            ClashTestPlan plan = Plan(
                Test(DuctsVsColumns, Ducts, Columns),
                Test(ColumnsVsDucts, Columns, Ducts));

            Assert.That(MirrorRule.SetsKey(plan.Buildable[0]), Is.EqualTo(ByDesignPairs.KeyFor(Ducts, Columns)));
            Assert.That(MirrorRule.SetsKey(plan.Buildable[1]), Is.EqualTo(ByDesignPairs.KeyFor(Ducts, Columns)));
        }

        [Test]
        public void ASavedSideThatCouldNotBeReadIsNeverPaired()
        {
            foreach (string unread in new[]
                { TestSettings.UnknownLocator, string.Empty, SavedClashTest.LeftAsSaved, SavedClashTest.RightAsSaved })
            {
                ClashTestPlan plan = ClashTestPlan.FromDocument(
                    new List<SavedClashTest>
                    {
                        Saved("first", unread, Ducts, 0),
                        Saved("second", Ducts, unread, 1)
                    },
                    "m");

                Assert.That(plan.Buildable.Count, Is.EqualTo(2));
                Assert.That(MirrorRule.Of(plan.Buildable, PriorityMap.NothingPicked()).Pairs.Count, Is.EqualTo(0),
                    "two sides nobody read are not one set, read as \"" + unread + "\"");
            }
        }

        // ---------- which one is kept ----------

        [Test]
        public void ABeatsBWhicheverComesFirst()
        {
            PriorityMap priorities = Priorities(DuctsVsColumns, "A", ColumnsVsDucts, "B");
            string a = Test(DuctsVsColumns, Ducts, Columns);
            string b = Test(ColumnsVsDucts, Columns, Ducts);

            foreach (ClashTestPlan plan in new[] { Plan(a, b), Plan(b, a) })
            {
                MirrorRule rule = MirrorRule.Of(plan.Buildable, priorities);

                Assert.That(rule.Pairs.Count, Is.EqualTo(1));
                Assert.That(rule.Pairs[0].Kept.Name, Is.EqualTo(DuctsVsColumns));
                Assert.That(rule.Pairs[0].Mirror.Name, Is.EqualTo(ColumnsVsDucts));
            }
        }

        [Test]
        public void BBeatsCWhicheverComesFirst()
        {
            PriorityMap priorities = Priorities(DuctsVsColumns, "C", ColumnsVsDucts, "B");
            string a = Test(DuctsVsColumns, Ducts, Columns);
            string b = Test(ColumnsVsDucts, Columns, Ducts);

            foreach (ClashTestPlan plan in new[] { Plan(a, b), Plan(b, a) })
            {
                Assert.That(MirrorRule.Of(plan.Buildable, priorities).Pairs[0].Kept.Name, Is.EqualTo(ColumnsVsDucts));
            }
        }

        [Test]
        public void EqualLettersKeepTheFirstInTheXml()
        {
            PriorityMap priorities = Priorities(DuctsVsColumns, "C", ColumnsVsDucts, "C");
            ClashTestPlan plan = Plan(
                Test(ColumnsVsDucts, Columns, Ducts),
                Test(DuctsVsColumns, Ducts, Columns));

            MirrorRule rule = MirrorRule.Of(plan.Buildable, priorities);

            Assert.That(rule.Pairs[0].Kept.Name, Is.EqualTo(ColumnsVsDucts));
            Assert.That(rule.Pairs[0].Mirror.Name, Is.EqualTo(DuctsVsColumns));
        }

        [Test]
        public void NoPriorityLosesToC()
        {
            PriorityMap priorities = Priorities(ColumnsVsDucts, "C");
            ClashTestPlan plan = Plan(
                Test(DuctsVsColumns, Ducts, Columns),
                Test(ColumnsVsDucts, Columns, Ducts));

            Assert.That(MirrorRule.Of(plan.Buildable, priorities).Pairs[0].Kept.Name, Is.EqualTo(ColumnsVsDucts));
        }

        // A second test in the kept test's own order is a duplicate. That is not a mirror
        // by his words, so it is named as a finding and run as before.
        [Test]
        public void ThreeTestsOnOnePairKeepOneNameOneMirrorAndOneDuplicate()
        {
            ClashTestPlan plan = Plan(
                Test(DuctsVsColumns, Ducts, Columns),
                Test("Ducts against Columns again", Ducts, Columns),
                Test(ColumnsVsDucts, Columns, Ducts));

            MirrorRule rule = MirrorRule.Of(plan.Buildable, PriorityMap.NothingPicked());

            Assert.That(rule.Pairs.Count, Is.EqualTo(1));
            Assert.That(rule.Pairs[0].Kept.Name, Is.EqualTo(DuctsVsColumns));
            Assert.That(rule.Pairs[0].Mirror.Name, Is.EqualTo(ColumnsVsDucts));

            string duplicate = LineNaming(rule, "Ducts against Columns again");

            Assert.That(duplicate, Is.Not.Null, "the duplicate is not named");
            Assert.That(duplicate, Does.Contain("duplicate"));
            Assert.That(duplicate, Does.Contain(DuctsVsColumns));

            List<string> stillRun = new List<string>();

            foreach (PlannedClashTest test in plan.WithoutMirrors(rule).Buildable)
            {
                stillRun.Add(test.Name);
            }

            Assert.That(stillRun.Count, Is.EqualTo(2));
            Assert.That(stillRun[0], Is.EqualTo(DuctsVsColumns));
            Assert.That(stillRun[1], Is.EqualTo("Ducts against Columns again"));
        }

        // ---------- what the log says ----------

        [Test]
        public void NoPairIsOneLineSayingSo()
        {
            ClashTestPlan plan = Plan(
                Test(DuctsVsColumns, Ducts, Columns),
                Test("BLD-ME-Ducts-vs-BLD-AR-Walls", Ducts, Walls));

            IList<string> lines = MirrorRule.Of(plan.Buildable, PriorityMap.NothingPicked()).Lines();

            Assert.That(lines.Count, Is.EqualTo(1));
            Assert.That(lines[0], Does.StartWith(MirrorRule.Prefix + " "));
            Assert.That(lines[0], Does.Contain("0 pairs"));
        }

        [Test]
        public void EveryPairIsALineNamingBothTests()
        {
            ClashTestPlan plan = Plan(
                Test(DuctsVsColumns, Ducts, Columns),
                Test("BLD-ME-Ducts-vs-BLD-AR-Walls", Ducts, Walls),
                Test(ColumnsVsDucts, Columns, Ducts),
                Test("BLD-AR-Walls-vs-BLD-ME-Ducts", Walls, Ducts));

            MirrorRule rule = MirrorRule.Of(plan.Buildable, PriorityMap.NothingPicked());
            IList<string> lines = rule.Lines();

            Assert.That(rule.Pairs.Count, Is.EqualTo(2));
            Assert.That(lines[0], Does.Contain("2 pairs"));

            string first = LineNaming(rule, ColumnsVsDucts);
            string second = LineNaming(rule, "BLD-AR-Walls-vs-BLD-ME-Ducts");

            Assert.That(first, Does.StartWith(MirrorRule.Prefix + " "));
            Assert.That(first, Does.Contain(DuctsVsColumns));
            Assert.That(first, Does.Contain("not created"));
            Assert.That(second, Does.Contain("BLD-ME-Ducts-vs-BLD-AR-Walls"));

            foreach (string line in lines)
            {
                Assert.That(line, Does.StartWith(MirrorRule.Prefix + " "));
            }
        }

        /// <summary>
        /// A matrix written both ways round, so every test of the first half has its swap in
        /// the second half, the swap carrying a priority of its own where one is given.
        /// </summary>
        private static MirrorRule BothWaysRound(int sets, params string[] nameThenLetter)
        {
            List<string> first = new List<string>();
            List<string> second = new List<string>();

            for (int i = 0; i < sets; i++)
            {
                for (int j = i + 1; j < sets; j++)
                {
                    string a = Root + "/S" + i;
                    string b = Root + "/S" + j;

                    first.Add(Test("S" + i + "-vs-S" + j, a, b));
                    second.Add(Test("S" + j + "-vs-S" + i, b, a));
                }
            }

            first.AddRange(second);
            return MirrorRule.Of(Plan(first.ToArray()).Buildable, Priorities(nameThenLetter));
        }

        private static int LinesNamingAPair(MirrorRule rule)
        {
            int named = 0;

            foreach (string line in rule.Lines())
            {
                if (line.Contains(" is its mirror "))
                {
                    named++;
                }
            }

            return named;
        }

        // Five sets both ways round are ten pairs alike in priority, test type and tolerance.
        // The skip block already names five and counts the rest, so the MIRROR lines do the
        // same, and say alike in those three, the three compared, and never in everything.
        [Test]
        public void PairsAlikeInPriorityTypeAndToleranceNameFiveAndCountTheRest()
        {
            MirrorRule rule = BothWaysRound(5);
            string all = Text(rule.Lines());

            Assert.That(rule.Pairs.Count, Is.EqualTo(10));
            Assert.That(LinesNamingAPair(rule), Is.EqualTo(5));
            Assert.That(all, Does.Contain(
                "and 5 more pairs alike in priority, test type and tolerance, counted and not listed"));
            Assert.That(all, Does.Not.Contain("everything"));
        }

        // Bader asked for both named where the two differ, so a pair that differs is never
        // among the ones counted and not listed.
        [Test]
        public void EveryPairThatDiffersIsNamed()
        {
            MirrorRule rule = BothWaysRound(
                5,
                "S1-vs-S0", "A", "S2-vs-S0", "A", "S3-vs-S0", "A", "S4-vs-S0", "A",
                "S2-vs-S1", "A", "S3-vs-S1", "A", "S4-vs-S1", "A");

            string all = Text(rule.Lines());

            Assert.That(rule.Pairs.Count, Is.EqualTo(10));
            Assert.That(LinesNamingAPair(rule), Is.EqualTo(10), "7 that differ and 3 alike, all within five or named");
            Assert.That(all, Does.Contain("S1-vs-S0 is kept"));
            Assert.That(all, Does.Contain("S4-vs-S1 is kept"));
            Assert.That(all, Does.Not.Contain("counted and not listed"));
        }

        [Test]
        public void DifferentTolerancesNameBoth()
        {
            ClashTestPlan plan = Plan(
                Test(DuctsVsColumns, Ducts, Columns, Mm25),
                Test(ColumnsVsDucts, Columns, Ducts, Mm75));

            string line = LineNaming(MirrorRule.Of(plan.Buildable, PriorityMap.NothingPicked()), ColumnsVsDucts);

            Assert.That(line, Does.Contain("tolerance 0.025 m"));
            Assert.That(line, Does.Contain("0.075 m"));
        }

        // A tolerance travels through a unit conversion, so two written a hair apart are one
        // tolerance, compared within TestDrift.ToleranceEpsilon and never exactly.
        [Test]
        public void TolerancesWithinTheEpsilonAreAlike()
        {
            ClashTestPlan plan = Plan(
                Test(DuctsVsColumns, Ducts, Columns, Mm25),
                Test(ColumnsVsDucts, Columns, Ducts, "0.08202099"));

            MirrorRule rule = MirrorRule.Of(plan.Buildable, PriorityMap.NothingPicked());

            Assert.That(plan.Buildable[0].Tolerance, Is.Not.EqualTo(plan.Buildable[1].Tolerance),
                "the two are written apart, by less than the epsilon");
            Assert.That(rule.Pairs.Count, Is.EqualTo(1));
            Assert.That(LineNaming(rule, ColumnsVsDucts), Does.Not.Contain("tolerance"));
        }

        [Test]
        public void DifferentPrioritiesNameBoth()
        {
            ClashTestPlan plan = Plan(
                Test(DuctsVsColumns, Ducts, Columns),
                Test(ColumnsVsDucts, Columns, Ducts));

            string line = LineNaming(
                MirrorRule.Of(plan.Buildable, Priorities(DuctsVsColumns, "A", ColumnsVsDucts, "B")), ColumnsVsDucts);

            Assert.That(line, Does.Contain("priority A and B"));
        }

        [Test]
        public void APriorityAgainstNoneNamesBoth()
        {
            ClashTestPlan plan = Plan(
                Test(DuctsVsColumns, Ducts, Columns),
                Test(ColumnsVsDucts, Columns, Ducts));

            string line = LineNaming(MirrorRule.Of(plan.Buildable, Priorities(DuctsVsColumns, "B")), ColumnsVsDucts);

            Assert.That(line, Does.Contain("priority B and No priority"));
        }

        [Test]
        public void DifferentTestTypesNameBoth()
        {
            ClashTestPlan plan = Plan(
                Test(DuctsVsColumns, Ducts, Columns),
                Test(ColumnsVsDucts, Columns, Ducts, Mm25, "hard"));

            string line = LineNaming(MirrorRule.Of(plan.Buildable, PriorityMap.NothingPicked()), ColumnsVsDucts);

            Assert.That(line, Does.Contain("type hard_conservative and hard"));
        }

        [Test]
        public void APairAlikeInPriorityTypeAndToleranceNamesNoDifference()
        {
            ClashTestPlan plan = Plan(
                Test(DuctsVsColumns, Ducts, Columns),
                Test(ColumnsVsDucts, Columns, Ducts));

            string line = LineNaming(
                MirrorRule.Of(plan.Buildable, Priorities(DuctsVsColumns, "A", ColumnsVsDucts, "A")), ColumnsVsDucts);

            Assert.That(line, Is.Not.Null);
            Assert.That(line, Does.Not.Contain("differ"));
        }

        // ---------- with no XML, the saved tests ----------

        [Test]
        public void TheSavedTestsArePairedTheSameWayInDocumentOrder()
        {
            ClashTestPlan plan = ClashTestPlan.FromDocument(
                new List<SavedClashTest>
                {
                    Saved(ColumnsVsDucts, Columns, Ducts, 0),
                    Saved(DuctsVsColumns, Ducts, Columns, 1)
                },
                "m");

            MirrorRule rule = MirrorRule.Of(plan.Buildable, PriorityMap.NothingPicked());

            Assert.That(rule.Pairs.Count, Is.EqualTo(1));
            Assert.That(rule.Pairs[0].Kept.Name, Is.EqualTo(ColumnsVsDucts));
            Assert.That(rule.Pairs[0].Mirror.Name, Is.EqualTo(DuctsVsColumns));

            string all = Text(rule.Lines());
            string pair = LineNaming(rule, DuctsVsColumns);

            Assert.That(all, Does.Contain("no XML was picked"));
            Assert.That(pair, Does.Contain("not run"));
            Assert.That(pair, Does.Not.Contain("created"));
        }

        // The add-in reads a saved test off the document and not which sets its sides point
        // at, so it hands every saved test the same two placeholders. Read as sets, every
        // test would be a duplicate of the first. A placeholder is UNKNOWN, so no test is
        // paired, left out or named a duplicate, and that is said once with the count.
        [Test]
        public void ThePlaceholderSidesOfSavedTestsAreUnknownAndSaidOnce()
        {
            ClashTestPlan plan = ClashTestPlan.FromDocument(
                new List<SavedClashTest>
                {
                    Saved(DuctsVsColumns, SavedClashTest.LeftAsSaved, SavedClashTest.RightAsSaved, 0),
                    Saved(ColumnsVsDucts, SavedClashTest.LeftAsSaved, SavedClashTest.RightAsSaved, 1),
                    Saved("BLD-ME-Ducts-vs-BLD-AR-Walls", SavedClashTest.LeftAsSaved, SavedClashTest.RightAsSaved, 2)
                },
                "m");

            MirrorRule rule = MirrorRule.Of(plan.Buildable, PriorityMap.NothingPicked());
            IList<string> lines = rule.Lines();

            Assert.That(rule.Pairs.Count, Is.EqualTo(0));
            Assert.That(lines.Count, Is.EqualTo(2), "the count and the one line saying UNKNOWN, no duplicate");
            Assert.That(lines[0], Is.EqualTo(MirrorRule.Prefix
                + "   0 pairs among the 0 tests whose two sets were read, so no test is left out"));
            Assert.That(lines[1], Is.EqualTo(MirrorRule.Prefix
                + "   3 of the 3 tests have a side whose set was not read, so whether each is a mirror "
                + "or a duplicate is UNKNOWN and none of them is left out"));
        }

        // One test whose side was not read among tests whose sides were. The pair is still
        // found, and the one not read is said once and counted apart.
        [Test]
        public void OneTestNotReadAmongTestsReadIsSaidOnce()
        {
            ClashTestPlan plan = ClashTestPlan.FromDocument(
                new List<SavedClashTest>
                {
                    Saved(DuctsVsColumns, Ducts, Columns, 0),
                    Saved(ColumnsVsDucts, Columns, Ducts, 1),
                    Saved("BLD-ME-Ducts-vs-BLD-AR-Walls", Ducts, TestSettings.UnknownLocator, 2)
                },
                "m");

            MirrorRule rule = MirrorRule.Of(plan.Buildable, PriorityMap.NothingPicked());
            IList<string> lines = rule.Lines();
            int sayingNotRead = 0;

            foreach (string line in lines)
            {
                if (line.Contains("not read"))
                {
                    sayingNotRead++;
                }
            }

            Assert.That(rule.Pairs.Count, Is.EqualTo(1));
            Assert.That(lines[0], Does.Contain(
                "1 pair of tests with the same two sets swapped among the 2 tests whose two sets were read"));
            Assert.That(sayingNotRead, Is.EqualTo(1));
            Assert.That(Text(lines), Does.Contain(
                "1 of the 3 tests has a side whose set was not read, so whether it is a mirror "
                + "or a duplicate is UNKNOWN and it is not left out"));
        }

        // ---------- the plan: the mirror is not created and not run ----------

        [Test]
        public void TheMirrorIsSkippedByNameAndTheKeptTestStays()
        {
            ClashTestPlan plan = Plan(
                Test(DuctsVsColumns, Ducts, Columns),
                Test(ColumnsVsDucts, Columns, Ducts));

            ClashTestPlan without = plan.WithoutMirrors(MirrorRule.Of(plan.Buildable, PriorityMap.NothingPicked()));

            Assert.That(without.Buildable.Count, Is.EqualTo(1));
            Assert.That(without.Buildable[0].Name, Is.EqualTo(DuctsVsColumns));
            Assert.That(without.TestsInFile, Is.EqualTo(2));
            Assert.That(without.Skipped.Count, Is.EqualTo(1));

            SkippedClashTest mirror = without.Skipped[0];

            Assert.That(mirror.Name, Is.EqualTo(ColumnsVsDucts));
            Assert.That(mirror.Kind, Is.EqualTo(ClashSkipReason.Mirror));
            Assert.That(mirror.Reason, Does.StartWith("a mirror of " + DuctsVsColumns));
            Assert.That(mirror.FileIndex, Is.EqualTo(1));
        }

        [Test]
        public void AMirrorIsNeverHandedToTheCreationPlan()
        {
            ClashTestPlan plan = Plan(
                Test(DuctsVsColumns, Ducts, Columns),
                Test(ColumnsVsDucts, Columns, Ducts));

            ClashTestPlan without = plan.WithoutMirrors(MirrorRule.Of(plan.Buildable, PriorityMap.NothingPicked()));
            CreationPlan creation = CreationPlan.For(
                without.Buildable, new Dictionary<string, int> { { Ducts, 5 }, { Columns, 7 } });

            Assert.That(creation.CreateCount, Is.EqualTo(1));
            Assert.That(creation.Create[0].Name, Is.EqualTo(DuctsVsColumns));
        }

        [Test]
        public void ASavedMirrorIsNotRun()
        {
            ClashTestPlan plan = ClashTestPlan.FromDocument(
                new List<SavedClashTest>
                {
                    Saved(DuctsVsColumns, Ducts, Columns, 0),
                    Saved(ColumnsVsDucts, Columns, Ducts, 1)
                },
                "m");

            ClashTestPlan without = plan.WithoutMirrors(MirrorRule.Of(plan.Buildable, PriorityMap.NothingPicked()));

            Assert.That(without.Buildable.Count, Is.EqualTo(1));
            Assert.That(without.Buildable[0].Name, Is.EqualTo(DuctsVsColumns));
            Assert.That(without.Skipped[0].Kind, Is.EqualTo(ClashSkipReason.Mirror));
        }

        // The rule is matched to the plan's tests as the same objects. A rule built over a
        // second read of the same tests would move nothing while its lines said each mirror
        // is not run, so it is refused.
        [Test]
        public void ARuleBuiltOverAnotherReadOfTheTestsIsRefused()
        {
            List<SavedClashTest> saved = new List<SavedClashTest>
            {
                Saved(DuctsVsColumns, Ducts, Columns, 0),
                Saved(ColumnsVsDucts, Columns, Ducts, 1)
            };

            ClashTestPlan plan = ClashTestPlan.FromDocument(saved, "m");
            MirrorRule overAnotherRead = MirrorRule.Of(
                ClashTestPlan.FromDocument(saved, "m").Buildable, PriorityMap.NothingPicked());

            Assert.That(overAnotherRead.Pairs.Count, Is.EqualTo(1));
            Assert.Throws<ArgumentException>(() => plan.WithoutMirrors(overAnotherRead));
        }

        // A reason missing from the skip block's order is counted and never said, so a
        // mirror would vanish from the CLASH block while its count stayed in the total.
        [Test]
        public void AMirrorReachesTheSkipBlock()
        {
            ClashRunOutcome outcome = new ClashRunOutcome();
            outcome.TestsInFile = 1;
            outcome.AddSkipped(ColumnsVsDucts, ClashSkipReason.Mirror, "a mirror of " + DuctsVsColumns);

            Assert.That(outcome.SkipLines(), Does.Contain("SKIPPED 1 test, " + ClashTestPlan.Describe(ClashSkipReason.Mirror)));
            Assert.That(outcome.SkipLines(), Does.Contain("        " + ColumnsVsDucts + "  a mirror of " + DuctsVsColumns));
            Assert.That(outcome.Lines(), Does.Contain("        1  " + ClashTestPlan.Describe(ClashSkipReason.Mirror)));
        }

        // ---------- the picked XML's own samples, measured with no pair ----------

        private static PriorityMap TheSamplePriorities()
        {
            string path = Samples.PriorityMap();
            return PriorityMap.Read(File.ReadAllText(path), path);
        }

        private static ClashTestPlan PlanOf(string path)
        {
            return ClashTestPlan.From(new ExchangeReader().ReadFile(path), "m");
        }

        // The client's matrix holds 1830 tests and no test is another's mirror, so the rule
        // drops nothing. The tests are counted first, because nought pairs out of nought
        // tests proves nothing.
        [Test]
        public void TheClientsMatrixHoldsNoPair()
        {
            ClashTestPlan plan = PlanOf(Samples.Matrix());
            MirrorRule rule = MirrorRule.Of(plan.Buildable, TheSamplePriorities());

            Assert.That(plan.Buildable.Count, Is.EqualTo(1830));
            Assert.That(rule.Pairs.Count, Is.EqualTo(0));
            Assert.That(rule.Lines().Count, Is.EqualTo(1), "no pair and no duplicate is one line");
            Assert.That(plan.WithoutMirrors(rule).Buildable.Count, Is.EqualTo(1830));
        }

        [Test]
        public void TheCorrectedMatrixHoldsNoPair()
        {
            ClashTestPlan plan = PlanOf(Samples.CorrectedMatrix());
            MirrorRule rule = MirrorRule.Of(plan.Buildable, TheSamplePriorities());

            Assert.That(plan.Buildable.Count, Is.EqualTo(1830));
            Assert.That(rule.Pairs.Count, Is.EqualTo(0));
            Assert.That(rule.Lines().Count, Is.EqualTo(1), "no pair and no duplicate is one line");
            Assert.That(plan.WithoutMirrors(rule).Buildable.Count, Is.EqualTo(1830));
            Assert.That(plan.WithoutMirrors(rule).Skipped.Count, Is.EqualTo(plan.Skipped.Count));
        }

        // The same matrix with one test's swap added is exactly one pair, so the nought
        // above is the rule reading the file and not the rule reading nothing.
        [Test]
        public void OneSwapAddedToTheCorrectedMatrixIsTheOnePair()
        {
            ClashTestPlan plan = PlanOf(Samples.CorrectedMatrix());
            PlannedClashTest first = plan.Buildable[0];
            PlannedClashTest swapped = new PlannedClashTest(
                "the swap of the first test",
                first.TestType,
                first.TestTypeName,
                first.ToleranceInFileUnits,
                first.FileUnits,
                first.Tolerance,
                first.DocumentUnits,
                first.MergeComposites,
                first.Right,
                first.Left,
                plan.TestsInFile);

            List<PlannedClashTest> tests = new List<PlannedClashTest>(plan.Buildable);
            tests.Add(swapped);

            MirrorRule rule = MirrorRule.Of(tests, TheSamplePriorities());

            Assert.That(rule.Pairs.Count, Is.EqualTo(1));
            Assert.That(rule.Pairs[0].Kept.Name, Is.EqualTo(first.Name));
            Assert.That(rule.Pairs[0].Mirror.Name, Is.EqualTo("the swap of the first test"));
        }
    }
}
