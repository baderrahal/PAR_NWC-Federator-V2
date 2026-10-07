using System.Collections.Generic;
using System.IO;
using System.Text;
using Federator.Core.Exchange;
using Federator.Core.Sets;
using NUnit.Framework;

namespace Federator.Core.Tests
{
    /// <summary>
    /// The also-ask line of the list of corrections, F131, FR-181: the correction drafted for a
    /// silent miss, approved by Bader by copying it into the list beside the XML, never written
    /// there by the tool. One line holds a value a set asks and every other spelling it also
    /// accepts, and every group asking one of them is copied once for each, the spellings in
    /// Ordinal order, the way the both-spellings rows of Q102 are built. The set and the spellings
    /// are SAMPLE DATA, measure-teams.md section 6.
    /// </summary>
    [TestFixture]
    public class AlsoAskTests
    {
        private const string Draft = "also-ask: ME-DUCTWORK | HV-Ductwork";

        private const string Category = "LcRevitPropertyElementCategory";

        private const string CannotBeUsed =
            "is an also-ask that cannot be used: it needs the value a set asks and at least one other spelling to accept"
            + " beside it, each named once";

        private static MatrixCorrectionList ListOf(string text)
        {
            return MatrixCorrectionList.Read(new StringReader(text), "a list in a test");
        }

        /// <summary>The client's matrix as it came, asking ME-DUCTWORK alone, the text the corrections are applied to.</summary>
        private static string TheClientsXml()
        {
            return ExchangeReader.ReadFileText(Samples.Matrix());
        }

        [Test]
        public void AnAlsoAskLineIsReadAndCountedOnTheFirstLine()
        {
            MatrixCorrectionList list = ListOf("# a comment\n" + Draft + "\nalso-ask: ME-Piping | HV-Piping | FP-Piping\n" + Draft + "\n");

            Assert.That(list.Unread, Is.Null);
            Assert.That(list.HoldsNone, Is.False);
            Assert.That(list.AlsoAsks.Count, Is.EqualTo(2), "a line written twice is kept once");
            TeamMapTests.Same(list.AlsoAsks[0], "ME-DUCTWORK", "HV-Ductwork");
            TeamMapTests.Same(list.AlsoAsks[1], "ME-Piping", "HV-Piping", "FP-Piping");
            Assert.That(list.Said(), Is.EqualTo(
                "the corrections are read from a list in a test, the list beside this file. It holds 2 corrections,"
                    + " 0 renames, 0 catch-alls, 0 Source File rules and 2 also-asks, and 0 workset spellings"));
        }

        /// <summary>
        /// An also-ask that cannot be used unreads the whole list with its line, as every line of the
        /// list does. A spelling named on two lines is one too, because two lines sharing a spelling
        /// would ask each value where the other is, or move on every run, so a value and every
        /// spelling it also accepts go on one line.
        /// </summary>
        [Test]
        public void AnAlsoAskThatCannotBeUsedUnreadsTheList()
        {
            foreach (string line in new[]
            {
                "also-ask: ME-Ductwork | ME-Ductwork",
                "also-ask: ME-Ductwork",
                "also-ask:  | HV-Ductwork",
                "also-ask: ME-Ductwork | ",
                "also-ask: ME-Ductwork | HV-Ductwork | HV-Ductwork"
            })
            {
                MatrixCorrectionList list = ListOf("rename: BLD-X | BLD-Y\n" + line + "\n");

                Assert.That(list.Unread, Is.EqualTo("line 2, \"" + line + "\", " + CannotBeUsed), line);
                Assert.That(list.Renames.Count, Is.EqualTo(0), line);
            }

            Assert.That(
                ListOf("also-ask: ME-Piping | HV-Piping\nalso-ask: ME-Piping | FP-Piping\n").Unread,
                Is.EqualTo("line 2, \"also-ask: ME-Piping | FP-Piping\", names ME-Piping, which line 1 names already."
                    + " A value and every spelling it also accepts go on one line"));
            Assert.That(
                ListOf("also-ask: ME-PIPING | HV-Piping\nalso-ask: FP-PIPING | HV-Piping\n").Unread,
                Is.EqualTo("line 2, \"also-ask: FP-PIPING | HV-Piping\", names HV-Piping, which line 1 names already."
                    + " A value and every spelling it also accepts go on one line"));
        }

