using System;
using System.Collections.Generic;
using Federator.Core.Grouping;
using NUnit.Framework;

namespace Federator.Core.Tests
{
    /// <summary>
    /// F130, Bader's request 5 under Q112. A click on one Run box then a Shift click on another
    /// gives every row between them the first one's state. Each test breaks one input, the
    /// Shift, the anchor, the order the grid shows or the row clicked, and asserts which rows
    /// the rule hands back and the state it hands with them.
    /// </summary>
    [TestFixture]
    public class ShiftRangeTests
    {
        private sealed class Row
        {
            public Row(string name)
            {
                Name = name;
            }

            public string Name { get; private set; }
        }

        private static readonly Row A = new Row("A");
        private static readonly Row B = new Row("B");
        private static readonly Row C = new Row("C");
        private static readonly Row D = new Row("D");
        private static readonly Row E = new Row("E");

        private static IList<Row> InOrder()
        {
            return new List<Row> { A, B, C, D, E };
        }

        private static string Names(RowsToSet<Row> set)
        {
            List<string> names = new List<string>();

            foreach (Row row in set.Rows)
            {
                names.Add(row.Name);
            }

            return string.Join(" ", names.ToArray());
        }

        [Test]
        public void APlainClickSetsOnlyTheRowClickedToTheStateTheClickLeft()
        {
            RowsToSet<Row> set = ShiftRange.Of(InOrder(), B, false, D, true, false);

            Assert.That(Names(set), Is.EqualTo("D"));
            Assert.That(set.State, Is.True);
        }

        [Test]
        public void AShiftClickDownGivesEveryRowFromTheAnchorToTheClickTheAnchorsState()
        {
            RowsToSet<Row> set = ShiftRange.Of(InOrder(), B, false, E, true, true);

            Assert.That(Names(set), Is.EqualTo("B C D E"), "both ends included, in the shown order");
            Assert.That(set.State, Is.False, "the anchor's state, not the state the Shift click left its own box in");
        }

        [Test]
        public void AShiftClickUpGivesTheSameRangeInTheShownOrder()
        {
            RowsToSet<Row> set = ShiftRange.Of(InOrder(), D, true, A, false, true);

            Assert.That(Names(set), Is.EqualTo("A B C D"));
            Assert.That(set.State, Is.True);
        }

        /// <summary>
        /// A sorted grid shows the rows in another order from the list behind it, and the range
        /// is what the person sees between the two boxes, so B to C over this order is B E C.
        /// </summary>
        [Test]
        public void ASortedGridRangesOverTheOrderItShows()
        {
            IList<Row> sorted = new List<Row> { D, B, E, C, A };

            RowsToSet<Row> set = ShiftRange.Of(sorted, B, false, C, true, true);

            Assert.That(Names(set), Is.EqualTo("B E C"));
            Assert.That(set.State, Is.False);
        }

        /// <summary>A filtered grid shows fewer rows, and a row it hides is not in the range.</summary>
        [Test]
        public void AFilteredGridLeavesOutTheRowsItHides()
        {
            IList<Row> filtered = new List<Row> { A, C, E };

            RowsToSet<Row> set = ShiftRange.Of(filtered, A, true, E, false, true);

            Assert.That(Names(set), Is.EqualTo("A C E"));
            Assert.That(set.State, Is.True);
        }

        [Test]
        public void AShiftClickWithNoAnchorIsAPlainClick()
        {
            RowsToSet<Row> set = ShiftRange.Of(InOrder(), null, false, C, true, true);

            Assert.That(Names(set), Is.EqualTo("C"));
            Assert.That(set.State, Is.True);
        }

        /// <summary>
        /// The groups made again after a scan, a grouping change or a Use box are new rows, so the
        /// row clicked before is in no list the grid shows and cannot start a range.
        /// </summary>
        [Test]
        public void AShiftClickWhoseAnchorIsNoLongerInTheListIsAPlainClick()
        {
            Row gone = new Row("B");

            RowsToSet<Row> set = ShiftRange.Of(InOrder(), gone, false, D, true, true);

            Assert.That(Names(set), Is.EqualTo("D"));
            Assert.That(set.State, Is.True, "the click's own state, since no anchor gave one");
        }

        [Test]
        public void AShiftClickOnTheAnchorItselfIsAPlainClick()
        {
            RowsToSet<Row> set = ShiftRange.Of(InOrder(), C, false, C, true, true);

            Assert.That(Names(set), Is.EqualTo("C"));
            Assert.That(set.State, Is.True, "the box keeps the state the click left it in");
        }

        [Test]
        public void ARowClickedThatTheGridDoesNotShowIsAPlainClick()
        {
            Row hidden = new Row("F");

            RowsToSet<Row> set = ShiftRange.Of(InOrder(), B, false, hidden, true, true);

            Assert.That(new List<Row>(set.Rows), Is.EqualTo(new[] { hidden }));
            Assert.That(set.State, Is.True);
        }

        [Test]
        public void NoListOrNoRowClickedIsRefusedByName()
        {
            ArgumentNullException noList = Assert.Throws<ArgumentNullException>(
                delegate { ShiftRange.Of<Row>(null, A, true, B, true, true); });
            ArgumentNullException noRow = Assert.Throws<ArgumentNullException>(
                delegate { ShiftRange.Of(InOrder(), A, true, null, true, true); });

            Assert.That(noList.ParamName, Is.EqualTo("shown"));
            Assert.That(noRow.ParamName, Is.EqualTo("clicked"));
        }

        /// <summary>The tick box rule: one grey line of at most twelve words and no capitals for emphasis.</summary>
        [Test]
        public void TheGreyLineIsTwelveWordsAtMostAndNamesShiftAndTheRunBox()
        {
            string line = ShiftRange.HelpLine;

            Assert.That(line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).Length, Is.LessThanOrEqualTo(12));
            Assert.That(line, Does.StartWith("Shift click a Run box"));
            Assert.That(line, Does.Contain("tick or untick"));
            Assert.That(line, Does.Not.Contain("SHIFT"));
            Assert.That(line, Does.Not.Contain(";"));
            Assert.That(line, Does.Not.Contain(((char)0x2014).ToString()), "no em dash, by the writing rule");
        }
    }
}
