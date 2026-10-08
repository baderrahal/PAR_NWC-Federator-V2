using System;
using System.Collections.Generic;
using System.Globalization;
using Federator.Core.Clash;
using Federator.Core.Teams;

namespace Federator.Core.Views
{
    /// <summary>
    /// Every number and word the saved views are shaped by, the rule for every number that
    /// shapes a run. Since Q114 a view is one per clash test of its open clashes, in a folder
    /// by priority and team pair, F114. The per clash words below them, the seven codes, the
    /// name separator and the cap per test, serve the per clash plan until the add-in pass of
    /// F114 moves the builder onto the per test plan, and the name separator then reads only
    /// the per clash views of earlier runs.
    /// </summary>
    public sealed class ViewpointSettings
    {
        /// <summary>
        /// The disciplines whose large items get a sub group of their own. The ISO 19650
        /// codes for Mechanical and Electrical, which is where pipes, ducts and cable trays
        /// live. It is a SETTING and not a constant, because another project may code its
        /// disciplines differently and nothing in this code decides that for them.
        /// </summary>
        public static readonly string[] DefaultSubGroupDisciplines = { "ME", "EL" };

        /// <summary>
        /// The discipline codes a set name can carry, F85. The seven this project uses:
        /// Architecture, Structure, Mechanical, Fire fighting, Plumbing, Drainage and
        /// Electrical. A SETTING and not a constant, because another project codes its
        /// disciplines differently and nothing in this code decides that for them.
        /// </summary>
        public static readonly string[] DefaultDisciplineCodes =
            { "AR", "ST", "ME", "FF", "PL", "DR", "EL" };

        /// <summary>
        /// What separates the parts of a SET name. A set name is not a file name and this
        /// is not the file name split character, which is its own setting.
        /// </summary>
        public const char DefaultSetNameSeparator = '-';

        /// <summary>What goes between the two codes of a pair folder.</summary>
        public const string DefaultPairSeparator = " vs ";

        /// <summary>
        /// The pair folder for a clash where a side's set name carries no code this tool
        /// knows. The client's own file holds one: BLD-Security Devices breaks the pattern
        /// its siblings follow, so it has no code in the place the others carry one. The
        /// clash still gets a viewpoint and the folder SAYS the code is unknown rather
        /// than guessing at one. The word is the team map's, set once, F131.
        /// </summary>
        public const string DefaultUnknownDiscipline = TeamMapSettings.DefaultUnknownTeam;

        /// <summary>The folder layer 1 uses for a test the priority file says nothing about. The words are Priorities.Words, named once, A13.</summary>
        public static readonly string DefaultNoPriorityFolder = Priorities.Words(ClashPriority.None);

        /// <summary>What goes between the test name and the clash name in a viewpoint name.</summary>
        public const string DefaultNameSeparator = "  ";

        /// <summary>
        /// How far, in document units, the camera read back off a written viewpoint may
        /// sit from the camera the writer asked for before the viewpoint is counted as
        /// failed. The first viewpoints run wrote every viewpoint on the one view the
        /// window happened to show, and nothing read the camera back, so the tree looked
        /// complete and every viewpoint opened on sky. A thousandth of a unit is a
        /// millimetre in a metre file and a third of a millimetre in a foot file, well
        /// under anything a person could see and well over rounding.
        /// </summary>
        public const double DefaultCameraReadBackTolerance = 0.001;

        /// <summary>
        /// How transparent everything that is not the two clashing items is made in a
        /// viewpoint, where 0 is solid and 1 is invisible.
        ///
        /// 0.85 IS CHOSEN AND NOT MEASURED, and it says so because the rule is to say
        /// UNKNOWN rather than fill a gap. Clash Detective's own dim value was looked for
        /// on 2026-09-20 and this API will not say it: Application.Options on the install
        /// exposes one member, Grids, and the COM state exposes no option member at all,
        /// docs\history\scan.md 5o. The number a person can see through is somewhere near
        /// four fifths, and this is a setting so the next person can move it without a
        /// build.
        /// </summary>
        public const double DefaultDimTransparency = 0.85;

        /// <summary>
        /// Whether the two clashing items are painted as well as left solid, Q58
        /// answered b on 2026-09-20: red and green, the way Clash Detective does it.
        /// Off switches the painting and leaves the dimming, which is what the dimming
        /// round shipped.
        /// </summary>
        public const bool DefaultColoursTheTwoItems = true;

        /// <summary>
        /// Which route a viewpoint is written by, Q59 answered d on 2026-09-20. TRUE is
        /// the COM folder collection, one tree operation. FALSE is the root add, the copy
        /// into the folder and the remove, three of them, which is what the dimming round
        /// shipped and what every viewpoint on disk today was written by.
        ///
        /// IT IS A SETTING AND NOT A REPLACEMENT, so the two can be run against each
        /// other on a real group out of one binary, and so a route that turns out to lose
        /// something on a shape not yet seen can be turned off without a build.
        /// </summary>
        public const bool DefaultRecordsThroughTheFolder = true;

