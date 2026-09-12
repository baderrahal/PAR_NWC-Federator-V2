using System;
using System.Collections.Generic;
using Federator.Core.Clash;
using Federator.Core.Findings;
using Federator.Core.Grouping;
using Federator.Core.Naming;
using NUnit.Framework;

namespace Federator.Core.Tests
{
    /// <summary>
    /// Copy findings has to paste into Excel as a table. The code stays as its own short
    /// column so it can be sorted, and the plain words sit next to it.
    /// </summary>
    [TestFixture]
    public class FindingsTableTests
    {
        private static readonly string[] Folder =
        {
            "1104-PAR-1B06K1-ZZZ-AR-MOD-000001.nwc",
            "1104-PAR-1B06K1-ZZZ-ST-MOD-000001.nwc",
            "1104-PAR-1B06KI-ZZZ-AR-MOD-000001.nwc",
            "1104-PAR-1C06PK-ZZZ-AR-MOD-000001.nwc",
            "1104-PAR-1B06BC-ZZZ-AR-MOD-000001.nwc",
            "1104-PAR-1B06BC-ZZZ-ST-MOD-000001.nwc"
        };

        private static ScanFindings Findings()
        {
            return ScanFindings.From(
                BuildingGrouping.GroupNames(Folder, new ContainerNameSettings()));
        }

        private static string[] Split(string row)
        {
            return row.Split(FindingsTable.Separator);
        }

        // ---------- it is a table ----------

        [Test]
        public void ThereIsAHeaderAndItIsTabSeparated()
        {
            Assert.That(FindingsTable.Separator, Is.EqualTo('\t'));
            Assert.That(Split(FindingsTable.Header()).Length, Is.EqualTo(4));
            Assert.That(Split(FindingsTable.Header())[0], Is.EqualTo("Code"));
            Assert.That(Split(FindingsTable.Header())[2], Is.EqualTo("What it means"));
        }

        // The one the brief asks for by name.
        [Test]
        public void TheFindingsCopyOutAsTabSeparatedRowsWithAHeader()
        {
            string tsv = FindingsTable.Tsv(Findings().All);
            string[] lines = tsv.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);

            Assert.That(lines.Length, Is.GreaterThan(1), "there is a header and at least one row");
            Assert.That(lines[0], Is.EqualTo(FindingsTable.Header()));

            foreach (string line in lines)
            {
                Assert.That(Split(line).Length, Is.EqualTo(4),
                    "every row has to have the same number of columns or Excel shears it: " + line);
            }
        }

        [Test]
        public void EveryRowStartsWithItsCodeSoItStaysSortable()
        {
            IList<string> rows = FindingsTable.Rows(Findings().All);

            Assert.That(rows.Count, Is.GreaterThan(0));

            foreach (string row in rows)
            {
                string code = Split(row)[0];

                Assert.That(code, Is.Not.Empty);
                Assert.That(code, Is.EqualTo(code.ToUpperInvariant()),
                    "the code column should stay the short shouty form");
            }
        }

        [Test]
        public void TheSentenceSitsNextToTheCode()
        {
            IList<string> rows = FindingsTable.Rows(Findings().All);
            bool foundNear = false;

            foreach (string row in rows)
            {
                string[] cells = Split(row);

                if (cells[0] == ScanFindings.NearMatchLabel)
                {
                    foundNear = true;
                    Assert.That(cells[1], Does.Contain("1B06K1"));
                    Assert.That(cells[2], Does.Contain("look almost the same"));
                    Assert.That(cells[2], Does.Contain("NWC file names needs correcting"));
                }
            }

            Assert.That(foundNear, Is.True, "the near match was not in the table");
        }

        // ---------- plain words, not code ----------

        [Test]
        public void NoRowReadsLikeCode()
        {
            foreach (string row in FindingsTable.Rows(Findings().All))
            {
                string sentence = Split(row)[2];

                Assert.That(sentence, Does.Not.Contain("9A99AA"), sentence);
                Assert.That(sentence, Does.Not.Contain("shaped"), sentence);
                Assert.That(sentence, Does.Match("^[A-Z0-9]"), "a sentence should start properly");
                Assert.That(sentence, Does.EndWith("."), "a sentence should end with a full stop");
            }
        }

        [Test]
        public void TheSentenceIsTheHeadlineAndTheDetailTogether()
        {
            foreach (ScanFinding finding in Findings().All)
            {
                Assert.That(finding.Sentence, Does.StartWith(finding.Headline));
                Assert.That(finding.Sentence, Does.Contain(finding.Detail));
            }
        }

