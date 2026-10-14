using System;
using System.Collections.Generic;
using System.IO;
using Federator.Core.Clash;
using Federator.Core.Rerun;
using NUnit.Framework;

namespace Federator.Core.Tests
{
    /// <summary>
    /// F187, Bader's order of 14 Oct 2026, task 7. The manifest of the linked NWC files, the backup name of the NWF and
    /// the snapshot of every clash result with its comparison after a refresh. Names are sample data.
    /// </summary>
    [TestFixture]
    public class RefreshTests
    {
        private static readonly DateTime T1 = new DateTime(2026, 10, 14, 8, 0, 0, DateTimeKind.Utc);
        private static readonly DateTime T2 = new DateTime(2026, 10, 14, 9, 30, 0, DateTimeKind.Utc);

        // ---------- the manifest ----------

        private static NwcManifest Last()
        {
            NwcManifest m = new NwcManifest();
            m.Add(new ManifestEntry(@"C:\p\AR.nwc", 1000, T1, "AAAA"));
            m.Add(new ManifestEntry(@"C:\p\ST.nwc", 2000, T1, "BBBB"));
            m.Add(new ManifestEntry(@"C:\p\ME.nwc", 3000, T1, "CCCC"));
            return m;
        }

        [Test]
        public void TheManifestWritesAndReadsBackWhole()
        {
            IList<string> lines = Last().ToLines();

            Assert.That(lines[0], Is.EqualTo("NWC-MANIFEST v1"));
            Assert.That(lines.Count, Is.EqualTo(4));

            NwcManifest read = NwcManifest.Parse(lines);

            Assert.That(read, Is.Not.Null);
            Assert.That(read.Entries.Count, Is.EqualTo(3));
            Assert.That(read.Find(@"c:\P\ar.nwc").Size, Is.EqualTo(1000), "paths are compared without case as Windows does");
            Assert.That(read.Find(@"C:\p\AR.nwc").LastWriteUtc, Is.EqualTo(T1));
            Assert.That(read.Find(@"C:\p\AR.nwc").Sha256, Is.EqualTo("AAAA"));
        }

        [Test]
        public void TheNumbersARunKnewAreKeptInTheManifestForTheNextPage()
        {
            NwcManifest manifest = Last();
            manifest.SetFact("toolviews", 204);
            manifest.SetFact("tests", 741);

            NwcManifest read = NwcManifest.Parse(manifest.ToLines());

            Assert.That(read, Is.Not.Null);
            Assert.That(read.Entries.Count, Is.EqualTo(3));
            Assert.That(read.Fact("toolviews"), Is.EqualTo(204));
            Assert.That(read.Fact("tests"), Is.EqualTo(741));
            Assert.That(read.Fact("nothing"), Is.EqualTo(-1));
        }

        [Test]
        public void AFileThatIsNotAManifestIsNoManifestAndNeverGuessedAt()
        {
            Assert.That(NwcManifest.Parse(null), Is.Null);
            Assert.That(NwcManifest.Parse(new string[0]), Is.Null);
            Assert.That(NwcManifest.Parse(new[] { "something else" }), Is.Null);
            Assert.That(NwcManifest.Parse(new[] { "NWC-MANIFEST v1", "C:\\a.nwc\tnot a number\t2026-10-14T08:00:00.0000000Z\t" }), Is.Null);
        }

        [Test]
        public void WithNoEarlierManifestEveryFileIsFirstSeenAndNoneIsCalledChanged()
        {
            List<ManifestEntry> now = new List<ManifestEntry> { new ManifestEntry(@"C:\p\AR.nwc", 1000, T1, null) };
            ManifestComparison result = NwcManifest.Compare(null, now, p => "X");

            Assert.That(result.HadAnEarlierManifest, Is.False);
            Assert.That(result.FirstSeen.Count, Is.EqualTo(1));
            Assert.That(result.Changed, Is.Empty);
            Assert.That(result.Lines()[0], Does.Contain("no earlier manifest"));
        }

