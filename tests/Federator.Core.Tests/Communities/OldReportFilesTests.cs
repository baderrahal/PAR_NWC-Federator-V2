using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Federator.Core.Communities;
using Federator.Core.Disciplines;
using Federator.Core.Report;
using Federator.Core.Rerun;
using NUnit.Framework;

namespace Federator.Core.Tests
{
    /// <summary>
    /// F243, Bader's order of 19 Oct 2026 point 7. At the start of a run the files earlier runs wrote under names the tool no
    /// longer writes are deleted, inside Report\COMMUNITY\ only, and only where a name matches a pattern exactly. Each pattern
    /// has its test, and each test puts a near miss beside it that must stay. The names are the 19 Oct run's, read off
    /// C:\NWC-PC-Test\work\final-2026-10-19\Report\C06.
    /// </summary>
    [TestFixture]
    public class OldReportFilesTests
    {
        private const string Community = "C06";
        private const string Group = "1104-PAR-1B06BC-ZZZ-BM-MOD-000001";
        private const string Other = "1104-PAR-1B06G1-ZZZ-BM-MOD-000001";
        private const string NotInTheRun = "1104-PAR-1C07BC-ZZZ-BM-MOD-000001";

        private string root;
        private string community;
        private string log;
        private string backup;

        [SetUp]
        public void MakeFolder()
        {
            root = TempFolder.Make("FederatorOldReportFiles");
            community = PackageOutput.CommunityFolderFor(root, Community);
            log = Path.Combine(community, ReportPaths.Subfolder);
            backup = Path.Combine(community, NwfBackupName.FolderName);
            Directory.CreateDirectory(log);
            Directory.CreateDirectory(backup);
        }

        [TearDown]
        public void RemoveFolder()
        {
            TempFolder.Remove(root);
        }

        private static string Put(string folder, string name)
        {
            Directory.CreateDirectory(folder);
            string path = Path.Combine(folder, name);
            File.WriteAllText(path, name);
            return path;
        }

        /// <summary>The community's own package, C06 itself, and a package below it, each with its report folder.</summary>
        private Dictionary<string, string> Packages()
        {
            return new Dictionary<string, string>
            {
                { Community, community },
                { Path.Combine(Community, "Infrastructure"), Path.Combine(community, "Infrastructure") }
            };
        }

        private OldReportFilesOutcome Clean()
        {
            return OldReportFiles.Clean(community, Community, Packages(), new[] { Group, Other }, new DisciplineReportSettings());
        }

        private static void Gone(params string[] paths)
        {
            foreach (string path in paths)
            {
                Assert.That(File.Exists(path), Is.False, Path.GetFileName(path) + " is an old name and was not deleted");
            }
        }

        private static void Kept(params string[] paths)
        {
            foreach (string path in paths)
            {
                Assert.That(File.Exists(path), Is.True, Path.GetFileName(path) + " is not an old name and was deleted");
            }
        }

        [Test]
        public void TheClashWorkbookLeftInTheLogFolderGoesAndANearMissStays()
        {
            string old = Put(log, Group + ".xlsx");
            string[] near =
            {
                Put(log, NotInTheRun + ".xlsx"),
                Put(log, Group + " copy.xlsx"),
                Put(log, Group + ".xml"),
                Put(community, Group + ".xlsx")
            };

            OldReportFilesOutcome outcome = Clean();

            Gone(old);
            Kept(near);
            Assert.That(outcome.Lines, Does.Contain("OLD      deleted " + old
                + ", the clash workbook written in the Log folder before it moved to the Clash report folder, a name this tool no longer writes"));
            Assert.That(outcome.SummaryLine(), Is.EqualTo("OLD      " + community + " and 1 package folder read for the old names of 2 groups"
                + " and 4 disciplines of this run, 1 file deleted"));
        }

        [Test]
        public void ADatedClashWorkbookGoesWithOrWithoutItsCounterAndANearMissStays()
        {
            string[] old =
            {
                Put(log, Group + " 2026-10-19 062133.xlsx"),
                Put(log, Other + " 2026-10-19 062232 2.xlsx"),
                Put(log, Group + " 2026-10-19 062133 12.xlsx")
            };
            string[] near =
            {
                Put(log, Group + " 2026-10-19 06213.xlsx"),
                Put(log, Group + " 2026-13-19 062133.xlsx"),
                Put(log, Group + " 2026-10-19 062133 1.xlsx"),
                Put(log, Group + " 2026-10-19 062133 02.xlsx"),
                Put(log, Group + " 2026-10-19 062133 2x.xlsx"),
                Put(log, NotInTheRun + " 2026-10-19 062133.xlsx")
            };

            Clean();

            Gone(old);
            Kept(near);
        }

