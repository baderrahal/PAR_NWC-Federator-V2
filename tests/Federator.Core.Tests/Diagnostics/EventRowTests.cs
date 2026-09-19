using System;
using Federator.Core.Diagnostics;
using NUnit.Framework;

namespace Federator.Core.Tests
{
    /// <summary>
    /// The row shape, the header and the escaping.
    ///
    /// One stray tab inside a value moves every column after it on that row and a reader
    /// has no way of telling, so the escaping is the part of this that matters most and
    /// it is proved by reading the value back rather than by asserting it changed.
    /// </summary>
    [TestFixture]
    public class EventRowTests
    {
        private static EventRow Row(string name, string number, string text)
        {
            return new EventRow("09:14:22.117", 12.5, "1B06PH", RunSteps.TestsRun,
                "step finished", name, number, text);
        }

        [Test]
        public void TheHeaderIsTheEightNamesInTheOrderTheBriefSet()
        {
            string[] fields = EventRow.Header().Split('\t');

            Assert.That(fields.Length, Is.EqualTo(EventRow.FieldCount));
            Assert.That(fields[0], Is.EqualTo("time"));
            Assert.That(fields[1], Is.EqualTo("seconds"));
            Assert.That(fields[2], Is.EqualTo("group"));
            Assert.That(fields[3], Is.EqualTo("step"));
            Assert.That(fields[4], Is.EqualTo("event"));
            Assert.That(fields[5], Is.EqualTo("name"));
            Assert.That(fields[6], Is.EqualTo("number"));
            Assert.That(fields[7], Is.EqualTo("text"));
        }

        [Test]
        public void ARowHasTheSameEightFieldsInTheSameOrder()
        {
            string[] fields = Row("BLD-ME v BLD-EL", "42", "ran").Line().Split('\t');

            Assert.That(fields.Length, Is.EqualTo(EventRow.FieldCount));
            Assert.That(fields[0], Is.EqualTo("09:14:22.117"));
            Assert.That(fields[1], Is.EqualTo("12.500"));
            Assert.That(fields[2], Is.EqualTo("1B06PH"));
            Assert.That(fields[3], Is.EqualTo(RunSteps.TestsRun));
            Assert.That(fields[4], Is.EqualTo("step finished"));
            Assert.That(fields[5], Is.EqualTo("BLD-ME v BLD-EL"));
            Assert.That(fields[6], Is.EqualTo("42"));
            Assert.That(fields[7], Is.EqualTo("ran"));
        }

        [Test]
        public void AnEmptyFieldIsEmptyAndNotTheWordNull()
        {
            string[] fields = new EventRow("09:00:00.000", 0.0, null, null, "opened", null, null, null)
                .Line().Split('\t');

            Assert.That(fields.Length, Is.EqualTo(EventRow.FieldCount));

            foreach (int at in new[] { 2, 3, 5, 6, 7 })
            {
                Assert.That(fields[at], Is.Empty, "field " + at);
            }
        }

        /// <summary>
        /// The break. A tab inside a value would move every column after it, so it is
        /// escaped, and the row still has exactly eight fields.
        /// </summary>
        [Test]
        public void ATabInsideAValueNeverMovesAColumn()
        {
            string[] fields = Row("left\tright", "1", null).Line().Split('\t');

            Assert.That(fields.Length, Is.EqualTo(EventRow.FieldCount));
            Assert.That(fields[5], Is.EqualTo("left\\tright"));
            Assert.That(EventRow.Unescape(fields[5]), Is.EqualTo("left\tright"));
        }

        /// <summary>
        /// A newline would end the row halfway through, so it is escaped too, and the
        /// row is still one line.
        /// </summary>
        [Test]
        public void ANewlineInsideAValueNeverEndsTheRow()
        {
            string line = Row("first\nsecond", "1", "and\r\nagain").Line();

            Assert.That(line, Does.Not.Contain("\n"));
            Assert.That(line, Does.Not.Contain("\r"));
            Assert.That(line.Split('\t').Length, Is.EqualTo(EventRow.FieldCount));
            Assert.That(EventRow.Unescape(line.Split('\t')[5]), Is.EqualTo("first\nsecond"));
            Assert.That(EventRow.Unescape(line.Split('\t')[7]), Is.EqualTo("and\r\nagain"));
        }

        /// <summary>
        /// A backslash is escaped FIRST, or reading the value back would turn a path that
        /// happened to end in a t into a tab. Every Windows path in this tool holds
        /// backslashes, so this is the ordinary case and not the odd one.
        /// </summary>
        [Test]
        public void ABackslashIsEscapedSoAPathReadsBackAsItself()
        {
            string path = "C:\\C06\\NWF\\1B06PH.nwf";

            Assert.That(EventRow.Unescape(EventRow.Escape(path)), Is.EqualTo(path));

            // The ordering, stated as what it guarantees: a REAL tab and the two
            // characters backslash and t escape differently, so reading a value back
            // cannot turn one into the other. Without the backslash going first they
            // would both come out as "\t" and a path ending in a t would read as a tab.
            Assert.That(EventRow.Escape("\t"), Is.EqualTo("\\t"));
            Assert.That(EventRow.Escape("\\t"), Is.EqualTo("\\\\t"));
            Assert.That(EventRow.Escape("\t"), Is.Not.EqualTo(EventRow.Escape("\\t")));
            Assert.That(EventRow.Unescape(EventRow.Escape("\t")), Is.EqualTo("\t"));
            Assert.That(EventRow.Unescape(EventRow.Escape("\\t")), Is.EqualTo("\\t"));
        }

        [Test]
        public void EscapingAndReadingBackGivesTheSameValueForEverythingAwkward()
        {
            foreach (string value in new[]
            {
                "plain", "", "\t", "\n", "\r\n", "\\", "\\t", "\\\\", "a\tb\nc\\d",
                "Mechanical / Mechanical-HVAC / BLD-ME Air Terminals ",
                "1104-PAR_CLASH_AllInOne (2) (1).xml"
            })
            {
                Assert.That(EventRow.Unescape(EventRow.Escape(value)), Is.EqualTo(value), value);
            }
        }

        [Test]
        public void TheSecondsAlwaysCarryThreeDecimalsAndNoSeparator()
        {
            string[] fields = new EventRow("09:00:00.000", 1234.5, null, null, "x", null, null, null)
                .Line().Split('\t');

            Assert.That(fields[1], Is.EqualTo("1234.500"));
            Assert.That(fields[1], Does.Not.Contain(","));
        }

        [Test]
        public void ANumberCarriesNoThousandsSeparator()
        {
            Assert.That(EventRow.Count(1830), Is.EqualTo("1830"));
            Assert.That(EventRow.Count(0), Is.EqualTo("0"));
            Assert.That(EventRow.Exact(742.1255), Is.EqualTo("742.126"));
            Assert.That(EventRow.Exact(5.0), Is.EqualTo("5"));
        }

        [Test]
        public void TheRowFileSitsBesideTheTextLogWithADifferentExtension()
        {
            string log = TestPaths.At("logs", "run-20260919-091422.log");
            string rows = RowLog.PathFor(log);

            Assert.That(rows, Does.EndWith("run-20260919-091422" + EventRow.FileNameExtension));
            Assert.That(
                System.IO.Path.GetDirectoryName(rows),
                Is.EqualTo(System.IO.Path.GetDirectoryName(log)));
        }

        [Test]
        public void WithNoTextLogThereIsNoRowFileAndNoThrow()
        {
            Assert.That(RowLog.PathFor(null), Is.Null);
            Assert.That(RowLog.PathFor(string.Empty), Is.Null);
        }
    }
}
