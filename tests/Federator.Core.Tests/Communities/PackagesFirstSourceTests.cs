using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using NUnit.Framework;

namespace Federator.Core.Tests
{
    /// <summary>
    /// F232, Bader's round of 19 Oct 2026, fix 2: packages first, one route per page. The window cannot be built here, so
    /// what each page holds is read off its XAML and its code, as a test for a list read off a file reads the file. What a
    /// run showed of them is in steps\03_bader_next.md.
    /// </summary>
    [TestFixture]
    public class PackagesFirstSourceTests
    {
        private static string Xaml()
        {
            return File.ReadAllText(Path.Combine(Samples.Repo(), "src", "Federator.Addin", "Ui", "FederatorWindow.xaml"));
        }

        private static string Code(string name)
        {
            return File.ReadAllText(Path.Combine(Samples.Repo(), "src", "Federator.Addin", "Ui", name));
        }

        private static string NwcPage(string xaml)
        {
            return xaml.Substring(0, xaml.IndexOf("<TabItem Header=\"NWF\">", StringComparison.Ordinal));
        }

        private static string NwfPage(string xaml)
        {
            return xaml.Substring(xaml.IndexOf("<TabItem Header=\"NWF\">", StringComparison.Ordinal));
        }

        private static List<string> NamesIn(string part)
        {
            List<string> names = new List<string>();

            foreach (Match match in Regex.Matches(part, "x:Name=\"([^\"]+)\""))
            {
                names.Add(match.Groups[1].Value);
            }

            return names;
        }

        private static string Between(string text, string from, string to)
        {
            int start = text.IndexOf(from, StringComparison.Ordinal);
            int end = text.IndexOf(to, start, StringComparison.Ordinal);
            Assert.That(start, Is.GreaterThanOrEqualTo(0), from);
            Assert.That(end, Is.GreaterThan(start), to);
            return text.Substring(start, end - start);
        }

        [Test]
        public void TheNwcPageHasFourStepsPackagesFirstAndNoSourceStepOrStepFive()
        {
            string page = NwcPage(Xaml());
            int packages = page.IndexOf("x:Name=\"PackagesStep\"", StringComparison.Ordinal);
            int grouping = page.IndexOf("Header=\"2. Grouping\"", StringComparison.Ordinal);
            int outputs = page.IndexOf("Header=\"3. Outputs\"", StringComparison.Ordinal);
            int clash = page.IndexOf("Header=\"4. Clash\"", StringComparison.Ordinal);

            Assert.That(packages, Is.GreaterThan(0));
            Assert.That(grouping, Is.GreaterThan(packages));
            Assert.That(outputs, Is.GreaterThan(grouping));
            Assert.That(clash, Is.GreaterThan(outputs));
            Assert.That(Regex.Matches(Between(page, "x:Name=\"Steps\"", "</TabControl>"), "<TabItem").Count, Is.EqualTo(4));
            Assert.That(page, Does.Not.Contain("Header=\"1. Source\""));
            Assert.That(page, Does.Not.Contain("5. Packages"));
        }

        [Test]
        public void TheNwcPageHasOneRunButtonTheOneAtTheTopAndNoControlOfTheRemovedSteps()
        {
            string page = NwcPage(Xaml());

            Assert.That(page, Does.Contain("x:Name=\"RunButton\" Content=\"Run\" Click=\"OnRun\""));
            Assert.That(Regex.Matches(Xaml(), "x:Name=\"RunButton\"").Count, Is.EqualTo(1));

            foreach (string gone in new[]
            {
                "SourceFolderBox", "IncludeSubfolders", "FilesGrid", "SourceSummary", "RunPackagesButton",
                "NwfFolderBox", "NwdFolderBox", "OutputsGrid", "NamePreview", "OutputsSummary", "DateTheNwd", "WriteClashXml",
                "GhostOtherItems", "ImagePixelsBox", "ImageCapBox"
            })
            {
                Assert.That(Xaml(), Does.Not.Contain("x:Name=\"" + gone + "\""), gone);
            }
        }

        [Test]
        public void StepOneHoldsTheFoldersTheTreeAndTheFindingsUnderTheTree()
        {
            string step = Between(NwcPage(Xaml()), "x:Name=\"PackagesStep\"", "Header=\"2. Grouping\"");

            foreach (string id in new[] { "PackageParentBox", "ReadPackagesButton", "PackageReportBox", "TickAllPackagesButton", "ClearAllPackagesButton", "PackagesTree", "FindingsPanel", "FindingsBox", "CopyFindingsButton" })
            {
                Assert.That(step, Does.Contain("x:Name=\"" + id + "\""), id);
            }

            Assert.That(step, Does.Contain("Click=\"OnBrowsePackageParent\""));
            Assert.That(step, Does.Contain("Click=\"OnBrowsePackageReport\""));
            Assert.That(step.IndexOf("x:Name=\"FindingsPanel\"", StringComparison.Ordinal), Is.GreaterThan(step.IndexOf("x:Name=\"TickAllPackagesButton\"", StringComparison.Ordinal)));
            Assert.That(step, Does.Contain("<Border DockPanel.Dock=\"Bottom\" x:Name=\"FindingsPanel\""), "docked under the tree");
        }

