using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using Federator.Core.Exchange;
using Federator.Core.Health;
using Federator.Core.Sets;

namespace Federator.Core.Teams
{
    /// <summary>
    /// The sets of a group that can find nothing in a model of their own team with another code,
    /// F131, FR-181, Q114 point 3 in Bader's words: if a mechanical set asks for ME in the file
    /// name or an ME workset, and so finds nothing in an HV, PL or FP model, that is a silent miss,
    /// said in the COVERAGE block and in the form with a correction for Bader to approve, built the
    /// way the both-spellings rows are. Nothing here names a team, a code, a set or a workset.
    ///
    /// A CANDIDATE is a set of team T and code C and a model of team T whose code is not C, where
    /// EVERY GROUP of the set asks, not negated, a workset the model's whole workset list does not
    /// carry, or a Source File its file name does not hold. So it is a candidate whether or not the
    /// set found items in other models, which is point 3's case on 1A04PK, four ME models feeding
    /// a set while the HV model is missed. Workset values compare Ordinal, as measured, and a
    /// Source File asks contains, read against the NWC's file name, Bader's file name.
    ///
    /// A WORKSET LIST THAT IS NOT WHOLE GIVES UNKNOWN, NEVER A MISS. A model whose element walk did
    /// not finish carries no names, ModelExport.Counted, so a pair blocked only by its worksets is
    /// counted as not judged.
    ///
    /// NAMED ONLY WHERE THE COVERAGE CONFIRMS IT, the judge's rule of the design: the count of items
    /// of the set's categories in that model that no set catches, Q112 request 2, handed in by the
    /// caller, must be above zero. Otherwise a model with no ducts would be named against every
    /// duct set. Without that count a candidate is counted on one line that says UNKNOWN.
    ///
    /// THE CORRECTION IS DRAFTED AND NEVER APPLIED. For each workset value asked, the model's
    /// spellings whose text after the prefix, split on the separator, is the value's, compared case
    /// blind, go on one also-ask line of the list of corrections, which Bader approves by copying.
    /// Where no spelling matches, the spelling is UNKNOWN and nothing is drafted. A Source File has
    /// no line in the list, so nothing is drafted for one.
    ///
    /// WITH NO TEAM MAP every code is a team of its own, so no set is judged against another code.
    /// </summary>
    public sealed class SilentMisses
    {
        /// <summary>Navisworks' own name of the Source File property of an item, the same in every file it writes, read off the client's matrix.</summary>
        internal const string SourceFileProperty = "LcOaNodeSourceFile";

        private readonly TeamMap map;

        private SilentMisses(IList<SilentMiss> found, int unjudged, TeamMap map)
        {
            Found = new ReadOnlyCollection<SilentMiss>(found);
            Unjudged = unjudged;
            this.map = map;
        }

        /// <summary>Every candidate, confirmed or not, in the order of the sets and then of the models.</summary>
        public ReadOnlyCollection<SilentMiss> Found { get; private set; }

        /// <summary>How many set and model pairs of one team could not be judged, the model's worksets not all read.</summary>
        public int Unjudged { get; private set; }

        /// <summary>
        /// The candidates among those sets and the group's models, with that map, the separator of a
        /// set name's parts and of a workset's prefix, ViewpointSettings.SetNameSeparator, and the
        /// coverage count of a model file and a category, null where none was taken.
        /// </summary>
        public static SilentMisses Find(
            IEnumerable<SelectionSetDefinition> sets,
            IList<ModelExport> models,
            TeamMap map,
            char separator,
            Func<string, string, int?> uncaught)
        {
            if (map == null)
            {
                throw new ArgumentNullException("map");
            }

            List<SilentMiss> found = new List<SilentMiss>();
            int unjudged = 0;

            if (map.Teams.Count == 0 || sets == null || models == null)
            {
                return new SilentMisses(found, unjudged, map);
            }

            List<string> groupCodes = new List<string>();

            foreach (ModelExport model in models)
            {
                groupCodes.Add(model.Discipline);
            }

            IList<string> known = map.KnownCodes(groupCodes);

            foreach (SelectionSetDefinition set in sets)
            {
                string code = CodeOf.Set(set.Name, known, separator);

                if (code.Length == 0)
                {
                    continue;
                }

                string team = map.TeamOf(code);
                IList<IList<SearchConditionDefinition>> groups = PlannedSet.GroupsOf(
                    set.Conditions, condition => PlannedCondition.StartsAGroupWith(condition.Flags));

                foreach (ModelExport model in models)
                {
                    if (string.IsNullOrEmpty(model.Discipline)
                        || string.Equals(model.Discipline, code, StringComparison.Ordinal)
                        || !string.Equals(map.TeamOf(model.Discipline), team, StringComparison.Ordinal))
                    {
                        continue;
                    }

                    Reach reach = Judge(groups, model);

                    if (reach.Open)
                    {
                        continue;
                    }

                    if (!reach.Sure)
                    {
                        unjudged++;
                        continue;
                    }

                    found.Add(new SilentMiss(
                        set.Name,
                        code,
                        team,
                        model.File,
                        model.Discipline,
                        reach.Worksets,
                        reach.FileNames,
                        reach.Categories,
                        Count(model.File, reach.Categories, uncaught),
                        Drafts(reach.Worksets, model.Worksets, separator)));
                }
            }

            return new SilentMisses(found, unjudged, map);
        }

