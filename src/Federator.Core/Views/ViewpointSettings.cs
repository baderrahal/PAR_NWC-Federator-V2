using System;

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

        public ViewpointSettings()
        {
            NameSuffix = DefaultNameSuffix;
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
