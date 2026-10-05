using System.Collections.Generic;
using Federator.Core.Exchange;
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

        /// <summary>Judged against the names inside Core alone, which is a file read with no list beside it.</summary>
        private static EmptySet Why(string path, List<ReadCondition> asked)
        {
            return EmptySets.Why(path, asked, RevitWorksets.With(null));
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
            EmptySet why = Why(
                "a/BLD-ME-Ducts", new List<ReadCondition> { Workset("ME-DUCTWORK") });

            Assert.That(why.Reason, Is.EqualTo(EmptyReason.NoModelCarriesTheValue));
            Assert.That(why.Asked, Is.EqualTo("ME-DUCTWORK"));
            Assert.That(why.Nearest, Is.EqualTo("ME-Ductwork"));
            Assert.That(why.Line(), Does.Contain("NO MODEL IN THIS PROJECT CARRIES IT"));
            Assert.That(why.Line(), Does.Contain("a suggestion and not a correction"));
        }

        /// <summary>
        /// The judge knows the spellings it is handed, the names inside Core and those of the list
        /// beside the picked XML, RevitWorksets.With, the ones the corrections asked, F116 on the
        /// Q113 pass. ME-DUCTWORK is a spelling of this project's list and not of Core, so with the
        /// list models in this project carry it and with Core's names alone none does.
        /// </summary>
        [Test]
        public void ASpellingTheListBesideThePickedFileHoldsIsOneTheModelsCarry()
        {
            EmptySet withTheList = EmptySets.Why(
                "a/BLD-ME-Ducts", new List<ReadCondition> { Workset("ME-DUCTWORK") }, RevitWorksets.With(new[] { "ME-DUCTWORK" }));

            Assert.That(withTheList.Reason, Is.EqualTo(EmptyReason.TheValueIsThereAnyway), withTheList.Line());
            Assert.That(
                EmptySets.Why("a/BLD-ME-Ducts", new List<ReadCondition> { Workset("ME-DUCTWORK") }, RevitWorksets.With(null)).Reason,
                Is.EqualTo(EmptyReason.NoModelCarriesTheValue));
        }

        /// <summary>Bucket two. The value is really there, so the fault is somewhere else and a person looks.</summary>
        [Test]
        public void ASetAskingForAValueTheModelsDoCarrySaysSomethingElseIsWrong()
        {
            EmptySet why = Why(
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
            EmptySet why = Why(
                "a/BLD-AR-Source",
                new List<ReadCondition> { new ReadCondition(string.Empty, "LcOaNodeSourceFile", "contains", "-AR-") });

            Assert.That(why.Reason, Is.EqualTo(EmptyReason.CannotTell));
            Assert.That(why.Line(), Does.Contain("CANNOT TELL WHY"));
        }

        [Test]
        public void ASetWithNoConditionsAtAllAlsoSaysItCannotTell()
        {
            Assert.That(
                Why("a/b", new List<ReadCondition>()).Reason,
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
                Why("a/BLD-ME-Ducts", new List<ReadCondition> { Workset("ME-DUCTWORK") })
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
                Why("a", new List<ReadCondition> { Workset("ME-DUCTWORK") }),
                Why("b", new List<ReadCondition> { Workset("ME-Piping") }),
                Why("c", new List<ReadCondition>())
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
                Why("a", new List<ReadCondition> { Workset("ME-DUCTWORK") })
            };

            Assert.That(Joined(EmptySets.Lines(empty, 0, 0)), Does.Contain("UNKNOWN"));
        }

        /// <summary>
        /// A CONTAINS CONDITION IS A STEM AND NOT A WHOLE VALUE, FR-010. The corrected matrix asks
        /// contains Cable Tray, Conduit and Devices, and the models carry Cable Trays, Conduits and
        /// five Devices categories. Compared as equals, each was called a value NO MODEL IN THIS
        /// PROJECT CARRIES, where the HEALTH block, by the same list, says they are carried.
        /// </summary>
        [Test]
        public void AContainsConditionIsJudgedByThePartOfANameItAsksFor()
        {
            foreach (string stem in new[] { "Cable Tray", "Conduit", "Devices" })
            {
                EmptySet why = Why("a/BLD-EL-" + stem, new List<ReadCondition>
                {
                    new ReadCondition("LcRevitData_Element", EmptySets.CategoryProperty, "contains", stem)
                });

                Assert.That(why.Reason, Is.EqualTo(EmptyReason.TheValueIsThereAnyway), stem);
            }

            Assert.That(
                Why("a", new List<ReadCondition> { new ReadCondition("LcRevitData_Element", EmptySets.CategoryProperty, "contains", "Nurse Call") }).Reason,
                Is.EqualTo(EmptyReason.NoModelCarriesTheValue),
                "a stem no measured category holds is still carried by no model");
        }

        /// <summary>
        /// A NEGATED CONDITION ASKS FOR EVERYTHING BUT ITS VALUE, FR-010 with FR-023's rule. A
        /// negation of a category no model carries leaves out nothing and stops nothing, 5g, so
        /// BLD-EL-Devices is never judged on the Telephone Devices it leaves out.
        /// </summary>
        [Test]
        public void ANegatedConditionIsNotJudgedAsAValueTheSetAsksFor()
        {
            EmptySet why = Why("a/BLD-EL-Devices", new List<ReadCondition>
            {
                new ReadCondition("LcRevitData_Element", EmptySets.CategoryProperty, "contains", "Devices"),
                new ReadCondition("LcRevitData_Element", EmptySets.CategoryProperty, "equals", "Telephone Devices", PlannedCondition.NegateFlag)
            });

            Assert.That(why.Reason, Is.EqualTo(EmptyReason.TheValueIsThereAnyway));
            Assert.That(why.Asked, Is.EqualTo("Devices"));
        }

        /// <summary>A comparison the file never writes is one this reader cannot judge, never read as equals.</summary>
        [Test]
        public void AnotherComparisonIsNotJudged()
        {
            Assert.That(
                Why("a", new List<ReadCondition> { new ReadCondition("LcRevitData_Element", EmptySets.CategoryProperty, "NotEqual", "Nurse Call Devices") }).Reason,
                Is.EqualTo(EmptyReason.CannotTell));
        }

        /// <summary>
        /// A category value the models really carry, off the measured 374, so the bucket
        /// rule is proved against the real list and not only against worksets.
        /// </summary>
        [Test]
        public void TheCategoryListIsReadTheSameWayTheWorksetListIs()
        {
            Assert.That(
                Why("a", new List<ReadCondition> { Category("Floors") }).Reason,
                Is.EqualTo(EmptyReason.TheValueIsThereAnyway));

            Assert.That(
                Why("a", new List<ReadCondition> { Category("Nurse Call Devices") }).Reason,
                Is.EqualTo(EmptyReason.NoModelCarriesTheValue),
                "the fallback F87 removed asked for a category no model in this project has");
        }
    }
}
