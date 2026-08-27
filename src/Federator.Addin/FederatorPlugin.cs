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

        public override int Execute(params string[] parameters)
        {
            // First line of the handler, before the folder is read, before the window
            // opens anything, before any Navisworks call. A run that dies at startup still
            // leaves a file behind. StartOrDisabled never throws.
            RunLog log = RunLog.StartOrDisabled();

            try
            {
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