        /// <summary>
        /// The draft of measure-teams.md section 6 for BLD-ME-Ducts&amp;Duct Fittings, its four groups,
        /// each still asking its own category, the spellings in Ordinal order as Q102's are, so the
        /// group asking HV-Ductwork comes first and each group after the first starts with flags 64.
        /// </summary>
        [Test]
        public void AnAlsoAskBuildsTheFourGroupsOfTheDraft()
        {
            CorrectionOutcome outcome = MatrixCorrections.ForPickedFile(TheClientsXml(), ListOf(Draft + "\n"));
            SelectionSetDefinition ducts = new List<SelectionSetDefinition>(new ExchangeReader().ReadText(outcome.Text).Sets)
                .Find(set => set.Name == "BLD-ME-Ducts&Duct Fittings");
            IList<IList<SearchConditionDefinition>> groups = PlannedSet.GroupsOf(
                ducts.Conditions, condition => PlannedCondition.StartsAGroupWith(condition.Flags));
            string[][] expected =
            {
                new[] { "Ducts", "HV-Ductwork" },
                new[] { "Ducts", "ME-DUCTWORK" },
                new[] { "Duct Fittings", "HV-Ductwork" },
                new[] { "Duct Fittings", "ME-DUCTWORK" }
            };

            Assert.That(groups.Count, Is.EqualTo(4));

            for (int i = 0; i < expected.Length; i++)
            {
                Assert.That(groups[i].Count, Is.EqualTo(2), "group " + i);
                Assert.That(groups[i][0].Property.InternalName, Is.EqualTo(Category), "group " + i);
                Assert.That(groups[i][0].Value.Data, Is.EqualTo(expected[i][0]), "group " + i);
                Assert.That(groups[i][1].Value.Data, Is.EqualTo(expected[i][1]), "group " + i);
                Assert.That(groups[i][0].Flags, Is.EqualTo(i == 0 ? 0 : 64), "group " + i);
            }

            Assert.That(outcome.Lines(), Has.Member(
                "MATRIX   the value ME-DUCTWORK also accepts HV-Ductwork, a line of the list beside this file  5 occurrences"));
        }

        [Test]
        public void AnAlsoAskAppliedTwiceChangesNothingTheSecondTime()
        {
            MatrixCorrectionList list = ListOf(Draft + "\n");
            CorrectionOutcome once = MatrixCorrections.ForPickedFile(TheClientsXml(), list);
            CorrectionOutcome twice = MatrixCorrections.ForPickedFile(once.Text, list);

            Assert.That(once.TotalChanged, Is.EqualTo(5));
            Assert.That(twice.Text, Is.EqualTo(once.Text));
            Assert.That(twice.TotalChanged, Is.EqualTo(0));
        }

        /// <summary>
        /// The drafted line copied into this project's own list, beside its workset spellings that
        /// ask ME-DUCTWORK or ME-Ductwork already, Q102: the picked file gains the HV group beside
        /// them, and a second run on its own output changes nothing.
        /// </summary>
        [Test]
        public void TheDraftInThisProjectsListChangesNothingOnASecondRun()
        {
            MatrixCorrectionList list = ListOf(File.ReadAllText(Samples.CorrectionList()) + Draft + "\n");
            CorrectionOutcome once = MatrixCorrections.ForPickedFile(TheClientsXml(), list);
            CorrectionOutcome twice = MatrixCorrections.ForPickedFile(once.Text, list);

            Assert.That(list.Unread, Is.Null);
            Assert.That(once.Text, Does.Contain("<data type=\"wstring\">HV-Ductwork</data>"));
            Assert.That(twice.Text, Is.EqualTo(once.Text));
            Assert.That(twice.TotalChanged, Is.EqualTo(0));
        }

        [Test]
        public void AnAlsoAskForAValueNoSetAsksIsSaidAndChangesNothing()
        {
            string xml = TheClientsXml();
            CorrectionOutcome outcome = MatrixCorrections.ForPickedFile(xml, ListOf("also-ask: HV-Nothing | HV-Other\n"));

            Assert.That(outcome.Text, Is.EqualTo(xml));
            Assert.That(outcome.TotalChanged, Is.EqualTo(0));
            Assert.That(outcome.Lines(), Has.Member(
                "MATRIX   the value HV-Nothing also accepts HV-Other, a line of the list beside this file"
                    + "  0 occurrences. this file holds no condition asking for any spelling the line names"));
        }

