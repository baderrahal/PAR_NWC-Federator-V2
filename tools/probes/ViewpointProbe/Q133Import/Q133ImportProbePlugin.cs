using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;
using Autodesk.Navisworks.Api;
using Autodesk.Navisworks.Api.Clash;
using Autodesk.Navisworks.Api.DocumentParts;
using Autodesk.Navisworks.Api.Plugins;
using Federator.Addin.Engine;
using Federator.Core.Clash;
using Federator.Core.Diagnostics;
using Federator.Core.Exchange;
using Federator.Core.Sets;

namespace Federator.Addin
{
    /// <summary>
    /// NavisworksFacts names this type for the build stamp. It is the add-in's plugin class in
    /// src, and here it is an empty stand-in, so the probe carries no second plugin under the
    /// add-in's name.
    /// </summary>
    internal sealed class FederatorPlugin
    {
    }
}

namespace Q133ImportProbe
{
    /// <summary>
    /// Q133 on 1A04PK, 2026-10-07. The NWF of set 04 holds the models and no sets and no tests,
    /// so the picked XML is brought in first the way the add-in brings it in: read with
    /// MatrixCorrections.ReadPicked, which applies the corrections list beside it, the sets made
    /// by SetBuilder, the tests the tool would create chosen by ClashRunner's PlanTheCreation and
    /// each made by ClashRunner's Create, both called on a real ClashRunner. Then every created
    /// test, in the order the XML lists them, is run with TestsRunTest timed alone, and each one
    /// that finds at least one clash not Resolved gets a swap made beside it the way P1 and the
    /// 1A02MM probe made theirs, run, timed and compared by the unordered pair of item index
    /// paths. No new test is started once the cap has passed. Part 1, the rule's pairs, is read
    /// at the end from the results already in memory, so each of its tests ran once.
    /// </summary>
    [Plugin(PluginName, DeveloperCode, DisplayName = "Q133 import probe", ToolTip = "Q133 on a building whose NWF holds no tests")]
    [AddInPlugin(AddInLocation.AddIn)]
    public sealed class Q133ImportProbePlugin : AddInPlugin
    {
        public const string PluginName = "Q133ImportProbe";
        public const string DeveloperCode = "PARS";

        private static readonly CultureInfo Inv = CultureInfo.InvariantCulture;

        private StreamWriter results;

        public override int Execute(params string[] parameters)
        {
            if (parameters == null || parameters.Length < 7 || parameters[0] != "q133import")
            {
                return 2;
            }

            using (results = new StreamWriter(parameters[1], true, new UTF8Encoding(false)))
            {
                results.AutoFlush = true;
                Say("probe started, mode " + parameters[0]);

                try
                {
                    double cap = double.Parse(parameters[5], Inv);
                    Measure(parameters[2], parameters[3], parameters[4], cap, parameters[6]);
                }
                catch (Exception error)
                {
                    Exception inner = error is TargetInvocationException && error.InnerException != null ? error.InnerException : error;
                    Say("THREW " + inner.GetType().Name + ": " + inner.Message);
                    Say(inner.StackTrace ?? string.Empty);
                    return 1;
                }

                Say("probe finished");
            }

            return 0;
        }

        private void Say(string line)
        {
            results.WriteLine(DateTime.Now.ToString("HH:mm:ss.fff") + "  " + line);
        }

        private static string S(double seconds)
        {
            return seconds.ToString("0.000", Inv);
        }

        private void Measure(string nwf, string xml, string pairsFile, double capSeconds, string logFolder)
        {
            Document document = Autodesk.Navisworks.Api.Application.ActiveDocument;

            if (document == null)
            {
                Say("UNKNOWN: no active document in this host");
                return;
            }

            Say("opening " + Path.GetFileName(nwf));
            System.Diagnostics.Stopwatch clock = System.Diagnostics.Stopwatch.StartNew();
            bool opened = document.TryOpenFile(nwf);
            Say("TryOpenFile returned " + opened + " after " + S(clock.Elapsed.TotalSeconds) + " s");

            if (!opened)
            {
                Say("UNKNOWN: TryOpenFile returned false");
                return;
            }

            string loopRoot = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "NwcFederatorLoop") + "\\";
            Say("models " + document.Models.Count + ", document units " + document.Units);

