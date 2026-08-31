using System;
using System.Collections.Generic;
using Federator.Core.Findings;
using Federator.Core.Grouping;
using Federator.Core.Naming;
using NUnit.Framework;

namespace Federator.Core.Tests
{
    /// <summary>
    /// The shapes here are the ones from the real run of 22 groups and 73 NWC. Reading
    /// that log by hand is what surfaced these, so the tool has to surface them itself.
    /// </summary>
    [TestFixture]
    public class ScanFindingsTests
    {
        private static string Nwc(string building, string discipline)
        {
            return "1104-PAR-" + building + "-ZZZ-" + discipline + "-MOD-000001.nwc";
        }

        private static string Nwc(string building, string discipline, string number)
        {
            return "1104-PAR-" + building + "-ZZZ-" + discipline + "-MOD-" + number + ".nwc";
        }

        private static ScanFindings FindingsFor(params string[] fileNames)
        {
            BuildingGroupingResult grouped =
                BuildingGrouping.GroupNames(fileNames, new ContainerNameSettings());
            return ScanFindings.From(grouped);
        }

        /// <summary>Four disciplines for one building, which is what a complete group looks like.</summary>
        private static IEnumerable<string> FullGroup(string building)
        {
            yield return Nwc(building, "AR");
            yield return Nwc(building, "ST");
            yield return Nwc(building, "ME");
            yield return Nwc(building, "EL");
        }

        private static string[] Files(params IEnumerable<string>[] parts)
        {
            List<string> all = new List<string>();

            foreach (IEnumerable<string> part in parts)
            {
                all.AddRange(part);
            }

            return all.ToArray();
        }

        // ---------- shape ----------

        [Test]
        public void ShapeReadsLettersAndDigitsAndNothingElse()
        {
            Assert.That(ScanFindings.Shape("1B06PK"), Is.EqualTo("9A99AA"));
            Assert.That(ScanFindings.Shape("1C06M2"), Is.EqualTo("9A99A9"));
            Assert.That(ScanFindings.Shape("100000"), Is.EqualTo("999999"));
        }

        [Test]
        public void ShapeKeepsACharacterThatIsNeitherLetterNorDigit()
        {
            Assert.That(ScanFindings.Shape("1B06_K"), Is.EqualTo("9A99_A"));
        }

        // ---------- job 1, odd shape ----------

        // The real run: 21 codes shaped like 1B06PK or 1C06M2, and one 100000.
        [Test]
        public void OneOddCodeAmongManyRegularOnesIsReported()
        {
            // Both normal shapes are shared, 9A99AA and 9A99A9, exactly as in the real
            // run. Only 999999 stands alone.
            ScanFindings findings = FindingsFor(Files(
                FullGroup("1B06PK"),
                FullGroup("1B06BS"),
                FullGroup("1C06M2"),
                FullGroup("1C06M3"),
                FullGroup("100000")));

            IList<ScanFinding> odd = findings.OfKind(FindingKind.OddShape);

            Assert.That(odd.Count, Is.EqualTo(1));
            Assert.That(odd[0].Buildings, Is.EqualTo(new[] { "100000" }));
            Assert.That(odd[0].Label, Is.EqualTo("ODD SHAPE"));
            Assert.That(odd[0].Headline, Does.Contain("100000"));
            Assert.That(odd[0].Headline, Does.Contain("written differently"));

            // An example of what the others look like, not a shape string. Nobody reads
            // 9A99AA, everybody reads a real code.
            Assert.That(odd[0].Sentence, Does.Contain("codes look like"));
            Assert.That(odd[0].Sentence, Does.Not.Contain("9A99AA"),
                "the finding still prints a shape string at the reader");
            Assert.That(odd[0].Sentence, Does.Not.Contain("shape"),
                "the word shape is how the tool thinks, not how a person reads");
        }

