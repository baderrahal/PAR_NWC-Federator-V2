using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace Federator.Core.Diagnostics
{
    /// <summary>
    /// Loads this add-in's own assemblies out of its bundle folder by simple name,
    /// ignoring the version.
    ///
    /// WHY THIS EXISTS. Measured on 2026-08-31 against the installed bundle. Every file
    /// the workbook needs was already there and the workbook still would not write:
    ///
    ///     SixLabors.Fonts       needs System.Numerics.Vectors 4.1.3.0, the file is 4.1.4.0
    ///     System.Memory         needs System.Runtime.CompilerServices.Unsafe 4.0.4.1,
    ///                           the file is 4.0.6.0
    ///     ClosedXML             needs System.Buffers 4.0.2.0, the file is 4.0.3.0
    ///     SixLabors.Fonts       needs System.Memory 4.0.1.1, the file is 4.0.1.2
    ///
    /// Not one file is missing. All four are version mismatches, and .NET Framework binds
    /// a strong named assembly by exact version, so it refuses a file that is only a build
    /// number away. The usual answer is a bindingRedirect, and NuGet writes those into the
    /// application config. A Navisworks add-in has no config of its own, it runs inside
    /// Roamer.exe and its config belongs to Autodesk, so there is nowhere to put one.
    ///
    /// An AssemblyResolve handler is what is left, and it is better than a redirect here.
    /// An assembly handed back from AssemblyResolve is accepted without a version check,
    /// so this survives a version mismatch and a missing file in the same way, and it
    /// keeps working when a package bumps a build number.
    ///
    /// It lives in Core rather than in the add-in so the whole of it can be tested without
    /// Navisworks. Only the hooking up happens in the plugin.
    /// </summary>
    public static class BundleAssemblies
    {
        private static readonly object Gate = new object();
        private static bool installed;
        private static string bundleFolder;
        private static Action<string> report;

        /// <summary>
        /// A resource lookup is asked for by name plus .resources and is meant to fail
        /// when there is no satellite assembly. Answering one would recurse.
        /// </summary>
        public const string ResourceSuffix = ".resources";

        /// <summary>The simple name out of a full assembly name, which is everything before the first comma.</summary>
        public static string SimpleName(string fullName)
        {
            if (string.IsNullOrEmpty(fullName))
            {
                return string.Empty;
            }

            int comma = fullName.IndexOf(',');
            return (comma < 0 ? fullName : fullName.Substring(0, comma)).Trim();
        }

        /// <summary>True for the satellite resource lookups that are meant to fail.</summary>
        public static bool IsResourceRequest(string fullName)
        {
            return SimpleName(fullName).EndsWith(ResourceSuffix, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// The file in the folder that answers this request, or null when there is none.
        /// Matched on the simple name alone, which is the whole point: the version the
        /// caller asked for is the thing that does not match.
        /// </summary>
        public static string FileFor(string fullName, string folder)
        {
            if (string.IsNullOrEmpty(folder) || IsResourceRequest(fullName))
            {
                return null;
            }

            string name = SimpleName(fullName);

            if (name.Length == 0)
            {
                return null;
            }

            foreach (string extension in new[] { ".dll", ".exe" })
            {
                string path;

                try
                {
                    path = Path.Combine(folder, name + extension);
                }
                catch (ArgumentException)
                {
                    // A name holding characters that are not legal in a path is not one of
                    // ours, so there is nothing to answer with.
                    return null;
                }

                if (File.Exists(path))
                {
                    return path;
                }
            }

            return null;
        }

        /// <summary>
        /// Hooks the handler up, once, against the folder this assembly is in. Called from
        /// the plugin before anything else, so it is already there when the workbook
        /// writer first touches ClosedXML.
        /// </summary>
        public static void InstallBesideThisAssembly(Action<string> note)
        {
            Install(FolderOfThisAssembly(), note);
        }

        public static void Install(string folder, Action<string> note)
        {
            lock (Gate)
            {
                report = note;

                if (installed)
                {
                    return;
                }

                bundleFolder = folder;
                AppDomain.CurrentDomain.AssemblyResolve += Resolve;
                installed = true;
            }

            Say("BUNDLE   assemblies are resolved from " + Words.Or(folder, "UNKNOWN, this assembly has no location"));
        }

        /// <summary>Where this assembly is, which is the bundle folder. Empty when it has no location.</summary>
        public static string FolderOfThisAssembly()
        {
            try
            {
                string location = typeof(BundleAssemblies).Assembly.Location;

                return string.IsNullOrEmpty(location)
                    ? string.Empty
                    : Path.GetDirectoryName(location);
            }
            catch (Exception)
            {
                // An assembly loaded from bytes has no location. Nothing to resolve from,
                // and never worth throwing over.
                return string.Empty;
            }
        }

        private static Assembly Resolve(object sender, ResolveEventArgs args)
        {
            try
            {
                string wanted = args == null ? null : args.Name;

                if (IsResourceRequest(wanted))
                {
                    return null;
                }

                string name = SimpleName(wanted);

                // Already loaded under this simple name at some other version. Handing the
                // same one back is what makes a mismatch work rather than loading a second
                // copy of it.
                Assembly already = AlreadyLoaded(name);

                if (already != null)
                {
                    Remember(name + " was already loaded, that one was used");
                    return already;
                }

                string path = FileFor(wanted, bundleFolder);

                if (path == null)
                {
                    return null;
                }

                Assembly loaded = Assembly.LoadFrom(path);
                Remember(wanted + " resolved from " + path);
                return loaded;
            }
            catch (Exception)
            {
                // Never throw out of a resolve handler. Returning null lets the runtime
                // report the original failure, which says more than this would.
                return null;
            }
        }

        private static Assembly AlreadyLoaded(string name)
        {
            foreach (Assembly candidate in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (string.Equals(candidate.GetName().Name, name, StringComparison.OrdinalIgnoreCase))
                {
                    return candidate;
                }
            }

            return null;
        }

        private static void Remember(string what)
        {
            Say("BUNDLE   " + what);
        }

        private static void Say(string line)
        {
            Action<string> note;

            lock (Gate)
            {
                note = report;
            }

            if (note == null)
            {
                return;
            }

            try
            {
                note(line);
            }
            catch (Exception)
            {
                // Reporting is never the thing that stops a load.
            }
        }
    }
}
