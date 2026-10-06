using System.Collections.Generic;
using Federator.Core.Clash;
using NUnit.Framework;

namespace Federator.Core.Tests
{
    /// <summary>
    /// F132, Bader's answer A to Q136. An NWF made before the mirror rule holds the mirror of
    /// a kept test under the XML's own name of it, Y. With the XML picked, each such saved
    /// test whose sides ask the kept test's question as a mirror is planned to be renamed to
    /// its own name with (mirror) at the end, Y (mirror), never the kept test's, its
    /// statuses kept, and run as the mirror, its clashes merged as Q133 D says. A rename is
    /// refused and named where the new name is taken. The set and test names in here are
    /// sample data.
    /// </summary>
    [TestFixture]
    public class MirrorRenamesTests
    {
        private const string Ducts = MirrorRuleTests.Ducts;
        private const string Columns = MirrorRuleTests.Columns;
        private const string Walls = MirrorRuleTests.Walls;
        private const string Kept = MirrorRuleTests.DuctsVsColumns;
        private const string Swap = MirrorRuleTests.ColumnsVsDucts;
        private const string NewName = Swap + " (mirror)";

        private static MirrorRule TheXmlRule()
        {
            return MirrorRuleTests.Rule(
                MirrorRuleTests.Plan(
                    MirrorRuleTests.Test(Kept, Ducts, Columns),
                    MirrorRuleTests.Test(Swap, Columns, Ducts)).Buildable,
                PriorityMap.NothingPicked());
        }

        private static string Said(string how)
        {
            return MirrorRule.Prefix + "   " + Swap + ", saved before the mirror rule under the XML's name of the mirror "
                + "of " + Kept + ", " + how;
        }

        [Test]
        public void AnOldTestUnderTheMirrorsXmlNameIsRenamedToTheToolsName()
        {
            MirrorRenames renames = TheXmlRule().RenamesIn(
                MirrorRuleTests.SavedWithSides(Kept, Ducts, Columns, Swap, Columns, Ducts).Buildable);

            Assert.That(renames.Planned.Count, Is.EqualTo(1));
            Assert.That(renames.Planned[0].Saved.Name, Is.EqualTo(Swap));
            Assert.That(renames.Planned[0].Saved.Address[0], Is.EqualTo(1), "renamed where it sits");
            Assert.That(renames.Planned[0].NewName, Is.EqualTo(NewName));
            Assert.That(renames.Planned[0].NewName, Is.Not.EqualTo(Kept + " (mirror)"), "never the kept test's name");
            Assert.That(renames.Planned[0].KeptName, Is.EqualTo(Kept));
            Assert.That(renames.Lines(), Is.EqualTo(new[]
            {
                MirrorRule.Prefix + "   1 of the 2 tests saved in the document is renamed as the mirror it was saved for "
                    + "before the mirror rule, and 0 under the XML's name of a mirror are not",
                Said("is renamed " + NewName + ", its statuses kept, and run as that mirror, its clashes merged into "
                    + Kept + "'s")
            }));
        }

        // The pairs of the picked matrix are two sets of one rule list, Q121 B, and an old
        // test of one of them is renamed by the same rule the pair was read by.
        [Test]
        public void AnOldMirrorBySetsOfOneRuleListIsRenamed()
        {
            MirrorRule rule = MirrorRuleTests.Rule(
                MirrorRuleTests.Plan(
                    MirrorRuleTests.Test(MirrorRuleTests.TelecomVsWalls, MirrorRuleTests.Telecom, Walls),
                    MirrorRuleTests.Test(MirrorRuleTests.TelephoneVsWalls, MirrorRuleTests.Telephone, Walls)).Buildable,
                PriorityMap.NothingPicked());

            MirrorRenames renames = rule.RenamesIn(MirrorRuleTests.SavedWithSides(
                MirrorRuleTests.TelecomVsWalls, MirrorRuleTests.Telecom, Walls,
                MirrorRuleTests.TelephoneVsWalls, MirrorRuleTests.Telephone, Walls).Buildable);

            Assert.That(renames.Planned.Count, Is.EqualTo(1));
            Assert.That(renames.Planned[0].Saved.Name, Is.EqualTo(MirrorRuleTests.TelephoneVsWalls));
            Assert.That(renames.Planned[0].NewName, Is.EqualTo(MirrorRuleTests.TelephoneVsWalls + " (mirror)"));
        }

        // The document already holds a test of the new name, a mirror an earlier run made or
        // a person's own, so the old test is not renamed and the line names both.
        [Test]
        public void ARenameWhoseNewNameIsTakenIsRefusedAndNamed()
        {
            MirrorRenames renames = TheXmlRule().RenamesIn(MirrorRuleTests.SavedWithSides(
                Kept, Ducts, Columns, Swap, Columns, Ducts, NewName, Columns, Ducts).Buildable);

            Assert.That(renames.Planned, Is.Empty);
            Assert.That(renames.Lines(), Is.EqualTo(new[]
            {
                MirrorRule.Prefix + "   0 of the 3 tests saved in the document are renamed as the mirror they were saved "
                    + "for before the mirror rule, and 1 under the XML's name of a mirror is not",
                Said("is not renamed " + NewName + ", because the document already holds a test of that name, so it is "
                    + "left as it is and not run")
            }));
        }

