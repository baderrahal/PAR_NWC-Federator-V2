using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Federator.Core.Clash;

namespace Federator.Core.Views
{
    /// <summary>One clash, as the plan is handed it. No Navisworks type reaches this.</summary>
    public sealed class ClashToPlan
    {
        public ClashToPlan(string testName, string clashName, string leftSet, string rightSet,
            ClashStatus status, ClashPriority priority, SizeVerdict? serviceSize)
        {
            TestName = testName ?? string.Empty;
            ClashName = clashName ?? string.Empty;
            LeftSet = leftSet ?? string.Empty;
            RightSet = rightSet ?? string.Empty;
            Status = status;
            Priority = priority;
            ServiceSize = serviceSize;
        }

        public string TestName { get; private set; }

        public string ClashName { get; private set; }

        /// <summary>The set name of the left side, not the locator.</summary>
        public string LeftSet { get; private set; }

        public string RightSet { get; private set; }

        public ClashStatus Status { get; private set; }

        /// <summary>The test's priority, or None where no file was picked.</summary>
        public ClashPriority Priority { get; private set; }

        /// <summary>
        /// What the size rule said about the SERVICE side of this clash, or null where
        /// neither side is a service at all, which is the ordinary case for an
        /// architecture against structure clash.
        ///
        /// A CLASH SIDE IS READ THE WAY F72a READS ONE, which is the LARGEST size property
        /// the item carries and not the first one on the list. Reading it the other way
        /// would let the same 600 by 150 duct be set Reviewed by F72a as a large service
        /// and filed by this plan as a small one, and the two rules would disagree about
        /// one clash with nothing saying so. Q51.
        /// </summary>
        public SizeVerdict? ServiceSize { get; private set; }
    }

    /// <summary>Why one clash is not in the tree.</summary>
    public enum ViewpointLeftOutReason
    {
        /// <summary>Approved or Resolved. Closed under both readings, so nobody is looking.</summary>
        NotOpen = 0,

        /// <summary>
        /// A service at or under the threshold. F72a's rule calls it a penetration and
        /// this rule leaves it out of the tree, and the two are stated apart on purpose:
        /// F72a is OFF by default, so a run with that box off still must not fill the tree
        /// with every small pipe through every wall.
        /// </summary>
        SmallService = 1
    }

    /// <summary>One viewpoint this run means to put in the NWF, F85.</summary>
    public sealed class PlannedClashViewpoint
    {
        internal PlannedClashViewpoint(
            string priorityFolder, string pairFolder, string sizeFolder, string name,
            ClashToPlan clash, DisciplinePair pair)
        {
            PriorityFolder = priorityFolder;
            PairFolder = pairFolder;
            SizeFolder = sizeFolder;
            Name = name;
            Clash = clash;
            Pair = pair;
        }

        /// <summary>Layer 1, or null where no priority file was picked.</summary>
        public string PriorityFolder { get; private set; }

        /// <summary>Layer 2. Always there.</summary>
        public string PairFolder { get; private set; }

        /// <summary>Layer 3, or null.</summary>
        public string SizeFolder { get; private set; }

        /// <summary>
        /// What it is called. The TEST and the clash, because a clash name is unique only
        /// within its test and this tree puts clashes from many tests in one folder. A
        /// leaf named after the clash alone would collide, and then the already there
        /// check and the read back both stop meaning anything.
        /// </summary>
        public string Name { get; private set; }

        public ClashToPlan Clash { get; private set; }

        public DisciplinePair Pair { get; private set; }

        /// <summary>
        /// Where it sits. The address a rerun matches on, built once here rather than
        /// joined by each caller in its own spelling.
        /// </summary>
        public string Path
        {
            get
            {
                List<string> parts = new List<string>();

                if (!string.IsNullOrEmpty(PriorityFolder))
                {
                    parts.Add(PriorityFolder);
                }

                parts.Add(PairFolder);

                if (!string.IsNullOrEmpty(SizeFolder))
                {
                    parts.Add(SizeFolder);
                }

                parts.Add(Name);
                return string.Join("/", parts.ToArray());
            }
        }

        /// <summary>The folders it needs, outermost first.</summary>
        public IList<string> Folders
        {
            get
            {
                List<string> folders = new List<string>();

                if (!string.IsNullOrEmpty(PriorityFolder))
                {
                    folders.Add(PriorityFolder);
                }

                folders.Add(PairFolder);

                if (!string.IsNullOrEmpty(SizeFolder))
                {
                    folders.Add(SizeFolder);
                }

                return folders;
            }
        }

        public override string ToString()
        {
            return Path;
        }
    }

    /// <summary>What the plan wants and what it left out, F85.</summary>
    public sealed class ClashViewpointPlanOutcome
    {
        private readonly List<PlannedClashViewpoint> planned = new List<PlannedClashViewpoint>();
        private readonly Dictionary<ViewpointLeftOutReason, int> leftOut =
            new Dictionary<ViewpointLeftOutReason, int>();

        internal ClashViewpointPlanOutcome(bool priorityPicked)
        {
            PriorityPicked = priorityPicked;

            foreach (ViewpointLeftOutReason reason in AllReasons)
            {
                leftOut[reason] = 0;
            }
        }

