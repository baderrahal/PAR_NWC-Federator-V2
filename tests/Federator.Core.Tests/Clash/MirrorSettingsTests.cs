using System;
using System.Collections.Generic;
using Federator.Core.Clash;
using NUnit.Framework;

namespace Federator.Core.Tests
{
    /// <summary>
    /// F132, Bader's answer D to Q133: in Clash Detective the mirror test stays, its name
    /// ending with (mirror). The ending is a setting, and a rerun never adds it twice.
    /// </summary>
    [TestFixture]
    public class MirrorSettingsTests
    {
        private const string Name = "BLD-ST-Columns-vs-BLD-ME-Ducts";

        [Test]
        public void TheEndingIsHisWord()
        {
            Assert.That(new MirrorSettings().Ending, Is.EqualTo("(mirror)"));
            Assert.That(MirrorSettings.DefaultEnding, Is.EqualTo("(mirror)"));
        }

        [Test]
        public void TheEndingFollowsTheNameAfterOneSpace()
        {
            Assert.That(new MirrorSettings().NameOf(Name), Is.EqualTo(Name + " (mirror)"));
        }

        [Test]
        public void ANameThatAlreadyEndsWithItGetsNoSecondEnding()
        {
            MirrorSettings settings = new MirrorSettings();

            Assert.That(settings.NameOf(Name + " (mirror)"), Is.EqualTo(Name + " (mirror)"));
            Assert.That(settings.NameOf(settings.NameOf(Name)), Is.EqualTo(settings.NameOf(Name)));
        }

        // Only the ending after one space is the ending. A name that ends with the word and
        // no space before it is another name.
        [Test]
        public void TheWordWithNoSpaceBeforeItIsNotTheEnding()
        {
            Assert.That(new MirrorSettings().NameOf(Name + "(mirror)"), Is.EqualTo(Name + "(mirror) (mirror)"));
        }

        [Test]
        public void AnotherEndingIsUsedWhereItIsSet()
        {
            MirrorSettings settings = new MirrorSettings();
            settings.Ending = "[swap]";

            Assert.That(settings.NameOf(Name), Is.EqualTo(Name + " [swap]"));
            Assert.That(settings.NameOf(Name + " [swap]"), Is.EqualTo(Name + " [swap]"));
        }

        [Test]
        public void AnEndingThatCannotBeReadOffANameIsRefused()
        {
            foreach (string refused in new[] { null, string.Empty, " ", " (mirror)", "(mirror) " })
            {
                MirrorSettings settings = new MirrorSettings();

                Assert.Throws<ArgumentException>(() => settings.Ending = refused, "\"" + refused + "\"");
                Assert.That(settings.Ending, Is.EqualTo("(mirror)"));
            }
        }

        // ---------- the rule gives every mirror that name ----------

        [Test]
        public void TheMirrorOfTheXmlIsCreatedUnderItsNameWithTheEnding()
        {
            ClashTestPlan plan = MirrorRuleTests.Plan(
                MirrorRuleTests.Test(MirrorRuleTests.DuctsVsColumns, MirrorRuleTests.Ducts, MirrorRuleTests.Columns),
                MirrorRuleTests.Test(MirrorRuleTests.ColumnsVsDucts, MirrorRuleTests.Columns, MirrorRuleTests.Ducts));

            MirrorPair pair = MirrorRuleTests.Rule(plan.Buildable, PriorityMap.NothingPicked()).Pairs[0];

            Assert.That(pair.Mirror.Name, Is.EqualTo(MirrorRuleTests.ColumnsVsDucts));
            Assert.That(pair.MirrorName, Is.EqualTo(MirrorRuleTests.ColumnsVsDucts + " (mirror)"));
        }

        // A rerun with no XML reads the mirror it made as it is saved, and never adds the
        // ending again. This tool renames no saved test.
        [Test]
        public void ASavedMirrorKeepsItsSavedName()
        {
            ClashTestPlan plan = ClashTestPlan.FromDocument(
                new List<SavedClashTest>
                {
                    MirrorRuleTests.Saved(MirrorRuleTests.DuctsVsColumns, MirrorRuleTests.Ducts, MirrorRuleTests.Columns, 0),
                    MirrorRuleTests.Saved(
                        MirrorRuleTests.ColumnsVsDucts + " (mirror)", MirrorRuleTests.Columns, MirrorRuleTests.Ducts, 1),
                    MirrorRuleTests.Saved("a swap a person made", MirrorRuleTests.Columns, MirrorRuleTests.Ducts, 2)
                },
                "m");

            MirrorRule rule = MirrorRule.Of(plan.Buildable, PriorityMap.NothingPicked(), null, new MirrorSettings());

            Assert.That(rule.Pairs.Count, Is.EqualTo(2));
            Assert.That(rule.Pairs[0].MirrorName, Is.EqualTo(MirrorRuleTests.ColumnsVsDucts + " (mirror)"));
            Assert.That(rule.Pairs[1].MirrorName, Is.EqualTo("a swap a person made"));
        }

        // A rerun with the XML picked makes the same name again, so it finds the mirror the
        // run before created and never makes a second one.
        [Test]
        public void ARerunWithTheXmlGivesTheSameName()
        {
            string[] xml =
            {
                MirrorRuleTests.Test(MirrorRuleTests.DuctsVsColumns, MirrorRuleTests.Ducts, MirrorRuleTests.Columns),
                MirrorRuleTests.Test(MirrorRuleTests.ColumnsVsDucts, MirrorRuleTests.Columns, MirrorRuleTests.Ducts)
            };

            string first = MirrorRuleTests.Rule(MirrorRuleTests.Plan(xml).Buildable, PriorityMap.NothingPicked())
                .Pairs[0].MirrorName;
            string second = MirrorRuleTests.Rule(MirrorRuleTests.Plan(xml).Buildable, PriorityMap.NothingPicked())
                .Pairs[0].MirrorName;

            Assert.That(second, Is.EqualTo(first));
            Assert.That(second, Is.EqualTo(MirrorRuleTests.ColumnsVsDucts + " (mirror)"));
        }

