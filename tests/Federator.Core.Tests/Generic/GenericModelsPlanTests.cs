using System;
using System.Collections.Generic;
using Federator.Core.Exchange;
using Federator.Core.Generic;
using Federator.Core.Sets;
using NUnit.Framework;

namespace Federator.Core.Tests
{
    /// <summary>
    /// The search sets that count the Generic Models of a group, F128 and FR-177, Bader's request 3
    /// under Q112. Every file name here is sample data, and nothing in the code under test names any
    /// one project's file.
    /// </summary>
    [TestFixture]
    public class GenericModelsPlanTests
    {
        private const string Root = "lcop_selection_set_tree";

        private static GenericModelInput Model(string file)
        {
            return new GenericModelInput(file);
        }

        private static GenericModelsPlan Plan(params string[] files)
        {
            List<GenericModelInput> models = new List<GenericModelInput>();

            foreach (string file in files)
            {
                models.Add(Model(file));
            }

            return GenericModelsPlan.For(models, new GenericModelsSettings());
        }

        // ---------- the settings ----------

        [Test]
        public void TheDefaultsAreTheToolsOwnReadingAndBadersWords()
        {
            GenericModelsSettings settings = new GenericModelsSettings();

            Assert.That(settings.CategoryValue, Is.EqualTo("Generic Models"));
            Assert.That(settings.FolderName, Is.EqualTo("Generic Models"));
            Assert.That(settings.SheetName, Is.EqualTo("Generic Models"));
        }

        /// <summary>The value the tool asks for is a value its own list of categories holds, so the default answers to a file.</summary>
        [Test]
        public void TheDefaultCategoryIsOneTheToolsListOfMeasuredCategoriesHolds()
        {
            Assert.That(RevitCategories.All(), Does.Contain(GenericModelsSettings.DefaultCategoryValue));
        }

        [Test]
        public void ASettingThatCannotWorkIsRefusedWhereItIsSet()
        {
            GenericModelsSettings settings = new GenericModelsSettings();

            Assert.Throws<ArgumentException>(() => settings.CategoryValue = string.Empty);
            Assert.Throws<ArgumentException>(() => settings.CategoryValue = null);
            Assert.Throws<ArgumentException>(() => settings.CategoryValue = "   ");
            Assert.Throws<ArgumentException>(() => settings.FolderName = "  ");
            Assert.Throws<ArgumentException>(() => settings.FolderName = "Generic/Models");
            Assert.Throws<ArgumentException>(() => settings.SheetName = "Generic:Models");
            Assert.Throws<ArgumentException>(() => settings.SheetName = new string('x', 32));

            Assert.That(settings.CategoryValue, Is.EqualTo("Generic Models"), "a refused value changes nothing");
            Assert.That(settings.FolderName, Is.EqualTo("Generic Models"));
            Assert.That(settings.SheetName, Is.EqualTo("Generic Models"));
        }

        [Test]
        public void ASheetNameExcelAcceptsIsKept()
        {
            GenericModelsSettings settings = new GenericModelsSettings { SheetName = "Generic" };

            Assert.That(settings.SheetName, Is.EqualTo("Generic"));
        }

        /// <summary>The value is compared exactly and never trimmed, so a trailing space is the person's value.</summary>
        [Test]
        public void ACategoryValueIsKeptExactlyAsGiven()
        {
            GenericModelsSettings settings = new GenericModelsSettings { CategoryValue = "Generic Models " };

            Assert.That(settings.CategoryValue, Is.EqualTo("Generic Models "));
        }

        // ---------- one set for each model ----------

