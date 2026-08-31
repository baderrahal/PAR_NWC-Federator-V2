using System;
using System.Collections.Generic;
using Federator.Core.Findings;
using Federator.Core.Naming;
using NUnit.Framework;

namespace Federator.Core.Tests
{
    /// <summary>
    /// Across one real run the Revit container inside an NWC was often a different
    /// building from the NWC name. The pairs used here are the measured ones off that run.
    /// They are sample data, not settings. Both codes are read with the same parser used
    /// on the NWC names, never a second rule.
    ///
    /// This is information. Nothing is blocked, unpicked or merged.
    /// </summary>
    [TestFixture]
    public class SourceMismatchFindingsTests
    {
        private const string Docs = "Autodesk Docs://KSA_New Murabba/";

        private static string Nwc(string building, string discipline)
        {
            return @"C:\in\1104-PAR-" + building + "-ZZZ-" + discipline + "-MOD-000001.nwc";
        }

        private static string Revit(string building, string discipline, string number)
        {
            return Docs + "1104-PAR-" + building + "-ZZZ-" + discipline + "-MOD-" + number + ".rvt";
        }

        private static SourcePair Pair(string nwcBuilding, string revitBuilding)
        {
            return new SourcePair(nwcBuilding, Nwc(nwcBuilding, "AR"), Revit(revitBuilding, "AR", "003000"));
        }

        /// <summary>The eight pairs measured on the real run, in the order they were given.</summary>
        private static IList<SourcePair> RealRun()
        {
            return new List<SourcePair>
            {
                Pair("1B06BC", "0000BC"),
                Pair("1B06BS", "1A02BS"),
                Pair("1B06G1", "0000PG"),
                new SourcePair("1B06G1", Nwc("1B06G1", "ST"), Revit("1B06PG", "ST", "003000")),
                Pair("1B06K1", "0000KI"),
                Pair("1B06KI", "0000KI"),
                Pair("1B06M1", "1B06MM"),
                Pair("1B06P1", "0000PW"),

                // Same building code both sides. The number differs, 000101 against 000001,
                // and a number is not a building, so this one must stay quiet.
                new SourcePair("1C06PK", Nwc("1C06PK", "AR"), Revit("1C06PK", "AR", "000101"))
            };
        }

        // ---------- it fires on the real pairs ----------

        [Test]
        public void EveryPairWhoseBuildingCodesDifferIsReported()
        {
            SourceMismatchFindings findings = SourceMismatchFindings.From(RealRun());
            IList<ScanFinding> mismatches = findings.OfKind(FindingKind.SourceMismatch);

            // Eight of the nine pairs differ. 1C06PK against 1C06PK does not.
            Assert.That(mismatches.Count, Is.EqualTo(8));

            foreach (ScanFinding finding in mismatches)
            {
                Assert.That(finding.Label, Is.EqualTo("SOURCE MISMATCH"));
                Assert.That(finding.Buildings.Count, Is.EqualTo(2),
                    "both codes have to be named, neither is assumed right");
                Assert.That(finding.Buildings[0], Is.Not.EqualTo(finding.Buildings[1]));
            }
        }

        [Test]
        public void EachRealPairIsNamedBothWays()
        {
            string block = Block(SourceMismatchFindings.From(RealRun()));

            string[,] pairs =
            {
                { "1B06BC", "0000BC" },
                { "1B06BS", "1A02BS" },
                { "1B06G1", "0000PG" },
                { "1B06G1", "1B06PG" },
                { "1B06K1", "0000KI" },
                { "1B06KI", "0000KI" },
                { "1B06M1", "1B06MM" },
                { "1B06P1", "0000PW" }
            };

            for (int i = 0; i < pairs.GetLength(0); i++)
            {
                Assert.That(block,
                    Does.Contain("The NWC files named " + pairs[i, 0]
                        + " were published from a Revit model named " + pairs[i, 1]),
                    pairs[i, 0] + " against " + pairs[i, 1] + " was not reported");
            }
        }

        // ---------- it stays quiet when the codes agree ----------

        [Test]
        public void CodesThatAgreeAreNotReportedEvenWhenTheRestOfTheNameDiffers()
        {
            // 1C06PK on both sides, numbered 000101 against 000001. A number is not a
            // building, so there is nothing to report.
            SourceMismatchFindings findings = SourceMismatchFindings.From(
                new List<SourcePair>
                {
                    new SourcePair("1C06PK", Nwc("1C06PK", "AR"), Revit("1C06PK", "AR", "000101"))
                });

            Assert.That(findings.OfKind(FindingKind.SourceMismatch).Count, Is.EqualTo(0),
                "a matching building code was reported as a mismatch");
            Assert.That(findings.Any, Is.False);
            Assert.That(Block(findings), Does.Contain(SourceMismatchFindings.NothingOdd));
        }