        /// <summary>The two reasons, in the order the block lists them.</summary>
        public static ViewpointLeftOutReason[] AllReasons
        {
            get
            {
                return new[]
                {
                    ViewpointLeftOutReason.NotOpen,
                    ViewpointLeftOutReason.SmallService
                };
            }
        }

        /// <summary>The words for one reason, so the block and nothing else spells them.</summary>
        public static string Describe(ViewpointLeftOutReason reason)
        {
            switch (reason)
            {
                case ViewpointLeftOutReason.NotOpen:
                    return "Approved or Resolved, so nobody is looking at it";
                case ViewpointLeftOutReason.SmallService:
                    return "a service at or under the threshold, which is a penetration "
                        + "and not a coordination item";
                default:
                    return "UNKNOWN";
            }
        }

        /// <summary>Whether a priority file was picked, which decides whether layer 1 exists.</summary>
        public bool PriorityPicked { get; private set; }

        /// <summary>How many clashes were looked at at all.</summary>
        public int Considered { get; internal set; }

        /// <summary>How many went in a pair folder because their size could not be read.</summary>
        public int SizeUnknownCount { get; internal set; }

        /// <summary>How many carried a set name with no code this tool knows.</summary>
        public int UnknownDisciplineCount { get; internal set; }

        /// <summary>How many a per test cap kept out.</summary>
        public int OverTheCapCount { get; internal set; }

        public ReadOnlyCollection<PlannedClashViewpoint> Planned
        {
            get { return new ReadOnlyCollection<PlannedClashViewpoint>(planned); }
        }

        public int Of(ViewpointLeftOutReason reason)
        {
            return leftOut.ContainsKey(reason) ? leftOut[reason] : 0;
        }

        internal void Add(PlannedClashViewpoint viewpoint)
        {
            planned.Add(viewpoint);
        }

        internal void LeaveOut(ViewpointLeftOutReason reason)
        {
            if (!leftOut.ContainsKey(reason))
            {
                leftOut[reason] = 0;
            }

            leftOut[reason] = leftOut[reason] + 1;
        }

        /// <summary>
        /// The VIEWS block. The counts and one line per reason, including the reasons at
        /// zero, which is how every other block here reads. The planned viewpoints
        /// themselves are NOT listed: a real group holds thousands and a log that wrote
        /// one line each would be the log drowning the run again.
        /// </summary>
        public IList<string> Lines()
        {
            List<string> lines = new List<string>();

            lines.Add("clashes looked at : " + Considered);
            lines.Add("viewpoints planned: " + planned.Count);

            foreach (ViewpointLeftOutReason reason in AllReasons)
            {
                lines.Add("    " + Of(reason).ToString().PadLeft(5) + "  " + Describe(reason));
            }

            if (OverTheCapCount > 0)
            {
                lines.Add("    " + OverTheCapCount.ToString().PadLeft(5)
                    + "  over the cap on how many one test may write");
            }

            lines.Add("size could not be read : " + SizeUnknownCount
                + ", every one of them is in its pair folder and none was dropped");
            lines.Add("a set name with no code this tool knows : " + UnknownDisciplineCount
                + ", every one of them is in a folder saying UNKNOWN and none was guessed at");

            if (!PriorityPicked)
            {
                lines.Add("NO PRIORITY FILE was picked, so the tree has no priority layer and "
                    + "the pair folders sit at the top. Pick one on the Clash step to get it");
            }

            return lines;
        }
    }

    /// <summary>
    /// The saved viewpoints, in three layers, F85. ONE VIEWPOINT PER CLASH and not per
    /// test, because the thing a person presses has to be the thing they are looking at.
    ///
    ///     Layer 1   the priority off the client's matrix, A, B, C or No priority. Dropped
    ///               entirely when no priority file was picked, and the block says so
    ///     Layer 2   the two disciplines, sorted, so AR vs ST and ST vs AR are one folder
    ///     Layer 3   Over 150mm, and ONLY under a pair involving one of the disciplines the
    ///               settings name, which defaults to Mechanical and Electrical
    ///
    /// WHAT IS LEFT OUT, AND THE TWO REASONS ARE COUNTED APART. A clash at Approved or
    /// Resolved is closed under both readings and nobody is looking at it. A SERVICE at or
    /// under the threshold is a penetration, and it stays out of the tree.
    ///
    /// THE SMALL SERVICE RULE IS THIS PLAN'S OWN AND IS NOT INHERITED FROM F72a. The brief
    /// says small services stay out because F72a already set them Reviewed, and that is
    /// true of a service against a SOLID and only that: F72a leaves a service against
    /// another service exactly as it was, Q44, and F72a is OFF by default anyway. So a run
    /// with that box off would fill this tree with every small pipe through every wall if
    /// this rule read the status instead of the size. It reads the size.
    ///
    /// AND REVIEWED IS IN SCOPE, which is the other half of the same trap. Reviewed is one
    /// of the three statuses this tree carries, so a service F72a DID move is still here
    /// and is kept out by the size branch rather than by the status filter.
    ///
    /// A SIZE THAT COULD NOT BE READ GOES IN THE PAIR FOLDER AND IS COUNTED. It is branched
    /// on SizeVerdict and never on SizeDecision.Included, which folds Large and SizeUnknown
    /// together for F53's own reasons and would put every fitting with no size property
    /// into Over 150mm.
    ///
    /// WRITTEN BY ViewpointBuilder SINCE THE VIEWPOINTS ROUND, 2026-09-19. Whether a
    /// viewpoint saved while items are hidden records that hiding is MEASURED, scan.md 5j,
    /// and it does when it is captured with the runtime overrides. A planned viewpoint that
    /// was not written is still not a viewpoint, and the VIEWS BUILT block says which were.
    /// </summary>
    public static class ClashViewpointPlan
    {
        /// <summary>
        /// The statuses a clash has to be at to get a viewpoint: New, Active and Reviewed.
        /// Read off OpenClashes rather than typed, because that is Navisworks's own
        /// definition of open and the image filter already reads the same place.
        /// </summary>
        public static IList<ClashStatus> StatusesInScope()
        {
            return OpenClashes.StatusesFor(OpenClashCount.NavisworksOpen);
        }