        [Test]
        public void EachModelGetsOneSetInTheFolderUnderTheRootInTheOrderHandedIn()
        {
            string first = TestPaths.At("NWC", "1104-PAR-1A02MM-ZZZ-ME-MOD-000001.nwc");
            GenericModelsPlan plan = Plan(
                first,
                TestPaths.At("NWC", "1104-PAR-1A02MM-ZZZ-AR-MOD-000001.nwc"),
                "1104-PAR-1A02MM-ZZZ-ST-MOD-000001");

            Assert.That(plan.Sets.Count, Is.EqualTo(3));
            Assert.That(plan.Folder, Is.EqualTo("Generic Models"));
            Assert.That(plan.Sets[0].ModelName, Is.EqualTo("1104-PAR-1A02MM-ZZZ-ME-MOD-000001"));
            Assert.That(plan.Sets[1].ModelName, Is.EqualTo("1104-PAR-1A02MM-ZZZ-AR-MOD-000001"));
            Assert.That(plan.Sets[2].ModelName, Is.EqualTo("1104-PAR-1A02MM-ZZZ-ST-MOD-000001"));
            Assert.That(plan.Sets[0].ModelFile, Is.EqualTo(first));

            PlannedSet set = plan.Sets[0].Set;

            Assert.That(set.Name, Is.EqualTo("1104-PAR-1A02MM-ZZZ-ME-MOD-000001"));
            Assert.That(set.Path, Is.EqualTo(Root + "/Generic Models/1104-PAR-1A02MM-ZZZ-ME-MOD-000001"));
            Assert.That(new List<string>(set.Folders), Is.EqualTo(new[] { "Generic Models" }));
            Assert.That(plan.Notes, Is.Empty);
        }

        /// <summary>
        /// The two conditions are the two the client's own file writes, a category condition and a Source
        /// File condition, and every word of them is held against that file, so the plan answers to the file
        /// and not to whoever typed it.
        /// </summary>
        [Test]
        public void TheConditionsAreTheShapeTheClientsOwnFileWrites()
        {
            ExchangeDocument document = new ExchangeReader().ReadFile(Samples.Matrix());
            SetBuildPlan clients = SetBuildPlan.From(document);

            PlannedCondition theirCategory = null;
            PlannedCondition theirSourceFile = null;

            foreach (PlannedSet set in clients.Buildable)
            {
                foreach (PlannedCondition condition in set.Conditions)
                {
                    if (theirCategory == null && condition.HasCategory
                        && condition.PropertyInternalName == EmptySets.CategoryProperty
                        && condition.Test == ConditionTest.Equals)
                    {
                        theirCategory = condition;
                    }

                    if (theirSourceFile == null && !condition.HasCategory
                        && condition.Test == ConditionTest.Contains
                        && condition.PropertyInternalName.IndexOf("SourceFile", StringComparison.Ordinal) >= 0)
                    {
                        theirSourceFile = condition;
                    }
                }
            }

            Assert.That(theirCategory, Is.Not.Null, "the client's file holds a category condition");
            Assert.That(theirSourceFile, Is.Not.Null, "the client's file holds a Source File condition");

            PlannedSet ours = Plan("1104-PAR-1A02MM-ZZZ-ME-MOD-000001.nwc").Sets[0].Set;

            Assert.That(ours.ConditionCount, Is.EqualTo(2));
            AssertSameShape(ours.Conditions[0], theirCategory);
            AssertSameShape(ours.Conditions[1], theirSourceFile);
            Assert.That(ours.Conditions[0].Value, Is.EqualTo("Generic Models"));
            Assert.That(ours.Conditions[1].Value, Is.EqualTo("1104-PAR-1A02MM-ZZZ-ME-MOD-000001"));
        }

        private static void AssertSameShape(PlannedCondition ours, PlannedCondition theirs)
        {
            Assert.That(ours.Test, Is.EqualTo(theirs.Test));
            Assert.That(ours.Flags, Is.EqualTo(theirs.Flags));
            Assert.That(ours.HasCategory, Is.EqualTo(theirs.HasCategory));
            Assert.That(ours.CategoryInternalName, Is.EqualTo(theirs.CategoryInternalName));
            Assert.That(ours.CategoryDisplayName, Is.EqualTo(theirs.CategoryDisplayName));
            Assert.That(ours.PropertyInternalName, Is.EqualTo(theirs.PropertyInternalName));
            Assert.That(ours.PropertyDisplayName, Is.EqualTo(theirs.PropertyDisplayName));
            Assert.That(ours.ValueType, Is.EqualTo(theirs.ValueType));
        }

