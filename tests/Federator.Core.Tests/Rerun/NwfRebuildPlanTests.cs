using System;
using System.Collections.Generic;
using System.IO;
using Federator.Core.Rerun;
using NUnit.Framework;

namespace Federator.Core.Tests
{
    /// <summary>
    /// F24. A CHANGED NWF is rebuilt from the scan, keeping its saved tests. The sample is
    /// 1B06BC from the run log of 2026-09-07: the NWF held four files from the old Native
    /// Model folder, the scan found five under the Published folder, so four moved, one
    /// was added, none was removed.
    /// </summary>
    [TestFixture]
    public class NwfRebuildPlanTests
    {
        // Built with Path.Combine so the same names split the same way on Windows, where
        // the run happens, and on Linux, where the container runs the tests.
        private static readonly string OldFolder = Path.Combine(Path.GetTempPath(), "01_WIP", "Native Model", "NWC", "C06");
        private static readonly string NewFolder = Path.Combine(Path.GetTempPath(), "04_Published", "C06");

        private const string Ar = "1104-PAR-1B06BC-ZZZ-AR-MOD-000001.nwc";
        private const string El = "1104-PAR-1B06BC-ZZZ-EL-MOD-000001.nwc";
        private const string Me = "1104-PAR-1B06BC-ZZZ-ME-MOD-000001.nwc";
        private const string St1 = "1104-PAR-1B06BC-ZZZ-ST-MOD-000001.nwc";
        private const string St2 = "1104-PAR-1B06BC-ZZZ-ST-MOD-000002.nwc";

        private static string Old(string name)
        {
            return Path.Combine(OldFolder, name);
        }

        private static string New(string name)
        {
            return Path.Combine(NewFolder, name);
        }

        private static string Join(IEnumerable<string> lines)
        {
            return string.Join(Environment.NewLine, new List<string>(lines).ToArray());
        }

        // ---------- the 1B06BC case from the log ----------

        [Test]
        public void TheLogCaseIsFourMovesOneAdditionAndNoRemoval()
        {
            NwfComparison comparison = NwfComparison.Compare(
                new[] { Old(Ar), Old(Me), Old(St1), Old(St2) },
                new[] { New(Ar), New(El), New(Me), New(St1), New(St2) });

            NwfRebuildPlan plan = NwfRebuildPlan.From(comparison);

            Assert.That(plan.Rebuild, Is.True);
            Assert.That(plan.Moved.Count, Is.EqualTo(4), "four files kept their name and changed folder");
            Assert.That(plan.Added, Is.EqualTo(new[] { New(El) }), "EL was never in the NWF");
            Assert.That(plan.Removed, Is.Empty, "nothing the NWF held is gone from the scan");
            Assert.That(plan.Summary(), Is.EqualTo("1 added, 4 moved, 0 removed"));
        }

        [Test]
        public void TheFilesToAppendAreTheScanInScanOrder()
        {
            NwfComparison comparison = NwfComparison.Compare(
                new[] { Old(Ar), Old(Me), Old(St1), Old(St2) },
                new[] { New(St2), New(Ar), New(El), New(Me), New(St1) });

            NwfRebuildPlan plan = NwfRebuildPlan.From(comparison);

            Assert.That(plan.Files, Is.EqualTo(new[] { New(St2), New(Ar), New(El), New(Me), New(St1) }),
                "the scan is what was asked for, and its order is kept");
        }

        [Test]
        public void AMoveKnowsWhereItCameFromAndWhereItWent()
        {
            NwfComparison comparison = NwfComparison.Compare(new[] { Old(Ar) }, new[] { New(Ar) });

            NwfRebuildPlan plan = NwfRebuildPlan.From(comparison);

            Assert.That(plan.Moved.Count, Is.EqualTo(1));
            Assert.That(plan.Moved[0].Name, Is.EqualTo(Ar));
            Assert.That(plan.Moved[0].From, Is.EqualTo(Old(Ar)));
            Assert.That(plan.Moved[0].To, Is.EqualTo(New(Ar)));
            Assert.That(plan.Moved[0].FromFolder, Is.EqualTo(OldFolder));
            Assert.That(plan.Moved[0].ToFolder, Is.EqualTo(NewFolder));
        }

        // ---------- pure addition, pure removal ----------

