using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Security;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Threading;
using Autodesk.Navisworks.Api;
using Autodesk.Navisworks.Api.ApplicationParts;
using Autodesk.Navisworks.Api.Clash;
using Autodesk.Navisworks.Api.Plugins;
using NavisworksApplication = Autodesk.Navisworks.Api.Application;

namespace DocumentReadProbe
{
    /// <summary>
    /// Reads what an NWF holds, test by test and result by result, into a text read-out,
    /// F104. tools\loop\compare-document.ps1 sets it beside the workbook's read-out, and
    /// the two files are all that pass between them.
    ///
    /// THE PARAMETERS. The first is the folder the read-outs go in, every one after it an
    /// NWF. The number handed back is a hint and the read-out files are the proof:
    ///
    ///     0   every read-out is whole, it ends in END OF READ-OUT
    ///     1   at least one says READ-OUT FAILED or REFUSED, or was there already
    ///     2   fewer than two parameters
    ///     3   the output folder is missing or not under %LOCALAPPDATA%\NwcFederatorLoop
    ///     4   a path gives no usable file name, or two NWFs share one
    ///
    /// Nothing is written on 2 to 4. The output folder and every NWF must sit under
    /// %LOCALAPPDATA%\NwcFederatorLoop, compared case blind with a trailing separator,
    /// which keeps every live folder out without naming one.
    ///
    /// THE READ-OUT, one per NWF, named after it with -document.txt. Written as .partial
    /// with every line flushed and moved into place once it is finished, so a read that
    /// stopped half way never leaves a file that looks whole. A read-out that is there
    /// already is never written over. Key lines are the key, a tab and the value. The two
    /// tables are tab separated under a line naming their columns, which is how the
    /// comparison finds a column, never by its position. A tab or a line break inside a
    /// name is written as one space, as read-workbook.ps1 does. Numbers are invariant and
    /// round trip. A count that could not be taken is -1, read as UNKNOWN and never zero.
    ///
    /// TWO PASSES. The document is read once straight after TryOpenFile and again after
    /// five seconds of pumping the dispatcher. Both totals are written and any line that
    /// differs between the passes is a doubt, because whether the document is whole on
    /// the first read in an automation start is UNKNOWN here. The tables are pass two's.
    ///
    /// WHAT IT NEVER DOES. TryOpenFile is the one call that changes what Navisworks holds.
    /// No save, publish, append or clear, none of the DocumentClashTests methods named Tests
    /// and a verb, TestsRunTest and TestsAddCopy among them, where the Tests property is the
    /// one member read. No copy, edit, move or remove, nothing hidden, no viewpoint set. Every
    /// SavedItem it reads is disposed before the loop that read it moves on, and a pass
    /// keeps strings and numbers only. DocumentClash and its TestsData are document parts
    /// and are not disposed. A throw is a doubt with its type and message, never a count.
    /// </summary>
    [Plugin(PluginName, DeveloperCode, DisplayName = "Document read probe", ToolTip = "Reads an NWF's clash tests for the workbook check, F104")]
    [AddInPlugin(AddInLocation.AddIn)]
    public sealed class DocumentReadProbePlugin : AddInPlugin
    {
        public const string PluginName = "DocumentReadProbe";
        public const string DeveloperCode = "PARS";

        internal const int Unknown = -1;
        internal const string UnknownWord = "UNKNOWN";
        internal const string NoValue = "-";

        internal const string NwfExtension = ".nwf";

        private const string WorkFolderName = "NwcFederatorLoop";
        private const string ReadOutSuffix = "-document.txt";
        private const string PartialSuffix = ".partial";

        /// <summary>The five result statuses in the order every table writes them.</summary>
        internal static readonly ClashResultStatus[] Statuses =
        {
            ClashResultStatus.New,
            ClashResultStatus.Active,
            ClashResultStatus.Reviewed,
            ClashResultStatus.Approved,
            ClashResultStatus.Resolved
        };

        public override int Execute(params string[] parameters)
        {
            if (parameters == null || parameters.Length < 2)
            {
                return 2;
            }

            string work = WorkFolder();
            string output = FullPath(parameters[0], out _);

            if (output == null || !IsUnder(output, work) || !Directory.Exists(output))
            {
                return 3;
            }

            HashSet<string> names = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            for (int i = 1; i < parameters.Length; i++)
            {
                string name = ReadOutName(parameters[i]);

                if (name == null || !names.Add(name))
                {
                    return 4;
                }
            }

            int code = 0;

            for (int i = 1; i < parameters.Length; i++)
            {
                if (!ReadOne(output, parameters[i], work))
                {
                    code = 1;
                }
            }

            return code;
        }

