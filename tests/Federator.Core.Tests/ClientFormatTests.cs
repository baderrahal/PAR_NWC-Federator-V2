using System.Collections.Generic;
using Federator.Core.Report;
using NUnit.Framework;

namespace Federator.Core.Tests
{
    /// <summary>
    /// The columns of the report the client has already accepted, in their order and in
    /// their words.
    ///
    /// Every expected value in here was read off the two files Bader supplied on
    /// 2026-08-31 and off the stylesheet Navisworks wrote them with. None of it is a
    /// guess, so a failure here means the format moved rather than that the test is fussy:
    ///
    ///   C:\00_NM\Clash report\1104-PAR-1A02WO-XXX-BM-RPT-000001.html
    ///   C:\00_NM\Clash report\1104-PAR-1A02WO-XXX-BM-RPT-000001.xlsx
    ///   Navisworks Manage 2025\en-US\stylesheets\clash_report_html_tabular.xsl
    /// </summary>
    [TestFixture]
    public class ClientFormatTests
    {
        // ---------- their columns, their order, their words ----------

        // The one the brief asks for by name.
        [Test]
        public void EveryClientColumnIsThereInTheirOrder()
        {
            Assert.That(ClientFormat.ClashColumns, Is.EqualTo(new[]
            {
                "Image", "Clash Name", "Status", "Distance", "Grid Location", "Description",
                "Clash Point",
                "Item ID", "Layer", "Item Name", "Item Type",
                "Item ID", "Layer", "Item Name", "Item Type"
            }));
        }

        [Test]
        public void TheTestHeaderIsTheirNineInTheirOrder()
        {
            Assert.That(ClientFormat.TestHeader, Is.EqualTo(new[]
            {
                "Tolerance", "Clashes", "New", "Active", "Reviewed", "Approved", "Resolved",
                "Type", "Status"
            }));
        }

        [Test]
        public void TheItemBlocksSitWhereTheirMergedLabelsSay()
        {
            // Seven general columns, then three per item, twice. That is what the
            // colspans in their header row add up to.
            Assert.That(ClientFormat.FirstItemColumn, Is.EqualTo(7));
            Assert.That(ClientFormat.ItemColumns, Is.EqualTo(4), "Item ID, Layer, Item Name, Item Type");
            Assert.That(ClientFormat.ClashColumns.Length,
                Is.EqualTo(ClientFormat.FirstItemColumn + (2 * ClientFormat.ItemColumns)));
            Assert.That(ClientFormat.ItemGroup1, Is.EqualTo("Item 1"));
            Assert.That(ClientFormat.ItemGroup2, Is.EqualTo("Item 2"));
        }

        // ---------- Grid Location is one field ----------

        // The one the brief asks for by name.
        [Test]
        public void GridLocationIsOneFieldAndNotTwoColumns()
        {
            Assert.That(ClientFormat.GridLocation("B-1", "ROF"), Is.EqualTo("B-1 : ROF"));
            Assert.That(ClientFormat.GridLocation("A-2", "LGF"), Is.EqualTo("A-2 : LGF"));
        }

        [Test]
        public void AMissingHalfTakesTheSeparatorWithIt()
        {
            Assert.That(ClientFormat.GridLocation("B-1", ""), Is.EqualTo("B-1"),
                "a model with no levels should not read B-1 with a dangling colon");
            Assert.That(ClientFormat.GridLocation("", "ROF"), Is.EqualTo("ROF"));
            Assert.That(ClientFormat.GridLocation("", ""), Is.EqualTo(string.Empty));
            Assert.That(ClientFormat.GridLocation(null, null), Is.EqualTo(string.Empty));
        }

        // ---------- Clash Point is one field ----------

        // The one the brief asks for by name.
        [Test]
        public void ClashPointIsOneFieldAndNotThreeColumns()
        {
            Assert.That(ClientFormat.ClashPoint(31.643, -2.913, 3.325),
                Is.EqualTo("x:31.643, y:-2.913, z:3.325"));
            Assert.That(ClientFormat.ClashPoint(28.596, 0.814, 2.114),
                Is.EqualTo("x:28.596, y:0.814, z:2.114"));
        }

        // Their file writes z:-0.050, so the zeros are kept rather than trimmed.
        [Test]
        public void ThreeDecimalsAlwaysIncludingTheTrailingZeros()
        {
            Assert.That(ClientFormat.ClashPoint(31.601, -1.479, -0.05),
                Is.EqualTo("x:31.601, y:-1.479, z:-0.050"));
            Assert.That(ClientFormat.ClashPoint(0, 0, 0), Is.EqualTo("x:0.000, y:0.000, z:0.000"));
        }

        [Test]
        public void MorePrecisionThanTheyShowIsRoundedRatherThanPrinted()
        {
            Assert.That(ClientFormat.ClashPoint(31.6431119, -2.9134444, 3.3249),
                Is.EqualTo("x:31.643, y:-2.913, z:3.325"));
        }