        [Test]
        public void AFileOfADifferentSizeOrTimeWithADifferentHashIsChangedAndTheOthersAreNot()
        {
            int hashed = 0;
            List<ManifestEntry> now = new List<ManifestEntry>
            {
                new ManifestEntry(@"C:\p\AR.nwc", 1000, T1, null),
                new ManifestEntry(@"C:\p\ST.nwc", 2500, T2, null),
                new ManifestEntry(@"C:\p\ME.nwc", 3000, T2, null)
            };

            ManifestComparison result = NwcManifest.Compare(Last(), now, delegate (string path)
            {
                hashed++;
                return path.EndsWith("ST.nwc") ? "NEW" : "CCCC";
            });

            Assert.That(result.Unchanged, Is.EqualTo(new[] { @"C:\p\AR.nwc" }));
            Assert.That(result.Changed, Is.EqualTo(new[] { @"C:\p\ST.nwc" }));
            Assert.That(result.Touched, Is.EqualTo(new[] { @"C:\p\ME.nwc" }), "written again with the same content");
            Assert.That(hashed, Is.EqualTo(2), "the hash is taken only where the size or the time differs");
            Assert.That(result.Lines(), Is.EqualTo(new[] { @"changed  C:\p\ST.nwc" }));
            Assert.That(result.ChangedCount, Is.EqualTo(1));
            Assert.That(result.Next.Find(@"C:\p\ST.nwc").Sha256, Is.EqualTo("NEW"));
            Assert.That(result.Next.Find(@"C:\p\AR.nwc").Sha256, Is.EqualTo("AAAA"), "an unchanged file keeps the hash it had");
        }

        [Test]
        public void AddedAndRemovedFilesAreNamed()
        {
            List<ManifestEntry> now = new List<ManifestEntry>
            {
                new ManifestEntry(@"C:\p\AR.nwc", 1000, T1, null),
                new ManifestEntry(@"C:\p\ST.nwc", 2000, T1, null),
                new ManifestEntry(@"C:\p\EL.nwc", 500, T2, null)
            };

            ManifestComparison result = NwcManifest.Compare(Last(), now, p => null);

            Assert.That(result.Added, Is.EqualTo(new[] { @"C:\p\EL.nwc" }));
            Assert.That(result.Removed, Is.EqualTo(new[] { @"C:\p\ME.nwc" }));
            Assert.That(result.Lines(), Is.EqualTo(new[]
            {
                @"added    C:\p\EL.nwc",
                @"removed  C:\p\ME.nwc  (the NWF no longer links it)"
            }));
        }

        [Test]
        public void AFileWhoseHashCannotBeReadIsChangedBecauseNothingProvesItIsNot()
        {
            List<ManifestEntry> now = new List<ManifestEntry> { new ManifestEntry(@"C:\p\AR.nwc", 1001, T2, null) };

            ManifestComparison result = NwcManifest.Compare(Last(), now, p => null);

            Assert.That(result.Changed, Is.EqualTo(new[] { @"C:\p\AR.nwc" }));
        }

        [Test]
        public void NothingChangedIsSaidSo()
        {
            List<ManifestEntry> now = new List<ManifestEntry>
            {
                new ManifestEntry(@"C:\p\AR.nwc", 1000, T1, null),
                new ManifestEntry(@"C:\p\ST.nwc", 2000, T1, null),
                new ManifestEntry(@"C:\p\ME.nwc", 3000, T1, null)
            };

            Assert.That(NwcManifest.Compare(Last(), now, p => "X").Lines()[0],
                Is.EqualTo("no linked file changed since the last manifest, 3 unchanged"));
        }

        // ---------- the backup ----------

