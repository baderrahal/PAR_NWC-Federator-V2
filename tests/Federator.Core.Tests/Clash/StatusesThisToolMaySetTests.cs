using System;
using System.Collections.Generic;
using Federator.Core.Clash;
using NUnit.Framework;

namespace Federator.Core.Tests
{
    /// <summary>
    /// Which statuses this tool may set on a clash.
    ///
    /// Reviewed and nothing else. The tests that matter here are the refusals, because
    /// setting Approved or Resolved would have this tool make a statement about a building
    /// that nobody made, into the only file that records what has been fixed.
    /// </summary>
    [TestFixture]
    public class StatusesThisToolMaySetTests
    {
        [Test]
        public void ReviewedIsTheOneStatusThisToolSets()
        {
            Assert.That(StatusesThisToolMaySet.Allows(ClashStatus.Reviewed), Is.True);
            Assert.That(StatusesThisToolMaySet.All(), Is.EqualTo(new[] { ClashStatus.Reviewed }).AsCollection);
            Assert.That(StatusesThisToolMaySet.WhyNot(ClashStatus.Reviewed), Is.Null);
        }

        /// <summary>
        /// The break that matters. Approved says somebody with the authority to accept a
        /// clash accepted it, and Resolved says the clash is gone from the model. Both are
        /// a person's statement about work that was actually done.
        /// </summary>
        [Test]
        public void ApprovedAndResolvedAreNeverSetByThisTool()
        {
            Assert.That(StatusesThisToolMaySet.Allows(ClashStatus.Approved), Is.False);
            Assert.That(StatusesThisToolMaySet.Allows(ClashStatus.Resolved), Is.False);

            Assert.That(
                StatusesThisToolMaySet.WhyNot(ClashStatus.Approved),
                Does.Contain("work that was actually done"));

            Assert.That(
                StatusesThisToolMaySet.WhyNot(ClashStatus.Resolved),
                Does.Contain("work that was actually done"));
        }

        [Test]
        public void NewAndActiveAreNotSetEither()
        {
            Assert.That(StatusesThisToolMaySet.Allows(ClashStatus.New), Is.False);
            Assert.That(StatusesThisToolMaySet.Allows(ClashStatus.Active), Is.False);

            Assert.That(
                StatusesThisToolMaySet.WhyNot(ClashStatus.New),
                Does.Contain("running a test produces it"));
        }

        /// <summary>
        /// Every refusal names WHICH status was asked for. A line saying only that
        /// something was refused sends the reader back to the code.
        /// </summary>
        [Test]
        public void EveryRefusalNamesTheStatusItRefused()
        {
            foreach (ClashStatus status in new[]
                { ClashStatus.New, ClashStatus.Active, ClashStatus.Approved, ClashStatus.Resolved })
            {
                Assert.That(
                    StatusesThisToolMaySet.WhyNot(status),
                    Does.Contain(status.ToString()),
                    status.ToString());
            }
        }

        [Test]
        public void AStatusThisCoreDoesNotRecogniseIsRefused()
        {
            Assert.That(StatusesThisToolMaySet.Allows((ClashStatus)99), Is.False);
            Assert.That(StatusesThisToolMaySet.WhyNot((ClashStatus)99), Is.Not.Null);
        }
    }

    /// <summary>
    /// The words that stop a clash status and a test status being confused for each other.
    /// They are different sets on different things and they share the word New, which is
    /// how it happens.
    /// </summary>
    [TestFixture]
    public class StatusWordsTests
    {
        private static string All()
        {
            return string.Join("\n", new List<string>(StatusWords.Lines()).ToArray());
        }

        [Test]
        public void TheFiveClashStatusesAreNamed()
        {
            string said = All();

            foreach (string status in new[] { "New", "Active", "Reviewed", "Approved", "Resolved" })
            {
                Assert.That(said, Does.Contain(status), status);
            }
        }

        [Test]
        public void TheFourTestStatusesAreNamedAndCalledATestsAndNotAClashs()
        {
            string said = All();

            Assert.That(said, Does.Contain("a TEST carries one of four: New, Old, Partial, Complete"));
        }

        /// <summary>
        /// The one this block exists for. Old is a test word. No clash is ever at Old, so
        /// nothing in this tool ever moves a clash from it, and saying that out loud is
        /// what stops somebody adding a rule that tries.
        /// </summary>
        [Test]
        public void OldIsSaidToBeATestWordAndNeverAClashWord()
        {
            string said = All();

            Assert.That(said, Does.Contain("Old is a test word and never a clash word"));
            Assert.That(said, Does.Contain("no clash is ever at Old"));
        }

        [Test]
        public void ItSaysReviewedIsTheOnlyOneThisToolSets()
        {
            Assert.That(All(), Does.Contain("sets Reviewed and nothing else"));
        }

        [Test]
        public void EveryLineCarriesTheStatusPrefixSoTheBlockReadsAsOneThing()
        {
            foreach (string line in StatusWords.Lines())
            {
                Assert.That(line, Does.StartWith("STATUS   "), line);
            }
        }
    }
}