        private static bool ReadOne(string output, string raw, string work)
        {
            string target = Path.Combine(output, ReadOutName(raw) + ReadOutSuffix);
            string partial = target + PartialSuffix;

            // A partial file left by a read that was stopped is evidence too, so neither is
            // ever written over.
            if (File.Exists(target) || File.Exists(partial))
            {
                return false;
            }

            bool whole;

            using (FileStream stream = new FileStream(partial, FileMode.CreateNew, FileAccess.Write, FileShare.Read))
            using (StreamWriter writer = new StreamWriter(stream, new UTF8Encoding(false)))
            {
                writer.AutoFlush = true;
                whole = new ReadOut(writer).Write(raw, work);
            }

            File.Move(partial, target);
            return whole;
        }

        internal static string WorkFolder()
        {
            return Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                WorkFolderName) + Path.DirectorySeparatorChar;
        }

        /// <summary>Strictly under the work folder, the folder itself is not.</summary>
        internal static bool IsUnder(string full, string work)
        {
            string trimmed = full.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

            return trimmed.Length > work.Length
                && trimmed.StartsWith(work, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// The full path, or null with the reason when the text is not a usable path. The
        /// four exceptions caught are the ones GetFullPath documents for a malformed path,
        /// and each becomes a refusal, never a silent pass. Inside a read-out the REFUSED
        /// line carries the reason. Before any read-out, where Execute checks the output
        /// folder and the names and writes nothing on a fault, the reason is dropped and
        /// the code 3 or 4 is all that goes back.
        /// </summary>
        internal static string FullPath(string path, out string why)
        {
            why = null;

            if (string.IsNullOrWhiteSpace(path))
            {
                why = "the parameter is empty";
                return null;
            }

            try
            {
                return Path.GetFullPath(path);
            }
            catch (ArgumentException error)
            {
                why = Describe(error);
            }
            catch (NotSupportedException error)
            {
                why = Describe(error);
            }
            catch (PathTooLongException error)
            {
                why = Describe(error);
            }
            catch (SecurityException error)
            {
                why = Describe(error);
            }

            return null;
        }

        private static string ReadOutName(string raw)
        {
            string full = FullPath(raw, out _);

            if (full == null)
            {
                return null;
            }

            string name = Path.GetFileNameWithoutExtension(full);
            return string.IsNullOrEmpty(name) ? null : name;
        }

        internal static string Clean(string text)
        {
            return Regex.Replace(text ?? string.Empty, "[\t\r\n]+", " ");
        }

        internal static string Describe(Exception error)
        {
            return error.GetType().Name + ": " + Clean(error.Message);
        }

        internal static string Number(long value)
        {
            return value.ToString(CultureInfo.InvariantCulture);
        }

        internal static string Real(double value)
        {
            return double.IsNaN(value) ? UnknownWord : value.ToString("R", CultureInfo.InvariantCulture);
        }

        internal static int StatusIndex(ClashResultStatus status)
        {
            return Array.IndexOf(Statuses, status);
        }
    }

    /// <summary>One read-out, written line by line as it is read.</summary>
    internal sealed class ReadOut
    {
        private const int PumpRounds = 20;
        private const int PumpPauseMilliseconds = 250;

        private readonly StreamWriter writer;
        private readonly List<string> doubts = new List<string>();

        internal ReadOut(StreamWriter writer)
        {
            this.writer = writer;
        }

        internal bool Write(string raw, string work)
        {
            writer.WriteLine("DOCUMENT READ-OUT");

            try
            {
                return Read(raw, work);
            }
            catch (Exception error)
            {
                // Whatever the steps below did not expect. Written where the read-out stops,
                // so the file says READ-OUT FAILED, the code says 1 and nothing reads whole.
                writer.WriteLine("READ-OUT FAILED: " + DocumentReadProbePlugin.Describe(error));
                return false;
            }
        }

        private bool Read(string raw, string work)
        {
            Line("probe", DocumentReadProbePlugin.PluginName + " " + Stamp());
            Line("navisworks", VersionText());
            Line("automated", NavisworksApplication.IsAutomated ? "True" : "False");
            Line("parameter", DocumentReadProbePlugin.Clean(raw));

            string why;
            string full = DocumentReadProbePlugin.FullPath(raw, out why);
            string refusal = Refusal(full, why, work);

            if (refusal != null)
            {
                writer.WriteLine("REFUSED: " + refusal);
                return false;
            }

            FileInfo info = new FileInfo(full);
            Line("nwf bytes", DocumentReadProbePlugin.Number(info.Length));
            Line("nwf written", info.LastWriteTimeUtc.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture) + " UTC");
            Line("nwf sha256", Sha256(full));

            Document document = NavisworksApplication.ActiveDocument;

            if (document == null)
            {
                writer.WriteLine("READ-OUT FAILED: there is no active document to open the NWF into");
                return false;
            }

            Stopwatch clock = Stopwatch.StartNew();
            bool opened = document.TryOpenFile(full);
            clock.Stop();
            Line("opened", (opened ? "True" : "False") + " in "
                + clock.Elapsed.TotalSeconds.ToString("0.000", CultureInfo.InvariantCulture) + " s");

            if (!opened)
            {
                writer.WriteLine("READ-OUT FAILED: TryOpenFile returned false");
                return false;
            }

            string fileName = document.FileName;
            Line("document file", DocumentReadProbePlugin.Clean(fileName));

            if (!string.Equals(fileName, full, StringComparison.OrdinalIgnoreCase))
            {
                doubts.Add("the document reads its file as [" + DocumentReadProbePlugin.Clean(fileName)
                    + "], which is not the NWF asked for");
            }

            string unitsName = DocumentReadProbePlugin.UnknownWord;
            double perUnit = double.NaN;
            double perMetre = double.NaN;
            Units units;

            if (TryUnits(document, out units))
            {
                unitsName = units.ToString();
                perUnit = Scale(units, Units.Meters, "metres per unit");
                perMetre = Scale(Units.Meters, units, "units per metre");
            }

            double perMillimetre = Scale(Units.Millimeters, Units.Meters, "metres per millimetre");
            Line("units", unitsName);
            Line("metres per millimetre", DocumentReadProbePlugin.Real(perMillimetre));

            if (!double.IsNaN(perUnit) && !double.IsNaN(perMetre) && Math.Abs((perUnit * perMetre) - 1.0) > 1e-9)
            {
                doubts.Add("ScaleFactor gives " + DocumentReadProbePlugin.Real(perUnit) + " metres per unit and "
                    + DocumentReadProbePlugin.Real(perMetre) + " units per metre, which are not each other's inverse");
            }

            // A number times its inverse is one whichever way ScaleFactor converts, so the
            // check above cannot tell the unit converted from the unit converted into. A
            // millimetre is less than a metre, so from millimetres into metres reads below
            // one only when the first parameter is the unit converted from. NaN fails too.
            if (!(perMillimetre < 1.0))
            {
                doubts.Add("ScaleFactor(Millimeters, Meters) reads " + DocumentReadProbePlugin.Real(perMillimetre)
                    + ", which is not below 1, so which way ScaleFactor converts is UNKNOWN and every value in metres is written UNKNOWN");
                perUnit = double.NaN;
            }

            Line("metres per unit", DocumentReadProbePlugin.Real(perUnit));
            Line("units per metre", DocumentReadProbePlugin.Real(perMetre));

            Pass first = Pass.Read(document, perUnit);
            Pump();
            Pass second = Pass.Read(document, perUnit);

            Line("pass 1", first.Totals());
            Line("pass 2", second.Totals());
            ComparePasses(first, second);

            writer.WriteLine();
            writer.WriteLine("-- tests: " + Pass.TestColumns());

            foreach (string line in second.TestLines())
            {
                writer.WriteLine(line);
            }

            writer.WriteLine();
            writer.WriteLine("-- results: " + Pass.ResultColumns());

            foreach (string line in second.ResultLines())
            {
                writer.WriteLine(line);
            }

            writer.WriteLine();
            writer.WriteLine("TOTALS: " + second.Totals());

            List<string> all = new List<string>(doubts);

            foreach (string doubt in first.Doubts)
            {
                all.Add("pass 1: " + doubt);
            }

            foreach (string doubt in second.Doubts)
            {
                all.Add("pass 2: " + doubt);
            }

            writer.WriteLine("DOUBTS: " + DocumentReadProbePlugin.Number(all.Count));

            foreach (string doubt in all)
            {
                writer.WriteLine("  " + doubt);
            }

            writer.WriteLine("END OF READ-OUT");
            return true;
        }

        /// <summary>Why this NWF is not read, or null when it may be.</summary>
        private static string Refusal(string full, string why, string work)
        {
            if (full == null)
            {
                return "the parameter is not a usable path, " + why;
            }

            if (!DocumentReadProbePlugin.IsUnder(full, work))
            {
                return "the NWF is not under " + work;
            }

            if (!full.EndsWith(DocumentReadProbePlugin.NwfExtension, StringComparison.OrdinalIgnoreCase))
            {
                return "the file is not an .nwf";
            }

            if (!File.Exists(full))
            {
                return "there is no file at the path";
            }

            return null;
        }

        /// <summary>
        /// Every line that differs between the two passes is a doubt. The totals are
        /// compared, then the tables line by line, so a status that moved without moving a
        /// total is caught as well.
        /// </summary>
        private void ComparePasses(Pass first, Pass second)
        {
            if (first.Totals() != second.Totals())
            {
                doubts.Add("the two passes read different totals, see pass 1 and pass 2 above");
            }

            CompareLines("tests", first.TestLines(), second.TestLines());
            CompareLines("results", first.ResultLines(), second.ResultLines());
        }

        private void CompareLines(string table, List<string> first, List<string> second)
        {
            if (first.Count != second.Count)
            {
                doubts.Add("the " + table + " table held " + DocumentReadProbePlugin.Number(first.Count)
                    + " lines in pass 1 and " + DocumentReadProbePlugin.Number(second.Count) + " in pass 2");
                return;
            }

            for (int i = 0; i < first.Count; i++)
            {
                if (first[i] != second[i])
                {
                    doubts.Add("the " + table + " table differs between the passes from line "
                        + DocumentReadProbePlugin.Number(i + 1) + ", pass 1 [" + first[i] + "], pass 2 [" + second[i] + "]");
                    return;
                }
            }
        }

        private bool TryUnits(Document document, out Units units)
        {
            try
            {
                units = document.Units;
                return true;
            }
            catch (Exception error)
            {
                doubts.Add("reading Document.Units threw " + DocumentReadProbePlugin.Describe(error)
                    + ", so the units and every value in metres are UNKNOWN");
                units = Units.Meters;
                return false;
            }
        }

        private double Scale(Units from, Units to, string what)
        {
            try
            {
                return UnitConversion.ScaleFactor(from, to);
            }
            catch (Exception error)
            {
                doubts.Add(what + ": UnitConversion.ScaleFactor threw " + DocumentReadProbePlugin.Describe(error)
                    + ", so every value in metres is UNKNOWN");
                return double.NaN;
            }
        }

        /// <summary>
        /// Five seconds of the dispatcher at background priority, so anything the open
        /// queued has its turn before the second pass.
        /// </summary>
        private static void Pump()
        {
            for (int i = 0; i < PumpRounds; i++)
            {
                Dispatcher.CurrentDispatcher.Invoke(DispatcherPriority.Background, new Action(delegate { }));
                Thread.Sleep(PumpPauseMilliseconds);
            }
        }

        private void Line(string key, string value)
        {
            writer.WriteLine(key + "\t" + value);
        }

        private static string Stamp()
        {
            AssemblyInformationalVersionAttribute stamp =
                typeof(DocumentReadProbePlugin).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>();

            return stamp == null ? DocumentReadProbePlugin.UnknownWord : stamp.InformationalVersion;
        }

        private static string VersionText()
        {
            ApplicationVersion version = NavisworksApplication.Version;

            if (version == null)
            {
                return DocumentReadProbePlugin.UnknownWord + ", Application.Version is null";
            }

            return DocumentReadProbePlugin.Clean(version.RuntimeProductName)
                + ", runtime " + DocumentReadProbePlugin.Number(version.RuntimeMajor)
                + "." + DocumentReadProbePlugin.Number(version.RuntimeMinor)
                + ", build " + DocumentReadProbePlugin.Number(version.Build)
                + ", api " + DocumentReadProbePlugin.Number(version.ApiMajor)
                + "." + DocumentReadProbePlugin.Number(version.ApiMinor);
        }

        private static string Sha256(string path)
        {
            using (SHA256 sha = SHA256.Create())
            using (FileStream stream = File.OpenRead(path))
            {
                byte[] hash = sha.ComputeHash(stream);
                StringBuilder text = new StringBuilder(hash.Length * 2);

                foreach (byte b in hash)
                {
                    text.Append(b.ToString("X2", CultureInfo.InvariantCulture));
                }

                return text.ToString();
            }
        }
    }