        /// <summary>
        /// Which clashes a view shows, Q114 point 13: New and Active, all at once. Reviewed,
        /// Approved and Resolved are left out and counted by status. Read off OpenClashes, the
        /// one place a set of statuses is named.
        /// </summary>
        public const OpenClashCount DefaultViewStatuses = OpenClashCount.NewAndActive;

        /// <summary>
        /// How far past the clash centres a view of several clashes is framed, in millimetres,
        /// so the outermost clash is not on the very edge. CHOSEN AND NOT MEASURED: nothing has
        /// framed a camera on several clashes yet, probe P16 does it first, and this is a
        /// setting so it moves without a build.
        /// </summary>
        public const double DefaultFramingMarginMillimetres = 1000.0;

        /// <summary>
        /// The plain sentence a person reads in the Comments window of every view and folder
        /// this tool makes, Q114 point 16 and Q120 by its default A.
        /// </summary>
        public const string DefaultMarkSentence =
            "Made by the NWC Federator and replaced on its next run. Rename it, move it or add a comment to keep it.";

        /// <summary>
        /// The start of the line no person would type that carries the mark's fingerprint,
        /// after the sentence in the same comment.
        /// </summary>
        public const string DefaultMarkTag = "NWC-FEDERATOR-VIEW v1";

        /// <summary>
        /// What sat between the test name and the clash number in a per clash viewpoint F85
        /// wrote, such as Clash1. Read only to know those viewpoints again, F114.
        /// </summary>
        public const string DefaultLegacyClashPrefix = "Clash";

        /// <summary>
        /// How many lines of the VIEWS TREE block go in the .log, where the .tsv keeps every row.
        /// CHOSEN: a group of 1A02MM's size plans between 59 and 109 views, so the whole tree of
        /// such a group fits, and Q108 keeps the .log from growing without end.
        /// </summary>
        public const int DefaultTreeLinesInLog = 300;

        /// <summary>
        /// Whether a folder the tool removes takes every view under it in the one call. TRUE,
        /// MEASURED by probe P14 on 2026-10-07, docs\history\scan.md 5z-v: RemoveAt(parent, index)
        /// on the AR vs AR folder took it and all 2617 viewpoints in it in 0.174 s, where one view
        /// at a time took 20.012 s, and every other item kept its place through a save and a
        /// reopen. False asks for one view at a time from the end, without a build.
        /// </summary>
        public const bool DefaultFolderGoesWithChildren = true;

        /// <summary>
        /// Who the mark's comment says wrote it, the author Document.CreateCommentWithUniqueId
        /// takes, P9. The tool and never a person, so a person reading the Comments window sees
        /// who put it there. The judge reads the body alone, so the author is a word a project
        /// may change and nothing turns on it.
        /// </summary>
        public const string DefaultMarkAuthor = "Parsons NWC Federator";

        /// <summary>
        /// How often, in seconds, the VIEWS step writes a progress line to the log, FR-071. Set
        /// 03 left the log still for up to 1269 s inside the step, so a busy step and a hung one
        /// looked alike. Once a minute at least.
        /// </summary>
        public const double DefaultProgressEverySeconds = 60.0;

        public ViewpointSettings()
        {
            ViewStatuses = DefaultViewStatuses;
            FramingMarginMillimetres = DefaultFramingMarginMillimetres;
            MarkSentence = DefaultMarkSentence;
            MarkTag = DefaultMarkTag;
            MarkAuthor = DefaultMarkAuthor;
            LegacyClashPrefix = DefaultLegacyClashPrefix;
            TreeLinesInLog = DefaultTreeLinesInLog;
            FolderGoesWithChildren = DefaultFolderGoesWithChildren;
            ProgressEverySeconds = DefaultProgressEverySeconds;
            SubGroupDisciplines = new List<string>(DefaultSubGroupDisciplines);
            Sizes = new SizeSettings();
            DisciplineCodes = new List<string>(DefaultDisciplineCodes);
            SetNameSeparator = DefaultSetNameSeparator;
            PairSeparator = DefaultPairSeparator;
            UnknownDiscipline = DefaultUnknownDiscipline;
            NoPriorityFolder = DefaultNoPriorityFolder;
            NameSeparator = DefaultNameSeparator;
            MaxPerTest = 0;
            CameraReadBackTolerance = DefaultCameraReadBackTolerance;
            DimTransparency = DefaultDimTransparency;
            ColoursTheTwoItems = DefaultColoursTheTwoItems;
            FirstItemColour = ViewpointColour.DefaultFirst();
            SecondItemColour = ViewpointColour.DefaultSecond();
            RecordsThroughTheFolder = DefaultRecordsThroughTheFolder;
        }

        /// <summary>Which clashes a view shows, Q114 point 13.</summary>
        public OpenClashCount ViewStatuses { get; set; }

        /// <summary>How far past the clash centres a view of several clashes is framed, in millimetres.</summary>
        public double FramingMarginMillimetres { get; set; }

        /// <summary>The sentence a person reads on every view and folder this tool makes.</summary>
        public string MarkSentence { get; set; }

