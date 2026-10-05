using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Federator.Core.Teams;

namespace Federator.Core.Views
{
    /// <summary>
    /// One of the seven checks of the VIEWS TREE block, F114, Q114 point 19: the five Bader named
    /// and two the design adds, read off a fresh walk of the tree after the VIEWS step where the
    /// document can say, and off the plan where it cannot, the Basis saying which.
    ///
    ///   1  no team pair folder holds a test of another pair
    ///   2  no Over 150mm folder sits outside its own pair
    ///   3  no view shows a model of a third team, the exceptions Q118 A allows named
    ///   4  no clash is in two views, the plan's keys, and each view's painted items read back
    ///   5  no mirrored test is run or has a view, against F132's mirror rule where one ran
    ///   6  every view and folder the inventory kept is there after, same place and name
    ///   7  no per clash viewpoint of an earlier run is left without a reason
    ///
    /// A FAILED CHECK IS A FAILED LINE and the group keeps its own result, because CLAUDE.md says a
    /// report check never fails a group, the same as Bader's answer on Q112's notes for COVERAGE.
    ///
    /// A CHECK THAT COULD NOT RUN IS NEVER COUNTED AS HOLDING, F114 attempt 2, because CLAUDE.md says
    /// never to report a check that did not run. Check 5 with no mirror rule handed in, checks 1
    /// and 2 with no walk, no run stamp or not one planned view found marked by this run, check 3
    /// with no models, checks 6 and 7 with no walk or no inventory, and 7 with no codes or test
    /// names, did not run. Each says why, and the block counts them apart from those that hold.
    /// </summary>
    public sealed class ViewsTreeCheck
    {
        private readonly List<string> failures = new List<string>();
        private readonly List<string> notes = new List<string>();

        internal ViewsTreeCheck(int number, string words)
        {
            Number = number;
            Words = words;
            Basis = string.Empty;
        }

        public int Number { get; private set; }

        /// <summary>What the check holds to, in plain words.</summary>
        public string Words { get; private set; }

        /// <summary>What it was read off, the document or the plan, and how many it looked at.</summary>
        public string Basis { get; internal set; }

        /// <summary>Each thing that broke it, named.</summary>
        public ReadOnlyCollection<string> Failures
        {
            get { return new ReadOnlyCollection<string>(failures); }
        }

        /// <summary>What it allowed and named, or what it could not read.</summary>
        public ReadOnlyCollection<string> Notes
        {
            get { return new ReadOnlyCollection<string>(notes); }
        }

        /// <summary>Why the check could not run, or null where it ran.</summary>
        public string NotRunWhy { get; private set; }

        /// <summary>Whether the check ran over what it names.</summary>
        public bool Ran
        {
            get { return NotRunWhy == null; }
        }

        /// <summary>Whether it ran and nothing broke it. A check that did not run never holds.</summary>
        public bool Holds
        {
            get { return Ran && failures.Count == 0; }
        }

        /// <summary>The seven checks over those facts, in their order.</summary>
        public static IList<ViewsTreeCheck> Of(ViewsTreeFacts facts)
        {
            if (facts == null)
            {
                throw new ArgumentNullException("facts");
            }

            Read read = new Read(facts);

            return new List<ViewsTreeCheck>
            {
                NoTestInAnotherPair(read),
                NoSizeFolderOutsideItsPair(read),
                NoThirdTeamShown(read),
                NoClashInTwoViews(read),
                NoMirrorRunOrViewed(read),
                EveryKeptItemStays(read),
                NoPerClashViewpointLeft(read)
            };
        }

        internal void Fail(string why)
        {
            failures.Add(why);
        }

        internal void Note(string what)
        {
            notes.Add(what);
        }

        /// <summary>Marks the check as not run, with why, where why is not null. True where it did not run.</summary>
        internal bool CouldNotRun(string why)
        {
            if (why == null)
            {
                return false;
            }

            NotRunWhy = why;
            return true;
        }