    /// <summary>
    /// One read of the whole document. Holds strings and numbers only, so nothing it keeps
    /// points into the document once the read is over.
    /// </summary>
    internal sealed class Pass
    {
        private int models = DocumentReadProbePlugin.Unknown;
        private int sets = DocumentReadProbePlugin.Unknown;
        private int setFolders = DocumentReadProbePlugin.Unknown;
        private int viewpoints = DocumentReadProbePlugin.Unknown;
        private int viewpointFolders = DocumentReadProbePlugin.Unknown;
        private int viewpointOther = DocumentReadProbePlugin.Unknown;
        private int tests = DocumentReadProbePlugin.Unknown;
        private int testFolders = DocumentReadProbePlugin.Unknown;

        private readonly List<TestRead> read = new List<TestRead>();

        internal readonly List<string> Doubts = new List<string>();

        internal static Pass Read(Document document, double perUnit)
        {
            Pass pass = new Pass();
            pass.ReadModels(document);
            pass.ReadSets(document);
            pass.ReadViewpoints(document);
            pass.ReadTests(document, perUnit);
            return pass;
        }

        internal static string TestColumns()
        {
            StringBuilder text = new StringBuilder("position, folder, test, type, status, tolerance, tolerance m, top level");

            foreach (ClashResultStatus status in DocumentReadProbePlugin.Statuses)
            {
                text.Append(", top ").Append(Word(status));
            }

            text.Append(", leaves");

            foreach (ClashResultStatus status in DocumentReadProbePlugin.Statuses)
            {
                text.Append(", ").Append(Word(status));
            }

            return text.Append(", groups, empty groups, nested groups, other").ToString();
        }

