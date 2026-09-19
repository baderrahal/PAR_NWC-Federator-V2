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
    /// IT IS EMPTY UNTIL IT IS MEASURED, AND WHILE IT IS EMPTY THE CHECK REPORTS NOTHING.
    /// The only way to fill it is to walk every item of a real federation and write out
    /// the distinct values of its category property, which needs Navisworks on the
    /// machine. That is docs\history\scan.md 5i, and the property probe does the same walk
    /// as part of its own job so the two are measured on one run. A check with nothing to
    /// compare against says so rather than calling every category in the client's file
    /// unknown, which is the rule about saying UNKNOWN rather than filling a gap.
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

        private static readonly object Gate = new object();
        private static List<string> known;

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
        /// Whether that value is a category this project's models carry. Compared Ordinal
        /// and never trimmed, the same as every other name read out of the exchange file.
        /// Always TRUE while the list is unmeasured, so nothing is reported on a guess.
        /// </summary>
        public static bool Holds(string category)
        {
            if (!Measured || string.IsNullOrEmpty(category))
            {
                return true;
            }

            return Load().Contains(category);
        }

        /// <summary>
        /// The one line the health block carries about the list itself, so a reader knows
        /// whether the check ran at all rather than reading no findings as a clean file.
        /// </summary>
        public static string Line()
        {
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
                            return known;
                        }

                        using (StreamReader reader = new StreamReader(stream))
                        {
                            string line;

                            while ((line = reader.ReadLine()) != null)
                            {
                                if (line.Length == 0 || line[0] == '#')
                                {
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
                }

                return known;
            }
        }
    }
}
