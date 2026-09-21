using System;
using System.Collections.Generic;
using System.Globalization;
using Federator.Core.Clash;

namespace Federator.Core.Views
{
    /// <summary>
    /// What a discipline's folder and viewpoint are called.
    ///
    /// Both are settings and neither is a constant, which is the rule for every name that
    /// shapes a run. The defaults are the discipline code for the folder, because that is
    /// what the scan already reads off part 5 of the NWC name and what every other output
    /// of this tool is named after, and the code with one word after it for the viewpoint,
    /// because a viewpoint called ME sitting in a folder called ME says nothing about what
    /// pressing it does.
    /// </summary>
    public sealed class ViewpointSettings
    {
        /// <summary>What a viewpoint's name carries after the discipline code.</summary>
        public const string DefaultNameSuffix = " only";

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
        /// than guessing at one.
        /// </summary>
        public const string DefaultUnknownDiscipline = "UNKNOWN";

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

        public ViewpointSettings()
        {
            NameSuffix = DefaultNameSuffix;
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

        /// <summary>
        /// HOW MANY VIEWPOINTS ONE GROUP MAY WRITE, or zero for no cap. OFF BY DEFAULT, so
        /// a run that sets nothing behaves exactly as it did.
        ///
        /// WHY IT EXISTS. Writing a saved viewpoint costs time in proportion to how many
        /// the document ALREADY holds, MEASURED on 2026-09-21 and written up at
        /// docs\history\scan.md 6a: on one copy of 1A02MM, 40 viewpoints cost 1,878 ms each
        /// against 568 already there and 6.5 ms each after SavedViewpoints.Clear(), the
        /// same document with one thing changed. So a group writing one per clash pays a
        /// cost that grows as it goes, and 1A04EP with 2,566 clashes spent 2,188.961s in
        /// VIEWS, of which 1,953.136s was the recording alone.
        ///
        /// IT IS NOT ONLY ABOUT TIME. That same group ended with models 5 to 0, sets 61 to
        /// 0, tests 666 to 0 and results 2,566 to 0, the last 1,408 viewpoints written
        /// without a hidden state because there was no longer a model to hide, and the NWF
        /// save threw Can't save an empty document. A ceiling is what stops a group
        /// reaching that far while the real fix is found, and the real fix is Q80.
        ///
        /// A cap is never silent. The plan counts what it left out and the block says how
        /// many were not written, the same as the per test cap beside it.
        /// </summary>
        public int MaxPerGroup { get; set; }

        /// <summary>
        /// Whether that word is a discipline code this tool knows. Matched Ordinal and
        /// never trimmed or cased, the same way HasSubGroup matches, because a code is
        /// read off a name and every other comparison here treats it as it was read.
        /// </summary>
        public bool IsADisciplineCode(string code)
        {
            if (string.IsNullOrEmpty(code) || DisciplineCodes == null)
            {
                return false;
            }

            for (int i = 0; i < DisciplineCodes.Count; i++)
            {
                if (string.Equals(DisciplineCodes[i], code, StringComparison.Ordinal))
                {
                    return true;
                }
            }

            return false;
        }

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

        /// <summary>
        /// Put after the discipline code to make the viewpoint's name. Empty is a real
        /// answer and gives a viewpoint named exactly the discipline code.
        /// </summary>
        public string NameSuffix { get; set; }

        /// <summary>
        /// The folder one discipline's viewpoints go in. The discipline code itself, so a
        /// person opening the tree in Navisworks reads the same codes the file names carry.
        /// </summary>
        public string FolderNameFor(string discipline)
        {
            if (string.IsNullOrEmpty(discipline))
            {
                throw new ArgumentException("A viewpoint folder needs a discipline code.", "discipline");
            }

            return discipline;
        }

        /// <summary>
        /// What the viewpoint inside that folder is called. Never trimmed and never cased,
        /// because a discipline code is read off a file name and is matched elsewhere
        /// exactly as it was read.
        /// </summary>
        public string ViewpointNameFor(string discipline)
        {
            if (string.IsNullOrEmpty(discipline))
            {
                throw new ArgumentException("A viewpoint needs a discipline code.", "discipline");
            }

            return discipline + (NameSuffix ?? string.Empty);
        }
    }
}
