using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace Federator.Core.Exchange
{
    /// <summary>
    /// Every category value a Revit model in this project really carries, F84.
    ///
    /// WHY A FILE AND NOT A LIST IN A METHOD. This list is read off the CLIENT'S MODELS
    /// and it changes when their models do. UnitTable is compiled in for the opposite
    /// reason: the Navisworks units enum never changes, so a row of it can be typed once
    /// and trusted. A category list typed into a method would be wrong the first time a
    /// project added a family in a category nobody had seen, and the check built on it
    /// would then report a perfectly good set as broken.
    ///
    /// IT WAS EMPTY UNTIL IT WAS MEASURED, AND WHILE IT WAS EMPTY THE CHECK REPORTED
    /// NOTHING. The only way to fill it is to walk every item of a real federation and
    /// write out the distinct values of its category property, which needs Navisworks on
    /// the machine. That walk was run on 2026-09-20 over the ten C02 federations,
    /// docs\history\scan.md 5i, and the 374 values it found are the list, family names
    /// and all, because that is what the tool reads as a category. A test proves the
    /// list is exactly the probe's result. A check with nothing to compare against says
    /// so rather than calling every category in the client's file unknown, which is the
    /// rule about saying UNKNOWN rather than filling a gap, and that path stays for a
    /// build whose list is emptied to re-measure.
    ///
    /// IT IS NOT THE PENETRATION RULE'S LISTS. Those are a handful of categories this tool
    /// has made a judgement about, a service or a solid. This is every category that
    /// exists. Merging the two would make a penetration rule change whenever a model
    /// gained a category.
    ///
    /// IT SHIPS INSIDE THE DLL as an embedded resource, because the bundle copies what
    /// install.ps1 names and a loose file beside the assembly is a file that arrives on 26
    /// machines and not on the 27th.
    /// </summary>
    public static class RevitCategories
    {
        /// <summary>The resource the list lives in, named once.</summary>
        public const string ResourceName = "Federator.Core.Exchange.revit-categories.txt";

        /// <summary>
        /// How a line of a measured list names the project its models were measured on,
        /// FR-011, the project part of their file names. Named once, for this list and the
        /// workset list.
        /// </summary>
        public const string ProjectMarker = "project:";

        private static readonly object Gate = new object();
        private static List<string> known;
        private static bool resourceFound;
        private static string project;

        /// <summary>
        /// The project the list was measured on, read off its project line, or null where it
        /// names none, FR-011. A category no model of THIS project carries is only said where
        /// the group's own models name the same project.
        /// </summary>
        public static string Project
        {
            get
            {
                Load();
                return project;
            }
        }

        /// <summary>The project a line of a measured list names, or null where it is not a project line.</summary>
        internal static string ProjectIn(string line)
        {
            if (line == null || line.IndexOf(ProjectMarker, StringComparison.Ordinal) != 0)
            {
                return null;
            }

            string named = line.Substring(ProjectMarker.Length).Trim();
            return named.Length == 0 ? null : named;
        }

        /// <summary>
        /// Every category the list names, in the order the file wrote them. Empty until
        /// the list is measured, and empty is a real answer.
        /// </summary>
        public static IList<string> All()
        {
            return new List<string>(Load());
        }

        /// <summary>How many the list names. Zero until it is measured.</summary>
        public static int Count
        {
            get { return Load().Count; }
        }

        /// <summary>
        /// Whether the list has been measured at all. Everything that reads it asks this
        /// first, because a check that compares against an empty list would report every
        /// category in the client's file as one nobody has heard of.
        /// </summary>
        public static bool Measured
        {
            get { return Count > 0; }
        }

        /// <summary>
        /// Whether the list could be READ out of the DLL at all, A11. A resource that is
        /// missing or will not read is a different fact from a list nobody has filled in
        /// yet, and the two used to give the same empty list and the same words, so a
        /// build that lost the resource would have said none yet on every run and nobody
        /// would have known the check had stopped running.
        /// </summary>
        public static bool ResourceFound
        {
            get
            {
                Load();
                return resourceFound;
            }
        }

        /// <summary>
        /// The one line the health block carries about the list itself, so a reader knows
        /// whether the check ran at all rather than reading no findings as a clean file.
        /// A list that could not be read says UNKNOWN, never none yet.
        /// </summary>
        public static string Line()
        {
            if (!ResourceFound)
            {
                return "Revit categories known: UNKNOWN, the category list could not be read out of "
                    + "Federator.Core.dll, so no set was checked against them";
            }

            return Measured
                ? "Revit categories known: " + Count
                : "Revit categories known: none yet, so no set was checked against them. "
                    + "The list is measured off a real federation, see the scan notes";
        }

        /// <summary>
        /// Reads the list once. A resource that cannot be read is an EMPTY list and never
        /// a throw, because a health check is information and information never stops a
        /// run.
        /// </summary>
        private static List<string> Load()
        {
            lock (Gate)
            {
                if (known != null)
                {
                    return known;
                }

                known = new List<string>();

                try
                {
                    Assembly assembly = typeof(RevitCategories).Assembly;

                    using (Stream stream = assembly.GetManifestResourceStream(ResourceName))
                    {
                        if (stream == null)
                        {
                            resourceFound = false;
                            return known;
                        }

                        resourceFound = true;

                        using (StreamReader reader = new StreamReader(stream))
                        {
                            string line;

                            while ((line = reader.ReadLine()) != null)
                            {
                                if (line.Length == 0 || line[0] == '#')
                                {
                                    continue;
                                }

                                if (ProjectIn(line) != null)
                                {
                                    project = ProjectIn(line);
                                    continue;
                                }

                                known.Add(line);
                            }
                        }
                    }
                }
                catch (Exception)
                {
                    // A list that cannot be read is no list. The check then reports
                    // nothing, which is the same answer an unmeasured list gives.
                    known = new List<string>();
                    resourceFound = false;
                    project = null;
                }

                return known;
            }
        }
    }
}
