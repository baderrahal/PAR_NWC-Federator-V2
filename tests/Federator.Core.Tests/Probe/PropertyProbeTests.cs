using System;
using System.Collections.Generic;
using System.IO;
using Federator.Core.Clash;
using Federator.Core.Probe;
using NUnit.Framework;

namespace Federator.Core.Tests
{
    /// <summary>
    /// F86. The mechanical selection sets ask for categories and nobody knows what the
    /// models actually carry. The probe reads and writes one CSV per file, changes
    /// nothing, and says whether FS or Fire Suppression appears in any property.
    /// </summary>
    [TestFixture]
    public class PropertyProbeTests
    {
        // ---------- the seventeen ----------

        [Test]
        public void TheProbeReadsSeventeenCategories()
        {
            Assert.That(ProbeSettings.DefaultCategories().Count, Is.EqualTo(17));
            Assert.That(new ProbeSettings().Categories.Count, Is.EqualTo(17));
        }

        /// <summary>
        /// The seventeen are not typed anywhere. They are the thirteen this tool calls a
        /// service plus the four it has decided are not one, both lists read from
        /// PenetrationSettings, so a category cannot be on one list and missing from the
        /// other.
        /// </summary>
        [Test]
        public void TheSeventeenAreTheTwoListsAndNothingElse()
        {
            IList<string> seventeen = ProbeSettings.DefaultCategories();
            List<string> both = new List<string>();
            both.AddRange(PenetrationSettings.DefaultServiceCategories);
            both.AddRange(PenetrationSettings.DefaultNotAServiceCategories);

            Assert.That(seventeen.Count, Is.EqualTo(both.Count));

            for (int i = 0; i < both.Count; i++)
            {
                Assert.That(seventeen[i], Is.EqualTo(both[i]), "position " + i);
            }
        }

        // ---------- what it refuses to read ----------

        [Test]
        public void TheProbeReadsNwcAndRefusesAnNwfByName()
        {
            string why;

            Assert.That(ProbeSettings.MayRead(TestPaths.At("in", "a.nwc"), out why), Is.True);
            Assert.That(why, Is.Null);

            Assert.That(ProbeSettings.MayRead(TestPaths.At("in", "a.nwf"), out why), Is.False);
            Assert.That(why, Does.Contain("never opens an NWF"));
            Assert.That(why, Does.Contain("clash result"));

            Assert.That(ProbeSettings.MayRead(TestPaths.At("in", "a.nwd"), out why), Is.False);
            Assert.That(why, Does.Contain("this tool writes"));

            Assert.That(ProbeSettings.MayRead(TestPaths.At("in", "a.xml"), out why), Is.False);
            Assert.That(ProbeSettings.MayRead(null, out why), Is.False);
            Assert.That(why, Does.Contain("no file was named"));
        }

        [Test]
        public void CaseNeverChangesWhatIsRefused()
        {
            string why;

            Assert.That(ProbeSettings.MayRead(TestPaths.At("in", "A.NWC"), out why), Is.True);
            Assert.That(ProbeSettings.MayRead(TestPaths.At("in", "A.NWF"), out why), Is.False);
        }

        [Test]
        public void TheCsvSitsBesideTheModelAndIsNamedAfterIt()
        {
            string model = TestPaths.At("in", "1104-PAR-1C07BC-ZZZ-ME-MOD-000001.nwc");
            string csv = ProbeSettings.CsvPathFor(model);

            Assert.That(Path.GetDirectoryName(csv), Is.EqualTo(Path.GetDirectoryName(model)));
            Assert.That(Path.GetFileName(csv),
                Is.EqualTo("1104-PAR-1C07BC-ZZZ-ME-MOD-000001-properties.csv"));
        }

        // ---------- counting, capping and sorting ----------

        [Test]
        public void TheSameValueOnManyElementsIsCountedOnce()
        {
            ProbeTally tally = new ProbeTally();

            for (int i = 0; i < 40; i++)
            {
                tally.Add("Pipes", "Element", "System Type", "Domestic Cold Water");
            }

            tally.Add("Pipes", "Element", "System Type", "Fire Protection Wet");

            IList<ProbeRow> rows = tally.Rows();

            Assert.That(rows.Count, Is.EqualTo(2));
            Assert.That(rows[0].Value, Is.EqualTo("Domestic Cold Water"));
            Assert.That(rows[0].Elements, Is.EqualTo(40));
            Assert.That(rows[1].Elements, Is.EqualTo(1));
        }

