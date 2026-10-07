using System.Collections.Generic;
using Federator.Core.Exchange;
using Federator.Core.Health;
using Federator.Core.Sets;
using NUnit.Framework;

namespace Federator.Core.Tests.Health
{
    /// <summary>
    /// PART 5. Two faults that are invisible until a set finds nothing or a report column
    /// comes out blank. The numbers are the ones read off the client's real groups on
    /// 2026-09-20, docs\history\scan.md 5q.
    /// </summary>
    [TestFixture]
    public class ExportCheckTests
    {
        private static ModelExport Model(string discipline, int elements, int worksets, int ids, params string[] names)
        {
            return new ModelExport(
                "1104-PAR-1A02MM-ZZZ-" + discipline + "-MOD-000001.nwc",
                discipline,
                elements,
                worksets,
                ids,
                new List<string>(names));
        }

        private static string Joined(IList<string> lines)
        {
            return string.Join("\n", new List<string>(lines).ToArray());
        }

        /// <summary>A run with no clash file picked, so no set says what it asks.</summary>
        private static readonly IList<SelectionSetDefinition> NoFile = null;

        /// <summary>The sets of a clash file, read the way the run reads the picked one.</summary>
        private static IList<SelectionSetDefinition> SetsOf(string path)
        {
            return new ExchangeReader().ReadFile(path).Sets;
        }

        /// <summary>One set with one condition, for the shapes the client's files do not hold.</summary>
        private static IList<SelectionSetDefinition> OneSet(string name, string test, string property, string value)
        {
            return new ExchangeReader().ReadText(
                "<exchange units=\"ft\"><selectionsets><selectionset name=\"" + name + "\">"
                + "<findspec mode=\"all\" disjoint=\"0\"><conditions>"
                + "<condition test=\"" + test + "\" flags=\"0\">"
                + "<category><name internal=\"LcRevitData_Element\">Element</name></category>"
                + "<property><name internal=\"" + property + "\">a property</name></property>"
                + "<value><data type=\"wstring\">" + value + "</data></value></condition>"
                + "</conditions></findspec></selectionset></selectionsets></exchange>").Sets;
        }

        /// <summary>One workset condition with those flags, 64 starting an Or group and 32 negating it.</summary>
        private static string WorksetCondition(int flags, string value)
        {
            return "<condition test=\"equals\" flags=\"" + flags + "\">"
                + "<category><name internal=\"LcRevitData_Element\">Element</name></category>"
                + "<property><name internal=\"" + EmptySets.WorksetProperty + "\">Workset</name></property>"
                + "<value><data type=\"wstring\">" + value + "</data></value></condition>";
        }

        /// <summary>The sets of a clash file holding those sets, each a name and its conditions.</summary>
        private static IList<SelectionSetDefinition> SetsAsking(params string[] nameThenConditions)
        {
            string sets = string.Empty;

            for (int i = 0; i + 1 < nameThenConditions.Length; i += 2)
            {
                sets += "<selectionset name=\"" + nameThenConditions[i] + "\"><findspec mode=\"all\" disjoint=\"0\"><conditions>"
                    + nameThenConditions[i + 1] + "</conditions></findspec></selectionset>";
            }

            return new ExchangeReader().ReadText("<exchange units=\"ft\"><selectionsets>" + sets + "</selectionsets></exchange>").Sets;
        }

        /// <summary>The real 1A02MM, where every model carries both on every element.</summary>
        private static IList<ModelExport> TheRealGroup()
        {
            return new List<ModelExport>
            {
                Model("AR", 86, 86, 86, "AR-EXTERIOR", "AR-INTERIOR"),
                Model("ME", 236, 236, 236, "ME-Ductwork", "ME-Piping", "PL-Drainage")
            };
        }

        /// <summary>
        /// Each model's line carries how many of its elements hold a workset, beside how
        /// many workset NAMES it carries, because the names count alone read as a count of
        /// elements and the elements on a workset were never printed, T1-S50.
        /// </summary>
        [Test]
        public void EveryModelIsNamedWithItsElementsItsWorksetsAndItsIdShare()
        {
            string block = Joined(ExportCheck.Lines(TheRealGroup(), NoFile));

            Assert.That(block, Does.Contain(
                "AR  1104-PAR-1A02MM-ZZZ-AR-MOD-000001.nwc   elements 86   with a workset 86   worksets 2   element id 100%"));
            Assert.That(block, Does.Contain(
                "ME  1104-PAR-1A02MM-ZZZ-ME-MOD-000001.nwc   elements 236   with a workset 236   worksets 3   element id 100%"));
        }

        // ---------- T1-S50, every element is a claim that needs counting ----------

