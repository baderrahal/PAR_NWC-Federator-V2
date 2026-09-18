using System;
using System.Collections.Generic;
using Federator.Core.Rerun;
using NUnit.Framework;

namespace Federator.Core.Tests
{
    /// <summary>
    /// What the NWF carries across a rebuild, counted out and counted back.
    ///
    /// The NWF is the only record of what has been fixed. A rebuild that quietly lost the
    /// clash history would be the most expensive thing this tool could do, so every test
    /// here is about the tally refusing to say kept when it does not know.
    /// </summary>
    [TestFixture]
    public class RebuildTallyTests
    {
        private static RebuiltThing Counted(int before, int afterAppends, int afterRestore)
        {
            RebuildTally tally = new RebuildTally();
            RebuiltThing thing = tally.Count("SETS", "selection sets");

            thing.Before = before;
            thing.AfterAppends = afterAppends;
            thing.AfterRestore = afterRestore;

            return thing;
        }

        [Test]
        public void EverythingBackIsKept()
        {
            RebuiltThing thing = Counted(61, 0, 61);

            Assert.That(thing.Kept, Is.True);
            Assert.That(thing.LostReason(), Is.Null);
            Assert.That(thing.Line(), Does.Contain("kept"));
            Assert.That(thing.Line(), Does.Not.Contain("LOST"));
        }

        [Test]
        public void NothingBeforeTheClearNeedsNothingPuttingBack()
        {
            RebuiltThing thing = Counted(0, 0, 0);

            Assert.That(thing.Kept, Is.True);
            Assert.That(thing.NeedsRestoring, Is.False);
            Assert.That(thing.Line(), Does.Contain("none, the NWF held none before the clear"));
        }

        [Test]
        public void TheClearKeepingThemMeansThereIsNothingToPutBack()
        {
            RebuiltThing thing = Counted(61, 61, 61);

            Assert.That(thing.NeedsRestoring, Is.False);
            Assert.That(thing.Kept, Is.True);
            Assert.That(thing.Line(), Does.Contain("nothing to put back"));
        }

        /// <summary>
        /// The break. One short of what went in is LOST, not kept, however close it is.
        /// A rebuild that put back 1829 of 1830 tests has lost a week of somebody's review
        /// on that one test and there is no second copy of it anywhere.
        /// </summary>
        [Test]
        public void OneShortIsLostAndSaysSo()
        {
            RebuiltThing thing = Counted(1830, 0, 1829);

            Assert.That(thing.Kept, Is.False);
            Assert.That(thing.Line(), Does.Contain("LOST"));
            Assert.That(thing.Line(), Does.Contain("The NWF on disk was NOT saved over"));
            Assert.That(thing.LostReason(), Does.Contain("could not keep the 1830 selection sets"));
        }

        /// <summary>
        /// Never counted is not the same as kept. A rebuild that skipped the count does not
        /// know, and saying UNKNOWN is the difference between a check that ran and one that
        /// did not.
        /// </summary>
        [Test]
        public void NeverCountedIsUnknownAndNotKept()
        {
            RebuildTally tally = new RebuildTally();
            RebuiltThing thing = tally.Count("VIEWS", "saved viewpoints");

            thing.Before = 4;

            Assert.That(thing.WasCounted, Is.False);
            Assert.That(thing.Line(), Does.Contain("NOT COUNTED"));
            Assert.That(thing.Line(), Does.Contain("UNKNOWN"));
            Assert.That(tally.EverythingKept, Is.False);
            Assert.That(thing.LostReason(), Does.Contain("UNKNOWN"));
        }

        [Test]
        public void MoreBackThanWentInIsStillKept()
        {
            RebuiltThing thing = Counted(61, 0, 62);

            Assert.That(thing.Kept, Is.True);
        }

