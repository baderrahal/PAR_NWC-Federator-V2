using Federator.Core.Views;
using NUnit.Framework;

namespace Federator.Core.Tests
{
    /// <summary>
    /// The second reading's F4 of F114's add-in pass. A removal is removed only where its
    /// folder's count fell by exactly one, the one rule for a removal of the inventory and for
    /// the view this run could not mark, so a RemoveAt that returned and changed nothing, or
    /// changed more, is never counted as removed.
    /// </summary>
    [TestFixture]
    public class RemovalOutcomeTests
    {
        private static readonly ViewNode Node = new ViewNode(new[] { "A" }, "Walls", false, 2, null, 0, null, null, false);

        [Test]
        public void ACountThatFellByOneIsRemoved()
        {
            RemovalOutcome outcome = RemovalOutcome.Counted(Node, InventoryDecision.RemoveLegacy, 5, 4);

            Assert.That(outcome.Removed, Is.True);
            Assert.That(outcome.WhyNot, Is.Null);
            Assert.That(RemovalOutcome.CountWords(5, 4), Is.Null);
        }

        [Test]
        public void ACountThatDidNotFallByOneIsNotRemovedAndSaysWhatHappened()
        {
            RemovalOutcome same = RemovalOutcome.Counted(Node, InventoryDecision.RemoveLegacy, 5, 5);
            RemovalOutcome two = RemovalOutcome.Counted(Node, InventoryDecision.RemoveLegacy, 5, 3);
            RemovalOutcome lost = RemovalOutcome.Counted(Node, InventoryDecision.RemoveLegacy, 5, -1);

            Assert.That(same.Removed, Is.False);
            Assert.That(same.WhyNot, Is.EqualTo("RemoveAt returned and its folder went from 5 to 5 children, not one fewer"));
            Assert.That(two.Removed, Is.False);
            Assert.That(two.WhyNot, Is.EqualTo("RemoveAt returned and its folder went from 5 to 3 children, not one fewer"));
            Assert.That(lost.Removed, Is.False);
            Assert.That(lost.WhyNot, Is.EqualTo("RemoveAt returned and its folder was not found again, so whether it fell by one is UNKNOWN"));
        }
    }
}