        // ---------- a tab or a newline inside a cell would shear the table ----------

        [Test]
        public void ATabOrNewlineInsideACellBecomesASpace()
        {
            ScanFindings findings = ScanFindings.From(new List<BuildingGroup>());

            Assert.That(findings.Any, Is.False);

            // Every real finding goes through the same cleaning, so this pins the rule on
            // the one thing a test can hand it directly.
            string tsv = FindingsTable.Tsv(findings.All);

            foreach (string line in tsv.Split(
                         new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries))
            {
                Assert.That(Split(line).Length, Is.EqualTo(4), line);
            }
        }

        // An empty clipboard reads as a failed copy, so a clean run still gets a table.
        [Test]
        public void ARunWithNothingOddStillCopiesAHeaderAndOneRow()
        {
            string tsv = FindingsTable.Tsv(new List<ScanFinding>());
            string[] lines = tsv.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);

            Assert.That(lines.Length, Is.EqualTo(2));
            Assert.That(lines[0], Is.EqualTo(FindingsTable.Header()));
            Assert.That(lines[1], Does.Contain("NOTHING ODD"));
            Assert.That(Split(lines[1]).Length, Is.EqualTo(4));
        }

        [Test]
        public void NoFindingsAtAllIsRefusedRatherThanCopied()
        {
            Assert.Throws<ArgumentNullException>(delegate { FindingsTable.Rows(null); });
        }

        [Test]
        public void ANullFindingInTheListIsSkippedRatherThanThrowing()
        {
            List<ScanFinding> withNull = new List<ScanFinding>(Findings().All);
            withNull.Add(null);

            Assert.That(FindingsTable.Rows(withNull).Count, Is.EqualTo(Findings().All.Count));
        }

        // ---------- the count line at the top of the Source step ----------

        [Test]
        public void TheCountLineSaysFilesFoundReadableAndGroups()
        {
            ScanCounts counts = new ScanCounts();
            counts.FilesFound = 73;
            counts.FilesReadable = 71;
            counts.Groups = 22;
            counts.BlockedGroups = 1;
            counts.GroupingDescription = GroupingModes.Describe(GroupingMode.PerBuilding);
            counts.Count(Findings().All);

            string block = string.Join("\n", new List<string>(counts.Lines()).ToArray());

            Assert.That(block, Does.Contain("73 files found, 71 readable, 2 that cannot be read."));
            Assert.That(block, Does.Contain("22 groups"));
            Assert.That(block, Does.Contain("one file per building"));
            Assert.That(block, Does.Contain("1 blocked"));
        }

        [Test]
        public void TheCountLineBreaksTheFindingsDownByKind()
        {
            ScanCounts counts = new ScanCounts();
            counts.Count(Findings().All);

            string block = string.Join("\n", new List<string>(counts.Lines()).ToArray());

            Assert.That(counts.TotalFindings, Is.GreaterThan(0));
            Assert.That(block, Does.Contain("worth a look"));
            Assert.That(block, Does.Contain(ScanCounts.Describe(FindingKind.NearMatch)));
            Assert.That(counts.Of(FindingKind.NearMatch), Is.EqualTo(1));
        }

        [Test]
        public void TheKindCountsAddUpToTheTotal()
        {
            ScanCounts counts = new ScanCounts();
            counts.Count(Findings().All);

            int total = 0;

            foreach (FindingKind kind in ScanCounts.Kinds())
            {
                total += counts.Of(kind);
            }

            Assert.That(total, Is.EqualTo(counts.TotalFindings));
        }

        [Test]
        public void ACleanRunSaysNothingFoundRatherThanShowingAnEmptyList()
        {
            ScanCounts counts = new ScanCounts();
            counts.FilesFound = 4;
            counts.FilesReadable = 4;
            counts.Groups = 2;

            Assert.That(string.Join("\n", new List<string>(counts.Lines()).ToArray()),
                Does.Contain("Nothing found worth a look."));
        }

        [Test]
        public void EveryKindHasPlainWordsForIt()
        {
            foreach (FindingKind kind in ScanCounts.Kinds())
            {
                Assert.That(ScanCounts.Describe(kind), Is.Not.EqualTo("UNKNOWN"), kind.ToString());
            }

            Assert.That(ScanCounts.Kinds().Length,
                Is.EqualTo(Enum.GetValues(typeof(FindingKind)).Length),
                "a kind was added and the count line does not know about it");
        }
    }
}