        [Test]
        public void TheInternalWorkbookAndItsDatedFormsGoAndANearMissStays()
        {
            string[] old =
            {
                Put(log, Group + " Internal.xlsx"),
                Put(log, Group + " 2026-10-19 062133 Internal.xlsx"),
                Put(log, Other + " 2026-10-19 062232 3 Internal.xlsx")
            };
            string[] near =
            {
                Put(log, Group + " Internals.xlsx"),
                Put(log, Group + " Internal 2026-10-19 062133.xlsx"),
                Put(log, Group + " 2026-10-19 062133 1 Internal.xlsx"),
                Put(log, NotInTheRun + " Internal.xlsx"),
                Put(community, Group + " Internal.xlsx")
            };

            Clean();

            Gone(old);
            Kept(near);
        }

        [Test]
        public void TheDatedListOfModelsOffTheCoordinatesGoesAndTheOneListAndANearMissStay()
        {
            string old = Put(log, "Models not on the same shared coordinates, run 2026-10-19 055733.txt");
            string[] near =
            {
                Put(log, "Models not on the same shared coordinates.txt"),
                Put(log, "Models not on the same shared coordinates, run 2026-10-19 055733 2.txt"),
                Put(log, "Models not on the same shared coordinates, run 2026-10-19.txt"),
                Put(community, "Models not on the same shared coordinates, run 2026-10-19 055733.txt")
            };

            Clean();

            Gone(old);
            Kept(near);
        }

        [Test]
        public void ADatedDisciplineWorkbookGoesAndTheOneWorkbookAndANearMissStay()
        {
            string[] old =
            {
                Put(community, "C06 AR checks 2026-10-19 064154.xlsx"),
                Put(community, "C06 EL checks 2026-10-19 064156 2.xlsx")
            };
            string[] near =
            {
                Put(community, "C06 AR checks.xlsx"),
                Put(community, "C06 XX checks 2026-10-19 064154.xlsx"),
                Put(community, "C07 AR checks 2026-10-19 064154.xlsx"),
                Put(community, "C06 AR check 2026-10-19 064154.xlsx"),
                Put(log, "C06 AR checks 2026-10-19 064154.xlsx")
            };

            Clean();

            Gone(old);
            Kept(near);
        }

        [Test]
        public void ADatedClashCountCheckGoesAndTheOneCheckAndANearMissStay()
        {
            string[] old =
            {
                Put(community, "C06 clash count check 2026-10-19 064158.xlsx"),
                Put(community, "C06 clash count check 2026-10-19 064158 2.xlsx")
            };
            string[] near =
            {
                Put(community, "C06 clash count check.xlsx"),
                Put(community, "C06 clash count check 2026-10-19 064158.xls"),
                Put(community, "C07 clash count check 2026-10-19 064158.xlsx")
            };

            Clean();

            Gone(old);
            Kept(near);
        }

        [Test]
        public void TheNewestDatedBackupBecomesTheOneBackupAndTheOlderGo()
        {
            string older = Put(backup, Group + " 2026-10-19 062133.nwf");
            string sameSecond = Put(backup, Group + " 2026-10-19 062133 (2).nwf");
            string earlier = Put(backup, Group + " 2026-10-18 101500.nwf");
            string[] near =
            {
                Put(backup, Group + " 2026-10-19 062133 2.nwf"),
                Put(backup, Group + " 2026-10-19 062133 (1).nwf"),
                Put(backup, NotInTheRun + " 2026-10-19 062133.nwf")
            };

            OldReportFilesOutcome outcome = Clean();
            string one = Path.Combine(backup, Group + ".nwf");

            Assert.That(File.Exists(one), Is.True);
            Assert.That(File.ReadAllText(one), Is.EqualTo(Path.GetFileName(sameSecond)), "the newest, by its moment and then its counter");
            Gone(older, sameSecond, earlier);
            Kept(near);
            Assert.That(outcome.Lines, Does.Contain("OLD      renamed " + sameSecond + " to " + one
                + ", the newest dated backup under the one name a backup has now"));
        }

