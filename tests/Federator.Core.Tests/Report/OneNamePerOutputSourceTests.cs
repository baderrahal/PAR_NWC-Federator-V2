using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using NUnit.Framework;

namespace Federator.Core.Tests
{
    /// <summary>
    /// F243, Bader's order of 19 Oct 2026: one name per output, overwritten, no dated and no internal files. The add-in cannot
    /// be built here, so where it calls what is read off its source, the way GroupOrderSourceTests reads the order of a group.
    /// The rules themselves are Core's and proved by their own tests: FileInUse, WorkbookLinks, PicturesFolder, OldReportFiles.
    /// </summary>
    [TestFixture]
    public class OneNamePerOutputSourceTests
    {
        private static string Source(params string[] parts)
        {
            string repo = Samples.Repo();
            Assert.That(repo, Is.Not.Null, "the checkout was not found from the test assembly");

            string path = Path.Combine(repo, Path.Combine(parts));
            Assert.That(File.Exists(path), Is.True, path);
            return File.ReadAllText(path);
        }

        private static string Engine()
        {
            return Source("src", "Federator.Addin", "Engine", "FederationEngine.cs");
        }

        private static string Packages()
        {
            return Source("src", "Federator.Addin", "Ui", "FederatorWindow.Packages.cs");
        }

        private static string BodyOf(string source, string signature)
        {
            int start = source.IndexOf(signature, StringComparison.Ordinal);
            Assert.That(start, Is.GreaterThanOrEqualTo(0), signature + " is not in the source");

            int next = source.IndexOf("\n        /// <summary>", start, StringComparison.Ordinal);
            int nextPrivate = source.IndexOf("\n        private ", start + signature.Length, StringComparison.Ordinal);
            int end = next < 0 ? source.Length : next;

            if (nextPrivate >= 0 && nextPrivate < end)
            {
                end = nextPrivate;
            }

            return source.Substring(start, end - start);
        }

        private static int At(string body, string token)
        {
            int index = body.IndexOf(token, StringComparison.Ordinal);
            Assert.That(index, Is.GreaterThanOrEqualTo(0), token + " is not in the body read");
            return index;
        }

        /// <summary>Every .cs file under src, never one under bin or obj.</summary>
        private static IList<string> SourceFiles()
        {
            List<string> files = new List<string>();
            string src = Path.Combine(Samples.Repo(), "src");

            foreach (string file in Directory.GetFiles(src, "*.cs", SearchOption.AllDirectories))
            {
                string relative = file.Substring(src.Length + 1);

                if (relative.IndexOf(Path.DirectorySeparatorChar + "bin" + Path.DirectorySeparatorChar, StringComparison.Ordinal) < 0
                    && relative.IndexOf(Path.DirectorySeparatorChar + "obj" + Path.DirectorySeparatorChar, StringComparison.Ordinal) < 0)
                {
                    files.Add(file);
                }
            }

            return files;
        }

        /// <summary>Point 5. No output is given a dated name: the free name is taken by the copy of the run log alone.</summary>
        [Test]
        public void TheFreeNameIsTakenByTheCopyOfTheRunLogAloneAndNoOutputIsDated()
        {
            List<string> callers = new List<string>();

            foreach (string file in SourceFiles())
            {
                string text = File.ReadAllText(file);

                Assert.That(text, Does.Not.Contain("NameThatOverwritesNothing"), file);

                if (Regex.IsMatch(text, @"ReportPaths\.FreeFilePath\("))
                {
                    callers.Add(Path.GetFileName(file));
                }
            }

            Assert.That(callers.Count, Is.EqualTo(1), string.Join(", ", callers.ToArray()));
            Assert.That(callers[0], Is.EqualTo("LogCopy.cs"));
        }

        /// <summary>Point 4. No internal workbook is written, by any file under src.</summary>
        [Test]
        public void NoInternalWorkbookIsWritten()
        {
            foreach (string file in SourceFiles())
            {
                string text = File.ReadAllText(file);

                Assert.That(text, Does.Not.Contain("InternalWorkbook"), file);
                Assert.That(text, Does.Not.Contain("WorkbookNameFor("), file);
            }
        }

        /// <summary>Point 1 and 3. The clash Excel is written and read back at the page's path, beside its pictures.</summary>
        [Test]
        public void TheClashWorkbookIsWrittenAndReadBackBesideItsPictures()
        {
            string engine = Engine();

            Assert.That(engine, Does.Not.Contain("ReportPaths.Workbook(reportFolder, job.WorkbookName)"),
                "the clash workbook is no longer written into the Log folder");
            Assert.That(BodyOf(engine, "private void WriteWorkbook("), Does.Contain("string path = ThePagePath(job);"));
            Assert.That(BodyOf(engine, "private void TakeTheCoverage("), Does.Contain("string path = ThePagePath(job);"));
        }