        [Test]
        public void TheRowsAreSortedByCategoryThenPropertyThenCountHighestFirst()
        {
            ProbeTally tally = new ProbeTally();

            tally.Add("Pipes", "Element", "System Type", "one");
            tally.Add("Pipes", "Element", "System Type", "two");
            tally.Add("Pipes", "Element", "System Type", "two");
            tally.Add("Pipes", "Element", "Diameter", "100");
            tally.Add("Ducts", "Element", "Width", "600");

            IList<ProbeRow> rows = tally.Rows();

            Assert.That(rows[0].Category, Is.EqualTo("Ducts"));
            Assert.That(rows[1].Category, Is.EqualTo("Pipes"));
            Assert.That(rows[1].Property, Is.EqualTo("Diameter"));
            Assert.That(rows[2].Property, Is.EqualTo("System Type"));
            Assert.That(rows[2].Value, Is.EqualTo("two"), "two elements beats one");
            Assert.That(rows[3].Value, Is.EqualTo("one"));
        }

        [Test]
        public void TwoValuesOnTheSameCountKeepTheSameOrderEveryRun()
        {
            ProbeTally first = new ProbeTally();
            ProbeTally second = new ProbeTally();

            first.Add("Pipes", "Element", "Mark", "b");
            first.Add("Pipes", "Element", "Mark", "a");

            second.Add("Pipes", "Element", "Mark", "a");
            second.Add("Pipes", "Element", "Mark", "b");

            Assert.That(first.Rows()[0].Value, Is.EqualTo("a"));
            Assert.That(second.Rows()[0].Value, Is.EqualTo("a"));
        }

        /// <summary>
        /// A mark or a comment carries a different value on every element, so an uncapped
        /// probe would write hundreds of thousands of rows nobody reads. What was dropped
        /// is SAID rather than left out.
        /// </summary>
        [Test]
        public void ThePropertyOverTheCapKeepsTheCommonestAndSaysWhatItLeftOut()
        {
            ProbeTally tally = new ProbeTally(100);

            for (int i = 0; i < 150; i++)
            {
                tally.Add("Pipes", "Element", "Mark", "P-" + i.ToString("000"));
            }

            IList<ProbeRow> rows = tally.Rows();

            Assert.That(rows.Count, Is.EqualTo(101), "a hundred values and one line saying so");
            Assert.That(rows[100].IsTheCapLine, Is.True);
            Assert.That(rows[100].Value, Does.StartWith(ProbeTally.CappedMarker));
            Assert.That(rows[100].Value, Does.Contain("50 more distinct values"));
            Assert.That(rows[100].Value, Does.Contain("the cap is 100"));
            Assert.That(rows[100].Elements, Is.EqualTo(50));
            Assert.That(tally.DistinctValueCount, Is.EqualTo(150), "the count is of what was seen");
            Assert.That(tally.Capped().Count, Is.EqualTo(1));
        }

        [Test]
        public void APropertyUnderTheCapWritesNoCapLine()
        {
            ProbeTally tally = new ProbeTally(100);
            tally.Add("Pipes", "Element", "System Type", "one");

            Assert.That(tally.Rows().Count, Is.EqualTo(1));
            Assert.That(tally.Rows()[0].IsTheCapLine, Is.False);
            Assert.That(tally.Capped(), Is.Empty);
        }

        [Test]
        public void ACapBelowOneIsRefusedWhereItIsSet()
        {
            Assert.Throws<ArgumentOutOfRangeException>(delegate { new ProbeTally(0); });
            Assert.Throws<ArgumentOutOfRangeException>(delegate { new ProbeTally(-5); });
        }

        /// <summary>
        /// Nothing is trimmed. A value with a space on the end is a real thing in this
        /// project's files, and the probe exists to find out what is really there.
        /// </summary>
        [Test]
        public void AValueWithASpaceOnTheEndIsADifferentValue()
        {
            ProbeTally tally = new ProbeTally();
            tally.Add("Pipes", "Element", "System Type", "Domestic");
            tally.Add("Pipes", "Element", "System Type", "Domestic ");

            Assert.That(tally.Rows().Count, Is.EqualTo(2));
        }

