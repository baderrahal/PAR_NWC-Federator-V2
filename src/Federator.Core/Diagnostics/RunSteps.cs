using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Federator.Core.Diagnostics
{
    /// <summary>
    /// The steps a group goes through, in the order it meets them, and the only place any
    /// of their names is written down.
    ///
    /// WHY A LIST AND NOT A STRING AT EACH CALL. The log has to answer where the time
    /// went, and it can only do that if the same work carries the same name every time it
    /// is timed. A name typed at the call is a name that can be typed differently at the
    /// next call, and then one step reads as two in the timing block and neither total is
    /// the truth. Nothing outside this file types a step name.
    ///
    /// A step is not the same as a method. APPEND covers building from scratch and
    /// rebuilding from the scan, because both put the models in and a reader asking where
    /// the time went wants one number for that. TESTS RUN is entered once per test and
    /// its seconds add up across every one of them.
    /// </summary>
    public static class RunSteps
    {
        public const string Decide = "DECIDE";
        public const string Append = "APPEND";
        public const string NwfSave = "NWF SAVE";
        public const string Units = "UNITS";
        public const string Sets = "SETS";
        public const string TestsCreate = "TESTS CREATE";
        public const string TestsRun = "TESTS RUN";
        public const string Harvest = "HARVEST";
        public const string Images = "IMAGES";
        public const string Workbook = "WORKBOOK";
        public const string Html = "HTML";
        public const string Xml = "XML";
        public const string Nwd = "NWD";
        public const string Confirm = "CONFIRM";

        private static readonly string[] InOrder =
        {
            Decide,
            Append,
            NwfSave,
            Units,
            Sets,
            TestsCreate,
            TestsRun,
            Harvest,
            Images,
            Workbook,
            Html,
            Xml,
            Nwd,
            Confirm
        };

        /// <summary>
        /// Every step, in the order a group meets them. Read only, because a caller that
        /// could add to it could add a step this file does not name.
        /// </summary>
        public static IList<string> All
        {
            get { return new ReadOnlyCollection<string>(InOrder); }
        }

        /// <summary>
        /// Whether this is one of the steps. Ordinal and never trimmed, the way every
        /// other name in this tool is matched.
        /// </summary>
        public static bool IsAStep(string name)
        {
            return OrderOf(name) >= 0;
        }

        /// <summary>Where the step sits in the order, or minus one when it is not one.</summary>
        public static int OrderOf(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return -1;
            }

            for (int i = 0; i < InOrder.Length; i++)
            {
                if (string.Equals(InOrder[i], name, StringComparison.Ordinal))
                {
                    return i;
                }
            }

            return -1;
        }

        /// <summary>
        /// How wide the name column is, read off the longest name rather than typed, so
        /// adding a longer step cannot leave the block ragged.
        /// </summary>
        public static int NameWidth
        {
            get
            {
                int widest = 0;

                for (int i = 0; i < InOrder.Length; i++)
                {
                    if (InOrder[i].Length > widest)
                    {
                        widest = InOrder[i].Length;
                    }
                }

                return widest;
            }
        }

        /// <summary>The name padded to the column width, so every step line starts alike.</summary>
        public static string Padded(string name)
        {
            return (name ?? string.Empty).PadRight(NameWidth);
        }

        /// <summary>
        /// The words for a name that is not on the list. A step is refused rather than
        /// logged under a name nothing else uses, because a timing block holding a step
        /// nobody named is worse than one that is short.
        /// </summary>
        public static string NotAStep(string name)
        {
            return "\"" + (name ?? "null") + "\" is not one of the steps this tool times. "
                + "The steps are " + string.Join(", ", InOrder) + ".";
        }
    }
}
