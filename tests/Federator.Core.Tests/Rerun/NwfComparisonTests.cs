using System;
using System.Collections.Generic;
using System.IO;
using Federator.Core.Rerun;
using NUnit.Framework;

namespace Federator.Core.Tests
{
    /// <summary>
    /// An NWF holds pointers to the NWC files and the clash results are the only record of
    /// what has been fixed. So a rerun has to work out whether the group still matches
    /// before it touches anything.
    /// </summary>
    [TestFixture]
    public class NwfComparisonTests
    {
        private static string In(string folder, string name)
        {
            return Path.Combine(folder, name);
        }

        private static readonly string Incoming = @"C:\models\incoming";
        private static readonly string Elsewhere = @"C:\models\archive";

        private static string[] Group(params string[] names)
        {
            List<string> paths = new List<string>();

            foreach (string name in names)
            {
                paths.Add(In(Incoming, name));
            }

            return paths.ToArray();
        }

        private const string Ar = "1104-PAR-1C07BC-ZZZ-AR-MOD-000001.nwc";
        private const string St = "1104-PAR-1C07BC-ZZZ-ST-MOD-000001.nwc";
        private const string Me = "1104-PAR-1C07BC-ZZZ-ME-MOD-000001.nwc";
        private const string El = "1104-PAR-1C07BC-ZZZ-EL-MOD-000001.nwc";

        // ---------- case one, no NWF yet ----------

        [Test]
        public void WithNoNwfTheDecisionIsToBuildIt()
        {
            NwfComparison comparison = NwfComparison.NoNwfYet(Group(Ar, St));

            Assert.That(comparison.Decision, Is.EqualTo(RerunDecision.Build));
            Assert.That(comparison.Matches, Is.False);
            Assert.That(comparison.InNwf.Count, Is.EqualTo(0));
            Assert.That(comparison.Scanned.Count, Is.EqualTo(2));
            Assert.That(comparison.Added.Count, Is.EqualTo(2), "everything is new when there is no NWF");
            Assert.That(comparison.Removed.Count, Is.EqualTo(0));
        }

        [Test]
        public void TheBuildLineNamesThePathAndTheCount()
        {
            IList<string> lines = NwfComparison.NoNwfYet(Group(Ar, St)).Lines(@"C:\out\x.nwf");

            Assert.That(lines[0], Does.StartWith("BUILD"));
            Assert.That(lines[0], Does.Contain(@"C:\out\x.nwf"));
            Assert.That(lines[0], Does.Contain("2 files"));
        }

        // ---------- case two, the list matches ----------

        [Test]
        public void AMatchingListIsOpenedAndNothingIsTouched()
        {
            NwfComparison comparison = NwfComparison.Compare(
                Group(Ar, St, Me, El),
                Group(Ar, St, Me, El));

            Assert.That(comparison.Decision, Is.EqualTo(RerunDecision.Open));
            Assert.That(comparison.Matches, Is.True);
            Assert.That(comparison.Added.Count, Is.EqualTo(0));
            Assert.That(comparison.Removed.Count, Is.EqualTo(0));
            Assert.That(comparison.Unchanged.Count, Is.EqualTo(4));
        }

        [Test]
        public void TheOrderTheFilesAreInDoesNotMatter()
        {
            NwfComparison comparison = NwfComparison.Compare(
                Group(El, Me, St, Ar),
                Group(Ar, St, Me, El));

            Assert.That(comparison.Decision, Is.EqualTo(RerunDecision.Open));
        }