        [Test]
        public void ADropAfterTheAppendsIsWhatAsksForTheCopyBack()
        {
            Assert.That(Counted(61, 0, 61).NeedsRestoring, Is.True);
            Assert.That(Counted(61, 60, 61).NeedsRestoring, Is.True);
            Assert.That(Counted(61, 61, 61).NeedsRestoring, Is.False);
            Assert.That(Counted(0, 0, 0).NeedsRestoring, Is.False);
        }

        /// <summary>
        /// The whole point of F50. Four things, one rule, and any one of them missing holds
        /// the NWF shut.
        /// </summary>
        [Test]
        public void OneThingLostOutOfFourFailsTheWholeRebuild()
        {
            RebuildTally tally = new RebuildTally();

            RebuiltThing sets = tally.Count("SETS", "selection sets");
            RebuiltThing tests = tally.Count("TESTS", "saved clash tests");
            RebuiltThing views = tally.Count("VIEWS", "saved viewpoints");
            RebuiltThing status = tally.Count("RESULTS", "clash results carrying a status a person set");

            foreach (RebuiltThing thing in new[] { sets, tests, views })
            {
                thing.Before = 10;
                thing.AfterAppends = 0;
                thing.AfterRestore = 10;
            }

            status.Before = 10;
            status.AfterAppends = 0;
            status.AfterRestore = 9;

            Assert.That(tally.EverythingKept, Is.False);
            Assert.That(tally.LostReasons().Count, Is.EqualTo(1));
            Assert.That(tally.LostReasons()[0], Does.Contain("clash results carrying a status a person set"));
            Assert.That(tally.Lines().Count, Is.EqualTo(4));
        }

        [Test]
        public void EverythingBackOnAllFourIsTheOnlyWayTheNwfIsSavedOver()
        {
            RebuildTally tally = new RebuildTally();

            foreach (string label in new[] { "SETS", "TESTS", "VIEWS", "RESULTS" })
            {
                RebuiltThing thing = tally.Count(label, label.ToLowerInvariant());
                thing.Before = 3;
                thing.AfterAppends = 0;
                thing.AfterRestore = 3;
            }

            Assert.That(tally.EverythingKept, Is.True);
            Assert.That(tally.LostReasons(), Is.Empty);
        }

        /// <summary>
        /// Four things reading four ways is how a log stops being read, so every line starts
        /// with its label padded to the same width the other blocks use.
        /// </summary>
        [Test]
        public void EveryLineStartsWithItsLabelInTheSameWidth()
        {
            RebuildTally tally = new RebuildTally();

            foreach (string label in new[] { "SETS", "TESTS", "VIEWS", "RESULTS" })
            {
                RebuiltThing thing = tally.Count(label, "things");
                thing.Before = 1;
                thing.AfterAppends = 1;
                thing.AfterRestore = 1;
            }

            foreach (string line in tally.Lines())
            {
                Assert.That(line.Substring(0, 9).TrimEnd(), Is.Not.Empty);
                Assert.That(line[9], Is.Not.EqualTo(' '), line);
            }
        }

        [Test]
        public void AThingWithNoLabelOrNoNameIsRefused()
        {
            RebuildTally tally = new RebuildTally();

            Assert.That(() => tally.Count(null, "a"), Throws.ArgumentException);
            Assert.That(() => tally.Count("A", null), Throws.ArgumentException);
            Assert.That(() => tally.Count(string.Empty, "a"), Throws.ArgumentException);
            Assert.That(tally.Things, Is.Empty);
        }

        [Test]
        public void TheThingsComeBackInTheOrderTheyWereCounted()
        {
            RebuildTally tally = new RebuildTally();

            tally.Count("SETS", "a");
            tally.Count("TESTS", "b");
            tally.Count("VIEWS", "c");

            Assert.That(tally.Things.Count, Is.EqualTo(3));
            Assert.That(tally.Things[0].Label, Is.EqualTo("SETS"));
            Assert.That(tally.Things[1].Label, Is.EqualTo("TESTS"));
            Assert.That(tally.Things[2].Label, Is.EqualTo("VIEWS"));
        }
    }
}