        private static ViewsTreeCheck NoTestInAnotherPair(Read read)
        {
            ViewsTreeCheck check = new ViewsTreeCheck(1, "no team pair folder holds a test of another pair");

            if (check.CouldNotRun(read.WhyThisRunsViewsCannotBeRead()))
            {
                return check;
            }

            int looked = 0;

            foreach (ViewNode node in read.Mine)
            {
                if (node.IsFolder)
                {
                    continue;
                }

                looked++;
                TeamPair pair = read.Plan.PairOfTest(node.Name);

                if (pair == null)
                {
                    check.Fail(node + " is a view of a test this run's plan never made");
                }
                else if (node.Folders.Count < 2 || !string.Equals(node.Folders[1], pair.Folder, StringComparison.Ordinal))
                {
                    check.Fail(node.Name + " sits in " + node.FolderPath + " and its pair is " + pair.Folder);
                }
            }

            check.Basis = looked + " views of this run of " + read.Plan.Views.Count + " planned, off the document";
            return check;
        }

        private static ViewsTreeCheck NoSizeFolderOutsideItsPair(Read read)
        {
            ViewsTreeCheck check = new ViewsTreeCheck(2, "no Over 150mm folder sits outside its own pair");

            if (check.CouldNotRun(read.WhyThisRunsViewsCannotBeRead()))
            {
                return check;
            }

            string size = read.Settings.SubGroupFolderName();
            int looked = 0;

            foreach (ViewNode node in read.Mine)
            {
                for (int i = 0; i < node.Folders.Count; i++)
                {
                    if (i != 2 && string.Equals(node.Folders[i], size, StringComparison.Ordinal))
                    {
                        check.Fail(node + " sits under a " + size + " folder that is not directly in a pair folder");
                    }
                }

                if (node.IsFolder && string.Equals(node.Name, size, StringComparison.Ordinal))
                {
                    looked++;

                    if (node.Folders.Count != 2)
                    {
                        check.Fail(node + " sits outside a pair folder");
                    }
                    else if (!read.PairCarriesSizeFolder(node.Folders[1]))
                    {
                        check.Fail(node + " sits in a pair that carries no size folder");
                    }
                }

                if (!node.IsFolder && node.Folders.Count == 3 && string.Equals(node.Folders[2], size, StringComparison.Ordinal))
                {
                    TeamPair pair = read.Plan.PairOfTest(node.Name);

                    if (pair == null || !pair.CarriesSizeFolder || !string.Equals(node.Folders[1], pair.Folder, StringComparison.Ordinal))
                    {
                        check.Fail(node + " sits under the " + size + " folder of a pair that is not its own");
                    }
                }
            }

            check.Basis = looked + " size folders of this run, off the document";
            return check;
        }

        private static ViewsTreeCheck NoThirdTeamShown(Read read)
        {
            ViewsTreeCheck check = new ViewsTreeCheck(3, "no view shows a model of a third team");

            if (check.CouldNotRun(read.Facts.Models == null
                ? "no models of the group were handed in, so what a view shows is UNKNOWN"
                : null))
            {
                return check;
            }

            int offTheDocument = 0;

            foreach (PlannedTestView view in read.Plan.Views)
            {
                ShownModels planned = read.ShownFor(view);

                foreach (ModelTeam exception in planned.Exceptions)
                {
                    check.Note(view + " shows " + exception.FileName + " of " + exception.Team
                        + ", where one of its clashing items lives, allowed by Q118 A");
                }

                if (planned.HomesNotRead > 0)
                {
                    check.Note(view + " has " + planned.HomesNotRead
                        + " clashing items whose model could not be read, so whether that model is shown is UNKNOWN");
                }

                foreach (string home in planned.HomesNotInGroup)
                {
                    check.Note(view + " has a clashing item in " + home
                        + ", which is no model of this group, so whether it is shown is UNKNOWN");
                }

                IList<string> hidden = read.HiddenReadBack(view);
                IList<ModelTeam> shown = new List<ModelTeam>(planned.Shown);

                if (hidden != null)
                {
                    offTheDocument++;
                    shown = read.Models.FindAll(model => !hidden.Contains(model.FileName));
                }

                foreach (ModelTeam model in shown)
                {
                    bool ofThePair = string.Equals(model.Team, view.Pair.First, StringComparison.Ordinal)
                        || string.Equals(model.Team, view.Pair.Second, StringComparison.Ordinal);

                    if (model.Code.Length > 0 && !ofThePair && !planned.Exceptions.Contains(model))
                    {
                        check.Fail(view + " shows " + model.FileName + " of " + model.Team + ", a team of neither side");
                    }
                }
            }

            int views = read.Plan.Views.Count;
            check.Basis = offTheDocument == views
                ? views + " views, their hidden models read back off the document"
                : offTheDocument + " of " + views + " views read back off the document, the rest off the plan's list";
            return check;
        }

