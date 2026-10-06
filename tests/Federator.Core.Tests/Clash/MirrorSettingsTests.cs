using System;
using System.Collections.Generic;
using Federator.Core.Clash;
using NUnit.Framework;

namespace Federator.Core.Tests
{
    /// <summary>
    /// F132, Bader's answer D to Q133: in Clash Detective the mirror test stays, its name
    /// ending with (mirror), and his answer A to Q136, the old test renamed to its name with
    /// (mirror) at the end. The ending is a setting. A mirror is named by its own name with
    /// the ending, never the kept test's, numbered before the ending where that name is
    /// taken. A run with no XML never reads the name back to a test, MirrorRuleTests.
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

        private static readonly string[] NoneTaken = new string[0];

        [Test]
        public void TheEndingGoesAfterTheNameAndOneSpace()
        {
            Assert.That(new MirrorSettings().NameFor(Name, NoneTaken), Is.EqualTo(Name + " (mirror)"));
        }

        // A name another test already carries: the next number goes before the ending, so
        // every mirror's name still ends with it, his words, and no two tests share one name.
        [Test]
        public void ANameTakenGetsTheNextNumberBeforeTheEnding()
        {
            MirrorSettings settings = new MirrorSettings();

            Assert.That(settings.NameFor(Name, new[] { Name + " (mirror)" }), Is.EqualTo(Name + " 2 (mirror)"));
            Assert.That(settings.NameFor(Name, new[] { Name + " (mirror)", Name + " 2 (mirror)" }),
                Is.EqualTo(Name + " 3 (mirror)"));
        }

        // A mirror whose own name already ends with the ending is named as it is, so it
        // never carries the ending twice. Its own name is among the names taken, as every
        // name of the XML is, and is not taken from itself.
        [Test]
        public void ANameThatAlreadyEndsWithItGetsNoSecondEnding()
        {
            MirrorSettings settings = new MirrorSettings();

            Assert.That(settings.NameFor(Name + " (mirror)", new[] { Name + " (mirror)" }), Is.EqualTo(Name + " (mirror)"));
            Assert.That(settings.NameFor(Name + " (mirror)", NoneTaken), Is.EqualTo(Name + " (mirror)"));
        }

        [Test]
        public void ANameEndingWithItAfterOneSpaceCarriesIt()
        {
            Assert.That(new MirrorSettings().CarriesTheEnding(Name + " (mirror)"), Is.True);
            Assert.That(new MirrorSettings().CarriesTheEnding(Name), Is.False);
            Assert.That(new MirrorSettings().CarriesTheEnding("(mirror)"), Is.False, "the ending alone names no test");
        }

        // Only the ending after one space is the ending. A name that ends with the word and
        // no space before it is another name.
        [Test]
        public void TheWordWithNoSpaceBeforeItIsNotTheEnding()
        {
            Assert.That(new MirrorSettings().CarriesTheEnding(Name + "(mirror)"), Is.False);
        }