        /// <summary>
        /// The closing sentence said every element carries a workset when the only
        /// workset test was whether ANY element did, so one element of a thousand passed.
        /// The sets that filter on workset then find nothing for the other 999 and nothing
        /// warned.
        /// </summary>
        [Test]
        public void AModelWithAWorksetOnOneElementOfAThousandIsNotCalledWhole()
        {
            IList<ModelExport> models = new List<ModelExport> { Model("ME", 1000, 1, 1000, "ME-Piping") };

            string block = Joined(ExportCheck.Lines(models, NoFile));

            Assert.That(block, Does.Not.Contain("carry a workset on every element"));
            Assert.That(block, Does.Contain("elements 1000   with a workset 1   worksets 1"));
            Assert.That(block, Does.Contain(
                "only 1 of 1000 element(s) carry a workset. A set that filters on workset finds none of the other 999 here"));
            Assert.That(block, Does.Contain("0 of 1 model(s) carry no workset at all, and 1 carry a workset on only some of their elements"));
        }

        [Test]
        public void AModelWhoseWalkThrewIsNotCalledWhole()
        {
            IList<ModelExport> models = new List<ModelExport>
            {
                Model("AR", 86, 86, 86, "AR-EXTERIOR"),
                Model("ST", ModelExport.NotCounted, ModelExport.NotCounted, ModelExport.NotCounted)
            };

            string block = Joined(ExportCheck.Lines(models, NoFile));

            Assert.That(block, Does.Not.Contain("carry a workset on every element"));
            Assert.That(block, Does.Contain("with a workset UNKNOWN"));
            Assert.That(block, Does.Contain("0 of 2 model(s) carry no workset at all, and 1 could not be counted"));
        }

        /// <summary>Every element of none is not every element, and a model with no Revit element was called whole.</summary>
        [Test]
        public void AModelWithNoRevitElementIsNotCalledWhole()
        {
            IList<ModelExport> models = new List<ModelExport> { Model("EL", 0, 0, 0) };

            string block = Joined(ExportCheck.Lines(models, NoFile));

            Assert.That(block, Does.Not.Contain("carry a workset on every element"));
            Assert.That(block, Does.Contain("no item in this model is a Revit element, so it has no workset and no element id to check"));
            Assert.That(block, Does.Contain("0 of 1 model(s) carry no workset at all, and 1 hold no Revit element"));
        }

        [Test]
        public void TheRuleTheRunLineCountsIsTheRuleTheBlockUses()
        {
            ModelExport none = Model("EL", 494, 0, 494);
            ModelExport some = Model("ME", 1000, 1, 1000, "ME-Piping");
            ModelExport every = Model("AR", 86, 86, 86, "AR-EXTERIOR");
            ModelExport threw = Model("ST", ModelExport.NotCounted, ModelExport.NotCounted, ModelExport.NotCounted);

            Assert.That(none.CarriesNoWorkset, Is.True);
            Assert.That(some.CarriesNoWorkset, Is.False);
            Assert.That(some.CarriesAWorksetOnSomeElements, Is.True);
            Assert.That(every.CarriesAWorksetOnSomeElements, Is.False);
            Assert.That(every.CarriesNoWorkset, Is.False);
            Assert.That(threw.Counted, Is.False);
            Assert.That(threw.CarriesNoWorkset, Is.False, "a count nobody took is never called none");
            Assert.That(threw.CarriesAWorksetOnSomeElements, Is.False);
        }

        /// <summary>
        /// The whole point of the block. The names are what a person holds beside the
        /// matrix, and on 2026-09-20 that comparison was the answer to why 33 sets found
        /// nothing: the models carry ME-Ductwork and the matrix asks for ME-DUCTWORK. That
        /// comparison is now made against the sets of the picked file, the client's own
        /// matrix here, and names each set and both spellings.
        ///
        /// THIS REPLACES A TEST THAT PINNED ONE SENTENCE, S03-2. The block wrote 'The match
        /// is CASE SENSITIVE, so ME-Ductwork does not match ME-DUCTWORK' in all 22 groups of
        /// set 03 whether any name differed or not, 1B06BC among them, whose models carry
        /// the capitals, log lines 210 and 4294. The rule changed, so the test changed.
        /// </summary>
        [Test]
        public void TheWorksetNamesAreListedAndASetAskingThemInAnotherCaseIsNamed()
        {
            string block = Joined(ExportCheck.Lines(TheRealGroup(), SetsOf(Samples.Matrix())));

            Assert.That(block, Does.Contain("worksets seen: AR-EXTERIOR, AR-INTERIOR, ME-Ductwork, ME-Piping, PL-Drainage"));
            Assert.That(block, Does.Contain(
                "2 pair(s) of workset names differ by letter case alone, one asked by a set of the picked file"
                + " and one carried by a model here. The match is CASE SENSITIVE"));
            Assert.That(block, Does.Contain(
                "   the file asks for \"ME-DUCTWORK\" in BLD-ME-Ducts&Duct Fittings, BLD-ME-Duct Accessory,"
                + " BLD-ME-Flex Ducts, and BLD-ME-Air Terminals, and a model here carries \"ME-Ductwork\""));
            Assert.That(block, Does.Contain(
                "   the file asks for \"ME-PIPING\" in BLD-ME-Pipes&Pipe Fittings, BLD-ME-Pipe Accessories,"
                + " BLD-ME-Plumbing Fixtures, and BLD-ME-Flex Pipes, and a model here carries \"ME-Piping\""));
            Assert.That(block, Does.Not.Contain("PL-Drainage\" in"), "asked and carried in the same case, so not named");
        }