        private static ViewsTreeCheck NoClashInTwoViews(Read read)
        {
            ViewsTreeCheck check = new ViewsTreeCheck(4, "no clash is in two views");
            Dictionary<string, int> seen = new Dictionary<string, int>(StringComparer.Ordinal);
            int clashes = 0;
            int painted = 0;

            foreach (PlannedTestView view in read.Plan.Views)
            {
                foreach (ViewClash clash in view.Clashes)
                {
                    clashes++;
                    int count;
                    seen.TryGetValue(clash.Key, out count);
                    seen[clash.Key] = count + 1;

                    if (count == 1)
                    {
                        check.Fail(clash.TestName + " / " + clash.ClashName + " is in more than one view");
                    }
                }

                IList<ItemPath> readBack = read.PaintedReadBack(view);

                if (readBack == null)
                {
                    continue;
                }

                painted++;
                HashSet<ItemPath> wanted = new HashSet<ItemPath>(PaintPlan.For(view.Clashes).Solid);
                HashSet<ItemPath> found = new HashSet<ItemPath>(readBack);
                int missing = 0;

                foreach (ItemPath item in wanted)
                {
                    if (!found.Contains(item))
                    {
                        missing++;
                    }
                }

                found.ExceptWith(wanted);

                if (missing > 0 || found.Count > 0)
                {
                    check.Fail(view + " reads back " + missing + " of its clashing items unpainted and "
                        + found.Count + " painted items that are not its clashes'");
                }
            }

            check.Basis = "the plan's keys of " + clashes + " open clashes, and the painted items of "
                + painted + " of " + read.Plan.Views.Count + " views read back off the document";
            return check;
        }

        /// <summary>
        /// Check 5 reads the tests the plan made views for, since a mirror not run this week can
        /// still hold the results of an earlier run in the document, and the tests the clash step ran.
        /// </summary>
        private static ViewsTreeCheck NoMirrorRunOrViewed(Read read)
        {
            ViewsTreeCheck check = new ViewsTreeCheck(5, "no mirrored test is run or has a view");
            ICollection<string> mirrors = read.Facts.Mirrors;

            if (check.CouldNotRun(mirrors == null
                ? "no mirror rule was handed in, F132 is not in this build, so whether a mirrored test ran or has a view is UNKNOWN"
                : null))
            {
                return check;
            }

            if (mirrors.Count == 0)
            {
                check.Note("the mirror rule found 0 mirrors in this matrix, so this check proves nothing here");
            }

            List<string> viewed = new List<string>();

            foreach (PlannedTestView view in read.Plan.Views)
            {
                if (!viewed.Contains(view.Name))
                {
                    viewed.Add(view.Name);
                }
            }

            foreach (string test in viewed)
            {
                if (mirrors.Contains(test))
                {
                    check.Fail(test + " is a mirror and has a view");
                }
            }

            string ran;

            if (read.Facts.TestsRun == null)
            {
                ran = "not the tests the clash step ran, which were not handed in";
            }
            else
            {
                ran = "the " + read.Facts.TestsRun.Count + " tests the clash step ran";

                foreach (string test in read.Facts.TestsRun)
                {
                    if (mirrors.Contains(test))
                    {
                        check.Fail(test + " is a mirror and was run");
                    }
                }
            }

            check.Basis = mirrors.Count + " mirrors against the " + viewed.Count + " tests the plan made views for and " + ran;
            return check;
        }

