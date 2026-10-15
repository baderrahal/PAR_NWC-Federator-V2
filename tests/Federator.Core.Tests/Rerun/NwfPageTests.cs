using System;
using System.Collections.Generic;
using Federator.Core.Report;
using Federator.Core.Rerun;
using NUnit.Framework;

namespace Federator.Core.Tests
{
    /// <summary>
    /// F188, Bader's answer 3 of Q174 and his order of 14 Oct 2026, task 9. The logic of the two pages that needs no
    /// window: the steps of the NWF page and what each needs, the NWFs last used, the words of the confirmation, and
    /// the NWC page's rule for a group that already has an NWF.
    /// </summary>
    [TestFixture]
    public class NwfPageTests
    {
        // ---------- the steps ----------

        [Test]
        public void EveryStepIsOnWhenThePageOpens()
        {
            NwfPageSteps steps = new NwfPageSteps();

            Assert.That(steps.Refresh && steps.RunTests && steps.Pictures && steps.Viewpoints && steps.Reports, Is.True);
            Assert.That(steps.Words(), Is.EqualTo("Refresh, Run tests, Pictures, Viewpoints, Reports"));
        }

        [Test]
        public void WithRunTestsOffViewpointsReportsAndPicturesCannotRunAndSayWhy()
        {
            NwfPageSteps steps = new NwfPageSteps();
            steps.RunTests = false;
            NwfPageSteps resolved = steps.Resolved();

            Assert.That(resolved.Viewpoints, Is.False);
            Assert.That(resolved.Reports, Is.False);
            Assert.That(resolved.Pictures, Is.False);
            Assert.That(resolved.Refresh, Is.True);
            Assert.That(steps.WhyOff(NwfPageSteps.ViewpointsName), Does.Contain("Run tests is needed"));
            Assert.That(steps.WhyOff(NwfPageSteps.ReportsName), Does.Contain("Run tests is needed"));
            Assert.That(steps.WhyOff(NwfPageSteps.PicturesName), Does.Contain("Reports is needed"));
            Assert.That(steps.WhyOff(NwfPageSteps.RefreshName), Is.Null);
            Assert.That(steps.Words(), Is.EqualTo("Refresh"));
        }

        [Test]
        public void PicturesNeedReportsToLinkThemFrom()
        {
            NwfPageSteps steps = new NwfPageSteps();
            steps.Reports = false;

            Assert.That(steps.Resolved().Pictures, Is.False);
            Assert.That(steps.Resolved().Viewpoints, Is.True, "the viewpoints need the tests and not the reports");
            Assert.That(steps.WhyOff(NwfPageSteps.PicturesName), Does.Contain("Reports is needed"));
        }

        [Test]
        public void TheResolvedStepsAreWhatTheRunsOptionsGet()
        {
            NwfPageSteps steps = new NwfPageSteps();
            steps.Refresh = false;
            steps.Viewpoints = false;
            ReportOptions options = new ReportOptions();

            steps.Apply(options);

            Assert.That(options.RefreshOnOpen, Is.False);
            Assert.That(options.RunTests, Is.True);
            Assert.That(options.MakeViewpoints, Is.False);
            Assert.That(options.WriteWorkbook, Is.True);
            Assert.That(options.WriteHtml, Is.True);
            Assert.That(options.Images.Write, Is.True);

            NwfPageSteps testsOff = new NwfPageSteps();
            testsOff.RunTests = false;
            testsOff.Apply(options);

            Assert.That(options.RunTests, Is.False);
            Assert.That(options.WriteWorkbook, Is.False);
            Assert.That(options.Images.Write, Is.False);
        }

        [Test]
        public void ThePageStartsWithTheRefreshAndTheTestsOnAndTheRebuildOff()
        {
            ReportOptions options = new ReportOptions();

            Assert.That(options.RefreshOnOpen, Is.True);
            Assert.That(options.RunTests, Is.True);
            Assert.That(options.RebuildExisting, Is.False);
        }

        // ---------- the NWFs last used ----------

        [Test]
        public void TheLastNwfUsedIsFirstAndAPathIsHeldOnce()
        {
            RecentNwfs list = new RecentNwfs();
            list.Remember(@"C:\n\A.nwf");
            list.Remember(@"C:\n\B.nwf");
            list.Remember(@"c:\N\a.NWF");

            Assert.That(list.Paths, Is.EqualTo(new[] { @"c:\N\a.NWF", @"C:\n\B.nwf" }));
        }

