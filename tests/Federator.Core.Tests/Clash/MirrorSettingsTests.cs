using System;
using System.Collections.Generic;
using Federator.Core.Clash;
using NUnit.Framework;

namespace Federator.Core.Tests
{
    /// <summary>
    /// F132, Bader's answer D to Q133: in Clash Detective the mirror test stays, its name
    /// ending with (mirror). The ending is a setting. A mirror is named after the test kept,
    /// numbered before the ending where that name is taken, and the name is read back to the
    /// test kept, so a run with no XML finds the pair by the name alone.
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

        // F132 attempt 5. The mirror is named after the test kept, so a run with no XML reads
        // off the name alone which test it mirrors, an NWF holding X and X (mirror).
        [Test]
        public void TheMirrorIsNamedAfterTheTestKeptWithTheEndingAfterOneSpace()
        {
            Assert.That(new MirrorSettings().NameFor(Name, NoneTaken), Is.EqualTo(Name + " (mirror)"));
        }

        // A test kept over more than one mirror, Q121 B, or a name another test already
        // carries: the next number goes before the ending, so every mirror's name still ends
        // with it, his words, and no two tests share one name.
        [Test]
        public void ANameTakenGetsTheNextNumberBeforeTheEnding()
        {
            MirrorSettings settings = new MirrorSettings();

            Assert.That(settings.NameFor(Name, new[] { Name + " (mirror)" }), Is.EqualTo(Name + " 2 (mirror)"));
            Assert.That(settings.NameFor(Name, new[] { Name + " (mirror)", Name + " 2 (mirror)" }),
                Is.EqualTo(Name + " 3 (mirror)"));
        }

        // What NameFor writes, KeptNamesOf reads back to the test kept, numbered or not, so
        // the run after finds the test a mirror's name could have been made for.
        [Test]
        public void TheNameWrittenIsReadBackToTheTestKept()
        {
            MirrorSettings settings = new MirrorSettings();
            string[] saved = { Name, Name + " (mirror)", Name + " 2 (mirror)" };

            Assert.That(settings.KeptNamesOf(Name + " (mirror)", saved), Is.EqualTo(new[] { Name }));
            Assert.That(settings.KeptNamesOf(Name + " 2 (mirror)", saved), Is.EqualTo(new[] { Name }));
            Assert.That(settings.KeptNamesOf(Name, saved), Is.Empty, "a name with no ending is no mirror");
        }

        // The breaker's finding on attempt 5. Where a person's test carries the name with a
        // number, both tests the name could have been made for are given, the exact name
        // first, and MirrorRule pairs with the one whose question the sides ask.
        [Test]
        public void ANumberedNameGivesBothTestsItCouldHaveBeenMadeFor()
        {
            string[] saved = { Name, Name + " 2", Name + " 2 (mirror)" };

            Assert.That(new MirrorSettings().KeptNamesOf(Name + " 2 (mirror)", saved),
                Is.EqualTo(new[] { Name + " 2", Name }));
        }

        [Test]
        public void ANameWhoseTestKeptIsNotSavedIsReadAsNoPair()
        {
            Assert.That(new MirrorSettings().KeptNamesOf(Name + " (mirror)", new[] { Name + " (mirror)" }), Is.Empty);
            Assert.That(new MirrorSettings().CarriesTheEnding(Name + " (mirror)"), Is.True);
        }

        // Only the ending after one space is the ending. A name that ends with the word and
        // no space before it is another name.
        [Test]
        public void TheWordWithNoSpaceBeforeItIsNotTheEnding()
        {
            MirrorSettings settings = new MirrorSettings();

            Assert.That(settings.CarriesTheEnding(Name + "(mirror)"), Is.False);
            Assert.That(settings.KeptNamesOf(Name + "(mirror)", new[] { Name, Name + "(mirror)" }), Is.Empty);
        }

