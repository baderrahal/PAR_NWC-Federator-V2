using System;
using System.Collections.Generic;
using Federator.Core.Generic;
using Federator.Core.Grouping;
using Federator.Core.Naming;
using NUnit.Framework;

namespace Federator.Core.Tests
{
    /// <summary>
    /// The scan feeds the table, and the patterns are the starting point rather than the
    /// last word. A name typed over by hand stays typed over.
    /// </summary>
    [TestFixture]
    public class OutputNameTableTests
    {
        private static readonly ContainerNameSettings Settings = new ContainerNameSettings();
        private static readonly DateTime Friday = new DateTime(2026, 9, 4);
        private static readonly GenericModelsSettings Internal = new GenericModelsSettings();

        private static readonly string[] Folder =
        {
            "1104-PAR-1C07BC-ZZZ-AR-MOD-000001.nwc",
            "1104-PAR-1C07BC-ZZZ-ST-MOD-000001.nwc",
            "1104-PAR-1C07K1-ZZZ-AR-MOD-000001.nwc",
            "1104-PAR-1B06BS-ZZZ-EL-MOD-000001.nwc"
        };

        private static OutputNameTable Table(OutputNaming naming)
        {
            return TableOf(Folder, naming);
        }

        private static OutputNameTable TableOf(string[] files, OutputNaming naming)
        {
            BuildingGroupingResult result = BuildingGrouping.GroupNames(
                files, Settings, GroupingMode.PerBuilding);

            return OutputNameTable.From(result.Groups, naming, Settings, Friday);
        }

        // ---------- the scan fills it ----------

        [Test]
        public void ThereIsOneRowPerGroupAndTheNamesAreFilledIn()
        {
            OutputNameTable table = Table(new OutputNaming());

            Assert.That(table.Count, Is.EqualTo(3));
            Assert.That(table.Find("1C07BC").NwfName,
                Is.EqualTo("1104-PAR-1C07BC-ZZZ-BM-MOD-000001"));
            Assert.That(table.Find("1C07BC").NwdName, Is.EqualTo(table.Find("1C07BC").NwfName));
            Assert.That(table.Find("1C07BC").WorkbookName, Is.EqualTo(table.Find("1C07BC").NwfName));
        }

        // ---------- two rows on one name stops the run ----------

        // The one the brief asks for by name.
        [Test]
        public void TwoRowsOnOneNameIsCaughtBeforeTheRun()
        {
            OutputNameTable table = Table(new OutputNaming());

            Assert.That(table.WhyTheRunCannotStart(Internal), Is.Null, "it should start out clean");

            // Typing a name that another row already has is exactly how this happens.
            table.Find("1C07K1").Fill(
                OutputKind.Nwf, table.Find("1C07BC").NwfName);

            string why = table.WhyTheRunCannotStart(Internal);

            Assert.That(why, Is.Not.Null, "a collision went unnoticed");
            Assert.That(why, Does.Contain("would be written to the same"));
            Assert.That(why, Does.Contain("the run does not start"));
            Assert.That(why, Does.Contain("1C07BC"));
            Assert.That(why, Does.Contain("1C07K1"));
        }

        [Test]
        public void ACollisionInTheWorkbookNameAloneStillStopsTheRun()
        {
            OutputNameTable table = Table(new OutputNaming());
            table.Find("1C07K1").Fill(
                OutputKind.Workbook, table.Find("1C07BC").WorkbookName);

            IList<NameCollision> collisions = table.Collisions();

            Assert.That(collisions.Count, Is.EqualTo(1));
            Assert.That(collisions[0].Kind, Is.EqualTo("Workbook"));
            Assert.That(table.WhyTheRunCannotStart(Internal), Is.Not.Null);
        }
        // ---------- a workbook named as another group's internal workbook stops the run, F127-R5 ----------