        /// <summary>
        /// The lines for the COVERAGE block and the form: one SILENT MISS line for each confirmed
        /// candidate with its drafted correction under it, then a count of the candidates the
        /// coverage did not confirm, of those it could not say, and of the pairs not judged.
        /// </summary>
        public IList<string> Lines()
        {
            List<string> lines = new List<string>();

            if (map.Teams.Count == 0)
            {
                lines.Add("no set was judged against the models of its team, because no team map maps a code");
                return lines;
            }

            int notConfirmed = 0;
            int notCounted = 0;

            foreach (SilentMiss miss in Found)
            {
                if (!miss.Uncaught.HasValue)
                {
                    notCounted++;
                    continue;
                }

                if (!miss.Confirmed)
                {
                    notConfirmed++;
                    continue;
                }

                lines.Add("SILENT MISS  " + miss.SetName + " finds nothing in " + miss.Model + ", " + map.CodeWithTeam(miss.ModelCode)
                    + ", because it asks " + Asks(miss) + ". That model holds "
                    + miss.Uncaught.Value.ToString(CultureInfo.InvariantCulture) + " item(s) of "
                    + string.Join(" or ", new List<string>(miss.Categories).ToArray()) + " that no set catches");

                foreach (string draft in miss.Drafted)
                {
                    lines.Add("   to approve it, copy this line into the list of corrections beside the XML: " + draft);
                }

                if (miss.Drafted.Count == 0 && miss.WorksetsAsked.Count > 0)
                {
                    lines.Add("   no workset of that model is spelled like " + Either(miss.WorksetsAsked)
                        + " after its prefix, so the spelling to accept is UNKNOWN and no line is drafted");
                }

                if (miss.FileNameAsks.Count > 0)
                {
                    lines.Add("   no line is drafted for a Source File, which the list of corrections has no line for");
                }
            }

            if (notConfirmed > 0)
            {
                lines.Add(notConfirmed.ToString(CultureInfo.InvariantCulture)
                    + " set and model pair(s) of one team where the set cannot reach the model are not listed, because the model"
                    + " holds no item of the set's categories that no set catches");
            }

            if (notCounted > 0)
            {
                lines.Add(notCounted.ToString(CultureInfo.InvariantCulture)
                    + " set and model pair(s) of one team where the set cannot reach the model are not listed, because whether the"
                    + " model holds items of the set's categories that no set catches is UNKNOWN until the coverage counts them");
            }

            if (Unjudged > 0)
            {
                lines.Add(Unjudged.ToString(CultureInfo.InvariantCulture)
                    + " set and model pair(s) of one team could not be judged, because the model's worksets were not all read");
            }

            if (lines.Count == 0)
            {
                lines.Add("no set asks a workset or a file name that a model of its own team with another code does not carry");
            }

            return lines;
        }

        private static string Asks(SilentMiss miss)
        {
            List<string> asks = new List<string>();

            if (miss.WorksetsAsked.Count > 0)
            {
                asks.Add("the workset " + Either(miss.WorksetsAsked) + ", which that model does not carry");
            }

            if (miss.FileNameAsks.Count > 0)
            {
                asks.Add("a Source File holding " + Either(miss.FileNameAsks) + ", which that model's file name does not hold");
            }

            return string.Join(" and ", asks.ToArray());
        }

        private static string Either(IList<string> values)
        {
            return string.Join(" or ", new List<string>(values).ToArray());
        }

