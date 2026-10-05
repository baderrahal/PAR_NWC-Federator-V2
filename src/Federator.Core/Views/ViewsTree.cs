using System;
using System.Collections.Generic;
using Federator.Core.Clash;

namespace Federator.Core.Views
{
    /// <summary>
    /// The VIEWS TREE block, F114, Q114 point 19: the tree the VIEWS step left, priority, team
    /// pair, size folder and each view with its open clashes, the models it shows and hides and
    /// whether it read back, then what the step found before and did after, then the seven
    /// checks, each FAILED line naming what broke it, each check that could not run saying DID
    /// NOT RUN and why, and each that ran without something it names saying RAN IN PART and
    /// naming what it did not read, then how many hold with those that did not run, whole or in
    /// part, counted apart, F114 attempt 3.
    ///
    /// The tree lines are cut at the TreeLinesInLog setting for the .log, which says it cut them,
    /// and the .tsv takes them whole, so a group of thousands of views cannot drown the log, Q108.
    /// The checks and the counts are never cut. The shown and hidden models of a view are read off
    /// the document where the add-in read them back under PlannedTestView.Key, and off the plan
    /// where it did not, which check 3 counts as a view it did not run for.
    /// </summary>
    public static class ViewsTree
    {
        /// <summary>The block's title.</summary>
        public const string Title = "VIEWS TREE";

        /// <summary>The block's lines, the tree cut at that many view lines where it is above zero.</summary>
        public static IList<string> Lines(ViewsTreeFacts facts, IList<ViewsTreeCheck> checks, int treeLines)
        {
            if (facts == null)
            {
                throw new ArgumentNullException("facts");
            }

            if (facts.Plan == null)
            {
                throw new ArgumentException("The VIEWS TREE block needs the plan.", "facts");
            }

            List<string> lines = new List<string>();
            List<ModelTeam> models = new List<ModelTeam>(facts.Models ?? new ModelTeam[0]);

            lines.Add(Title + " " + facts.Group);
            lines.Add("team map   : " + MapWords(facts));
            lines.Add("models     : " + ModelWords(models));
            lines.Add("before     : " + BeforeWords(facts));

            List<string> tree = TreeLines(facts, models);

            if (treeLines > 0 && tree.Count > treeLines)
            {
                int cut = tree.Count - treeLines;
                tree = tree.GetRange(0, treeLines);
                tree.Add("... " + cut + " more tree lines not written here, the .tsv holds every one");
            }

            lines.AddRange(tree);
            lines.Add("left out   : " + LeftOutWords(facts));
            lines.Add("after      : " + AfterWords(facts));

            int holding = 0;
            int notRun = 0;
            int inPart = 0;
            int failed = 0;

            foreach (ViewsTreeCheck check in checks ?? new ViewsTreeCheck[0])
            {
                if (!check.Ran)
                {
                    notRun++;
                    lines.Add("CHECK " + check.Number + "  " + check.Words + "  DID NOT RUN, " + check.NotRunWhy);
                }
                else
                {
                    string state;

                    if (check.Holds)
                    {
                        holding++;
                        state = "CHECK ";
                    }
                    else if (check.Failures.Count > 0)
                    {
                        failed++;
                        state = "FAILED CHECK ";
                    }
                    else
                    {
                        inPart++;
                        state = "CHECK ";
                    }

                    lines.Add(state + check.Number + "  " + check.Words + "  "
                        + (check.Failures.Count == 0 && check.NotRead.Count > 0 ? "RAN IN PART, " : string.Empty)
                        + check.Failures.Count + " broke it, " + check.Basis);

                    foreach (string failure in check.Failures)
                    {
                        lines.Add("    " + failure);
                    }

                    foreach (string what in check.NotRead)
                    {
                        lines.Add("    not read: " + what);
                    }
                }

                foreach (string note in check.Notes)
                {
                    lines.Add("    " + note);
                }
            }

            int testsWithViews = 0;
            HashSet<string> tests = new HashSet<string>(StringComparer.Ordinal);

            foreach (PlannedTestView view in facts.Plan.Views)
            {
                if (tests.Add(view.Name))
                {
                    testsWithViews++;
                }
            }

            lines.Add("open clashes in views " + facts.Plan.InViews + ", tests with open clashes " + testsWithViews
                + ", views planned " + facts.Plan.Views.Count + ", views written " + (facts.Written == null ? 0 : facts.Written.Count));

            int all = checks == null ? 0 : checks.Count;
            string last = holding + " of " + all + " hold";
            List<string> apart = new List<string>();

            if (notRun > 0)
            {
                apart.Add(notRun + " did not run");
            }

            if (inPart > 0)
            {
                apart.Add(inPart + " ran in part");
            }

            if (apart.Count > 0)
            {
                last += ", " + string.Join(" and ", apart.ToArray())
                    + (notRun + inPart == 1 ? " and is not counted as holding" : ", none of them counted as holding");
            }

            if (failed > 0)
            {
                last += ", each failure named above, and the group keeps its own result";
            }

            lines.Add(last);

            return lines;
        }