        /// <summary>
        /// The internal workbook is named after the group's workbook, one space and the suffix, so a group
        /// whose workbook is named X Internal beside a group whose workbook is X would have one written over
        /// the other with no line, the breaker's point 6 on F127b. The check names both groups and the name.
        /// </summary>
        [Test]
        public void AWorkbookNamedAsAnotherGroupsInternalWorkbookStopsTheRun()
        {
            OutputNameTable table = Table(new OutputNaming());
            string internalName = Internal.WorkbookNameFor(table.Find("1C07BC").WorkbookName);

            table.Find("1C07K1").Fill(OutputKind.Workbook, internalName);

            Assert.That(table.Collisions(), Is.Empty, "no two rows share a name, so the three kinds alone see nothing");
            Assert.That(table.WhyTheRunCannotStart(Internal), Is.EqualTo(
                "The internal workbook of 1C07BC and the workbook of 1C07K1 would both be written as " + internalName
                + ". One would overwrite the other, so the run does not start."));
        }

        /// <summary>Windows reads two names apart only in case as one file, as the three kinds are read.</summary>
        [Test]
        public void AnInternalWorkbookApartOnlyInCaseStillStopsTheRun()
        {
            OutputNameTable table = Table(new OutputNaming());
            string internalName = Internal.WorkbookNameFor(table.Find("1C07BC").WorkbookName);

            table.Find("1C07K1").Fill(OutputKind.Workbook, internalName.ToLowerInvariant());

            Assert.That(table.WhyTheRunCannotStart(Internal), Does.Contain("would both be written as " + internalName + "."));
        }

        /// <summary>The suffix is a setting, so the name is read through it and not as the default word.</summary>
        [Test]
        public void TheInternalWorkbookIsReadThroughTheSuffixSetting()
        {
            OutputNameTable table = Table(new OutputNaming());
            GenericModelsSettings other = new GenericModelsSettings();
            other.WorkbookSuffix = "Checks";

            table.Find("1C07K1").Fill(OutputKind.Workbook, Internal.WorkbookNameFor(table.Find("1C07BC").WorkbookName));

            Assert.That(table.WhyTheRunCannotStart(other), Is.Null, "the default suffix was read in place of the setting");

            table.Find("1C07K1").Fill(OutputKind.Workbook, other.WorkbookNameFor(table.Find("1C07BC").WorkbookName));

            Assert.That(table.WhyTheRunCannotStart(other), Does.Contain("The internal workbook of 1C07BC and the workbook of 1C07K1"));
        }

        [Test]
        public void TheStartCheckRefusesNoInternalWorkbookSettings()
        {
            Assert.Throws<ArgumentNullException>(() => Table(new OutputNaming()).WhyTheRunCannotStart(null));
        }

        // ---------- a name that cannot be used stops the run, FR-159 and FR-165 ----------

        /// <summary>
        /// Emptying a pattern box turned the refusal sentence into the file name, and the one check
        /// before a run looked at collisions only, so it passed for one ticked group and called the
        /// sentence a collision for several. The check names the empty field instead.
        /// </summary>
        [Test]
        public void AnEmptiedPatternFieldIsNamedAndIsNeverACollision()
        {
            OutputNaming naming = new OutputNaming();
            naming.Nwf.Level = string.Empty;
            OutputNameTable table = Table(naming);

            string all = table.WhyTheRunCannotStart(Internal);

            Assert.That(all, Is.Not.Null);
            Assert.That(all, Is.EqualTo(
                "The NWF name of 3 groups cannot be used. The level is empty, so the name would have a hole in it."
                + " They are 1B06BS, 1C07BC, 1C07K1. The run does not start."));
            Assert.That(all, Does.Not.Contain("would be written to the same"));

            string one = TableOf(new[] { Folder[0], Folder[1] }, naming).WhyTheRunCannotStart(Internal);

            Assert.That(one, Is.Not.Null, "one ticked group passed the check with its name a sentence");
            Assert.That(one, Is.EqualTo(
                "The NWF name of 1C07BC cannot be used. The level is empty, so the name would have a hole in it."
                + " The run does not start."));
        }

