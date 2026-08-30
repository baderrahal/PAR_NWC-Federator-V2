using System;
using System.Collections.Generic;
using Federator.Core.Exchange;
using Federator.Core.Sets;
using NUnit.Framework;

namespace Federator.Core.Tests
{
    /// <summary>
    /// Everything here is sample data from the files that happen to be to hand. Nothing
    /// about any one project is in the code under test, so these names, folder names and
    /// counts are examples and not a contract.
    /// </summary>
    [TestFixture]
    public class SetBuildPlanTests
    {
        private static ExchangeDocument Read(string xml)
        {
            return new ExchangeReader().ReadText(xml);
        }

        private static string SetsXml(string body)
        {
            return "<exchange units=\"ft\"><selectionsets>" + body + "</selectionsets></exchange>";
        }

        private static string Condition(
            string test, string flags, string categoryInternal, string categoryDisplay,
            string propertyInternal, string propertyDisplay, string value)
        {
            string category = categoryInternal == null
                ? string.Empty
                : "<category><name internal=\"" + categoryInternal + "\">" + categoryDisplay + "</name></category>";

            return "<condition test=\"" + test + "\" flags=\"" + flags + "\">"
                + category
                + "<property><name internal=\"" + propertyInternal + "\">" + propertyDisplay + "</name></property>"
                + "<value><data type=\"wstring\">" + value + "</data></value>"
                + "</condition>";
        }

        private static string Set(string name, string conditions)
        {
            return "<selectionset name=\"" + name + "\"><findspec mode=\"all\" disjoint=\"0\">"
                + "<conditions>" + conditions + "</conditions><locator>/</locator></findspec></selectionset>";
        }

        // ---------- the folder tree ----------

        [Test]
        public void FoldersRebuildToTheDepthTheFileUsesAndAreNeverFlattened()
        {
            ExchangeDocument document = Read(SetsXml(
                "<viewfolder name=\"Mechanical\">"
                + "  <viewfolder name=\"Mechanical-HVAC\">"
                + Set("Air Terminals", Condition("equals", "0", "LcRevitData_Element", "Element",
                        "LcRevitPropertyElementCategory", "Category", "Air Terminals"))
                + "  </viewfolder>"
                + "</viewfolder>"));

            SetBuildPlan plan = SetBuildPlan.From(document);

            Assert.That(plan.Buildable.Count, Is.EqualTo(1));
            Assert.That(plan.Buildable[0].Folders, Is.EqualTo(new[] { "Mechanical", "Mechanical-HVAC" }));
            Assert.That(plan.Buildable[0].Name, Is.EqualTo("Air Terminals"),
                "the folder names leaked into the set name");
            Assert.That(plan.DeepestFolderDepth(), Is.EqualTo(2));
        }

        [Test]
        public void FolderPathsComeBackParentsBeforeChildrenWithNoRepeats()
        {
            ExchangeDocument document = Read(SetsXml(
                "<viewfolder name=\"A\">"
                + "<viewfolder name=\"B\">"
                + "<viewfolder name=\"C\">"
                + Set("Deep", Condition("equals", "0", "Cat", "Cat", "Prop", "Prop", "x"))
                + "</viewfolder>"
                + Set("Shallower", Condition("equals", "0", "Cat", "Cat", "Prop", "Prop", "y"))
                + "</viewfolder>"
                + "</viewfolder>"));

            IList<IList<string>> paths = SetBuildPlan.From(document).FolderPaths();
            List<string> joined = new List<string>();

            foreach (IList<string> path in paths)
            {
                joined.Add(string.Join("/", new List<string>(path).ToArray()));
            }

            Assert.That(joined, Is.EqualTo(new[] { "A", "A/B", "A/B/C" }));
        }