            for (int m = 0; m < document.Models.Count; m++)
            {
                Model model = document.Models[m];
                Say("   model " + m + "  " + Path.GetFileName(model.FileName)
                    + "  under the loop folder " + (model.FileName ?? string.Empty).StartsWith(loopRoot, StringComparison.OrdinalIgnoreCase));
            }

            DocumentClashTests clashTests = document.GetClash().TestsData;
            Say("BEFORE THE IMPORT  sets in the document " + CountSets(document.SelectionSets.RootItem.Children)
                + ", clash tests at the root " + clashTests.Tests.Count + ", saved viewpoints at the root " + document.SavedViewpoints.Value.Count);
            Say(string.Empty);

            using (RunLog log = RunLog.Start(logFolder, DateTime.Now, 5))
            {
                MeasureWithLog(document, clashTests, xml, pairsFile, capSeconds, log);
            }
        }

        private void MeasureWithLog(Document document, DocumentClashTests clashTests, string xml, string pairsFile, double capSeconds, RunLog log)
        {
            // ---- The import, the add-in's way ----
            Say("==== THE IMPORT, the add-in's own code ====");
            System.Diagnostics.Stopwatch clock = System.Diagnostics.Stopwatch.StartNew();
            ExchangeDocument exchange = MatrixCorrections.ReadPicked(xml);
            Say("MatrixCorrections.ReadPicked took " + S(clock.Elapsed.TotalSeconds) + " s: sets in the file " + exchange.Sets.Count
                + ", tests in the file " + exchange.Tests.Count + ", units " + exchange.Units);

            foreach (string line in exchange.Corrections)
            {
                Say("   correction  " + line);
            }

            SetBuildPlan setPlan = SetBuildPlan.From(exchange);
            Say("SetBuildPlan  to build " + setPlan.Buildable.Count + ", skipped " + setPlan.Skipped.Count);

            foreach (string unknown in setPlan.UnknownTestValues)
            {
                Say("   condition test not rebuilt: " + unknown);
            }

            clock = System.Diagnostics.Stopwatch.StartNew();
            SetBuildOutcome sets = new SetBuilder(delegate { }, log, new SetRebuildSettings()).Build(setPlan);
            Say("SetBuilder.Build took " + S(clock.Elapsed.TotalSeconds) + " s: " + sets.Summary());
            Say("   created " + sets.CreatedCount + ", already there " + sets.AlreadyPresentCount + ", finding items " + sets.FindingItemsCount
                + ", at zero " + sets.ZeroCount + ", failed " + sets.FailedCount + ", skipped " + sets.SkippedCount
                + ", damaged " + (string.IsNullOrEmpty(sets.TheDocumentIsDamaged) ? "no" : sets.TheDocumentIsDamaged));
            Say("AFTER THE SETS  sets in the document " + CountSets(document.SelectionSets.RootItem.Children));

            string units = ClashRunner.DocumentUnits();
            ClashTestPlan plan = ClashTestPlan.From(exchange, units);
            Say("ClashTestPlan.From  tests in the file " + plan.TestsInFile + ", buildable " + plan.Buildable.Count + ", skipped before the model "
                + plan.Skipped.Count + ", document units " + units);

            ClashRunner runner = new ClashRunner(delegate { }, log, new RepeatedFailureGuard());
            Type rt = typeof(ClashRunner);
            BindingFlags hidden = BindingFlags.NonPublic | BindingFlags.Instance;
            rt.GetField("documentUnits", hidden).SetValue(runner, ClashRunner.UnitName(document.Units));
            MethodInfo planTheCreation = rt.GetMethod("PlanTheCreation", hidden);
            MethodInfo create = rt.GetMethod("Create", hidden, null,
                new[] { typeof(DocumentSelectionSets), typeof(DocumentClashTests), typeof(Dictionary<string, SelectionSet>), typeof(PlannedClashTest) }, null);
            MethodInfo release = rt.GetMethod("ReleaseTheIndex", hidden);

            if (planTheCreation == null || create == null || release == null)
            {
                Say("UNKNOWN: ClashRunner does not carry PlanTheCreation, Create and ReleaseTheIndex as read, nothing is created");
                return;
            }

            DocumentSelectionSets docSets = document.SelectionSets;
            Dictionary<string, SelectionSet> byPath = runner.IndexSets(docSets);

            try
            {
                ClashTestPlan resolved = plan.ResolveAgainst(byPath.Keys);
                Dictionary<string, TestAddress> present = runner.IndexTests(clashTests);
                ClashRunOutcome outcome = new ClashRunOutcome();
                outcome.TestsInFile = plan.TestsInFile;
                IList<PlannedClashTest> toRun = (IList<PlannedClashTest>)planTheCreation.Invoke(runner, new object[] { document, byPath, present, resolved, outcome });
                Say("the document's sets indexed " + byPath.Count + ", the plan resolved against them: buildable " + resolved.Buildable.Count
                    + ", skipped " + resolved.Skipped.Count + ", tests already in the document " + present.Count);
                Say("PlanTheCreation  the tool would create " + toRun.Count + " of " + plan.TestsInFile + ", not created as a side finds nothing "
                    + outcome.NotCreatedASideFindsNothing);
                Say(string.Empty);

                Dictionary<string, int> nameCount = new Dictionary<string, int>(StringComparer.Ordinal);

                foreach (PlannedClashTest p in toRun)
                {
                    int had;
                    nameCount.TryGetValue(p.Name, out had);
                    nameCount[p.Name] = had + 1;
                }

                int repeated = 0;

                foreach (KeyValuePair<string, int> pair in nameCount)
                {
                    if (pair.Value > 1)
                    {
                        repeated++;
                    }
                }

                Say("names the tool would create more than once: " + repeated);

                // ---- Part 2, every created test in XML order, its swap where it finds a clash ----
                Say("==== PART 2. Every test the tool would create, in XML order, run, and its swap made and run where it finds a clash. Cap " + S(capSeconds) + " s ====");
                Dictionary<string, Ran> ran = new Dictionary<string, Ran>(StringComparer.Ordinal);
                Totals all = new Totals();
                System.Diagnostics.Stopwatch loop = System.Diagnostics.Stopwatch.StartNew();
                int index = 0;
                int notReached = 0;

                foreach (PlannedClashTest planned in toRun)
                {
                    index++;

                    if (loop.Elapsed.TotalSeconds >= capSeconds)
                    {
                        notReached++;
                        continue;
                    }

                    OneTest(document, clashTests, docSets, byPath, runner, create, planned, index, ran, all);
                }

                Say("PART 2 LOOP  " + S(loop.Elapsed.TotalSeconds) + " s, tests measured " + all.Measured + " of " + toRun.Count + ", not reached for the cap " + notReached);
                Say("PART 2 TOTAL  originals run " + all.Measured + ", finding a clash " + all.WithClash + ", finding none " + all.NoClash + ", UNKNOWN " + all.Unknown);
                Say("PART 2 TOTAL  of the " + all.WithClash + " that find a clash, the swap finds the same " + all.Same + ", more " + all.More
                    + ", fewer " + all.Fewer + ", other clashes " + all.Other + ", UNKNOWN " + all.SwapUnknown);
                Say("PART 2 TOTAL  clashes, the originals that find one " + all.OriginalClashes + ", their swaps " + all.SwapClashes
                    + ", only the swap finds " + all.OnlySwap + ", only the original finds " + all.OnlyOriginal);
                Say("PART 2 TOTAL  seconds of TestsRunTest, the originals that find a clash " + S(all.OriginalSecondsWithClash)
                    + ", their swaps " + S(all.SwapSeconds) + ", both " + S(all.OriginalSecondsWithClash + all.SwapSeconds)
                    + ", the originals that find none " + S(all.OriginalSecondsNoClash)
                    + ", every original " + S(all.OriginalSecondsWithClash + all.OriginalSecondsNoClash)
                    + ", every original and every swap " + S(all.OriginalSecondsWithClash + all.OriginalSecondsNoClash + all.SwapSeconds));
                Say("PART 2 TOTAL  seconds making the originals with the tool's Create " + S(all.CreateSeconds) + ", making the swaps " + S(all.SwapMakeSeconds));
                Say(string.Empty);

                Part1(pairsFile, nameCount, ran);
                Say("tests at the root at the end " + clashTests.Tests.Count);
            }
            finally
            {
                release.Invoke(runner, new object[] { byPath });
            }
        }