        /// <summary>Point 3. Every link is looked for on the disk once the workbook is written, and one missing is the group's error.</summary>
        [Test]
        public void TheWorkbooksLinksAreReadBackAfterItsWriteAndAMissingOneIsAnError()
        {
            string body = BodyOf(Engine(), "private void WriteTheWorkbook(");

            Assert.That(At(body, "WorkbookLinks.Check(path, linked, expected)"), Is.GreaterThan(At(body, "writer.Write(report, path)")));
            Assert.That(body, Does.Contain("outcome.AddError(links.Error())"));
        }

        /// <summary>
        /// Point 2. The group's pictures folder is deleted once, before the pictures are taken where they are, and before the
        /// workbook and the page where they are not.
        /// </summary>
        [Test]
        public void ThePicturesFolderIsDeletedBeforeThePicturesAndBeforeTheReport()
        {
            string engine = Engine();
            string pictures = BodyOf(engine, "private void TakeThePicturesOfTheGroup(");
            string report = BodyOf(engine, "private void WriteWorkbook(");
            string clash = BodyOf(engine, "private bool CreateAndRunTheTests(");

            Assert.That(At(pictures, "DeleteThePicturesFolderOnce("), Is.LessThan(At(pictures, "runner.TakeTheDeferredPictures(clash)")));
            Assert.That(At(report, "DeleteThePicturesFolderOnce("), Is.LessThan(At(report, "WriteTheWorkbook(job, outcome, report, path)")));
            Assert.That(At(report, "DeleteThePicturesFolderOnce("), Is.LessThan(At(report, "WriteHtmlTabular(job, outcome, report)")));
            Assert.That(clash, Does.Not.Contain("DeleteThePicturesFolderOnce("),
                "F243 attempt 2, the reviewer's N2: the hand button, the one route taking pictures inside the clash step, writes no report");
        }

        /// <summary>Point 6. Each file the tool's own code writes says a file left open in one line and is never written elsewhere.</summary>
        [Test]
        public void EveryWriterOfAnOutputSaysAFileLeftOpen()
        {
            string engine = Engine();
            string packages = Packages();
            string refresh = Source("src", "Federator.Addin", "Engine", "RefreshRun.cs");

            foreach (string body in new[]
            {
                BodyOf(engine, "private void WriteTheWorkbook("),
                BodyOf(engine, "private void WriteHtmlTabular("),
                BodyOf(engine, "private void WriteTheClashXml("),
                BodyOf(engine, "internal static void WriteTheList("),
                BodyOf(engine, "private void DeleteThePicturesFolderOnce("),
                BodyOf(refresh, "internal string BackupTheNwf("),
                BodyOf(packages, "private void WriteTheClashCountChecksOf("),
                BodyOf(packages, "private void WriteOnePackagesReports(")
            })
            {
                Assert.That(body, Does.Contain("NotWrittenWhileOpen("), body.Substring(0, Math.Min(80, body.Length)));
            }
        }

        /// <summary>Point 7. The old names are cleaned once, by the window that starts the run, before any group starts, on both pages.</summary>
        [Test]
        public void TheOldNamesAreCleanedOnceBeforeAnyGroupOnBothPages()
        {
            string packages = Packages();
            string nwc = BodyOf(packages, "private void OnRun(");
            string nwf = BodyOf(packages, "private void OnRunNwfPackages(");

            Assert.That(At(nwc, "CleanTheOldReportFiles("), Is.LessThan(At(nwc, "RunPackagesInFreshStarts(PackageRunOrder.NwcPage")));
            Assert.That(At(nwf, "CleanTheOldReportFiles("), Is.LessThan(At(nwf, "RunPackagesInFreshStarts(PackageRunOrder.NwfPage")));
            Assert.That(At(nwc, "if (child == null)"), Is.LessThan(At(nwc, "CleanTheOldReportFiles(")));
            Assert.That(At(nwf, "if (child == null)"), Is.LessThan(At(nwf, "CleanTheOldReportFiles(")));

            // F243 attempt 2, the breaker's S1 and the reviewer's N5: each package's own report folder, and both names of a group.
            Assert.That(nwc, Does.Contain("PackageOutput.FolderFor(report, package)"));
            Assert.That(nwf, Does.Contain("PackageOutput.FolderFor(report, nwf.Package)"));
            Assert.That(nwc, Does.Contain(".Add(group.WorkbookName)"));
            Assert.That(nwc, Does.Contain(".Add(group.NwfName)"));
        }