        /// <summary>
        /// The other half of the proof: a group where no set misses a name by letter case alone
        /// prints no warning. Since F116 the corrected matrix asks every spelling measured, so a
        /// set asking ME-DUCTWORK also asks ME-Ductwork, the spelling this group carries.
        /// </summary>
        [Test]
        public void AGroupWhereNoAskedNameDiffersByCaseGetsNoWarning()
        {
            string block = Joined(ExportCheck.Lines(TheRealGroup(), SetsOf(Samples.CorrectedMatrix())));

            Assert.That(block, Does.Not.Contain("CASE SENSITIVE"));
            Assert.That(block, Does.Not.Contain("the file asks for"));
            Assert.That(block, Does.Contain(
                "no workset name a set of the picked file asks for differs by letter case alone from one a model here carries"));
        }

        /// <summary>
        /// The real 1B06BC of the C06 run, log line 605: its models carry the capitals. A file
        /// asking the title case alone, as the corrected matrix did until F116, lands the same
        /// trouble on this group the other way round, Q102, and the rule names it in whichever
        /// direction it runs. The corrected matrix since F116 asks both spellings in every set
        /// asking one, so against it nothing is named for this group.
        /// </summary>
        [Test]
        public void TheCapitalsOf1B06BCAgainstAFileAskingTitleCaseAreNamed()
        {
            IList<ModelExport> models = new List<ModelExport>
            {
                new ModelExport("1104-PAR-1B06BC-ZZZ-ME-MOD-000001.nwc", "ME", 100, 100, 100,
                    new List<string> { "ME-DUCTWORK", "ME-EQUIPMENT", "ME-PIPING" })
            };

            IList<SelectionSetDefinition> titleCase = SetsAsking(
                "BLD-ME-Ducts&amp;Duct Fittings", WorksetCondition(0, "ME-Ductwork"),
                "BLD-ME-Mechanical Equipment", WorksetCondition(0, "ME-Equipment"),
                "BLD-ME-Pipes&amp;Pipe Fittings", WorksetCondition(0, "ME-Piping"));

            string block = Joined(ExportCheck.Lines(models, titleCase));

            Assert.That(block, Does.Contain("3 pair(s) of workset names differ by letter case alone"));
            Assert.That(block, Does.Contain(
                "the file asks for \"ME-Equipment\" in BLD-ME-Mechanical Equipment, and a model here carries \"ME-EQUIPMENT\""));
            Assert.That(block, Does.Contain("and a model here carries \"ME-DUCTWORK\""));

            string corrected = Joined(ExportCheck.Lines(models, SetsOf(Samples.CorrectedMatrix())));

            Assert.That(corrected, Does.Not.Contain("CASE SENSITIVE"), corrected);
            Assert.That(corrected, Does.Contain(
                "no workset name a set of the picked file asks for differs by letter case alone from one a model here carries"));
        }

        /// <summary>
        /// A set that also asks the carried spelling exactly is not named as missing it, F116 once
        /// F112 merged. The corrections ask a workset in every spelling measured, so a set asks
        /// ME-DUCTWORK or ME-Ductwork and finds the items carrying either, and naming it would be
        /// the overclaim this check is there to end. A set asking the other spelling alone is
        /// still named, and so is one asking the carried spelling negated, which finds none of
        /// those items.
        /// </summary>
        [Test]
        public void ASetThatAlsoAsksTheCarriedSpellingIsNotNamedAsMissingIt()
        {
            IList<SelectionSetDefinition> sets = SetsAsking(
                "BLD-Both", WorksetCondition(0, "ME-DUCTWORK") + WorksetCondition(64, "ME-Ductwork"),
                "BLD-Capitals", WorksetCondition(0, "ME-DUCTWORK"),
                "BLD-Negated", WorksetCondition(0, "ME-DUCTWORK") + WorksetCondition(32, "ME-Ductwork"));

            string block = Joined(ExportCheck.Lines(TheRealGroup(), sets));

            Assert.That(block, Does.Contain("1 pair(s) of workset names differ by letter case alone"), block);
            Assert.That(block, Does.Contain(
                "   the file asks for \"ME-DUCTWORK\" in BLD-Capitals, and BLD-Negated, and a model here carries \"ME-Ductwork\""), block);
            Assert.That(block, Does.Not.Contain("BLD-Both"), block);
        }

        [Test]
        public void WithNoFilePickedNoNameIsComparedAndTheBlockSaysSo()
        {
            string block = Joined(ExportCheck.Lines(TheRealGroup(), NoFile));

            Assert.That(block, Does.Not.Contain("CASE SENSITIVE"));
            Assert.That(block, Does.Contain(
                "no set was read from a picked file, so no workset name was compared with what a set asks"));
        }