        private void OneTest(
            Document document,
            DocumentClashTests clashTests,
            DocumentSelectionSets docSets,
            Dictionary<string, SelectionSet> byPath,
            ClashRunner runner,
            MethodInfo create,
            PlannedClashTest planned,
            int index,
            Dictionary<string, Ran> ran,
            Totals all)
        {
            string head = "LINE " + index.ToString(Inv) + "  \"" + planned.Name + "\"";
            System.Diagnostics.Stopwatch clock = System.Diagnostics.Stopwatch.StartNew();
            TestAddress address;

            try
            {
                address = (TestAddress)create.Invoke(runner, new object[] { docSets, clashTests, byPath, planned });
            }
            catch (Exception error)
            {
                Exception inner = error is TargetInvocationException && error.InnerException != null ? error.InnerException : error;
                all.Measured++;
                all.Unknown++;
                Say(head + "  UNKNOWN, the tool's Create threw " + inner.GetType().Name + ": " + inner.Message);
                return;
            }

            double made = clock.Elapsed.TotalSeconds;
            all.CreateSeconds += made;

            if (address == null)
            {
                all.Measured++;
                all.Unknown++;
                Say(head + "  UNKNOWN, the tool's Create returned no address");
                return;
            }

            List<int> at = new List<int>();

            for (int level = 0; level < address.Depth; level++)
            {
                at.Add(address.IndexAt(level));
            }

            double s1;
            PairsFound original = RunAndRead(document, clashTests, at, planned.Name, out s1);
            all.Measured++;

            if (original == null)
            {
                all.Unknown++;
                Say(head + "  UNKNOWN, the test was not at its address when run");
                return;
            }

            ran[planned.Name] = new Ran { Found = original, Seconds = s1 };

            if (original.Open.Count == 0)
            {
                all.NoClash++;
                all.OriginalSecondsNoClash += s1;
                Say(head + "  made in " + S(made) + " s, original 0 in " + S(s1) + " s, results read " + original.Leaves
                    + ", items that did not read " + original.NullItems + ", no clash so no swap");
                return;
            }

            all.WithClash++;
            all.OriginalSecondsWithClash += s1;
            all.OriginalClashes += original.Open.Count;

            clock = System.Diagnostics.Stopwatch.StartNew();
            string swapName = planned.Name + " Q133 swap";
            List<int> swapAt;
            bool sidesSwapped;
            string why;

            try
            {
                why = AddSwap(document, clashTests, at, planned.Name, swapName, out swapAt, out sidesSwapped);
            }
            catch (Exception error)
            {
                why = "the swap threw " + error.GetType().Name + ": " + error.Message;
                swapAt = null;
                sidesSwapped = false;
            }

            double swapMade = clock.Elapsed.TotalSeconds;
            all.SwapMakeSeconds += swapMade;

            if (why != null)
            {
                all.SwapUnknown++;
                Say(head + "  made in " + S(made) + " s, original " + original.Open.Count + " in " + S(s1) + " s, swap UNKNOWN, " + why);
                return;
            }

            double s2;
            PairsFound swapped = RunAndRead(document, clashTests, swapAt, swapName, out s2);

            if (swapped == null)
            {
                all.SwapUnknown++;
                Say(head + "  made in " + S(made) + " s, original " + original.Open.Count + " in " + S(s1) + " s, swap UNKNOWN, not at its address when run");
                return;
            }

            all.SwapSeconds += s2;
            all.SwapClashes += swapped.Open.Count;
            Diff d = Compare(original, swapped);
            string verdict;

            if (original.NullItems > 0 || swapped.NullItems > 0 || !sidesSwapped)
            {
                verdict = "UNKNOWN";
                all.SwapUnknown++;
            }
            else if (d.OnlyLeft.Count == 0 && d.OnlyRight.Count == 0)
            {
                verdict = "same";
                all.Same++;
            }
            else if (d.OnlyLeft.Count == 0)
            {
                verdict = "swap finds more";
                all.More++;
            }
            else if (d.OnlyRight.Count == 0)
            {
                verdict = "swap finds fewer";
                all.Fewer++;
            }
            else
            {
                verdict = "other clashes";
                all.Other++;
            }

            all.OnlySwap += d.OnlyRight.Count;
            all.OnlyOriginal += d.OnlyLeft.Count;

            Say(head + "  made in " + S(made) + " s, original " + original.Open.Count + " in " + S(s1) + " s"
                + ", swap " + swapped.Open.Count + " in " + S(s2) + " s, swap made in " + S(swapMade) + " s"
                + ", in both " + d.Both + ", original only " + d.OnlyLeft.Count + ", swap only " + d.OnlyRight.Count
                + ", items that did not read " + original.NullItems + " and " + swapped.NullItems
                + ", sides read swapped " + sidesSwapped + ", " + verdict);

            foreach (string k in d.OnlyLeft)
            {
                Say("      only in the original: " + k);
            }

            foreach (string k in d.OnlyRight)
            {
                Say("      only in the swap: " + k);
            }
        }

