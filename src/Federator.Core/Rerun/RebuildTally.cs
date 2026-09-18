using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Federator.Core.Rerun
{
    /// <summary>
    /// One thing the NWF carries across a rebuild, with the three counts read off the
    /// document and never assumed: what was there before the clear, what the document held
    /// after the appends, and what it held once the copy was put back.
    ///
    /// F50. The NWF is the record. It carries the file list, the sets, the tests, the clash
    /// results with the statuses a person set by hand, and since F52 the viewpoints. A
    /// CHANGED group clears the document and appends again, so every one of those has to be
    /// counted out and counted back, and the group fails with the NWF left alone if any of
    /// them does not return.
    /// </summary>
    public sealed class RebuiltThing
    {
        internal RebuiltThing(string label, string name)
        {
            Label = label;
            Name = name;
            AfterAppends = -1;
            AfterRestore = -1;
        }

        /// <summary>The block label the log line starts with, such as SETS.</summary>
        public string Label { get; private set; }

        /// <summary>What it is in words, such as selection sets, for a failure a person reads.</summary>
        public string Name { get; private set; }

        /// <summary>Counted before the clear.</summary>
        public int Before { get; internal set; }

        /// <summary>Counted after the clear and the appends. Minus one until it is read.</summary>
        public int AfterAppends { get; internal set; }

        /// <summary>Counted after the copy was put back. Minus one until it is read.</summary>
        public int AfterRestore { get; internal set; }

        /// <summary>
        /// Whether the copy has to be put back, which is any drop in the count. Nothing
        /// there before the clear needs nothing putting back.
        /// </summary>
        public bool NeedsRestoring
        {
            get { return Before > 0 && AfterAppends >= 0 && AfterAppends < Before; }
        }

        /// <summary>
        /// Whether everything counted before the clear is in the document at the end. This
        /// is the ONE keep rule, written once and read by all four things. It used to be
        /// written twice, once for the sets and once for the tests, in the same words.
        /// </summary>
        public bool Kept
        {
            get { return Before <= 0 || AfterRestore >= Before; }
        }

        /// <summary>
        /// Whether the counts were ever read. A thing that was never counted is not kept
        /// and is not lost, it is unread, and saying so is the difference between a rebuild
        /// that checked and one that did not.
        /// </summary>
        public bool WasCounted
        {
            get { return AfterAppends >= 0 && AfterRestore >= 0; }
        }

        /// <summary>
        /// The one line the log carries for this thing. Every one of the four reads the
        /// same way, because four things reading four ways is how a log stops being read.
        /// </summary>
        public string Line()
        {
            string head = (Label + "         ").Substring(0, 9);

            if (!WasCounted)
            {
                return head + Name + " NOT COUNTED, so whether the rebuild kept them is UNKNOWN";
            }

            if (Before <= 0)
            {
                return head + Name + ": none, the NWF held none before the clear";
            }

            if (AfterAppends >= Before)
            {
                return head + Name + " kept: before clear " + Before
                    + ", after appends " + AfterAppends + ", nothing to put back";
            }

            if (Kept)
            {
                return head + Name + " kept: before clear " + Before
                    + ", after appends " + AfterAppends
                    + ", after restore " + AfterRestore + ", put back from the copy";
            }

            return head + Name + " LOST: before clear " + Before
                + ", after appends " + AfterAppends
                + ", after restore " + AfterRestore
                + ". The NWF on disk was NOT saved over, so it keeps them";
        }

        /// <summary>
        /// Why the group failed, in the words the outcome carries, or null where nothing
        /// was lost. Null and a sentence are the two answers, never an empty string.
        /// </summary>
        public string LostReason()
        {
            if (Kept && WasCounted)
            {
                return null;
            }

            if (!WasCounted)
            {
                return "the rebuild did not count the " + Name
                    + ", so whether they came back is UNKNOWN and the NWF on disk was not saved over";
            }

            return "the rebuild could not keep the " + Before + " " + Name + ", "
                + AfterRestore + " are in the document, so the NWF on disk was not saved over";
        }
    }

    /// <summary>
    /// Everything the NWF carries across a rebuild, counted out and counted back.
    ///
    /// WHY THIS IS ONE TYPE AND NOT FOUR PAIRS OF METHODS. Before F50 the keep rule was
    /// written twice, once for the sets and once for the saved tests, in the same words
    /// with two names. Widening the rebuild to cover the viewpoints and the clash result
    /// statuses by the same means would have written it four times. One rule lives in one
    /// place, so the rule is here once and the four things are four rows.
    /// </summary>
    public sealed class RebuildTally
    {
        private readonly List<RebuiltThing> things = new List<RebuiltThing>();

        /// <summary>Adds a thing to count, and hands it back so its counts can be filled in.</summary>
        public RebuiltThing Count(string label, string name)
        {
            if (string.IsNullOrEmpty(label))
            {
                throw new ArgumentException("A counted thing needs a log label.", "label");
            }

            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException("A counted thing needs a name in words.", "name");
            }

            RebuiltThing added = new RebuiltThing(label, name);
            things.Add(added);
            return added;
        }

        /// <summary>The things counted, in the order they were added.</summary>
        public ReadOnlyCollection<RebuiltThing> Things
        {
            get { return new ReadOnlyCollection<RebuiltThing>(things); }
        }

        /// <summary>
        /// True only when every thing counted came back. The NWF on disk is saved over on
        /// this and nothing else, so a thing that was never counted holds it shut too.
        /// </summary>
        public bool EverythingKept
        {
            get
            {
                for (int i = 0; i < things.Count; i++)
                {
                    if (!things[i].Kept || !things[i].WasCounted)
                    {
                        return false;
                    }
                }

                return true;
            }
        }

        /// <summary>One line per thing, for the log, in the order they were counted.</summary>
        public IList<string> Lines()
        {
            List<string> lines = new List<string>();

            for (int i = 0; i < things.Count; i++)
            {
                lines.Add(things[i].Line());
            }

            return lines;
        }

        /// <summary>
        /// One reason per thing that did not come back, for the outcome's error list. Empty
        /// where everything came back, which is what lets the caller ask one question.
        /// </summary>
        public IList<string> LostReasons()
        {
            List<string> reasons = new List<string>();

            for (int i = 0; i < things.Count; i++)
            {
                string reason = things[i].LostReason();

                if (reason != null)
                {
                    reasons.Add(reason);
                }
            }

            return reasons;
        }
    }
}