        internal static string ResultColumns()
        {
            StringBuilder text = new StringBuilder("test position, test, position, kind, name, status, leaves");

            foreach (ClashResultStatus status in DocumentReadProbePlugin.Statuses)
            {
                text.Append(", ").Append(Word(status));
            }

            return text.Append(", distance, distance m").ToString();
        }

        internal List<string> TestLines()
        {
            List<string> lines = new List<string>();

            foreach (TestRead test in read)
            {
                lines.Add(test.Line());
            }

            return lines;
        }

        internal List<string> ResultLines()
        {
            List<string> lines = new List<string>();

            foreach (TestRead test in read)
            {
                foreach (ResultRead result in test.Results)
                {
                    lines.Add(DocumentReadProbePlugin.Number(test.Position) + "\t" + test.Name + "\t" + result.Line());
                }
            }

            return lines;
        }

        internal string Totals()
        {
            int[] leavesBy = new int[DocumentReadProbePlugin.Statuses.Length];
            int topLevel = 0;
            int leaves = 0;
            int groups = 0;
            int emptyGroups = 0;
            int nestedGroups = 0;
            int other = 0;
            bool counted = tests != DocumentReadProbePlugin.Unknown;

            foreach (TestRead test in read)
            {
                if (!test.Counted)
                {
                    counted = false;
                    continue;
                }

                topLevel += test.TopLevel;
                leaves += test.Leaves;
                groups += test.Groups;
                emptyGroups += test.EmptyGroups;
                nestedGroups += test.NestedGroups;
                other += test.Other;

                for (int s = 0; s < leavesBy.Length; s++)
                {
                    leavesBy[s] += test.LeavesBy[s];
                }
            }

            StringBuilder text = new StringBuilder();
            text.Append("models ").Append(DocumentReadProbePlugin.Number(models));
            text.Append(", selection sets ").Append(DocumentReadProbePlugin.Number(sets));
            text.Append(", set folders ").Append(DocumentReadProbePlugin.Number(setFolders));
            text.Append(", saved viewpoints ").Append(DocumentReadProbePlugin.Number(viewpoints));
            text.Append(", viewpoint folders ").Append(DocumentReadProbePlugin.Number(viewpointFolders));
            text.Append(", other viewpoint items ").Append(DocumentReadProbePlugin.Number(viewpointOther));
            text.Append(", tests ").Append(DocumentReadProbePlugin.Number(tests));
            text.Append(", test folders ").Append(DocumentReadProbePlugin.Number(testFolders));
            text.Append(", top level ").Append(Counted(counted, topLevel));
            text.Append(", leaves ").Append(Counted(counted, leaves));

            for (int s = 0; s < leavesBy.Length; s++)
            {
                text.Append(", leaves ").Append(Word(DocumentReadProbePlugin.Statuses[s])).Append(' ').Append(Counted(counted, leavesBy[s]));
            }

            text.Append(", groups ").Append(Counted(counted, groups));
            text.Append(", empty groups ").Append(Counted(counted, emptyGroups));
            text.Append(", nested groups ").Append(Counted(counted, nestedGroups));
            text.Append(", other results ").Append(Counted(counted, other));
            return text.ToString();
        }