        [Test]
        public void AnOddCodeCarriesItsFileList()
        {
            ScanFindings findings = FindingsFor(Files(
                FullGroup("1B06PK"),
                FullGroup("1B06BS"),
                new[] { Nwc("100000", "AR"), Nwc("100000", "ST"), Nwc("100000", "ME"), Nwc("100000", "EL") }));

            ScanFinding odd = findings.OfKind(FindingKind.OddShape)[0];

            Assert.That(odd.Files.Count, Is.EqualTo(4));
            Assert.That(odd.Files, Does.Contain("1104-PAR-100000-ZZZ-AR-MOD-000001"));
        }

        // 1B06PK and 1C06M2 are different shapes. Neither is odd, because each is shared.
        [Test]
        public void TwoShapesThatAreEachSharedProduceNoOddShape()
        {
            ScanFindings findings = FindingsFor(Files(
                FullGroup("1B06PK"),
                FullGroup("1B06BS"),
                FullGroup("1C06M2"),
                FullGroup("1C06M3")));

            Assert.That(findings.OfKind(FindingKind.OddShape).Count, Is.EqualTo(0));
        }

        // With every shape held once there is no majority to differ from, so calling them
        // all odd would say nothing.
        [Test]
        public void WhenEveryShapeIsUniqueNothingIsCalledOdd()
        {
            ScanFindings findings = FindingsFor(Files(
                FullGroup("1B06PK"),
                FullGroup("1C06M2"),
                FullGroup("100000")));

            Assert.That(findings.OfKind(FindingKind.OddShape).Count, Is.EqualTo(0));
        }

        [Test]
        public void AnOddShapeIsNeverBlockedOrUnticked()
        {
            BuildingGroupingResult grouped = BuildingGrouping.GroupNames(
                Files(FullGroup("1B06PK"), FullGroup("1B06BS"), FullGroup("100000")),
                new ContainerNameSettings());

            Assert.That(ScanFindings.From(grouped).OfKind(FindingKind.OddShape).Count, Is.EqualTo(1));

            // The grouping itself is untouched. All three still run.
            Assert.That(grouped.Groups.Count, Is.EqualTo(3));
            Assert.That(grouped.Skipped.Count, Is.EqualTo(0));
            Assert.That(grouped.Find("100000"), Is.Not.Null);
        }

        // ---------- job 2, near match ----------

        // The one real typing error in the 22 group run, a digit one against a capital i.
        [Test]
        public void TwoCodesApartByAConfusableCharacterAreReportedWithBothFileCounts()
        {
            ScanFindings findings = FindingsFor(Files(
                FullGroup("1B06K1"),
                new[] { Nwc("1B06KI", "AR"), Nwc("1B06KI", "ST") }));

            IList<ScanFinding> near = findings.OfKind(FindingKind.NearMatch);

            Assert.That(near.Count, Is.EqualTo(1));
            Assert.That(near[0].Label, Is.EqualTo("NEAR MATCH"));
            Assert.That(near[0].Buildings, Is.EquivalentTo(new[] { "1B06K1", "1B06KI" }));
            Assert.That(near[0].Detail, Does.Contain("4 files"));
            Assert.That(near[0].Detail, Does.Contain("2 files"));
            Assert.That(near[0].Detail, Does.Contain("character 6"),
                "the finding should say where the two codes differ");
            Assert.That(near[0].Headline, Does.Contain("look almost the same"));
            Assert.That(near[0].Detail, Does.Contain("one of the NWC file names needs correcting"),
                "the finding should say what a person would do about it");
        }

        [Test]
        public void NeitherSideOfANearMatchIsMergedOrGuessedAt()
        {
            BuildingGroupingResult grouped = BuildingGrouping.GroupNames(
                Files(FullGroup("1B06K1"), FullGroup("1B06KI")),
                new ContainerNameSettings());

            Assert.That(grouped.Groups.Count, Is.EqualTo(2), "the two codes were merged");
            Assert.That(ScanFindings.From(grouped).OfKind(FindingKind.NearMatch).Count, Is.EqualTo(1));
        }