        /// <summary>
        /// Check 6 follows every item the inventory did not remove, a person's view under two
        /// folders of one name and a person's folder included, not two kinds of kept item alone.
        /// </summary>
        private static ViewsTreeCheck EveryKeptItemStays(Read read)
        {
            ViewsTreeCheck check = new ViewsTreeCheck(6, "every view and folder the inventory kept is there after, same place and name");

            if (check.CouldNotRun(read.WhyTheInventoryCannotBeFollowed()))
            {
                return check;
            }

            Dictionary<string, int> after = read.PlacesAfter();
            int kept = 0;

            foreach (InventoryItem item in read.Inventory)
            {
                if (item.Removes)
                {
                    continue;
                }

                kept++;

                if (!Read.Take(after, Read.Place(item.Node)))
                {
                    check.Fail(item.Node + " was there before and is not there after");
                }
            }

            check.Basis = (kept - check.failures.Count) + " of " + kept + " kept there after, off the document";
            return check;
        }

        private static ViewsTreeCheck NoPerClashViewpointLeft(Read read)
        {
            ViewsTreeCheck check = new ViewsTreeCheck(7, "no per clash viewpoint of an earlier run is left without a reason");

            if (check.CouldNotRun(read.WhyTheInventoryCannotBeFollowed())
                || check.CouldNotRun(read.Facts.KnownCodes == null || read.Facts.TestNames == null
                    ? "no known codes or test names were handed in, so a per clash viewpoint of an earlier run cannot be told"
                    : null))
            {
                return check;
            }

            Dictionary<string, int> keptWithAReason = new Dictionary<string, int>(StringComparer.Ordinal);
            Dictionary<string, string> why = new Dictionary<string, string>(StringComparer.Ordinal);

            foreach (InventoryItem item in read.Inventory)
            {
                if (!item.Removes && !item.Node.IsFolder && read.IsLegacy(item.Node))
                {
                    string place = Read.Place(item.Node);
                    int count;
                    keptWithAReason.TryGetValue(place, out count);
                    keptWithAReason[place] = count + 1;
                    why[place] = item.Why;
                }
            }

            int left = 0;

            foreach (ViewNode node in read.After)
            {
                if (node.IsFolder || !read.IsLegacy(node))
                {
                    continue;
                }

                left++;
                string place = Read.Place(node);

                if (Read.Take(keptWithAReason, place))
                {
                    check.Note(node + " kept: " + why[place]);
                }
                else
                {
                    check.Fail(node + " is a per clash viewpoint of an earlier run left with no reason");
                }
            }

            check.Basis = left + " left, " + check.notes.Count + " kept and named, off the document";
            return check;
        }

        /// <summary>The facts as every check reads them, a missing part read as empty where a check runs without it.</summary>
        private sealed class Read
        {
            internal Read(ViewsTreeFacts facts)
            {
                if (facts.Plan == null)
                {
                    throw new ArgumentException("The VIEWS TREE checks need the plan.", "facts");
                }

                Facts = facts;
                Settings = facts.Settings ?? new ViewpointSettings();
                Plan = facts.Plan;
                After = new List<ViewNode>(facts.After ?? new ViewNode[0]);
                Models = new List<ModelTeam>(facts.Models ?? new ModelTeam[0]);
                Inventory = facts.Inventory == null ? new List<InventoryItem>() : new List<InventoryItem>(facts.Inventory.Items);
                Mine = string.IsNullOrEmpty(facts.RunStamp) ? new List<ViewNode>() : After.FindAll(node =>
                {
                    MarkJudgement judged = ToolViewMark.Judge(
                        node.Folders, node.Name, node.Camera, node.Comments, node.Redlines, node.Guid, Settings);

                    return judged.Owner == ViewOwner.Ours && string.Equals(judged.Mark.Stamp, facts.RunStamp, StringComparison.Ordinal);
                });
            }

