using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Federator.Core.Clash;
using Federator.Core.Report;
using NUnit.Framework;

namespace Federator.Core.Tests
{
    /// <summary>
    /// The report carries the logo Navisworks puts on its own, because that is the report
    /// the client already accepts. Not a Parsons mark, and nothing to pick before a first
    /// run.
    ///
    /// Measured on 2026-09-01. The file is
    ///
    ///     C:\Program Files\Autodesk\Navisworks Manage 2025\Images\logo.jpg   6137 bytes
    ///
    /// and it is byte for byte the same as the logo.jpg in the _files folder of both
    /// reports Bader supplied, MD5 b5df301defc444485c0461748c56e0d9, which is what proves
    /// Navisworks copies that exact file into the report folder.
    ///
    /// No copy of it is in this repo or in the bundle. It is read off the install on every
    /// run and copied into the report's own folder, so what goes to a client is a picture
    /// that was already on the machine that wrote the report.
    /// </summary>
    [TestFixture]
    public class ReportLogoTests
    {
        private const string Root = "lcop_selection_set_tree";
        private const string Install = @"C:\Program Files\Autodesk\Navisworks Manage 2025";
        private const string OutputName = "1104-PAR-1C07BC-ZZZ-BM-RPT-000001";

        private string folder;

        [SetUp]
        public void MakeFolder()
        {
            folder = Path.Combine(Path.GetTempPath(), "FederatorLogo", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(folder);
        }

        [TearDown]
        public void RemoveFolder()
        {
            try
            {
                if (Directory.Exists(folder))
                {
                    Directory.Delete(folder, true);
                }
            }
            catch (IOException)
            {
                // A leftover temp folder is not worth failing a test over.
            }
        }

        private string Workbook()
        {
            return Path.Combine(folder, OutputName + ".xlsx");
        }

        // ---------- it is found on the install ----------

        // The one the brief asks for by name.
        [Test]
        public void TheLogoIsFoundOnTheInstall()
        {
            if (!Directory.Exists(Install))
            {
                Assert.Ignore("Navisworks is not on this machine.");
            }

            string found = InstallFiles.FindLogo(Install, "en-US");

            Assert.That(found, Is.Not.Empty, "the install's own logo was not found");
            Assert.That(found, Is.EqualTo(Path.Combine(Install, @"Images\logo.jpg")));
            Assert.That(File.Exists(found), Is.True);
            Assert.That(new FileInfo(found).Length, Is.EqualTo(6137),
                "the file measured on 2026-09-01 was 6137 bytes");
        }

        // The proof that this is the file Navisworks itself copies into a report folder.
        // Both reports Bader supplied carry it, byte for byte.
        [Test]
        public void ItIsTheSameFileTheSuppliedReportsCarry()
        {
            if (!Directory.Exists(Install))
            {
                Assert.Ignore("Navisworks is not on this machine.");
            }

            string repo = Samples.Repo();

            if (repo == null)
            {
                Assert.Ignore("The checkout is not beside the test binaries.");
            }

            string mine = InstallFiles.FindLogo(Install, "en-US");

            // Found from the solution file rather than counted in ..\ steps, which is how
            // this test came to be silently skipped instead of run.
            string theirs = Path.Combine(repo,
                @"samples\client-report\1104-PAR-1A02WN-XXX-BM-RPT-000001_files\logo.jpg");

            Assert.That(File.Exists(theirs), Is.True,
                "the supplied client export is not in this checkout: " + theirs);

            Assert.That(File.ReadAllBytes(mine), Is.EqualTo(File.ReadAllBytes(theirs)));
        }

        [Test]
        public void ImagesSitsAtTheTopOfTheInstallAndIsTriedFirst()
        {
            IList<string> tried = InstallFiles.LogoCandidates(@"C:\NW", "de-DE");

            Assert.That(tried[0], Is.EqualTo(@"C:\NW\Images\logo.jpg"),
                "Images is at the top of the install, with no language folder");
            Assert.That(tried, Does.Contain(@"C:\NW\de-DE\Images\logo.jpg"));
            Assert.That(tried, Does.Contain(@"C:\NW\en-US\Images\logo.jpg"));
        }

        [Test]
        public void NoInstallFolderIsNoCandidatesRatherThanAThrow()
        {
            Assert.That(InstallFiles.LogoCandidates(string.Empty, "en-US").Count, Is.EqualTo(0));
            Assert.That(InstallFiles.FindLogo(string.Empty, "en-US"), Is.Empty);
            Assert.That(InstallFiles.FindLogo(@"Q:\nowhere", "en-US"), Is.Empty);
        }

        // ---------- nothing of theirs is in the repo or the bundle ----------

        // The one the brief asks for by name.
        [Test]
        public void NoLogoFileIsAnywhereInTheRepoOrTheBundle()
        {
            string repo = Samples.Repo();

            if (repo == null)
            {
                Assert.Ignore("The checkout is not beside the test binaries.");
            }

            // samples is left out ON PURPOSE and checked separately below. Bader committed
            // two whole client exports there and Navisworks had already put its logo.jpg
            // inside each _files folder. Those are his reference copies of what the client
            // receives, not anything this tool ships.
            foreach (string where in new[] { "src", "build", "bundle", "tests", "docs" })
            {
                string look = Path.Combine(repo, where);

                if (!Directory.Exists(look))
                {
                    continue;
                }

                string[] found = Directory.GetFiles(look, "logo.*", SearchOption.AllDirectories);

                Assert.That(found, Is.Empty,
                    "a logo file is committed under " + where + ": "
                    + string.Join(", ", found));
            }

            // install.ps1 must not carry one in its file list either.
            string install = Path.Combine(repo, @"build\install.ps1");

            if (File.Exists(install))
            {
                // A logo FILE, not the word. install.ps1 passes --nologo to dotnet, which
                // is the build flag and has nothing to do with a picture.
                Assert.That(
                    Regex.IsMatch(File.ReadAllText(install), @"logo\.(jpg|jpeg|png|gif|bmp)",
                        RegexOptions.IgnoreCase),
                    Is.False,
                    "install.ps1 names a logo file, so one would ship in the bundle");
            }
        }

        /// <summary>
        /// The only logo files in the checkout are the ones inside the client exports
        /// Bader committed. Named here so the exception above is visible rather than a
        /// silent gap, and so a logo appearing anywhere else fails the test above.
        /// </summary>
        [Test]
        public void TheOnlyLogosInTheCheckoutAreInsideTheSuppliedClientExports()
        {
            string repo = Samples.Repo();

            if (repo == null)
            {
                Assert.Ignore("The checkout is not beside the test binaries.");
            }

            foreach (string found in Directory.GetFiles(repo, "logo.*", SearchOption.AllDirectories))
            {
                if (found.IndexOf(@"\.git\", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    continue;
                }

                // Two places, both of them sample REPORT folders rather than anything this
                // tool ships. client-report holds the exports Bader received, and
                // our-report holds one this tool produced, where the logo was copied in
                // beside the clash pictures exactly as it is meant to be.
                Assert.That(found, Does.Contain(@"samples\"),
                    "a logo file is in the checkout outside the sample reports");
                Assert.That(found, Does.Contain("_files"),
                    "it is not inside a report folder, so it did not get there by being copied");
            }
        }

        // ---------- it goes into the report's own folder ----------

        // The one the brief asks for by name.
        [Test]
        public void TheLogoGoesBesideTheClashPicturesAndIsLinkedRelatively()
        {
            Assert.That(ImageNaming.LogoPathFor(Workbook()),
                Is.EqualTo(Path.Combine(folder, OutputName + "_files", "logo.jpg")));

            string link = ImageNaming.LogoLinkFor(Workbook());

            Assert.That(link, Is.EqualTo(OutputName + "_files/logo.jpg"));
            Assert.That(link, Does.Not.Contain(":"),
                "an absolute link only works on the machine that wrote it");
            Assert.That(link, Does.Not.Contain("file:///"));
            Assert.That(link, Does.Not.Contain("\\"));
        }

        [Test]
        public void ItSitsInTheSameFolderAsTheClashPictures()
        {
            Assert.That(Path.GetDirectoryName(ImageNaming.LogoPathFor(Workbook())),
                Is.EqualTo(Path.GetDirectoryName(ImageNaming.PathFor(Workbook(), 0, 1))));
        }

        [Test]
        public void NothingAtAllIsNothingRatherThanAThrow()
        {
            Assert.That(ImageNaming.LogoPathFor(string.Empty), Is.Empty);
            Assert.That(ImageNaming.LogoLinkFor(null), Is.Empty);
        }

        // ---------- the written page, read back off the disk ----------

        private static ClashReport Report()
        {
            ClashReport report = new ClashReport("1C07BC", OutputName);
            report.SetTreeRoot = Root;
            report.DocumentUnits = "m";
            report.RunAt = new DateTime(2026, 9, 1, 12, 0, 0);

            TestReport test = report.AddTest("BLD-ST-Walls-vs-BLD-AR-Floors");
            test.LeftLocator = Root + "/Structural/BLD-ST-Walls";
            test.RightLocator = Root + "/Architecture/BLD-AR-Floors";
            test.Tolerance = 0.025;
            test.ToleranceUnits = "m";
            test.TestTypeName = "hard_conservative";
            test.StatusWord = "OK";
            test.State = TestState.FoundClashes;

            ClashRow row = new ClashRow();
            row.Name = "Clash1";
            row.Status = ClashStatus.New;
            row.Distance = -0.05;
            row.GridLocation = "B-1";
            row.Level = "LGF";
            row.Description = "Hard (Conservative)";
            row.X = -2.1;
            row.Y = -2.726;
            row.Z = -0.051;
            row.Left.ElementId = "2635048";
            row.Left.Name = "Concrete";
            row.Left.ItemType = "Solid";
            row.Right.ElementId = "814542";
            row.Right.Name = "TRENCH";
            row.Right.ItemType = "Solid";
            test.Add(row);

            return report;
        }

        private string WritePage(string logoHref)
        {
            string stylesheet = InstallFiles.FindStylesheet(Install, "en-US");

            if (stylesheet.Length == 0)
            {
                Assert.Ignore("Navisworks is not on this machine, so its stylesheet cannot be read.");
            }

            ClashReportXml writer = new ClashReportXml();
            writer.LogoHref = logoHref;

            string path = HtmlTabularWriter.PathFor(Workbook());
            new HtmlTabularWriter().Write(writer.Build(Report()), stylesheet, path);

            return path;
        }

        /// <summary>
        /// The logo's src as the written file actually holds it, read straight out of the
        /// page rather than off the object model. The broken image link was caught this
        /// way before, because the object model said it was fine while the file was wrong.
        /// </summary>
        private static string LogoSrcInTheFile(string page)
        {
            Match found = Regex.Match(
                File.ReadAllText(page), "<img[^>]*alt=\"logo\"[^>]*>", RegexOptions.IgnoreCase);

            Assert.That(found.Success, Is.True, "the page has no logo img at all");

            Match src = Regex.Match(found.Value, "src=\"([^\"]*)\"", RegexOptions.IgnoreCase);

            return src.Success ? src.Groups[1].Value : string.Empty;
        }

        // The one the brief asks for by name.
        [Test]
        public void TheWrittenPageReferencesTheLogoRelatively()
        {
            string page = WritePage(ImageNaming.LogoLinkFor(Workbook()));
            string src = LogoSrcInTheFile(page);

            // A backslash, the same as every picture reference and the same as theirs.
            Assert.That(src, Is.EqualTo(OutputName + "_files" + "\\" + "logo.jpg"));
            Assert.That(src, Does.Not.Contain("file:///"),
                "the accepted xlsx does this and its pictures break everywhere else");
            Assert.That(src, Does.Not.Contain(":"));
            Assert.That(src, Does.Not.Contain(folder),
                "the page points at the machine that wrote it");
        }

        [Test]
        public void NoAbsolutePathOfAnyKindReachesThePage()
        {
            string page = WritePage(ImageNaming.LogoLinkFor(Workbook()));
            string html = File.ReadAllText(page);

            Assert.That(html, Does.Not.Contain("file:///"));
            Assert.That(html, Does.Not.Contain(folder));
            Assert.That(html, Does.Not.Contain(Install));
        }

        // ---------- a missing logo never stops a report ----------

        // The one the brief asks for by name.
        [Test]
        public void ThePageIsStillWrittenWhenThereIsNoLogo()
        {
            string page = WritePage(string.Empty);

            Assert.That(File.Exists(page), Is.True, "the report was lost with the logo");
            Assert.That(new FileInfo(page).Length, Is.GreaterThan(0));

            string html = File.ReadAllText(page);

            Assert.That(html, Does.Contain("Clash1"), "the report still has its content");
            Assert.That(html, Does.Contain("BLD-ST-Walls-vs-BLD-AR-Floors"));
            Assert.That(LogoSrcInTheFile(page), Is.Empty,
                "no logo element means an empty src, which shows no picture");
        }

        [Test]
        public void NoLogoElementIsWrittenWhenThereIsNoLogo()
        {
            ClashReportXml writer = new ClashReportXml();

            Assert.That(writer.Build(Report()).Root.Element("logo"), Is.Null);
        }

        [Test]
        public void ItNamesEveryPathItLookedAtWhenItCannotFindOne()
        {
            string block = string.Join("\n",
                new List<string>(InstallFiles.WhyNoLogo(@"C:\Nowhere", "fr-FR")).ToArray());

            Assert.That(block, Does.Contain(@"C:\Nowhere\Images\logo.jpg"));
            Assert.That(block, Does.Contain(@"C:\Nowhere\fr-FR\Images\logo.jpg"));
            Assert.That(block, Does.Contain(@"C:\Nowhere\en-US\Images\logo.jpg"));
            Assert.That(block, Does.Contain("written without one"));
            Assert.That(block, Does.Contain("unaffected"));
        }

        [Test]
        public void NoInstallFolderSaysSoRatherThanListingNothing()
        {
            string block = string.Join("\n",
                new List<string>(InstallFiles.WhyNoLogo(string.Empty, "en-US")).ToArray());

            Assert.That(block, Does.Contain("No install folder was given"));
        }

        // ---------- what the logo does to the Image column ----------

        // Worth pinning, because the stylesheet's Image column is boolean(//@href) and the
        // logo carries an href too. So a page with a logo and no clash pictures shows an
        // empty Image column. That is Autodesk's own behaviour, not something added here,
        // and their own reports do the same.
        [Test]
        public void ALogoAloneTurnsOnTheStylesheetsImageColumn()
        {
            string page = WritePage(ImageNaming.LogoLinkFor(Workbook()));

            Assert.That(File.ReadAllText(page), Does.Contain(">Image</td>"),
                "the logo's own href satisfies the stylesheet's image test");
        }
    }
}