        [Test]
        public void AFileOnlyInTheScanIsAnAddition()
        {
            NwfComparison comparison = NwfComparison.Compare(
                new[] { New(Ar), New(Me) },
                new[] { New(Ar), New(Me), New(El) });

            NwfRebuildPlan plan = NwfRebuildPlan.From(comparison);

            Assert.That(plan.Rebuild, Is.True);
            Assert.That(plan.Added, Is.EqualTo(new[] { New(El) }));
            Assert.That(plan.Moved, Is.Empty);
            Assert.That(plan.Removed, Is.Empty);
            Assert.That(plan.Unchanged, Is.EqualTo(new[] { New(Ar), New(Me) }));
            Assert.That(plan.Summary(), Is.EqualTo("1 added, 0 moved, 0 removed"));
        }

        [Test]
        public void AFileOnlyInTheNwfIsARemoval()
        {
            NwfComparison comparison = NwfComparison.Compare(
                new[] { New(Ar), New(Me), New(El) },
                new[] { New(Ar), New(Me) });

            NwfRebuildPlan plan = NwfRebuildPlan.From(comparison);

            Assert.That(plan.Rebuild, Is.True);
            Assert.That(plan.Added, Is.Empty);
            Assert.That(plan.Moved, Is.Empty);
            Assert.That(plan.Removed, Is.EqualTo(new[] { New(El) }));
            Assert.That(plan.Files, Is.EqualTo(new[] { New(Ar), New(Me) }), "the removed file is not appended");
            Assert.That(plan.Summary(), Is.EqualTo("0 added, 0 moved, 1 removed"));
        }

        [Test]
        public void AMoveAndARemovalOfTheSameNameAreToldApart()
        {
            // Two St files in the NWF, one St file in the scan under the new folder. One
            // is a move and the other is a removal, matched in order, never both moves.
            NwfComparison comparison = NwfComparison.Compare(
                new[] { Old(St1), Path.Combine(OldFolder, "again", St1) },
                new[] { New(St1) });

            NwfRebuildPlan plan = NwfRebuildPlan.From(comparison);

            Assert.That(plan.Moved.Count, Is.EqualTo(1));
            Assert.That(plan.Moved[0].From, Is.EqualTo(Old(St1)));
            Assert.That(plan.Removed, Is.EqualTo(new[] { Path.Combine(OldFolder, "again", St1) }));
        }

        // ---------- no rebuild ----------

        [Test]
        public void AMatchingNwfNeedsNoRebuildAndWritesNoLine()
        {
            NwfComparison comparison = NwfComparison.Compare(new[] { New(Ar), New(Me) }, new[] { New(Ar), New(Me) });

            NwfRebuildPlan plan = NwfRebuildPlan.From(comparison);

            Assert.That(comparison.Decision, Is.EqualTo(RerunDecision.Open));
            Assert.That(plan.Rebuild, Is.False);
            Assert.That(plan.Files, Is.Empty);
            Assert.That(plan.Lines("x.nwf"), Is.Empty);
        }

        [Test]
        public void NoNwfYetNeedsNoRebuildEither()
        {
            NwfRebuildPlan plan = NwfRebuildPlan.From(NwfComparison.NoNwfYet(new[] { New(Ar) }));

            Assert.That(plan.Rebuild, Is.False);
            Assert.That(plan.Lines("x.nwf"), Is.Empty);
        }

        [Test]
        public void NoComparisonIsRefusedRatherThanPlanned()
        {
            Assert.Throws<ArgumentNullException>(delegate { NwfRebuildPlan.From(null); });
        }

        // ---------- the log lines ----------

        [Test]
        public void TheLinesAreOneHeadingThenOneLinePerFile()
        {
            NwfComparison comparison = NwfComparison.Compare(
                new[] { Old(Ar), Old(Me), Old(St1), Old(St2) },
                new[] { New(Ar), New(El), New(Me), New(St1), New(St2) });

            IList<string> lines = NwfRebuildPlan.From(comparison).Lines("x.nwf");

            Assert.That(lines[0], Is.EqualTo("REBUILT  x.nwf  1 added, 4 moved, 0 removed"));
            Assert.That(lines.Count, Is.EqualTo(6), "the heading, one added, four moved");
            Assert.That(lines[1], Is.EqualTo("         added   " + New(El)));
            Assert.That(lines[2], Is.EqualTo("         moved   " + Ar + "  from " + OldFolder + "  to " + NewFolder));
            Assert.That(Join(lines), Does.Not.Contain("removed "), "nothing was removed, so no removed line");
        }

