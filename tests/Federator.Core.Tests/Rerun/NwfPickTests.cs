using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Federator.Core.Diagnostics;
using Federator.Core.Report;
using Federator.Core.Rerun;
using NUnit.Framework;

namespace Federator.Core.Tests
{
    /// <summary>
    /// F129, Bader's request 4 under Q112, FR-178. A third choice on the Source step picks
    /// one NWF or a folder of NWFs, and each runs the way the open file run runs it. What
    /// the choice decides is all Core: which NWFs the pick gives, which of them can run,
    /// what each writes and where, and whether two of them would write the same file.
    /// </summary>
    [TestFixture]
    public class NwfPickTests
    {
        private const string Name = "1104-PAR-1A02MM-ZZZ-BM-MOD-000001";

        /// <summary>
        /// A disk held in memory, so every reason can be proved without folders on the machine
        /// running the tests, and a folder that will not read can be made to throw.
        /// </summary>
        private sealed class FakeDisk
        {
            private readonly HashSet<string> files = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            private readonly HashSet<string> folders = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            private readonly HashSet<string> unreadable = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            private readonly Dictionary<string, string> parentOf = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            /// <summary>
            /// A file, its folder built and its name joined on, so a name holding a character a
            /// path may not is still a file here, as it can be in a listing.
            /// </summary>
            public string File(params string[] parts)
            {
                string folder = TestPaths.At(parts.Take(parts.Length - 1).ToArray());
                string path = folder + Path.DirectorySeparatorChar + parts[parts.Length - 1];
                files.Add(path);
                parentOf[path] = folder;
                Folder(folder);
                return path;
            }

            public string Folder(string path)
            {
                string at = path;

                while (!string.IsNullOrEmpty(at))
                {
                    folders.Add(at);
                    at = Path.GetDirectoryName(at);
                }

                return path;
            }

            public void WillNotRead(string folder)
            {
                unreadable.Add(folder);
            }

            public NwfPickDisk Disk()
            {
                return new NwfPickDisk(
                    path => files.Contains(path),
                    path => folders.Contains(path),
                    FilesIn,
                    FoldersIn);
            }

