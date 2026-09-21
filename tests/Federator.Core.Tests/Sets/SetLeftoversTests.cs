using System.Collections.Generic;
using Federator.Core.Sets;
using NUnit.Framework;

namespace Federator.Core.Tests.Sets
{
    /// <summary>
    /// Q74, and the measurement that inverted it. 5z counted 60 clash test sides pointing
    /// at the BROKEN `BLD-DRPipe Accessories` and NOTHING pointing at the corrected
    /// `BLD-DR-Pipe Accessories`, in each of seven groups. So removing what the file no
    /// longer names would have orphaned 420 sides and removing the unused twin costs
    /// nothing. 5z-b then proved a rename keeps every side.
    /// </summary>
    [TestFixture]
    public class SetLeftoversTests
    {
        private const string Asks = "LcRevitData_Element|LcRevitPropertyElementCategory|equals|Pipe Accessories";
        private const string AsksSomethingElse = "LcRevitData_Element|LcRevitPropertyElementCategory|equals|Ducts";

        private static DocumentSet Set(string name, int sides, params string[] keys)
        {
            return new DocumentSet("a/folder/" + name, name, new List<string>(keys), sides);
        }

        private static string Joined(IList<string> lines)
        {
            return string.Join("\n", new List<string>(lines).ToArray());
        }

        [Test]
        public void ASetTheFileNamesIsLeftAloneEntirely()
        {
            IList<LeftoverSet> leftovers = SetLeftovers.For(
                new List<DocumentSet> { Set("BLD-DR-Pipe Accessories", 60, Asks) },
                new List<string> { "BLD-DR-Pipe Accessories" });

            Assert.That(leftovers, Is.Empty);
        }

        [Test]
        public void ALeftoverNothingPointsAtIsRemoved()
        {
            IList<LeftoverSet> leftovers = SetLeftovers.For(
                new List<DocumentSet> { Set("BLD-Old-Thing", 0, Asks) },
                new List<string> { "BLD-Something-Else" });

            Assert.That(leftovers.Count, Is.EqualTo(1));
            Assert.That(leftovers[0].Action, Is.EqualTo(LeftoverAction.Remove));
            Assert.That(leftovers[0].Line(), Does.Contain("NOTHING points at it"));
        }

        /// <summary>
        /// THE WHOLE CASE, in his own numbers. The broken name carries the 60 sides, the
        /// corrected name is in the file and carries none, and the two ask the identical
        /// question.
        /// </summary>
        [Test]
        public void TheWorkingHalfOfAPairIsRenamedAndTheUnusedHalfIsRemoved()
        {
            IList<LeftoverSet> leftovers = SetLeftovers.For(
                new List<DocumentSet>
                {
                    Set("BLD-DRPipe Accessories", 60, Asks),
                    Set("BLD-DR-Pipe Accessories", 0, Asks)
                },
                new List<string> { "BLD-DR-Pipe Accessories" });

            Assert.That(leftovers.Count, Is.EqualTo(1), "only the broken name is a leftover");
            Assert.That(leftovers[0].Name, Is.EqualTo("BLD-DRPipe Accessories"));
            Assert.That(leftovers[0].Sides, Is.EqualTo(60));
            Assert.That(leftovers[0].Action, Is.EqualTo(LeftoverAction.RemoveTheTwinThenRename));
            Assert.That(leftovers[0].TwinName, Is.EqualTo("BLD-DR-Pipe Accessories"));
            Assert.That(leftovers[0].Line(), Does.Contain("60 clash test side(s) point at it"));
        }

        /// <summary>
        /// REFUSE WHEN ANYTHING POINTING AT IT WOULD STOP RESOLVING. Wider than the brief's
        /// "loses its results", which 5z showed would never have caught this: the results
        /// survive a removal and the SIDE is what stops resolving.
        /// </summary>
        [Test]
        public void ALeftoverWithSidesAndNoTwinIsRefusedAndNamed()
        {
            IList<LeftoverSet> leftovers = SetLeftovers.For(
                new List<DocumentSet> { Set("BLD-Orphan", 17, Asks) },
                new List<string> { "BLD-Something-Else" });

            Assert.That(leftovers[0].Action, Is.EqualTo(LeftoverAction.Refuse));
            Assert.That(leftovers[0].Line(), Does.Contain("17 clash test side(s) point at it"));
            Assert.That(leftovers[0].Line(), Does.Contain("NOTHING IS DONE"));
            Assert.That(leftovers[0].Line(), Does.Contain("could never find anything again"));
        }

