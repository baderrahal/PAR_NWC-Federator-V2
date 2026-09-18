using System;
using System.Collections.Generic;
using System.Globalization;

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

        public ViewpointSettings()
        {
            NameSuffix = DefaultNameSuffix;
            SubGroupDisciplines = new List<string>(DefaultSubGroupDisciplines);
            Sizes = new SizeSettings();
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
