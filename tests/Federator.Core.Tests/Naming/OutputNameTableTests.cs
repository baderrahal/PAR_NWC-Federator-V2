using System;
using System.Collections.Generic;
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

        private static readonly string[] Folder =
        {
            "1104-PAR-1C07BC-ZZZ-AR-MOD-000001.nwc",
            "1104-PAR-1C07BC-ZZZ-ST-MOD-000001.nwc",
            "1104-PAR-1C07K1-ZZZ-AR-MOD-000001.nwc",
            "1104-PAR-1B06BS-ZZZ-EL-MOD-000001.nwc"
        };

        private static OutputNameTable Table(OutputNaming naming)
        {
            BuildingGroupingResult result = BuildingGrouping.GroupNames(
                Folder, Settings, GroupingMode.PerBuilding);

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

        [Test]
        public void NothingStartsOutTypedOver()
        {
            foreach (OutputNameRow row in Table(new OutputNaming()).Rows)
            {
                Assert.That(row.WasEdited, Is.False);

                foreach (OutputKind kind in OutputNameTable.AllKinds())
                {
                    Assert.That(row.IsByHand(kind), Is.False);
                }
            }
        }

        // ---------- editing one row changes only that row ----------

        [Test]
        public void TypingOverOneNameChangesOnlyThatName()
        {
            OutputNameTable table = Table(new OutputNaming());
            string before = table.Find("1C07K1").NwfName;

            table.Find("1C07BC").SetByHand(OutputKind.Nwf, "HAND-TYPED-NAME");

            Assert.That(table.Find("1C07BC").NwfName, Is.EqualTo("HAND-TYPED-NAME"));
            Assert.That(table.Find("1C07K1").NwfName, Is.EqualTo(before),
                "editing one row changed another");
            Assert.That(table.Find("1C07BC").NwdName,
                Is.EqualTo("1104-PAR-1C07BC-ZZZ-BM-MOD-000001"),
                "editing the NWF name changed the NWD name");
        }

        // ---------- a pattern change refills only what was not typed over ----------

        // The one the brief asks for by name.
        [Test]
        public void APatternChangeRefillsOnlyTheRowsNotTypedOver()
        {
            OutputNaming naming = new OutputNaming();
            OutputNameTable table = Table(naming);

            table.Find("1C07BC").SetByHand(OutputKind.Nwf, "HAND-TYPED-NAME");

            naming.Nwf.TypeCode = "FED";
            int kept = table.Refill(naming, Settings);

            Assert.That(kept, Is.EqualTo(1), "one row had been typed over");
            Assert.That(table.Find("1C07BC").NwfName, Is.EqualTo("HAND-TYPED-NAME"),
                "a hand typed name was overwritten by a pattern change");
            Assert.That(table.Find("1C07K1").NwfName,
                Is.EqualTo("1104-PAR-1C07K1-ZZZ-BM-FED-000001"),
                "a row that was not typed over should have been refilled");
        }

        [Test]
        public void OnlyTheNameThatWasTypedOverIsHeld()
        {
            OutputNaming naming = new OutputNaming();
            OutputNameTable table = Table(naming);

            table.Find("1C07BC").SetByHand(OutputKind.Nwf, "HAND-TYPED-NAME");

            naming.Nwf.TypeCode = "FED";
            naming.Nwd.TypeCode = "PUB";
            table.Refill(naming, Settings);

            Assert.That(table.Find("1C07BC").NwfName, Is.EqualTo("HAND-TYPED-NAME"));
            Assert.That(table.Find("1C07BC").NwdName,
                Is.EqualTo("1104-PAR-1C07BC-ZZZ-BM-PUB-000001"),
                "the NWD name was held even though only the NWF name was typed over");
        }

        [Test]
        public void TheRefillSaysHowManyItKept()
        {
            OutputNaming naming = new OutputNaming();
            OutputNameTable table = Table(naming);

            Assert.That(OutputNameTable.DescribeRefill(table.Count, table.Refill(naming, Settings)),
                Does.Contain("3 rows refilled, every name of each."));

            table.Find("1C07BC").SetByHand(OutputKind.Nwf, "A");
            table.Find("1C07K1").SetByHand(OutputKind.Nwd, "B");

            string said = OutputNameTable.DescribeRefill(table.Count, table.Refill(naming, Settings));

            Assert.That(said, Is.EqualTo("7 of 9 names refilled. 2 names were typed over by hand and left alone."));
        }

        /// <summary>
        /// FR-130. A row with only its NWF name typed over has two of its three names refilled, and the
        /// message counted rows, so it said a whole row was left alone. It counts names, and one row
        /// with nothing kept is not said to be one name refilled.
        /// </summary>
        [Test]
        public void TheRefillCountsNamesAndNeverCallsThreeNamesOne()
        {
            OutputNaming naming = new OutputNaming();
            OutputNameTable table = Table(naming);

            table.Find("1C07BC").SetByHand(OutputKind.Nwf, "A");

            Assert.That(OutputNameTable.DescribeRefill(table.Count, table.Refill(naming, Settings)),
                Is.EqualTo("8 of 9 names refilled. 1 name was typed over by hand and left alone."));
            Assert.That(OutputNameTable.DescribeRefill(1, 0), Is.EqualTo("1 row refilled, every name of each."));
            Assert.That(OutputNameTable.DescribeRefill(1, 0), Does.Not.Contain("1 name refilled"));
            Assert.That(OutputNameTable.DescribeRefill(1, 1), Is.EqualTo("2 of 3 names refilled. 1 name was typed over by hand and left alone."));
        }

        // Setting a name back to what the pattern gives still counts as by hand, because
        // the person meant that value.
        [Test]
        public void TypingTheSameValueStillHoldsItAgainstAPatternChange()
        {
            OutputNaming naming = new OutputNaming();
            OutputNameTable table = Table(naming);

            string same = table.Find("1C07BC").NwfName;
            table.Find("1C07BC").SetByHand(OutputKind.Nwf, same);

            naming.Nwf.TypeCode = "FED";
            table.Refill(naming, Settings);

            Assert.That(table.Find("1C07BC").NwfName, Is.EqualTo(same));
        }

        [Test]
        public void AHandTypedNameCanBeGivenBackToThePattern()
        {
            OutputNaming naming = new OutputNaming();
            OutputNameTable table = Table(naming);

            table.Find("1C07BC").SetByHand(OutputKind.Nwf, "HAND-TYPED-NAME");
            table.Find("1C07BC").ReleaseToPattern(OutputKind.Nwf);

            naming.Nwf.TypeCode = "FED";
            table.Refill(naming, Settings);

            Assert.That(table.Find("1C07BC").NwfName,
                Is.EqualTo("1104-PAR-1C07BC-ZZZ-BM-FED-000001"));
        }

        // ---------- two rows on one name stops the run ----------

        // The one the brief asks for by name.
        [Test]
        public void TwoRowsOnOneNameIsCaughtBeforeTheRun()
        {
            OutputNameTable table = Table(new OutputNaming());

            Assert.That(table.WhyTheRunCannotStart(), Is.Null, "it should start out clean");

            // Typing a name that another row already has is exactly how this happens.
            table.Find("1C07K1").SetByHand(
                OutputKind.Nwf, table.Find("1C07BC").NwfName);

            string why = table.WhyTheRunCannotStart();

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
            table.Find("1C07K1").SetByHand(
                OutputKind.Workbook, table.Find("1C07BC").WorkbookName);

            IList<NameCollision> collisions = table.Collisions();

            Assert.That(collisions.Count, Is.EqualTo(1));
            Assert.That(collisions[0].Kind, Is.EqualTo("Workbook"));
            Assert.That(table.WhyTheRunCannotStart(), Is.Not.Null);
        }

        // Only the ticked rows matter. A name shared with a group nobody ticked is not a
        // collision, because only one of them is going to be written.
        [Test]
        public void ACollisionWithAGroupNobodyTickedIsNotACollision()
        {
            OutputNameTable table = Table(new OutputNaming());
            table.Find("1C07K1").SetByHand(OutputKind.Nwf, table.Find("1C07BC").NwfName);

            Assert.That(table.WhyTheRunCannotStart(), Is.Not.Null);
            Assert.That(
                table.Only(new[] { "1C07BC", "1B06BS" }).WhyTheRunCannotStart(),
                Is.Null,
                "a group nobody ticked was counted as a collision");
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
            OutputNameTable table = Table(naming);

            naming.Nwf.Level = string.Empty;
            table.Refill(naming, Settings);

            string all = table.WhyTheRunCannotStart();

            Assert.That(all, Is.Not.Null);
            Assert.That(all, Is.EqualTo(
                "The NWF name of 3 groups cannot be used. The level is empty, so the name would have a hole in it."
                + " They are 1B06BS, 1C07BC, 1C07K1. The run does not start."));
            Assert.That(all, Does.Not.Contain("would be written to the same"));

            string one = table.Only(new[] { "1C07BC" }).WhyTheRunCannotStart();

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
            OutputNameTable table = OutputNameTable.From(result.Groups, naming, Settings, Friday);

            naming.Nwf.Level = string.Empty;
            table.Refill(naming, Settings);

            string all = table.WhyTheRunCannotStart();

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
                    table.Find("1C07BC").SetByHand(kind, cleared);

                    string why = table.Only(new[] { "1C07BC" }).WhyTheRunCannotStart();

                    Assert.That(why, Is.Not.Null, kind + " cleared to \"" + cleared + "\"");
                    Assert.That(why, Is.EqualTo(
                        "The " + OutputNaming.Labels()[(int)kind] + " name of 1C07BC cannot be used, it is empty."
                        + " The run does not start."));
                }
            }
        }

        /// <summary>A table whose names are all usable still starts, and a collision is still worded as one.</summary>
        [Test]
        public void UsableNamesStillStartAndACollisionIsStillACollision()
        {
            OutputNameTable table = Table(new OutputNaming());

            Assert.That(table.WhyTheRunCannotStart(), Is.Null);

            table.Find("1C07K1").SetByHand(OutputKind.Nwf, table.Find("1C07BC").NwfName);

            Assert.That(table.WhyTheRunCannotStart(), Does.Contain("would be written to the same"));
            Assert.That(table.WhyTheRunCannotStart(), Does.Not.Contain("cannot be used"));
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

        // ---------- refusals ----------

        [Test]
        public void NoGroupsOrNoNamingIsRefusedRatherThanBuilt()
        {
            Assert.Throws<ArgumentNullException>(
                delegate { OutputNameTable.From(null, new OutputNaming(), Settings); });
            Assert.Throws<ArgumentNullException>(
                delegate { new OutputNameTable().Refill(null, Settings); });
        }

        [Test]
        public void AGroupThatIsNotThereComesBackAsNothing()
        {
            Assert.That(Table(new OutputNaming()).Find("nope"), Is.Null);
        }
    }
}
