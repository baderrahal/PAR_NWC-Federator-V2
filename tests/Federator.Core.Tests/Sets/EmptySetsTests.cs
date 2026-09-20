using System.Collections.Generic;
using Federator.Core.Sets;
using NUnit.Framework;

namespace Federator.Core.Tests.Sets
{
    /// <summary>
    /// 3b of the drift round, and the thing Bader's own report made urgent. His 1A02MM
    /// workbook holds 1,830 clash test blocks, 1,781 of which found zero, and 1,677 of
    /// the tests touch one of the 43 sets that never produce a clash. Nothing in this
    /// tool told him which of those sets is WRONG and which is a model with no such
    /// content, and those are two completely different problems.
    /// </summary>
    [TestFixture]
    public class EmptySetsTests
    {
        private static ReadCondition Category(string value)
        {
            return new ReadCondition("LcRevitData_Element", EmptySets.CategoryProperty, "equals", value);
        }

        private static ReadCondition Workset(string value)
        {
            return new ReadCondition("LcRevitData_Element", EmptySets.WorksetProperty, "equals", value);
        }

        private static string Joined(IList<string> lines)
        {
            return string.Join("\n", new List<string>(lines).ToArray());
        }

        /// <summary>
        /// Bucket one. `ME-DUCTWORK` is what his saved sets ask and no model carries it,
        /// and the nearest the models do carry is `ME-Ductwork`, which is a suggestion
        /// and never a correction.
        /// </summary>
        [Test]
        public void ASetAskingForAValueNoModelCarriesIsNamedWithTheNearestOneThatIs()
        {
            EmptySet why = EmptySets.Why(
                "a/BLD-ME-Ducts", new List<ReadCondition> { Workset("ME-DUCTWORK") });

            Assert.That(why.Reason, Is.EqualTo(EmptyReason.NoModelCarriesTheValue));
            Assert.That(why.Asked, Is.EqualTo("ME-DUCTWORK"));
            Assert.That(why.Nearest, Is.EqualTo("ME-Ductwork"));
            Assert.That(why.Line(), Does.Contain("NO MODEL IN THIS PROJECT CARRIES IT"));
            Assert.That(why.Line(), Does.Contain("a suggestion and not a correction"));
        }

        /// <summary>Bucket two. The value is really there, so the fault is somewhere else and a person looks.</summary>
        [Test]
        public void ASetAskingForAValueTheModelsDoCarrySaysSomethingElseIsWrong()
        {
            EmptySet why = EmptySets.Why(
                "a/BLD-ME-Piping", new List<ReadCondition> { Workset("ME-Piping") });

            Assert.That(why.Reason, Is.EqualTo(EmptyReason.TheValueIsThereAnyway));
            Assert.That(why.Line(), Does.Contain("WHICH MODELS IN THIS PROJECT DO CARRY"));
            Assert.That(why.Line(), Does.Contain("this reader cannot tell which"));

            // THE LIST IS THE WHOLE PROJECT AND NOT THIS GROUP, so the line must not
            // claim something else is wrong. A group holding two disciplines out of seven
            // lands most of the 61 sets here, 33 of 54 on 1000BS, and every one of those
            // is ordinary.
            Assert.That(why.Line(), Does.Contain("holds no model of that kind"));
        }

        /// <summary>
        /// Bucket three. A property nobody measured a list for is one this reader has no
        /// opinion about, and it says so rather than reporting every set as wrong.
        /// </summary>
        [Test]
        public void ASetAskingOnAPropertyWithNoMeasuredListSaysItCannotTell()
        {
            EmptySet why = EmptySets.Why(
                "a/BLD-AR-Source",
                new List<ReadCondition> { new ReadCondition(string.Empty, "LcOaNodeSourceFile", "contains", "-AR-") });

            Assert.That(why.Reason, Is.EqualTo(EmptyReason.CannotTell));
            Assert.That(why.Line(), Does.Contain("CANNOT TELL WHY"));
        }

        [Test]
        public void ASetWithNoConditionsAtAllAlsoSaysItCannotTell()
        {
            Assert.That(
                EmptySets.Why("a/b", new List<ReadCondition>()).Reason,
                Is.EqualTo(EmptyReason.CannotTell));
        }

        /// <summary>
        /// The cost line, which is the number his report made urgent. A set that finds
        /// nothing is not one dead set, it is every clash test that points at it.
        /// </summary>
        [Test]
        public void TheBlockSaysWhatTheEmptySetsCostInClashTests()
        {
            IList<EmptySet> empty = new List<EmptySet>
            {
                EmptySets.Why("a/BLD-ME-Ducts", new List<ReadCondition> { Workset("ME-DUCTWORK") })
            };

            string block = Joined(EmptySets.Lines(empty, 1677, 1830));

            Assert.That(block, Does.Contain("IT COSTS 1677 of this group's 1830 clash tests"));
            Assert.That(block, Does.Contain("can never report a clash"));
        }

        [Test]
        public void TheBlockCountsTheThreeBucketsSeparately()
        {
            IList<EmptySet> empty = new List<EmptySet>
            {
                EmptySets.Why("a", new List<ReadCondition> { Workset("ME-DUCTWORK") }),
                EmptySets.Why("b", new List<ReadCondition> { Workset("ME-Piping") }),
                EmptySets.Why("c", new List<ReadCondition>())
            };

            string block = Joined(EmptySets.Lines(empty, 10, 100));

            Assert.That(block, Does.Contain("3 set(s) found nothing"));
            Assert.That(block, Does.Contain("1 ask for a value NO MODEL IN THIS PROJECT CARRIES"));
            Assert.That(block, Does.Contain("1 ask for a value models in this project DO carry"));
            Assert.That(block, Does.Contain("1 this reader cannot tell about"));
        }

        /// <summary>
        /// The block is ABSENT when no set is empty, rather than written saying none.
        /// A block that appears on every run saying nothing is wrong is one people stop
        /// reading, which costs the runs where something is.
        /// </summary>
        [Test]
        public void TheBlockIsAbsentWhenNoSetIsEmpty()
        {
            Assert.That(EmptySets.Lines(new List<EmptySet>(), 0, 1830), Is.Empty);
            Assert.That(EmptySets.Lines(null, 0, 1830), Is.Empty);
        }

        [Test]
        public void ACostLineWithNoTestCountSaysUnknownRatherThanZero()
        {
            IList<EmptySet> empty = new List<EmptySet>
            {
                EmptySets.Why("a", new List<ReadCondition> { Workset("ME-DUCTWORK") })
            };

            Assert.That(Joined(EmptySets.Lines(empty, 0, 0)), Does.Contain("UNKNOWN"));
        }

        /// <summary>
        /// A category value the models really carry, off the measured 374, so the bucket
        /// rule is proved against the real list and not only against worksets.
        /// </summary>
        [Test]
        public void TheCategoryListIsReadTheSameWayTheWorksetListIs()
        {
            Assert.That(
                EmptySets.Why("a", new List<ReadCondition> { Category("Floors") }).Reason,
                Is.EqualTo(EmptyReason.TheValueIsThereAnyway));

            Assert.That(
                EmptySets.Why("a", new List<ReadCondition> { Category("Nurse Call Devices") }).Reason,
                Is.EqualTo(EmptyReason.NoModelCarriesTheValue),
                "the fallback F87 removed asked for a category no model in this project has");
        }
    }
}