        /// <summary>Both conditions are one group, so they are ANDed, and the set reads as one question.</summary>
        [Test]
        public void TheTwoConditionsAreOneGroupAndSayTheQuestionAsOne()
        {
            PlannedSet set = Plan("1104-PAR-1A02MM-ZZZ-ME-MOD-000001.nwc").Sets[0].Set;

            Assert.That(set.GroupCount, Is.EqualTo(1));
            Assert.That(set.Conditions[0].StartsAGroup, Is.False);
            Assert.That(set.Conditions[1].StartsAGroup, Is.False);
            Assert.That(set.Describe(), Is.EqualTo(
                "LcRevitData_Element/LcRevitPropertyElementCategory (Category) equals \"Generic Models\""
                    + " and LcOaNodeSourceFile (Source File) contains \"1104-PAR-1A02MM-ZZZ-ME-MOD-000001\""));
        }

        [Test]
        public void ASettingChangedReachesEverySetAndTheWordsOfWhatWasAsked()
        {
            GenericModelsSettings settings = new GenericModelsSettings { CategoryValue = "Generic Model", FolderName = "Other" };
            GenericModelsPlan plan = GenericModelsPlan.For(new[] { Model("a.nwc") }, settings);

            Assert.That(plan.Sets[0].Set.Path, Is.EqualTo(Root + "/Other/a"));
            Assert.That(plan.Sets[0].Set.Conditions[0].Value, Is.EqualTo("Generic Model"));
            Assert.That(plan.Asked, Is.EqualTo("Category equals \"Generic Model\""));
        }

        /// <summary>
        /// The add-in builds these sets through the plan the builder already takes. Every set is buildable, the
        /// folder is one, and nothing is skipped, so a set is never silently dropped on the way.
        /// </summary>
        [Test]
        public void TheSetsGoToTheBuilderAsThePlanItAlreadyTakes()
        {
            GenericModelsPlan plan = Plan("a.nwc", "b.nwc");
            SetBuildPlan build = plan.ToBuildPlan();

            Assert.That(build.HasWork, Is.True);
            Assert.That(build.Buildable.Count, Is.EqualTo(2));
            Assert.That(build.Buildable[0], Is.SameAs(plan.Sets[0].Set));
            Assert.That(build.Buildable[1], Is.SameAs(plan.Sets[1].Set));
            Assert.That(build.Skipped, Is.Empty);
            Assert.That(build.FolderPaths().Count, Is.EqualTo(1));
            Assert.That(new List<string>(build.FolderPaths()[0]), Is.EqualTo(new[] { "Generic Models" }));

            Assert.That(GenericModelsPlan.For(null, new GenericModelsSettings()).ToBuildPlan().HasWork, Is.False);
            Assert.Throws<ArgumentNullException>(() => SetBuildPlan.Of(null));
            Assert.That(SetBuildPlan.Of(new PlannedSet[] { null }).HasWork, Is.False, "a null set is left out");
        }

        // ---------- the text a model is found by ----------

        /// <summary>
        /// Which text of a model its items' Source File holds is UNKNOWN until the laptop lane's probe reads it,
        /// so a text handed in finds the model and the file's stem still names the set.
        /// </summary>
        [Test]
        public void ATextHandedInFindsTheModelAndTheStemStillNamesTheSet()
        {
            GenericModelsPlan plan = GenericModelsPlan.For(
                new[] { new GenericModelInput("1104-PAR-1C07BC-ZZZ-AR-MOD-000001.nwc", "1104-PAR-100000-ZZZ-AR-MOD-003000") },
                new GenericModelsSettings());

            Assert.That(plan.Sets[0].Set.Name, Is.EqualTo("1104-PAR-1C07BC-ZZZ-AR-MOD-000001"));
            Assert.That(plan.Sets[0].Set.Conditions[1].Value, Is.EqualTo("1104-PAR-100000-ZZZ-AR-MOD-003000"));
            Assert.That(plan.Sets[0].MatchText, Is.EqualTo("1104-PAR-100000-ZZZ-AR-MOD-003000"));
        }

