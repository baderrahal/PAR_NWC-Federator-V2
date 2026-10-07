using Federator.Core.Health;
using NUnit.Framework;

namespace Federator.Core.Tests.Health
{
    /// <summary>
    /// 4e. C04 carries `EL-Fire alarm` in two spellings, one holding a NON-BREAKING SPACE.
    /// Today no group carries both models so nothing compares them, and that stops being
    /// true the moment one group gets both or somebody types the matrix value with an
    /// ordinary space. Then a set finds nothing and every line on screen shows two strings
    /// a person would swear are the same.
    /// </summary>
    [TestFixture]
    public class InvisibleDifferenceTests
    {
        private const string Ordinary = "EL-Fire alarm";
        private const string NoBreak = "EL-Fire alarm";

        /// <summary>The real pair, off C04's own models.</summary>
        [Test]
        public void ANonBreakingSpaceIsNamedWithItsCodePointAndItsPosition()
        {
            string said = InvisibleDifference.Between(Ordinary, NoBreak);

            Assert.That(said, Does.Contain("NON-BREAKING SPACE"));
            Assert.That(said, Does.Contain("U+00A0"));
            Assert.That(said, Does.Contain("AT CHARACTER 8"), "the space is the eighth character");
            Assert.That(said, Does.Contain("an ordinary space"), "and it says what the other side holds");
        }

        [Test]
        public void ATabAgainstASpaceIsNamed()
        {
            string said = InvisibleDifference.Between("a b", "a\tb");

            Assert.That(said, Does.Contain("TAB"));
            Assert.That(said, Does.Contain("U+0009"));
            Assert.That(said, Does.Contain("AT CHARACTER 2"));
        }

        [Test]
        public void AZeroWidthSpaceIsNamed()
        {
            Assert.That(
                InvisibleDifference.Between("ab", "a​b"),
                Does.Contain("ZERO WIDTH SPACE"));
        }

        /// <summary>A trailing space makes the lengths differ, and it is still entirely invisible.</summary>
        [Test]
        public void ATrailingSpaceIsFoundEvenThoughTheLengthsDiffer()
        {
            Assert.That(InvisibleDifference.Between("ME-Piping", "ME-Piping "), Is.Not.Null);
        }

        [Test]
        public void ADoubleSpaceIsFoundEvenThoughTheLengthsDiffer()
        {
            Assert.That(InvisibleDifference.Between("a b", "a  b"), Is.Not.Null);
        }

        /// <summary>
        /// FR-007. An ordinary space is not invisible, but two names that differ only by one were
        /// described as differing in invisible characters, with no character named. The extra space is
        /// named and where it sits, on whichever name holds it.
        /// </summary>
        [Test]
        public void AnOrdinarySpaceThatOneNameHoldsAndTheOtherDoesNotIsNamedWithItsPlace()
        {
            string said = InvisibleDifference.Between("EL-Fire alarm", "EL-Firealarm");

            Assert.That(said, Is.EqualTo("an ordinary space (U+0020) AT CHARACTER 8 of the first"));
            Assert.That(InvisibleDifference.Between("EL-Firealarm", "EL-Fire alarm"),
                Is.EqualTo("an ordinary space (U+0020) AT CHARACTER 8 of the second"));
        }

        [Test]
        public void ALeadingTrailingAndDoubleOrdinarySpaceAreEachNamedWhereTheySit()
        {
            Assert.That(InvisibleDifference.Between(" ME-Piping", "ME-Piping"),
                Is.EqualTo("an ordinary space (U+0020) AT CHARACTER 1 of the first"));
            Assert.That(InvisibleDifference.Between("ME-Piping", "ME-Piping "),
                Is.EqualTo("an ordinary space (U+0020) AT CHARACTER 10 of the second"));
            Assert.That(InvisibleDifference.Between("a b", "a  b"),
                Is.EqualTo("an ordinary space (U+0020) AT CHARACTER 3 of the second"));
        }

        [Test]
        public void NoDifferenceOfLengthEverFallsBackToTheUnnamedSentence()
        {
            foreach (string[] pair in new[]
            {
                new[] { "a b", "ab" }, new[] { "a b", "a\tb" }, new[] { "ab", "a b " }, new[] { "a b c", "abc" }
            })
            {
                Assert.That(InvisibleDifference.Between(pair[0], pair[1]),
                    Does.Not.Contain("they differ only in invisible characters"), pair[0] + " against " + pair[1]);
            }
        }

        /// <summary>
        /// A DIFFERENCE A PERSON CAN SEE IS NOT THIS RULE'S BUSINESS, and it returns null
        /// rather than an empty string, because nothing invisible and an unnamed invisible
        /// thing are different answers.
        /// </summary>
        [Test]
        public void AVisibleDifferenceIsNotReported()
        {
            Assert.That(InvisibleDifference.Between("EL-Lightning", "EL-Lightining"), Is.Null);
            Assert.That(InvisibleDifference.Between("AR-EXTERIOR", "AR-INTERIOR"), Is.Null);
            Assert.That(InvisibleDifference.Between("FP-PIPING", "ME-PIPING"), Is.Null);
        }

        [Test]
        public void TwoIdenticalStringsHaveNoInvisibleDifference()
        {
            Assert.That(InvisibleDifference.Between(Ordinary, Ordinary), Is.Null);
        }

        [Test]
        public void NullIsNeverADifference()
        {
            Assert.That(InvisibleDifference.Between(null, "a"), Is.Null);
            Assert.That(InvisibleDifference.Between("a", null), Is.Null);
        }

        /// <summary>
        /// TWO INVISIBLE DIFFERENCES AT ONCE still name a character, and say WHICH STRING
        /// it is in rather than claiming a position the two share. Naming one character
        /// of two is honest. Claiming a single paired position when there are two would
        /// not be.
        /// </summary>
        [Test]
        public void MoreThanOneDifferingPositionNamesACharacterAndWhichStringItIsIn()
        {
            string said = InvisibleDifference.Between("a b c", "a\tb\tc");

            Assert.That(said, Is.Not.Null);
            Assert.That(said, Does.Contain("TAB"));
            Assert.That(said, Does.Contain("of the second"), "it says which string, not a shared position");
        }

        [Test]
        public void EveryInvisibleCharacterIsRecognised()
        {
            Assert.That(InvisibleDifference.IsInvisible(' '), Is.True);
            Assert.That(InvisibleDifference.IsInvisible('​'), Is.True);
            Assert.That(InvisibleDifference.IsInvisible('‌'), Is.True);
            Assert.That(InvisibleDifference.IsInvisible('‍'), Is.True);
            Assert.That(InvisibleDifference.IsInvisible('﻿'), Is.True);
            Assert.That(InvisibleDifference.IsInvisible('\t'), Is.True);

            Assert.That(InvisibleDifference.IsInvisible(' '), Is.False, "an ordinary space is visible by its width");
            Assert.That(InvisibleDifference.IsInvisible('a'), Is.False);
        }
    }
}