        [Test]
        public void APathThatDiffersOnlyInCaseStillMatches()
        {
            NwfComparison comparison = NwfComparison.Compare(
                new[] { @"C:\MODELS\INCOMING\" + Ar.ToUpperInvariant() },
                new[] { In(Incoming, Ar) });

            Assert.That(comparison.Decision, Is.EqualTo(RerunDecision.Open),
                "Windows does not care about case and neither should this");
        }

        [Test]
        public void ARelativePathAndTheSameAbsolutePathMatch()
        {
            string absolute = Path.Combine(Directory.GetCurrentDirectory(), "a.nwc");

            NwfComparison comparison = NwfComparison.Compare(new[] { "a.nwc" }, new[] { absolute });

            Assert.That(comparison.Decision, Is.EqualTo(RerunDecision.Open));
        }

        [Test]
        public void TheOpenedLineNamesThePathAndSaysNothingWasTouched()
        {
            IList<string> lines = NwfComparison
                .Compare(Group(Ar, St), Group(Ar, St))
                .Lines(@"C:\out\1104-PAR-1C07BC-ZZZ-BM-MOD-000001.nwf");

            Assert.That(lines[0], Does.StartWith("OPENED"));
            Assert.That(lines[0], Does.Contain(@"C:\out\1104-PAR-1C07BC-ZZZ-BM-MOD-000001.nwf"));
            Assert.That(lines[1], Does.Contain("not cleared"));
            Assert.That(lines[1], Does.Contain("nothing was re-appended"));
        }

        // ---------- case three, the list differs ----------

        [Test]
        public void AFileAddedSinceLastTimeIsReportedAndNothingIsTouched()
        {
            NwfComparison comparison = NwfComparison.Compare(
                Group(Ar, St),
                Group(Ar, St, Me));

            Assert.That(comparison.Decision, Is.EqualTo(RerunDecision.Changed));
            Assert.That(comparison.Added, Is.EqualTo(new[] { In(Incoming, Me) }));
            Assert.That(comparison.Removed.Count, Is.EqualTo(0));
            Assert.That(comparison.Unchanged.Count, Is.EqualTo(2));
        }

        [Test]
        public void AFileRemovedSinceLastTimeIsReported()
        {
            NwfComparison comparison = NwfComparison.Compare(
                Group(Ar, St, Me),
                Group(Ar, St));

            Assert.That(comparison.Decision, Is.EqualTo(RerunDecision.Changed));
            Assert.That(comparison.Added.Count, Is.EqualTo(0));
            Assert.That(comparison.Removed, Is.EqualTo(new[] { In(Incoming, Me) }));
        }

        // The case the brief asked for by name.
        [Test]
        public void OneFileAddedAndOneRemovedAtTheSameTimeNamesBoth()
        {
            NwfComparison comparison = NwfComparison.Compare(
                Group(Ar, St, Me),
                Group(Ar, St, El));

            Assert.That(comparison.Decision, Is.EqualTo(RerunDecision.Changed));
            Assert.That(comparison.Added, Is.EqualTo(new[] { In(Incoming, El) }));
            Assert.That(comparison.Removed, Is.EqualTo(new[] { In(Incoming, Me) }));
            Assert.That(comparison.Unchanged.Count, Is.EqualTo(2));
            Assert.That(comparison.Moved.Count, Is.EqualTo(0), "different names, so not a move");
        }

        [Test]
        // F24. The comparison names the NWF and the counts and says it is rebuilt. Which
        // file was added, moved or removed is said once, by NwfRebuildPlan, so a move
        // never reads as an addition and a removal here and a move there.
        public void TheChangedLinesNameTheNwfTheCountsAndTheRebuild()
        {
            IList<string> lines = NwfComparison
                .Compare(Group(Ar, St, Me), Group(Ar, St, El))
                .Lines(@"C:\out\x.nwf");

            string all = string.Join(Environment.NewLine, new List<string>(lines).ToArray());

            Assert.That(lines.Count, Is.EqualTo(2), "the heading and the counts, no per file line here");
            Assert.That(all, Does.Contain("CHANGED"));
            Assert.That(all, Does.Not.Contain("left exactly as it is"));
            Assert.That(all, Does.Contain("2 unchanged, 1 added, 1 removed, so it is rebuilt from the scan folder"));
        }

        [Test]
        public void AFileThatKeptItsNameAndChangedFolderIsCalledOutAsAMove()
        {
            NwfComparison comparison = NwfComparison.Compare(
                new[] { In(Incoming, Ar) },
                new[] { In(Elsewhere, Ar) });

            Assert.That(comparison.Decision, Is.EqualTo(RerunDecision.Changed));
            Assert.That(comparison.Added.Count, Is.EqualTo(1));
            Assert.That(comparison.Removed.Count, Is.EqualTo(1));
            Assert.That(comparison.Moved, Is.EqualTo(new[] { Ar }));
        }

        [Test]
        public void EverythingReplacedIsStillJustChanged()
        {
            NwfComparison comparison = NwfComparison.Compare(Group(Ar, St), Group(Me, El));

            Assert.That(comparison.Decision, Is.EqualTo(RerunDecision.Changed));
            Assert.That(comparison.Added.Count, Is.EqualTo(2));
            Assert.That(comparison.Removed.Count, Is.EqualTo(2));
            Assert.That(comparison.Unchanged.Count, Is.EqualTo(0));
        }

        // ---------- edges ----------

        [Test]
        public void AnEmptyNwfAgainstAnEmptyScanMatches()
        {
            NwfComparison comparison = NwfComparison.Compare(new string[0], new string[0]);

            Assert.That(comparison.Decision, Is.EqualTo(RerunDecision.Open),
                "an empty NWF still exists, so there is still history to keep");
        }

        [Test]
        public void AnEmptyNwfAgainstARealGroupIsChangedNotBuild()
        {
            NwfComparison comparison = NwfComparison.Compare(new string[0], Group(Ar));

            Assert.That(comparison.Decision, Is.EqualTo(RerunDecision.Changed));
            Assert.That(comparison.Added.Count, Is.EqualTo(1));
        }

        [Test]
        public void ARepeatedPathIsCountedOnce()
        {
            NwfComparison comparison = NwfComparison.Compare(
                new[] { In(Incoming, Ar), In(Incoming, Ar) },
                new[] { In(Incoming, Ar) });

            Assert.That(comparison.InNwf.Count, Is.EqualTo(1));
            Assert.That(comparison.Decision, Is.EqualTo(RerunDecision.Open));
        }

        [Test]
        public void ABlankPathIsIgnoredRatherThanCountedAsAFile()
        {
            NwfComparison comparison = NwfComparison.Compare(
                new[] { In(Incoming, Ar), null, string.Empty },
                new[] { In(Incoming, Ar) });

            Assert.That(comparison.Decision, Is.EqualTo(RerunDecision.Open));
        }

        [Test]
        public void ANullListIsRefusedRatherThanTreatedAsEmpty()
        {
            Assert.Throws<ArgumentNullException>(delegate { NwfComparison.Compare(null, new string[0]); });
            Assert.Throws<ArgumentNullException>(delegate { NwfComparison.Compare(new string[0], null); });
        }

    }
}