        [Test]
        public void StepTwoShowsCommunityPackageBuildingFilesDisciplinesRunAsAndStatus()
        {
            string step = Between(NwcPage(Xaml()), "Header=\"2. Grouping\"", "Header=\"3. Outputs\"");
            string[] headers = { "Run", "Community", "Package", "Building", "Files", "Disciplines", "Run as", "Status" };
            int at = 0;

            foreach (string header in headers)
            {
                int found = step.IndexOf("Header=\"" + header + "\"", at, StringComparison.Ordinal);
                Assert.That(found, Is.GreaterThan(at), header);
                at = found;
            }

            Assert.That(step, Does.Contain("x:Name=\"GroupingModeBox\""));
            Assert.That(step, Does.Contain("x:Name=\"RebuildExisting\""));
        }

        [Test]
        public void StepThreeHoldsTheNamingBoxesAndTheLogoOnly()
        {
            string step = Between(NwcPage(Xaml()), "Header=\"3. Outputs\"", "Header=\"4. Clash\"");
            List<string> expected = new List<string> { "OutputsHeading" };

            foreach (string row in new[] { "Nwf", "Nwd", "Workbook" })
            {
                foreach (string field in new[] { "Level", "Discipline", "Type", "Number", "AllBuildings" })
                {
                    expected.Add(row + field);
                }
            }

            expected.Add("LogoBox");
            List<string> names = NamesIn(step);

            Assert.That(names.Count, Is.EqualTo(expected.Count), string.Join(", ", names.ToArray()));

            for (int i = 0; i < expected.Count; i++)
            {
                Assert.That(names[i], Is.EqualTo(expected[i]));
            }
        }

        [Test]
        public void TheNwfPageHasPackagesFirstThenTheToleranceTheFiveStepsAndOneRun()
        {
            string page = NwfPage(Xaml());
            string[] inOrder =
            {
                "NwfPackageReportBox", "NwfReadPackagesButton", "NwfTickAllPackagesButton", "NwfClearAllPackagesButton", "NwfPackagesTree",
                "NwfToleranceBox", "NwfStepRefresh", "NwfStepTests", "NwfStepPictures", "NwfStepViewpoints", "NwfStepReports", "RunNwfPackagesButton"
            };
            int at = 0;

            foreach (string id in inOrder)
            {
                int found = page.IndexOf("x:Name=\"" + id + "\"", StringComparison.Ordinal);
                Assert.That(found, Is.GreaterThan(at), id);
                at = found;
            }

            Assert.That(page, Does.Contain("Click=\"OnRunNwfPackages\""));
            Assert.That(page, Does.Contain("ItemsSource=\"{Binding NwfNodes}\""));
            Assert.That(Regex.Matches(page, "<Button[^>]*FontWeight=\"Bold\"").Count, Is.EqualTo(2), "Read and Run");

            foreach (string gone in new[] { "PickedNwfBox", "RecentNwfBox", "PickedNwfLabel", "RunPickedNwfsButton", "NwfPackageParentBox", "NwfPageHeading" })
            {
                Assert.That(page, Does.Not.Contain("x:Name=\"" + gone + "\""), gone);
            }

            Assert.That(page, Does.Not.Contain("Click=\"OnBrowsePickedNwfFile\""));
            Assert.That(page, Does.Not.Contain("DockPanel.Dock=\"Right\""), "no packages panel on the right");
        }

        [Test]
        public void BothTreesDrawALineOneWay()
        {
            string xaml = Xaml();

            Assert.That(xaml.Split(new[] { "ItemTemplate=\"{StaticResource PackageLine}\"" }, StringSplitOptions.None).Length - 1, Is.EqualTo(2));
        }

        [Test]
        public void TheNwfPagesRunAsksForCancelFirstAndFillsTheTableFromTheRun()
        {
            string packages = Code("FederatorWindow.Packages.cs");

            Assert.That(packages, Does.Contain("MessageBoxButton.OKCancel, MessageBoxImage.Question, MessageBoxResult.Cancel"));
            Assert.That(packages, Does.Contain("ShowTheNwfTable(outcomes);"));
            Assert.That(packages, Does.Contain("nwfSteps.Apply(options);"));
            Assert.That(packages, Does.Contain("RunPackagesInFreshStarts(PackageRunOrder.NwfPage, report, run, units);"));
            Assert.That(packages, Does.Contain("RunPackagesInFreshStarts(PackageRunOrder.NwcPage, packageTree.Parent, run, units);"));
        }
    }
}