        [Test]
        public void ADifferentDisciplineOnTheSourceIsNotAMismatch()
        {
            SourceMismatchFindings findings = SourceMismatchFindings.From(
                new List<SourcePair>
                {
                    new SourcePair("1B06BC", Nwc("1B06BC", "AR"), Revit("1B06BC", "ST", "003000"))
                });

            Assert.That(findings.Any, Is.False, "only the building code is compared");
        }

        [Test]
        public void ARunWhereEveryCodeAgreesSaysSoInOneLine()
        {
            SourceMismatchFindings findings = SourceMismatchFindings.From(
                new List<SourcePair>
                {
                    Pair("1B06BC", "1B06BC"),
                    Pair("1C07K1", "1C07K1")
                });

            Assert.That(findings.Any, Is.False);
            Assert.That(findings.PairsRead, Is.EqualTo(2));
            Assert.That(Block(findings), Does.Contain(SourceMismatchFindings.NothingOdd));
        }

        // ---------- two groups fed by one Revit building ----------

        [Test]
        public void TwoGroupsFedByOneRevitBuildingAreReported()
        {
            SourceMismatchFindings findings = SourceMismatchFindings.From(RealRun());
            IList<ScanFinding> shared = findings.OfKind(FindingKind.SharedSourceBuilding);

            // 1B06K1 and 1B06KI are both fed by 0000KI. Nothing else is shared.
            Assert.That(shared.Count, Is.EqualTo(1));
            Assert.That(shared[0].Label, Is.EqualTo("SHARED SOURCE"));
            Assert.That(shared[0].Headline, Does.Contain("0000KI"));
            Assert.That(shared[0].Detail, Does.Contain("1B06K1"));
            Assert.That(shared[0].Detail, Does.Contain("1B06KI"));

            // The words a person would say, which is the whole point of the rewrite.
            Assert.That(shared[0].Headline, Does.Contain("same Revit building"));
            Assert.That(shared[0].Detail, Does.Contain("one of the NWC file names is wrong"));
            Assert.That(shared[0].Detail, Does.Contain("Nothing is merged"));
        }

        [Test]
        public void OneRevitBuildingFeedingOneGroupIsNotReported()
        {
            SourceMismatchFindings findings = SourceMismatchFindings.From(
                new List<SourcePair>
                {
                    Pair("1B06BC", "0000BC"),
                    new SourcePair("1B06BC", Nwc("1B06BC", "ST"), Revit("0000BC", "ST", "003000"))
                });

            Assert.That(findings.OfKind(FindingKind.SharedSourceBuilding).Count, Is.EqualTo(0),
                "one Revit building feeding one group is the ordinary case");
            Assert.That(findings.OfKind(FindingKind.SourceMismatch).Count, Is.EqualTo(2));
        }

        [Test]
        public void ThreeGroupsSharingOneRevitBuildingNameAllThree()
        {
            SourceMismatchFindings findings = SourceMismatchFindings.From(
                new List<SourcePair>
                {
                    Pair("1B06K1", "0000KI"),
                    Pair("1B06KI", "0000KI"),
                    Pair("1B06KX", "0000KI")
                });

            IList<ScanFinding> shared = findings.OfKind(FindingKind.SharedSourceBuilding);

            Assert.That(shared.Count, Is.EqualTo(1), "one line for the shared code, not one per group");
            Assert.That(shared[0].Headline, Does.Contain("3 federations"));
            Assert.That(shared[0].Buildings.Count, Is.EqualTo(4), "the source and its three groups");
        }

        // Two groups whose codes agree with their own sources cannot be sharing anything.
        [Test]
        public void GroupsWithTheirOwnMatchingSourcesShareNothing()
        {
            SourceMismatchFindings findings = SourceMismatchFindings.From(
                new List<SourcePair> { Pair("1B06BC", "1B06BC"), Pair("1B06BS", "1B06BS") });

            Assert.That(findings.OfKind(FindingKind.SharedSourceBuilding).Count, Is.EqualTo(0));
        }

        // ---------- reading the names ----------

        // The Revit name arrives with an Autodesk Docs prefix and a project folder on it.
        [Test]
        public void TheAutodeskDocsPrefixAndFolderAreStrippedByTheSameParser()
        {
            ParsedContainerName parsed = ContainerName.Parse(
                "Autodesk Docs://KSA_New Murabba/1104-PAR-0000BC-ZZZ-AR-MOD-003000.rvt");

            Assert.That(parsed.IsReadable, Is.True,
                "the parser could not read a real Revit source name");
            Assert.That(parsed.Building, Is.EqualTo("0000BC"));
            Assert.That(parsed.Discipline, Is.EqualTo("AR"));
        }

