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