        [Test]
        public void ARemovedFileGetsItsOwnLine()
        {
            NwfComparison comparison = NwfComparison.Compare(new[] { New(Ar), New(El) }, new[] { New(Ar) });

            IList<string> lines = NwfRebuildPlan.From(comparison).Lines("x.nwf");

            Assert.That(lines, Is.EqualTo(new[]
            {
                "REBUILT  x.nwf  0 added, 0 moved, 1 removed",
                "         removed " + New(El),
            }));
        }

        // ---------- the saved tests line ----------

        [Test]
        public void TestsStillThereAfterTheClearSaySoAndPutNothingBack()
        {
            string line = NwfRebuildPlan.SavedTestsLine(1830, 1830, 1830);

            Assert.That(line, Does.StartWith("         saved tests kept: 1830 read before the clear"));
            Assert.That(line, Does.Contain("1830 still in the document after it"));
            Assert.That(line, Does.Contain("nothing to put back"));
            Assert.That(NwfRebuildPlan.SavedTestsKept(1830, 1830), Is.True);
        }

        [Test]
        public void TestsDroppedByTheClearAndPutBackAreKept()
        {
            string line = NwfRebuildPlan.SavedTestsLine(1830, 0, 1830);

            Assert.That(line, Does.StartWith("         saved tests kept: 1830 read before the clear"));
            Assert.That(line, Does.Contain("0 left after it"));
            Assert.That(line, Does.Contain("1830 put back from the copy"));
            Assert.That(NwfRebuildPlan.SavedTestsKept(1830, 1830), Is.True);
        }

        [Test]
        public void TestsThatCouldNotBePutBackAreLostAndSayTheNwfWasNotSavedOver()
        {
            string line = NwfRebuildPlan.SavedTestsLine(1830, 0, 0);

            Assert.That(line, Does.StartWith("         saved tests LOST: 1830 read before the clear"));
            Assert.That(line, Does.Contain("NOT saved over"));
            Assert.That(NwfRebuildPlan.SavedTestsKept(1830, 0), Is.False);
            Assert.That(NwfRebuildPlan.SavedTestsKept(1830, 1829), Is.False, "one short is still lost");
        }

        [Test]
        public void AnNwfWithNoTestHasNothingToKeep()
        {
            Assert.That(NwfRebuildPlan.SavedTestsLine(0, 0, 0), Does.Contain("none, the NWF held no clash test"));
            Assert.That(NwfRebuildPlan.SavedTestsKept(0, 0), Is.True);
        }
        // ---------- the sets line, F29 ----------

        [Test]
        public void TheSetsLineCarriesTheThreeCountsInOrder()
        {
            Assert.That(NwfRebuildPlan.SetsLine(61, 61, 61),
                Is.EqualTo("SETS     before clear 61, after appends 61, after restore 61"));
            Assert.That(NwfRebuildPlan.SetsLine(61, 0, 61),
                Is.EqualTo("SETS     before clear 61, after appends 0, after restore 61"));
        }

        [Test]
        public void SetsThatDidNotComeBackAreLostAndTheLineSaysTheNwfWasNotSavedOver()
        {
            string line = NwfRebuildPlan.SetsLine(61, 0, 0);

            Assert.That(line, Does.StartWith("SETS     before clear 61, after appends 0, after restore 0"));
            Assert.That(line, Does.Contain("LOST"));
            Assert.That(line, Does.Contain("NOT saved over"));
            Assert.That(NwfRebuildPlan.SetsKept(61, 0), Is.False);
            Assert.That(NwfRebuildPlan.SetsKept(61, 60), Is.False, "one short is still lost");
            Assert.That(NwfRebuildPlan.SetsKept(61, 61), Is.True);
        }

        [Test]
        public void TheSetsArePutBackOnAnyDropWhetherOrNotTheTestsDropped()
        {
            Assert.That(NwfRebuildPlan.SetsNeedRestoring(61, 0), Is.True);
            Assert.That(NwfRebuildPlan.SetsNeedRestoring(61, 60), Is.True);
            Assert.That(NwfRebuildPlan.SetsNeedRestoring(61, 61), Is.False);
        }

        [Test]
        public void AnNwfWithNoSetHasNothingToKeepOrRestore()
        {
            Assert.That(NwfRebuildPlan.SetsKept(0, 0), Is.True);
            Assert.That(NwfRebuildPlan.SetsNeedRestoring(0, 0), Is.False);
            Assert.That(NwfRebuildPlan.SetsLine(0, 0, 0), Is.EqualTo("SETS     before clear 0, after appends 0, after restore 0"));
        }
    }
}