            private IList<string> FilesIn(string folder)
            {
                Refuse(folder);
                return files.Where(f => string.Equals(parentOf[f], folder, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            private IList<string> FoldersIn(string folder)
            {
                Refuse(folder);
                return folders.Where(f => string.Equals(Path.GetDirectoryName(f), folder, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            private void Refuse(string folder)
            {
                if (unreadable.Contains(folder))
                {
                    throw new UnauthorizedAccessException("Access to the path '" + folder + "' is denied.");
                }
            }
        }

        [Test]
        public void NothingPickedRefusesTheRunAndSaysWhatToPick()
        {
            NwfPickPlan plan = NwfPick.From("  ", false, string.Empty, new FakeDisk().Disk());

            Assert.That(plan.CanStart, Is.False);
            Assert.That(plan.WhyNoRun, Does.Contain("Pick one NWF or a folder of NWFs"));
            Assert.That(plan.Nwfs, Is.Empty);
        }

        [Test]
        public void APickThatIsNotThereIsNamedAndRefused()
        {
            string gone = TestPaths.At("Feds", "gone");
            NwfPickPlan plan = NwfPick.From(gone, false, string.Empty, new FakeDisk().Disk());

            Assert.That(plan.CanStart, Is.False);
            Assert.That(plan.WhyNoRun, Does.Contain(gone));
            Assert.That(plan.WhyNoRun, Does.Contain("no file and no folder"));
        }

        /// <summary>
        /// The outputs are named exactly as the open file run names them, read through the same
        /// members, so an NWF run this way and the same NWF run open write the same files.
        /// </summary>
        [Test]
        public void OnePickedNwfIsOneRunWithItsOutputsNamedAsTheOpenFileRunNamesThem()
        {
            FakeDisk disk = new FakeDisk();
            string nwf = disk.File("Feds", Name + ".nwf");

            NwfPickPlan plan = NwfPick.From(nwf, false, string.Empty, disk.Disk());

            Assert.That(plan.CanStart, Is.True, plan.WhyNoRun);
            Assert.That(plan.PickedAFolder, Is.False);
            Assert.That(plan.Nwfs.Count, Is.EqualTo(1));

            PickedNwf one = plan.Nwfs[0];
            string reports = Path.Combine(TestPaths.At("Feds"), ReportPaths.Subfolder);

            Assert.That(one.Path, Is.EqualTo(nwf));
            Assert.That(one.Name, Is.EqualTo(Name));
            Assert.That(one.NwdPath, Is.EqualTo(TestPaths.At("Feds", Name + ".nwd")));
            Assert.That(one.NwdPath, Is.EqualTo(OpenDocumentJob.NwdBeside(nwf)));
            Assert.That(one.ReportFolder, Is.EqualTo(reports));
            Assert.That(one.ReportFolder, Is.EqualTo(OpenDocumentJob.ReportFolder(nwf, string.Empty).Folder));
            Assert.That(one.WorkbookPath, Is.EqualTo(ReportPaths.Workbook(reports, Name)));
            Assert.That(one.CanRun, Is.True);
            Assert.That(plan.Paths(), Is.EqualTo(new[] { nwf }));
            Assert.That(plan.LogFolder, Is.EqualTo(TestPaths.At("Feds")));
        }

        [Test]
        public void APickedExcelFolderTakesTheReportsAsItDoesForTheOpenFileRun()
        {
            FakeDisk disk = new FakeDisk();
            string nwf = disk.File("Feds", Name + ".nwf");
            string excel = TestPaths.At("Reports");

            PickedNwf one = NwfPick.From(nwf, false, excel, disk.Disk()).Nwfs[0];

            Assert.That(one.ReportFolder, Is.EqualTo(excel));
            Assert.That(one.WorkbookPath, Is.EqualTo(ReportPaths.Workbook(excel, Name)));
        }

        /// <summary>
        /// The guard the open file run keeps against an NWD, an NWC and an unsaved document is
        /// the guard here too, in its words, read through OpenDocumentJob.WhyNot.
        /// </summary>
        [Test]
        public void APickedNwdIsRefusedInTheOpenFileRunsWords()
        {
            FakeDisk disk = new FakeDisk();
            string nwd = disk.File("Feds", Name + ".nwd");

            NwfPickPlan plan = NwfPick.From(nwd, false, string.Empty, disk.Disk());

            Assert.That(plan.CanStart, Is.False);
            Assert.That(plan.Nwfs.Count, Is.EqualTo(1));
            Assert.That(plan.Nwfs[0].CanRun, Is.False);
            Assert.That(plan.Nwfs[0].WhyNot, Is.EqualTo(OpenDocumentJob.WhyNot(nwd, f => true)));
            Assert.That(plan.WhyNoRun, Does.Contain(OpenDocumentJob.WhyNot(nwd, f => true)));
        }

        [Test]
        public void AFolderGivesItsNwfsInNameOrderAndNoOtherFile()
        {
            FakeDisk disk = new FakeDisk();
            string folder = TestPaths.At("Feds");
            string b = disk.File("Feds", "b.nwf");
            string a = disk.File("Feds", "a.NWF");
            disk.File("Feds", "c.nwd");
            disk.File("Feds", "d.nwc");
            disk.File("Feds", "e.nwf.bak");
            disk.File("Feds", "f.nwfx");

            NwfPickPlan plan = NwfPick.From(folder, false, string.Empty, disk.Disk());

            Assert.That(plan.CanStart, Is.True, plan.WhyNoRun);
            Assert.That(plan.PickedAFolder, Is.True);
            Assert.That(plan.Paths(), Is.EqualTo(new[] { a, b }));
            Assert.That(plan.LogFolder, Is.EqualTo(folder));
        }

        [Test]
        public void AFolderIsReadWithNoRecursionUnlessTheSubfoldersBoxAsks()
        {
            FakeDisk disk = new FakeDisk();
            string folder = TestPaths.At("Feds");
            string top = disk.File("Feds", "a.nwf");
            string deep = disk.File("Feds", "Zone 1", "Level 2", "b.nwf");

            Assert.That(NwfPick.From(folder, false, string.Empty, disk.Disk()).Paths(), Is.EqualTo(new[] { top }));
            Assert.That(NwfPick.From(folder, true, string.Empty, disk.Disk()).Paths(), Is.EqualTo(new[] { top, deep }));
        }

        [Test]
        public void AFolderHoldingNoNwfIsRefusedAndNamed()
        {
            FakeDisk disk = new FakeDisk();
            string folder = TestPaths.At("Feds");
            disk.File("Feds", "a.nwd");
            disk.File("Feds", "Zone 1", "b.nwf");

            NwfPickPlan plan = NwfPick.From(folder, false, string.Empty, disk.Disk());

            Assert.That(plan.CanStart, Is.False);
            Assert.That(plan.WhyNoRun, Does.Contain(folder));
            Assert.That(plan.WhyNoRun, Does.Contain("holds no NWF"));
            Assert.That(plan.WhyNoRun, Does.Contain("subfolders were not read"));
        }

        /// <summary>
        /// Two NWFs of one name in two subfolders write two NWDs, each beside its own NWF, and
        /// with no Excel folder their reports go beside each of them too. With an Excel folder
        /// both reports land on one path, and the second would write over the first, so the run
        /// is refused before anything is opened, naming both and the path.
        /// </summary>
        [Test]
        public void TwoNwfsThatWouldWriteTheSameReportAreRefusedBeforeTheRun()
        {
            FakeDisk disk = new FakeDisk();
            string folder = TestPaths.At("Feds");
            string one = disk.File("Feds", "Zone 1", Name + ".nwf");
            string two = disk.File("Feds", "Zone 2", Name + ".nwf");
            string excel = TestPaths.At("Reports");

            NwfPickPlan apart = NwfPick.From(folder, true, string.Empty, disk.Disk());
            Assert.That(apart.CanStart, Is.True, apart.WhyNoRun);

            NwfPickPlan together = NwfPick.From(folder, true, excel, disk.Disk());

            Assert.That(together.CanStart, Is.False);
            Assert.That(together.WhyNoRun, Does.Contain(one));
            Assert.That(together.WhyNoRun, Does.Contain(two));
            Assert.That(together.WhyNoRun, Does.Contain(ReportPaths.Workbook(excel, Name)));
            Assert.That(together.WhyNoRun, Does.Contain("write over"));
        }

        /// <summary>The NWD names are compared the same way, read case blind as Windows reads a name.</summary>
        [Test]
        public void TwoNwfsThatWouldWriteTheSameNwdAreRefusedBeforeTheRun()
        {
            FakeDisk disk = new FakeDisk();
            string one = TestPaths.At("Feds", "a.nwf");
            string two = TestPaths.At("Feds", "A.nwf");
            NwfPickDisk both = new NwfPickDisk(
                path => false,
                path => true,
                folder => new List<string> { one, two },
                folder => new List<string>());

            NwfPickPlan plan = NwfPick.From(TestPaths.At("Feds"), false, string.Empty, both);

            Assert.That(plan.CanStart, Is.False);
            Assert.That(plan.WhyNoRun, Does.Contain(OpenDocumentJob.NwdBeside(one)).IgnoreCase);
        }

        [Test]
        public void ASubfolderThatWillNotReadIsNamedAndTheRestStillRun()
        {
            FakeDisk disk = new FakeDisk();
            string folder = TestPaths.At("Feds");
            string top = disk.File("Feds", "a.nwf");
            disk.File("Feds", "Locked", "b.nwf");
            string locked = TestPaths.At("Feds", "Locked");
            disk.WillNotRead(locked);

            NwfPickPlan plan = NwfPick.From(folder, true, string.Empty, disk.Disk());

            Assert.That(plan.CanStart, Is.True, plan.WhyNoRun);
            Assert.That(plan.Paths(), Is.EqualTo(new[] { top }));
            Assert.That(plan.Problems.Count, Is.EqualTo(1));
            Assert.That(plan.Problems[0], Does.Contain(locked));
            Assert.That(plan.Problems[0], Does.Contain("is denied"));
            Assert.That(plan.SettingsLines(), Has.Some.Contains(locked));
        }

        [Test]
        public void APickedFolderThatWillNotReadRefusesTheRunAndSaysWhy()
        {
            FakeDisk disk = new FakeDisk();
            string folder = disk.Folder(TestPaths.At("Feds"));
            disk.WillNotRead(folder);

            NwfPickPlan plan = NwfPick.From(folder, false, string.Empty, disk.Disk());

            Assert.That(plan.CanStart, Is.False);
            Assert.That(plan.WhyNoRun, Does.Contain(folder));
            Assert.That(plan.WhyNoRun, Does.Contain("is denied"));
        }

        /// <summary>
        /// An NWF the open file run's checks refuse is named with its reason and never opened,
        /// and the rest of the folder runs. It still goes to the engine, which judges it by the
        /// same checks before any open and counts it a failed group, so RESULT carries it.
        /// </summary>
        [Test]
        public void AnNwfTheChecksRefuseIsNamedWithItsReasonAndTheRestRun()
        {
            FakeDisk disk = new FakeDisk();
            string folder = TestPaths.At("Feds");
            string good = disk.File("Feds", "a.nwf");
            string bad = disk.File("Feds", "b|c.nwf");

            NwfPickPlan plan = NwfPick.From(folder, false, string.Empty, disk.Disk());

            Assert.That(plan.CanStart, Is.True, plan.WhyNoRun);
            Assert.That(plan.Runnable, Is.EqualTo(1));
            Assert.That(plan.Refused, Is.EqualTo(1));
            Assert.That(plan.Paths(), Is.EqualTo(new[] { good, bad }));

            PickedNwf refused = plan.Nwfs.Single(n => n.Path == bad);
            Assert.That(refused.CanRun, Is.False);
            Assert.That(refused.WhyNot, Does.Contain("which Windows does not allow in a path"));

            string settings = string.Join("\n", plan.SettingsLines().ToArray());
            Assert.That(settings, Does.Contain(bad));
            Assert.That(settings, Does.Contain("which Windows does not allow in a path"));
        }

        [Test]
        public void AFolderWhereTheChecksRefuseEveryNwfRefusesTheRun()
        {
            FakeDisk disk = new FakeDisk();
            string folder = TestPaths.At("Feds");
            disk.File("Feds", "b|c.nwf");
            disk.File("Feds", "d|e.nwf");

            NwfPickPlan plan = NwfPick.From(folder, false, string.Empty, disk.Disk());

            Assert.That(plan.CanStart, Is.False);
            Assert.That(plan.WhyNoRun, Does.Contain("holds 2 NWFs and none of them can run"));
        }

        [Test]
        public void TheSettingsLinesSayWhatWasPickedAndWhatEachRunWrites()
        {
            FakeDisk disk = new FakeDisk();
            string folder = TestPaths.At("Feds");
            disk.File("Feds", "a.nwf");
            disk.File("Feds", "b.nwf");

            IList<string> lines = NwfPick.From(folder, false, string.Empty, disk.Disk()).SettingsLines();

            Assert.That(lines, Has.Some.EqualTo("NWFs picked       : " + folder + ", a folder"));
            Assert.That(lines, Has.Some.EqualTo("include subfolders: no"));
            Assert.That(lines, Has.Some.EqualTo("NWFs found        : 2"));
            Assert.That(lines, Has.Some.EqualTo("NWFs refused      : 0"));
            Assert.That(lines, Has.Some.StartsWith("each NWF          : opened where it sits"));
            Assert.That(lines, Has.Some.StartsWith("NWD               : beside each NWF"));
            Assert.That(lines, Has.Some.StartsWith("reports           : in " + ReportPaths.Subfolder + " beside each NWF"));
            Assert.That(lines, Has.Some.StartsWith("source folder     : not applicable"));
            Assert.That(lines, Has.Some.StartsWith("grouping          : not applicable"));

            foreach (string line in lines)
            {
                Assert.That(line.TrimEnd(), Does.Not.EndWith(":"), "a field is never left blank: " + line);
            }
        }

        [Test]
        public void TheSettingsLinesForOneFileSayTheSubfoldersBoxDoesNotApply()
        {
            FakeDisk disk = new FakeDisk();
            string nwf = disk.File("Feds", "a.nwf");
            string excel = TestPaths.At("Reports");

            IList<string> lines = NwfPick.From(nwf, true, excel, disk.Disk()).SettingsLines();

            Assert.That(lines, Has.Some.EqualTo("NWFs picked       : " + nwf + ", one file"));
            Assert.That(lines, Has.Some.StartsWith("include subfolders: not applicable"));
            Assert.That(lines, Has.Some.EqualTo("reports           : in the Excel folder picked, " + excel));
        }

        [Test]
        public void TheResultLineCountsTheNwfsAndNamesThePick()
        {
            FakeDisk disk = new FakeDisk();
            string folder = TestPaths.At("Feds");
            disk.File("Feds", "a.nwf");
            disk.File("Feds", "b|c.nwf");

            string line = NwfPick.From(folder, false, string.Empty, disk.Disk()).ResultLine();

            Assert.That(line, Does.StartWith("NWFs picked    : 2 from " + folder));
            Assert.That(line, Does.Contain("1 refused before it was opened"));
        }

        /// <summary>The window line after a pick, and the refusal where the run cannot start.</summary>
        [Test]
        public void TheWindowLineSaysHowManyRunOrWhyNone()
        {
            FakeDisk disk = new FakeDisk();
            string folder = TestPaths.At("Feds");
            disk.File("Feds", "a.nwf");
            disk.File("Feds", "b.nwf");

            Assert.That(NwfPick.From(folder, false, string.Empty, disk.Disk()).Describe(),
                Does.StartWith("2 NWFs found in " + folder + ", 2 can run."));
            Assert.That(NwfPick.From(string.Empty, false, string.Empty, disk.Disk()).Describe(),
                Does.Contain("Pick one NWF or a folder of NWFs"));
        }

        [Test]
        public void TheLabelAndTheGreyLineKeepTheTickBoxRule()
        {
            Assert.That(NwfPick.Label.Split(' ').Length, Is.LessThanOrEqualTo(8));
            Assert.That(NwfPick.HelpLine.Split(' ').Length, Is.LessThanOrEqualTo(12));

            foreach (string words in new[] { NwfPick.Label, NwfPick.HelpLine })
            {
                Assert.That(words, Does.Not.Contain(";"));
                Assert.That(words, Does.Not.Contain("\u2014"));
                Assert.That(words, Is.Not.EqualTo(words.ToUpperInvariant()));
            }
        }

        [Test]
        public void TheReasonsAGroupStopsAtTheOpenNameTheFile()
        {
            string nwf = TestPaths.At("Feds", "a.nwf");

            Assert.That(NwfPick.WouldNotOpen(nwf), Does.Contain(nwf));
            Assert.That(NwfPick.WouldNotOpen(nwf), Does.Contain("nothing was run on it"));
            Assert.That(NwfPick.ReadEmpty(nwf, 120.0), Does.Contain(nwf));
            Assert.That(NwfPick.ReadEmpty(nwf, 120.0), Does.Contain("no models"));
            Assert.That(NwfPick.ReadEmpty(nwf, 120.0), Does.Contain("120.0"));
        }

        /// <summary>
        /// The reader the add-in hands in reads a folder with no pattern and no wildcard, and
        /// walks the subfolders only where asked. Proved on a real folder.
        /// </summary>
        [Test]
        public void TheRealDiskReadsTheTopFolderAloneUnlessTheBoxAsks()
        {
            string folder = TempFolder.Make("nwf-pick");

            try
            {
                string top = Path.Combine(folder, "a.nwf");
                string deep = Path.Combine(folder, "Zone 1", "b.nwf");
                Directory.CreateDirectory(Path.GetDirectoryName(deep));
                File.WriteAllText(top, string.Empty);
                File.WriteAllText(deep, string.Empty);
                File.WriteAllText(Path.Combine(folder, "a.nwd"), string.Empty);
                File.WriteAllText(Path.Combine(folder, "c.nwfx"), string.Empty);

                Assert.That(NwfPick.From(folder, false, string.Empty).Paths(), Is.EqualTo(new[] { top }));
                Assert.That(NwfPick.From(folder, true, string.Empty).Paths(), Is.EqualTo(new[] { top, deep }));
                Assert.That(NwfPick.From(top, false, string.Empty).Paths(), Is.EqualTo(new[] { top }));
            }
            finally
            {
                TempFolder.Remove(folder);
            }
        }
    }
}
