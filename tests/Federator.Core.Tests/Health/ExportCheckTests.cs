using System.Collections.Generic;
using Federator.Core.Health;
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
            string block = Joined(ExportCheck.Lines(TheRealGroup()));

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

            string block = Joined(ExportCheck.Lines(models));

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

            string block = Joined(ExportCheck.Lines(models));

            Assert.That(block, Does.Not.Contain("carry a workset on every element"));
            Assert.That(block, Does.Contain("with a workset UNKNOWN"));
            Assert.That(block, Does.Contain("0 of 2 model(s) carry no workset at all, and 1 could not be counted"));
        }

        /// <summary>Every element of none is not every element, and a model with no Revit element was called whole.</summary>
        [Test]
        public void AModelWithNoRevitElementIsNotCalledWhole()
        {
            IList<ModelExport> models = new List<ModelExport> { Model("EL", 0, 0, 0) };

            string block = Joined(ExportCheck.Lines(models));

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
        /// nothing: the models carry ME-Ductwork and the matrix asks for ME-DUCTWORK.
        /// </summary>
        [Test]
        public void TheWorksetNamesAreListedAndTheBlockSaysTheMatchIsCaseSensitive()
        {
            string block = Joined(ExportCheck.Lines(TheRealGroup()));

            Assert.That(block, Does.Contain("worksets seen: AR-EXTERIOR, AR-INTERIOR, ME-Ductwork, ME-Piping, PL-Drainage"));
            Assert.That(block, Does.Contain("The match is CASE SENSITIVE"));
            Assert.That(block, Does.Contain("ME-Ductwork does not match ME-DUCTWORK"));
        }

        [Test]
        public void AModelCarryingNoWorksetAtAllSaysEverySetFilteringOnOneWillFindNothing()
        {
            IList<ModelExport> models = new List<ModelExport> { Model("EL", 494, 0, 494) };

            string block = Joined(ExportCheck.Lines(models));

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

            string block = Joined(ExportCheck.Lines(models));

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

            string block = Joined(ExportCheck.Lines(models));

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
            Assert.That(Joined(ExportCheck.Lines(new List<ModelExport> { Model("ME", 1052, 1052, 1051) })),
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
                Joined(ExportCheck.Lines(TheRealGroup())),
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

            string block = Joined(ExportCheck.Lines(models));

            Assert.That(block, Does.Contain("elements UNKNOWN"));
            Assert.That(block, Does.Contain("element id UNKNOWN"));
            Assert.That(block, Does.Not.Contain("re-export with Convert element Ids"),
                "a count nobody took is never reported as a fault");
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

            string block = Joined(ExportCheck.Lines(models));

            Assert.That(block, Does.Contain("Juliett"));
            Assert.That(block, Does.Not.Contain("Kilo"));
            Assert.That(block, Does.Contain("and 4 more, counted and not listed"));
        }

        [Test]
        public void WithNoModelAtAllItSaysSoRatherThanWritingAnEmptyBlock()
        {
            Assert.That(
                Joined(ExportCheck.Lines(new List<ModelExport>())),
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

            string block = Joined(ExportCheck.Lines(models));

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

            string block = Joined(ExportCheck.Lines(models));

            Assert.That(block, Does.Contain("EL-Lightining Protection"));
            Assert.That(block, Does.Contain("1 pair(s) of workset names are close enough"));
            Assert.That(block, Does.Not.Contain("\"AR-EXTERIOR\" in"));
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
    }
}
