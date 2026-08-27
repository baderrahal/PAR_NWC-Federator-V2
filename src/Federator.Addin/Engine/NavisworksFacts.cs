using System;
using System.Globalization;
using System.Reflection;
using System.Text;
using Autodesk.Navisworks.Api;
using Autodesk.Navisworks.Api.ApplicationParts;
using NavisworksApplication = Autodesk.Navisworks.Api.Application;

namespace Federator.Addin.Engine
{
    /// <summary>
    /// The few facts the log header needs out of the running application. Every one of
    /// these is read defensively, because a header that throws would cost the whole log.
    /// </summary>
    public static class NavisworksFacts
    {
        /// <summary>
        /// ApplicationVersion does not override ToString, so the string is built from its
        /// properties. See docs\scan.md section 4c.
        /// </summary>
        public static string VersionString()
        {
            try
            {
                ApplicationVersion version = NavisworksApplication.Version;

                if (version == null)
                {
                    return "UNKNOWN, Application.Version was null";
                }

                StringBuilder text = new StringBuilder();
                text.Append(Or(version.RuntimeProductName, "unnamed product"));
                text.Append("  runtime ").Append(Number(version.RuntimeMajor))
                    .Append('.').Append(Number(version.RuntimeMinor));
                text.Append("  build ").Append(Number(version.Build));
                text.Append("  api ").Append(Number(version.ApiMajor))
                    .Append('.').Append(Number(version.ApiMinor));
                text.Append(version.IsApiStable ? " (stable)" : " (not stable)");

                if (version.IsRuntimeBeta)
                {
                    text.Append("  RUNTIME IS BETA");
                }

                text.Append("  language ").Append(Or(version.RuntimeLanguage, "unknown"));
                text.Append("  runtime string ").Append(Or(version.Runtime, "none"));
                return text.ToString();
            }
            catch (Exception error)
            {
                return "UNKNOWN, reading Application.Version threw "
                    + error.GetType().FullName + ": " + error.Message;
            }
        }

        /// <summary>The document open right now, with its path, or the word none.</summary>
        public static string OpenDocument()
        {
            try
            {
                Document document = NavisworksApplication.ActiveDocument;

                if (document == null)
                {
                    return "none, there is no active document";
                }

                if (document.IsClear)
                {
                    return "none, the document is clear";
                }

                string name = Or(document.CurrentFileName, document.FileName);
                int models = document.Models == null ? 0 : document.Models.Count;

                return Or(name, "an unsaved document")
                    + "  (" + models + (models == 1 ? " model" : " models") + " loaded)";
            }
            catch (Exception error)
            {
                return "UNKNOWN, reading the active document threw "
                    + error.GetType().FullName + ": " + error.Message;
            }
        }

        public static string PluginVersion()
        {
            try
            {
                Assembly assembly = typeof(FederatorPlugin).Assembly;
                Version version = assembly.GetName().Version;
                return version == null ? "UNKNOWN" : version.ToString();
            }
            catch (Exception error)
            {
                return "UNKNOWN, " + error.GetType().Name + ": " + error.Message;
            }
        }

        private static string Number(int value)
        {
            return value.ToString(CultureInfo.InvariantCulture);
        }

        private static string Or(string value, string fallback)
        {
            return string.IsNullOrEmpty(value) ? fallback : value;
        }
    }
}