        [Test]
        public void WhereTheOneBackupIsThereTheNewestDatedStaysAndIsSaidAndTheOlderGo()
        {
            string one = Put(backup, Group + ".nwf");
            string newest = Put(backup, Group + " 2026-10-19 062133.nwf");
            string older = Put(backup, Group + " 2026-10-18 101500.nwf");

            OldReportFilesOutcome outcome = Clean();

            Assert.That(File.ReadAllText(one), Is.EqualTo(Group + ".nwf"), "the one backup is not overwritten");
            Kept(newest);
            Gone(older);
            Assert.That(outcome.Lines, Does.Contain("OLD      kept " + newest + " as it is, the newest dated backup, because " + one + " is already there"));
        }

        /// <summary>
        /// F243 attempt 2, the breaker's S1. A package below the community writes its Log, its Backup and its discipline
        /// workbooks in its own report folder, Report\C06\Infrastructure, so that folder is cleaned by its own names: the
        /// discipline workbook's stem is the package's path in words, and the clash count check is the community's alone.
        /// </summary>
        [Test]
        public void APackageFolderBelowTheCommunityIsCleanedByItsOwnNames()
        {
            string package = Path.Combine(community, "Infrastructure");
            string packageBackup = Path.Combine(package, NwfBackupName.FolderName);
            string[] old =
            {
                Put(Path.Combine(package, ReportPaths.Subfolder), Group + ".xlsx"),
                Put(Path.Combine(package, ReportPaths.Subfolder), Group + " Internal.xlsx"),
                Put(package, "C06 Infrastructure AR checks 2026-10-19 064154.xlsx")
            };
            string dated = Put(packageBackup, Group + " 2026-10-19 062133.nwf");
            string[] near =
            {
                Put(package, "C06 AR checks 2026-10-19 064154.xlsx"),
                Put(package, "C06 clash count check 2026-10-19 064158.xlsx"),
                Put(Path.Combine(community, "Other", ReportPaths.Subfolder), Group + ".xlsx")
            };

            Clean();

            Gone(old);
            Gone(dated);
            Kept(near);
            Assert.That(File.Exists(Path.Combine(packageBackup, Group + ".nwf")), Is.True, "its newest dated backup under the one name");
        }

        /// <summary>The break: a package folder handed in that is not inside the community is never read and is said.</summary>
        [Test]
        public void AFolderOutsideTheCommunityIsNeverRead()
        {
            string outside = Path.Combine(root, "C07");
            string[] there =
            {
                Put(Path.Combine(outside, ReportPaths.Subfolder), Group + ".xlsx"),
                Put(outside, "C07 AR checks 2026-10-19 064154.xlsx")
            };

            OldReportFilesOutcome outcome = OldReportFiles.Clean(community, Community,
                new Dictionary<string, string> { { "C07", outside } }, new[] { Group }, new DisciplineReportSettings());

            Kept(there);
            Assert.That(outcome.Lines, Does.Contain("OLD      " + outside + " is not inside " + community + ", so nothing in it is read"));
        }

        /// <summary>F243 attempt 2, the reviewer's N5. A group's workbook name and its NWF name are both read, each once.</summary>
        [Test]
        public void AWorkbookNameAndAnNwfNameOfOneGroupAreBothRead()
        {
            string byWorkbook = Put(log, "BC workbook 2026-10-19 062133.xlsx");
            string byNwf = Put(backup, Group + " 2026-10-19 062133.nwf");

            OldReportFiles.Clean(community, Community, Packages(), new[] { Group, "BC workbook", Group }, new DisciplineReportSettings());

            Gone(byWorkbook, byNwf);
            Assert.That(File.Exists(Path.Combine(backup, Group + ".nwf")), Is.True);
        }

        [Test]
        public void AFolderWithNoOldNameSaysSoAndOneThatIsNotThereIsLeftAlone()
        {
            Put(log, "run-20261019-055732.log");

            OldReportFilesOutcome outcome = Clean();

            Assert.That(outcome.Lines.Count, Is.EqualTo(0));
            Assert.That(outcome.SummaryLine(), Is.EqualTo("OLD      " + community + " and 1 package folder read for the old names of 2 groups"
                + " and 4 disciplines of this run, none of them was there"), "never a claim about names it did not look for");

            string absent = Path.Combine(root, "C99");
            OldReportFilesOutcome none = OldReportFiles.Clean(absent, "C99", new Dictionary<string, string> { { "C99", absent } }, new[] { Group }, new DisciplineReportSettings());

            Assert.That(none.Lines.Count, Is.EqualTo(0));
            Assert.That(Directory.Exists(absent), Is.False);
        }