        [Test]
        public void ItHoldsAtMostTenAndDropsTheOldest()
        {
            RecentNwfs list = new RecentNwfs();

            for (int i = 1; i <= 13; i++)
            {
                list.Remember(@"C:\n\" + i + ".nwf");
            }

            Assert.That(list.Paths.Count, Is.EqualTo(10));
            Assert.That(list.Paths[0], Is.EqualTo(@"C:\n\13.nwf"));
            Assert.That(list.Paths[9], Is.EqualTo(@"C:\n\4.nwf"));
        }

        [Test]
        public void TheFileReadsBackInItsOrderAndIgnoresEmptyLines()
        {
            RecentNwfs list = new RecentNwfs();
            list.Remember(@"C:\n\A.nwf");
            list.Remember(@"C:\n\B.nwf");

            List<string> lines = new List<string>(list.ToLines());
            lines.Add(string.Empty);
            RecentNwfs again = RecentNwfs.Read(lines);

            Assert.That(again.Paths, Is.EqualTo(list.Paths));
            Assert.That(RecentNwfs.Read(null).Paths, Is.Empty);
            list.Remember("   ");
            Assert.That(list.Paths.Count, Is.EqualTo(2));
        }

        // ---------- the confirmation ----------

        private static ManifestComparison OneChanged()
        {
            DateTime t1 = new DateTime(2026, 10, 14, 8, 0, 0, DateTimeKind.Utc);
            DateTime t2 = new DateTime(2026, 10, 14, 9, 0, 0, DateTimeKind.Utc);
            NwcManifest last = new NwcManifest();
            last.Add(new ManifestEntry(@"C:\p\AR.nwc", 10, t1, "AA"));
            last.Add(new ManifestEntry(@"C:\p\ST.nwc", 20, t1, "BB"));

            List<ManifestEntry> now = new List<ManifestEntry>
            {
                new ManifestEntry(@"C:\p\AR.nwc", 10, t1, null),
                new ManifestEntry(@"C:\p\ST.nwc", 25, t2, null)
            };

            return NwcManifest.Compare(last, now, p => "CC");
        }

        [Test]
        public void TheConfirmListsTheChangedFilesTheTestsTheViewpointFolderAndTheOldPictures()
        {
            string text = RefreshConfirm.Text(
                "C04.nwf", @"C:\n\Backup", OneChanged(), "Refresh, Run tests, Pictures, Viewpoints, Reports", 1830, 204, 37, 7904,
                "Clash Status");

            Assert.That(text, Does.Contain("This run refreshes C04.nwf."));
            Assert.That(text, Does.Contain(@"A copy of the NWF is made first in C:\n\Backup."));
            Assert.That(text, Does.Contain(@"    changed  C:\p\ST.nwc"));
            Assert.That(text, Does.Contain("the 1830 of the built in matrix"));
            Assert.That(text, Does.Contain("the folder Clash Status with 204 viewpoints is deleted and made again"));
            Assert.That(text, Does.Contain("A viewpoint moved by hand into that folder is deleted with it."));
            Assert.That(text, Does.Contain("ALL viewpoints and folders outside the tool folder Clash Status and the folder Comments will be deleted"));
            Assert.That(text, Does.Contain("The last run deleted 37 items there"));
            Assert.That(text, Does.Contain("7904 old jpg files"));
            Assert.That(text, Does.EndWith("Nothing is changed before OK. Cancel is the default."));
        }

        [Test]
        public void WhatIsNotKnownIsSaidUnknownAndNeverInvented()
        {
            string text = RefreshConfirm.Text("C04.nwf", null, null, "Refresh", 0, -1, -1, -1, "Clash Status");

            Assert.That(text, Does.Contain("UNKNOWN, no Log folder was found"));
            Assert.That(text, Does.Contain("UNKNOWN. No earlier run of this NWF left a list"));
            Assert.That(text, Does.Contain("with UNKNOWN viewpoints"));
            Assert.That(text, Does.Contain("How many is UNKNOWN until the NWF opens"));
            Assert.That(text, Does.Contain("UNKNOWN old jpg files"));
            Assert.That(text, Does.Contain("the tests saved in the NWF run again where they sit"));
        }

        [Test]
        public void ALongListOfChangedFilesIsCutAndCounted()
        {
            DateTime t1 = new DateTime(2026, 10, 14, 8, 0, 0, DateTimeKind.Utc);
            NwcManifest last = new NwcManifest();
            List<ManifestEntry> now = new List<ManifestEntry>();

            for (int i = 0; i < 20; i++)
            {
                last.Add(new ManifestEntry(@"C:\p\F" + i + ".nwc", 10, t1, "AA"));
                now.Add(new ManifestEntry(@"C:\p\F" + i + ".nwc", 11, t1, null));
            }

            string text = RefreshConfirm.Text("C04.nwf", @"C:\n\Backup", NwcManifest.Compare(last, now, p => "XX"), "Refresh", 1, 1, 1, 1, "T");

            Assert.That(text, Does.Contain("    and 8 more"));
        }

        // ---------- where an NWF's own folders are ----------

        [Test]
        public void AnNwfBuiltByTheNwcPageKeepsItsNwdAndLogFolderForTheNwfPage()
        {
            NwfPlaces places = new NwfPlaces();
            places.Remember(@"C:\NM fed\NWF\C04\G.nwf", @"C:\NM fed\NWD\C04\G.nwd", @"C:\NM fed\Log");

            Assert.That(places.NwdOf(@"c:\nm fed\nwf\c04\g.NWF"), Is.EqualTo(@"C:\NM fed\NWD\C04\G.nwd"));
            Assert.That(places.LogFolderOf(@"C:\NM fed\NWF\C04\G.nwf"), Is.EqualTo(@"C:\NM fed\Log"));
            Assert.That(places.NwdOf(@"C:\other.nwf"), Is.EqualTo(string.Empty), "an NWF the list does not know is run beside its file");
            Assert.That(places.LogFolderOf(@"C:\other.nwf"), Is.EqualTo(string.Empty));
        }

        [Test]
        public void ARunRememberedAgainReplacesItsLineAndTheFileReadsBack()
        {
            NwfPlaces places = new NwfPlaces();
            places.Remember(@"C:\n\G.nwf", @"C:\d\1\G.nwd", @"C:\l1");
            places.Remember(@"C:\n\H.nwf", @"C:\d\H.nwd", @"C:\l2");
            places.Remember(@"c:\N\g.nwf", @"C:\d\2\G.nwd", @"C:\l3");

            Assert.That(places.Rows.Count, Is.EqualTo(2));

            NwfPlaces again = NwfPlaces.Read(places.ToLines());

            Assert.That(again.NwdOf(@"C:\n\G.nwf"), Is.EqualTo(@"C:\d\2\G.nwd"));
            Assert.That(again.LogFolderOf(@"C:\n\H.nwf"), Is.EqualTo(@"C:\l2"));
            Assert.That(NwfPlaces.Read(new[] { "garbage", string.Empty }).Rows, Is.Empty);
            Assert.That(NwfPlaces.Read(null).Rows, Is.Empty);
        }

        // ---------- the NWC page ----------

        [Test]
        public void AGroupWhoseNwfIsThereIsRunFromTheNwfPageUnlessRebuildIsTicked()
        {
            Assert.That(NwcPageRule.CanRun(false, false, false), Is.True);
            Assert.That(NwcPageRule.CanRun(false, true, false), Is.False);
            Assert.That(NwcPageRule.CanRun(false, true, true), Is.True);
            Assert.That(NwcPageRule.CanRun(true, false, true), Is.False, "a group blocked for another reason stays blocked");
        }

        [Test]
        public void TheRebuildConfirmNamesTheGroupsAndSaysItDestroysTheClashHistory()
        {
            string text = NwcPageRule.RebuildConfirm(new[] { "1A04PK", "1A02MM" });

            Assert.That(text, Does.Contain("    1A04PK"));
            Assert.That(text, Does.Contain("    1A02MM"));
            Assert.That(text, Does.Contain("DESTROYS THE CLASH HISTORY"));
            Assert.That(text, Does.Contain("The old NWF is not overwritten"));
            Assert.That(text, Does.EndWith("Cancel is the default."));
        }

        [Test]
        public void ARebuildGoesInADatedFolderNamedByTheMoment()
        {
            Assert.That(RebuildFolder.Name(new DateTime(2026, 10, 14, 18, 26, 5)), Is.EqualTo("run 2026-10-14 1826"));
        }
    }
}