        /// <summary>
        /// F243 attempt 2, the breaker's W6. The backup says which file is held: the NWF is opened for reading before the copy,
        /// so a held NWF is named and never the Backup path in its place.
        /// </summary>
        [Test]
        public void TheBackupNamesTheFileThatIsHeld()
        {
            string body = BodyOf(Source("src", "Federator.Addin", "Engine", "RefreshRun.cs"), "internal string BackupTheNwf(");

            Assert.That(At(body, "new FileStream(nwfPath, FileMode.Open, FileAccess.Read, FileShare.Read)"),
                Is.LessThan(At(body, "File.Copy(nwfPath, target, true)")));
            Assert.That(body, Does.Contain("log.NotWrittenWhileOpen(held)"));
        }

        /// <summary>
        /// F243 attempt 2, the breaker's S3 and W5. Once the report step is over, a page or a workbook this run did not write and
        /// an earlier run left at its path, after the pictures folder went, is named.
        /// </summary>
        [Test]
        public void TheEarlierReportsLeftAreNamedOnceTheReportIsWritten()
        {
            string engine = Engine();
            string tail = engine.Substring(engine.IndexOf("private void FinishTheGroup(", StringComparison.Ordinal));

            Assert.That(At(tail, "SayTheEarlierReportsLeft(job, outcome)"), Is.GreaterThan(At(tail, "WriteWorkbook(job, outcome)")));
            Assert.That(BodyOf(engine, "private void SayTheEarlierReportsLeft("), Does.Contain("PicturesFolder.EarlierFileLine("));
        }

        /// <summary>
        /// F243 attempt 2, the lead's decision on item 1. A group's own Navisworks writes no list and carries its tally's groups
        /// back in its result, and the window that started the run writes the one list over them before its joined RESULT.
        /// </summary>
        [Test]
        public void TheOneListIsWrittenByTheWindowThatStartedTheRun()
        {
            string engine = BodyOf(Engine(), "private void WriteTheListForTheModellers(");
            string fresh = Source("src", "Federator.Addin", "Ui", "FederatorWindow.FreshStarts.cs");
            string join = BodyOf(fresh, "private void JoinTheResults(");

            Assert.That(At(engine, "if (packagesRun)"), Is.LessThan(At(engine, "WriteTheList(log, CoordinatesAcrossTheRun")));
            Assert.That(BodyOf(fresh, "private void WriteThePackagesResult("), Does.Contain("result.OffCoordinates.Add(group)"));
            Assert.That(At(join, "WriteTheOneListOfTheRun(run, results, order.Units.Count)"), Is.LessThan(At(join, "log.WriteJoinedResult(")));
            Assert.That(BodyOf(fresh, "private void WriteTheOneListOfTheRun("), Does.Contain("PackageRunResult.OffCoordinatesOf(run.Started"));
        }

        /// <summary>F243 attempt 2, the breaker's S5. The links are checked against the count the writer made, each missing one a row.</summary>
        [Test]
        public void TheLinksAreCheckedAgainstWhatTheWriterMade()
        {
            string body = BodyOf(Engine(), "private void WriteTheWorkbook(");

            Assert.That(body, Does.Contain("WorkbookLinks.Check(path, linked, expected)"));
            Assert.That(body, Does.Contain("foreach (string missing in links.Missing)"));
        }

        /// <summary>Point 6. The window that started the run puts each file left open in its end message, its own and each group's.</summary>
        [Test]
        public void TheEndMessageOfTheRunCarriesEachFileLeftOpen()
        {
            string fresh = Source("src", "Federator.Addin", "Ui", "FederatorWindow.FreshStarts.cs");
            string join = BodyOf(fresh, "private void JoinTheResults(");

            Assert.That(join, Does.Contain("FileInUse.LineIn("));
            Assert.That(join, Does.Contain("FileInUse.EndMessage("));
            Assert.That(join, Does.Contain("PackageRunWords.JoinedResult(order.Units, results, whys, tally, own)"),
                "F243 attempt 2, the reviewer's N4: the window's own lines are in its joined RESULT");
            Assert.That(At(join, "log.FilesInUse"), Is.LessThan(At(join, "log.WriteJoinedResult(")), "read before RESULT closes the account");
        }
    }
}