        [Test]
        public void AFileWhoseSetsAskForNoWorksetSaysThereWasNothingToCompare()
        {
            string block = Joined(ExportCheck.Lines(
                TheRealGroup(), OneSet("BLD-AR-Walls", "equals", "LcRevitPropertyElementCategory", "Walls")));

            Assert.That(block, Does.Not.Contain("CASE SENSITIVE"));
            Assert.That(block, Does.Contain("no set of the picked file asks for a workset, so there was nothing to compare"));
        }

        /// <summary>A contains condition asks for part of a name, so it is compared as part of one.</summary>
        [Test]
        public void AContainsConditionIsComparedAsPartOfAName()
        {
            string named = Joined(ExportCheck.Lines(
                TheRealGroup(), OneSet("BLD-ME-Ducts", "contains", "lcldrevit_parameter_-1002053", "DUCT")));
            string matched = Joined(ExportCheck.Lines(
                TheRealGroup(), OneSet("BLD-ME-Ducts", "contains", "lcldrevit_parameter_-1002053", "Duct")));

            Assert.That(named, Does.Contain(
                "the file asks for \"DUCT\" in BLD-ME-Ducts, and a model here carries \"ME-Ductwork\""));
            Assert.That(matched, Does.Not.Contain("CASE SENSITIVE"));
        }

        [Test]
        public void AModelCarryingNoWorksetAtAllSaysEverySetFilteringOnOneWillFindNothing()
        {
            IList<ModelExport> models = new List<ModelExport> { Model("EL", 494, 0, 494) };

            string block = Joined(ExportCheck.Lines(models, NoFile));

            Assert.That(block, Does.Contain("worksets NONE"));
            Assert.That(block, Does.Contain("no element carries a workset. Every set that filters on workset will find nothing here"));
            Assert.That(block, Does.Contain("1 of 1 model(s) carry no workset at all"));
        }

        /// <summary>
        /// The C02 run of 2026-09-20 had one group with 192 of 790 ids blank, which is
        /// the Revit exporter's Convert element Ids setting being off.
        /// </summary>
        [Test]
        public void AModelMissingSomeIdsIsToldWhichExportSettingToSwitchOn()
        {
            IList<ModelExport> models = new List<ModelExport> { Model("ME", 790, 790, 598, "ME-Piping") };

            string block = Joined(ExportCheck.Lines(models, NoFile));

            Assert.That(block, Does.Contain("element id 76%"));
            Assert.That(block, Does.Contain("re-export with Convert element Ids switched on"));
            Assert.That(block, Does.Contain("192 element(s) reach the report with an empty id cell"));
        }

        // ---------- T1-S48, a share rounded to 100 hid the missing ids ----------

        /// <summary>
        /// 1051 ids on 1052 elements is 99.9 per cent and read as 100, so no re-export line
        /// was written, the blank id cell was not counted, and the closing sentence said
        /// every element carries an id. Up to 50 blank cells hid that way on a model of
        /// 10,000 elements. The test is on the counts and never on the rounded share.
        /// </summary>
        [Test]
        public void OneMissingIdInAThousandAndFiftyTwoIsSaidAndNotCalledWhole()
        {
            IList<ModelExport> models = new List<ModelExport> { Model("ME", 1052, 1052, 1051, "ME-Piping") };

            string block = Joined(ExportCheck.Lines(models, NoFile));

            Assert.That(block, Does.Contain(
                "re-export with Convert element Ids switched on, or 1 element(s) reach the report with an empty id cell"));
            Assert.That(block, Does.Not.Contain("carry a workset on every element and an element id on every element"));
            Assert.That(block, Does.Contain("1 do not carry an element id on every element"));
        }

        /// <summary>
        /// The share is still a whole per cent to read, but it says 100 only when every
        /// element carries an id and 0 only when none does, so it can never sit beside a
        /// line about missing ids reading 100.
        /// </summary>
        [Test]
        public void TheShareReadsAHundredOnlyWhenEveryIdIsThereAndNoughtOnlyWhenNoneIs()
        {
            Assert.That(Model("ME", 1052, 1052, 1051).IdShare, Is.EqualTo(99));
            Assert.That(Model("ME", 1000, 1000, 1).IdShare, Is.EqualTo(1));
            Assert.That(Model("ME", 1000, 1000, 1000).IdShare, Is.EqualTo(100));
            Assert.That(Model("ME", 1000, 1000, 0).IdShare, Is.EqualTo(0));
            Assert.That(Model("ME", 790, 790, 598).IdShare, Is.EqualTo(76));
            Assert.That(Joined(ExportCheck.Lines(new List<ModelExport> { Model("ME", 1052, 1052, 1051) }, NoFile)),
                Does.Contain("element id 99%"));
        }