        // This is the noise the narrowing exists to remove. E and G are not confusable, so
        // these are two real buildings and nothing should be said about them.
        [Test]
        public void TwoRealBuildingsOneOrdinaryCharacterApartProduceNothing()
        {
            ScanFindings findings = FindingsFor(Files(
                FullGroup("1B06PE"),
                FullGroup("1B06PG")));

            Assert.That(findings.OfKind(FindingKind.NearMatch).Count, Is.EqualTo(0));
            Assert.That(ScanFindings.IsConfusablePair("1B06PE", "1B06PG"), Is.False);
        }

        [Test]
        public void CodesSharingAPrefixDoNotFloodTheFindings()
        {
            // Eight codes that all share 1B06P and differ only in the last character. A
            // plain one character rule would report 28 pairs here.
            ScanFindings findings = FindingsFor(Files(
                FullGroup("1B06PA"), FullGroup("1B06PC"), FullGroup("1B06PD"),
                FullGroup("1B06PE"), FullGroup("1B06PF"), FullGroup("1B06PH"),
                FullGroup("1B06PJ"), FullGroup("1B06PK")));

            Assert.That(findings.OfKind(FindingKind.NearMatch).Count, Is.EqualTo(0));
        }

        [Test]
        public void CodesTwoCharactersApartAreNotANearMatch()
        {
            ScanFindings findings = FindingsFor(Files(
                FullGroup("1B06K1"),
                FullGroup("1B06J2")));

            Assert.That(findings.OfKind(FindingKind.NearMatch).Count, Is.EqualTo(0));
        }

        // Every pair that has to be caught, in both directions.
        [TestCase('1', 'I')]
        [TestCase('1', 'l')]
        [TestCase('I', 'l')]
        [TestCase('0', 'O')]
        [TestCase('5', 'S')]
        [TestCase('8', 'B')]
        [TestCase('2', 'Z')]
        [TestCase('6', 'G')]
        public void EveryConfusablePairIsCaughtBothWays(char left, char right)
        {
            Assert.That(ScanFindings.AreConfusableCharacters(left, right), Is.True);
            Assert.That(ScanFindings.AreConfusableCharacters(right, left), Is.True, "reversed");
        }

        [TestCase('E', 'G', "E is not confusable with anything")]
        [TestCase('A', 'B', "B is confusable only with 8")]
        [TestCase('1', '7', "7 is not in any group")]
        [TestCase('0', 'D', "D is not confusable with 0")]
        [TestCase('5', '6', "5 pairs with S and 6 pairs with G, not with each other")]
        [TestCase('A', 'A', "the same character is not a difference")]
        public void CharactersThatAreNotConfusableAreNotTreatedAsSuch(char left, char right, string why)
        {
            Assert.That(ScanFindings.AreConfusableCharacters(left, right), Is.False, why);
            Assert.That(ScanFindings.AreConfusableCharacters(right, left), Is.False, why + ", reversed");
        }

        [TestCase("1B06K1", "1B06KI", true, "digit one against capital i")]
        [TestCase("1B06K0", "1B06KO", true, "zero against capital o")]
        [TestCase("1B065A", "1B06SA", true, "five against s")]
        [TestCase("1B06PE", "1B06PG", false, "e against g is two real buildings")]
        [TestCase("1B06K1", "1B06K1", false, "identical is not a near match")]
        [TestCase("1B06K1", "1B06J2", false, "two positions apart")]
        [TestCase("1B06K1", "1B06K", false, "a missing character is not a near match")]
        [TestCase("1B06K", "1B06K1", false, "an extra character is not a near match")]
        [TestCase("1B06K1", "1B06", false, "two shorter")]
        public void AConfusablePairIsMeasuredNotGuessed(
            string left, string right, bool expected, string why)
        {
            Assert.That(ScanFindings.IsConfusablePair(left, right), Is.EqualTo(expected), why);
            Assert.That(ScanFindings.IsConfusablePair(right, left), Is.EqualTo(expected), why + ", reversed");
        }