        private void Part1(string pairsFile, Dictionary<string, int> created, Dictionary<string, Ran> ran)
        {
            Say("==== PART 1. The pairs F132's rule finds in the XMLs, read from the results above, each test run once ====");
            List<string[]> pairs = new List<string[]>();
            HashSet<string> seen = new HashSet<string>(StringComparer.Ordinal);

            if (string.IsNullOrEmpty(pairsFile) || !File.Exists(pairsFile))
            {
                Say("UNKNOWN: no pairs file was handed in, or it is not there");
                return;
            }

            foreach (string line in File.ReadAllLines(pairsFile, new UTF8Encoding(false)))
            {
                if (line.Length == 0)
                {
                    continue;
                }

                if (line.StartsWith("#", StringComparison.Ordinal))
                {
                    Say("   " + line);
                    continue;
                }

                string[] f = line.Split('\t');

                if (f.Length < 4)
                {
                    Say("   a line of the pairs file with " + f.Length + " fields, skipped: " + line);
                    continue;
                }

                if (seen.Add(f[1] + "\n" + f[2] + "\n" + f[3]))
                {
                    pairs.Add(new[] { f[1], f[2], f[3] });
                }
            }

            int both = 0;
            int measured = 0;

            foreach (string[] p in pairs)
            {
                bool self = p[2].Length == 0;
                int first = created.ContainsKey(p[1]) ? created[p[1]] : 0;
                int second = self ? -1 : (created.ContainsKey(p[2]) ? created[p[2]] : 0);
                string head = "P1 PAIR  " + p[0] + "  \"" + p[1] + "\" created " + first
                    + (self ? string.Empty : ", \"" + p[2] + "\" created " + second);

                if (self)
                {
                    if (first != 1)
                    {
                        Say(head + "  NOT CREATED on this building, so not run");
                        continue;
                    }

                    both++;
                    Ran r;

                    if (!ran.TryGetValue(p[1], out r))
                    {
                        Say(head + "  not reached before the cap, UNKNOWN");
                        continue;
                    }

                    measured++;
                    Say(head + "  ran in " + S(r.Seconds) + " s, clashes " + r.Found.Open.Count);
                    continue;
                }

                if (first != 1 || second != 1)
                {
                    Say(head + "  NOT BOTH CREATED on this building, so not run");
                    continue;
                }

                both++;
                Ran one;
                Ran two;

                if (!ran.TryGetValue(p[1], out one) || !ran.TryGetValue(p[2], out two))
                {
                    Say(head + "  not both reached before the cap, UNKNOWN");
                    continue;
                }

                measured++;
                Diff d = Compare(one.Found, two.Found);
                Say(head + "  first " + one.Found.Open.Count + " in " + S(one.Seconds) + " s, second " + two.Found.Open.Count + " in "
                    + S(two.Seconds) + " s, in both " + d.Both + ", first only " + d.OnlyLeft.Count + ", second only " + d.OnlyRight.Count
                    + ", items that did not read " + one.Found.NullItems + " and " + two.Found.NullItems);

                foreach (string k in d.OnlyLeft)
                {
                    Say("      only in the first: " + k);
                }

                foreach (string k in d.OnlyRight)
                {
                    Say("      only in the second: " + k);
                }
            }

            Say("PART 1 TOTAL  pairs and self tests " + pairs.Count + ", with every test created " + both + ", measured " + measured);
            Say(string.Empty);
        }