        private static string Counted(bool counted, int value)
        {
            return DocumentReadProbePlugin.Number(counted ? value : DocumentReadProbePlugin.Unknown);
        }

        internal static string Word(ClashResultStatus status)
        {
            return status.ToString().ToLowerInvariant();
        }

        private void ReadModels(Document document)
        {
            try
            {
                models = document.Models.Count;
            }
            catch (Exception error)
            {
                Doubts.Add("counting the models threw " + DocumentReadProbePlugin.Describe(error) + ", models is -1");
            }
        }

        private void ReadSets(Document document)
        {
            int setCount = 0;
            int folderCount = 0;

            try
            {
                using (FolderItem root = document.SelectionSets.RootItem)
                {
                    WalkSets(root, ref setCount, ref folderCount);
                }

                sets = setCount;
                setFolders = folderCount;
            }
            catch (Exception error)
            {
                Doubts.Add("walking the selection sets threw " + DocumentReadProbePlugin.Describe(error) + ", both counts are -1");
            }
        }

        /// <summary>A SelectionSet counts one, any other GroupItem is a folder and is walked.</summary>
        private void WalkSets(GroupItem parent, ref int setCount, ref int folderCount)
        {
            if (parent == null)
            {
                return;
            }

            SavedItemCollection children = parent.Children;

            for (int i = 0; i < children.Count; i++)
            {
                using (SavedItem child = children[i])
                {
                    if (child is SelectionSet)
                    {
                        setCount++;
                        continue;
                    }

                    GroupItem folder = child as GroupItem;

                    if (folder != null)
                    {
                        folderCount++;
                        WalkSets(folder, ref setCount, ref folderCount);
                        continue;
                    }

                    Doubts.Add("the selection sets tree holds a " + child.GetType().Name + " named ["
                        + DocumentReadProbePlugin.Clean(child.DisplayName) + "], neither a set nor a folder");
                }
            }
        }