        private static string MapWords(ViewsTreeFacts facts)
        {
            if (facts.Map == null)
            {
                return "UNKNOWN, none was handed to this block";
            }

            return facts.Map.IsRead
                ? facts.Map.ListPath + ", " + facts.Map.Teams.Count + " teams, " + facts.Map.Codes.Count + " codes"
                : "none read, so every code is a team of its own and no pair carries the size folder, the TEAMS lines say why";
        }

        private static string ModelWords(IList<ModelTeam> models)
        {
            List<string> words = new List<string>();

            foreach (ModelTeam model in models)
            {
                string word = model.Code.Length == 0
                    ? model.FileName + " whose code will not read"
                    : model.Code + " " + model.Team;

                if (!words.Contains(word))
                {
                    words.Add(word);
                }
            }

            return words.Count == 0 ? "none" : string.Join(", ", words.ToArray());
        }

        private static string BeforeWords(ViewsTreeFacts facts)
        {
            if (facts.Inventory == null)
            {
                return "UNKNOWN, no inventory was handed to this block";
            }

            ViewpointSettings settings = facts.Settings ?? new ViewpointSettings();
            int views = 0, thisRun = 0, earlier = 0, legacy = 0, changed = 0, notOurs = 0;

            foreach (InventoryItem item in facts.Inventory.Items)
            {
                ViewNode node = item.Node;

                if (node.IsFolder)
                {
                    continue;
                }

                views++;

                if (item.Decision == InventoryDecision.KeepWrittenThisRun || item.Decision == InventoryDecision.RemoveAtOnce)
                {
                    thisRun++;
                    continue;
                }

                ViewOwner owner = ToolViewMark.Judge(
                    node.Folders, node.Name, node.Camera, node.Comments, node.Redlines, node.Guid, settings).Owner;

                if (owner == ViewOwner.Ours)
                {
                    earlier++;
                }
                else if (owner == ViewOwner.ChangedByAPerson)
                {
                    changed++;
                }
                else if (LegacyClashView.TestOf(node.Folders, node.Name, false, node.Comments.Count, node.Redlines,
                    facts.KnownCodes ?? new string[0], facts.TestNames ?? new string[0], settings) != null)
                {
                    legacy++;
                }
                else
                {
                    notOurs++;
                }
            }

            return views + " views once this run had written its own: " + thisRun + " written by this run, "
                + earlier + " this tool's of earlier runs, " + legacy + " per clash viewpoints of earlier runs, "
                + changed + " this tool's changed by a person, " + notOurs + " not this tool's";
        }

        private static List<string> TreeLines(ViewsTreeFacts facts, List<ModelTeam> models)
        {
            List<string> lines = new List<string>();
            string priority = null;
            string pair = null;
            string size = null;

            foreach (PlannedTestView view in facts.Plan.Views)
            {
                if (!string.Equals(view.PriorityFolder, priority, StringComparison.Ordinal))
                {
                    lines.Add(view.PriorityFolder);
                    priority = view.PriorityFolder;
                    pair = null;
                }

                if (!string.Equals(view.Pair.Folder, pair, StringComparison.Ordinal))
                {
                    lines.Add("  " + view.Pair.Folder);
                    pair = view.Pair.Folder;
                    size = null;
                }

                if (view.SizeFolder != null && !string.Equals(view.SizeFolder, size, StringComparison.Ordinal))
                {
                    lines.Add("    " + view.SizeFolder);
                    size = view.SizeFolder;
                }

                lines.Add((view.SizeFolder == null ? "    " : "      ") + view.Name + "  " + view.Clashes.Count
                    + " open clashes, " + ShowsAndHides(facts, models, view) + ", " + StateOf(facts, view));
            }

            return lines;
        }

        private static string ShowsAndHides(ViewsTreeFacts facts, List<ModelTeam> models, PlannedTestView view)
        {
            IList<string> hiddenNames;
            List<ModelTeam> hidden;

            if (facts.HiddenReadBack != null && facts.HiddenReadBack.TryGetValue(view.Key, out hiddenNames))
            {
                hidden = models.FindAll(model => hiddenNames.Contains(model.FileName));
            }
            else
            {
                List<string> homes = new List<string>();

                foreach (ViewClash clash in view.Clashes)
                {
                    homes.Add(clash.FirstHome);
                    homes.Add(clash.SecondHome);
                }

                hidden = new List<ModelTeam>(ShownModels.For(view.Pair, models, homes).Hidden);
            }

            List<ModelTeam> shown = models.FindAll(model => !hidden.Contains(model));
            string hides = Codes(hidden);

            return "shows " + Codes(shown) + ", " + (hides.Length == 0 ? "hides nothing" : "hides " + hides);
        }

