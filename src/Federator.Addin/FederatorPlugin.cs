using System;
using System.Windows.Interop;
using Autodesk.Navisworks.Api.Plugins;
using Federator.Addin.Engine;
using Federator.Addin.Ui;
using Federator.Core.Communities;
using Federator.Core.Diagnostics;
using NavisworksApplication = Autodesk.Navisworks.Api.Application;

namespace Federator.Addin
{
    /// <summary>
    /// The button on the Tool Add-ins tab. It opens the window and does nothing else.
    /// Every Navisworks call this add-in makes happens on the thread that calls Execute.
    /// </summary>
    [Plugin(
        PluginName,
        DeveloperCode,
        DisplayName = "Parsons NWC Federator",
        ToolTip = "Federate discipline NWC files into one NWF and one NWD per building")]
    [AddInPlugin(AddInLocation.AddIn)]
    public sealed class FederatorPlugin : AddInPlugin
    {
        public const string PluginName = "ParsonsNwcFederator";
        public const string DeveloperCode = "PARS";

        /// <summary>
        /// Runs when Navisworks first touches this type, which is before Execute and long
        /// before anything reaches the workbook writer. The handler has to be in place by
        /// then, because the runtime asks for a dependency the first time it compiles a
        /// method that mentions one.
        ///
        /// A run on aa163c9e got all the way to writing the workbook and then failed with
        /// System.Numerics.Vectors 4.1.3.0 not found, while a copy of that assembly was
        /// sitting in the bundle at 4.1.4.0. See BundleAssemblies for the measurements.
        /// </summary>
        static FederatorPlugin()
        {
            BundleAssemblies.InstallBesideThisAssembly(null);
        }

        public override int Execute(params string[] parameters)
        {
            // First line of the handler, before the folder is read, before the window
            // opens anything, before any Navisworks call. A run that dies at startup still
            // leaves a file behind. StartOrDisabled never throws.
            // F222. A group's own Navisworks deletes no log, so the logs the joined RESULT names are all still there.
            RunLog log = IsAPackageRun(parameters)
                ? RunLog.StartOrDisabled(RunLog.DefaultLogFolder(), DateTime.Now, RunLog.KeepEveryLog)
                : RunLog.StartOrDisabled();

            try
            {
                // Already hooked up by the static constructor. This only points it at the
                // log, so what it resolved is on the record, and re-registers it if this
                // type was somehow reached without its initialiser running.
                BundleAssemblies.InstallBesideThisAssembly(log.Line);

                log.Session(
                    NavisworksFacts.PluginVersion(),
                    NavisworksFacts.VersionString(),
                    NavisworksFacts.OpenDocument());

                if (!log.IsWritingToDisk)
                {
                    log.Line("WARNING  the run carries on but nothing is being written to disk");
                }

                PackageRunChild child = ThePackageToRun(parameters, log);

                if (IsAPackageRun(parameters) && child == null)
                {
                    return 2;
                }

                // F223, Q207. A record of the Auto-Save switch that a run left, one that ended or crashed before its put back,
                // is put back as the window a person opens starts, and never in a group's own Navisworks, whose runner holds it.
                if (child == null)
                {
                    AutoSaveOff.PutBackWhatARunLeft(log.Line, false);
                }

                FederatorWindow window = child == null ? new FederatorWindow(log) : new FederatorWindow(log, child);

                IntPtr owner = OwnerHandle();

                if (owner != IntPtr.Zero)
                {
                    new WindowInteropHelper(window).Owner = owner;
                }

                window.ShowDialog();
                log.Line("Window closed.");
                return 0;
            }
            catch (Exception error)
            {
                log.Failure("starting the add-in", error, "stopped, the window never opened");

                // F214. Nobody is at a group's own Navisworks, so the log carries it and the runner goes on.
                if (IsAPackageRun(parameters))
                {
                    return 1;
                }

                System.Windows.MessageBox.Show(
                    "Parsons NWC Federator could not start." + Environment.NewLine + Environment.NewLine
                        + error + Environment.NewLine + Environment.NewLine
                        + "Log: " + (log.Path ?? "none, the log could not be opened"),
                    "Parsons NWC Federator",
                    System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Error);
                return 1;
            }
            finally
            {
                log.Dispose();
            }
        }

        /// <summary>
        /// Whether Navisworks was started for one package of a run of packages, F214: the runner hands the mode, the
        /// order's path and the package's index. A person's press of the button hands nothing.
        /// </summary>
        private static bool IsAPackageRun(string[] parameters)
        {
            return parameters != null && parameters.Length > 0 && parameters[0] == PackageRunOrder.ChildMode;
        }

        /// <summary>The group this Navisworks was started to run, or null where it was not, or where the order would not read, said.</summary>
        private static PackageRunChild ThePackageToRun(string[] parameters, RunLog log)
        {
            if (!IsAPackageRun(parameters))
            {
                return null;
            }

            int index;

            if (parameters.Length != 3
                || !int.TryParse(parameters[2], System.Globalization.NumberStyles.None, System.Globalization.CultureInfo.InvariantCulture, out index))
            {
                log.Line(PackageRunWords.Prefix + " this Navisworks was started for a group with " + (parameters.Length - 1)
                    + " arguments where it takes the order and the index, so nothing runs");
                return null;
            }

            string why;
            PackageRunOrder order = PackageRunOrder.Read(parameters[1], out why);

            if (order == null || index >= order.Units.Count)
            {
                log.Line(PackageRunWords.Prefix + " the order of this group does not read, "
                    + (why ?? "it holds no group of index " + index) + ", so nothing runs");
                return null;
            }

            return new PackageRunChild(order, index);
        }

        /// <summary>
        /// Application.Gui.MainWindow is a Windows Forms IWin32Window, so a WPF dialog is
        /// parented through its handle rather than through a WPF Window.
        /// </summary>
        private static IntPtr OwnerHandle()
        {
            try
            {
                if (NavisworksApplication.Gui != null && NavisworksApplication.Gui.MainWindow != null)
                {
                    return NavisworksApplication.Gui.MainWindow.Handle;
                }
            }
            catch (Exception)
            {
                // A dialog with no owner is still usable, so this is never worth failing over.
            }

            return IntPtr.Zero;
        }
    }
}