        private static int CountSets(SavedItemCollection items)
        {
            int n = 0;

            for (int i = 0; i < items.Count; i++)
            {
                using (SavedItem item = items[i])
                {
                    if (item is SelectionSet)
                    {
                        n++;
                        continue;
                    }

                    GroupItem folder = item as GroupItem;

                    if (folder != null)
                    {
                        n += CountSets(folder.Children);
                    }
                }
            }

            return n;
        }

        // ---------- the same reads as ViewpointProbePlugin's P1 and Q133-1A02MM, copied ----------

        private sealed class PairsFound
        {
            public int Leaves;
            public int NullItems;
            public int Duplicates;
            public readonly Dictionary<string, string> Open = new Dictionary<string, string>(StringComparer.Ordinal);
            public readonly Dictionary<string, double> Distance = new Dictionary<string, double>(StringComparer.Ordinal);
        }

        private sealed class Ran
        {
            public PairsFound Found;
            public double Seconds;
        }

        private sealed class Diff
        {
            public int Both;
            public readonly List<string> OnlyLeft = new List<string>();
            public readonly List<string> OnlyRight = new List<string>();
        }

        private sealed class Totals
        {
            public int Measured;
            public int WithClash;
            public int NoClash;
            public int Unknown;
            public int Same;
            public int More;
            public int Fewer;
            public int Other;
            public int SwapUnknown;
            public int OriginalClashes;
            public int SwapClashes;
            public int OnlySwap;
            public int OnlyOriginal;
            public double OriginalSecondsWithClash;
            public double OriginalSecondsNoClash;
            public double SwapSeconds;
            public double CreateSeconds;
            public double SwapMakeSeconds;
        }