        [Test]
        public void ElementsAreCountedOncePerElementAndNotOncePerProperty()
        {
            ProbeTally tally = new ProbeTally();

            tally.AddElement("Pipes");
            tally.Add("Pipes", "Element", "System Type", "a");
            tally.Add("Pipes", "Element", "Diameter", "100");
            tally.Add("Pipes", "Item", "Name", "Pipe Types");

            Assert.That(tally.ElementsIn("Pipes"), Is.EqualTo(1));
            Assert.That(tally.PropertyCount, Is.EqualTo(3));
            Assert.That(tally.ElementsIn("Ducts"), Is.EqualTo(0));
        }

        // ---------- the CSV itself ----------

        [Test]
        public void TheHeaderIsTheFiveColumnsInTheOrderAsked()
        {
            Assert.That(ProbeCsv.Columns, Is.EqualTo(new[]
            {
                "category", "property tab", "property name", "distinct value", "how many elements"
            }).AsCollection);
            Assert.That(ProbeCsv.Header(),
                Is.EqualTo("\"category\",\"property tab\",\"property name\",\"distinct value\",\"how many elements\""));
        }

        [Test]
        public void AValueCarryingACommaAQuoteOrANewlineStillReadsBack()
        {
            ProbeTally tally = new ProbeTally();
            tally.Add("Pipes", "Element", "Comment", "one, two");
            tally.Add("Pipes", "Element", "Comment", "he said \"no\"");
            tally.Add("Pipes", "Element", "Comment", "first\r\nsecond");

            string text = ProbeCsv.Text(tally.Rows());

            Assert.That(text, Does.Contain("\"one, two\""));
            Assert.That(text, Does.Contain("\"he said \"\"no\"\"\""));
            Assert.That(text, Does.Contain("\"first\r\nsecond\""));
        }

        /// <summary>
        /// Read back rather than asserted to have changed, which is the rule the machine
        /// readable log follows for the same reason.
        /// </summary>
        [Test]
        public void AQuotedCellReadsBackAsExactlyWhatWentIn()
        {
            string[] awkward = { "one, two", "he said \"no\"", "a\ttab", "line\nbreak", "" };

            foreach (string value in awkward)
            {
                string quoted = ProbeCsv.Quote(value);
                string back = quoted.Substring(1, quoted.Length - 2).Replace("\"\"", "\"");

                Assert.That(back, Is.EqualTo(value), "value \"" + value + "\"");
            }
        }

        [Test]
        public void EveryRowIsOneLineOfFiveCells()
        {
            ProbeTally tally = new ProbeTally();
            tally.Add("Pipes", "Element", "System Type", "Domestic");

            string text = ProbeCsv.Text(tally.Rows());

            Assert.That(text, Is.EqualTo(ProbeCsv.Header() + "\r\n"
                + "\"Pipes\",\"Element\",\"System Type\",\"Domestic\",\"1\"\r\n"));
        }

        // ---------- the verdict ----------

        [Test]
        public void FireSuppressionIsFoundWhereverItIsWritten()
        {
            Assert.That(ProbeVerdict.NamesFireSuppression("Fire Suppression"), Is.True);
            Assert.That(ProbeVerdict.NamesFireSuppression("fire suppression wet"), Is.True);
            Assert.That(ProbeVerdict.NamesFireSuppression("FS"), Is.True);
            Assert.That(ProbeVerdict.NamesFireSuppression("FS-01"), Is.True);
            Assert.That(ProbeVerdict.NamesFireSuppression("FS_MAIN"), Is.True,
                "an underscore is not a letter or a digit, so it ends the token and FS_MAIN is FS");
            Assert.That(ProbeVerdict.NamesFireSuppression("Duct FS"), Is.True);
            Assert.That(ProbeVerdict.NamesFireSuppression("fs"), Is.True);
        }

        /// <summary>
        /// The break. Two letters matched anywhere would make half the properties in a
        /// model read as fire suppression.
        /// </summary>
        [Test]
        public void TwoLettersInsideAWordIsNotFireSuppression()
        {
            Assert.That(ProbeVerdict.NamesFireSuppression("OFFSET"), Is.False);
            Assert.That(ProbeVerdict.NamesFireSuppression("TRANSFER"), Is.False);
            Assert.That(ProbeVerdict.NamesFireSuppression("Offset Height"), Is.False);
            Assert.That(ProbeVerdict.NamesFireSuppression("Fire Rating"), Is.False);
            Assert.That(ProbeVerdict.NamesFireSuppression(null), Is.False);
            Assert.That(ProbeVerdict.NamesFireSuppression(string.Empty), Is.False);
        }

