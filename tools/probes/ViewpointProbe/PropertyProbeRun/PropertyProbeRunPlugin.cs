using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading;
using Autodesk.Navisworks.Api;
using Autodesk.Navisworks.Api.Plugins;
using Federator.Addin.Engine;
using Federator.Core.Diagnostics;
using Federator.Core.Probe;

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

namespace PropertyProbeRun
{
    /// <summary>
    /// Step 364, 2026-10-07. Opens one NWC copy and runs the add-in's PropertyProbe over every
    /// model in it, the same class and the same default ProbeSettings the button Probe model
    /// properties uses, with a RunLog started in a folder the script names. The CSV goes beside
    /// the model's file, so a model whose CSV would land outside the folder of the copy is
    /// refused by name and not read. It opens nothing else, saves nothing and writes nothing
    /// but the output file, the log and the CSV.
    /// </summary>
    [Plugin(PluginName, DeveloperCode, DisplayName = "Property probe run", ToolTip = "Step 364, the F86 property probe on one NWC copy")]
    [AddInPlugin(AddInLocation.AddIn)]
    public sealed class PropertyProbeRunPlugin : AddInPlugin
    {
        public const string PluginName = "PropertyProbeRun";
        public const string DeveloperCode = "PARS";

        private static readonly CultureInfo Inv = CultureInfo.InvariantCulture;

        private StreamWriter results;

        public override int Execute(params string[] parameters)
        {
            if (parameters == null || parameters.Length < 5 || parameters[0] != "properties")
            {
                return 2;
            }

            using (results = new StreamWriter(parameters[1], true, new UTF8Encoding(false)))
            {
                results.AutoFlush = true;
                Say("probe started, mode " + parameters[0]);

                try
                {
                    int code = Measure(parameters[2], parameters[3], parameters[4]);
                    Say("probe finished, code " + code);
                    return code;
                }
                catch (Exception error)
                {
                    Exception inner = error is TargetInvocationException && error.InnerException != null ? error.InnerException : error;
                    Say("THREW " + inner.GetType().Name + ": " + inner.Message);
                    Say(inner.StackTrace ?? string.Empty);
                    return 1;
                }
            }
        }

        private int Measure(string nwcCopy, string logFolder, string blockFile)
        {
            string copyFolder = Path.GetDirectoryName(Path.GetFullPath(nwcCopy));
            Say("the copy " + nwcCopy + ", " + new FileInfo(nwcCopy).Length + " bytes");

            ProbeSettings settings = new ProbeSettings();
            Say("ProbeSettings default: " + settings.Categories.Count + " categories, cap " + settings.DistinctValueCap
                + ", category read from " + string.Join(" then ", settings.CategoryNames));
            Say("categories asked: " + string.Join(" | ", settings.Categories));

            string why;

            if (!ProbeSettings.MayRead(nwcCopy, out why))
            {
                Say("REFUSED by ProbeSettings.MayRead: " + why);
                return 3;
            }

            Document document = Autodesk.Navisworks.Api.Application.ActiveDocument;

            if (document == null)
            {
                Say("UNKNOWN: no active document in this host");
                return 4;
            }

            Say("models before the open " + document.Models.Count);
            Stopwatch open = Stopwatch.StartNew();
            bool opened = document.TryOpenFile(nwcCopy);
            open.Stop();
            Say("TryOpenFile returned " + opened + " after " + open.Elapsed.TotalSeconds.ToString("0.000", Inv) + " s");

            if (!opened)
            {
                Say("UNKNOWN: TryOpenFile returned false, nothing was read");
                return 5;
            }

            Stopwatch wait = Stopwatch.StartNew();

            while (document.Models.Count == 0 && wait.Elapsed.TotalSeconds < 60)
            {
                Thread.Sleep(250);
            }

            Say("models after the open " + document.Models.Count + ", waited " + wait.Elapsed.TotalSeconds.ToString("0.000", Inv) + " s");

            List<Model> allowed = new List<Model>();

            foreach (Model model in document.Models)
            {
                string file = model.FileName ?? string.Empty;
                string source = model.SourceFileName ?? string.Empty;
                Say("model FileName " + file + ", SourceFileName " + source);

                string csv;

                try
                {
                    csv = Path.GetFullPath(ProbeSettings.CsvPathFor(file));
                }
                catch (Exception error)
                {
                    Say("REFUSED this model: its CSV path cannot be made, " + error.Message);
                    continue;
                }

                if (!string.Equals(Path.GetDirectoryName(csv), copyFolder, StringComparison.OrdinalIgnoreCase))
                {
                    Say("REFUSED this model: its CSV would be written to " + csv + ", outside the folder of the copy");
                    continue;
                }

                Say("its CSV goes to " + csv);
                allowed.Add(model);
            }

            if (allowed.Count == 0)
            {
                Say("UNKNOWN: no model may be read");
                return 6;
            }

            IList<string> lines;
            Stopwatch run = Stopwatch.StartNew();

            using (RunLog log = RunLog.Start(logFolder, DateTime.Now))
            {
                Say("RunLog at " + log.Path);
                PropertyProbe probe = new PropertyProbe(log, settings);

                foreach (Model model in allowed)
                {
                    probe.ProbeModel(model);
                }

                lines = probe.Lines;
            }

            run.Stop();
            Say("PropertyProbe.ProbeModel over " + allowed.Count + " model(s) took " + run.Elapsed.TotalSeconds.ToString("0.000", Inv) + " s");

            File.WriteAllLines(blockFile, lines, new UTF8Encoding(false));
            Say("the probe's own lines, " + lines.Count + ", written to " + blockFile + ", every one:");

            foreach (string line in lines)
            {
                Say("  | " + line);
            }

            return 0;
        }

        private void Say(string line)
        {
            results.WriteLine(DateTime.Now.ToString("HH:mm:ss.fff", Inv) + "  " + line);
        }
    }
}
