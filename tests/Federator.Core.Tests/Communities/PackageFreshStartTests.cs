using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Federator.Core.Communities;
using Federator.Core.Diagnostics;
using NUnit.Framework;

namespace Federator.Core.Tests
{
    /// <summary>
    /// F214, Bader's answer to Q204 of 18 Oct 2026: a fresh Navisworks per package. The order the parent hands every
    /// package's Navisworks, the result each writes back, the RESULT block lifted out of a package's log and the one
    /// RESULT the parent joins. Each file is written to the disk and read back off it, and each check is broken one thing
    /// at a time.
    /// </summary>
    [TestFixture]
    public class PackageFreshStartTests
    {
        private string root;

        [SetUp]
        public void MakeRoot()
        {
            root = TempFolder.Make("freshstart");
        }

        [TearDown]
        public void RemoveRoot()
        {
            TempFolder.Remove(root);
        }

        private PackageRunOrder Order()
        {
            PackageRunOrder order = new PackageRunOrder
            {
                Page = PackageRunOrder.NwcPage,
                Parent = TestPaths.At("work", "multi2", "Parent"),
                Report = TestPaths.At("work", "multi2", "Report"),
                Folder = root,
                PluginId = "ParsonsNwcFederator.PARS",
                ParentLog = TestPaths.At("logs", "run-20261018-110000.log")
            };

            order.Packages.Add("C06");
            order.Packages.Add("C07\\Infrastructure and Roads");
            order.Controls.Add(new ControlValue(ControlValue.TextKind, "NwfLevel", "ZZZ"));
            order.Controls.Add(new ControlValue(ControlValue.TextKind, "LogoBox", "C:\\a\tb\\logo.jpg\r\nsecond line"));
            order.Controls.Add(new ControlValue(ControlValue.TickKind, "MakeViewpoints", "True"));
            order.Controls.Add(new ControlValue(ControlValue.ChoiceKind, "ToleranceBox", "0"));
            return order;
        }

        private void WriteOrderText(string text)
        {
            File.WriteAllText(Path.Combine(root, PackageRunOrder.FileName), text, new UTF8Encoding(false));
        }

        [Test]
        public void AnOrderReadsBackEveryValueWholeThroughTabsAndLineBreaks()
        {
            PackageRunOrder written = Order();
            written.Settings.CeilingMinutes = 90;
            written.Write();

            string why;
            PackageRunOrder read = PackageRunOrder.Read(written.OrderPath, out why);

            Assert.That(why, Is.Null);
            Assert.That(read.Page, Is.EqualTo(PackageRunOrder.NwcPage));
            Assert.That(read.Parent, Is.EqualTo(written.Parent));
            Assert.That(read.Report, Is.EqualTo(written.Report));
            Assert.That(read.Folder, Is.EqualTo(root));
            Assert.That(read.PluginId, Is.EqualTo("ParsonsNwcFederator.PARS"));
            Assert.That(read.ParentLog, Is.EqualTo(written.ParentLog));
            Assert.That(read.Settings.CeilingMinutes, Is.EqualTo(90));
            Assert.That(read.Settings.GoneWithinSeconds, Is.EqualTo(PackageRunSettings.DefaultGoneWithinSeconds));
            Assert.That(read.Packages.Count, Is.EqualTo(2));
            Assert.That(read.Packages[1], Is.EqualTo("C07\\Infrastructure and Roads"));
            Assert.That(read.Controls.Count, Is.EqualTo(4));

            for (int i = 0; i < 4; i++)
            {
                Assert.That(read.Controls[i].Kind, Is.EqualTo(written.Controls[i].Kind));
                Assert.That(read.Controls[i].Name, Is.EqualTo(written.Controls[i].Name));
                Assert.That(read.Controls[i].Value, Is.EqualTo(written.Controls[i].Value));
            }
        }

        [Test]
        public void EachPackageHasItsOwnResultFileInTheRunsFolder()
        {
            PackageRunOrder order = Order();

            Assert.That(order.ResultPathFor(0), Is.EqualTo(Path.Combine(root, "result-001.txt")));
            Assert.That(order.ResultPathFor(1), Is.EqualTo(Path.Combine(root, "result-002.txt")));
            Assert.That(order.StatusPath, Is.EqualTo(Path.Combine(root, PackageRunOrder.StatusFileName)));
        }

        [Test]
        public void ARunsFolderNeverLandsOnOneAlreadyThere()
        {
            DateTime now = new DateTime(2026, 10, 18, 11, 0, 5);
            HashSet<string> there = new HashSet<string> { Path.Combine(root, "20261018-110005"), Path.Combine(root, "20261018-110005-2") };

            Assert.That(PackageRunOrder.FolderFor(root, now, p => false), Is.EqualTo(Path.Combine(root, "20261018-110005")));
            Assert.That(PackageRunOrder.FolderFor(root, now, there.Contains), Is.EqualTo(Path.Combine(root, "20261018-110005-3")));
        }

