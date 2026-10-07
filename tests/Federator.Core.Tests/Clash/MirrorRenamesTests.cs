using System;
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
    /// refused and named where the new name is taken. Since attempt 8 the rule is handed the
    /// tests the document holds, so a mirror is never named onto a name the document holds
    /// for another test, and the mirror an earlier run made of a test the XML now runs under
    /// its own name is renamed back to that name. The set and test names in here are sample
    /// data.
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

        private static ClashTestPlan TheXmlPlan()
        {
            return MirrorRuleTests.Plan(
                MirrorRuleTests.Test(Kept, Ducts, Columns),
                MirrorRuleTests.Test(Swap, Columns, Ducts));
        }

        /// <summary>The rule of the XML over a document holding those saved tests.</summary>
        private static MirrorRule TheXmlRuleOver(ClashTestPlan saved)
        {
            return MirrorRuleTests.RuleOver(TheXmlPlan().Buildable, PriorityMap.NothingPicked(), saved);
        }

        private static string Said(string how)
        {
            return MirrorRule.Prefix + "   " + Swap + ", saved before the mirror rule under the XML's name of the mirror "
                + "of " + Kept + ", " + how;
        }

        /// <summary>The line counting the mirrors an earlier run made renamed back, always written on an XML run.</summary>
        private static string BackCount(int renamed, int handed, int refused)
        {
            return MirrorRule.Prefix + "   " + renamed + " of the " + handed + " tests saved in the document "
                + (renamed == 1 ? "is" : "are") + " renamed to the name of a test the XML runs under its own name, whose "
                + "mirror an earlier run made " + (renamed == 1 ? "it" : "them") + ", and " + refused + " "
                + (refused == 1 ? "is" : "are") + " not";
        }

        [Test]
        public void AnOldTestUnderTheMirrorsXmlNameIsRenamedToTheToolsName()
        {
            MirrorRenames renames = TheXmlRuleOver(
                MirrorRuleTests.SavedWithSides(Kept, Ducts, Columns, Swap, Columns, Ducts)).Renames;

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
                    + Kept + "'s"),
                BackCount(0, 2, 0)
            }));
        }

        // The pairs of the picked matrix are two sets of one rule list, Q121 B, and an old
        // test of one of them is renamed by the same rule the pair was read by.
        [Test]
        public void AnOldMirrorBySetsOfOneRuleListIsRenamed()
        {
            MirrorRule rule = MirrorRuleTests.RuleOver(
                MirrorRuleTests.Plan(
                    MirrorRuleTests.Test(MirrorRuleTests.TelecomVsWalls, MirrorRuleTests.Telecom, Walls),
                    MirrorRuleTests.Test(MirrorRuleTests.TelephoneVsWalls, MirrorRuleTests.Telephone, Walls)).Buildable,
                PriorityMap.NothingPicked(),
                MirrorRuleTests.SavedWithSides(
                    MirrorRuleTests.TelecomVsWalls, MirrorRuleTests.Telecom, Walls,
                    MirrorRuleTests.TelephoneVsWalls, MirrorRuleTests.Telephone, Walls));

            MirrorRenames renames = rule.Renames;

            Assert.That(renames.Planned.Count, Is.EqualTo(1));
            Assert.That(renames.Planned[0].Saved.Name, Is.EqualTo(MirrorRuleTests.TelephoneVsWalls));
            Assert.That(renames.Planned[0].NewName, Is.EqualTo(MirrorRuleTests.TelephoneVsWalls + " (mirror)"));
        }

        // The document already holds a test of the new name, the mirror an earlier run made,
        // its sides the mirror's own, so the old test is not renamed and the line names both.
        [Test]
        public void ARenameWhoseNewNameIsTakenIsRefusedAndNamed()
        {
            MirrorRenames renames = TheXmlRuleOver(MirrorRuleTests.SavedWithSides(
                Kept, Ducts, Columns, Swap, Columns, Ducts, NewName, Columns, Ducts)).Renames;

            Assert.That(renames.Planned, Is.Empty);
            Assert.That(renames.Lines(), Is.EqualTo(new[]
            {
                MirrorRule.Prefix + "   0 of the 3 tests saved in the document are renamed as the mirror they were saved "
                    + "for before the mirror rule, and 1 under the XML's name of a mirror is not",
                Said("is not renamed " + NewName + ", because the document already holds a test of that name, so it is "
                    + "left as it is and not run"),
                BackCount(0, 3, 0)
            }));
        }

        // A test a person changed under the mirror's old name asks another question, so it is
        // not the mirror and is not renamed.
        [Test]
        public void AnOldTestWhoseSidesAskAnotherQuestionIsNotRenamed()
        {
            MirrorRenames renames = TheXmlRuleOver(
                MirrorRuleTests.SavedWithSides(Kept, Ducts, Columns, Swap, Ducts, Walls)).Renames;

            Assert.That(renames.Planned, Is.Empty);
            Assert.That(renames.Lines()[1], Is.EqualTo(Said("is not renamed, because its sides do not ask the question "
                + "of " + Kept + " as a mirror, so it is left as it is and not run")));
        }

        /// <summary>
        /// A document holding the kept test with its sides read and the other test as the
        /// add-in hands it today, its sides placeholders. Since attempt 9 a kept test whose
        /// sides were not read makes no pair, so only the other test's sides are not read here.
        /// </summary>
        private static ClashTestPlan KeptReadAndHanded(string handed)
        {
            return ClashTestPlan.FromDocument(
                new List<SavedClashTest> { MirrorRuleTests.Saved(Kept, Ducts, Columns, 0), MirrorRuleTests.AsHanded(handed, 1) },
                "m");
        }

        // The add-in today hands a saved test's sides as placeholders, so whether the old test
        // is the mirror is UNKNOWN, and it is not renamed.
        [Test]
        public void AnOldTestWhoseSidesWereNotReadIsNotRenamed()
        {
            MirrorRenames renames = TheXmlRuleOver(KeptReadAndHanded(Swap)).Renames;

            Assert.That(renames.Planned, Is.Empty);
            Assert.That(renames.Lines()[1], Is.EqualTo(Said("is not renamed, because whether its sides ask the question "
                + "of " + Kept + " is UNKNOWN, a side or its set not read, so it is left as it is and not run")));
        }

        [Test]
        public void TwoOldTestsOfTheNameAreNotRenamed()
        {
            MirrorRenames renames = TheXmlRuleOver(MirrorRuleTests.SavedWithSides(
                Kept, Ducts, Columns, Swap, Columns, Ducts, Swap, Columns, Ducts)).Renames;

            Assert.That(renames.Planned, Is.Empty);
            Assert.That(renames.Lines()[1], Is.EqualTo(Said("is not renamed, because the document holds 2 tests of "
                + "that name, so which one is the mirror is UNKNOWN and each is left as it is and not run")));
        }

        // An NWF made under the mirror rule holds no test under the mirror's XML name, so
        // nothing is renamed, and the lines say so, because a missing line reads as a check
        // that did not run.
        [Test]
        public void NoOldTestRenamesNothingAndTheLineSaysSo()
        {
            MirrorRenames renames = TheXmlRuleOver(
                MirrorRuleTests.SavedWithSides(Kept, Ducts, Columns, NewName, Columns, Ducts)).Renames;

            Assert.That(renames.Planned, Is.Empty);
            Assert.That(renames.Lines(), Is.EqualTo(new[]
            {
                MirrorRule.Prefix + "   0 of the 2 tests saved in the document are renamed as the mirror they were saved "
                    + "for before the mirror rule, and 0 under the XML's name of a mirror are not",
                BackCount(0, 2, 0)
            }));
        }

        // The old test renamed ends with (mirror), so the run after with no XML pairs it with
        // the kept test by its sides, the same rule.
        [Test]
        public void TheRenamedTestIsTheMirrorARunWithNoXmlPairsAfter()
        {
            MirrorRenames renames = TheXmlRuleOver(
                MirrorRuleTests.SavedWithSides(Kept, Ducts, Columns, Swap, Columns, Ducts)).Renames;
            MirrorRename rename = renames.Planned[0];

            MirrorRule after = MirrorRule.Of(
                MirrorRuleTests.SavedWithSides(
                    Kept, Ducts, Columns,
                    rename.NewName, rename.Saved.Left.Locator, rename.Saved.Right.Locator).Buildable,
                PriorityMap.NothingPicked(),
                null,
                new MirrorSettings(),
                null);

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
            MirrorRule rule = MirrorRuleTests.RuleOver(
                MirrorRuleTests.Plan(
                    MirrorRuleTests.Test(Kept, Ducts, Columns),
                    MirrorRuleTests.Test(NewName, Columns, Ducts)).Buildable,
                PriorityMap.NothingPicked(),
                MirrorRuleTests.SavedWithSides(Kept, Ducts, Columns, NewName, Columns, Ducts));

            MirrorRenames renames = rule.Renames;

            Assert.That(rule.Pairs[0].MirrorName, Is.EqualTo(NewName));
            Assert.That(renames.Planned, Is.Empty);
            Assert.That(renames.Lines(), Is.EqualTo(new[]
            {
                MirrorRule.Prefix + "   0 of the 2 tests saved in the document are renamed as the mirror they were saved "
                    + "for before the mirror rule, and 0 under the XML's name of a mirror are not",
                BackCount(0, 2, 0)
            }));
        }

        // A run with no XML has no mirror of the XML to rename for, so it plans nothing and
        // writes no rename line. An XML run over a document holding no test still writes its
        // two count lines, since a missing line reads as a check that did not run.
        [Test]
        public void ARuleWithNoXmlRenamesNothing()
        {
            ClashTestPlan saved = MirrorRuleTests.SavedWithSides(Kept, Ducts, Columns, NewName, Columns, Ducts);
            MirrorRule rule = MirrorRule.Of(saved.Buildable, PriorityMap.NothingPicked(), null, new MirrorSettings(), null);

            Assert.That(rule.Pairs.Count, Is.EqualTo(1));
            Assert.That(rule.Renames.Planned, Is.Empty);
            Assert.That(rule.Renames.Lines(), Is.Empty);
            Assert.That(TheXmlRuleOver(MirrorRuleTests.NothingSaved()).Renames.Lines(), Is.EqualTo(new[]
            {
                MirrorRule.Prefix + "   0 of the 0 tests saved in the document are renamed as the mirror they were saved "
                    + "for before the mirror rule, and 0 under the XML's name of a mirror are not",
                BackCount(0, 0, 0)
            }));
        }

        // ---------- a change of roles between two XML runs, F132 attempt 8 ----------

        /// <summary>The tests an NWF holds after the first XML run, which kept Ducts against Columns.</summary>
        private static ClashTestPlan AfterTheFirstRun()
        {
            ClashTestPlan xml = TheXmlPlan();
            MirrorRule first = MirrorRuleTests.Rule(xml.Buildable, PriorityMap.NothingPicked());

            return MirrorRuleTests.SavedWithSides(MirrorRuleTests.AsSaved(xml.WithMirrorsNamed(first)));
        }

        /// <summary>The names a document holds once those renames are made.</summary>
        private static List<string> NamesAfter(ClashTestPlan saved, MirrorRenames renames)
        {
            List<string> names = new List<string>();

            foreach (PlannedClashTest test in saved.Buildable)
            {
                string name = test.Name;

                foreach (MirrorRename rename in renames.Planned)
                {
                    if (ReferenceEquals(rename.Saved, test))
                    {
                        name = rename.NewName;
                    }
                }

                names.Add(name);
            }

            return names;
        }

        private static List<string> NamesOf(ClashTestPlan plan)
        {
            List<string> names = new List<string>();

            foreach (PlannedClashTest test in plan.Buildable)
            {
                names.Add(test.Name);
            }

            return names;
        }

        // The reviewer's and the breaker's point on attempt 7, and Bader's words of 2026-10-06.
        // The first XML run kept Ducts against Columns and made its swap Columns against Ducts
        // (mirror). A priority file then puts the swap first, so the second XML run keeps
        // Columns against Ducts and makes Ducts against Columns its mirror. Until attempt 8 the
        // old kept test was renamed as the mirror, a new Columns against Ducts was created
        // beside the old Columns against Ducts (mirror), which asks its question in its order,
        // and every run after counted each clash of that question twice. Now the old mirror is
        // renamed back to the name of the test now kept, where it sits with its statuses, so the
        // document holds exactly the tests the plan runs and no second test of the question
        // runs beside the one kept.
        [Test]
        public void AChangeOfRolesRenamesTheOldMirrorToTheTestNowKept()
        {
            ClashTestPlan saved = AfterTheFirstRun();
            ClashTestPlan again = TheXmlPlan();
            MirrorRule second = MirrorRuleTests.RuleOver(again.Buildable, MirrorRuleTests.Priorities(Swap, "A"), saved);
            MirrorRenames renames = second.Renames;

            Assert.That(NamesOf(saved), Is.EqualTo(new[] { Kept, NewName }), "what the first run left");
            Assert.That(second.Pairs[0].Kept.Name, Is.EqualTo(Swap), "the priority file changed the roles");
            Assert.That(renames.Planned.Count, Is.EqualTo(2));
            Assert.That(renames.Planned[1].Saved.Name, Is.EqualTo(NewName));
            Assert.That(renames.Planned[1].Saved.Address[0], Is.EqualTo(1), "renamed where it sits, with its statuses");
            Assert.That(renames.Planned[1].NewName, Is.EqualTo(Swap));
            Assert.That(NamesAfter(saved, renames), Is.EquivalentTo(NamesOf(again.WithMirrorsNamed(second))),
                "every test the plan runs is found by its name, and nothing is left beside it");
            Assert.That(renames.Lines(), Does.Contain(BackCount(1, 2, 0)));
            Assert.That(renames.Lines(), Does.Contain(MirrorRule.Prefix + "   " + NewName + ", the mirror an earlier run "
                + "made of " + Swap + " with its two sets in its order, is renamed " + Swap + ", its statuses kept, and run "
                + "as " + Swap + ", which the XML runs under its own name, so no second test of its question runs beside it"));
        }

        // The run with no XML after the change of roles pairs the two by their sides and keeps
        // the test the second XML run kept.
        [Test]
        public void TheRunWithNoXmlAfterAChangeOfRolesKeepsTheTestNowKept()
        {
            ClashTestPlan saved = AfterTheFirstRun();
            MirrorRule second = MirrorRuleTests.RuleOver(
                TheXmlPlan().Buildable, MirrorRuleTests.Priorities(Swap, "A"), saved);
            List<string> after = NamesAfter(saved, second.Renames);
            List<string> sides = new List<string>();

            for (int i = 0; i < after.Count; i++)
            {
                sides.Add(after[i]);
                sides.Add(saved.Buildable[i].Left.Locator);
                sides.Add(saved.Buildable[i].Right.Locator);
            }

            MirrorRule weekly = MirrorRule.Of(
                MirrorRuleTests.SavedWithSides(sides.ToArray()).Buildable,
                PriorityMap.NothingPicked(),
                null,
                new MirrorSettings(),
                null);

            Assert.That(weekly.Pairs.Count, Is.EqualTo(1));
            Assert.That(weekly.Pairs[0].Kept.Name, Is.EqualTo(Swap));
            Assert.That(weekly.Pairs[0].Mirror.Name, Is.EqualTo(Kept + " (mirror)"));
        }

        // The XML no longer holds the test the first run kept, and runs its old mirror's test
        // alone under its own name. The old mirror asks that test's question in its order, so
        // it is renamed back too, and no second test of the question is created beside it.
        [Test]
        public void ATestTheXmlNowRunsAloneTakesBackTheMirrorAnEarlierRunMadeOfIt()
        {
            MirrorRule alone = MirrorRuleTests.RuleOver(
                MirrorRuleTests.Plan(MirrorRuleTests.Test(Swap, Columns, Ducts)).Buildable,
                PriorityMap.NothingPicked(),
                AfterTheFirstRun());

            Assert.That(alone.Pairs, Is.Empty);
            Assert.That(alone.Renames.Planned.Count, Is.EqualTo(1));
            Assert.That(alone.Renames.Planned[0].Saved.Name, Is.EqualTo(NewName));
            Assert.That(alone.Renames.Planned[0].NewName, Is.EqualTo(Swap));
        }

        // Two saved tests carry a name this tool gives the test now kept as a mirror, each with
        // its two sets in its order. Which one is its mirror is UNKNOWN, so neither is renamed,
        // and the line says the test is created beside them and a clash may be counted twice.
        [Test]
        public void TwoOldMirrorsOfTheTestNowKeptAreNotRenamedAndSaid()
        {
            MirrorRule second = MirrorRuleTests.RuleOver(
                TheXmlPlan().Buildable,
                MirrorRuleTests.Priorities(Swap, "A"),
                MirrorRuleTests.SavedWithSides(
                    Kept, Ducts, Columns, NewName, Columns, Ducts, Swap + " 2 (mirror)", Columns, Ducts));
            MirrorRenames renames = second.Renames;

            Assert.That(renames.Planned.Count, Is.EqualTo(1), "only the old kept test, renamed as the mirror");
            Assert.That(renames.Lines(), Does.Contain(BackCount(0, 3, 2)));
            Assert.That(renames.Lines(), Does.Contain(MirrorRule.Prefix + "   " + NewName + " and " + Swap + " 2 (mirror) "
                + "carry a name this tool gives " + Swap + " as a mirror, each with its two sets in its order, so which one "
                + "is its mirror is UNKNOWN. None is renamed " + Swap + ", " + Swap + " is created beside them, and a clash "
                + "both find may be counted twice"));
        }

        // The old mirror's sides were not read, as the add-in hands them today, so whether it
        // asks the question of the test now kept is UNKNOWN, and it is not renamed.
        [Test]
        public void AnOldMirrorWhoseSidesWereNotReadIsNotRenamedAndSaid()
        {
            MirrorRule second = MirrorRuleTests.RuleOver(
                TheXmlPlan().Buildable, MirrorRuleTests.Priorities(Swap, "A"), MirrorRuleTests.SavedPlan(Kept, NewName));

            Assert.That(second.Renames.Planned, Is.Empty);
            Assert.That(second.Renames.Lines(), Does.Contain(MirrorRule.Prefix + "   " + NewName + " carries a name this "
                + "tool gives " + Swap + " as a mirror, and whether the sides of " + NewName + " are its two sets in its "
                + "order is UNKNOWN, a side not read. It is not renamed " + Swap + ", " + Swap + " is created beside it, and "
                + "a clash both find may be counted twice"));
        }

        // A saved test under that name whose sides ask another question is not the old mirror
        // of the test now kept, so it is left as it is.
        [Test]
        public void ATestUnderThatNameAskingAnotherQuestionIsNotRenamed()
        {
            MirrorRule second = MirrorRuleTests.RuleOver(
                TheXmlPlan().Buildable,
                MirrorRuleTests.Priorities(Swap, "A"),
                MirrorRuleTests.SavedWithSides(Kept, Ducts, Columns, NewName, Ducts, Walls));

            Assert.That(second.Renames.Planned.Count, Is.EqualTo(1), "only the old kept test, renamed as the mirror");
            Assert.That(second.Renames.Planned[0].Saved.Name, Is.EqualTo(Kept));
            Assert.That(second.Renames.Lines(), Does.Contain(BackCount(0, 2, 0)));
        }

        // ---------- the names the document holds, F132 attempt 8 ----------

        // The reviewer's point on attempt 7. The document holds a test under the name the
        // mirror would take whose sides ask another question, a person's or one left behind.
        // Until attempt 8 the plan found it by that name and ran it as the mirror, so its
        // clashes were added to the kept test as found by the mirror only. Now that name is
        // taken, the mirror is created and run under the next number, and the line says why.
        [Test]
        public void ATestTheDocumentHoldsUnderTheMirrorsNameAskingAnotherQuestionIsNotRunAsTheMirror()
        {
            ClashTestPlan xml = TheXmlPlan();
            MirrorRule rule = MirrorRuleTests.RuleOver(
                xml.Buildable, PriorityMap.NothingPicked(),
                MirrorRuleTests.SavedWithSides(Kept, Ducts, Columns, NewName, Ducts, Walls));

            Assert.That(rule.Pairs[0].MirrorName, Is.EqualTo(Swap + " 2 (mirror)"));
            Assert.That(NamesOf(xml.WithMirrorsNamed(rule)), Does.Not.Contain(NewName));
            Assert.That(rule.Renames.Lines(), Does.Contain(MirrorRule.Prefix + "   the document holds a test named "
                + NewName + " whose sides ask another question than " + Kept + "'s, so the mirror " + Swap + " of " + Kept
                + " is created and run as " + Swap + " 2 (mirror)"));
        }

        // Where whether the test under the name is the mirror is UNKNOWN, its sides not read or
        // two tests of the name, it is not run as the mirror either.
        [Test]
        public void ATestUnderTheMirrorsNameThatMayNotBeItIsNotRunAsTheMirror()
        {
            MirrorRule notRead = TheXmlRuleOver(KeptReadAndHanded(NewName));
            MirrorRule twoOfIt = TheXmlRuleOver(MirrorRuleTests.SavedWithSides(
                Kept, Ducts, Columns, NewName, Columns, Ducts, NewName, Columns, Ducts));

            Assert.That(notRead.Pairs[0].MirrorName, Is.EqualTo(Swap + " 2 (mirror)"));
            Assert.That(notRead.Renames.Lines(), Does.Contain(MirrorRule.Prefix + "   the document holds a test named "
                + NewName + " whose sides were not read, and whether it is that mirror is UNKNOWN, so the mirror " + Swap
                + " of " + Kept + " is created and run as " + Swap + " 2 (mirror)"));
            Assert.That(twoOfIt.Pairs[0].MirrorName, Is.EqualTo(Swap + " 2 (mirror)"));
            Assert.That(twoOfIt.Renames.Lines(), Does.Contain(MirrorRule.Prefix + "   the document holds 2 tests named "
                + NewName + ", and which one would run as the mirror is UNKNOWN, so the mirror " + Swap + " of " + Kept
                + " is created and run as " + Swap + " 2 (mirror)"));
        }

        // The mirror an earlier run made is the very test the XML would create, its sides the
        // mirror's own, so a rerun gives it the same name and finds it, and makes no second one.
        // A saved test whose sides ask the kept test's question by sets of one rule list is that
        // mirror as well.
        [Test]
        public void ARerunFindsTheMirrorTheRunBeforeMadeUnderTheSameName()
        {
            ClashTestPlan xml = TheXmlPlan();
            MirrorRule first = MirrorRuleTests.Rule(xml.Buildable, PriorityMap.NothingPicked());
            MirrorRule again = TheXmlRuleOver(MirrorRuleTests.SavedWithSides(MirrorRuleTests.AsSaved(xml.WithMirrorsNamed(first))));

            Assert.That(again.Pairs[0].MirrorName, Is.EqualTo(NewName));
            Assert.That(again.Renames.Planned, Is.Empty);

            MirrorRule byRuleList = MirrorRuleTests.RuleOver(
                MirrorRuleTests.Plan(
                    MirrorRuleTests.Test(MirrorRuleTests.TelecomVsWalls, MirrorRuleTests.Telecom, Walls),
                    MirrorRuleTests.Test(MirrorRuleTests.TelephoneVsWalls, MirrorRuleTests.Telephone, Walls)).Buildable,
                PriorityMap.NothingPicked(),
                MirrorRuleTests.SavedWithSides(
                    MirrorRuleTests.TelecomVsWalls, MirrorRuleTests.Telecom, Walls,
                    MirrorRuleTests.TelephoneVsWalls + " (mirror)", Walls, MirrorRuleTests.Telecom));

            Assert.That(byRuleList.Pairs[0].MirrorName, Is.EqualTo(MirrorRuleTests.TelephoneVsWalls + " (mirror)"));
        }

        // The breaker's point on attempt 7. The document holds the new name under a test of a
        // type this tool does not run, which ClashTestPlan.FromDocument leaves out of the tests
        // it plans. Until attempt 8 the old test was renamed onto that name, so two tests
        // shared it. Every name the document holds is read now, so the mirror takes the next
        // number and the old test is renamed to it.
        [Test]
        public void ARenameNeverLandsOnANameATestOfAnotherTypeHolds()
        {
            List<SavedClashTest> saved = new List<SavedClashTest>
            {
                MirrorRuleTests.Saved(Kept, Ducts, Columns, 0),
                MirrorRuleTests.Saved(Swap, Columns, Ducts, 1),
                new SavedClashTest(NewName, 99, 0.025, true, false, 1, Ducts, false, 1, Walls, new[] { 2 })
            };

            ClashTestPlan document = ClashTestPlan.FromDocument(saved, "m");
            MirrorRule rule = TheXmlRuleOver(document);

            Assert.That(document.Buildable.Count, Is.EqualTo(2), "the plan off the document leaves the other type out");
            Assert.That(rule.Renames.Planned.Count, Is.EqualTo(1));
            Assert.That(rule.Renames.Planned[0].NewName, Is.EqualTo(Swap + " 2 (mirror)"));
            Assert.That(rule.Renames.Lines(), Does.Contain(MirrorRule.Prefix + "   the document holds a test named "
                + NewName + " of a type this tool does not run, so the mirror " + Swap + " of " + Kept + " is created and "
                + "run as " + Swap + " 2 (mirror)"));
        }

        // ---------- the kept test's own name in the document, F132 attempt 9 ----------

        /// <summary>The line of a pair not made because of what the document holds under the kept test's name.</summary>
        private static string NotPairedFor(string held, bool unknown)
        {
            return MirrorRule.Prefix + "   the document holds " + held + ". A test is run by its name, so " + Swap
                + " is not paired with it as a mirror and keeps its own clashes"
                + (unknown ? ", and a clash both find may be counted twice" : string.Empty);
        }

        // The breaker's point on attempt 8. Week one the XML ran Ducts against Walls under the
        // kept test's name and the NWF saved it. The XML now runs that name on Ducts against
        // Columns, and its swap beside it. A test is run by its name and a drifted test is left
        // as it is, so the document's test asking Ducts against Walls runs. Until attempt 9 the
        // two were paired on the XML's sets and every clash only the swap found was added to a
        // test of another question. Now they are not paired and the line says why.
        [Test]
        public void ATestTheDocumentHoldsUnderTheKeptTestsNameAskingAnotherQuestionIsNotPaired()
        {
            ClashTestPlan xml = TheXmlPlan();
            MirrorRule rule = TheXmlRuleOver(MirrorRuleTests.SavedWithSides(Kept, Ducts, Walls));

            Assert.That(rule.Pairs, Is.Empty, "no clash of the swap goes under a test asking Ducts against Walls");
            Assert.That(NamesOf(xml.WithMirrorsNamed(rule)), Is.EqualTo(new[] { Kept, Swap }));
            Assert.That(rule.Renames.Planned, Is.Empty);
            Assert.That(rule.Renames.Lines(), Does.Contain(NotPairedFor(
                "a test named " + Kept + " whose sides ask another question than the XML's " + Kept, false)));
        }

        // Where what the test of the kept test's name asks is UNKNOWN, its sides not read, two
        // tests of the name, a type this tool does not run, or a set whose rule list was not
        // read, the two are not paired either, and the line says a clash may be counted twice.
        [Test]
        public void ATestUnderTheKeptTestsNameThatMayAskAnotherQuestionIsNotPaired()
        {
            string elsewhere = "lcop_selection_set_tree/Mechanical/BLD-ME-Ducts Elsewhere";
            MirrorRule notRead = TheXmlRuleOver(MirrorRuleTests.SavedPlan(Kept));
            MirrorRule twoOfIt = TheXmlRuleOver(MirrorRuleTests.SavedWithSides(
                Kept, Ducts, Columns, Kept, Ducts, Columns));
            MirrorRule otherType = TheXmlRuleOver(ClashTestPlan.FromDocument(
                new List<SavedClashTest> { new SavedClashTest(Kept, 99, 0.025, true, false, 1, Ducts, false, 1, Columns, new[] { 0 }) },
                "m"));
            MirrorRule noRules = TheXmlRuleOver(MirrorRuleTests.SavedWithSides(Kept, elsewhere, Columns));

            Assert.That(notRead.Pairs, Is.Empty);
            Assert.That(notRead.Renames.Lines(), Does.Contain(NotPairedFor("a test named " + Kept + " whose sides were "
                + "not read, and whether it asks the XML's question of " + Kept + " is UNKNOWN", true)));
            Assert.That(twoOfIt.Pairs, Is.Empty);
            Assert.That(twoOfIt.Renames.Lines(), Does.Contain(NotPairedFor("2 tests named " + Kept + ", and which one "
                + "would run as " + Kept + " is UNKNOWN", true)));
            Assert.That(otherType.Pairs, Is.Empty);
            Assert.That(otherType.Renames.Lines(), Does.Contain(NotPairedFor("a test named " + Kept + " of a type this "
                + "tool does not run", true)));
            Assert.That(noRules.Pairs, Is.Empty);
            Assert.That(noRules.Renames.Lines(), Does.Contain(NotPairedFor("a test named " + Kept + ", and whether its "
                + "sides ask the XML's question of " + Kept + " is UNKNOWN, a set's rule list not read", true)));
        }

        // The test of the kept test's name asks the XML's question, in its order or swapped, so
        // what runs under that name asks what the pair was judged on, and the two are paired.
        [Test]
        public void ATestUnderTheKeptTestsNameAskingItsQuestionIsPaired()
        {
            MirrorRule inOrder = TheXmlRuleOver(MirrorRuleTests.SavedWithSides(Kept, Ducts, Columns));
            MirrorRule swapped = TheXmlRuleOver(MirrorRuleTests.SavedWithSides(Kept, Columns, Ducts));

            Assert.That(inOrder.Pairs.Count, Is.EqualTo(1));
            Assert.That(inOrder.Pairs[0].MirrorName, Is.EqualTo(NewName));
            Assert.That(swapped.Pairs.Count, Is.EqualTo(1));
            Assert.That(swapped.Pairs[0].MirrorName, Is.EqualTo(NewName));
        }

        /// <summary>The rule of the XML over a document holding only that one test of the kept test's name.</summary>
        private static MirrorRule OverTheKeptTestSavedAs(SavedClashTest saved)
        {
            return TheXmlRuleOver(ClashTestPlan.FromDocument(new List<SavedClashTest> { saved }, "m"));
        }

        /// <summary>What the document holds under the kept test's name where a setting of it was changed by hand.</summary>
        private static string ChangedByHand(string differences)
        {
            return "a test named " + Kept + " that asks the XML's question of " + Kept + " at other settings, the XML's "
                + "first: " + differences;
        }

        // The breaker's first finding on attempt 9. Week two of an XML run, the document's test
        // of the kept test's name asks the XML's question, but a person changed a setting of it
        // by hand. A drifted test is left as it is and runs at the document's settings, and the
        // mirror is created at the XML's, so a clash the mirror finds that it does not would be
        // added to it though it never found it. Until attempt 10 the two were paired on the
        // sets alone. Now they are not paired and the line names each setting, the mirror's
        // side set against the side it stands for where the test is saved swapped.
        [Test]
        public void ATestUnderTheKeptTestsNameWithASettingChangedByHandIsNotPaired()
        {
            MirrorRule tolerance = OverTheKeptTestSavedAs(
                new SavedClashTest(Kept, 1, 0.05, true, false, 1, Ducts, false, 1, Columns, new[] { 0 }));
            MirrorRule type = OverTheKeptTestSavedAs(
                new SavedClashTest(Kept, 2, 0.025, true, false, 1, Ducts, false, 1, Columns, new[] { 0 }));
            MirrorRule merge = OverTheKeptTestSavedAs(
                new SavedClashTest(Kept, 1, 0.025, false, false, 1, Ducts, false, 1, Columns, new[] { 0 }));
            MirrorRule primitives = OverTheKeptTestSavedAs(
                new SavedClashTest(Kept, 1, 0.025, true, false, 1, Ducts, false, 3, Columns, new[] { 0 }));
            MirrorRule selfSwapped = OverTheKeptTestSavedAs(
                new SavedClashTest(Kept, 1, 0.025, true, true, 1, Columns, false, 1, Ducts, new[] { 0 }));

            Assert.That(tolerance.Pairs, Is.Empty, "no clash the mirror found at 0.025 m goes under a test run at 0.05 m");
            Assert.That(tolerance.Renames.Lines(), Does.Contain(NotPairedFor(
                ChangedByHand("the tolerance 0.025 m and 0.05 m"), true)));
            Assert.That(type.Pairs, Is.Empty);
            Assert.That(type.Renames.Lines(), Does.Contain(NotPairedFor(
                ChangedByHand("the test type HardConservative and Clearance"), true)));
            Assert.That(merge.Pairs, Is.Empty);
            Assert.That(merge.Renames.Lines(), Does.Contain(NotPairedFor(ChangedByHand("merge composites on and off"), true)));
            Assert.That(primitives.Pairs, Is.Empty);
            Assert.That(primitives.Renames.Lines(), Does.Contain(NotPairedFor(
                ChangedByHand("the right side primitive types 1 and 3"), true)));
            Assert.That(selfSwapped.Pairs, Is.Empty);
            Assert.That(selfSwapped.Renames.Lines(), Does.Contain(NotPairedFor(
                ChangedByHand("the right side self intersect off and on"), true)));
        }

        // ---------- what the rule is handed, F132 attempt 8 ----------

        // An XML run names its mirrors against every test the document holds, so a rule over
        // the XML's tests with no document is refused rather than naming a mirror blind.
        [Test]
        public void AnXmlRuleWithNoDocumentIsRefused()
        {
            Assert.Throws<ArgumentNullException>(() => MirrorRule.Of(
                TheXmlPlan().Buildable, PriorityMap.NothingPicked(), null, new MirrorSettings(), null));
        }

        // The document is the plan read off the document, never a plan of an XML.
        [Test]
        public void ADocumentThatIsAnXmlsPlanIsRefused()
        {
            Assert.Throws<ArgumentException>(() => MirrorRule.Of(
                TheXmlPlan().Buildable, PriorityMap.NothingPicked(), null, new MirrorSettings(), TheXmlPlan()));
        }

        // The saved tests are handed once: as the tests where no XML was picked, as the
        // document where one was.
        [Test]
        public void SavedTestsHandedBothWaysAreRefused()
        {
            ClashTestPlan saved = MirrorRuleTests.SavedWithSides(Kept, Ducts, Columns, NewName, Columns, Ducts);

            Assert.Throws<ArgumentException>(() => MirrorRule.Of(
                saved.Buildable, PriorityMap.NothingPicked(), null, new MirrorSettings(), saved));
        }
    }
}