        // A test a person changed under the mirror's old name asks another question, so it is
        // not the mirror and is not renamed.
        [Test]
        public void AnOldTestWhoseSidesAskAnotherQuestionIsNotRenamed()
        {
            MirrorRenames renames = TheXmlRule().RenamesIn(
                MirrorRuleTests.SavedWithSides(Kept, Ducts, Columns, Swap, Ducts, Walls).Buildable);

            Assert.That(renames.Planned, Is.Empty);
            Assert.That(renames.Lines()[1], Is.EqualTo(Said("is not renamed, because its sides do not ask the question "
                + "of " + Kept + " as a mirror, so it is left as it is and not run")));
        }

        // The add-in today hands a saved test's sides as placeholders, so whether the old test
        // is the mirror is UNKNOWN, and it is not renamed.
        [Test]
        public void AnOldTestWhoseSidesWereNotReadIsNotRenamed()
        {
            MirrorRenames renames = TheXmlRule().RenamesIn(MirrorRuleTests.SavedPlan(Kept, Swap).Buildable);

            Assert.That(renames.Planned, Is.Empty);
            Assert.That(renames.Lines()[1], Is.EqualTo(Said("is not renamed, because whether its sides ask the question "
                + "of " + Kept + " is UNKNOWN, a side or its set not read, so it is left as it is and not run")));
        }

        [Test]
        public void TwoOldTestsOfTheNameAreNotRenamed()
        {
            MirrorRenames renames = TheXmlRule().RenamesIn(MirrorRuleTests.SavedWithSides(
                Kept, Ducts, Columns, Swap, Columns, Ducts, Swap, Columns, Ducts).Buildable);

            Assert.That(renames.Planned, Is.Empty);
            Assert.That(renames.Lines()[1], Is.EqualTo(Said("is not renamed, because the document holds 2 tests of "
                + "that name, so which one is the mirror is UNKNOWN and each is left as it is and not run")));
        }

        // An NWF made under the mirror rule holds no test under the mirror's XML name, so
        // nothing is renamed, and the line says so, because a missing line reads as a check
        // that did not run.
        [Test]
        public void NoOldTestRenamesNothingAndTheLineSaysSo()
        {
            MirrorRenames renames = TheXmlRule().RenamesIn(
                MirrorRuleTests.SavedWithSides(Kept, Ducts, Columns, NewName, Columns, Ducts).Buildable);

            Assert.That(renames.Planned, Is.Empty);
            Assert.That(renames.Lines(), Is.EqualTo(new[]
            {
                MirrorRule.Prefix + "   0 of the 2 tests saved in the document are renamed as the mirror they were saved "
                    + "for before the mirror rule, and 0 under the XML's name of a mirror are not"
            }));
        }

        // The old test renamed ends with (mirror), so the run after with no XML pairs it with
        // the kept test by its sides, the same rule.
        [Test]
        public void TheRenamedTestIsTheMirrorARunWithNoXmlPairsAfter()
        {
            MirrorRenames renames = TheXmlRule().RenamesIn(
                MirrorRuleTests.SavedWithSides(Kept, Ducts, Columns, Swap, Columns, Ducts).Buildable);
            MirrorRename rename = renames.Planned[0];

            MirrorRule after = MirrorRule.Of(
                MirrorRuleTests.SavedWithSides(
                    Kept, Ducts, Columns,
                    rename.NewName, rename.Saved.Left.Locator, rename.Saved.Right.Locator).Buildable,
                PriorityMap.NothingPicked(),
                null,
                new MirrorSettings());

            Assert.That(after.Pairs.Count, Is.EqualTo(1));
            Assert.That(after.Pairs[0].Kept.Name, Is.EqualTo(Kept));
            Assert.That(after.Pairs[0].Mirror.Name, Is.EqualTo(NewName));
        }

        // A mirror whose XML name already ends with (mirror) keeps that name, so the saved
        // test of that name is the mirror as it stands. Nothing is renamed and nothing is
        // refused, and it runs as the mirror where it sits.
        [Test]
        public void AMirrorWhoseXmlNameAlreadyEndsWithItIsNeitherRenamedNorRefused()
        {
            MirrorRule rule = MirrorRuleTests.Rule(
                MirrorRuleTests.Plan(
                    MirrorRuleTests.Test(Kept, Ducts, Columns),
                    MirrorRuleTests.Test(NewName, Columns, Ducts)).Buildable,
                PriorityMap.NothingPicked());

            MirrorRenames renames = rule.RenamesIn(
                MirrorRuleTests.SavedWithSides(Kept, Ducts, Columns, NewName, Columns, Ducts).Buildable);

            Assert.That(rule.Pairs[0].MirrorName, Is.EqualTo(NewName));
            Assert.That(renames.Planned, Is.Empty);
            Assert.That(renames.Lines(), Is.EqualTo(new[]
            {
                MirrorRule.Prefix + "   0 of the 2 tests saved in the document are renamed as the mirror they were saved "
                    + "for before the mirror rule, and 0 under the XML's name of a mirror are not"
            }));
        }

        // A run with no XML has no mirror of the XML to rename for.
        [Test]
        public void ARuleWithNoXmlRenamesNothing()
        {
            ClashTestPlan saved = MirrorRuleTests.SavedWithSides(Kept, Ducts, Columns, NewName, Columns, Ducts);
            MirrorRule rule = MirrorRule.Of(saved.Buildable, PriorityMap.NothingPicked(), null, new MirrorSettings());

            Assert.That(rule.Pairs.Count, Is.EqualTo(1));
            Assert.That(rule.RenamesIn(saved.Buildable).Planned, Is.Empty);
            Assert.That(rule.RenamesIn(new List<PlannedClashTest>()).Lines().Count, Is.EqualTo(1));
        }
    }
}