        /// <summary>Whether a clash at that status gets a viewpoint.</summary>
        public static bool InScope(ClashStatus status)
        {
            foreach (ClashStatus one in StatusesInScope())
            {
                if (one == status)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// The plan for one group. The clashes come in the order they were harvested and
        /// the plan keeps that order, so two runs of one model give the same tree.
        /// </summary>
        public static ClashViewpointPlanOutcome For(
            IEnumerable<ClashToPlan> clashes, ViewpointSettings settings, bool priorityPicked)
        {
            if (settings == null)
            {
                throw new ArgumentNullException("settings");
            }

            ClashViewpointPlanOutcome outcome = new ClashViewpointPlanOutcome(priorityPicked);
            Dictionary<string, int> perTest = new Dictionary<string, int>(StringComparer.Ordinal);

            if (clashes == null)
            {
                return outcome;
            }

            foreach (ClashToPlan clash in clashes)
            {
                if (clash == null)
                {
                    continue;
                }

                outcome.Considered = outcome.Considered + 1;

                if (!InScope(clash.Status))
                {
                    outcome.LeaveOut(ViewpointLeftOutReason.NotOpen);
                    continue;
                }

                if (clash.ServiceSize.HasValue && clash.ServiceSize.Value == SizeVerdict.Small)
                {
                    outcome.LeaveOut(ViewpointLeftOutReason.SmallService);
                    continue;
                }

                if (settings.MaxPerTest > 0)
                {
                    int already = perTest.ContainsKey(clash.TestName) ? perTest[clash.TestName] : 0;

                    if (already >= settings.MaxPerTest)
                    {
                        outcome.OverTheCapCount = outcome.OverTheCapCount + 1;
                        continue;
                    }

                    perTest[clash.TestName] = already + 1;
                }

                DisciplinePair pair = DisciplinePairRule.For(clash.LeftSet, clash.RightSet, settings);

                if (!pair.BothKnown)
                {
                    outcome.UnknownDisciplineCount = outcome.UnknownDisciplineCount + 1;
                }

                string sizeFolder = null;

                if (clash.ServiceSize.HasValue)
                {
                    if (clash.ServiceSize.Value == SizeVerdict.Large
                        && DisciplinePairRule.CarriesASizeFolder(pair, settings))
                    {
                        sizeFolder = settings.SubGroupFolderName();
                    }
                    else if (clash.ServiceSize.Value == SizeVerdict.SizeUnknown)
                    {
                        outcome.SizeUnknownCount = outcome.SizeUnknownCount + 1;
                    }
                }

                outcome.Add(new PlannedClashViewpoint(
                    priorityPicked ? FolderFor(clash.Priority, settings) : null,
                    pair.Folder,
                    sizeFolder,
                    NameFor(clash, settings),
                    clash,
                    pair));
            }

            return outcome;
        }

        /// <summary>
        /// The layer 1 folder for one priority. A test the file says nothing about gets a
        /// folder of its own and NOT a blank one, because an unnamed folder is not a
        /// folder, and it sorts last for the same reason A sorts first.
        /// </summary>
        public static string FolderFor(ClashPriority priority, ViewpointSettings settings)
        {
            if (priority == ClashPriority.None)
            {
                return settings == null
                    ? ViewpointSettings.DefaultNoPriorityFolder
                    : settings.NoPriorityFolder;
            }

            return Priorities.Words(priority);
        }

        /// <summary>
        /// What one viewpoint is called. The TEST and the clash, because a clash name is
        /// unique only within its test and this tree puts clashes from many tests into one
        /// folder.
        /// </summary>
        public static string NameFor(ClashToPlan clash, ViewpointSettings settings)
        {
            if (clash == null)
            {
                return string.Empty;
            }

            string separator = settings == null
                ? ViewpointSettings.DefaultNameSeparator
                : settings.NameSeparator;

            return clash.TestName + separator + clash.ClashName;
        }
    }
}