        /// <summary>The run line counts a missing id by the rule the block uses, so the two cannot disagree.</summary>
        [Test]
        public void AMissingIdIsCountedByOneRule()
        {
            Assert.That(Model("ME", 1052, 1052, 1051).MissesAnId, Is.True);
            Assert.That(Model("ME", 1052, 1052, 1052).MissesAnId, Is.False);
            Assert.That(Model("ST", ModelExport.NotCounted, ModelExport.NotCounted, ModelExport.NotCounted).MissesAnId, Is.False,
                "a count nobody took is never reported as a fault");
            Assert.That(Model("EL", 0, 0, 0).MissesAnId, Is.False);
        }

        [Test]
        public void AGroupWithNothingWrongSaysSoRatherThanSayingNothing()
        {
            Assert.That(
                Joined(ExportCheck.Lines(TheRealGroup(), NoFile)),
                Does.Contain("all 2 model(s) carry a workset on every element and an element id on every element"));
        }

        /// <summary>A count that could not be taken is UNKNOWN and never zero, which is the rule the census keeps.</summary>
        [Test]
        public void AModelThatCouldNotBeCountedReadsUnknownAndNotZero()
        {
            IList<ModelExport> models = new List<ModelExport>
            {
                Model("ST", ModelExport.NotCounted, ModelExport.NotCounted, ModelExport.NotCounted)
            };

            string block = Joined(ExportCheck.Lines(models, NoFile));

            Assert.That(block, Does.Contain("elements UNKNOWN"));
            Assert.That(block, Does.Contain("element id UNKNOWN"));
            Assert.That(block, Does.Not.Contain("re-export with Convert element Ids"),
                "a count nobody took is never reported as a fault");
        }

        // ---------- T1-S49, a workset count nobody took ----------

        /// <summary>
        /// A model whose walk threw printed worksets NONE beside elements UNKNOWN, which
        /// states a workset count nobody took, because the not counted value read as no
        /// workset.
        /// </summary>
        [Test]
        public void AModelThatCouldNotBeCountedSaysItsWorksetsAreUnknownAndNotNone()
        {
            IList<ModelExport> models = new List<ModelExport>
            {
                Model("ST", ModelExport.NotCounted, ModelExport.NotCounted, ModelExport.NotCounted, "ST-Framing")
            };

            string block = Joined(ExportCheck.Lines(models, NoFile));

            Assert.That(block, Does.Contain("worksets UNKNOWN"));
            Assert.That(block, Does.Not.Contain("worksets NONE"));
            Assert.That(block, Does.Contain("its elements could not be counted, so it is not called whole and none of its workset names is listed"));
        }

        /// <summary>
        /// The walk that threw still handed on the names it had gathered so far, and they
        /// were listed as seen and compared for typos. Part of a list is not a list, the
        /// same as part of a count is not a count.
        /// </summary>
        [Test]
        public void ThePartlyGatheredNamesOfAModelThatCouldNotBeCountedAreNotOfferedAsSeen()
        {
            ModelExport threw = Model(
                "EL", ModelExport.NotCounted, ModelExport.NotCounted, ModelExport.NotCounted, "EL-Lightining Protection");

            IList<ModelExport> models = new List<ModelExport>
            {
                Model("EL", 308, 308, 308, "EL-Lightning Protection"),
                threw
            };

            string block = Joined(ExportCheck.Lines(models, NoFile));

            Assert.That(threw.Worksets.Count, Is.EqualTo(0));
            Assert.That(block, Does.Contain("worksets seen: EL-Lightning Protection"));
            Assert.That(block, Does.Not.Contain("EL-Lightining Protection"),
                "a partly gathered name is neither listed as seen nor named as a typo");
        }

        /// <summary>The row file said 0 workset(s) for the same model, so its words come from the line's rule.</summary>
        [Test]
        public void TheRowFileCarriesTheWordsOfTheModelsLine()
        {
            ModelExport threw = Model("ST", ModelExport.NotCounted, ModelExport.NotCounted, ModelExport.NotCounted, "ST-Framing");
            ModelExport real = Model("AR", 86, 86, 86, "AR-EXTERIOR", "AR-INTERIOR");

            Assert.That(
                ExportCheck.Counts(threw),
                Is.EqualTo("elements UNKNOWN   with a workset UNKNOWN   worksets UNKNOWN   element id UNKNOWN"));
            Assert.That(
                ExportCheck.Counts(real),
                Is.EqualTo("elements 86   with a workset 86   worksets 2   element id 100%"));
            Assert.That(
                Joined(ExportCheck.Lines(new List<ModelExport> { real }, NoFile)),
                Does.Contain("AR  1104-PAR-1A02MM-ZZZ-AR-MOD-000001.nwc   " + ExportCheck.Counts(real)));
        }