        [Test]
        public void ABlankTextIsTheStem()
        {
            GenericModelsPlan plan = GenericModelsPlan.For(new[] { new GenericModelInput("a.nwc", string.Empty) }, new GenericModelsSettings());

            Assert.That(plan.Sets[0].MatchText, Is.EqualTo("a"));
        }

        /// <summary>A text of spaces alone would match every Source File that holds a space, so it is refused where it is handed in.</summary>
        [Test]
        public void ATextOfSpacesAloneIsRefusedWhereItIsHandedIn()
        {
            Assert.Throws<ArgumentException>(() => new GenericModelInput("a.nwc", "   "));
            Assert.That(new GenericModelInput("a.nwc", null).MatchText, Is.Null);
            Assert.That(new GenericModelInput("a.nwc", " x ").MatchText, Is.EqualTo(" x "), "a text with a space in it is kept as given");
        }

        // ---------- what the plan notices and never acts on ----------

        [Test]
        public void TwoModelsOfOneNameAreOneSetAndSaidSo()
        {
            GenericModelsPlan plan = Plan(
                TestPaths.At("a", "x-ME-1.nwc"), TestPaths.At("b", "X-ME-1.NWC"), TestPaths.At("a", "y.nwc"));

            Assert.That(plan.Sets.Count, Is.EqualTo(2));
            Assert.That(plan.Notes.Count, Is.EqualTo(1));
            Assert.That(plan.Notes[0], Does.Contain("two models of the group are named x-ME-1, so one set serves both"));
        }

        [Test]
        public void ThreeModelsOfOneNameAreOneSetAndSaidAsThreeAndNotAsTwo()
        {
            GenericModelsPlan plan = Plan(
                TestPaths.At("a", "x.nwc"), TestPaths.At("b", "x.nwc"), TestPaths.At("c", "X.nwc"));

            Assert.That(plan.Sets.Count, Is.EqualTo(1));
            Assert.That(plan.Notes.Count, Is.EqualTo(1));
            Assert.That(plan.Notes[0], Is.EqualTo("3 models of the group are named x, so one set serves all of them and counts all of them"));
        }

        /// <summary>
        /// Models of one name handed different texts are one set that looks for the first only, and the note says
        /// the others are not counted. The break: handed the same text, the note says one set serves both.
        /// </summary>
        [Test]
        public void ModelsOfOneNameHandedDifferentTextsSayTheOthersAreNotCounted()
        {
            GenericModelsPlan plan = GenericModelsPlan.For(
                new[]
                {
                    new GenericModelInput(TestPaths.At("a", "x.nwc"), "rvt1"),
                    new GenericModelInput(TestPaths.At("b", "x.nwc"), "RVT2"),
                    new GenericModelInput(TestPaths.At("c", "x.nwc"), "rvt1")
                },
                new GenericModelsSettings());

            Assert.That(plan.Sets.Count, Is.EqualTo(1));
            Assert.That(plan.Sets[0].MatchText, Is.EqualTo("rvt1"));
            Assert.That(plan.Notes.Count, Is.EqualTo(1));
            Assert.That(plan.Notes[0], Is.EqualTo("3 models of the group are named x and were handed the texts rvt1 and RVT2, so one set serves them"
                + " and looks for rvt1 only, and the items that carry the others are not counted"));

            GenericModelsPlan same = GenericModelsPlan.For(
                new[] { new GenericModelInput(TestPaths.At("a", "x.nwc"), "rvt1"), new GenericModelInput(TestPaths.At("b", "x.nwc"), "RVT1") },
                new GenericModelsSettings());

            Assert.That(same.Notes[0], Does.Contain("so one set serves both and counts both"));
        }

        [Test]
        public void AModelWithNoFileNameHasNoSetAndIsCounted()
        {
            GenericModelsPlan plan = GenericModelsPlan.For(
                new[] { Model("a.nwc"), Model(string.Empty), Model("   "), null, new GenericModelInput(null) },
                new GenericModelsSettings());

            Assert.That(plan.Sets.Count, Is.EqualTo(1));
            Assert.That(plan.Notes.Count, Is.EqualTo(1));
            Assert.That(plan.Notes[0], Does.StartWith("4 models of the group have no file name, so they have no Generic Models set"));

            GenericModelsPlan one = GenericModelsPlan.For(new[] { Model(string.Empty) }, new GenericModelsSettings());

            Assert.That(one.Notes[0], Does.StartWith("1 model of the group has no file name, so it has no Generic Models set"));
        }