        private static ClashTest ResolveTest(DocumentClashTests clashTests, List<int> address)
        {
            SavedItemCollection children = clashTests.Tests;

            for (int level = 0; level < address.Count; level++)
            {
                int index = address[level];

                if (children == null || index < 0 || index >= children.Count)
                {
                    return null;
                }

                SavedItem item = children[index];

                if (level + 1 == address.Count)
                {
                    return item as ClashTest;
                }

                GroupItem folder = item as GroupItem;
                children = folder == null ? null : folder.Children;
            }

            return null;
        }

        /// <summary>Runs the test at the address, its TestsRunTest timed alone, and reads its results. Null when the name does not match.</summary>
        private static PairsFound RunAndRead(Document document, DocumentClashTests clashTests, List<int> address, string name, out double seconds)
        {
            seconds = -1;

            using (ClashTest test = ResolveTest(clashTests, address))
            {
                if (test == null || test.DisplayName != name)
                {
                    return null;
                }

                System.Diagnostics.Stopwatch clock = System.Diagnostics.Stopwatch.StartNew();
                clashTests.TestsRunTest(test);
                clock.Stop();
                seconds = clock.Elapsed.TotalSeconds;
            }

            using (ClashTest test = ResolveTest(clashTests, address))
            {
                if (test == null || test.DisplayName != name)
                {
                    return null;
                }

                PairsFound found = new PairsFound();
                ReadPairsUnder(document, test.Children, found);
                return found;
            }
        }

        private static void ReadPairsUnder(Document document, SavedItemCollection children, PairsFound found)
        {
            for (int i = 0; i < children.Count; i++)
            {
                using (SavedItem item = children[i])
                {
                    ClashResultGroup group = item as ClashResultGroup;

                    if (group != null)
                    {
                        ReadPairsUnder(document, group.Children, found);
                        continue;
                    }

                    ClashResult result = item as ClashResult;

                    if (result == null)
                    {
                        continue;
                    }

                    found.Leaves++;
                    string first = ItemPath(document, result.Item1);
                    string second = ItemPath(document, result.Item2);

                    if (first == null || second == null)
                    {
                        found.NullItems++;
                        continue;
                    }

                    if (result.Status == ClashResultStatus.Resolved)
                    {
                        continue;
                    }

                    string key = string.CompareOrdinal(first, second) <= 0 ? first + " | " + second : second + " | " + first;

                    if (found.Open.ContainsKey(key))
                    {
                        found.Duplicates++;
                        continue;
                    }

                    found.Open[key] = first + " | " + second;
                    found.Distance[key] = result.Distance;
                }
            }
        }

        private static string ItemPath(Document document, ModelItem item)
        {
            if (item == null)
            {
                return null;
            }

            using (item)
            {
                System.Collections.ObjectModel.Collection<int> path = document.Models.CreateIndexPath(item);
                string[] text = new string[path.Count];

                for (int i = 0; i < path.Count; i++)
                {
                    text[i] = path[i].ToString(Inv);
                }

                return string.Join(".", text);
            }
        }