        /// <summary>
        /// The row file's number column carried -1 for a model whose walk threw, and a sum
        /// over a run took -1 for each. It carries what the row file carries for an unknown
        /// elsewhere, an empty field, the way a placement that was not read leaves its Z
        /// empty on the model placement row.
        /// </summary>
        [Test]
        public void TheRowFileNumberIsEmptyForAModelThatCouldNotBeCountedAndNeverMinusOne()
        {
            ModelExport threw = Model("ST", ModelExport.NotCounted, ModelExport.NotCounted, ModelExport.NotCounted);

            Assert.That(ExportCheck.ElementsNumber(threw), Is.EqualTo(string.Empty));
            Assert.That(ExportCheck.ElementsNumber(Model("AR", 1052, 1052, 1051)), Is.EqualTo("1052"));
            Assert.That(ExportCheck.ElementsNumber(Model("EL", 0, 0, 0)), Is.EqualTo("0"), "nought counted is a real count");
        }

        // ---------- the run line, the breaker's fifth finding at c5d8aa8 ----------

        /// <summary>
        /// A model holding no Revit element was counted by its group's block and by nothing
        /// in the engine, so the run line read as a clean run beside a block naming it. The
        /// run line is now added up in Core by the rule the block uses.
        /// </summary>
        [Test]
        public void TheRunLineCountsAModelHoldingNoRevitElementAsTheBlockDoes()
        {
            ModelExport empty = Model("EL", 0, 0, 0);
            ExportCheckAcrossTheRun run = new ExportCheckAcrossTheRun();
            run.Add(empty);
            run.Add(Model("AR", 86, 86, 86, "AR-EXTERIOR"));

            Assert.That(empty.HoldsNoElement, Is.True);
            Assert.That(Joined(ExportCheck.Lines(new List<ModelExport> { empty }, NoFile)), Does.Contain("1 hold no Revit element"));
            Assert.That(run.Line(), Is.EqualTo(
                "EXPORT CHECK across the run: 0 model(s) carry no workset at all, 0 carry one on only some of their"
                + " elements, 0 do not carry an element id on every element, 1 hold no Revit element, and 0 could not be"
                + " counted. Nothing was changed in any model."));
        }

        [Test]
        public void TheRunLineAddsEveryKindByTheBlocksRules()
        {
            ExportCheckAcrossTheRun run = new ExportCheckAcrossTheRun();
            run.Add(Model("EL", 494, 0, 494));
            run.Add(Model("ME", 1000, 1, 1000, "ME-Piping"));
            run.Add(Model("AR", 1052, 1052, 1051, "AR-EXTERIOR"));
            run.Add(Model("ST", ModelExport.NotCounted, ModelExport.NotCounted, ModelExport.NotCounted));

            Assert.That(run.Line(), Is.EqualTo(
                "EXPORT CHECK across the run: 1 model(s) carry no workset at all, 1 carry one on only some of their"
                + " elements, 1 do not carry an element id on every element, 0 hold no Revit element, and 1 could not be"
                + " counted. Nothing was changed in any model."));
        }

        /// <summary>
        /// The breaker's note on attempt 2, the export twin of its fourth finding at c5d8aa8. A
        /// group whose models could not be read added nothing, so the run line read as a clean
        /// run over a group nobody checked. The line says how many groups were not read whole
        /// and is never clean while one was not.
        /// </summary>
        [Test]
        public void AGroupWhoseModelsWereNotReadNeverLeavesTheRunLineClean()
        {
            ExportCheckAcrossTheRun run = new ExportCheckAcrossTheRun();
            run.Add(Model("AR", 86, 86, 86, "AR-EXTERIOR"));
            run.GroupNotRead();

            Assert.That(run.Line(), Is.EqualTo(
                "EXPORT CHECK across the run: 0 model(s) carry no workset at all, 0 carry one on only some of their"
                + " elements, 0 do not carry an element id on every element, 0 hold no Revit element, and 0 could not be"
                + " counted. The models of 1 group(s) were not all read, so these counts may leave some out. Nothing was"
                + " changed in any model."));
        }

        [Test]
        public void ACleanRunLineHasNoTail()
        {
            ExportCheckAcrossTheRun run = new ExportCheckAcrossTheRun();
            run.Add(Model("AR", 86, 86, 86, "AR-EXTERIOR"));

            Assert.That(run.Line(), Is.EqualTo(
                "EXPORT CHECK across the run: 0 model(s) carry no workset at all, 0 carry one on only some of their"
                + " elements, 0 do not carry an element id on every element, 0 hold no Revit element, and 0 could not be"
                + " counted"));
        }

        [Test]
        public void ItNamesTenWorksetsAndSaysHowManyItLeftOut()
        {
            // Names far enough apart that none is near another, or the disagreement
            // block under this one would name them and this test would be measuring
            // two things at once. WS-1 and WS-11 are one letter apart.
            string[] apart =
            {
                "Alpha", "Bravo", "Charlie", "Delta", "Echo", "Foxtrot", "Golf",
                "Hotel", "India", "Juliett", "Kilo", "Lima", "Mike", "November"
            };

            List<string> many = new List<string>(apart);

            IList<ModelExport> models = new List<ModelExport>
            {
                new ModelExport("a.nwc", "ME", 100, 100, 100, many)
            };

            string block = Joined(ExportCheck.Lines(models, NoFile));

            Assert.That(block, Does.Contain("Juliett"));
            Assert.That(block, Does.Not.Contain("Kilo"));
            Assert.That(block, Does.Contain("and 4 more, counted and not listed"));
        }