        [Test]
        public void TheVerdictSaysSoWhenFireSuppressionIsNowhereAtAll()
        {
            ProbeTally tally = new ProbeTally();
            tally.AddElement("Pipes");
            tally.Add("Pipes", "Element", "System Type", "Domestic Cold Water");

            string all = string.Join("\n", new List<string>(ProbeVerdict.Lines(
                "a.nwc", TestPaths.At("in", "a-properties.csv"), tally,
                ProbeSettings.DefaultCategories())).ToArray());

            Assert.That(all, Does.Contain("appear in NO property tab"));
            Assert.That(all, Does.Contain("nothing in the model was changed"));
            Assert.That(all, Does.Contain("categories asked for : 17"));
            Assert.That(all, Does.Contain("categories found     : 1"));
        }

        [Test]
        public void TheVerdictNamesWhereFireSuppressionWasFound()
        {
            ProbeTally tally = new ProbeTally();
            tally.AddElement("Pipes");
            tally.Add("Pipes", "Element", "System Type", "FS Wet Riser");

            IList<ProbeRow> fire = ProbeVerdict.FireSuppressionRows(tally.Rows());

            Assert.That(fire.Count, Is.EqualTo(1));

            string all = string.Join("\n", new List<string>(ProbeVerdict.Lines(
                "a.nwc", "a-properties.csv", tally, ProbeSettings.DefaultCategories())).ToArray());

            Assert.That(all, Does.Contain("FS or Fire Suppression appears in 1 row"));
            Assert.That(all, Does.Contain("System Type"));
            Assert.That(all, Does.Contain("FS Wet Riser"));
        }

        [Test]
        public void TheCapLineIsNeverCountedAsAFireSuppressionFinding()
        {
            ProbeTally tally = new ProbeTally(1);
            tally.Add("Pipes", "Element", "Mark", "a");
            tally.Add("Pipes", "Element", "Mark", "b");

            Assert.That(tally.Rows()[1].IsTheCapLine, Is.True);
            Assert.That(ProbeVerdict.FireSuppressionRows(tally.Rows()), Is.Empty);
        }

        [Test]
        public void TheVerdictNamesTheCategoriesThatFoundNothing()
        {
            ProbeTally tally = new ProbeTally();
            tally.AddElement("Pipes");

            string all = string.Join("\n", new List<string>(ProbeVerdict.Lines(
                "a.nwc", "a.csv", tally, new List<string> { "Pipes", "Sprinklers" })).ToArray());

            Assert.That(all, Does.Contain("categories with no elements at all : 1, Sprinklers"));
        }

        [Test]
        public void AProbeThatReadNothingSaysSoRatherThanReportingZeroes()
        {
            IList<string> lines = ProbeVerdict.Lines("a.nwc", "a.csv", null, null);

            Assert.That(lines.Count, Is.EqualTo(2));
            Assert.That(lines[1], Does.Contain("nothing was read"));
        }

        [Test]
        public void TheButtonLabelAndHelpLineFitTheirLimits()
        {
            Assert.That(ProbeSettings.ButtonLabel, Is.EqualTo("Probe model properties"));
            Assert.That(ProbeSettings.HelpLine.Split(' ').Length, Is.LessThanOrEqualTo(12));
        }

        /// <summary>
        /// F86. The probe asks for a category the way the penetration rule reads one,
        /// trimmed and case blind, so the two agree, and a category neither list holds is
        /// not asked for.
        /// </summary>
        [Test]
        public void TheProbeAsksForACategoryTheWayThePenetrationRuleReadsOne()
        {
            ProbeSettings settings = new ProbeSettings();
            PenetrationSettings rule = new PenetrationSettings();

            foreach (string category in settings.Categories)
            {
                Assert.That(settings.Asks(category), Is.True, category);
                Assert.That(settings.Asks(" " + category.ToUpperInvariant() + " "), Is.True, category);
                Assert.That(rule.IsDecided(category), Is.True, category);
            }

            Assert.That(settings.Asks("Walls"), Is.False);
            Assert.That(settings.Asks(string.Empty), Is.False);
            Assert.That(settings.Asks(null), Is.False);

            settings.Categories = new List<string> { "Ducts" };
            Assert.That(settings.Asks("Pipes"), Is.False, "a narrowed list is the list");
        }
    }
}
