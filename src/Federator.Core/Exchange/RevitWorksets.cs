using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace Federator.Core.Exchange
{
    /// <summary>
    /// Every workset name a Revit model in this project really carries, measured off the
    /// models and never typed, Q68 and Q69.
    ///
    /// IT IS THE SPELLINGS AND NOT A JUDGEMENT. The client's matrix asks for a workset
    /// called `ME-DUCTWORK` and every model writes `ME-Ductwork`, and a search condition
    /// compares a value CASE SENSITIVELY unless a flag nothing sets is set, so three
    /// mechanical sets found nothing in every group of every run, 5q. `MatrixCorrections`
    /// corrects the matrix to the spelling the models use and THIS is where the spellings
    /// come from. Nothing in this tool ever invents one.
    ///
    /// IT IS NOT THE PENETRATION LISTS AND NOT THE CATEGORY LIST. Those two say what
    /// somebody has decided about a category. This says what is in the models, the same
    /// way `RevitCategories` does and for the same reason, and it ships inside the DLL as
    /// an embedded resource so it cannot arrive on 26 machines and not on the 27th.
    /// </summary>
    public static class RevitWorksets
    {
        /// <summary>The resource the list lives in, named once.</summary>
        public const string ResourceName = "Federator.Core.Exchange.revit-worksets.txt";

        private static readonly object Gate = new object();
        private static List<string> known;
        private static bool resourceFound;

        /// <summary>Every workset the list names, in the order the file wrote them.</summary>
        public static IList<string> All()
        {
            return new List<string>(Load());
        }

        /// <summary>How many the list names. Zero until it is measured.</summary>
        public static int Count
        {
            get { return Load().Count; }
        }

        /// <summary>Whether the list has been measured at all. Everything that reads it asks this first.</summary>
        public static bool Measured
        {
            get { return Count > 0; }
        }

        /// <summary>
        /// Whether the list could be READ out of the DLL at all. A resource that is
        /// missing is a different fact from a list nobody has filled in yet, and the two
        /// would otherwise give the same empty list and the same words.
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
        /// The spellings the models carry for that value, differing from it by CASE
        /// ALONE, in the order the list holds them. A name differing by a letter is a
        /// different word and is never offered, which 5t proved twice over on this
        /// project: `AR-EXTERIOR` against `AR-INTERIOR` and `ST-SUB` against `ST-SUP` are
        /// two pairs of real worksets one and two letters apart.
        /// </summary>
        public static IList<string> SpelledLike(string value)
        {
            List<string> found = new List<string>();

            if (string.IsNullOrEmpty(value))
            {
                return found;
            }

            List<string> all = Load();

            for (int i = 0; i < all.Count; i++)
            {
                if (string.Equals(all[i], value, StringComparison.OrdinalIgnoreCase))
                {
                    found.Add(all[i]);
                }
            }

            return found;
        }

        /// <summary>The one line about the list itself, so a reader knows whether it was read at all.</summary>
        public static string Line()
        {
            if (!ResourceFound)
            {
                return "Revit worksets known: UNKNOWN, the workset list could not be read out of "
                    + "Federator.Core.dll, so no value was corrected against them";
            }

            return Measured
                ? "Revit worksets known: " + Count
                : "Revit worksets known: none yet, so no value was corrected against them. "
                    + "The list is measured off a real federation, see the scan notes";
        }

        /// <summary>
        /// Reads the list once. A resource that cannot be read is an EMPTY list and never
        /// a throw, and an empty list corrects nothing, which is the safe answer.
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
                    Assembly assembly = typeof(RevitWorksets).Assembly;

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

                                known.Add(line);
                            }
                        }
                    }
                }
                catch (Exception)
                {
                    known = new List<string>();
                    resourceFound = false;
                }

                return known;
            }
        }
    }
}