        [Test]
        public void AnOrderThatCannotRunIsNeverWritten()
        {
            PackageRunOrder order = Order();
            order.Packages.Clear();

            Assert.That(order.WhyNot(), Does.Contain("no package"));
            Assert.Throws<InvalidOperationException>(() => order.Write());

            order = Order();
            order.Page = "NWD";
            Assert.That(order.WhyNot(), Does.Contain("NWD"));
        }

        [Test]
        public void AnOrderFileBrokenOneWayAtATimeIsRefusedWithWhy()
        {
            PackageRunOrder order = Order();
            order.Write();
            string good = File.ReadAllText(order.OrderPath);
            string why;

            WriteOrderText(good.Replace(PackageRunOrder.Header, "another file"));
            Assert.That(PackageRunOrder.Read(order.OrderPath, out why), Is.Null);
            Assert.That(why, Does.Contain("does not start with"));

            WriteOrderText(good + "colour\tred\r\n");
            Assert.That(PackageRunOrder.Read(order.OrderPath, out why), Is.Null);
            Assert.That(why, Does.Contain("colour"));

            WriteOrderText(good.Replace("ceiling\t" + PackageRunSettings.DefaultCeilingMinutes, "ceiling\tsoon"));
            Assert.That(PackageRunOrder.Read(order.OrderPath, out why), Is.Null);
            Assert.That(why, Does.Contain("not a whole number"));

            WriteOrderText(good.Replace("ceiling\t" + PackageRunSettings.DefaultCeilingMinutes, "ceiling\t0"));
            Assert.That(PackageRunOrder.Read(order.OrderPath, out why), Is.Null);
            Assert.That(why, Does.Contain("refuse"));

            WriteOrderText(good.Replace("control\ttick\tMakeViewpoints\tTrue", "control\tslider\tMakeViewpoints\tTrue"));
            Assert.That(PackageRunOrder.Read(order.OrderPath, out why), Is.Null);
            Assert.That(why, Does.Contain("slider"));

            WriteOrderText(good.Replace("page\tNWC", "page"));
            Assert.That(PackageRunOrder.Read(order.OrderPath, out why), Is.Null);
            Assert.That(why, Does.Contain("holds 0 values"));

            Assert.That(PackageRunOrder.Read(Path.Combine(root, "none.txt"), out why), Is.Null);
            Assert.That(why, Does.Contain("not there"));
        }

        [Test]
        public void TheSettingsRefuseWhatCannotWork()
        {
            PackageRunSettings settings = new PackageRunSettings();

            Assert.That(settings.CeilingMinutes, Is.EqualTo(240));
            Assert.That(settings.GoneWithinSeconds, Is.EqualTo(60));
            Assert.Throws<ArgumentOutOfRangeException>(() => settings.CeilingMinutes = 0);
            Assert.Throws<ArgumentOutOfRangeException>(() => settings.GoneWithinSeconds = 0);
        }

        private static PackageRunResult Result()
        {
            PackageRunResult result = new PackageRunResult("C06", TestPaths.At("logs", "run-20261018-110010.log"));
            result.Groups.Add(new PackageRunGroup("1B06BC", GroupOutcome.Done, 812, null));
            result.Groups.Add(new PackageRunGroup("1B06KI", GroupOutcome.Failed, -1, "a.nwc could not be appended\tand more"));
            result.ResultLines.Add("10:21:18.577  +0044.309s  groups done    : 1");
            result.ResultLines.Add(string.Empty);
            result.ResultLines.Add("10:21:18.587  +0044.318s  groups failed  : 1");
            return result;
        }

        [Test]
        public void AResultReadsBackEveryGroupAndEveryLine()
        {
            string path = Path.Combine(root, "result-001.txt");
            Result().Write(path);

            string why;
            PackageRunResult read = PackageRunResult.Read(path, out why);

            Assert.That(why, Is.Null);
            Assert.That(read.Package, Is.EqualTo("C06"));
            Assert.That(read.Log, Does.EndWith("run-20261018-110010.log"));
            Assert.That(read.NoGroupBecause, Is.Null);
            Assert.That(read.Groups.Count, Is.EqualTo(2));
            Assert.That(read.Groups[0].Clashes, Is.EqualTo(812));
            Assert.That(read.Groups[0].Reason, Is.Null);
            Assert.That(read.Groups[1].Outcome, Is.EqualTo(GroupOutcome.Failed));
            Assert.That(read.Groups[1].Clashes, Is.EqualTo(-1), "a count not taken stays minus one and never nought");
            Assert.That(read.Groups[1].Reason, Is.EqualTo("a.nwc could not be appended\tand more"));
            Assert.That(read.ResultLines.Count, Is.EqualTo(3));
            Assert.That(read.ResultLines[1], Is.EqualTo(string.Empty));
        }