        /// <summary>
        /// F116. The block names ten and counts the rest, so in nine groups of set 03 on C06,
        /// whose worksets seen lines end "and N more, counted and not listed" at lines 605,
        /// 1354, 3248, 3928, 4670, 5071, 6482, 6853 and 7254 of its log, which spelling the
        /// models past the tenth name carry was UNKNOWN, and the matrix corrections act on
        /// exactly that. The row file carries every name of a model in
        /// full, in the order the model gave them, split by a bar so a name holding a comma
        /// stays one name.
        /// </summary>
        [Test]
        public void TheRowFileCarriesEveryWorksetOfAModelWhereTheBlockCountsTheRest()
        {
            List<string> many = new List<string>
            {
                "Alpha", "Bravo", "Charlie", "Delta", "Echo", "Foxtrot", "Golf",
                "Hotel", "India", "Juliett", "Kilo", "Lima", "Mike", "November, and more"
            };

            string every = ExportCheck.EveryWorkset(new ModelExport("a.nwc", "ME", 100, 100, 100, many));

            Assert.That(every.Split(new[] { " | " }, System.StringSplitOptions.None), Is.EqualTo(many));
            Assert.That(every, Does.Not.Contain("counted and not listed"));
            Assert.That(ExportCheck.EveryWorkset(Model("EL", 494, 0, 494)), Is.Empty);
            Assert.That(ExportCheck.WorksetCount(new ModelExport("a.nwc", "ME", 100, 100, 100, many)), Is.EqualTo("14"));
        }

        /// <summary>
        /// F116. A model whose element walk stopped part way hands back the worksets it saw
        /// before it stopped, and those are not every workset of the model. So the row leaves
        /// the count empty and says UNKNOWN, never a short list as every workset, because these
        /// rows are what the next spelling decision is measured from.
        /// </summary>
        [Test]
        public void TheRowFileSaysUnknownWhereAModelsWalkDidNotFinish()
        {
            ModelExport stopped = new ModelExport(
                "a.nwc", "ME", ModelExport.NotCounted, ModelExport.NotCounted, ModelExport.NotCounted,
                new List<string> { "ME-Ductwork", "ME-Piping" });

            Assert.That(ExportCheck.EveryWorkset(stopped), Does.StartWith("UNKNOWN"));
            Assert.That(ExportCheck.EveryWorkset(stopped), Does.Not.Contain("ME-Ductwork"));
            Assert.That(ExportCheck.WorksetCount(stopped), Is.Empty);
        }

        [Test]
        public void WithNoModelAtAllItSaysSoRatherThanWritingAnEmptyBlock()
        {
            Assert.That(
                Joined(ExportCheck.Lines(new List<ModelExport>(), NoFile)),
                Does.Contain("no model was read, so nothing could be checked"));
        }

        // ---------- PART 8, the check that was crying wolf ----------

        /// <summary>
        /// The run of 2026-09-20 named `AR-EXTERIOR` against `AR-INTERIOR` on every group,
        /// and those are two real worksets, which 5t settled. A check that flags something
        /// known good is one a person learns to skip past, and skipping past it costs the
        /// three real typos beside it. So a person decided once, the decision lives in the
        /// measured list, and the tool reads it instead of asking again.
        /// </summary>
        [Test]
        public void APairAPersonHasDecidedAboutIsCountedAndNotNamed()
        {
            IList<ModelExport> models = new List<ModelExport>
            {
                Model("AR", 86, 86, 86, "AR-EXTERIOR", "AR-INTERIOR")
            };

            string block = Joined(ExportCheck.Lines(models, NoFile));

            Assert.That(block, Does.Not.Contain("\"AR-EXTERIOR\" in"),
                "a person has already decided these are two different worksets");
            Assert.That(block, Does.Contain("a person has already"));
            Assert.That(block, Does.Contain("NOT listed"),
                "and it says it left them out rather than going quiet about them");
        }

        /// <summary>The real typo is still named, which is the whole point of not going quiet.</summary>
        [Test]
        public void ARealTypoIsStillNamedBesideTheDecidedPair()
        {
            IList<ModelExport> models = new List<ModelExport>
            {
                Model("AR", 86, 86, 86, "AR-EXTERIOR", "AR-INTERIOR"),
                Model("EL", 308, 308, 308, "EL-Lightning Protection", "EL-Lightining Protection")
            };

            string block = Joined(ExportCheck.Lines(models, NoFile));

            Assert.That(block, Does.Contain("EL-Lightining Protection"));
            Assert.That(block, Does.Contain("1 pair(s) of workset names are close enough"));
            Assert.That(block, Does.Not.Contain("\"AR-EXTERIOR\" in"));
        }