        [Test]
        public void AnotherEndingIsUsedWhereItIsSet()
        {
            MirrorSettings settings = new MirrorSettings();
            settings.Ending = "[swap]";

            Assert.That(settings.NameFor(Name, NoneTaken), Is.EqualTo(Name + " [swap]"));
            Assert.That(settings.CarriesTheEnding(Name + " [swap]"), Is.True);
            Assert.That(settings.CarriesTheEnding(Name + " (mirror)"), Is.False);
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

        // ---------- the rule gives every mirror its own name with the ending ----------

        // Bader's words: the mirror test's own name ends with (mirror). The swap of Ducts
        // against Columns is Columns against Ducts, and it is created as Columns against
        // Ducts (mirror), never as Ducts against Columns (mirror).
        [Test]
        public void TheMirrorOfTheXmlIsCreatedUnderItsOwnNameWithTheEnding()
        {
            ClashTestPlan plan = MirrorRuleTests.Plan(
                MirrorRuleTests.Test(MirrorRuleTests.DuctsVsColumns, MirrorRuleTests.Ducts, MirrorRuleTests.Columns),
                MirrorRuleTests.Test(MirrorRuleTests.ColumnsVsDucts, MirrorRuleTests.Columns, MirrorRuleTests.Ducts));

            MirrorPair pair = MirrorRuleTests.Rule(plan.Buildable, PriorityMap.NothingPicked()).Pairs[0];

            Assert.That(pair.Kept.Name, Is.EqualTo(MirrorRuleTests.DuctsVsColumns));
            Assert.That(pair.Mirror.Name, Is.EqualTo(MirrorRuleTests.ColumnsVsDucts));
            Assert.That(pair.MirrorName, Is.EqualTo(MirrorRuleTests.ColumnsVsDucts + " (mirror)"));
            Assert.That(pair.MirrorName, Is.Not.EqualTo(MirrorRuleTests.DuctsVsColumns + " (mirror)"));
        }

        // A test of the XML whose own name already ends with (mirror) and is a mirror is
        // created under that name, with no second ending.
        [Test]
        public void AMirrorTheXmlAlreadyNamesWithTheEndingKeepsThatName()
        {
            ClashTestPlan plan = MirrorRuleTests.Plan(
                MirrorRuleTests.Test(MirrorRuleTests.DuctsVsColumns, MirrorRuleTests.Ducts, MirrorRuleTests.Columns),
                MirrorRuleTests.Test(MirrorRuleTests.ColumnsVsDucts + " (mirror)", MirrorRuleTests.Columns, MirrorRuleTests.Ducts));

            MirrorRule rule = MirrorRuleTests.Rule(plan.Buildable, PriorityMap.NothingPicked());

            Assert.That(rule.Pairs.Count, Is.EqualTo(1));
            Assert.That(rule.Pairs[0].MirrorName, Is.EqualTo(MirrorRuleTests.ColumnsVsDucts + " (mirror)"));
            Assert.That(plan.WithMirrorsNamed(rule).Buildable[1].Name, Is.EqualTo(MirrorRuleTests.ColumnsVsDucts + " (mirror)"));
        }

        // A rerun with no XML reads the mirror it made as it is saved, and never adds the
        // ending again. A swap a person made under a name of their own carries no ending, so
        // it is not a mirror, and the saved mirror's sides ask the question of the one saved
        // test without the ending they mirror.
        [Test]
        public void ASavedMirrorKeepsItsSavedName()
        {
            ClashTestPlan plan = MirrorRuleTests.SavedWithSides(
                MirrorRuleTests.DuctsVsColumns, MirrorRuleTests.Ducts, MirrorRuleTests.Columns,
                MirrorRuleTests.ColumnsVsDucts + " (mirror)", MirrorRuleTests.Columns, MirrorRuleTests.Ducts,
                "a swap a person made", MirrorRuleTests.Columns, MirrorRuleTests.Ducts);

            MirrorRule rule = MirrorRule.Of(plan.Buildable, PriorityMap.NothingPicked(), null, new MirrorSettings());

            Assert.That(rule.Pairs.Count, Is.EqualTo(1));
            Assert.That(rule.Pairs[0].Kept.Name, Is.EqualTo(MirrorRuleTests.DuctsVsColumns));
            Assert.That(rule.Pairs[0].MirrorName, Is.EqualTo(MirrorRuleTests.ColumnsVsDucts + " (mirror)"));
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
        // mirror takes the next number before the ending. Its name still ends with (mirror),
        // his words, and no two tests share one name.
        [Test]
        public void ANameAnotherTestCarriesIsNotTakenAndTheEndingStays()
        {
            ClashTestPlan plan = MirrorRuleTests.Plan(
                MirrorRuleTests.Test(MirrorRuleTests.DuctsVsColumns, MirrorRuleTests.Ducts, MirrorRuleTests.Columns),
                MirrorRuleTests.Test(MirrorRuleTests.ColumnsVsDucts, MirrorRuleTests.Columns, MirrorRuleTests.Ducts),
                MirrorRuleTests.Test(MirrorRuleTests.ColumnsVsDucts + " (mirror)", MirrorRuleTests.Ducts, MirrorRuleTests.Walls));

            MirrorRule rule = MirrorRuleTests.Rule(plan.Buildable, PriorityMap.NothingPicked());

            Assert.That(rule.Pairs.Count, Is.EqualTo(1));
            Assert.That(rule.Pairs[0].MirrorName, Is.EqualTo(MirrorRuleTests.ColumnsVsDucts + " 2 (mirror)"));
            Assert.That(string.Join("\n", new List<string>(rule.Lines()).ToArray()), Does.Contain(
                "created and run as " + MirrorRuleTests.ColumnsVsDucts + " 2 (mirror)"));
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
            ClashTestPlan plan = MirrorRuleTests.SavedWithSides(
                MirrorRuleTests.DuctsVsColumns, MirrorRuleTests.Ducts, MirrorRuleTests.Columns,
                MirrorRuleTests.ColumnsVsDucts + " (mirror)", MirrorRuleTests.Columns, MirrorRuleTests.Ducts);
            MirrorRule rule = MirrorRule.Of(plan.Buildable, PriorityMap.NothingPicked(), null, new MirrorSettings());
            ClashTestPlan named = plan.WithMirrorsNamed(rule);

            Assert.That(rule.Pairs.Count, Is.EqualTo(1));
            Assert.That(named.Buildable.Count, Is.EqualTo(2));
            Assert.That(named.Buildable[1].Name, Is.EqualTo(MirrorRuleTests.ColumnsVsDucts + " (mirror)"));
            Assert.That(named.Buildable[1].Address[0], Is.EqualTo(1));
        }
    }
}