        /// <summary>
        /// One emptied box made a paragraph for every group, twenty two of them in a refusal dialog. Each
        /// cause is one sentence with its count and at most five groups named, the repeat rule.
        /// </summary>
        [Test]
        public void ManyGroupsOnOneCauseAreOneSentenceWithTheFirstFiveNamed()
        {
            List<string> files = new List<string>();

            foreach (string building in new[] { "1A01AA", "1A02AA", "1A03AA", "1A04AA", "1A05AA", "1A06AA", "1A07AA", "1A08AA" })
            {
                files.Add("1104-PAR-" + building + "-ZZZ-AR-MOD-000001.nwc");
            }

            OutputNaming naming = new OutputNaming();
            BuildingGroupingResult result = BuildingGrouping.GroupNames(files.ToArray(), Settings, GroupingMode.PerBuilding);
            naming.Nwf.Level = string.Empty;
            OutputNameTable table = OutputNameTable.From(result.Groups, naming, Settings, Friday);

            string all = table.WhyTheRunCannotStart(Internal);

            Assert.That(all, Does.StartWith("The NWF name of 8 groups cannot be used."));
            Assert.That(all, Does.Contain("They are 1A01AA, 1A02AA, 1A03AA, 1A04AA, 1A05AA and 3 more."));
            Assert.That(all.Split(new[] { "cannot be used" }, StringSplitOptions.None).Length - 1, Is.EqualTo(1));
        }

        /// <summary>A name cell cleared by hand, for each of the three outputs, is refused and the refusal names which.</summary>
        [Test]
        public void AClearedNameCellIsRefusedForEachOutput()
        {
            foreach (OutputKind kind in OutputNameTable.AllKinds())
            {
                foreach (string cleared in new[] { string.Empty, "   " })
                {
                    OutputNameTable table = Table(new OutputNaming());
                    table.Find("1C07BC").Fill(kind, cleared);

                    string why = table.WhyTheRunCannotStart(Internal);

                    Assert.That(why, Is.Not.Null, kind + " cleared to \"" + cleared + "\"");
                    Assert.That(why, Is.EqualTo(
                        "The " + OutputNaming.Labels()[(int)kind] + " name of 1C07BC cannot be used, it is empty."
                        + " The run does not start."));
                }
            }
        }

        /// <summary>
        /// FR-165's second half. A name typed by hand with a character Windows refuses, a colon or a slash,
        /// passed the check before a run and reached the write. Each of the refused characters is named,
        /// and an ordinary name with spaces and dashes still passes.
        /// </summary>
        [Test]
        public void AHandTypedNameWithACharacterWindowsRefusesIsRefusedAndNamed()
        {
            foreach (char refused in FileNames.RefusedPrintable)
            {
                OutputNameTable table = Table(new OutputNaming());
                table.Find("1C07BC").Fill(OutputKind.Nwd, "Tower " + refused + " east");

                string why = table.WhyTheRunCannotStart(Internal);

                Assert.That(why, Is.EqualTo(
                    "The NWD name of 1C07BC cannot be used, it holds \"" + refused
                    + "\", which Windows does not allow in a file name. The run does not start."),
                    "the character " + refused);
            }
        }

        [Test]
        public void AControlCharacterInAHandTypedNameIsNamedByItsCode()
        {
            OutputNameTable table = Table(new OutputNaming());
            table.Find("1C07BC").Fill(OutputKind.Workbook, "Tower\teast");

            string why = table.WhyTheRunCannotStart(Internal);

            Assert.That(why, Does.Contain("a control character (U+0009)"));
            Assert.That(why, Does.Not.Contain("\t"));
        }

        [Test]
        public void AnOrdinaryHandTypedNameStillStarts()
        {
            OutputNameTable table = Table(new OutputNaming());
            table.Find("1C07BC").Fill(OutputKind.Nwf, "Tower East - rev 2 (final)");

            Assert.That(table.WhyTheRunCannotStart(Internal), Is.Null);
        }