        /// <summary>
        /// The spellings the list says a model carries: its workset lines, then every spelling an
        /// also-ask line accepts beside its value, each once. The value of an also-ask line is what
        /// a set asks, measured or not, so it is not among them, and the workset lines and the
        /// count on the first line are as they were.
        /// </summary>
        [Test]
        public void TheListsSpellingsAreItsWorksetLinesThenEverySpellingAnAlsoAskAccepts()
        {
            MatrixCorrectionList list = ListOf(
                "workset: ME-Ductwork\n" + Draft + "\nalso-ask: ME-Piping | HV-Piping | ME-Ductwork | HV-Ductwork-2\n");

            Assert.That(list.Unread, Is.Null);
            TeamMapTests.Same(list.Spellings, "ME-Ductwork", "HV-Ductwork", "HV-Piping", "HV-Ductwork-2");
            TeamMapTests.Same(list.Worksets, "ME-Ductwork");
            Assert.That(list.Said(), Does.EndWith(", and 1 workset spelling"));
        }

        /// <summary>
        /// The readers' finding on attempt 1, against F116's rule that the census and the list are
        /// put together in one place, RevitWorksets.With. With the draft copied into this project's
        /// list, the mechanical sets ask HV-Ductwork, first in Ordinal order, and the judge of a set
        /// that found nothing was handed the census and the workset lines alone, so it called
        /// HV-Ductwork a value NO MODEL IN THIS PROJECT CARRIES although the line was drafted from
        /// a model measured carrying it. Every value the corrected file asks is now one the judge
        /// says models in this project carry, and so is BLD-ME-Ducts&amp;Duct Fittings judged on
        /// its own conditions.
        /// </summary>
        [Test]
        public void EverySpellingAnAlsoAskLineAcceptsIsOneTheEmptySetJudgeKnows()
        {
            string folder = TempFolder.Make("f131-judge");

            try
            {
                string picked = Path.Combine(folder, "picked.xml");
                File.Copy(Samples.Matrix(), picked);
                File.WriteAllText(
                    new CorrectionListSettings().PathBeside(picked),
                    File.ReadAllText(Samples.CorrectionList()) + Draft + "\n",
                    new UTF8Encoding(false));

                ExchangeDocument document = MatrixCorrections.ReadPicked(picked);
                SetBuildPlan plan = SetBuildPlan.From(document);
                IList<string> asked = MatrixCorrections.WorksetValuesIn(document);
                List<string> calledWrong = new List<string>();

                foreach (string value in asked)
                {
                    EmptySet why = EmptySets.Why(
                        "a/" + value,
                        new List<ReadCondition> { new ReadCondition(string.Empty, EmptySets.WorksetProperty, SetBuildPlan.EqualsTest, value) },
                        new EmptySetJudge(plan.Worksets, RevitWorksets.Project, null));

                    if (why.Reason != EmptyReason.TheValueIsThereAnyway)
                    {
                        calledWrong.Add(why.Line());
                    }
                }

                Assert.That(asked, Has.Member("HV-Ductwork"));
                Assert.That(calledWrong, Is.Empty);

                SelectionSetDefinition ducts = new List<SelectionSetDefinition>(document.Sets)
                    .Find(set => set.Name == "BLD-ME-Ducts&Duct Fittings");
                List<ReadCondition> itsConditions = new List<ReadCondition>();

                foreach (SearchConditionDefinition condition in ducts.Conditions)
                {
                    itsConditions.Add(new ReadCondition(string.Empty, condition.Property.InternalName, condition.Test, condition.Value.Data));
                }

                Assert.That(itsConditions[1].Value, Is.EqualTo("HV-Ductwork"), "the spelling the judge reads first");

                EmptySet judged = EmptySets.Why("a/BLD-ME-Ducts&Duct Fittings", itsConditions, new EmptySetJudge(plan.Worksets, RevitWorksets.Project, null));

                Assert.That(judged.Reason, Is.EqualTo(EmptyReason.TheValueIsThereAnyway), judged.Line());
            }
            finally
            {
                TempFolder.Remove(folder);
            }
        }
    }
}