        [Test]
        public void AResultCutShortOrBrokenIsNoResultWithWhy()
        {
            string path = Path.Combine(root, "result-001.txt");
            Result().Write(path);
            string good = File.ReadAllText(path);
            string why;

            File.WriteAllText(path, good.Substring(0, good.IndexOf("end\t", StringComparison.Ordinal)), new UTF8Encoding(false));
            Assert.That(PackageRunResult.Read(path, out why), Is.Null);
            Assert.That(why, Does.Contain("part way"));

            File.WriteAllText(path, good.Replace("\tDone\t", "\tFine\t"), new UTF8Encoding(false));
            Assert.That(PackageRunResult.Read(path, out why), Is.Null);
            Assert.That(why, Does.Contain("Fine"));

            File.WriteAllText(path, good.Replace("\tDone\t", "\t0\t"), new UTF8Encoding(false));
            Assert.That(PackageRunResult.Read(path, out why), Is.Null, "an outcome is read by its name and never by its number");

            File.WriteAllText(path, good.Replace("\t812\t", "\t-5\t"), new UTF8Encoding(false));
            Assert.That(PackageRunResult.Read(path, out why), Is.Null);
            Assert.That(why, Does.Contain("not a count"));

            Assert.That(PackageRunResult.Read(Path.Combine(root, "result-009.txt"), out why), Is.Null);
            Assert.That(why, Does.Contain("not there"));
        }

        [Test]
        public void TheResultLinesAreEverythingAfterTheLastResultTitle()
        {
            List<string> log = new List<string>
            {
                "10:00:00.000  +0000.000s  GROUP    finished A",
                string.Empty,
                "================================================================",
                "RESULT",
                "================================================================",
                "first run's line",
                string.Empty,
                "================================================================",
                "RESULT",
                "================================================================",
                "groups done    : 3",
                "files written  : 9",
                string.Empty
            };

            IList<string> lines = PackageRunResult.ResultLinesOf(log);

            Assert.That(lines.Count, Is.EqualTo(2));
            Assert.That(lines[0], Is.EqualTo("groups done    : 3"));
            Assert.That(lines[1], Is.EqualTo("files written  : 9"));

            log[8] = "RESULT is a word in a line";
            log[3] = "RESULTS";
            Assert.That(PackageRunResult.ResultLinesOf(log).Count, Is.EqualTo(0), "a title that is not the title alone between two rules is not a RESULT block");
            Assert.That(PackageRunResult.ResultLinesOf(null).Count, Is.EqualTo(0));
        }

        [Test]
        public void TheJoinedResultCountsEveryPackagesGroupsAndNamesAPackageWithNoResult()
        {
            PackageRunResult c06 = Result();
            PackageRunResult c07 = new PackageRunResult("C07", null) { NoGroupBecause = "every group of it has an NWF already" };

            IList<string> lines = PackageRunWords.JoinedResult(
                new List<string> { "C06", "C07", "C08" },
                new List<PackageRunResult> { c06, c07, null },
                new List<string> { null, null, "its Navisworks ended before it wrote its result" },
                new List<string> { "run            : 3 packages" });

            Assert.That(lines, Does.Contain("groups done    : 1"));
            Assert.That(lines, Does.Contain("groups partial : 0"));
            Assert.That(lines, Does.Contain("groups failed  : 1"));
            Assert.That(lines, Does.Contain("run            : 3 packages"));
            Assert.That(string.Join("\n", lines), Does.Contain("no result      : 1 package"));
            Assert.That(lines, Does.Contain("PACKAGE C08 : NO RESULT, its Navisworks ended before it wrote its result"));
            Assert.That(lines, Does.Contain("    no group of it ran, every group of it has an NWF already"));
            Assert.That(lines, Does.Contain("    its log holds no RESULT block"));
            Assert.That(lines, Does.Contain("    10:21:18.587  +0044.318s  groups failed  : 1"));
            Assert.That(string.Join("\n", lines), Does.Contain("C07 : its own RESULT, from its log UNKNOWN"));
            Assert.Throws<ArgumentException>(() => PackageRunWords.JoinedResult(
                new List<string> { "C06" }, new List<PackageRunResult>(), new List<string>(), null));
        }

        [Test]
        public void TheRunnersLinesNameThePackageAndWhatHappened()
        {
            Assert.That(PackageRunWords.Starting(0, 2, "C06"), Is.EqualTo("package 1 of 2, C06, starting a fresh Navisworks"));
            Assert.That(PackageRunWords.Started("C06", 0, 4.31), Does.Contain("could not be told"));
            Assert.That(PackageRunWords.Ceiling("C06", 240), Does.Contain("240 minutes").And.Contain("next package"));
            Assert.That(PackageRunWords.Ended("C06", 60), Does.Contain("60 seconds").And.Contain("before the next package"));
            Assert.That(PackageRunWords.Gone("C06", 1.55), Does.Contain("1.6 s"));
            Assert.That(PackageRunWords.Threw("C06", "the run", "COMException", "gone"), Does.EndWith("The next package still runs"));
        }
    }
}
