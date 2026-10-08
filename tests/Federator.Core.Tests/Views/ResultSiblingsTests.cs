using System;
using Federator.Core.Views;
using NUnit.Framework;

namespace Federator.Core.Tests
{
    /// <summary>
    /// F132 attempt 2 of the add-in half, the reviewer's blocking finding on it. The compact
    /// after the mirror merge removes every Resolved result before the views run, so a live
    /// result after a Resolved sibling sits one index left of where the harvest recorded it.
    /// The rule of which sibling a recorded row resolves to: the one at the recorded index
    /// where it carries the row's name, else the one sibling carrying that name, and nothing
    /// where none or more than one does. The names in here are sample data.
    /// </summary>
    [TestFixture]
    public class ResultSiblingsTests
    {
        private static SiblingPick Pick(string wanted, int recordedIndex, params string[] names)
        {
            return ResultSiblings.Pick(wanted, recordedIndex, names.Length, i => names[i]);
        }

        [Test]
        public void TheResultAtTheRecordedIndexIsTakenWhereItCarriesTheRowsName()
        {
            SiblingPick pick = Pick("Clash2", 1, "Clash1", "Clash2", "Clash3");

            Assert.That(pick.Found, Is.True);
            Assert.That(pick.Index, Is.EqualTo(1));
            Assert.That(pick.AtRecorded, Is.True);
            Assert.That(pick.Carrying, Is.EqualTo(1));
        }

        /// <summary>Clash2 was Resolved and compacted away, so Clash3 now sits where Clash2 was recorded.</summary>
        [Test]
        public void AResultMovedLeftByTheCompactIsFoundByItsNameAmongTheSiblings()
        {
            SiblingPick pick = Pick("Clash3", 2, "Clash1", "Clash3", "Clash4");

            Assert.That(pick.Found, Is.True);
            Assert.That(pick.Index, Is.EqualTo(1));
            Assert.That(pick.AtRecorded, Is.False);
            Assert.That(pick.Carrying, Is.EqualTo(1));
        }

        [Test]
        public void ARecordedIndexBeyondTheSiblingsIsFoundByNameToo()
        {
            SiblingPick pick = Pick("Clash4", 3, "Clash1", "Clash4");

            Assert.That(pick.Found, Is.True);
            Assert.That(pick.Index, Is.EqualTo(1));
            Assert.That(pick.AtRecorded, Is.False);
        }

        [Test]
        public void TwoSiblingsCarryingTheNameRefuseAndSayHowMany()
        {
            SiblingPick pick = Pick("Clash2", 0, "Clash1", "Clash2", "Clash2");

            Assert.That(pick.Found, Is.False);
            Assert.That(pick.Index, Is.EqualTo(-1));
            Assert.That(pick.Carrying, Is.EqualTo(2));
        }

        [Test]
        public void NoSiblingCarryingTheNameRefusesWithNought()
        {
            SiblingPick pick = Pick("Clash9", 0, "Clash1", "Clash2");

            Assert.That(pick.Found, Is.False);
            Assert.That(pick.Carrying, Is.EqualTo(0));
        }

        /// <summary>The names are compared whole and Ordinal, so a trailing space keeps two names apart.</summary>
        [Test]
        public void TheNameIsComparedOrdinalAndWhole()
        {
            Assert.That(Pick("Clash1 ", 0, "Clash1", "Clash1 ").Index, Is.EqualTo(1));
            Assert.That(Pick("clash1", 0, "Clash1").Found, Is.False);
        }

        /// <summary>A row with no name can be nobody's, so it is refused before any sibling is read.</summary>
        [Test]
        public void ARowWithNoNameIsRefusedWithoutReadingASibling()
        {
            int read = 0;
            SiblingPick pick = ResultSiblings.Pick(string.Empty, 0, 2, i => { read++; return string.Empty; });

            Assert.That(pick.Found, Is.False);
            Assert.That(pick.Carrying, Is.EqualTo(0));
            Assert.That(read, Is.EqualTo(0));
        }

        [Test]
        public void TheSiblingsAreReadOnceEachAndOnlyWhenTheRecordedIndexDoesNotCarryTheName()
        {
            int read = 0;
            string[] names = { "Clash1", "Clash2", "Clash3" };
            Func<int, string> nameAt = i => { read++; return names[i]; };

            ResultSiblings.Pick("Clash2", 1, 3, nameAt);
            Assert.That(read, Is.EqualTo(1));

            read = 0;
            ResultSiblings.Pick("Clash3", 1, 3, nameAt);
            Assert.That(read, Is.EqualTo(4));
        }
    }
}
