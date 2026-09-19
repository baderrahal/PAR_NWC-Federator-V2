using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Federator.Core.Exchange;

namespace Federator.Core.Health
{
    /// <summary>
    /// Two or more sets that ask the model exactly the same question, F84. Every clash
    /// test of one is the same test as the matching test of the other, so half of them
    /// are run for nothing.
    /// </summary>
    public sealed class IdenticalSets
    {
        internal IdenticalSets(IList<SelectionSetDefinition> sets)
        {
            Sets = new ReadOnlyCollection<SelectionSetDefinition>(sets);
        }

        public ReadOnlyCollection<SelectionSetDefinition> Sets { get; private set; }

        public int Count
        {
            get { return Sets.Count; }
        }

        /// <summary>The names, in the order the file wrote them.</summary>
        public IList<string> Names()
        {
            List<string> names = new List<string>();

            foreach (SelectionSetDefinition set in Sets)
            {
                names.Add(set.Name);
            }

            return names;
        }

        public override string ToString()
        {
            return string.Join(" and ", new List<string>(Names()).ToArray());
        }
    }

    /// <summary>
    /// A set asking for a category value no model in this project carries, F84. It can
    /// never match anything, so every clash test that names it can never find a clash.
    /// </summary>
    public sealed class CategoryNobodyHas
    {
        internal CategoryNobodyHas(SelectionSetDefinition set, string category)
        {
            Set = set;
            Category = category;
        }

        public SelectionSetDefinition Set { get; private set; }

        /// <summary>The value the set asked for, exactly as the file wrote it.</summary>
        public string Category { get; private set; }

        public override string ToString()
        {
            return Set.Name + " asks for \"" + Category + "\"";
        }
    }

    /// <summary>
    /// A set whose name breaks the pattern the sets in its own folder follow, F84.
    /// </summary>
    public sealed class OddSetName
    {
        internal OddSetName(SelectionSetDefinition set, string shape, string theOthers, int howManyOthers)
        {
            Set = set;
            Shape = shape;
            TheOthers = theOthers;
            HowManyOthers = howManyOthers;
        }

        public SelectionSetDefinition Set { get; private set; }

        /// <summary>The name's own shape, which is everything before its last part.</summary>
        public string Shape { get; private set; }

        /// <summary>The shape the rest of the folder holds.</summary>
        public string TheOthers { get; private set; }

        /// <summary>How many sets in that folder hold it.</summary>
        public int HowManyOthers { get; private set; }

        public override string ToString()
        {
            return Set.Name + " reads " + Shape + " where " + HowManyOthers
                + " others in the same folder read " + TheOthers;
        }
    }

    /// <summary>
    /// The three warnings F84 adds about a selection set that cannot match anything.
    ///
    /// EVERY ONE OF THEM IS INFORMATION AND NOTHING ACTS ON IT. A finding does not correct
    /// a name, does not drop a set, does not skip a test and does not fail a group. It is
    /// a line in the HEALTH block, and Bader decides. That is the rule this tool is built
    /// on and F84 is no exception to it.
    /// </summary>
    public static class SetWarnings
    {
        /// <summary>
        /// Sets whose whole ordered list of conditions is identical, F84.
        ///
        /// The client's matrix holds two such pairs and they are already measured:
        /// Telecom Fixtures with Telephone Devices, and Electrical Fixtures with Devices.
        /// That is why 61 sets carry only 59 distinct rule lists. Every clash test of one
        /// is the same test as the matching test of the other, so 60 tests of each pair
        /// are run twice for the same answer.
        ///
        /// THE SIGNATURE LEAVES THE FLAGS OUT, which is SearchConditionDefinition's own
        /// rule and is right here too: two sets that differ only in how their conditions
        /// are grouped ask a different question and are not identical. It is the ORDERED
        /// list, because the same two conditions in the other order are the same question
        /// only when the grouping is the same, and comparing them unordered would report
        /// a pair that is not one.
        /// </summary>
        public static IList<IdenticalSets> FindIdentical(IEnumerable<SelectionSetDefinition> sets)
        {
            List<string> order = new List<string>();
            Dictionary<string, List<SelectionSetDefinition>> bySignature =
                new Dictionary<string, List<SelectionSetDefinition>>(StringComparer.Ordinal);

            if (sets != null)
            {
                foreach (SelectionSetDefinition set in sets)
                {
                    if (set == null || set.Conditions.Count == 0)
                    {
                        // A set with no condition at all is a different finding and is
                        // already reported elsewhere. Grouping them here would say every
                        // empty set is a copy of every other empty set.
                        continue;
                    }

                    string signature = SignatureOf(set);
                    List<SelectionSetDefinition> bucket;

                    if (!bySignature.TryGetValue(signature, out bucket))
                    {
                        bucket = new List<SelectionSetDefinition>();
                        bySignature.Add(signature, bucket);
                        order.Add(signature);
                    }

                    bucket.Add(set);
                }
            }

            List<IdenticalSets> found = new List<IdenticalSets>();

            foreach (string signature in order)
            {
                if (bySignature[signature].Count > 1)
                {
                    found.Add(new IdenticalSets(bySignature[signature]));
                }
            }

            return found;
        }