        /// <summary>The start of the mark's fingerprint line.</summary>
        public string MarkTag { get; set; }

        /// <summary>Who the mark's comment names as its author, the tool.</summary>
        public string MarkAuthor { get; set; }

        /// <summary>What sat between the test name and the clash number in an F85 viewpoint.</summary>
        public string LegacyClashPrefix { get; set; }

        /// <summary>How many lines of the VIEWS TREE block go in the .log.</summary>
        public int TreeLinesInLog { get; set; }

        /// <summary>Whether a folder the tool removes takes its views with it in one call, P14.</summary>
        public bool FolderGoesWithChildren { get; set; }

        /// <summary>How often, in seconds, the VIEWS step writes a progress line, FR-071.</summary>
        public double ProgressEverySeconds { get; set; }

        /// <summary>Which route a viewpoint is written by, Q59. True is the one tree operation route.</summary>
        public bool RecordsThroughTheFolder { get; set; }

        /// <summary>Whether the two clashing items are painted as well as left solid, Q58.</summary>
        public bool ColoursTheTwoItems { get; set; }

        /// <summary>
        /// What the FIRST item of a clash is painted, red by default. First and second
        /// are the clash's own two sides in the order Clash Detective holds them, so a
        /// person reading a viewpoint and reading the panel sees the same item in the
        /// same colour.
        /// </summary>
        public ViewpointColour FirstItemColour { get; set; }

        /// <summary>What the SECOND item of a clash is painted, green by default.</summary>
        public ViewpointColour SecondItemColour { get; set; }

        /// <summary>
        /// Whether the painting is on AND both colours are there to paint with. A colour
        /// set to nothing is off rather than black, because black is a colour a person
        /// might mean and null is not.
        /// </summary>
        public bool ColoursAnything
        {
            get { return ColoursTheTwoItems && FirstItemColour != null && SecondItemColour != null; }
        }

        /// <summary>How far a written viewpoint's camera may sit from the one asked for, in document units.</summary>
        public double CameraReadBackTolerance { get; set; }

        /// <summary>
        /// How transparent everything but the two clashing items is made, 0 solid and 1
        /// invisible. Zero switches the dimming off and the viewpoints go back to what
        /// F85 shipped, which a person could not read. A value outside 0 to 1 is refused
        /// by DimsAnything so a typo cannot ask the API for something it will not take.
        /// </summary>
        public double DimTransparency { get; set; }

        /// <summary>
        /// Whether the dimming is on at all. A value at or below zero is off, and one at
        /// or above one would make everything invisible and is refused rather than obeyed.
        /// </summary>
        public bool DimsAnything
        {
            get { return DimTransparency > 0.0 && DimTransparency < 1.0; }
        }

        /// <summary>The discipline codes a set name can carry, F85.</summary>
        public IList<string> DisciplineCodes { get; set; }

        /// <summary>What separates the parts of a set name.</summary>
        public char SetNameSeparator { get; set; }

        /// <summary>What goes between the two codes of a pair folder.</summary>
        public string PairSeparator { get; set; }

        /// <summary>What a code this tool does not know reads as in a folder name.</summary>
        public string UnknownDiscipline { get; set; }

        /// <summary>The layer 1 folder for a test the priority file says nothing about.</summary>
        public string NoPriorityFolder { get; set; }

        /// <summary>What goes between the test name and the clash name.</summary>
        public string NameSeparator { get; set; }

        /// <summary>
        /// How many viewpoints one test may write, or zero for no cap. Off by default and
        /// a SETTING, the same shape and the same reason the images cap has: one artefact
        /// per clash across 1830 tests is how a run stops fitting in forty five minutes.
        /// </summary>
        public int MaxPerTest { get; set; }

        /// <summary>Which disciplines carry a sub group for their large items.</summary>
        public IList<string> SubGroupDisciplines { get; set; }

        /// <summary>The threshold and the property names the sub group is built on.</summary>
        public SizeSettings Sizes { get; set; }

        /// <summary>
        /// Whether this discipline gets a sub group. Matched Ordinal and never trimmed or
        /// cased, because a discipline code is read off a file name and every other
        /// comparison in this tool treats it exactly as it was read.
        /// </summary>
        public bool HasSubGroup(string discipline)
        {
            if (string.IsNullOrEmpty(discipline) || SubGroupDisciplines == null)
            {
                return false;
            }

            for (int i = 0; i < SubGroupDisciplines.Count; i++)
            {
                if (string.Equals(SubGroupDisciplines[i], discipline, StringComparison.Ordinal))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// What the large items sub folder is called. Built from the threshold, so the
        /// folder name and the rule that fills it can never disagree. A folder reading
        /// Over 150mm beside a rule using 250 is the kind of drift nobody notices.
        /// </summary>
        public string SubGroupFolderName()
        {
            double threshold = Sizes == null
                ? SizeSettings.DefaultThresholdMillimetres
                : Sizes.ThresholdMillimetres;

            return "Over " + threshold.ToString("0.###", CultureInfo.InvariantCulture) + "mm";
        }
    }
}
