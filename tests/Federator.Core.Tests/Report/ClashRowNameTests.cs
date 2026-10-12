using Federator.Core.Report;
using Federator.Core.Views;
using NUnit.Framework;

namespace Federator.Core.Tests
{
    /// <summary>
    /// F149, the breaker's point on attempt 1. A row is named off its result's display name, or
    /// clash, or group for a result group, where it has none, and the picture pass and the views
    /// find the result again by that name among its siblings. The two read one rule, so a result
    /// with no display name is found again under the name its row carries.
    /// </summary>
    [TestFixture]
    public class ClashRowNameTests
    {
        [Test]
        public void ARowCarriesItsResultsDisplayName()
        {
            Assert.That(ClashRow.NameOf("Clash12", false), Is.EqualTo("Clash12"));
            Assert.That(ClashRow.NameOf("New Group", true), Is.EqualTo("New Group"));
        }

        [Test]
        public void AResultWithNoDisplayNameIsNamedClash()
        {
            Assert.That(ClashRow.NameOf(null, false), Is.EqualTo("clash"));
            Assert.That(ClashRow.NameOf(string.Empty, false), Is.EqualTo("clash"));
        }

        [Test]
        public void AGroupWithNoDisplayNameIsNamedGroup()
        {
            Assert.That(ClashRow.NameOf(null, true), Is.EqualTo("group"));
            Assert.That(ClashRow.NameOf(string.Empty, true), Is.EqualTo("group"));
        }

        // The break. The row is named by the rule and its siblings read back raw, as attempt 1 did,
        // and no sibling carries the name the row was given, so its picture was never taken.
        [Test]
        public void AResultWithNoDisplayNameIsFoundAgainOnlyWhereItsSiblingsAreReadByTheSameRule()
        {
            string[] displayNames = { "Clash1", null, "Clash3" };
            string rowName = ClashRow.NameOf(displayNames[1], false);

            SiblingPick byTheRule = ResultSiblings.Pick(rowName, 1, displayNames.Length, i => ClashRow.NameOf(displayNames[i], false));
            SiblingPick raw = ResultSiblings.Pick(rowName, 1, displayNames.Length, i => displayNames[i] ?? string.Empty);

            Assert.That(byTheRule.Found, Is.True);
            Assert.That(byTheRule.Index, Is.EqualTo(1));
            Assert.That(byTheRule.AtRecorded, Is.True);
            Assert.That(raw.Found, Is.False);
            Assert.That(raw.Carrying, Is.EqualTo(0), "read raw, no sibling carries the name the row was given");
        }
    }
}