        private static string Codes(IEnumerable<ModelTeam> models)
        {
            List<string> codes = new List<string>();

            foreach (ModelTeam model in models)
            {
                string code = model.Code.Length == 0 ? model.FileName : model.Code;

                if (!codes.Contains(code))
                {
                    codes.Add(code);
                }
            }

            return string.Join(" ", codes.ToArray());
        }

        private static string StateOf(ViewsTreeFacts facts, PlannedTestView view)
        {
            WrittenView written = null;

            string place = ViewPlace.Key(view.Folders, view.Name, false);

            foreach (WrittenView one in facts.Written ?? new WrittenView[0])
            {
                if (string.Equals(ViewPlace.Key(one.Folders, one.Name, false), place, StringComparison.Ordinal))
                {
                    written = one;
                }
            }

            if (written == null)
            {
                return "NOT WRITTEN";
            }

            if (!written.Marked)
            {
                return "NOT MARKED";
            }

            return written.ReadBack ? "read back" : "NOT READ BACK";
        }

        private static string LeftOutWords(ViewsTreeFacts facts)
        {
            TestViewPlanOutcome plan = facts.Plan;
            List<string> words = new List<string>();

            foreach (ClashStatus status in Enum.GetValues(typeof(ClashStatus)))
            {
                if (!plan.InScope.Contains(status))
                {
                    words.Add(plan.LeftOutAt(status) + " " + status);
                }
            }

            words.Add(plan.MirrorRuleHandedIn
                ? plan.LeftOutAsMirrors + " clashes of " + plan.Mirrored.Count + " mirrored tests"
                : "mirrored tests UNKNOWN, no mirror rule was handed to the plan");
            words.Add(TestsRunWords(facts));
            return string.Join(", ", words.ToArray());
        }

        /// <summary>
        /// The tests that ran with no open clash, off the tests the clash step ran, since a test
        /// with no clash at all is never handed to the plan. UNKNOWN where those were not handed in.
        /// </summary>
        private static string TestsRunWords(ViewsTreeFacts facts)
        {
            if (facts.TestsRun == null)
            {
                return "how many tests ran with no open clash is UNKNOWN, the tests the clash step ran were not handed in";
            }

            HashSet<string> viewed = new HashSet<string>(StringComparer.Ordinal);

            foreach (PlannedTestView view in facts.Plan.Views)
            {
                viewed.Add(view.Name);
            }

            HashSet<string> ran = new HashSet<string>(StringComparer.Ordinal);
            int withAView = 0, mirrored = 0;

            foreach (string test in facts.TestsRun)
            {
                if (test == null || !ran.Add(test))
                {
                    continue;
                }

                if (viewed.Contains(test))
                {
                    withAView++;
                }
                else if (facts.Plan.Mirrored.Contains(test))
                {
                    mirrored++;
                }
            }

            return ran.Count + " tests ran, " + withAView + " with a view and " + (ran.Count - withAView - mirrored)
                + " with no open clash" + (mirrored > 0 ? ", and " + mirrored + " mirrored with no view" : string.Empty);
        }

        private static string AfterWords(ViewsTreeFacts facts)
        {
            int written = 0, readBack = 0;

            foreach (WrittenView view in facts.Written ?? new WrittenView[0])
            {
                written++;

                if (view.Sound)
                {
                    readBack++;
                }
            }

            if (facts.Inventory == null)
            {
                return "written " + written + ", read back " + readBack + ", what was removed is UNKNOWN, no inventory was handed to this block";
            }

            int atOnce = 0, earlier = 0, legacy = 0, folders = 0, keptAndWhy = 0;

            foreach (InventoryItem item in facts.Inventory.Items)
            {
                switch (item.Decision)
                {
                    case InventoryDecision.RemoveAtOnce: atOnce++; break;
                    case InventoryDecision.RemoveReplaced:
                    case InventoryDecision.RemoveNoLongerNeeded: earlier++; break;
                    case InventoryDecision.RemoveLegacy: legacy++; break;
                    case InventoryDecision.RemoveFolder: folders++; break;
                    case InventoryDecision.KeepChangedByAPerson:
                    case InventoryDecision.KeepTestNotRead:
                    case InventoryDecision.KeepReplacementFailed:
                    case InventoryDecision.KeepPlaceNotUnique:
                    case InventoryDecision.KeepClashStepNotSound: keptAndWhy++; break;
                }
            }

            return "written " + written + ", read back " + readBack + ", and the inventory removed " + atOnce + " at once, "
                + earlier + " this tool's earlier views, " + legacy + " per clash viewpoints and " + folders
                + " folders, and kept " + keptAndWhy + " named with why";
        }
    }
}