        /// <summary>
        /// A text that another model's text holds finds that model's items too, so the first count is the
        /// sum of both. Said, not changed: both sets are still planned.
        /// </summary>
        [Test]
        public void ATextAnotherModelsTextHoldsIsNamedBothWaysAndBothSetsRemain()
        {
            GenericModelsPlan plan = Plan("A-ME.nwc", "A-ME2.nwc", "B-AR.nwc");

            Assert.That(plan.Sets.Count, Is.EqualTo(3));
            Assert.That(plan.Notes.Count, Is.EqualTo(1));
            Assert.That(plan.Notes[0], Is.EqualTo("the text A-ME that finds the model A-ME is also in the text of the model A-ME2,"
                + " so the set of A-ME can count the items of that model too"));
            Assert.That(plan.TextsMeet, Is.True);
        }

        /// <summary>One note for a text, however many texts hold it, and one for a text however many models share it.</summary>
        [Test]
        public void ManyModelsOfOneMeetingTextAreOneNoteAndNotOneForEachPair()
        {
            GenericModelsPlan inside = Plan("A-ME.nwc", "A-ME2.nwc", "A-ME3.nwc");

            Assert.That(inside.Notes.Count, Is.EqualTo(1));
            Assert.That(inside.Notes[0], Does.Contain("is also in the text of the models A-ME2 and A-ME3, so the set of A-ME can count the items of those models too"));

            List<GenericModelInput> many = new List<GenericModelInput>();

            for (int i = 0; i < 6; i++)
            {
                many.Add(new GenericModelInput("m" + i + ".nwc", "shared"));
            }

            GenericModelsPlan shared = GenericModelsPlan.For(many, new GenericModelsSettings());

            Assert.That(shared.Notes.Count, Is.EqualTo(1));
            Assert.That(shared.Notes[0], Does.StartWith("the models m0, m1, m2, m3, m4 and m5 are all found by the text shared"));
            Assert.That(shared.TextsMeet, Is.True);
        }

        [Test]
        public void TwoModelsFoundByOneTextAreNamedOnceAndBothSetsRemain()
        {
            GenericModelsPlan plan = GenericModelsPlan.For(
                new[] { new GenericModelInput("a.nwc", "shared"), new GenericModelInput("b.nwc", "SHARED") },
                new GenericModelsSettings());

            Assert.That(plan.Sets.Count, Is.EqualTo(2));
            Assert.That(plan.Notes.Count, Is.EqualTo(1));
            Assert.That(plan.Notes[0], Does.Contain("the models a and b are both found by the text shared"));
            Assert.That(plan.Notes[0], Does.Contain("each of their sets can count the items of both"));
        }

        /// <summary>The break of the two notes above: models whose texts do not meet get no note.</summary>
        [Test]
        public void ModelsWhoseTextsDoNotMeetGetNoNote()
        {
            GenericModelsPlan plan = Plan("1104-PAR-1A02MM-ZZZ-ME-MOD-000001.nwc", "1104-PAR-1A02MM-ZZZ-AR-MOD-000001.nwc");

            Assert.That(plan.Notes, Is.Empty);
            Assert.That(plan.TextsMeet, Is.False);
        }

        // ---------- no model, no set ----------

        [Test]
        public void NoModelsIsAPlanWithNoSetAndNullModelsToo()
        {
            Assert.That(GenericModelsPlan.For(new GenericModelInput[0], new GenericModelsSettings()).Sets, Is.Empty);
            Assert.That(GenericModelsPlan.For(null, new GenericModelsSettings()).Sets, Is.Empty);
            Assert.Throws<ArgumentNullException>(() => GenericModelsPlan.For(new GenericModelInput[0], null));
        }
    }
}
