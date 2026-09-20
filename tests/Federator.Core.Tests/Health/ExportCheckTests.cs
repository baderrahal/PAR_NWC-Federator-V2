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

        [Test]
        public void EveryModelIsNamedWithItsElementsItsWorksetsAndItsIdShare()
        {
            string block = Joined(ExportCheck.Lines(TheRealGroup()));

            Assert.That(block, Does.Contain("AR  1104-PAR-1A02MM-ZZZ-AR-MOD-000001.nwc   elements 86   worksets 2   element id 100%"));
            Assert.That(block, Does.Contain("ME  1104-PAR-1A02MM-ZZZ-ME-MOD-000001.nwc   elements 236   worksets 3   element id 100%"));
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
            List<string> many = new List<string>();

            for (int i = 1; i <= 14; i++)
            {
                many.Add("WS-" + i);
            }

            IList<ModelExport> models = new List<ModelExport>
            {
                new ModelExport("a.nwc", "ME", 100, 100, 100, many)
            };

            string block = Joined(ExportCheck.Lines(models));

            Assert.That(block, Does.Contain("WS-10"));
            Assert.That(block, Does.Not.Contain("WS-11"));
            Assert.That(block, Does.Contain("and 4 more, counted and not listed"));
        }

        [Test]
        public void WithNoModelAtAllItSaysSoRatherThanWritingAnEmptyBlock()
        {
            Assert.That(
                Joined(ExportCheck.Lines(new List<ModelExport>())),
                Does.Contain("no model was read, so nothing could be checked"));
        }
    }
}