        // An inserted or missing character was most of the noise, so it is deliberately out
        // even when the codes look close.
        [Test]
        public void AnInsertedOrMissingCharacterIsNeverANearMatch()
        {
            ScanFindings shorter = FindingsFor(Files(
                FullGroup("1B06K1"),
                FullGroup("1B06K")));

            Assert.That(shorter.OfKind(FindingKind.NearMatch).Count, Is.EqualTo(0));
        }

        [Test]
        public void TheDifferenceIsReportedByPosition()
        {
            // Words, not quoted characters. A reader should not have to decode it.
            Assert.That(ScanFindings.DescribeConfusion("1B06K1", "1B06KI"),
                Is.EqualTo("character 6, a 1 against an I"));
            Assert.That(ScanFindings.DescribeConfusion("1B06PE", "1B06PG"), Is.Null);
        }

        // ---------- job 3, missing disciplines ----------

        // The real run: 1B06BS held only EL, 1C06PK only AR.
        [Test]
        public void AGroupHoldingOneDisciplineIsReportedAsSingleDiscipline()
        {
            ScanFindings findings = FindingsFor(Files(
                FullGroup("1B06PK"),
                new[] { Nwc("1B06BS", "EL") }));

            IList<ScanFinding> single = findings.OfKind(FindingKind.SingleDiscipline);

            Assert.That(single.Count, Is.EqualTo(1));
            Assert.That(single[0].Label, Is.EqualTo("SINGLE DISCIPLINE"));
            Assert.That(single[0].Buildings, Is.EqualTo(new[] { "1B06BS" }));
            Assert.That(single[0].Headline, Does.Contain("only EL"));
            Assert.That(single[0].Headline, Does.Contain("nothing for them to clash against"));
        }

        [Test]
        public void ASingleDisciplineGroupIsNotAlsoReportedAsMissing()
        {
            ScanFindings findings = FindingsFor(Files(
                FullGroup("1B06PK"),
                new[] { Nwc("1C06PK", "AR") }));

            Assert.That(findings.OfKind(FindingKind.SingleDiscipline).Count, Is.EqualTo(1));

            foreach (ScanFinding missing in findings.OfKind(FindingKind.MissingDisciplines))
            {
                Assert.That(missing.Buildings, Does.Not.Contain("1C06PK"),
                    "a single discipline group was reported twice");
            }
        }

        [Test]
        public void AGroupMissingTwoDisciplinesNamesBothOfThem()
        {
            ScanFindings findings = FindingsFor(Files(
                FullGroup("1B06PK"),
                new[] { Nwc("1B06BS", "AR"), Nwc("1B06BS", "ST") }));

            IList<ScanFinding> missing = findings.OfKind(FindingKind.MissingDisciplines);

            Assert.That(missing.Count, Is.EqualTo(1));
            Assert.That(missing[0].Buildings, Is.EqualTo(new[] { "1B06BS" }));
            Assert.That(missing[0].Headline, Does.Contain("EL").And.Contains("ME"));
            Assert.That(missing[0].Detail, Does.Contain("AR, ST"));
            Assert.That(missing[0].Headline, Does.Contain("which other buildings in this run do have"));
        }

        // The set of disciplines comes from the run, never from a list in the code.
        [Test]
        public void TheDisciplineSetIsReadFromTheRunNotHardCoded()
        {
            ScanFindings findings = FindingsFor(Files(
                new[] { Nwc("1B06PK", "PH"), Nwc("1B06PK", "LS"), Nwc("1B06PK", "QQ") },
                new[] { Nwc("1B06BS", "PH"), Nwc("1B06BS", "LS") }));

            Assert.That(findings.DisciplinesInRun, Is.EqualTo(new[] { "LS", "PH", "QQ" }));

            IList<ScanFinding> missing = findings.OfKind(FindingKind.MissingDisciplines);
            Assert.That(missing.Count, Is.EqualTo(1));
            Assert.That(missing[0].Buildings, Is.EqualTo(new[] { "1B06BS" }));
            Assert.That(missing[0].Headline, Does.Contain("QQ"));
        }