        /// <summary>One set's whole ordered condition list, as one string.</summary>
        public static string SignatureOf(SelectionSetDefinition set)
        {
            if (set == null)
            {
                return string.Empty;
            }

            List<string> parts = new List<string>();

            foreach (SearchConditionDefinition condition in set.Conditions)
            {
                parts.Add(condition.RuleSignature);
            }

            // A separator no rule signature carries, so two different lists cannot join
            // into one string that reads the same.
            return string.Join("\u0001", parts.ToArray());
        }

        /// <summary>
        /// Sets asking for a category value no model in this project carries, F84.
        ///
        /// NOTHING IS REPORTED WHILE THE CATEGORY LIST IS UNMEASURED, because a check with
        /// nothing to compare against would call every category in the client's file one
        /// nobody has heard of. RevitCategories.Measured is what answers that, and the
        /// HEALTH block says which of the two happened rather than leaving a reader to
        /// read no findings as a clean file.
        ///
        /// A CONDITION WRITTEN AS contains IS A STEM AND NOT A CATEGORY. Cable Tray matches
        /// Cable Trays and Cable Tray Fittings, so a contains condition is checked by
        /// asking whether any known category holds it rather than whether one equals it.
        /// </summary>
        public static IList<CategoryNobodyHas> FindCategoriesNobodyHas(
            IEnumerable<SelectionSetDefinition> sets, string categoryPropertyInternalName)
        {
            List<CategoryNobodyHas> found = new List<CategoryNobodyHas>();

            if (sets == null || !RevitCategories.Measured)
            {
                return found;
            }

            foreach (SelectionSetDefinition set in sets)
            {
                if (set == null)
                {
                    continue;
                }

                foreach (SearchConditionDefinition condition in set.Conditions)
                {
                    if (condition.Property == null
                        || !string.Equals(
                            condition.Property.InternalName, categoryPropertyInternalName,
                            StringComparison.Ordinal))
                    {
                        continue;
                    }

                    string asked = condition.Value == null ? string.Empty : condition.Value.Data;

                    if (asked.Length == 0 || Known(condition.Test, asked))
                    {
                        continue;
                    }

                    found.Add(new CategoryNobodyHas(set, asked));
                }
            }

            return found;
        }

        /// <summary>The test attribute a condition carries when its value is a stem.</summary>
        public const string ContainsTest = "contains";

        private static bool Known(string test, string asked)
        {
            if (string.Equals(test, ContainsTest, StringComparison.OrdinalIgnoreCase))
            {
                foreach (string category in RevitCategories.All())
                {
                    if (category.IndexOf(asked, StringComparison.Ordinal) >= 0)
                    {
                        return true;
                    }
                }

                return false;
            }

            return RevitCategories.Holds(asked);
        }