        [Test]
        public void AFileHeldOpenIsNamedAsInUseAndTheRestStillGo()
        {
            TestPaths.OnWindowsOnly("a file held open refusing to be deleted");

            string held = Put(log, Group + " Internal.xlsx");
            string free = Put(log, Group + ".xlsx");
            OldReportFilesOutcome outcome;

            using (new FileStream(held, FileMode.Open, FileAccess.Read, FileShare.None))
            {
                outcome = Clean();
            }

            Assert.That(outcome.InUse.Count, Is.EqualTo(1));
            Assert.That(outcome.InUse[0], Is.EqualTo(held));
            Assert.That(outcome.Failed.Count, Is.EqualTo(0));
            Kept(held);
            Gone(free);
        }

        [Test]
        public void TheKindIsReadOffThePathBelowThePackageFolderAndNoneElsewhere()
        {
            ICollection<string> groups = new[] { Group };
            DisciplineReportSettings settings = new DisciplineReportSettings();
            string infrastructure = Path.Combine(Community, "Infrastructure");

            Assert.That(OldReportFiles.KindOf(Path.Combine(ReportPaths.Subfolder, Group + ".xlsx"), Community, groups, settings),
                Is.EqualTo("the clash workbook written in the Log folder before it moved to the Clash report folder"));
            Assert.That(OldReportFiles.KindOf(Group + ".xlsx", Community, groups, settings), Is.Null, "the package's own folder");
            Assert.That(OldReportFiles.KindOf(Path.Combine(ReportPaths.Subfolder, Group + " internal.xlsx"), Community, groups, settings), Is.Null,
                "matched as the tool wrote it, Ordinal");
            Assert.That(OldReportFiles.KindOf(Path.Combine("Clash report", Group + ".xlsx"), Community, groups, settings), Is.Null);
            Assert.That(OldReportFiles.KindOf("C06 ST checks 2026-10-19 064156.xlsx", Community, groups, settings),
                Is.EqualTo("a dated discipline workbook"));
            Assert.That(OldReportFiles.KindOf("C06 ST checks 2026-10-19 064156.xlsx", "C07", groups, settings), Is.Null);
            Assert.That(OldReportFiles.KindOf("C06 Infrastructure ST checks 2026-10-19 064156.xlsx", infrastructure, groups, settings),
                Is.EqualTo("a dated discipline workbook"));
            Assert.That(OldReportFiles.KindOf("C06 ST checks 2026-10-19 064156.xlsx", infrastructure, groups, settings), Is.Null);
            Assert.That(OldReportFiles.KindOf("C06 clash count check 2026-10-19 064158.xlsx", Community, groups, settings), Is.Null,
                "the clash count check is the community folder's alone, read apart");
            Assert.That(OldReportFiles.KindOf(null, Community, groups, settings), Is.Null);
        }

        [Test]
        public void ADatedBackupIsReadByItsMomentAndItsCounter()
        {
            DateTime moment;
            int counter;

            Assert.That(OldReportFiles.IsDatedBackup(Group + " 2026-10-19 062133.nwf", Group, out moment, out counter), Is.True);
            Assert.That(moment, Is.EqualTo(new DateTime(2026, 10, 19, 6, 21, 33)));
            Assert.That(counter, Is.EqualTo(0));
            Assert.That(OldReportFiles.IsDatedBackup(Group + " 2026-10-19 062133 (17).nwf", Group, out moment, out counter), Is.True);
            Assert.That(counter, Is.EqualTo(17));
            Assert.That(OldReportFiles.IsDatedBackup(Group + ".nwf", Group, out moment, out counter), Is.False);
            Assert.That(OldReportFiles.IsDatedBackup(Group + " 2026-10-19 062133 ().nwf", Group, out moment, out counter), Is.False);
            Assert.That(OldReportFiles.IsDatedBackup(Group + " 2026-10-19 062133.NWF", Group, out moment, out counter), Is.False);
        }
    }
}