        private void ReadViewpoints(Document document)
        {
            int viewpointCount = 0;
            int folderCount = 0;
            int otherCount = 0;

            try
            {
                using (FolderItem root = document.SavedViewpoints.RootItem)
                {
                    WalkViewpoints(root, ref viewpointCount, ref folderCount, ref otherCount);
                }

                viewpoints = viewpointCount;
                viewpointFolders = folderCount;
                viewpointOther = otherCount;
            }
            catch (Exception error)
            {
                Doubts.Add("walking the saved viewpoints threw " + DocumentReadProbePlugin.Describe(error) + ", its three counts are -1");
            }
        }

        /// <summary>
        /// A SavedViewpoint counts one, any other GroupItem is a folder and is walked, which
        /// takes in an animation, and anything else, an animation cut among them, is other.
        /// </summary>
        private static void WalkViewpoints(GroupItem parent, ref int viewpointCount, ref int folderCount, ref int otherCount)
        {
            if (parent == null)
            {
                return;
            }

            SavedItemCollection children = parent.Children;

            for (int i = 0; i < children.Count; i++)
            {
                using (SavedItem child = children[i])
                {
                    if (child is SavedViewpoint)
                    {
                        viewpointCount++;
                        continue;
                    }

                    GroupItem folder = child as GroupItem;

                    if (folder != null)
                    {
                        folderCount++;
                        WalkViewpoints(folder, ref viewpointCount, ref folderCount, ref otherCount);
                        continue;
                    }

                    otherCount++;
                }
            }
        }

        private void ReadTests(Document document, double perUnit)
        {
            int testCount = 0;
            int folderCount = 0;

            try
            {
                DocumentClash clash = document.GetClash();
                DocumentClashTests data = clash == null ? null : clash.TestsData;

                if (data == null)
                {
                    Doubts.Add("GetClash or its TestsData is null, so the tests are not read and both counts are -1");
                    return;
                }

                WalkTests(data.Tests, string.Empty, perUnit, ref testCount, ref folderCount);
                tests = testCount;
                testFolders = folderCount;
            }
            catch (Exception error)
            {
                Doubts.Add("walking the clash tests threw " + DocumentReadProbePlugin.Describe(error)
                    + " after " + DocumentReadProbePlugin.Number(testCount) + " tests, so tests and test folders are -1");
            }
        }

