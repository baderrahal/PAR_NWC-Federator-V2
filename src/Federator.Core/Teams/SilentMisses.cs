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
    /// a set while the HV model is missed. Whether a workset asked finds a name the model carries
    /// is ExportCheck.WorksetFinds, the one place that is said. A Source File asks contains, read
    /// against the NWC's file name, Bader's file name, which STANDS IN for the Source File its
    /// items carry. That is not read here, and the line of each such miss says so.
    ///
    /// A WORKSET LIST THAT IS NOT WHOLE GIVES UNKNOWN, NEVER A MISS. A model whose element walk did
    /// not finish carries no names, ModelExport.Counted, so a pair blocked only by its worksets is
    /// counted as not judged.
    ///
    /// NAMED ONLY WHERE THE COVERAGE CONFIRMS IT, the judge's rule of the design: the count of items
    /// of the set's categories in that model that no set catches, Q112 request 2, handed in by the
    /// caller, must be above zero. Otherwise a model with no ducts would be named against every
    /// duct set. Without that count a candidate is counted on one line that says UNKNOWN. A count
    /// not taken, null or below zero as ModelExport.NotCounted is, is never read as a zero.
    ///
    /// THE CORRECTION IS DRAFTED AND NEVER APPLIED. For each workset value asked, the model's
    /// spellings whose text after the prefix, WorksetDisagreements.BodyOf, the one place a
    /// workset name's prefix is split, is the value's, compared case blind, go on one also-ask
    /// line of the list of corrections, which Bader approves by copying. Where no spelling
    /// matches, the spelling is UNKNOWN and nothing is drafted. A Source File has no line in the
    /// list, so nothing is drafted for one.
    ///
    /// A SET WHOSE NAME CARRIES NO CODE takes the team a folder above it in the clash XML's set
    /// tree names, TeamMap.TeamOfSet, Q117 answered C by Bader on 2026-10-05, and is judged
    /// against every model of that team, each having a code other than its none.
    ///
    /// WHAT IS NOT JUDGED IS COUNTED AND SAID. A set whose name carries no code the map or a model
    /// of the group knows and whose folders name no team, and a model whose code was not read,
    /// have a team that is UNKNOWN, Q117's A, so they are judged against nothing, and the lines
    /// count them beside the all clear. Where NO
    /// PAIR is judged, no set or no model handed in, a list of none being the same as no list, or
    /// no model of a set's team with another code, the lines say nothing was judged and why, and
    /// never the all clear, which would speak of sets and models never read. F131, the breaker's
    /// finding on its second attempt.
    ///
    /// AN EMPTY VALUE ASKS NO NAME. A condition whose value is empty closes nothing, as every
    /// other reader of workset values in this repo skips one.
    ///
    /// WITH NO TEAM MAP every code is a team of its own, so no set is judged against another code.
    ///
    /// EACH MODEL'S LINE of the group's TEAMS block carries its team beside its code, Q116
    /// answered A, and how many sets of its team with another code cannot reach it or could not
    /// be judged, so a model missed by many sets is one line and not one a set. A model of the
    /// group that was not read for its worksets is never handed in, so the block counts it
    /// against the group's models, the breaker's finding on attempt 1.
    /// </summary>
    public sealed class SilentMisses
    {
        /// <summary>The title of a group's block, which the add-in puts the building after.</summary>
        public const string BlockTitle = "TEAMS";

        /// <summary>Navisworks' own name of the Source File property of an item, the same in every file it writes, read off the client's matrix.</summary>
        internal const string SourceFileProperty = "LcOaNodeSourceFile";

        private readonly TeamMap map;

        /// <summary>Why no pair was judged, or null where one was.</summary>
        private readonly string notJudged;

        /// <summary>Whether no set was handed in, a run with no clash XML, so whether any set misses a model is UNKNOWN, K27.</summary>
        private readonly bool noSet;

        /// <summary>The models handed in, in their order, and for each how many sets of its team with another code were judged against it, missed it and could not be judged.</summary>
        private readonly IList<ModelExport> models;

        private readonly int[] pairsOf;

        private readonly int[] missedOf;

        private readonly int[] unjudgedOf;

        private SilentMisses(
            IList<SilentMiss> found,
            int unjudged,
            int setsWithNoCode,
            int modelsWithNoCode,
            string notJudged,
            bool noSet,
            TeamMap map,
            IList<ModelExport> models,
            int[] pairsOf,
            int[] missedOf,
            int[] unjudgedOf)
        {
            Found = new ReadOnlyCollection<SilentMiss>(found);
            Unjudged = unjudged;
            SetsWithNoCode = setsWithNoCode;
            ModelsWithNoCode = modelsWithNoCode;
            this.notJudged = notJudged;
            this.noSet = noSet;
            this.map = map;
            this.models = models ?? new List<ModelExport>();
            this.pairsOf = pairsOf ?? new int[this.models.Count];
            this.missedOf = missedOf ?? new int[this.models.Count];
            this.unjudgedOf = unjudgedOf ?? new int[this.models.Count];
        }

        /// <summary>Every candidate, confirmed or not, in the order of the sets and then of the models.</summary>
        public ReadOnlyCollection<SilentMiss> Found { get; private set; }

        /// <summary>How many set and model pairs of one team could not be judged, the model's worksets not all read.</summary>
        public int Unjudged { get; private set; }

        /// <summary>How many sets carry no code in their name that the map or a model of the group knows and sit in no folder naming a team, so their team is UNKNOWN and they were judged against no model.</summary>
        public int SetsWithNoCode { get; private set; }

        /// <summary>How many models carry no code, their file name not read for one, so their team is UNKNOWN and no set was judged against them.</summary>
        public int ModelsWithNoCode { get; private set; }

        /// <summary>
        /// The candidates among those sets and the group's models, with that map, the separator of a
        /// set name's parts, ViewpointSettings.SetNameSeparator, and the coverage count of a model
        /// file and a category, null or below zero where none was taken.
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
            List<SelectionSetDefinition> setList = sets == null ? new List<SelectionSetDefinition>() : new List<SelectionSetDefinition>(sets);
            bool noSet = setList.Count == 0;
            bool noModel = models == null || models.Count == 0;

            if (noSet || noModel)
            {
                string none = noSet && noModel ? "no set and no model were handed in" : noSet ? "no set was handed in" : "no model was handed in";

                return new SilentMisses(found, 0, 0, 0, none, noSet, map, models, null, null, null);
            }

            if (map.Teams.Count == 0)
            {
                return new SilentMisses(found, 0, 0, 0, null, false, map, models, null, null, null);
            }

            int pairs = 0;
            int unjudged = 0;
            int setsWithNoCode = 0;
            int modelsWithNoCode = 0;
            int[] pairsOf = new int[models.Count];
            int[] missedOf = new int[models.Count];
            int[] unjudgedOf = new int[models.Count];
            List<string> groupCodes = new List<string>();

            foreach (ModelExport model in models)
            {
                if (string.IsNullOrEmpty(model.Discipline))
                {
                    modelsWithNoCode++;
                }

                groupCodes.Add(model.Discipline);
            }

            IList<string> known = map.KnownCodes(groupCodes);

            foreach (SelectionSetDefinition set in setList)
            {
                string code = CodeOf.Set(set.Name, known, separator);
                string team = map.TeamOfSet(code, set.Folders);

                if (string.Equals(team, map.UnknownTeam, StringComparison.Ordinal))
                {
                    setsWithNoCode++;
                    continue;
                }

                IList<IList<SearchConditionDefinition>> groups = PlannedSet.GroupsOf(
                    set.Conditions, condition => PlannedCondition.StartsAGroupWith(condition.Flags));

                for (int m = 0; m < models.Count; m++)
                {
                    ModelExport model = models[m];

                    if (string.IsNullOrEmpty(model.Discipline)
                        || string.Equals(model.Discipline, code, StringComparison.Ordinal)
                        || !string.Equals(map.TeamOf(model.Discipline), team, StringComparison.Ordinal))
                    {
                        continue;
                    }

                    pairs++;
                    pairsOf[m]++;
                    Reach reach = Judge(groups, model);

                    if (reach.Open)
                    {
                        continue;
                    }

                    if (!reach.Sure)
                    {
                        unjudged++;
                        unjudgedOf[m]++;
                        continue;
                    }

                    missedOf[m]++;

                    bool whole;
                    int? count = Count(model.File, reach.Categories, uncaught, out whole);

                    found.Add(new SilentMiss(
                        set.Name,
                        code,
                        team,
                        model.File,
                        model.Discipline,
                        reach.Worksets,
                        reach.FileNames,
                        reach.Categories,
                        count,
                        whole,
                        Drafts(reach.Worksets, model.Worksets)));
                }
            }

            string noPair = pairs == 0 ? "no model of the group is of a set's team with a code other than the set's" : null;
            return new SilentMisses(
                found, unjudged, setsWithNoCode, modelsWithNoCode, noPair, false, map, models, pairsOf, missedOf, unjudgedOf);
        }

        /// <summary>
        /// The group's TEAMS block: one line for each model handed in, its code with its team beside
        /// it and how many sets of its team with another code cannot reach it or could not be
        /// judged, then how many of the group's models were not handed in, then the judge's lines.
        /// No model line where no map maps a team, the judge's line saying so.
        /// </summary>
        public IList<string> GroupLines(int modelsInGroup)
        {
            List<string> lines = new List<string>();

            if (map.Teams.Count > 0)
            {
                for (int m = 0; m < models.Count; m++)
                {
                    lines.Add("   " + ExportCheck.Named(models[m]) + "   " + map.CodeWithTeam(models[m].Discipline) + ", " + ModelVerdict(m));
                }
            }

            int notRead = modelsInGroup - models.Count;

            if (notRead > 0)
            {
                lines.Add(notRead.ToString(CultureInfo.InvariantCulture) + " of the " + modelsInGroup.ToString(CultureInfo.InvariantCulture)
                    + " model(s) of the group could not be read for their worksets, so no set was judged against them."
                    + " The EXPORT CHECK lines name each");
            }

            lines.AddRange(Lines());
            return lines;
        }

        /// <summary>What the judge came to for the model at that place, in the words of its line.</summary>
        private string ModelVerdict(int m)
        {
            const string OfItsTeam = " set(s) of its team with another code ";
            const string NotAllRead = " could not be judged, its worksets not all read";

            if (string.IsNullOrEmpty(models[m].Discipline))
            {
                return "its code is not read, so no set was judged against it";
            }

            // A run with no clash XML hands in no set, so nothing is known of the sets that would
            // have been judged, and the line says UNKNOWN where none judged would read as a result.
            if (noSet)
            {
                return "whether a set of its team with another code cannot reach it is UNKNOWN, because no set was handed in";
            }

            int pairs = pairsOf[m];
            string of = " of the " + pairs.ToString(CultureInfo.InvariantCulture) + OfItsTeam;

            if (pairs == 0)
            {
                return "no set of its team with another code was judged against it";
            }

            if (missedOf[m] > 0)
            {
                return missedOf[m].ToString(CultureInfo.InvariantCulture) + of + "cannot reach it"
                    + (unjudgedOf[m] > 0 ? ", " + unjudgedOf[m].ToString(CultureInfo.InvariantCulture) + NotAllRead : string.Empty);
            }

            return unjudgedOf[m] > 0
                ? unjudgedOf[m].ToString(CultureInfo.InvariantCulture) + of + NotAllRead.TrimStart()
                : "none" + of + "is kept out of it by a workset or a file name it asks";
        }

        /// <summary>
        /// The lines for the COVERAGE block and the form: one SILENT MISS line for each confirmed
        /// candidate with its drafted correction under it, then a count of the candidates the
        /// coverage did not confirm, of those no count can confirm, of those it could not say, and
        /// of the pairs not judged, then the sets and the models with no code. Where no pair was
        /// judged, the line that says so and why stands where the all clear would.
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
            int noCategory = 0;
            int notCounted = 0;

            foreach (SilentMiss miss in Found)
            {
                if (!miss.Uncaught.HasValue)
                {
                    if (miss.Categories.Count == 0)
                    {
                        noCategory++;
                    }
                    else
                    {
                        notCounted++;
                    }

                    continue;
                }

                if (!miss.Confirmed)
                {
                    notConfirmed++;
                    continue;
                }

                // The set's code with its team beside it, as the model's is, Q116 answered A, or
                // the team its folder gave it, Q117 answered C.
                string setTeam = miss.SetCode.Length > 0
                    ? map.CodeWithTeam(miss.SetCode)
                    : "which carries no code and is in " + miss.Team + " by its folder";

                lines.Add("SILENT MISS  " + miss.SetName + ", " + setTeam + ", finds nothing in " + miss.Model + ", "
                    + map.CodeWithTeam(miss.ModelCode) + ", because it asks " + Asks(miss) + ". That model holds " + (miss.UncaughtWhole ? string.Empty : "at least ")
                    + miss.Uncaught.Value.ToString(CultureInfo.InvariantCulture) + " item(s) of "
                    + string.Join(" or ", new List<string>(miss.Categories).ToArray()) + " that no set catches"
                    + (miss.UncaughtWhole ? string.Empty : ", because a count of some of them was not taken"));

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
                    lines.Add("   the model's file name stands in for the Source File of its items, which is not read here");
                    lines.Add("   no line is drafted for a Source File, which the list of corrections has no line for");
                }
            }

            if (notConfirmed > 0)
            {
                lines.Add(notConfirmed.ToString(CultureInfo.InvariantCulture)
                    + " set and model pair(s) of one team where the set cannot reach the model are not listed, because the model"
                    + " holds no item of the set's categories that no set catches");
            }

            if (noCategory > 0)
            {
                lines.Add(noCategory.ToString(CultureInfo.InvariantCulture)
                    + " set and model pair(s) of one team where the set cannot reach the model are not listed, because the set"
                    + " asks no category by its whole name, so no coverage count can confirm them");
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
                lines.Add(notJudged == null
                    ? "no set asks a workset or a file name that a model of its own team with another code does not carry"
                    : "no set was judged against the models of its team, because " + notJudged);
            }

            if (SetsWithNoCode > 0)
            {
                lines.Add(SetsWithNoCode.ToString(CultureInfo.InvariantCulture)
                    + " set(s) carry no discipline code in their name that the map or a model of the group knows, and no folder above"
                    + " them in the clash XML's set tree names a team, so their team is UNKNOWN and they were judged against no model");
            }

            if (ModelsWithNoCode > 0)
            {
                lines.Add(ModelsWithNoCode.ToString(CultureInfo.InvariantCulture)
                    + " model(s) carry no discipline code in their file name, so their team is UNKNOWN and no set was judged against them");
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
                    if (condition.Property == null
                        || condition.Value == null
                        || string.IsNullOrEmpty(condition.Value.Data)
                        || (condition.Flags & MatrixCorrections.NegateCondition) != 0)
                    {
                        continue;
                    }

                    string property = condition.Property.InternalName;
                    string value = condition.Value.Data;
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

        /// <summary>Whether a workset asked that way finds any name of that list, by ExportCheck.WorksetFinds.</summary>
        private static bool Carries(IList<string> worksets, string value, bool contains)
        {
            foreach (string workset in worksets)
            {
                if (ExportCheck.WorksetFinds(value, contains, workset))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// The coverage count of those categories in that model, added up over the ones counted,
        /// and whether every one was. Above zero where any counted is. Zero where every one was
        /// counted and none is. Null where no count was handed in, the set asks no category by its
        /// whole name, or none counted is above zero and one was not counted. A count is not taken
        /// where it is null or below zero, as ModelExport.NotCounted is, and is never read as zero.
        /// </summary>
        private static int? Count(string file, IList<string> categories, Func<string, string, int?> uncaught, out bool whole)
        {
            whole = false;

            if (uncaught == null || categories.Count == 0)
            {
                return null;
            }

            int sum = 0;
            whole = true;

            foreach (string category in categories)
            {
                int? count = uncaught(file, category);

                if (count.HasValue && count.Value >= 0)
                {
                    sum += count.Value;
                }
                else
                {
                    whole = false;
                }
            }

            if (sum > 0)
            {
                return sum;
            }

            return whole ? 0 : (int?)null;
        }

        /// <summary>
        /// One also-ask line for each workset value asked, the first value of those spelled alike
        /// but for their case, Q102's, with every spelling of the model whose text after its
        /// prefix, WorksetDisagreements.BodyOf, is the value's, case blind. A line of the list
        /// cannot hold the bar that splits its parts, so a name holding it is never drafted.
        /// </summary>
        private static IList<string> Drafts(IList<string> asked, IList<string> carried)
        {
            List<string> drafts = new List<string>();
            List<string> done = new List<string>();

            foreach (string value in asked)
            {
                string body = WorksetDisagreements.BodyOf(value);

                if (body == null
                    || value.IndexOf(ListFile.Bar, StringComparison.Ordinal) >= 0
                    || done.Exists(one => string.Equals(one, body, StringComparison.OrdinalIgnoreCase)))
                {
                    continue;
                }

                done.Add(body);
                List<string> line = new List<string> { value };

                foreach (string workset in carried)
                {
                    string theirs = WorksetDisagreements.BodyOf(workset);

                    if (theirs != null
                        && string.Equals(theirs, body, StringComparison.OrdinalIgnoreCase)
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