        /// <summary>
        /// WHERE THE LIST OF DECIDED PAIRS COULD NOT BE READ THE BLOCK SAYS UNKNOWN, FR-012. It
        /// read as a list with no pair in it, so AR-EXTERIOR against AR-INTERIOR, which a person
        /// decided are two worksets, was named as a typo with nothing saying why.
        /// </summary>
        [Test]
        public void TheBlockSaysUnknownWhereTheDecidedPairsCouldNotBeRead()
        {
            IList<ModelExport> models = new List<ModelExport>
            {
                Model("AR", 86, 86, 86, "AR-EXTERIOR", "AR-INTERIOR")
            };

            string unread = Joined(ExportCheck.Lines(models, NoFile, ExportCheck.NamesShown, false));
            string read = Joined(ExportCheck.Lines(models, NoFile, ExportCheck.NamesShown, true));

            Assert.That(unread, Does.Contain(
                "the list of workset pairs a person decided are not typos could not be read out of Federator.Core.dll,"
                + " so whether a pair named here was already decided is UNKNOWN"));
            Assert.That(read, Does.Not.Contain("UNKNOWN"));
        }

        [Test]
        public void TheDecidedPairsAreReadOutOfTheMeasuredListAndNotTypedHere()
        {
            Assert.That(Federator.Core.Exchange.RevitWorksets.DecidedCount, Is.EqualTo(2));
            Assert.That(
                Federator.Core.Exchange.RevitWorksets.DecidedDifferent("ST-SUP", "ST-SUB"), Is.True,
                "either way round, because a pair is a pair");
            Assert.That(
                Federator.Core.Exchange.RevitWorksets.DecidedDifferent("EL-Lightning Protection", "EL-Lightining Protection"),
                Is.False, "nobody has decided about the typos, and they are named every run until the models are fixed");
        }

        // ---------- F131 attempt 2, one place for each workset rule the teams read ----------

        /// <summary>
        /// What follows a workset name's discipline prefix and its separator, read by the rule
        /// PrefixOf reads, so the judge of a silent miss splits a name where the disagreements do,
        /// the reviewer's finding on F131 attempt 1. A name PrefixOf gives whole, one with no
        /// separator or nothing before it, has no body, and nor has one with nothing after it.
        /// </summary>
        [Test]
        public void TheBodyOfAWorksetNameIsWhatFollowsItsPrefixAndTheSeparator()
        {
            Assert.That(WorksetDisagreements.BodyOf("ME-Ductwork"), Is.EqualTo("Ductwork"));
            Assert.That(WorksetDisagreements.BodyOf("EL-Fire-alarm"), Is.EqualTo("Fire-alarm"), "split at the first separator");
            Assert.That(
                WorksetDisagreements.PrefixOf("EL-Fire-alarm") + WorksetDisagreements.PrefixSeparator + WorksetDisagreements.BodyOf("EL-Fire-alarm"),
                Is.EqualTo("EL-Fire-alarm"));
            Assert.That(WorksetDisagreements.BodyOf("Ductwork"), Is.Null, "no separator, so PrefixOf gives the whole name");
            Assert.That(WorksetDisagreements.BodyOf("-Ductwork"), Is.Null, "nothing before the separator, so PrefixOf gives the whole name");
            Assert.That(WorksetDisagreements.BodyOf("ME-"), Is.Null, "nothing after the separator");
            Assert.That(WorksetDisagreements.BodyOf(string.Empty), Is.Null);
            Assert.That(WorksetDisagreements.BodyOf(null), Is.Null);
        }

        /// <summary>
        /// Whether a workset asked finds a name a model carries, as it is spelled: the whole name
        /// for equals and a part of it for contains, letter case and all. One place says it, read
        /// by the export check and by the judge of a silent miss, the reviewer's finding on F131
        /// attempt 1.
        /// </summary>
        [Test]
        public void AWorksetAskFindsACarriedNameAsItIsSpelled()
        {
            Assert.That(ExportCheck.WorksetFinds("ME-Ductwork", false, "ME-Ductwork"), Is.True);
            Assert.That(ExportCheck.WorksetFinds("ME-DUCTWORK", false, "ME-Ductwork"), Is.False, "letter case");
            Assert.That(ExportCheck.WorksetFinds("ME-Duct", false, "ME-Ductwork"), Is.False, "equals asks the whole name");
            Assert.That(ExportCheck.WorksetFinds("Duct", true, "ME-Ductwork"), Is.True);
            Assert.That(ExportCheck.WorksetFinds("ME-Ductwork", true, "ME-Ductwork"), Is.True, "the whole name is a part of itself");
            Assert.That(ExportCheck.WorksetFinds("duct", true, "ME-Ductwork"), Is.False, "letter case");
            Assert.That(ExportCheck.WorksetFinds("Pipe", true, "ME-Ductwork"), Is.False);
        }
    }
}