        // ---------- Item ID carries its own label ----------

        [Test]
        public void AnItemIdReadsAsTheirsDoes()
        {
            Assert.That(ClientFormat.ItemId("Element ID", "1554240"),
                Is.EqualTo("Element ID: 1554240"));
            Assert.That(ClientFormat.ItemId("Element ID", "784457"),
                Is.EqualTo("Element ID: 784457"));
        }

        // The label is whatever property carried the id, not a constant. Their stylesheet
        // writes objectattribute/name, so a model that is not from Revit reads differently
        // and that is correct rather than a fault.
        [Test]
        public void TheLabelIsWhateverCarriedTheIdAndFallsBackRatherThanBeingEmpty()
        {
            Assert.That(ClientFormat.ItemId("Instance GUID", "abc"), Is.EqualTo("Instance GUID: abc"));
            Assert.That(ClientFormat.ItemId("", "1554240"), Is.EqualTo("Element ID: 1554240"));
            Assert.That(ClientFormat.ItemId(null, "1554240"), Is.EqualTo("Element ID: 1554240"));
        }

        [Test]
        public void NoIdAtAllIsEmptyRatherThanALabelWithNothingAfterIt()
        {
            Assert.That(ClientFormat.ItemId("Element ID", ""), Is.EqualTo(string.Empty));
            Assert.That(ClientFormat.ItemId("Element ID", null), Is.EqualTo(string.Empty));
        }

        // ---------- Tolerance carries its unit ----------

        [Test]
        public void ToleranceCarriesItsUnitWithNoSpace()
        {
            // Three decimals, which is how theirs is written. Ours read 0.2460629921ft
            // on a real run, which was the file's own precision rather than a format.
            Assert.That(ClientFormat.Tolerance(0.025, "m"), Is.EqualTo("0.025m"));
            Assert.That(ClientFormat.Tolerance(0.075, "m"), Is.EqualTo("0.075m"));
            Assert.That(ClientFormat.Tolerance(0.2460629921, "ft"), Is.EqualTo("0.246ft"));
        }

        [Test]
        public void AToleranceWithNoUnitIsJustTheNumber()
        {
            Assert.That(ClientFormat.Tolerance(0.025, ""), Is.EqualTo("0.025"));
            Assert.That(ClientFormat.Tolerance(0.025, null), Is.EqualTo("0.025"));
            Assert.That(ClientFormat.Tolerance(1, "m"), Is.EqualTo("1.000m"),
                "three decimals always, the same as theirs");
        }

        // ---------- the row builds its own single fields ----------

        [Test]
        public void AClashRowBuildsBothSingleFieldsItself()
        {
            ClashRow row = new ClashRow();
            row.GridLocation = "B-1";
            row.Level = "ROF";
            row.X = 31.643;
            row.Y = -2.913;
            row.Z = 3.325;

            Assert.That(row.ClientGridLocation(), Is.EqualTo("B-1 : ROF"));
            Assert.That(row.ClientClashPoint(), Is.EqualTo("x:31.643, y:-2.913, z:3.325"));
        }

        [Test]
        public void AnItemBuildsItsOwnIdField()
        {
            ClashItem item = new ClashItem();
            item.ElementId = "1554240";

            Assert.That(item.IdLabel, Is.EqualTo("Element ID"), "the default should be theirs");
            Assert.That(item.ClientId(), Is.EqualTo("Element ID: 1554240"));

            item.IdLabel = "Instance GUID";
            Assert.That(item.ClientId(), Is.EqualTo("Instance GUID: 1554240"));
        }

        [Test]
        public void ATestBuildsItsOwnToleranceField()
        {
            TestReport test = new TestReport(1, "BLD-DR-Pipes & Pipe Fittings-vs-BLD-AR-Walls");
            test.Tolerance = 0.025;
            test.ToleranceUnits = "m";

            Assert.That(test.ClientTolerance(), Is.EqualTo("0.025m"));
        }

        // ---------- nothing of ours is in the client layout at all ----------

        // What used to be here tested that our columns came after theirs. There are no
        // columns of ours now. The workbook is the client's report and nothing else, so
        // the whole set is theirs and there is nothing to come after it.
        [Test]
        public void TheColumnSetIsTheirsAndHasNoRoomForOurs()
        {
            Assert.That(ClientFormat.ClashColumns.Length, Is.EqualTo(15));

            foreach (string ours in new[]
            {
                "Family", "Type Name", "Material", "Source File", "Discipline", "Date Found",
                "Raw clashes"
            })
            {
                Assert.That(ClientFormat.ClashColumns, Does.Not.Contain(ours), ours);
            }
        }

        [Test]
        public void TheTwoTypeColumnsAreDifferentThings()
        {
            ClashItem item = new ClashItem();
            item.ItemType = "Solid";
            item.Type = "Generic 200mm";

            Assert.That(item.ItemType, Is.Not.EqualTo(item.Type));
        }
    }
}