        // ---------- a clean run ----------

        [Test]
        public void ACleanRunProducesNoFindingsAtAll()
        {
            // Both shapes shared, every group complete, and no two codes within one
            // character of each other. 1C06M3 would have been a near match for 1C06M2.
            ScanFindings findings = FindingsFor(Files(
                FullGroup("1B06PK"),
                FullGroup("1B06BS"),
                FullGroup("1C06M2"),
                FullGroup("1D07N5")));

            Assert.That(findings.Any, Is.False);
            Assert.That(findings.Count, Is.EqualTo(0));
            Assert.That(findings.DisciplinesInRun, Is.EqualTo(new[] { "AR", "EL", "ME", "ST" }));
        }

        [Test]
        public void ACleanRunSaysSoInOneLineRatherThanShowingNothing()
        {
            ScanFindings findings = FindingsFor(Files(FullGroup("1B06PK"), FullGroup("1B06BS")));
            IList<string> lines = findings.Lines();

            Assert.That(findings.Any, Is.False);
            Assert.That(lines.Count, Is.EqualTo(2));
            Assert.That(lines[0], Does.Contain("Disciplines in this run: AR, EL, ME, ST"));
            Assert.That(lines[1], Is.EqualTo(ScanFindings.NothingOdd));
        }

        [Test]
        public void AnEmptyRunIsNotAnError()
        {
            ScanFindings findings = ScanFindings.From(new List<BuildingGroup>());

            Assert.That(findings.Any, Is.False);
            Assert.That(findings.DisciplinesInRun.Count, Is.EqualTo(0));
            Assert.That(findings.Lines()[0], Does.Contain("none"));
        }

        // ---------- all three together, which is what the real run looked like ----------

        [Test]
        public void AllThreeKindsAreFoundInOneRun()
        {
            // 9A99AA is held by 1B06PK, 1B06KI and 1B06BS. 9A99A9 is held by 1B06K1 and
            // 1C06M2. Only 999999 stands alone.
            ScanFindings findings = FindingsFor(Files(
                FullGroup("1B06PK"),
                FullGroup("1B06K1"),
                FullGroup("1C06M2"),
                new[] { Nwc("1B06KI", "AR"), Nwc("1B06KI", "ST") },
                new[] { Nwc("1B06BS", "EL") },
                FullGroup("100000")));

            Assert.That(findings.OfKind(FindingKind.OddShape).Count, Is.EqualTo(1), "the 100000 code");
            Assert.That(findings.OfKind(FindingKind.NearMatch).Count, Is.EqualTo(1), "1B06K1 against 1B06KI");
            Assert.That(findings.OfKind(FindingKind.SingleDiscipline).Count, Is.EqualTo(1), "1B06BS");
            Assert.That(findings.OfKind(FindingKind.MissingDisciplines).Count, Is.EqualTo(1), "1B06KI");
            Assert.That(findings.Any, Is.True);

            // Nothing is blocked by any of this.
            BuildingGroupingResult grouped = BuildingGrouping.GroupNames(
                Files(FullGroup("1B06PK"), FullGroup("1B06K1"),
                      new[] { Nwc("1B06KI", "AR") }, new[] { Nwc("1B06BS", "EL") }, FullGroup("100000")),
                new ContainerNameSettings());
            Assert.That(grouped.Skipped.Count, Is.EqualTo(0));
        }

        [Test]
        public void TheFindingLinesCarryTheLabelAndTheDetail()
        {
            ScanFindings findings = FindingsFor(Files(
                FullGroup("1B06PK"),
                FullGroup("1B06BS"),
                FullGroup("100000")));

            string all = string.Join(Environment.NewLine, new List<string>(findings.Lines()).ToArray());

            Assert.That(all, Does.Contain("ODD SHAPE"));
            Assert.That(all, Does.Contain("100000"));
            Assert.That(all, Does.Contain("file: 1104-PAR-100000-ZZZ-AR-MOD-000001"));
        }
    }
}