        /// <summary>
        /// Depth first. A ClashTest is tested before a GroupItem because a test is a
        /// GroupItem holding its results, scan.md 4f. Any other GroupItem is a folder.
        /// </summary>
        private void WalkTests(SavedItemCollection items, string folder, double perUnit, ref int testCount, ref int folderCount)
        {
            if (items == null)
            {
                return;
            }

            for (int i = 0; i < items.Count; i++)
            {
                using (SavedItem item = items[i])
                {
                    ClashTest test = item as ClashTest;

                    if (test != null)
                    {
                        testCount++;
                        read.Add(TestRead.From(test, testCount, folder, perUnit, Doubts));
                        continue;
                    }

                    GroupItem group = item as GroupItem;

                    if (group != null)
                    {
                        folderCount++;
                        string name = DocumentReadProbePlugin.Clean(group.DisplayName);
                        WalkTests(group.Children, folder.Length == 0 ? name : folder + "/" + name, perUnit, ref testCount, ref folderCount);
                        continue;
                    }

                    Doubts.Add("the tests tree holds a " + item.GetType().Name + " named ["
                        + DocumentReadProbePlugin.Clean(item.DisplayName) + "], neither a test nor a folder");
                }
            }
        }
    }

    /// <summary>One clash test as read, strings and numbers only.</summary>
    internal sealed class TestRead
    {
        internal int Position;
        internal string Folder = string.Empty;
        internal string Name = "(name not read)";
        internal string Type = DocumentReadProbePlugin.UnknownWord;
        internal string Status = DocumentReadProbePlugin.UnknownWord;
        internal double Tolerance = double.NaN;
        internal double ToleranceMetres = double.NaN;
        internal bool Counted;
        internal int TopLevel;
        internal readonly int[] TopBy = new int[DocumentReadProbePlugin.Statuses.Length];
        internal int Leaves;
        internal readonly int[] LeavesBy = new int[DocumentReadProbePlugin.Statuses.Length];
        internal int Groups;
        internal int EmptyGroups;
        internal int NestedGroups;
        internal int Other;
        internal readonly List<ResultRead> Results = new List<ResultRead>();

        internal static TestRead From(ClashTest test, int position, string folder, double perUnit, List<string> doubts)
        {
            TestRead read = new TestRead();
            read.Position = position;
            read.Folder = folder;

            try
            {
                read.Name = DocumentReadProbePlugin.Clean(test.DisplayName);
                read.Type = test.TestType.ToString();
                read.Status = test.Status.ToString();
                read.Tolerance = test.Tolerance;
                read.ToleranceMetres = read.Tolerance * perUnit;
            }
            catch (Exception error)
            {
                doubts.Add("test " + DocumentReadProbePlugin.Number(position) + " [" + read.Name
                    + "]: reading its name, type, status or tolerance threw " + DocumentReadProbePlugin.Describe(error));
            }

            try
            {
                read.ReadResults(test.Children, perUnit, doubts);
                read.Counted = true;
            }
            catch (Exception error)
            {
                read.Results.Clear();
                doubts.Add("test " + DocumentReadProbePlugin.Number(position) + " [" + read.Name
                    + "]: walking its results threw " + DocumentReadProbePlugin.Describe(error) + ", so its counts are -1");
            }

            return read;
        }

        /// <summary>
        /// Every top level child, in the order the test holds them. A group is one top level
        /// result at its own status, with every clash under it counted as a leaf at the
        /// clash's own status, nested groups descended. A plain clash is one top level
        /// result and one leaf. Anything else is kind other and a doubt.
        /// </summary>
        private void ReadResults(SavedItemCollection children, double perUnit, List<string> doubts)
        {
            if (children == null)
            {
                return;
            }

            for (int i = 0; i < children.Count; i++)
            {
                using (SavedItem child = children[i])
                {
                    ResultRead result = new ResultRead();
                    result.Position = i + 1;
                    TopLevel++;

                    ClashResultGroup group = child as ClashResultGroup;

                    if (group != null)
                    {
                        result.Kind = "group";
                        result.Name = DocumentReadProbePlugin.Clean(group.DisplayName);
                        result.Status = group.Status.ToString();
                        Groups++;
                        CountTop(group.Status, result, doubts);

                        if (Descend(group.Children, result, doubts) == 0)
                        {
                            EmptyGroups++;
                        }

                        Results.Add(result);
                        continue;
                    }

                    ClashResult clash = child as ClashResult;

                    if (clash != null)
                    {
                        result.Kind = "clash";
                        result.Name = DocumentReadProbePlugin.Clean(clash.DisplayName);
                        result.Status = clash.Status.ToString();
                        result.Distance = clash.Distance;
                        result.DistanceMetres = result.Distance * perUnit;
                        CountTop(clash.Status, result, doubts);
                        CountLeaf(clash.Status, result, doubts);
                        Results.Add(result);
                        continue;
                    }

                    result.Kind = "other";
                    result.Name = DocumentReadProbePlugin.Clean(child.DisplayName);
                    Other++;
                    doubts.Add("test [" + Name + "] holds a " + child.GetType().Name + " at position "
                        + DocumentReadProbePlugin.Number(result.Position) + ", neither a clash nor a group");
                    Results.Add(result);
                }
            }
        }