        [Test]
        public void TheSamePairIsNeverReportedTwice()
        {
            List<SourcePair> repeated = new List<SourcePair>();

            for (int i = 0; i < 5; i++)
            {
                repeated.Add(Pair("1B06BC", "0000BC"));
            }

            SourceMismatchFindings findings = SourceMismatchFindings.From(repeated);

            Assert.That(findings.PairsRead, Is.EqualTo(1));
            Assert.That(findings.OfKind(FindingKind.SourceMismatch).Count, Is.EqualTo(1));
        }

        [Test]
        public void ASourceNameThatCannotBeReadIsCountedRatherThanGuessedAt()
        {
            SourceMismatchFindings findings = SourceMismatchFindings.From(
                new List<SourcePair>
                {
                    new SourcePair("1B06BC", Nwc("1B06BC", "AR"), "SomeModel.rvt"),
                    Pair("1B06BS", "1A02BS")
                });

            Assert.That(findings.UnreadableSourceNames, Is.EqualTo(1));
            Assert.That(findings.PairsRead, Is.EqualTo(1), "only the readable pair was compared");
            Assert.That(findings.OfKind(FindingKind.SourceMismatch).Count, Is.EqualTo(1));
            // One reads "does not follow", several read "do not follow", so the assertion
            // is on the part that does not change with the count.
            Assert.That(Block(findings), Does.Contain("follow the naming standard"));
        }

        [Test]
        public void AnUnreadableNwcNameIsLeftToTheScanThatAlreadyReportsIt()
        {
            SourceMismatchFindings findings = SourceMismatchFindings.From(
                new List<SourcePair>
                {
                    new SourcePair("?", @"C:\in\rubbish.nwc", Revit("0000BC", "AR", "003000"))
                });

            Assert.That(findings.UnreadableNwcNames, Is.EqualTo(1));
            Assert.That(findings.Any, Is.False);
            Assert.That(Block(findings), Does.Contain("the scan already lists"));
        }

        // The split character and the part positions are settings, so a project using a
        // different shape reads with its own settings and not with these.
        [Test]
        public void TheParserSettingsAreUsedRatherThanAFixedShape()
        {
            ContainerNameSettings underscores = new ContainerNameSettings();
            underscores.Separator = '_';

            SourceMismatchFindings findings = SourceMismatchFindings.From(
                new List<SourcePair>
                {
                    new SourcePair(
                        "1B06BC",
                        @"C:\in\1104_PAR_1B06BC_ZZZ_AR_MOD_000001.nwc",
                        Docs + "1104_PAR_0000BC_ZZZ_AR_MOD_003000.rvt")
                },
                underscores);

            Assert.That(findings.OfKind(FindingKind.SourceMismatch).Count, Is.EqualTo(1));
            Assert.That(Block(findings),
                Does.Contain("The NWC files named 1B06BC were published from a Revit model named 0000BC"));
        }

        // ---------- it is information and nothing more ----------

        [Test]
        public void EveryFindingSaysPlainlyThatNothingWasActedOn()
        {
            string block = Block(SourceMismatchFindings.From(RealRun()));

            Assert.That(block, Does.Contain("neither name is assumed right"));
            Assert.That(block, Does.Contain("The federation is named from the NWC file name"));
        }

        [Test]
        public void TheBlockAlwaysSaysHowManyPairsItRead()
        {
            Assert.That(Block(SourceMismatchFindings.From(RealRun())),
                Does.Contain("9 NWC files were checked"));
        }

        [Test]
        public void NoPairsAtAllReadsAsNothingOddRatherThanAnEmptyBlock()
        {
            SourceMismatchFindings findings = SourceMismatchFindings.From(new List<SourcePair>());

            Assert.That(findings.Any, Is.False);
            Assert.That(findings.PairsRead, Is.EqualTo(0));
            Assert.That(Block(findings), Does.Contain(SourceMismatchFindings.NothingOdd));
        }

        [Test]
        public void NothingAtAllIsRefusedRatherThanReported()
        {
            Assert.Throws<ArgumentNullException>(
                delegate { SourceMismatchFindings.From(null); });
            Assert.Throws<ArgumentNullException>(
                delegate { SourceMismatchFindings.From(new List<SourcePair>(), null); });
        }

        [Test]
        public void ANullPairInTheListIsSkippedRatherThanThrowing()
        {
            SourceMismatchFindings findings = SourceMismatchFindings.From(
                new List<SourcePair> { null, Pair("1B06BC", "0000BC"), null });

            Assert.That(findings.PairsRead, Is.EqualTo(1));
        }

        private static string Block(SourceMismatchFindings findings)
        {
            return string.Join("\n", new List<string>(findings.Lines()).ToArray());
        }
    }
}