        /// <summary>
        /// The shape of a set name, which is everything before its last part, F84.
        /// BLD-EL-Devices reads BLD-EL and BLD-Security Devices reads BLD, which is what
        /// makes the second one stand out from the thirteen around it.
        ///
        /// IT IS NOT ScanFindings.Shape. That one is a letter and digit pattern of a fixed
        /// width building code and it is length sensitive, so on free text set names
        /// nearly every name would get a shape of its own and the majority guard below
        /// would never fire.
        /// </summary>
        public static string ShapeOf(string setName, char separator)
        {
            if (string.IsNullOrEmpty(setName))
            {
                return string.Empty;
            }

            string[] parts = setName.Split(separator);

            if (parts.Length < 2)
            {
                return string.Empty;
            }

            string[] before = new string[parts.Length - 1];
            Array.Copy(parts, before, parts.Length - 1);

            return string.Join(separator.ToString(), before);
        }

        /// <summary>
        /// Sets whose name breaks the pattern their own FOLDER follows, F84.
        ///
        /// A SIBLING GROUP IS A FOLDER AND NOT THE WHOLE FILE. The client's matrix proves
        /// it: 13 of the 14 Electrical sets read BLD-EL and one reads BLD, and every other
        /// folder is of one shape throughout. Comparing across the whole file would report
        /// every folder as odd against every other.
        ///
        /// THE MAJORITY GUARD. A shape is odd only when ONE set holds it and some OTHER
        /// shape in that folder is held by more than one, because with every shape held
        /// once there is no majority to differ from. Without it a folder of five
        /// differently named sets reports five findings and says nothing.
        ///
        /// The copy suffix is stripped first, or Manholes (2) reads as a pattern break.
        /// </summary>
        public static IList<OddSetName> FindOddNames(
            IEnumerable<SelectionSetDefinition> sets, char separator)
        {
            List<OddSetName> found = new List<OddSetName>();

            if (sets == null)
            {
                return found;
            }

            List<string> folderOrder = new List<string>();
            Dictionary<string, List<SelectionSetDefinition>> byFolder =
                new Dictionary<string, List<SelectionSetDefinition>>(StringComparer.Ordinal);

            foreach (SelectionSetDefinition set in sets)
            {
                if (set == null)
                {
                    continue;
                }

                string folder = string.Join("/", new List<string>(set.Folders).ToArray());
                List<SelectionSetDefinition> bucket;

                if (!byFolder.TryGetValue(folder, out bucket))
                {
                    bucket = new List<SelectionSetDefinition>();
                    byFolder.Add(folder, bucket);
                    folderOrder.Add(folder);
                }

                bucket.Add(set);
            }

            foreach (string folder in folderOrder)
            {
                found.AddRange(OddIn(byFolder[folder], separator));
            }

            return found;
        }

        private static IList<OddSetName> OddIn(
            IList<SelectionSetDefinition> sets, char separator)
        {
            List<OddSetName> found = new List<OddSetName>();
            List<string> order = new List<string>();
            Dictionary<string, List<SelectionSetDefinition>> byShape =
                new Dictionary<string, List<SelectionSetDefinition>>(StringComparer.Ordinal);

            foreach (SelectionSetDefinition set in sets)
            {
                string shape = ShapeOf(set.BaseName, separator);
                List<SelectionSetDefinition> bucket;

                if (!byShape.TryGetValue(shape, out bucket))
                {
                    bucket = new List<SelectionSetDefinition>();
                    byShape.Add(shape, bucket);
                    order.Add(shape);
                }

                bucket.Add(set);
            }

            string commonest = null;
            int most = 1;

            foreach (string shape in order)
            {
                if (byShape[shape].Count > most)
                {
                    most = byShape[shape].Count;
                    commonest = shape;
                }
            }

            if (commonest == null)
            {
                // Every shape held once. There is no majority to differ from, so nothing
                // here is odd, which is the same guard the scan findings already use.
                return found;
            }

            foreach (string shape in order)
            {
                if (byShape[shape].Count != 1)
                {
                    continue;
                }

                found.Add(new OddSetName(byShape[shape][0], shape, commonest, most));
            }

            return found;
        }
    }
}
