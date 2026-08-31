using System;
using System.Windows.Interop;
using Autodesk.Navisworks.Api.Plugins;
using Federator.Addin.Engine;
using Federator.Addin.Ui;
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
            RunLog log = RunLog.StartOrDisabled();

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

                FederatorWindow window = new FederatorWindow(log);

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
