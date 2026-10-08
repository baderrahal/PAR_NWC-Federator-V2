using Federator.Core.Diagnostics;
using NUnit.Framework;

namespace Federator.Core.Tests
{
    [TestFixture]
    public class WordsTests
    {
        [Test]
        public void AValueIsItself()
        {
            Assert.That(Words.Or("1C07BC", "UNKNOWN"), Is.EqualTo("1C07BC"));
        }

        [Test]
        public void NullAndEmptyBothTakeTheFallback()
        {
            Assert.That(Words.Or(null, "UNKNOWN"), Is.EqualTo("UNKNOWN"));
            Assert.That(Words.Or(string.Empty, "UNKNOWN"), Is.EqualTo("UNKNOWN"));
        }

        [Test]
        public void WhitespaceIsAValueAndNotAGap()
        {
            // Two set names in the reference file end in a space, so a space is never
            // trimmed into nothing here.
            Assert.That(Words.Or(" ", "UNKNOWN"), Is.EqualTo(" "));
        }

        /// <summary>One rule for a number and its noun, which four files each wrote out. Nought and a thousand take the plural.</summary>
        [Test]
        public void ANumberTakesItsNounInTheSingularOnlyAtOne()
        {
            Assert.That(Words.Counted(1, "test", "tests"), Is.EqualTo("1 test"));
            Assert.That(Words.Counted(0, "test", "tests"), Is.EqualTo("0 tests"));
            Assert.That(Words.Counted(2, "test", "tests"), Is.EqualTo("2 tests"));
            Assert.That(Words.Counted(1000, "test", "tests"), Is.EqualTo("1000 tests"));
            Assert.That(Words.Counted(-1, "test", "tests"), Is.EqualTo("-1 tests"));
        }
    }
}