        /// <summary>
        /// A collision between many groups named every one of them, where the repeat rule names five. The
        /// sentence carries the count, the first five and how many more, as an unusable name does.
        /// </summary>
        [Test]
        public void AManyGroupCollisionNamesFiveAndCountsTheRest()
        {
            List<string> files = new List<string>();

            foreach (string building in new[] { "1A01AA", "1A02AA", "1A03AA", "1A04AA", "1A05AA", "1A06AA", "1A07AA", "1A08AA" })
            {
                files.Add("1104-PAR-" + building + "-ZZZ-AR-MOD-000001.nwc");
            }

            BuildingGroupingResult result =
                BuildingGrouping.GroupNames(files.ToArray(), Settings, GroupingMode.PerBuilding);
            OutputNameTable table = OutputNameTable.From(result.Groups, new OutputNaming(), Settings, Friday);

            foreach (string building in new[] { "1A01AA", "1A02AA", "1A03AA", "1A04AA", "1A05AA", "1A06AA", "1A07AA", "1A08AA" })
            {
                table.Find(building).Fill(OutputKind.Nwf, "SameName");
            }

            IList<NameCollision> collisions = table.Collisions();

            Assert.That(collisions.Count, Is.EqualTo(1));
            Assert.That(collisions[0].Groups.Count, Is.EqualTo(8), "the collision itself still holds every group");
            Assert.That(collisions[0].Sentence(), Is.EqualTo(
                "8 groups would be written to the same NWF name, SameName. They are 1A01AA, 1A02AA, 1A03AA,"
                + " 1A04AA, 1A05AA and 3 more. One would overwrite the other, so the run does not start."));
        }

        /// <summary>A table whose names are all usable still starts, and a collision is still worded as one.</summary>
        [Test]
        public void UsableNamesStillStartAndACollisionIsStillACollision()
        {
            OutputNameTable table = Table(new OutputNaming());

            Assert.That(table.WhyTheRunCannotStart(Internal), Is.Null);

            table.Find("1C07K1").Fill(OutputKind.Nwf, table.Find("1C07BC").NwfName);

            Assert.That(table.WhyTheRunCannotStart(Internal), Does.Contain("would be written to the same"));
            Assert.That(table.WhyTheRunCannotStart(Internal), Does.Not.Contain("cannot be used"));
        }

        // ---------- the dated NWD ----------

        // The one the brief asks for by name.
        [Test]
        public void TheDatedNwdCarriesTheDateWhereTheNumberWas()
        {
            OutputNaming naming = new OutputNaming();
            naming.DateTheNwd = true;

            OutputNameTable table = Table(naming);

            Assert.That(table.Find("1C07BC").NwdName,
                Is.EqualTo("1104-PAR-1C07BC-ZZZ-BM-MOD-20260904"));
        }

        // The NWF always overwrites. Its clash results are the record, so it has to be the
        // same file week after week.
        [Test]
        public void OnlyTheNwdIsDatedAndTheNwfNeverIs()
        {
            OutputNaming naming = new OutputNaming();
            naming.DateTheNwd = true;

            OutputNameRow row = Table(naming).Find("1C07BC");

            Assert.That(row.NwfName, Is.EqualTo("1104-PAR-1C07BC-ZZZ-BM-MOD-000001"),
                "the NWF was dated, which would start its clash history again every week");
            Assert.That(row.WorkbookName, Is.EqualTo("1104-PAR-1C07BC-ZZZ-BM-MOD-000001"));
            Assert.That(row.NwdName, Is.Not.EqualTo(row.NwfName));
        }

        [Test]
        public void DatingIsOffByDefault()
        {
            Assert.That(new OutputNaming().DateTheNwd, Is.False);
            Assert.That(Table(new OutputNaming()).Find("1C07BC").NwdName,
                Is.EqualTo("1104-PAR-1C07BC-ZZZ-BM-MOD-000001"));
        }

        [Test]
        public void TheDateFormatIsASetting()
        {
            OutputNaming naming = new OutputNaming();
            naming.DateTheNwd = true;
            naming.Nwd.DateFormat = "yyyy-MM-dd";

            Assert.That(Table(naming).Find("1C07BC").NwdName,
                Is.EqualTo("1104-PAR-1C07BC-ZZZ-BM-MOD-2026-09-04"));
        }