        [Test]
        public void TheBackupFolderIsBesideTheLogFolder()
        {
            Assert.That(NwfBackupName.FolderBeside(@"C:\Users\x\NM fed\Log"), Is.EqualTo(@"C:\Users\x\NM fed\Backup"));
            Assert.That(NwfBackupName.FolderBeside(@"C:\Users\x\NM fed\Log\"), Is.EqualTo(@"C:\Users\x\NM fed\Backup"));
            Assert.That(NwfBackupName.FolderBeside(null), Is.Null);
        }

        [Test]
        public void TheBackupNameCarriesTheMomentAndNeverTakesAnExistingName()
        {
            DateTime now = new DateTime(2026, 10, 14, 19, 5, 9);
            string first = NwfBackupName.PathFor(@"C:\n\NWF\C04\Model.nwf", @"C:\n\Backup", now, p => false);

            Assert.That(first, Is.EqualTo(@"C:\n\Backup\Model 2026-10-14 190509.nwf"));

            HashSet<string> taken = new HashSet<string> { first };
            string second = NwfBackupName.PathFor(@"C:\n\NWF\C04\Model.nwf", @"C:\n\Backup", now, p => taken.Contains(p));

            Assert.That(second, Is.EqualTo(@"C:\n\Backup\Model 2026-10-14 190509 (2).nwf"));
            Assert.That(second, Is.Not.EqualTo(first));

            taken.Add(second);
            Assert.That(NwfBackupName.PathFor(@"C:\n\NWF\C04\Model.nwf", @"C:\n\Backup", now, p => taken.Contains(p)),
                Is.EqualTo(@"C:\n\Backup\Model 2026-10-14 190509 (3).nwf"));
        }

        // ---------- the snapshot and what it keeps ----------

        private static ResultRecord R(string test, string name, string guid, ClashStatus status,
            string assigned = "", string approver = "", string group = "", params string[] comments)
        {
            return new ResultRecord(test, name, guid, status, assigned, comments, approver, group);
        }

        private static ResultSnapshot Before()
        {
            ResultSnapshot s = new ResultSnapshot();
            s.Add(R("T1", "Clash1", "g1", ClashStatus.New));
            s.Add(R("T1", "Clash2", "g2", ClashStatus.Reviewed, "Rami", "", "", "checked on site"));
            s.Add(R("T1", "Clash3", "g3", ClashStatus.Approved, "Lena", "Omar"));
            s.Add(R("T1", "Clash4", "g4", ClashStatus.Active));
            s.Add(R("T2", "Clash1", "g5", ClashStatus.New, "", "", "Group 1"));
            s.AddTest("T3");
            return s;
        }

        [Test]
        public void WhatACoordinatorWroteIsKeptWhenTheClashIsStillThere()
        {
            ResultSnapshot after = new ResultSnapshot();
            after.Add(R("T1", "Clash1", "g1", ClashStatus.Active));
            after.Add(R("T1", "Clash2", "g2", ClashStatus.Reviewed, "Rami", "", "", "checked on site"));
            after.Add(R("T1", "Clash3", "g3", ClashStatus.Approved, "Lena", "Omar"));
            after.Add(R("T1", "Clash4", "g4", ClashStatus.Resolved));
            after.Add(R("T2", "Clash1", "g5", ClashStatus.New, "", "", "Group 1"));
            after.AddTest("T3");

            RefreshComparison result = RefreshComparison.Compare(Before(), after);

            Assert.That(result.MatchedByGuid, Is.EqualTo(5));
            Assert.That(result.MatchedByName, Is.EqualTo(0));

            TestRefresh t1 = result.PerTest[0];
            Assert.That(t1.Test, Is.EqualTo("T1"));
            Assert.That(t1.Before, Is.EqualTo(new[] { 1, 1, 1, 1, 0 }));
            Assert.That(t1.After, Is.EqualTo(new[] { 0, 1, 1, 1, 1 }));
            Assert.That(t1.Kept, Is.EqualTo(4));
            Assert.That(t1.Untouched, Is.EqualTo(4), "no assigned to, comment, approver or group changed on any of the four");
            Assert.That(t1.CoordinatorStatuses, Is.EqualTo(2));
            Assert.That(t1.CoordinatorStatusesKept, Is.EqualTo(2), "the reviewed and the approved one keep their status");
            Assert.That(t1.StatusMoved, Is.EqualTo(2), "a New found again is Active, and Clash4 is Resolved");
            Assert.That(t1.Dropped, Is.EqualTo(1), "Clash4 was Active and is Resolved");
            Assert.That(t1.Changes, Is.Empty, "a status moved by the run is counted and not listed as a thing a coordinator wrote");
        }

        [Test]
        public void AChangedAssigneeApproverGroupOrCommentIsNamedAndNeverLostSilently()
        {
            ResultSnapshot after = new ResultSnapshot();
            after.Add(R("T1", "Clash1", "g1", ClashStatus.New));
            after.Add(R("T1", "Clash2", "g2", ClashStatus.Reviewed, "Someone else", "", "", "checked on site"));
            after.Add(R("T1", "Clash3", "g3", ClashStatus.Approved, "Lena", "Nobody"));
            after.Add(R("T1", "Clash4", "g4", ClashStatus.Active, "", "", "G"));
            after.Add(R("T2", "Clash1", "g5", ClashStatus.New, "", "", "Group 1", "a new note"));

            RefreshComparison result = RefreshComparison.Compare(Before(), after);

            Assert.That(result.PerTest[0].Changes, Is.EqualTo(new[]
            {
                "Clash2 assigned to \"Rami\" to \"Someone else\"",
                "Clash3 approved by \"Omar\" to \"Nobody\"",
                "Clash4 group \"\" to \"G\""
            }));
            Assert.That(result.PerTest[1].Changes, Is.EqualTo(new[] { "Clash1 comments 0 to 1" }));
        }

        [Test]
        public void AClashNoLongerHeldAndANewClashAreCounted()
        {
            ResultSnapshot after = new ResultSnapshot();
            after.Add(R("T1", "Clash2", "g2", ClashStatus.Reviewed, "Rami", "", "", "checked on site"));
            after.Add(R("T1", "Clash3", "g3", ClashStatus.Approved, "Lena", "Omar"));
            after.Add(R("T1", "Clash4", "g4", ClashStatus.Active));
            after.Add(R("T1", "Clash9", "g9", ClashStatus.New));
            after.Add(R("T2", "Clash1", "g5", ClashStatus.New, "", "", "Group 1"));

            RefreshComparison result = RefreshComparison.Compare(Before(), after);
            TestRefresh t1 = result.PerTest[0];

            Assert.That(t1.Gone, Is.EqualTo(1));
            Assert.That(t1.Dropped, Is.EqualTo(1), "Clash1 was New and is not held at all");
            Assert.That(t1.Found, Is.EqualTo(1));
            Assert.That(result.Warnings()[0], Does.StartWith("WARNING  T1: 1 clashes it held as New or Active are no longer found"));
            Assert.That(result.Warnings().Count, Is.EqualTo(1), "only a test that dropped something is named");
        }

        [Test]
        public void AResultWhoseGuidMovedIsFoundAgainByTestAndName()
        {
            ResultSnapshot after = new ResultSnapshot();
            after.Add(R("T1", "Clash1", "newguid", ClashStatus.New));
            after.AddTest("T2");

            ResultSnapshot before = new ResultSnapshot();
            before.Add(R("T1", "Clash1", "oldguid", ClashStatus.New));

            RefreshComparison result = RefreshComparison.Compare(before, after);

            Assert.That(result.MatchedByGuid, Is.EqualTo(0));
            Assert.That(result.MatchedByName, Is.EqualTo(1));
            Assert.That(result.PerTest[0].Untouched, Is.EqualTo(1));
            Assert.That(result.Summary(), Does.Contain("Matched by Guid 0 and by test and name 1"));
        }

        [Test]
        public void TheTableListsOnlyTestsThatMovedWithBeforeAndAfterOfEachStatus()
        {
            ResultSnapshot after = new ResultSnapshot();
            after.Add(R("T1", "Clash1", "g1", ClashStatus.Active));
            after.Add(R("T1", "Clash2", "g2", ClashStatus.Reviewed, "Rami", "", "", "checked on site"));
            after.Add(R("T1", "Clash3", "g3", ClashStatus.Approved, "Lena", "Omar"));
            after.Add(R("T1", "Clash4", "g4", ClashStatus.Active));
            after.Add(R("T2", "Clash1", "g5", ClashStatus.New, "", "", "Group 1"));
            after.AddTest("T3");

            RefreshComparison result = RefreshComparison.Compare(Before(), after);
            IList<string> lines = result.TableLines();

            Assert.That(lines[0], Does.Contain("New, Active, Reviewed, Approved, Resolved"));
            Assert.That(lines.Count, Is.EqualTo(2), "T2 and T3 did not move");
            Assert.That(lines[1], Is.EqualTo("REFRESH  T1  1>0  1>2  1>1  1>1  0>0"));
        }

        [Test]
        public void AnUnchangedRunKeepsEveryClashAsItWasAndNamesNoWarning()
        {
            RefreshComparison result = RefreshComparison.Compare(Before(), Before());

            Assert.That(result.Moved(), Is.Empty);
            Assert.That(result.Warnings(), Is.Empty);
            Assert.That(result.Summary(), Does.StartWith("REFRESH  5 clashes found again. Of 2 Reviewed or Approved before, 2 keep that status. 5 keep every"));
        }

        // ---------- what the refresh never does, read off the add-in's source ----------

        [Test]
        public void TheRefreshNeverRemovesOrReAppendsAModel()
        {
            string path = Path.Combine(Samples.Repo(), "src", "Federator.Addin", "Engine", "RefreshRun.cs");

            Assert.That(File.Exists(path), Is.True, "the refresh route is its own file");

            string source = File.ReadAllText(path);

            Assert.That(source, Does.Not.Contain("TryRemoveFile"));
            Assert.That(source, Does.Not.Contain("RemoveFile("));
            Assert.That(source, Does.Not.Contain("AppendFile"));
            Assert.That(source, Does.Not.Contain("document.Clear("));
            Assert.That(source, Does.Not.Contain("File.Move"));
            Assert.That(source, Does.Not.Contain("File.Delete"));
            Assert.That(source, Does.Contain("document.UpdateFiles()"));
        }

        [Test]
        public void TheBackupIsACopyThatCannotOverwriteAnEarlierOne()
        {
            string path = Path.Combine(Samples.Repo(), "src", "Federator.Addin", "Engine", "RefreshRun.cs");
            string source = File.ReadAllText(path);

            Assert.That(source, Does.Contain("File.Copy(nwfPath, target, false)"), "overwrite is false");
        }
    }
}
