using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using Federator.Core.Exchange;
using Federator.Core.Sets;

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
    /// A set asking for category values no model in this project carries, F84, ONE FINDING PER
    /// SET however many such values it asks, because the block counts sets. A group of its
    /// conditions that asks one can never match, so a set whose every Or group asks one can
    /// never match anything and every clash test that names it can never find a clash. A set
    /// with a group asking none of them can still match through that group, and its line says
    /// how many groups ask one, FR-023. It was one finding per value, each counting only the
    /// groups that asked that value, so a set whose two groups asked two different missing
    /// values was named twice, each line saying a group could still match, and counted twice.
    /// </summary>
    public sealed class CategoryNobodyHas
    {
        internal CategoryNobodyHas(SelectionSetDefinition set, IList<string> categories, int groupsAsking, int groups)
        {
            Set = set;
            Categories = new ReadOnlyCollection<string>(new List<string>(categories));
            GroupsAsking = groupsAsking;
            Groups = groups;
        }

        public SelectionSetDefinition Set { get; private set; }

        /// <summary>Every value the set asks that no model carries, each once, exactly as the file wrote it, in the file's order.</summary>
        public ReadOnlyCollection<string> Categories { get; private set; }

        /// <summary>How many of the set's Or groups ask for one of them, and so can never match.</summary>
        public int GroupsAsking { get; private set; }

        /// <summary>How many Or groups the set holds, one more than the conditions starting a group, F78.</summary>
        public int Groups { get; private set; }

        public override string ToString()
        {
            List<string> quoted = new List<string>();

            foreach (string category in Categories)
            {
                quoted.Add("\"" + category + "\"");
            }

            return Set.Name + " asks for " + string.Join(" and ", quoted.ToArray())
                + (GroupsAsking < Groups
                    ? " in " + GroupsAsking + " of its " + Groups + " Or groups, so a group without "
                        + (Categories.Count == 1 ? "it" : "them") + " can still match"
                    : string.Empty);
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
        /// THE SIGNATURE CARRIES THE TWO FLAG BITS THAT ARE PART OF THE QUESTION, FR-024: the
        /// negation and the start of an Or group, `PlannedCondition.QuestionFlagsOf`, the rule
        /// the drift key reads. Two sets that differ only in how their conditions are grouped,
        /// or in a negation, ask a different question and are not identical. It left every
        /// flag out, by SearchConditionDefinition's RuleSignature, while this comment said
        /// the opposite. RuleSignature itself still leaves them out, because the distinct rule
        /// count reads it, and the Ignore bits stay out of both. It is the ORDERED list,
        /// because the same two conditions in the other order are the same question only when
        /// the grouping is the same, and comparing them unordered would report a pair that is
        /// not one.
        /// </summary>
        public static IList<IdenticalSets> FindIdentical(IEnumerable<SelectionSetDefinition> sets)
        {
            return FindIdentical(sets, SignatureOf);
        }

        /// <summary>
        /// Sets grouped by the signature handed, in the order each first came, a group only
        /// where it holds two or more and a set with no condition never grouped. The HEALTH
        /// block groups by SignatureOf. The mirrored tests group by the whole question a set
        /// asks, SelectionSetDefinition.WholeQuestion, F132, because a pair of tests merged on
        /// a comparison that leaves a flag out would add the clashes of one question to
        /// another. One grouping, two signatures, so the two cannot drift in how they group.
        /// </summary>
        internal static IList<IdenticalSets> FindIdentical(
            IEnumerable<SelectionSetDefinition> sets, Func<SelectionSetDefinition, string> signatureOf)
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

                    string signature = signatureOf(set);
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
                parts.Add(condition.RuleSignature + "\u0002"
                    + PlannedCondition.QuestionFlagsOf(condition.Flags).ToString(CultureInfo.InvariantCulture));
            }

            // Separators no rule signature carries, so two different lists cannot join
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

                IList<IList<SearchConditionDefinition>> groups = GroupsOf(set);

                // ONE FINDING PER SET, however many of its groups ask a missing value and however
                // many values, because the block counts sets. Since F116 a set asks each spelling
                // of its workset in a group of its own, each carrying the category. A group is
                // counted as asking one when ANY of its conditions asks any missing value, so the
                // claim that a group can still match is made only where some group asks none.
                List<string> missing = new List<string>();
                int groupsAsking = 0;

                foreach (IList<SearchConditionDefinition> group in groups)
                {
                    bool asksOne = false;

                    foreach (SearchConditionDefinition condition in group)
                    {
                        string asked = MissingCategory(condition, categoryPropertyInternalName);

                        if (asked == null)
                        {
                            continue;
                        }

                        asksOne = true;

                        if (!missing.Contains(asked))
                        {
                            missing.Add(asked);
                        }
                    }

                    if (asksOne)
                    {
                        groupsAsking++;
                    }
                }

                if (missing.Count > 0)
                {
                    found.Add(new CategoryNobodyHas(set, missing, groupsAsking, groups.Count));
                }
            }

            return found;
        }

        /// <summary>The category value that condition asks and no model carries, or null where it asks none.</summary>
        private static string MissingCategory(SearchConditionDefinition condition, string categoryPropertyInternalName)
        {
            if (!AsksTheCategory(condition, categoryPropertyInternalName))
            {
                return null;
            }

            string asked = condition.Value == null ? string.Empty : condition.Value.Data;

            return asked.Length == 0 || Known(condition.Test, asked) ? null : asked;
        }

        /// <summary>
        /// Whether that condition asks for a category, on the category property, and is one the
        /// EMPTY SETS judge would judge, by its one rule, EmptySets.Judgeable: NOT NEGATED, FR-023,
        /// and asking equals or contains. A negated condition asks for everything but its value, so
        /// BLD-EL-Devices, which leaves out Telephone Devices with flags 32, was named as asking for
        /// it. A test the file never writes was read as equals here while the judge said it cannot
        /// tell, so the two blocks disagreed about one set.
        /// </summary>
        private static bool AsksTheCategory(SearchConditionDefinition condition, string categoryPropertyInternalName)
        {
            return condition.Property != null
                && string.Equals(condition.Property.InternalName, categoryPropertyInternalName, StringComparison.Ordinal)
                && EmptySets.Judgeable(condition.Test, condition.Flags);
        }

        /// <summary>The set's conditions in their Or groups, by the plan's own grouping rule, F78.</summary>
        private static IList<IList<SearchConditionDefinition>> GroupsOf(SelectionSetDefinition set)
        {
            return PlannedSet.GroupsOf(set.Conditions, condition => PlannedCondition.StartsAGroupWith(condition.Flags));
        }

        /// <summary>The test attribute a condition carries when its value is a stem.</summary>
        public const string ContainsTest = "contains";

        /// <summary>
        /// Whether the measured categories carry what the condition asks, by the one rule the
        /// EMPTY SETS judge reads too, EmptySets.Carries, FR-010, read only for a condition both
        /// blocks judge, AsksTheCategory, so the two never disagree about a contains condition
        /// nor about a test the file never writes.
        /// </summary>
        private static bool Known(string test, string asked)
        {
            return EmptySets.Carries(RevitCategories.All(), test, asked);
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
