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
    /// F132, FR-182, Bader's Q114 points 4 to 6 and 8, his answer B to Q121 and his answer D
    /// to Q133. A test whose two sides are another test's two sets swapped, or whose sets
    /// carry the same rule lists as another test's, is its mirror. Both are created and run,
    /// the mirror under its name with the ending (mirror), and their clashes are merged into
    /// the one kept, the higher priority, A before B before C, and where equal the one first
    /// in the XML. The set and test names in here are sample data and not settings, and
    /// nothing in the rule names any of them.
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

        private const string Category = "LcRevitPropertyElementCategory";

        internal const string Telecom = Root + "/Electrical/BLD-EL-Telecom Fixtures";
        internal const string Telephone = Root + "/Electrical/BLD-EL-Telephone Devices";

        /// <summary>
        /// The selection sets of an XML, given as a locator then the category its one rule
        /// asks for, again and again, so two sets given one category carry one rule list.
        /// </summary>
        internal static IList<SelectionSetDefinition> TheSets(params string[] locatorThenCategory)
        {
            StringBuilder body = new StringBuilder();

            for (int i = 0; i + 1 < locatorThenCategory.Length; i += 2)
            {
                string[] parts = locatorThenCategory[i].Split('/');
                string set = "<selectionset name=\"" + parts[parts.Length - 1] + "\"><findspec mode=\"all\" disjoint=\"0\">"
                    + "<conditions>" + (locatorThenCategory[i + 1] == null ? string.Empty
                        : "<condition test=\"equals\" flags=\"0\">"
                            + "<category><name internal=\"LcRevitData_Element\">Element</name></category>"
                            + "<property><name internal=\"" + Category + "\">Category</name></property>"
                            + "<value><data type=\"wstring\">" + locatorThenCategory[i + 1] + "</data></value></condition>")
                    + "</conditions></findspec></selectionset>";

                for (int folder = parts.Length - 2; folder >= 1; folder--)
                {
                    set = "<viewfolder name=\"" + parts[folder] + "\">" + set + "</viewfolder>";
                }

                body.Append(set);
            }

            return new ExchangeReader().ReadText(
                "<exchange units=\"ft\"><selectionsets>" + body + "</selectionsets></exchange>").Sets;
        }

        /// <summary>The sets these tests name, each with a rule list of its own but the two that share one.</summary>
        private static IList<SelectionSetDefinition> TheUsualSets()
        {
            return TheSets(
                Ducts, "Ducts",
                Columns, "Structural Columns",
                ArColumns, "Columns",
                Walls, "Walls",
                Telecom, "Telephone Devices",
                Telephone, "Telephone Devices");
        }

        /// <summary>The rule over tests of an XML that holds the usual sets, with the default settings.</summary>
        internal static MirrorRule Rule(IEnumerable<PlannedClashTest> tests, PriorityMap priorities)
        {
            return MirrorRule.Of(tests, priorities, TheUsualSets(), new MirrorSettings());
        }

        /// <summary>The rule over tests saved in the document, with no XML and so no rule list read.</summary>
        private static MirrorRule SavedRule(IEnumerable<PlannedClashTest> tests)
        {
            return MirrorRule.Of(tests, PriorityMap.NothingPicked(), null, new MirrorSettings());
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

            MirrorRule rule = Rule(plan.Buildable, PriorityMap.NothingPicked());

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

            Assert.That(Rule(plan.Buildable, PriorityMap.NothingPicked()).Pairs.Count, Is.EqualTo(0));
        }

        [Test]
        public void ASetNameEndingInASpaceIsNotTheSameSet()
        {
            ClashTestPlan plan = Plan(
                Test(DuctsVsColumns, Ducts, Columns),
                Test("BLD-ST-Columns -vs-BLD-ME-Ducts", Columns + " ", Ducts));

            Assert.That(plan.Buildable[1].Left.Locator, Is.EqualTo(Columns + " "),
                "the reader kept the space, so the rule is handed it");
            Assert.That(Rule(plan.Buildable, PriorityMap.NothingPicked()).Pairs.Count, Is.EqualTo(0));
        }

        [Test]
        public void ATestWithOneSetOnBothSidesIsNeverAMirror()
        {
            ClashTestPlan plan = Plan(
                Test("BLD-ME-Ducts-vs-BLD-ME-Ducts", Ducts, Ducts),
                Test("BLD-ME-Ducts-vs-BLD-ME-Ducts again", Ducts, Ducts));

            MirrorRule rule = Rule(plan.Buildable, PriorityMap.NothingPicked());

            Assert.That(rule.Pairs.Count, Is.EqualTo(0));
            Assert.That(plan.WithMirrorsNamed(rule).Buildable[1].Name, Is.EqualTo("BLD-ME-Ducts-vs-BLD-ME-Ducts again"));
        }

        // ---------- two sets of one rule list, Q121 B ----------

        private const string TelecomVsWalls = "BLD-EL-Telecom Fixtures-vs-BLD-AR-Walls";
        private const string TelephoneVsWalls = "BLD-EL-Telephone Devices-vs-BLD-AR-Walls";
        private const string WallsVsTelephone = "BLD-AR-Walls-vs-BLD-EL-Telephone Devices";

        [Test]
        public void TwoSetsOfOneRuleListMakeTheirTestsAPair()
        {
            ClashTestPlan plan = Plan(
                Test(TelecomVsWalls, Telecom, Walls),
                Test(TelephoneVsWalls, Telephone, Walls));

            MirrorRule rule = Rule(plan.Buildable, PriorityMap.NothingPicked());

            Assert.That(rule.Pairs.Count, Is.EqualTo(1));
            Assert.That(rule.Pairs[0].Kind, Is.EqualTo(MirrorKind.SameRules));
            Assert.That(rule.Pairs[0].Kept.Name, Is.EqualTo(TelecomVsWalls));
            Assert.That(rule.Pairs[0].Mirror.Name, Is.EqualTo(TelephoneVsWalls));
        }

        [Test]
        public void TwoSetsOfOneRuleListPairTheOtherWayRoundToo()
        {
            ClashTestPlan plan = Plan(
                Test(TelecomVsWalls, Telecom, Walls),
                Test(WallsVsTelephone, Walls, Telephone));

            MirrorRule rule = Rule(plan.Buildable, PriorityMap.NothingPicked());

            Assert.That(rule.Pairs.Count, Is.EqualTo(1));
            Assert.That(rule.Pairs[0].Kind, Is.EqualTo(MirrorKind.SameRules));
            Assert.That(rule.Pairs[0].Mirror.Name, Is.EqualTo(WallsVsTelephone));
        }

        [Test]
        public void ThePriorityDecidesAcrossRuleListsAsAcrossSwaps()
        {
            ClashTestPlan plan = Plan(
                Test(TelecomVsWalls, Telecom, Walls),
                Test(TelephoneVsWalls, Telephone, Walls));

            MirrorRule rule = Rule(plan.Buildable, Priorities(TelecomVsWalls, "C", TelephoneVsWalls, "A"));

            Assert.That(rule.Pairs[0].Kept.Name, Is.EqualTo(TelephoneVsWalls));
            Assert.That(rule.Pairs[0].Mirror.Name, Is.EqualTo(TelecomVsWalls));
        }

        // The two sets of one rule list against each other ask what one set against itself
        // asks, so the test is its own swap. Swapped by name, the two tests still pair.
        [Test]
        public void TwoSetsOfOneRuleListOnOneTestAreItsOwnSwapAndStillPairSwappedByName()
        {
            string telecomVsTelephone = "BLD-EL-Telecom Fixtures-vs-BLD-EL-Telephone Devices";
            string telephoneVsTelecom = "BLD-EL-Telephone Devices-vs-BLD-EL-Telecom Fixtures";

            Assert.That(Rule(Plan(Test(telecomVsTelephone, Telecom, Telephone)).Buildable, PriorityMap.NothingPicked())
                .Pairs.Count, Is.EqualTo(0));

            MirrorRule rule = Rule(
                Plan(Test(telecomVsTelephone, Telecom, Telephone), Test(telephoneVsTelecom, Telephone, Telecom)).Buildable,
                PriorityMap.NothingPicked());

            Assert.That(rule.Pairs.Count, Is.EqualTo(1));
            Assert.That(rule.Pairs[0].Kind, Is.EqualTo(MirrorKind.Swapped));
            Assert.That(rule.Pairs[0].Mirror.Name, Is.EqualTo(telephoneVsTelecom));
        }

        // FindIdentical leaves out a set with no rule, so two sets with none are not alike.
        [Test]
        public void TwoSetsWithNoRuleAreNotOneRuleList()
        {
            ClashTestPlan plan = Plan(
                Test(TelecomVsWalls, Telecom, Walls),
                Test(TelephoneVsWalls, Telephone, Walls));

            MirrorRule rule = MirrorRule.Of(
                plan.Buildable, PriorityMap.NothingPicked(), TheSets(Telecom, null, Telephone, null, Walls, "Walls"),
                new MirrorSettings());

            Assert.That(rule.Pairs.Count, Is.EqualTo(0));
        }

        // With no XML no rule list was read, so only the same two sets swapped pair, and the
        // log says so rather than reading as a check of the rule lists.
        [Test]
        public void WithNoRuleListReadOnlySwapsPairAndTheLogSaysSo()
        {
            ClashTestPlan plan = Plan(
                Test(TelecomVsWalls, Telecom, Walls),
                Test(TelephoneVsWalls, Telephone, Walls));

            MirrorRule rule = MirrorRule.Of(plan.Buildable, PriorityMap.NothingPicked(), null, new MirrorSettings());

            Assert.That(rule.Pairs.Count, Is.EqualTo(0));
            Assert.That(rule.Lines(), Does.Contain(MirrorRule.Prefix
                + "   no rule list of a set was read, so only tests with the same two sets swapped are paired"));
        }

        [Test]
        public void ThePairLineNamesTheSetsOfOneRuleList()
        {
            ClashTestPlan plan = Plan(
                Test(TelecomVsWalls, Telecom, Walls),
                Test(TelephoneVsWalls, Telephone, Walls),
                Test(DuctsVsColumns, Ducts, Columns),
                Test(ColumnsVsDucts, Columns, Ducts));

            MirrorRule rule = Rule(plan.Buildable, PriorityMap.NothingPicked());
            IList<string> lines = rule.Lines();

            Assert.That(lines[0], Does.Contain(
                "2 pairs of tests that ask the same question among the 4 tests whose two sets were read, "
                    + "1 with the same two sets swapped and 1 whose sets carry the same rule lists"));
            Assert.That(LineNaming(rule, TelephoneVsWalls), Is.EqualTo(MirrorRule.Prefix + "   " + TelecomVsWalls
                + " is kept, " + TelephoneVsWalls + " is its mirror, its sets carry the same rule lists as "
                + TelecomVsWalls + "'s, BLD-EL-Telephone Devices as BLD-EL-Telecom Fixtures, created and run as "
                + TelephoneVsWalls + " (mirror)"));
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
                Assert.That(SavedRule(plan.Buildable).Pairs.Count, Is.EqualTo(0),
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
                MirrorRule rule = Rule(plan.Buildable, priorities);

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
                Assert.That(Rule(plan.Buildable, priorities).Pairs[0].Kept.Name, Is.EqualTo(ColumnsVsDucts));
            }
        }

        [Test]
        public void EqualLettersKeepTheFirstInTheXml()
        {
            PriorityMap priorities = Priorities(DuctsVsColumns, "C", ColumnsVsDucts, "C");
            ClashTestPlan plan = Plan(
                Test(ColumnsVsDucts, Columns, Ducts),
                Test(DuctsVsColumns, Ducts, Columns));

            MirrorRule rule = Rule(plan.Buildable, priorities);

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

            Assert.That(Rule(plan.Buildable, priorities).Pairs[0].Kept.Name, Is.EqualTo(ColumnsVsDucts));
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

            MirrorRule rule = Rule(plan.Buildable, PriorityMap.NothingPicked());

            Assert.That(rule.Pairs.Count, Is.EqualTo(1));
            Assert.That(rule.Pairs[0].Kept.Name, Is.EqualTo(DuctsVsColumns));
            Assert.That(rule.Pairs[0].Mirror.Name, Is.EqualTo(ColumnsVsDucts));

            string duplicate = LineNaming(rule, "Ducts against Columns again");

            Assert.That(duplicate, Is.Not.Null, "the duplicate is not named");
            Assert.That(duplicate, Does.Contain("duplicate"));
            Assert.That(duplicate, Does.Contain(DuctsVsColumns));

            List<string> stillRun = new List<string>();

            foreach (PlannedClashTest test in plan.WithMirrorsNamed(rule).Buildable)
            {
                stillRun.Add(test.Name);
            }

            Assert.That(stillRun.Count, Is.EqualTo(3));
            Assert.That(stillRun[0], Is.EqualTo(DuctsVsColumns));
            Assert.That(stillRun[1], Is.EqualTo("Ducts against Columns again"));
            Assert.That(stillRun[2], Is.EqualTo(ColumnsVsDucts + " (mirror)"));
        }

        // ---------- what the log says ----------

        [Test]
        public void NoPairIsOneLineSayingSo()
        {
            ClashTestPlan plan = Plan(
                Test(DuctsVsColumns, Ducts, Columns),
                Test("BLD-ME-Ducts-vs-BLD-AR-Walls", Ducts, Walls));

            IList<string> lines = Rule(plan.Buildable, PriorityMap.NothingPicked()).Lines();

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

            MirrorRule rule = Rule(plan.Buildable, PriorityMap.NothingPicked());
            IList<string> lines = rule.Lines();

            Assert.That(rule.Pairs.Count, Is.EqualTo(2));
            Assert.That(lines[0], Does.Contain("2 pairs"));

            string first = LineNaming(rule, ColumnsVsDucts);
            string second = LineNaming(rule, "BLD-AR-Walls-vs-BLD-ME-Ducts");

            Assert.That(first, Does.StartWith(MirrorRule.Prefix + " "));
            Assert.That(first, Does.Contain(DuctsVsColumns));
            Assert.That(first, Does.Contain("the same two sets swapped, created and run as " + ColumnsVsDucts + " (mirror)"));
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
            return Rule(Plan(first.ToArray()).Buildable, Priorities(nameThenLetter));
        }

        private static int LinesNamingAPair(MirrorRule rule)
        {
            int named = 0;

            foreach (string line in rule.Lines())
            {
                if (line.Contains(" is its mirror, "))
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

        // Four sets both ways round are six pairs, five named and one counted, in the singular.
        [Test]
        public void OneMorePairAlikeIsSaidInTheSingular()
        {
            MirrorRule rule = BothWaysRound(4);

            Assert.That(rule.Pairs.Count, Is.EqualTo(6));
            Assert.That(Text(rule.Lines()), Does.Contain(
                "and 1 more pair alike in priority, test type and tolerance, counted and not listed"));
        }

        // Six duplicates of one test, five named and one counted, in the singular.
        [Test]
        public void OneMoreDuplicateIsSaidInTheSingular()
        {
            List<string> tests = new List<string> { Test(DuctsVsColumns, Ducts, Columns) };

            for (int i = 1; i <= 6; i++)
            {
                tests.Add(Test(DuctsVsColumns + " copy " + i, Ducts, Columns));
            }

            MirrorRule rule = Rule(Plan(tests.ToArray()).Buildable, PriorityMap.NothingPicked());

            Assert.That(Text(rule.Lines()), Does.Contain("and 1 more duplicate, counted and not listed"));
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

            string line = LineNaming(Rule(plan.Buildable, PriorityMap.NothingPicked()), ColumnsVsDucts);

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

            MirrorRule rule = Rule(plan.Buildable, PriorityMap.NothingPicked());

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
                Rule(plan.Buildable, Priorities(DuctsVsColumns, "A", ColumnsVsDucts, "B")), ColumnsVsDucts);

            Assert.That(line, Does.Contain("priority A and B"));
        }

        [Test]
        public void APriorityAgainstNoneNamesBoth()
        {
            ClashTestPlan plan = Plan(
                Test(DuctsVsColumns, Ducts, Columns),
                Test(ColumnsVsDucts, Columns, Ducts));

            string line = LineNaming(Rule(plan.Buildable, Priorities(DuctsVsColumns, "B")), ColumnsVsDucts);

            Assert.That(line, Does.Contain("priority B and No priority"));
        }

        [Test]
        public void DifferentTestTypesNameBoth()
        {
            ClashTestPlan plan = Plan(
                Test(DuctsVsColumns, Ducts, Columns),
                Test(ColumnsVsDucts, Columns, Ducts, Mm25, "hard"));

            string line = LineNaming(Rule(plan.Buildable, PriorityMap.NothingPicked()), ColumnsVsDucts);

            Assert.That(line, Does.Contain("type hard_conservative and hard"));
        }

        [Test]
        public void APairAlikeInPriorityTypeAndToleranceNamesNoDifference()
        {
            ClashTestPlan plan = Plan(
                Test(DuctsVsColumns, Ducts, Columns),
                Test(ColumnsVsDucts, Columns, Ducts));

            string line = LineNaming(
                Rule(plan.Buildable, Priorities(DuctsVsColumns, "A", ColumnsVsDucts, "A")), ColumnsVsDucts);

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

            MirrorRule rule = SavedRule(plan.Buildable);

            Assert.That(rule.Pairs.Count, Is.EqualTo(1));
            Assert.That(rule.Pairs[0].Kept.Name, Is.EqualTo(ColumnsVsDucts));
            Assert.That(rule.Pairs[0].Mirror.Name, Is.EqualTo(DuctsVsColumns));

            string all = Text(rule.Lines());
            string pair = LineNaming(rule, DuctsVsColumns + " is its mirror");

            Assert.That(all, Does.Contain("no XML was picked"));
            Assert.That(pair, Does.Contain("run as it is saved"));
            Assert.That(pair, Does.Not.Contain("created"));
        }

        // The add-in reads a saved test off the document and not which sets its sides point
        // at, so it hands every saved test the same two placeholders. Read as sets, every
        // test would be a duplicate of the first. A placeholder is UNKNOWN, so no test is
        // paired, merged or named a duplicate, and that is said once with the count.
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

            MirrorRule rule = SavedRule(plan.Buildable);
            IList<string> lines = rule.Lines();

            Assert.That(rule.Pairs.Count, Is.EqualTo(0));
            Assert.That(lines.Count, Is.EqualTo(3), "the count, no rule list read, and the one line saying UNKNOWN");
            Assert.That(lines[0], Is.EqualTo(MirrorRule.Prefix
                + "   0 pairs among the 0 tests whose two sets were read, so every test keeps its own clashes"));
            Assert.That(lines[1], Is.EqualTo(MirrorRule.Prefix
                + "   no rule list of a set was read, so only tests with the same two sets swapped are paired"));
            Assert.That(lines[2], Is.EqualTo(MirrorRule.Prefix
                + "   3 of the 3 tests have a side whose set was not read, so whether each is a mirror "
                + "or a duplicate is UNKNOWN and none of their clashes is merged"));
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

            MirrorRule rule = SavedRule(plan.Buildable);
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
                "1 pair of tests that ask the same question among the 2 tests whose two sets were read, "
                    + "1 with the same two sets swapped and 0 whose sets carry the same rule lists"));
            Assert.That(sayingNotRead, Is.EqualTo(1));
            Assert.That(Text(lines), Does.Contain(
                "1 of the 3 tests has a side whose set was not read, so whether it is a mirror "
                + "or a duplicate is UNKNOWN and its clashes are not merged"));
        }

        // ---------- the names the coverage sheet gives a pair, for F127 ----------

        private static string Words(MirrorRule rule, string test)
        {
            foreach (KeyValuePair<string, string> named in rule.CoverageNames())
            {
                if (named.Key == test)
                {
                    return named.Value;
                }
            }

            return null;
        }

        [Test]
        public void TheCoverageSheetNamesBothTestsOfAPair()
        {
            ClashTestPlan plan = Plan(
                Test(DuctsVsColumns, Ducts, Columns),
                Test(ColumnsVsDucts, Columns, Ducts));

            MirrorRule rule = Rule(plan.Buildable, PriorityMap.NothingPicked());

            Assert.That(rule.CoverageNames().Count, Is.EqualTo(2));
            Assert.That(Words(rule, DuctsVsColumns), Is.EqualTo(
                "kept of a mirrored pair, the clashes only its mirror " + ColumnsVsDucts + " (mirror) finds are added to it"));
            Assert.That(Words(rule, ColumnsVsDucts), Is.EqualTo(
                "a mirror of " + DuctsVsColumns + ", the same two sets swapped, created and run as " + ColumnsVsDucts
                    + " (mirror), its clashes merged into " + DuctsVsColumns + "'s"));
        }

        [Test]
        public void TheCoverageSheetNamesTheSetsOfOneRuleList()
        {
            ClashTestPlan plan = Plan(
                Test(TelecomVsWalls, Telecom, Walls),
                Test(TelephoneVsWalls, Telephone, Walls));

            Assert.That(Words(Rule(plan.Buildable, PriorityMap.NothingPicked()), TelephoneVsWalls), Is.EqualTo(
                "a mirror of " + TelecomVsWalls + ", its sets carry the same rule lists as " + TelecomVsWalls
                    + "'s, BLD-EL-Telephone Devices as BLD-EL-Telecom Fixtures, created and run as " + TelephoneVsWalls
                    + " (mirror), its clashes merged into " + TelecomVsWalls + "'s"));
        }

        // A test kept over two mirrors is one row of the sheet, and the sheet's rows come in
        // the order of the XML, whichever of a pair is kept.
        [Test]
        public void ATestKeptOverTwoMirrorsIsNamedOnceAndTheNamesComeInTheXmlsOrder()
        {
            ClashTestPlan plan = Plan(
                Test(TelephoneVsWalls, Telephone, Walls),
                Test(WallsVsTelephone, Walls, Telephone),
                Test(TelecomVsWalls, Telecom, Walls));

            MirrorRule rule = Rule(plan.Buildable, Priorities(TelecomVsWalls, "A"));
            IList<KeyValuePair<string, string>> names = rule.CoverageNames();

            Assert.That(names.Count, Is.EqualTo(3));
            Assert.That(names[0].Key, Is.EqualTo(TelephoneVsWalls));
            Assert.That(names[1].Key, Is.EqualTo(WallsVsTelephone));
            Assert.That(names[2].Key, Is.EqualTo(TelecomVsWalls));
            Assert.That(names[2].Value, Is.EqualTo("kept of 2 mirrored pairs, the clashes only its mirrors "
                + TelephoneVsWalls + " (mirror) and " + WallsVsTelephone + " (mirror) find are added to it"));
        }

        [Test]
        public void ASavedMirrorIsNamedAsRunAsItIsSaved()
        {
            ClashTestPlan plan = ClashTestPlan.FromDocument(
                new List<SavedClashTest>
                {
                    Saved(DuctsVsColumns, Ducts, Columns, 0),
                    Saved(ColumnsVsDucts, Columns, Ducts, 1)
                },
                "m");

            Assert.That(Words(SavedRule(plan.Buildable), ColumnsVsDucts), Is.EqualTo(
                "a mirror of " + DuctsVsColumns + ", the same two sets swapped, run as it is saved, its clashes merged into "
                    + DuctsVsColumns + "'s"));
        }

        [Test]
        public void NoPairNamesNoTest()
        {
            ClashTestPlan plan = Plan(
                Test(DuctsVsColumns, Ducts, Columns),
                Test("BLD-ME-Ducts-vs-BLD-AR-Walls", Ducts, Walls));

            Assert.That(Rule(plan.Buildable, PriorityMap.NothingPicked()).CoverageNames(), Is.Empty);
        }

        // ---------- the picked XML's own samples ----------

        private static PriorityMap TheSamplePriorities()
        {
            string path = Samples.PriorityMap();
            return PriorityMap.Read(File.ReadAllText(path), path);
        }

        private static ExchangeDocument Read(string path)
        {
            return new ExchangeReader().ReadFile(path);
        }

        private static int OfKind(MirrorRule rule, MirrorKind kind)
        {
            int count = 0;

            foreach (MirrorPair pair in rule.Pairs)
            {
                if (pair.Kind == kind)
                {
                    count++;
                }
            }

            return count;
        }

        /// <summary>Every pair keeps the higher priority, and where equal the one first in the XML, Q114 point 6.</summary>
        private static void EachPairKeepsByQ114Point6(MirrorRule rule, PriorityMap priorities)
        {
            foreach (MirrorPair pair in rule.Pairs)
            {
                int kept = Federator.Core.Clash.Priorities.Order(priorities.Of(pair.Kept.Name));
                int mirror = Federator.Core.Clash.Priorities.Order(priorities.Of(pair.Mirror.Name));

                Assert.That(kept < mirror || (kept == mirror && pair.Kept.FileIndex < pair.Mirror.FileIndex), Is.True,
                    pair.Kept.Name + " kept over " + pair.Mirror.Name);
            }
        }

        // The client's matrix holds 1830 tests and no two of them are the same two sets
        // swapped. Its 61 sets carry 59 rule lists, Telecom Fixtures with Telephone Devices
        // and Electrical Fixtures with Devices, SetWarningsTests, so under Q121 B the tests of
        // each of those sets against the same other question pair: 57 for each pair against
        // the 57 sets of a rule list of their own, and 3 of the 4 tests between the two
        // pairs, 117 in all. The tests are counted first, because nought out of nought
        // proves nothing.
        [Test]
        public void TheClientsMatrixHoldsNoSwapAnd117PairsByRuleList()
        {
            ExchangeDocument matrix = Read(Samples.Matrix());
            ClashTestPlan plan = ClashTestPlan.From(matrix, "m");
            PriorityMap priorities = TheSamplePriorities();
            MirrorRule rule = MirrorRule.Of(plan.Buildable, priorities, matrix.Sets, new MirrorSettings());

            Assert.That(plan.Buildable.Count, Is.EqualTo(1830));
            Assert.That(OfKind(rule, MirrorKind.Swapped), Is.EqualTo(0));
            Assert.That(OfKind(rule, MirrorKind.SameRules), Is.EqualTo(117));
            EachPairKeepsByQ114Point6(rule, priorities);
        }

        // The corrected matrix, where Devices no longer asks what Electrical Fixtures asks,
        // F87: one pair of sets of one rule list, so 59 pairs of tests, the 59 measured in
        // turn5\measure-mirrors.md, and every test is still created and run, 59 of them
        // under the name with the ending.
        [Test]
        public void TheCorrectedMatrixHolds59PairsByRuleListAndEveryTestStillRuns()
        {
            ExchangeDocument matrix = Read(Samples.CorrectedMatrix());
            ClashTestPlan plan = ClashTestPlan.From(matrix, "m");
            PriorityMap priorities = TheSamplePriorities();
            MirrorRule rule = MirrorRule.Of(plan.Buildable, priorities, matrix.Sets, new MirrorSettings());
            ClashTestPlan named = plan.WithMirrorsNamed(rule);
            int ending = 0;

            foreach (PlannedClashTest test in named.Buildable)
            {
                if (test.Name.EndsWith(" (mirror)", StringComparison.Ordinal))
                {
                    ending++;
                }
            }

            Assert.That(plan.Buildable.Count, Is.EqualTo(1830));
            Assert.That(OfKind(rule, MirrorKind.Swapped), Is.EqualTo(0));
            Assert.That(OfKind(rule, MirrorKind.SameRules), Is.EqualTo(59));
            Assert.That(named.Buildable.Count, Is.EqualTo(1830));
            Assert.That(named.Skipped.Count, Is.EqualTo(plan.Skipped.Count));
            Assert.That(ending, Is.EqualTo(59));
            EachPairKeepsByQ114Point6(rule, priorities);

            foreach (MirrorPair pair in rule.Pairs)
            {
                bool telecom = pair.Mirror.Left.Locator.EndsWith("/BLD-EL-Telecom Fixtures", StringComparison.Ordinal)
                    || pair.Mirror.Right.Locator.EndsWith("/BLD-EL-Telecom Fixtures", StringComparison.Ordinal)
                    || pair.Mirror.Left.Locator.EndsWith("/BLD-EL-Telephone Devices", StringComparison.Ordinal)
                    || pair.Mirror.Right.Locator.EndsWith("/BLD-EL-Telephone Devices", StringComparison.Ordinal);

                Assert.That(telecom, Is.True, pair.Mirror.Name + " names neither set of the one rule list");
            }
        }

        // The same matrix with one test's swap added is exactly one pair more, of the swapped
        // kind, so the nought above is the rule reading the file and not the rule reading nothing.
        [Test]
        public void OneSwapAddedToTheCorrectedMatrixIsTheOneSwappedPair()
        {
            ExchangeDocument matrix = Read(Samples.CorrectedMatrix());
            ClashTestPlan plan = ClashTestPlan.From(matrix, "m");
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

            MirrorRule rule = MirrorRule.Of(tests, TheSamplePriorities(), matrix.Sets, new MirrorSettings());
            MirrorPair theSwap = null;

            foreach (MirrorPair pair in rule.Pairs)
            {
                if (pair.Kind == MirrorKind.Swapped)
                {
                    theSwap = pair;
                }
            }

            Assert.That(OfKind(rule, MirrorKind.Swapped), Is.EqualTo(1));
            Assert.That(theSwap.Kept.Name, Is.EqualTo(first.Name));
            Assert.That(theSwap.Mirror.Name, Is.EqualTo("the swap of the first test"));
        }
    }
}