            internal ViewsTreeFacts Facts { get; private set; }

            internal ViewpointSettings Settings { get; private set; }

            internal TestViewPlanOutcome Plan { get; private set; }

            internal List<ViewNode> After { get; private set; }

            internal List<ModelTeam> Models { get; private set; }

            internal List<InventoryItem> Inventory { get; private set; }

            /// <summary>The views and folders of the walk this run marked.</summary>
            internal List<ViewNode> Mine { get; private set; }

            /// <summary>The place an item is counted by, ViewPlace's one key.</summary>
            internal static string Place(ViewNode node)
            {
                return ViewPlace.Key(node.Folders, node.Name, node.IsFolder);
            }

            /// <summary>Takes one of that place off the count, false where none is left.</summary>
            internal static bool Take(Dictionary<string, int> counts, string place)
            {
                int count;

                if (!counts.TryGetValue(place, out count) || count == 0)
                {
                    return false;
                }

                counts[place] = count - 1;
                return true;
            }

            /// <summary>Why the views this run wrote cannot be read off the walk, or null where they can.</summary>
            internal string WhyThisRunsViewsCannotBeRead()
            {
                if (Facts.After == null)
                {
                    return "no walk of the tree after the removals was handed in";
                }

                if (string.IsNullOrEmpty(Facts.RunStamp))
                {
                    return "no run stamp was handed in, so this run's views cannot be told from the rest";
                }

                if (Plan.Views.Count > 0 && !Mine.Exists(node => !node.IsFolder))
                {
                    return "none of the " + Plan.Views.Count + " planned views was found marked by this run in the walk after,"
                        + " the tree lines say which were not written or not marked";
                }

                return null;
            }

            /// <summary>Why what the inventory kept cannot be followed into the walk after, or null where it can.</summary>
            internal string WhyTheInventoryCannotBeFollowed()
            {
                if (Facts.After == null)
                {
                    return "no walk of the tree after the removals was handed in";
                }

                return Facts.Inventory == null ? "no inventory was handed in, so what was kept is UNKNOWN" : null;
            }

            internal Dictionary<string, int> PlacesAfter()
            {
                Dictionary<string, int> counts = new Dictionary<string, int>(StringComparer.Ordinal);

                foreach (ViewNode node in After)
                {
                    int count;
                    counts.TryGetValue(Place(node), out count);
                    counts[Place(node)] = count + 1;
                }

                return counts;
            }

            internal bool PairCarriesSizeFolder(string pairFolder)
            {
                foreach (PlannedTestView view in Plan.Views)
                {
                    if (string.Equals(view.Pair.Folder, pairFolder, StringComparison.Ordinal) && view.Pair.CarriesSizeFolder)
                    {
                        return true;
                    }
                }

                return false;
            }

            internal ShownModels ShownFor(PlannedTestView view)
            {
                List<string> homes = new List<string>();

                foreach (ViewClash clash in view.Clashes)
                {
                    homes.Add(clash.FirstHome);
                    homes.Add(clash.SecondHome);
                }

                return ShownModels.For(view.Pair, Models, homes);
            }

            internal IList<string> HiddenReadBack(PlannedTestView view)
            {
                IList<string> hidden;
                return Facts.HiddenReadBack != null && Facts.HiddenReadBack.TryGetValue(view.ToString(), out hidden) ? hidden : null;
            }

            internal IList<ItemPath> PaintedReadBack(PlannedTestView view)
            {
                IList<ItemPath> painted;
                return Facts.PaintedReadBack != null && Facts.PaintedReadBack.TryGetValue(view.ToString(), out painted) ? painted : null;
            }

            internal bool IsLegacy(ViewNode node)
            {
                return LegacyClashView.TestOf(
                    node.Folders, node.Name, node.IsFolder, node.Comments.Count, node.Redlines,
                    Facts.KnownCodes ?? new string[0], Facts.TestNames ?? new string[0], Settings) != null;
            }
        }
    }
}
