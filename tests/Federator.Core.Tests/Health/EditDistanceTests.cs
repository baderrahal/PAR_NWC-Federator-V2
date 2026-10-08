using System;
using Federator.Core.Health;
using NUnit.Framework;

namespace Federator.Core.Tests
{
    /// <summary>
    /// T1-N89. The edit distance was written twice, line for line, in the EMPTY SETS judge and in the
    /// workset disagreements, so a fix to one would not reach the other. It is one routine now, and the
    /// two readers each hand it their own cap.
    /// </summary>
    [TestFixture]
    public class EditDistanceTests
    {
        [Test]
        public void TheSameWordIsNoEditsAway()
        {
            Assert.That(EditDistance.Between("ductwork", "ductwork", 2), Is.EqualTo(0));
        }

        [Test]
        public void ATypoIsOneEditAndAWordWithTwoLettersChangedIsTwo()
        {
            Assert.That(EditDistance.Between("el-lightining protection", "el-lightning protection", 2), Is.EqualTo(1));
            Assert.That(EditDistance.Between("ar-exterior", "ar-interior", 2), Is.EqualTo(2));
        }

        [Test]
        public void ALengthGapPastTheCapIsGivenUpOnAndNeverCounted()
        {
            Assert.That(EditDistance.Between("ab", "abcde", 2), Is.EqualTo(int.MaxValue));
            Assert.That(EditDistance.Between(string.Empty, "abc", 2), Is.EqualTo(int.MaxValue));
        }

        [Test]
        public void TheCapIsTheCallersAndNotAConstantOfTheRoutine()
        {
            Assert.That(EditDistance.Between("ab", "abcde", 3), Is.EqualTo(3));
            Assert.That(EditDistance.Between("abc", "xyz", 0), Is.EqualTo(3));
        }
    }
}