        [Test]
        public void AnotherEndingIsUsedWhereItIsSet()
        {
            MirrorSettings settings = new MirrorSettings();
            settings.Ending = "[swap]";

            Assert.That(settings.NameFor(Name, NoneTaken), Is.EqualTo(Name + " [swap]"));
            Assert.That(settings.KeptNamesOf(Name + " [swap]", new[] { Name }), Is.EqualTo(new[] { Name }));
            Assert.That(settings.KeptNamesOf(Name + " (mirror)", new[] { Name }), Is.Empty);
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
            Assert.That(pair.MirrorName, Is.EqualTo(MirrorRuleTests.DuctsVsColumns + " (mirror)"));
        }

        // A rerun with no XML reads the mirror it made as it is saved, and never adds the
        // ending again. A swap a person made under a name of their own is not found by its
        // name, so it keeps its own clashes.
        [Test]
        public void ASavedMirrorKeepsItsSavedName()
        {
            ClashTestPlan plan = MirrorRuleTests.SavedWithSides(
                MirrorRuleTests.DuctsVsColumns, MirrorRuleTests.Ducts, MirrorRuleTests.Columns,
                MirrorRuleTests.DuctsVsColumns + " (mirror)", MirrorRuleTests.Columns, MirrorRuleTests.Ducts,
                "a swap a person made", MirrorRuleTests.Columns, MirrorRuleTests.Ducts);

            MirrorRule rule = MirrorRule.Of(plan.Buildable, PriorityMap.NothingPicked(), null, new MirrorSettings());

            Assert.That(rule.Pairs.Count, Is.EqualTo(1));
            Assert.That(rule.Pairs[0].MirrorName, Is.EqualTo(MirrorRuleTests.DuctsVsColumns + " (mirror)"));
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
            Assert.That(second, Is.EqualTo(MirrorRuleTests.DuctsVsColumns + " (mirror)"));
        }

        // Another test of the XML already carries the name the mirror would take, so the
        // mirror takes the next number before the ending. Its name still ends with (mirror),
        // his words, where attempt 4 fell back to the XML's own name, and no two tests share
        // one name.
        [Test]
        public void ANameAnotherTestCarriesIsNotTakenAndTheEndingStays()
        {
            ClashTestPlan plan = MirrorRuleTests.Plan(
                MirrorRuleTests.Test(MirrorRuleTests.DuctsVsColumns, MirrorRuleTests.Ducts, MirrorRuleTests.Columns),
                MirrorRuleTests.Test(MirrorRuleTests.ColumnsVsDucts, MirrorRuleTests.Columns, MirrorRuleTests.Ducts),
                MirrorRuleTests.Test(MirrorRuleTests.DuctsVsColumns + " (mirror)", MirrorRuleTests.Ducts, MirrorRuleTests.Walls));

            MirrorRule rule = MirrorRuleTests.Rule(plan.Buildable, PriorityMap.NothingPicked());

            Assert.That(rule.Pairs.Count, Is.EqualTo(1));
            Assert.That(rule.Pairs[0].MirrorName, Is.EqualTo(MirrorRuleTests.DuctsVsColumns + " 2 (mirror)"));
            Assert.That(string.Join("\n", new List<string>(rule.Lines()).ToArray()), Does.Contain(
                "created and run as " + MirrorRuleTests.DuctsVsColumns + " 2 (mirror)"));
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
            Assert.That(named.Buildable[1].Name, Is.EqualTo(MirrorRuleTests.DuctsVsColumns + " (mirror)"));
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
                MirrorRuleTests.DuctsVsColumns + " (mirror)", MirrorRuleTests.Columns, MirrorRuleTests.Ducts);
            MirrorRule rule = MirrorRule.Of(plan.Buildable, PriorityMap.NothingPicked(), null, new MirrorSettings());
            ClashTestPlan named = plan.WithMirrorsNamed(rule);

            Assert.That(rule.Pairs.Count, Is.EqualTo(1));
            Assert.That(named.Buildable.Count, Is.EqualTo(2));
            Assert.That(named.Buildable[1].Name, Is.EqualTo(MirrorRuleTests.DuctsVsColumns + " (mirror)"));
            Assert.That(named.Buildable[1].Address[0], Is.EqualTo(1));
        }
    }
}
