using System;
using System.Collections.Generic;
using Federator.Core.Grouping;
using Federator.Core.Naming;
using NUnit.Framework;

namespace Federator.Core.Tests
{
    /// <summary>
    /// The four fields the tool supplies rather than reads out of a file name, and the
    /// check that stops two groups being written to one name.
    ///
    /// Outputs overwrite with no date suffix, so two groups sharing a name is not a
    /// warning. The second silently destroys the first and only shows up later as a
    /// federation nobody can find.
    /// </summary>
    [TestFixture]
    public class NamePatternTests
    {
        private static readonly ContainerNameSettings Settings = new ContainerNameSettings();

        private static readonly string[] TwoBuildings =
        {
            "1104-PAR-1C07BC-ZZZ-AR-MOD-000001.nwc",
            "1104-PAR-1C07BC-ZZZ-ST-MOD-000001.nwc",
            "1104-PAR-1C07K1-ZZZ-AR-MOD-000001.nwc"
        };

        private static BuildingGroupingResult Group(GroupingMode mode)
        {
            return BuildingGrouping.GroupNames(TwoBuildings, Settings, mode);
        }

        // ---------- the defaults ----------

        [Test]
        public void TheDefaultsAreTheOnesTheProjectHasBeenUsing()
        {
            NamePattern pattern = new NamePattern();

            Assert.That(pattern.Level, Is.EqualTo("ZZZ"));
            Assert.That(pattern.Discipline, Is.EqualTo("BM"));
            Assert.That(pattern.TypeCode, Is.EqualTo("MOD"));
            Assert.That(pattern.Number, Is.EqualTo("000001"));
            Assert.That(pattern.AllBuildings, Is.EqualTo("ZZZZZZ"));
        }

        [Test]
        public void TheThreePatternsStartIdenticalAndAreSeparateObjects()
        {
            OutputNaming naming = new OutputNaming();

            Assert.That(naming.Nwf.Level, Is.EqualTo(naming.Nwd.Level));
            Assert.That(naming.Nwf.Level, Is.EqualTo(naming.Workbook.Level));

            naming.Workbook.TypeCode = "RPT";

            Assert.That(naming.Nwf.TypeCode, Is.EqualTo("MOD"),
                "changing one pattern changed another");
            Assert.That(naming.Workbook.TypeCode, Is.EqualTo("RPT"));
        }

        [Test]
        public void EachFieldIsASettingAndReachesTheName()
        {
            NamePattern pattern = new NamePattern();
            pattern.Level = "L01";
            pattern.Discipline = "XX";
            pattern.TypeCode = "FED";
            pattern.Number = "000042";

            Assert.That(
                pattern.NameFor(Group(GroupingMode.PerBuilding).Find("1C07BC"), Settings),
                Is.EqualTo("1104-PAR-1C07BC-L01-XX-FED-000042"));
        }

        [Test]
        public void TheAllBuildingsCodeIsASettingToo()
        {
            NamePattern pattern = new NamePattern();
            pattern.AllBuildings = "SITE01";

            Assert.That(
                pattern.NameFor(Group(GroupingMode.Everything).Groups[0], Settings),
                Is.EqualTo("1104-PAR-SITE01-ZZZ-BM-MOD-000001"));
        }

        [Test]
        public void ThreePatternsCanGiveThreeDifferentNamesForOneGroup()
        {
            BuildingGroup group = Group(GroupingMode.PerBuilding).Find("1C07BC");

            OutputNaming naming = new OutputNaming();
            naming.Nwd.TypeCode = "PUB";
            naming.Workbook.TypeCode = "RPT";

            Assert.That(naming.Nwf.NameFor(group, Settings),
                Is.EqualTo("1104-PAR-1C07BC-ZZZ-BM-MOD-000001"));
            Assert.That(naming.Nwd.NameFor(group, Settings),
                Is.EqualTo("1104-PAR-1C07BC-ZZZ-BM-PUB-000001"));
            Assert.That(naming.Workbook.NameFor(group, Settings),
                Is.EqualTo("1104-PAR-1C07BC-ZZZ-BM-RPT-000001"));
        }

        [Test]
        public void TheSeparatorStillComesFromTheParserSettings()
        {
            ContainerNameSettings underscores = new ContainerNameSettings();
            underscores.Separator = '_';

            BuildingGroupingResult result = BuildingGrouping.GroupNames(
                new[] { "1104_PAR_1C07BC_ZZZ_AR_MOD_000001.nwc" }, underscores, GroupingMode.PerBuilding);

            Assert.That(new NamePattern().NameFor(result.Find("1C07BC"), underscores),
                Is.EqualTo("1104_PAR_1C07BC_ZZZ_BM_MOD_000001"));
        }

        // ---------- an empty field is refused rather than leaving a hole ----------

        [Test]
        public void AnEmptyFieldIsRefusedRatherThanWritingAHoleInTheName()
        {
            string[] fields = { "Level", "Discipline", "TypeCode", "Number", "AllBuildings" };

            foreach (string field in fields)
            {
                NamePattern pattern = new NamePattern();

                if (field == "Level") { pattern.Level = string.Empty; }
                if (field == "Discipline") { pattern.Discipline = string.Empty; }
                if (field == "TypeCode") { pattern.TypeCode = string.Empty; }
                if (field == "Number") { pattern.Number = string.Empty; }
                if (field == "AllBuildings") { pattern.AllBuildings = string.Empty; }

                Assert.That(pattern.WhyUnusable(), Is.Not.Null, field + " was accepted empty");
                Assert.Throws<InvalidOperationException>(
                    delegate { pattern.NameFor("1104", "PAR", string.Empty, string.Empty, Settings); },
                    field);
            }
        }

        [Test]
        public void AFieldOfNothingButSpacesIsAlsoRefused()
        {
            NamePattern pattern = new NamePattern();
            pattern.Number = "   ";

            Assert.That(pattern.WhyUnusable(), Does.Contain("number is empty"));
        }

        [Test]
        public void AUsablePatternSaysSoRatherThanGivingAReason()
        {
            Assert.That(new NamePattern().WhyUnusable(), Is.Null);
        }

        [Test]
        public void NothingAtAllIsRefusedRatherThanChecked()
        {
            Assert.Throws<ArgumentNullException>(
                delegate { new NamePattern().NameFor((BuildingGroup)null, Settings); });
        }
    }
}