        [Test]
        public void ASetAtTheRootHasNoFolders()
        {
            ExchangeDocument document = Read(SetsXml(
                Set("Loose", Condition("equals", "0", "Cat", "Cat", "Prop", "Prop", "x"))));

            SetBuildPlan plan = SetBuildPlan.From(document);

            Assert.That(plan.Buildable[0].Folders.Count, Is.EqualTo(0));
            Assert.That(plan.DeepestFolderDepth(), Is.EqualTo(0));
            Assert.That(plan.FolderPaths().Count, Is.EqualTo(0));
        }

        // ---------- internal names ----------

        [Test]
        public void InternalNamesSurviveAndTheDisplayWordsAreKeptSeparately()
        {
            ExchangeDocument document = Read(SetsXml(
                Set("S", Condition("equals", "0",
                        "LcRevitData_Element", "Element",
                        "LcRevitPropertyElementCategory", "Category", "Walls"))));

            PlannedCondition condition = SetBuildPlan.From(document).Buildable[0].Conditions[0];

            Assert.That(condition.CategoryInternalName, Is.EqualTo("LcRevitData_Element"));
            Assert.That(condition.CategoryDisplayName, Is.EqualTo("Element"));
            Assert.That(condition.PropertyInternalName, Is.EqualTo("LcRevitPropertyElementCategory"));
            Assert.That(condition.PropertyDisplayName, Is.EqualTo("Category"));
            Assert.That(condition.Value, Is.EqualTo("Walls"));
            Assert.That(condition.ValueType, Is.EqualTo("wstring"));
        }

        // An internal name nobody has seen before is not a reason to stop. It goes to the
        // API as it is.
        [Test]
        public void AnInternalNameNeverSeenBeforeIsPassedThroughUntouched()
        {
            ExchangeDocument document = Read(SetsXml(
                Set("S", Condition("equals", "0",
                        "LcSomethingNobodyHasSeen", "Whatever",
                        "lcldrevit_parameter_-9999999", "Made Up", "value"))));

            SetBuildPlan plan = SetBuildPlan.From(document);

            Assert.That(plan.Skipped.Count, Is.EqualTo(0), "an unfamiliar internal name stopped a set");
            Assert.That(plan.Buildable[0].Conditions[0].CategoryInternalName,
                Is.EqualTo("LcSomethingNobodyHasSeen"));
            Assert.That(plan.Buildable[0].Conditions[0].PropertyInternalName,
                Is.EqualTo("lcldrevit_parameter_-9999999"));
        }

        // ---------- a missing category ----------

        [Test]
        public void AConditionWithNoCategoryIsPlannedWithoutAssumingOne()
        {
            ExchangeDocument document = Read(SetsXml(
                Set("S", Condition("contains", "0", null, null,
                        "LcOaNodeSourceFile", "Source File", "-AR-"))));

            SetBuildPlan plan = SetBuildPlan.From(document);
            PlannedCondition condition = plan.Buildable[0].Conditions[0];

            Assert.That(plan.Skipped.Count, Is.EqualTo(0));
            Assert.That(condition.HasCategory, Is.False);
            Assert.That(condition.CategoryInternalName, Is.Null);
            Assert.That(condition.CategoryDisplayName, Is.Null);
            Assert.That(condition.PropertyInternalName, Is.EqualTo("LcOaNodeSourceFile"));
            Assert.That(condition.Test, Is.EqualTo(ConditionTest.Contains));
        }

        // ---------- test values ----------

        [TestCase("equals", ConditionTest.Equals)]
        [TestCase("contains", ConditionTest.Contains)]
        public void BothTestValuesSeenSoFarAreHandled(string test, ConditionTest expected)
        {
            ExchangeDocument document = Read(SetsXml(
                Set("S", Condition(test, "0", "Cat", "Cat", "Prop", "Prop", "v"))));

            SetBuildPlan plan = SetBuildPlan.From(document);

            Assert.That(plan.Skipped.Count, Is.EqualTo(0));
            Assert.That(plan.Buildable[0].Conditions[0].Test, Is.EqualTo(expected));
        }