        [Test]
        public void TheDefaultFormatSortsByNameTheWayItSortsByDate()
        {
            Assert.That(NamePattern.DefaultDateFormat, Is.EqualTo("yyyyMMdd"));

            OutputNaming naming = new OutputNaming();
            naming.DateTheNwd = true;

            string january = OutputNameTable.From(
                BuildingGrouping.GroupNames(Folder, Settings).Groups,
                naming, Settings, new DateTime(2026, 1, 5)).Find("1C07BC").NwdName;

            string december = OutputNameTable.From(
                BuildingGrouping.GroupNames(Folder, Settings).Groups,
                naming, Settings, new DateTime(2026, 12, 7)).Find("1C07BC").NwdName;

            Assert.That(string.Compare(january, december, StringComparison.Ordinal),
                Is.LessThan(0), "the two would sort out of order in a folder listing");
        }

        // DateTime.ToString does NOT throw on a format nobody can read. It treats what it
        // does not recognise as literal text, so a nonsense format gives a nonsense date
        // rather than an error. Measured, not assumed.
        [Test]
        public void ANonsenseFormatIsUsedAsTypedBecauseItIsASetting()
        {
            OutputNaming naming = new OutputNaming();
            naming.DateTheNwd = true;
            naming.Nwd.DateFormat = "not a real format";

            string name = Table(naming).Find("1C07BC").NwdName;

            Assert.That(name, Does.StartWith("1104-PAR-1C07BC-ZZZ-BM-MOD-"));
            Assert.That(name, Does.Not.EndWith("-000001"),
                "the format was typed by a person and is theirs to get wrong, visibly");
        }

        // A format that would make a name Windows refuses is a different matter. That one
        // falls back, because the alternative is a write that fails at the last moment.
        [Test]
        public void AFormatThatWouldMakeAnIllegalFileNameFallsBackToTheNumber()
        {
            foreach (string bad in new[] { "yyyy/MM/dd", "yyyy:MM", "yyyy|MM" })
            {
                OutputNaming naming = new OutputNaming();
                naming.DateTheNwd = true;
                naming.Nwd.DateFormat = bad;

                Assert.That(Table(naming).Find("1C07BC").NwdName, Does.EndWith("-000001"), bad);
            }
        }

        // A format DateTime.ToString throws on gives no date at all, so there is no result to fall back from.
        // The number in its place named this week's NWD as last week's and wrote over it, FR-173, so the name
        // says it cannot be built, as an emptied field does, and the run does not start.
        [Test]
        public void AFormatTheDateCannotBeWrittenWithIsToldAndNeverReplacedByTheNumber()
        {
            foreach (string bad in new[] { "yyyy'", "%" })
            {
                OutputNaming naming = new OutputNaming();
                naming.DateTheNwd = true;
                naming.Nwd.DateFormat = bad;

                OutputNameRow row = Table(naming).Find("1C07BC");

                Assert.That(row.NwdName, Does.StartWith(OutputNameTable.CannotBeNamed), bad);
                Assert.That(row.NwdName, Does.Contain(bad), bad);
                Assert.That(row.NwdName, Does.Not.EndWith("-000001"), bad);
                Assert.That(row.NwfName, Is.EqualTo("1104-PAR-1C07BC-ZZZ-BM-MOD-000001"), bad);

                string why = Table(naming).WhyTheRunCannotStart(Internal);

                Assert.That(why, Is.Not.Null, bad);
                Assert.That(why, Does.Contain("not one a date can be written with"), bad);
                Assert.That(why, Does.Not.Contain("Exception"), bad);
            }
        }

        // ---------- refusals ----------

        [Test]
        public void NoGroupsOrNoNamingIsRefusedRatherThanBuilt()
        {
            Assert.Throws<ArgumentNullException>(
                delegate { OutputNameTable.From(null, new OutputNaming(), Settings, Friday); });
            Assert.Throws<ArgumentNullException>(
                delegate { OutputNameTable.From(new List<BuildingGroup>(), null, Settings, Friday); });
        }

        [Test]
        public void AGroupThatIsNotThereComesBackAsNothing()
        {
            Assert.That(Table(new OutputNaming()).Find("nope"), Is.Null);
        }
    }
}