        /// <summary>A twin asking a DIFFERENT question is a different set that happens to sit nearby.</summary>
        [Test]
        public void ATwinAskingADifferentQuestionIsNotATwin()
        {
            IList<LeftoverSet> leftovers = SetLeftovers.For(
                new List<DocumentSet>
                {
                    Set("BLD-Broken", 60, Asks),
                    Set("BLD-Corrected", 0, AsksSomethingElse)
                },
                new List<string> { "BLD-Corrected" });

            Assert.That(leftovers[0].Action, Is.EqualTo(LeftoverAction.Refuse));
        }

        /// <summary>
        /// A twin SOMETHING POINTS AT is not a twin either, because removing it would
        /// orphan those sides instead. Swapping which set is broken changes nothing.
        /// </summary>
        [Test]
        public void ATwinThatSomethingPointsAtIsNotATwin()
        {
            IList<LeftoverSet> leftovers = SetLeftovers.For(
                new List<DocumentSet>
                {
                    Set("BLD-Broken", 60, Asks),
                    Set("BLD-Corrected", 5, Asks)
                },
                new List<string> { "BLD-Corrected" });

            Assert.That(leftovers[0].Action, Is.EqualTo(LeftoverAction.Refuse));
        }

        /// <summary>The conditions are compared IN ORDER, which is the rule SetDrift already keeps.</summary>
        [Test]
        public void TheSameConditionsInADifferentOrderAreNotTheSameQuestion()
        {
            IList<LeftoverSet> leftovers = SetLeftovers.For(
                new List<DocumentSet>
                {
                    Set("BLD-Broken", 60, Asks, AsksSomethingElse),
                    Set("BLD-Corrected", 0, AsksSomethingElse, Asks)
                },
                new List<string> { "BLD-Corrected" });

            Assert.That(leftovers[0].Action, Is.EqualTo(LeftoverAction.Refuse));
        }

        /// <summary>A set asking nothing at all pairs with nothing, or every unreadable set would pair.</summary>
        [Test]
        public void ASetAskingNothingNeverPairs()
        {
            IList<LeftoverSet> leftovers = SetLeftovers.For(
                new List<DocumentSet> { Set("BLD-Broken", 3), Set("BLD-Corrected", 0) },
                new List<string> { "BLD-Corrected" });

            Assert.That(leftovers[0].Action, Is.EqualTo(LeftoverAction.Refuse));
        }

        /// <summary>
        /// RUNNING TWICE OVER THE SAME NWF CHANGES NOTHING THE SECOND TIME. After the
        /// rename the document holds one set, carrying the name the file asks for, so
        /// there is no leftover at all.
        /// </summary>
        [Test]
        public void AfterTheRenameASecondRunFindsNothingToDo()
        {
            IList<LeftoverSet> leftovers = SetLeftovers.For(
                new List<DocumentSet> { Set("BLD-DR-Pipe Accessories", 60, Asks) },
                new List<string> { "BLD-DR-Pipe Accessories" });

            Assert.That(leftovers, Is.Empty);
        }

        [Test]
        public void TheBlockCountsTheThreeActionsAndIsAbsentWhenThereAreNone()
        {
            IList<LeftoverSet> leftovers = SetLeftovers.For(
                new List<DocumentSet>
                {
                    Set("BLD-Broken", 60, Asks),
                    Set("BLD-Corrected", 0, Asks),
                    Set("BLD-Unused", 0, AsksSomethingElse),
                    Set("BLD-Orphan", 4, "something|nobody|else|asks")
                },
                new List<string> { "BLD-Corrected" });

            string block = Joined(SetLeftovers.Lines(leftovers));

            Assert.That(block, Does.Contain("3 set(s) in this NWF are not named by the picked file"));
            Assert.That(block, Does.Contain("1 nothing points at"));
            Assert.That(block, Does.Contain("1 are the working half of a pair"));
            Assert.That(block, Does.Contain("1 are pointed at with no twin"));

            Assert.That(SetLeftovers.Lines(new List<LeftoverSet>()), Is.Empty);
            Assert.That(SetLeftovers.Lines(null), Is.Empty);
        }

        /// <summary>Names are Ordinal and never trimmed, because two set names in the reference file end in a space.</summary>
        [Test]
        public void ANameDifferingOnlyByCaseOrASpaceIsADifferentSet()
        {
            IList<LeftoverSet> leftovers = SetLeftovers.For(
                new List<DocumentSet> { Set("BLD-Thing ", 0, Asks) },
                new List<string> { "BLD-Thing" });

            Assert.That(leftovers.Count, Is.EqualTo(1), "the trailing space makes it a different name");
        }
    }
}