        [Test]
        public void AnUnknownTestValueIsReportedByNameAndItsSetSkipped()
        {
            ExchangeDocument document = Read(SetsXml(
                Set("Good", Condition("equals", "0", "Cat", "Cat", "Prop", "Prop", "v"))
                + Set("Bad", Condition("wildcard", "0", "Cat", "Cat", "Prop", "Prop", "v"))));

            SetBuildPlan plan = SetBuildPlan.From(document);

            Assert.That(plan.Buildable.Count, Is.EqualTo(1));
            Assert.That(plan.Buildable[0].Name, Is.EqualTo("Good"));

            Assert.That(plan.Skipped.Count, Is.EqualTo(1));
            Assert.That(plan.Skipped[0].Name, Is.EqualTo("Bad"));
            Assert.That(plan.Skipped[0].Reason, Does.Contain("wildcard"),
                "the reason does not name the offending test value");
            Assert.That(plan.UnknownTestValues, Is.EqualTo(new[] { "wildcard" }));
        }

        [Test]
        public void OneBadConditionSkipsItsOwnSetAndNothingElse()
        {
            ExchangeDocument document = Read(SetsXml(
                Set("Mixed",
                    Condition("equals", "0", "Cat", "Cat", "Prop", "Prop", "a")
                    + Condition("greaterthan", "64", "Cat", "Cat", "Prop", "Prop", "b"))
                + Set("Fine", Condition("contains", "0", "Cat", "Cat", "Prop", "Prop", "c"))));

            SetBuildPlan plan = SetBuildPlan.From(document);

            Assert.That(plan.Buildable.Count, Is.EqualTo(1));
            Assert.That(plan.Buildable[0].Name, Is.EqualTo("Fine"));
            Assert.That(plan.Skipped.Count, Is.EqualTo(1));
            Assert.That(plan.Skipped[0].Reason, Does.Contain("greaterthan"));
            Assert.That(plan.TotalSets, Is.EqualTo(2));
        }

        [Test]
        public void EachDistinctUnknownTestValueIsNamedOnce()
        {
            ExchangeDocument document = Read(SetsXml(
                Set("A", Condition("wildcard", "0", "C", "C", "P", "P", "v"))
                + Set("B", Condition("wildcard", "0", "C", "C", "P", "P", "v"))
                + Set("C", Condition("between", "0", "C", "C", "P", "P", "v"))));

            SetBuildPlan plan = SetBuildPlan.From(document);

            Assert.That(plan.Skipped.Count, Is.EqualTo(3));
            Assert.That(plan.UnknownTestValues, Is.EqualTo(new[] { "wildcard", "between" }));
        }

        // ---------- flags ----------

        // Read off the installed DLL on 2026-08-31: SearchConditionOptions is a Flags enum
        // over int and StartGroup is 64, so the file's flags attribute is that bitmask.
        // Core keeps it as the raw int because Core never references the Navisworks API.
        [Test]
        public void TheFlagsAttributeIsCarriedThroughUntouched()
        {
            ExchangeDocument document = Read(SetsXml(
                Set("S",
                    Condition("equals", "0", "C", "C", "P", "P", "a")
                    + Condition("equals", "64", "C", "C", "P", "P", "b"))));

            var conditions = SetBuildPlan.From(document).Buildable[0].Conditions;

            Assert.That(conditions[0].Flags, Is.EqualTo(0));
            Assert.That(conditions[1].Flags, Is.EqualTo(64));
        }

        // ---------- what a set asked for ----------

        [Test]
        public void AConditionDescribesItselfInInternalNames()
        {
            ExchangeDocument document = Read(SetsXml(
                Set("S", Condition("equals", "0",
                        "LcRevitData_Element", "Element",
                        "LcRevitPropertyElementCategory", "Category", "Roofs"))));

            PlannedCondition condition = SetBuildPlan.From(document).Buildable[0].Conditions[0];

            Assert.That(condition.Describe(),
                Is.EqualTo("LcRevitData_Element/LcRevitPropertyElementCategory equals \"Roofs\""));
            Assert.That(condition.Describe(), Does.Not.Contain("Element/Category"),
                "the display words must not be what gets reported as the question");
        }