        private static Diff Compare(PairsFound left, PairsFound right)
        {
            Diff d = new Diff();

            foreach (string key in left.Open.Keys)
            {
                if (right.Open.ContainsKey(key))
                {
                    d.Both++;
                }
                else
                {
                    d.OnlyLeft.Add(key + "   distance " + left.Distance[key].ToString("R", Inv));
                }
            }

            foreach (string key in right.Open.Keys)
            {
                if (!left.Open.ContainsKey(key))
                {
                    d.OnlyRight.Add(key + "   distance " + right.Distance[key].ToString("R", Inv));
                }
            }

            d.OnlyLeft.Sort(StringComparer.Ordinal);
            d.OnlyRight.Sort(StringComparer.Ordinal);
            return d;
        }

        private static string SideSets(Document document, ClashSelection side)
        {
            List<string> names = new List<string>();

            using (Selection selection = side.Selection)
            {
                SelectionSourceCollection sources = selection.SelectionSources;

                for (int i = 0; i < sources.Count; i++)
                {
                    try
                    {
                        using (SavedItem pointed = document.SelectionSets.ResolveSelectionSource(sources[i]))
                        {
                            names.Add(pointed == null ? "(a source that resolves to nothing)" : pointed.DisplayName);
                        }
                    }
                    catch (Exception error)
                    {
                        names.Add("(a source that threw " + error.GetType().Name + ")");
                    }
                }

                if (selection.HasExplicitSelection)
                {
                    names.Add("(explicit items)");
                }
            }

            return string.Join(" + ", names.ToArray());
        }

        /// <summary>
        /// A new ClashTest with the original's sides swapped at the end of the root, the way P1
        /// made its swap, its results cleared. Returns null and the swap's address, or why not.
        /// </summary>
        private static string AddSwap(Document document, DocumentClashTests clashTests, List<int> at, string name, string swapName, out List<int> swapAt, out bool sidesSwapped)
        {
            swapAt = null;
            sidesSwapped = false;
            int before = clashTests.Tests.Count;
            string originalA;
            string originalB;

            using (ClashTest original = ResolveTest(clashTests, at))
            {
                if (original == null || original.DisplayName != name)
                {
                    return "the original is not at its address";
                }

                if (original.IgnoreRules.Count != 0)
                {
                    return "the original carries " + original.IgnoreRules.Count + " ignore rules, which a new test does not";
                }

                using (ClashTest swap = new ClashTest())
                {
                    swap.DisplayName = swapName;
                    swap.TestType = original.TestType;
                    swap.Tolerance = original.Tolerance;
                    swap.MergeComposites = original.MergeComposites;
                    swap.SimulationType = original.SimulationType;

                    using (ClashSelection a = original.SelectionA)
                    using (ClashSelection b = original.SelectionB)
                    using (ClashSelection swapA = swap.SelectionA)
                    using (ClashSelection swapB = swap.SelectionB)
                    {
                        originalA = SideSets(document, a);
                        originalB = SideSets(document, b);
                        swapA.CopyFrom(b);
                        swapB.CopyFrom(a);
                        swapA.SelfIntersect = b.SelfIntersect;
                        swapB.SelfIntersect = a.SelfIntersect;
                        swapA.PrimitiveTypes = b.PrimitiveTypes;
                        swapB.PrimitiveTypes = a.PrimitiveTypes;
                    }

                    clashTests.TestsAddCopy(swap);
                }
            }

            int after = clashTests.Tests.Count;

            if (after != before + 1)
            {
                return "the root went from " + before + " to " + after + " tests, not one more";
            }

            List<int> added = new List<int> { before };

            using (ClashTest swap = ResolveTest(clashTests, added))
            {
                if (swap == null || swap.DisplayName != swapName)
                {
                    return "the last test at the root is not the swap";
                }

                using (ClashSelection a = swap.SelectionA)
                using (ClashSelection b = swap.SelectionB)
                {
                    sidesSwapped = SideSets(document, a) == originalB && SideSets(document, b) == originalA;
                }

                clashTests.TestsClearResults(swap);
            }

            swapAt = added;
            return null;
        }
    }
}