        /// <summary>
        /// Whether the set's groups can reach that model. A group is closed for sure where a
        /// condition of it, not negated, asks a Source File the model's file name does not hold or
        /// a workset its whole list does not carry, and closed perhaps where only a workset closes
        /// it and the list is not whole. A test this does not read, on a property this does not
        /// read, closes nothing.
        /// </summary>
        private static Reach Judge(IList<IList<SearchConditionDefinition>> groups, ModelExport model)
        {
            Reach reach = new Reach();
            reach.Open = groups.Count == 0;
            reach.Sure = true;

            foreach (IList<SearchConditionDefinition> group in groups)
            {
                bool closedForSure = false;
                bool closedPerhaps = false;

                foreach (SearchConditionDefinition condition in group)
                {
                    if (condition.Property == null || condition.Value == null || (condition.Flags & MatrixCorrections.NegateCondition) != 0)
                    {
                        continue;
                    }

                    string property = condition.Property.InternalName;
                    string value = condition.Value.Data ?? string.Empty;
                    bool equals = string.Equals(condition.Test, SetBuildPlan.EqualsTest, StringComparison.Ordinal);
                    bool contains = string.Equals(condition.Test, SetBuildPlan.ContainsTest, StringComparison.Ordinal);

                    if (string.Equals(property, EmptySets.CategoryProperty, StringComparison.Ordinal) && equals)
                    {
                        Once(reach.Categories, value);
                    }
                    else if (string.Equals(property, EmptySets.WorksetProperty, StringComparison.Ordinal) && (equals || contains)
                        && !Carries(model.Worksets, value, contains))
                    {
                        Once(reach.Worksets, value);

                        if (model.Counted)
                        {
                            closedForSure = true;
                        }
                        else
                        {
                            closedPerhaps = true;
                        }
                    }
                    else if (string.Equals(property, SourceFileProperty, StringComparison.Ordinal) && contains
                        && model.File.IndexOf(value, StringComparison.Ordinal) < 0)
                    {
                        Once(reach.FileNames, value);
                        closedForSure = true;
                    }
                }

                if (!closedForSure && !closedPerhaps)
                {
                    reach.Open = true;
                }
                else if (!closedForSure)
                {
                    reach.Sure = false;
                }
            }

            return reach;
        }

        private static bool Carries(IList<string> worksets, string value, bool contains)
        {
            foreach (string workset in worksets)
            {
                if (contains
                    ? workset.IndexOf(value, StringComparison.Ordinal) >= 0
                    : string.Equals(workset, value, StringComparison.Ordinal))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// The coverage count of those categories in that model, added up: above zero where any is,
        /// zero where every one was counted and none is, and null where no count was handed in, one
        /// was not taken, or the set asks no category.
        /// </summary>
        private static int? Count(string file, IList<string> categories, Func<string, string, int?> uncaught)
        {
            if (uncaught == null || categories.Count == 0)
            {
                return null;
            }

            int sum = 0;
            bool unknown = false;

            foreach (string category in categories)
            {
                int? count = uncaught(file, category);

                if (count.HasValue)
                {
                    sum += Math.Max(0, count.Value);
                }
                else
                {
                    unknown = true;
                }
            }

            if (sum > 0)
            {
                return sum;
            }

            return unknown ? (int?)null : 0;
        }

        /// <summary>
        /// One also-ask line for each workset value asked, the first value of those spelled alike
        /// but for their case, Q102's, with every spelling of the model whose text after its prefix
        /// is the value's, case blind. A line of the list cannot hold the bar that splits its
        /// parts, so a name holding it is never drafted.
        /// </summary>
        private static IList<string> Drafts(IList<string> asked, IList<string> carried, char separator)
        {
            List<string> drafts = new List<string>();
            List<string> done = new List<string>();

            foreach (string value in asked)
            {
                string after = AfterPrefix(value, separator);

                if (after == null
                    || value.IndexOf(ListFile.Bar, StringComparison.Ordinal) >= 0
                    || done.Exists(one => string.Equals(one, after, StringComparison.OrdinalIgnoreCase)))
                {
                    continue;
                }

                done.Add(after);
                List<string> line = new List<string> { value };

                foreach (string workset in carried)
                {
                    string theirs = AfterPrefix(workset, separator);

                    if (theirs != null
                        && string.Equals(theirs, after, StringComparison.OrdinalIgnoreCase)
                        && !asked.Contains(workset)
                        && !line.Contains(workset)
                        && workset.IndexOf(ListFile.Bar, StringComparison.Ordinal) < 0)
                    {
                        line.Add(workset);
                    }
                }

                if (line.Count > 1)
                {
                    drafts.Add(MatrixCorrectionList.AlsoAskMarker + " " + string.Join(ListFile.Bar, line.ToArray()));
                }
            }

            return drafts;
        }

        /// <summary>The text after the first separator, or null where there is none or nothing after it.</summary>
        private static string AfterPrefix(string name, char separator)
        {
            int at = name.IndexOf(separator);

            return at < 0 || at == name.Length - 1 ? null : name.Substring(at + 1);
        }

        private static void Once(IList<string> into, string value)
        {
            if (!into.Contains(value))
            {
                into.Add(value);
            }
        }

        /// <summary>What a set's groups ask of one model and whether they can reach it.</summary>
        private sealed class Reach
        {
            internal Reach()
            {
                Worksets = new List<string>();
                FileNames = new List<string>();
                Categories = new List<string>();
            }

            /// <summary>Whether some group can find items in the model.</summary>
            internal bool Open { get; set; }

            /// <summary>Whether every closed group is closed for sure, never on a workset list not whole.</summary>
            internal bool Sure { get; set; }

            internal List<string> Worksets { get; private set; }

            internal List<string> FileNames { get; private set; }

            internal List<string> Categories { get; private set; }
        }
    }
}
