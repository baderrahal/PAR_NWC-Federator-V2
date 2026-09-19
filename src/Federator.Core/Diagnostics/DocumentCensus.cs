using System;
using System.Collections.Generic;
using System.Text;

namespace Federator.Core.Diagnostics
{
    /// <summary>
    /// What the open document held at one moment: five counts and nothing else.
    ///
    /// MINUS ONE IS NOT ZERO AND NEVER READS AS ONE. A count that could not be taken is
    /// minus one, the same answer SavedViewpoints.Count gives, because zero reads as a
    /// real count and would let a run report that it kept everything while it had thrown
    /// the lot away. A census holding a minus one says UNKNOWN for that count and refuses
    /// to call any move in it a change, in either direction.
    ///
    /// No Navisworks type reaches this file. The add-in does the counting and hands the
    /// five numbers over, which is why every rule about what they mean can be tested.
    /// </summary>
    public sealed class DocumentCensus
    {
        /// <summary>What a count that could not be taken reads as.</summary>
        public const int NotCounted = -1;

        private readonly int[] counts;

        public DocumentCensus(int models, int sets, int tests, int results, int viewpoints)
        {
            counts = new int[5];
            counts[(int)CensusCount.Models] = Clean(models);
            counts[(int)CensusCount.Sets] = Clean(sets);
            counts[(int)CensusCount.Tests] = Clean(tests);
            counts[(int)CensusCount.Results] = Clean(results);
            counts[(int)CensusCount.Viewpoints] = Clean(viewpoints);
        }

        /// <summary>A census where nothing could be counted. What a throw comes back as.</summary>
        public static DocumentCensus Unknown()
        {
            return new DocumentCensus(NotCounted, NotCounted, NotCounted, NotCounted, NotCounted);
        }

        public int Of(CensusCount what)
        {
            int at = (int)what;
            return at < 0 || at >= counts.Length ? NotCounted : counts[at];
        }

        /// <summary>Whether that count was taken at all.</summary>
        public bool Knows(CensusCount what)
        {
            return Of(what) != NotCounted;
        }

        /// <summary>Whether every one of the five was taken.</summary>
        public bool KnowsEverything
        {
            get
            {
                foreach (CensusCount what in All)
                {
                    if (!Knows(what))
                    {
                        return false;
                    }
                }

                return true;
            }
        }

        /// <summary>The five, in the order they are always written.</summary>
        public static IList<CensusCount> All
        {
            get
            {
                return new[]
                {
                    CensusCount.Models,
                    CensusCount.Sets,
                    CensusCount.Tests,
                    CensusCount.Results,
                    CensusCount.Viewpoints
                };
            }
        }

        /// <summary>
        /// The words for one count, so the log and the rules cannot drift apart over
        /// whether it is sets or selection sets.
        /// </summary>
        public static string Words(CensusCount what)
        {
            switch (what)
            {
                case CensusCount.Models: return "models";
                case CensusCount.Sets: return "selection sets";
                case CensusCount.Tests: return "clash tests";
                case CensusCount.Results: return "clash results";
                case CensusCount.Viewpoints: return "saved viewpoints";
                default: return "UNKNOWN";
            }
        }

        /// <summary>The short name used in the count columns.</summary>
        public static string Short(CensusCount what)
        {
            switch (what)
            {
                case CensusCount.Models: return "models";
                case CensusCount.Sets: return "sets";
                case CensusCount.Tests: return "tests";
                case CensusCount.Results: return "results";
                case CensusCount.Viewpoints: return "views";
                default: return "UNKNOWN";
            }
        }

        /// <summary>
        /// The one line a census writes, saying when it was taken and what it found.
        /// UNKNOWN where a count could not be taken, never a zero standing in for it.
        /// </summary>
        public string Line(string when)
        {
            StringBuilder text = new StringBuilder();
            text.Append("CENSUS   ").Append(when ?? "UNKNOWN when");

            foreach (CensusCount what in All)
            {
                text.Append("  ").Append(Short(what)).Append(' ').Append(Show(what));
            }

            return text.ToString();
        }

        /// <summary>That count, or the word UNKNOWN.</summary>
        public string Show(CensusCount what)
        {
            int count = Of(what);
            return count == NotCounted ? "UNKNOWN" : count.ToString();
        }

        /// <summary>
        /// Every count that is a different number from the one in the census handed in.
        ///
        /// A count that either census could not take is NOT a move. Comparing a real
        /// number against UNKNOWN answers nothing, and reporting it as a change would
        /// bury the real ones under noise from a count nobody could read.
        /// </summary>
        public IList<CensusCount> MovedSince(DocumentCensus before)
        {
            List<CensusCount> moved = new List<CensusCount>();

            if (before == null)
            {
                return moved;
            }

            foreach (CensusCount what in All)
            {
                if (!Knows(what) || !before.Knows(what))
                {
                    continue;
                }

                if (Of(what) != before.Of(what))
                {
                    moved.Add(what);
                }
            }

            return moved;
        }

        /// <summary>
        /// The counts neither census could read, so a step can say plainly that it does
        /// not know whether they moved rather than saying nothing at all.
        /// </summary>
        public IList<CensusCount> CouldNotCompare(DocumentCensus before)
        {
            List<CensusCount> unknown = new List<CensusCount>();

            if (before == null)
            {
                return unknown;
            }

            foreach (CensusCount what in All)
            {
                if (!Knows(what) || !before.Knows(what))
                {
                    unknown.Add(what);
                }
            }

            return unknown;
        }

        public override string ToString()
        {
            return Line("census");
        }

        private static int Clean(int count)
        {
            return count < 0 ? NotCounted : count;
        }
    }
}
