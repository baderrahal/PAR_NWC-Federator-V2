using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Federator.Core.Findings
{
    /// <summary>
    /// What the scan noticed. None of these block a run. They are information, and the
    /// decision stays with the person reading them.
    /// </summary>
    public enum FindingKind
    {
        /// <summary>A building code whose letter and digit pattern no other code shares.</summary>
        OddShape,

        /// <summary>Two building codes one character apart, which reads like a typing error.</summary>
        NearMatch,

        /// <summary>A group holding one discipline, so it has nothing to clash against.</summary>
        SingleDiscipline,

        /// <summary>A group missing disciplines that other groups in this run have.</summary>
        MissingDisciplines,

        /// <summary>
        /// The building code inside the Revit source name is not the code on the NWC that
        /// was published from it. Only known once a document is open, so this one comes
        /// out of the run rather than out of the scan.
        /// </summary>
        SourceMismatch,

        /// <summary>
        /// One Revit building code feeding more than one group, which reads as one
        /// building split in two by a naming error.
        /// </summary>
        SharedSourceBuilding
    }

    /// <summary>One thing worth looking at, with enough detail to act on without the log.</summary>
    public sealed class ScanFinding
    {
        internal ScanFinding(
            FindingKind kind,
            string label,
            string headline,
            string detail,
            IList<string> buildings,
            IList<string> files)
        {
            Kind = kind;
            Label = label;
            Headline = headline;
            Detail = detail;
            Buildings = new ReadOnlyCollection<string>(buildings ?? new List<string>());
            Files = new ReadOnlyCollection<string>(files ?? new List<string>());
        }

        public FindingKind Kind { get; private set; }

        /// <summary>The short shouty name, for example ODD SHAPE.</summary>
        public string Label { get; private set; }

        /// <summary>
        /// What was noticed, in the words a person would say it in. Never a shape string
        /// or a code word. A reader should not have to know how the tool works.
        /// </summary>
        public string Headline { get; private set; }

        /// <summary>
        /// What it probably means and what to do about it, again in plain words. The
        /// reader should not have to work it out again.
        /// </summary>
        public string Detail { get; private set; }

        /// <summary>
        /// The whole thing as one sentence, which is what goes beside the code when the
        /// findings are copied out as a table.
        /// </summary>
        public string Sentence
        {
            get
            {
                return string.IsNullOrEmpty(Detail) ? Headline : Headline + " " + Detail;
            }
        }

        /// <summary>The building codes this concerns, one for most, two for a near match.</summary>
        public ReadOnlyCollection<string> Buildings { get; private set; }

        /// <summary>File names, carried for an odd shape so the offending files are named.</summary>
        public ReadOnlyCollection<string> Files { get; private set; }

        public override string ToString()
        {
            return Label + "  " + Headline;
        }
    }
}