        // Another test of the XML already carries the name the mirror would take, so the
        // mirror keeps its own and the line says why, and no two tests share one name.
        [Test]
        public void ANameAnotherTestCarriesIsNotTaken()
        {
            ClashTestPlan plan = MirrorRuleTests.Plan(
                MirrorRuleTests.Test(MirrorRuleTests.DuctsVsColumns, MirrorRuleTests.Ducts, MirrorRuleTests.Columns),
                MirrorRuleTests.Test(MirrorRuleTests.ColumnsVsDucts, MirrorRuleTests.Columns, MirrorRuleTests.Ducts),
                MirrorRuleTests.Test(MirrorRuleTests.ColumnsVsDucts + " (mirror)", MirrorRuleTests.Ducts, MirrorRuleTests.Walls));

            MirrorRule rule = MirrorRuleTests.Rule(plan.Buildable, PriorityMap.NothingPicked());

            Assert.That(rule.Pairs.Count, Is.EqualTo(1));
            Assert.That(rule.Pairs[0].MirrorName, Is.EqualTo(MirrorRuleTests.ColumnsVsDucts));
            Assert.That(string.Join("\n", new List<string>(rule.Lines()).ToArray()), Does.Contain(
                "created and run under its own name, because another test of the XML is named "
                    + MirrorRuleTests.ColumnsVsDucts + " with the ending"));
        }

        // ---------- the plan creates it under that name ----------

        [Test]
        public void ThePlanCreatesBothTheMirrorUnderItsNewName()
        {
            ClashTestPlan plan = MirrorRuleTests.Plan(
                MirrorRuleTests.Test(MirrorRuleTests.DuctsVsColumns, MirrorRuleTests.Ducts, MirrorRuleTests.Columns),
                MirrorRuleTests.Test(MirrorRuleTests.ColumnsVsDucts, MirrorRuleTests.Columns, MirrorRuleTests.Ducts));

            ClashTestPlan named = plan.WithMirrorsNamed(MirrorRuleTests.Rule(plan.Buildable, PriorityMap.NothingPicked()));
            CreationPlan creation = CreationPlan.For(
                named.Buildable,
                new Dictionary<string, int> { { MirrorRuleTests.Ducts, 5 }, { MirrorRuleTests.Columns, 7 } });

            Assert.That(named.Buildable.Count, Is.EqualTo(2));
            Assert.That(named.Skipped.Count, Is.EqualTo(plan.Skipped.Count));
            Assert.That(named.Buildable[0].Name, Is.EqualTo(MirrorRuleTests.DuctsVsColumns));
            Assert.That(named.Buildable[1].Name, Is.EqualTo(MirrorRuleTests.ColumnsVsDucts + " (mirror)"));
            Assert.That(named.Buildable[1].FileIndex, Is.EqualTo(1));
            Assert.That(named.Buildable[1].Left.Locator, Is.EqualTo(MirrorRuleTests.Columns));
            Assert.That(creation.CreateCount, Is.EqualTo(2));
        }

        // The rule is matched to the plan's tests as the same objects. A rule built over a
        // second read of the same tests would rename nothing while its lines named each
        // mirror by its new name, so it is refused.
        [Test]
        public void ARuleBuiltOverAnotherReadOfTheTestsIsRefused()
        {
            string[] xml =
            {
                MirrorRuleTests.Test(MirrorRuleTests.DuctsVsColumns, MirrorRuleTests.Ducts, MirrorRuleTests.Columns),
                MirrorRuleTests.Test(MirrorRuleTests.ColumnsVsDucts, MirrorRuleTests.Columns, MirrorRuleTests.Ducts)
            };

            ClashTestPlan plan = MirrorRuleTests.Plan(xml);
            MirrorRule overAnotherRead = MirrorRuleTests.Rule(MirrorRuleTests.Plan(xml).Buildable, PriorityMap.NothingPicked());

            Assert.That(overAnotherRead.Pairs.Count, Is.EqualTo(1));
            Assert.Throws<ArgumentException>(() => plan.WithMirrorsNamed(overAnotherRead));
        }

        [Test]
        public void ASavedMirrorIsRunWhereItIsAndNotRenamed()
        {
            ClashTestPlan plan = ClashTestPlan.FromDocument(
                new List<SavedClashTest>
                {
                    MirrorRuleTests.Saved(MirrorRuleTests.DuctsVsColumns, MirrorRuleTests.Ducts, MirrorRuleTests.Columns, 0),
                    MirrorRuleTests.Saved(MirrorRuleTests.ColumnsVsDucts, MirrorRuleTests.Columns, MirrorRuleTests.Ducts, 1)
                },
                "m");

            ClashTestPlan named = plan.WithMirrorsNamed(
                MirrorRule.Of(plan.Buildable, PriorityMap.NothingPicked(), null, new MirrorSettings()));

            Assert.That(named.Buildable.Count, Is.EqualTo(2));
            Assert.That(named.Buildable[1].Name, Is.EqualTo(MirrorRuleTests.ColumnsVsDucts));
            Assert.That(named.Buildable[1].Address[0], Is.EqualTo(1));
        }
    }
}