        [Test]
        public void AConditionWithNoCategoryDescribesItselfWithoutInventingOne()
        {
            ExchangeDocument document = Read(SetsXml(
                Set("S", Condition("contains", "0", null, null,
                        "LcOaNodeSourceFile", "Source File", "-AR-"))));

            Assert.That(SetBuildPlan.From(document).Buildable[0].Conditions[0].Describe(),
                Is.EqualTo("LcOaNodeSourceFile contains \"-AR-\""));
        }

        [Test]
        public void ASetDescribesEveryConditionItHolds()
        {
            ExchangeDocument document = Read(SetsXml(
                Set("BLD-AR-Walls",
                    Condition("equals", "0", "LcRevitData_Element", "Element",
                        "LcRevitPropertyElementCategory", "Category", "Walls")
                    + Condition("contains", "0", null, null,
                        "LcOaNodeSourceFile", "Source File", "-AR-"))));

            Assert.That(SetBuildPlan.From(document).Buildable[0].Describe(),
                Is.EqualTo("LcRevitData_Element/LcRevitPropertyElementCategory equals \"Walls\""
                    + " and LcOaNodeSourceFile contains \"-AR-\""));
        }

        // ---------- a file with no sets ----------

        [Test]
        public void AFileWithNoSetsIsAcceptedAndGivesAnEmptyPlan()
        {
            ExchangeDocument document = Read(
                "<exchange units=\"ft\"><batchtest name=\"B\" units=\"ft\"><clashtests>"
                + "<clashtest name=\"T\" test_type=\"hard_conservative\" status=\"new\""
                + " tolerance=\"0.2460629921\" merge_composites=\"1\">"
                + "<left><clashselection selfintersect=\"0\" primtypes=\"1\">"
                + "<locator>lcop_selection_set_tree/A</locator></clashselection></left>"
                + "<right><clashselection selfintersect=\"0\" primtypes=\"1\">"
                + "<locator>lcop_selection_set_tree/B</locator></clashselection></right>"
                + "</clashtest></clashtests></batchtest></exchange>");

            SetBuildPlan plan = SetBuildPlan.From(document);

            Assert.That(document.Tests.Count, Is.EqualTo(1), "the tests half should still read");
            Assert.That(plan.Buildable.Count, Is.EqualTo(0));
            Assert.That(plan.Skipped.Count, Is.EqualTo(0));
            Assert.That(plan.HasWork, Is.False);
            Assert.That(plan.TotalSets, Is.EqualTo(0));
            Assert.That(plan.FolderPaths().Count, Is.EqualTo(0));
        }

        [Test]
        public void AnEmptySetListIsNotAnError()
        {
            SetBuildPlan plan = SetBuildPlan.From(new List<SelectionSetDefinition>());

            Assert.That(plan.HasWork, Is.False);
            Assert.That(plan.DeepestFolderDepth(), Is.EqualTo(0));
        }

        // ---------- against the real sample, as sample data only ----------

        [Test]
        public void TheReferenceFilePlansEverySetItHolds()
        {
            ExchangeDocument document = new ExchangeReader().ReadFile(Samples.AllInOne());
            SetBuildPlan plan = SetBuildPlan.From(document);

            Assert.That(plan.TotalSets, Is.EqualTo(document.Sets.Count));
            Assert.That(plan.Skipped.Count, Is.EqualTo(0),
                "the reference file uses only equals and contains, so nothing should be skipped");
            Assert.That(plan.Buildable.Count, Is.EqualTo(61));
            Assert.That(plan.DeepestFolderDepth(), Is.EqualTo(2));

            int noCategory = 0;

            foreach (PlannedSet set in plan.Buildable)
            {
                foreach (PlannedCondition condition in set.Conditions)
                {
                    if (!condition.HasCategory)
                    {
                        noCategory++;
                    }
                }
            }

            Assert.That(noCategory, Is.EqualTo(6), "the Source File conditions carry no category");
        }
    }
}