        /// <summary>The clashes under one group, at any depth. Returns how many it found.</summary>
        private int Descend(SavedItemCollection children, ResultRead top, List<string> doubts)
        {
            if (children == null)
            {
                return 0;
            }

            int found = 0;

            for (int i = 0; i < children.Count; i++)
            {
                using (SavedItem child = children[i])
                {
                    ClashResult clash = child as ClashResult;

                    if (clash != null)
                    {
                        CountLeaf(clash.Status, top, doubts);
                        found++;
                        continue;
                    }

                    ClashResultGroup nested = child as ClashResultGroup;

                    if (nested != null)
                    {
                        NestedGroups++;
                        int under = Descend(nested.Children, top, doubts);

                        if (under == 0)
                        {
                            EmptyGroups++;
                        }

                        found += under;
                        continue;
                    }

                    Other++;
                    doubts.Add("test [" + Name + "], group [" + top.Name + "] holds a " + child.GetType().Name
                        + ", neither a clash nor a group");
                }
            }

            return found;
        }

        private void CountTop(ClashResultStatus status, ResultRead result, List<string> doubts)
        {
            int s = DocumentReadProbePlugin.StatusIndex(status);

            if (s < 0)
            {
                doubts.Add("test [" + Name + "], result [" + result.Name + "] is at status " + status
                    + ", which is none of the five, and is left out of the counts by status");
                return;
            }

            TopBy[s]++;
        }

        private void CountLeaf(ClashResultStatus status, ResultRead top, List<string> doubts)
        {
            Leaves++;
            top.Leaves++;
            int s = DocumentReadProbePlugin.StatusIndex(status);

            if (s < 0)
            {
                doubts.Add("test [" + Name + "], a clash under [" + top.Name + "] is at status " + status
                    + ", which is none of the five, and is left out of the counts by status");
                return;
            }

            LeavesBy[s]++;
            top.LeavesBy[s]++;
        }

        internal string Line()
        {
            List<string> fields = new List<string>();
            fields.Add(DocumentReadProbePlugin.Number(Position));
            fields.Add(Folder.Length == 0 ? DocumentReadProbePlugin.NoValue : Folder);
            fields.Add(Name);
            fields.Add(Type);
            fields.Add(Status);
            fields.Add(DocumentReadProbePlugin.Real(Tolerance));
            fields.Add(DocumentReadProbePlugin.Real(ToleranceMetres));
            fields.Add(Count(TopLevel));

            foreach (int n in TopBy)
            {
                fields.Add(Count(n));
            }

            fields.Add(Count(Leaves));

            foreach (int n in LeavesBy)
            {
                fields.Add(Count(n));
            }

            fields.Add(Count(Groups));
            fields.Add(Count(EmptyGroups));
            fields.Add(Count(NestedGroups));
            fields.Add(Count(Other));
            return string.Join("\t", fields);
        }

        private string Count(int value)
        {
            return DocumentReadProbePlugin.Number(Counted ? value : DocumentReadProbePlugin.Unknown);
        }
    }

    /// <summary>One top level result as read. A group's distance is not read, the dash says so.</summary>
    internal sealed class ResultRead
    {
        internal int Position;
        internal string Kind = "other";
        internal string Name = string.Empty;
        internal string Status = DocumentReadProbePlugin.NoValue;
        internal int Leaves;
        internal readonly int[] LeavesBy = new int[DocumentReadProbePlugin.Statuses.Length];
        internal double Distance = double.NaN;
        internal double DistanceMetres = double.NaN;

        internal string Line()
        {
            List<string> fields = new List<string>();
            fields.Add(DocumentReadProbePlugin.Number(Position));
            fields.Add(Kind);
            fields.Add(Name);
            fields.Add(Status);
            fields.Add(DocumentReadProbePlugin.Number(Leaves));

            foreach (int n in LeavesBy)
            {
                fields.Add(DocumentReadProbePlugin.Number(n));
            }

            bool clash = Kind == "clash";
            fields.Add(clash ? DocumentReadProbePlugin.Real(Distance) : DocumentReadProbePlugin.NoValue);
            fields.Add(clash ? DocumentReadProbePlugin.Real(DistanceMetres) : DocumentReadProbePlugin.NoValue);
            return string.Join("\t", fields);
        }
    }
}
