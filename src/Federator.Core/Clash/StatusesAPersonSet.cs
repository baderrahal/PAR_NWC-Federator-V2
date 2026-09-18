using System;
using System.Collections.Generic;

namespace Federator.Core.Clash
{
    /// <summary>
    /// Which result statuses mean a person decided something, and which is only where a
    /// clash starts.
    ///
    /// WHY THIS IS A RULE AND NOT A COMPARISON WRITTEN WHERE IT IS NEEDED. F50 counts what
    /// the NWF carries across a rebuild, and the clash results are the part with no second
    /// copy anywhere. A result at New is what running a test produces, so losing it costs
    /// nothing, the next run makes it again. A result a person moved to Active, Reviewed,
    /// Approved or Resolved is a decision somebody made while looking at the model, and
    /// this tool runs weekly, so losing those is losing however many weeks of review the
    /// file has collected.
    ///
    /// Reviewed sits on this list even though F54 lets the tool set it, because the tool
    /// setting a status does not make it worth less. It makes it worth the same and cheaper
    /// to produce.
    ///
    /// New is the only status this list leaves out, and it is left out because it is the
    /// absence of a decision rather than one of the choices.
    /// </summary>
    public static class StatusesAPersonSet
    {
        /// <summary>
        /// True where the status is one somebody chose. Anything that is not New, which
        /// includes any value the enum does not name, because a status this Core does not
        /// recognise is not one to throw away quietly.
        /// </summary>
        public static bool Counts(ClashStatus status)
        {
            return status != ClashStatus.New;
        }

        /// <summary>Every status that counts, in the order the enum declares them.</summary>
        public static ClashStatus[] All()
        {
            return new[]
            {
                ClashStatus.Active,
                ClashStatus.Reviewed,
                ClashStatus.Approved,
                ClashStatus.Resolved
            };
        }

        /// <summary>
        /// How many of a tally's results carry a status somebody chose. The tally is the one
        /// counted off the document, so this is the number the rebuild has to see come back.
        /// </summary>
        public static int In(ClashTally tally)
        {
            if (tally == null)
            {
                return 0;
            }

            int total = 0;
            ClashStatus[] counted = All();

            for (int i = 0; i < counted.Length; i++)
            {
                total += tally.Of(counted[i]);
            }

            return total;
        }

        /// <summary>
        /// The words for a log line, naming each status with its count, so a rebuild that
        /// lost something says WHICH kind of decision went rather than only how many.
        /// </summary>
        public static string Describe(ClashTally tally)
        {
            if (tally == null)
            {
                return "none, nothing was counted";
            }

            List<string> parts = new List<string>();
            ClashStatus[] counted = All();

            for (int i = 0; i < counted.Length; i++)
            {
                int howMany = tally.Of(counted[i]);

                if (howMany > 0)
                {
                    parts.Add(howMany + " " + counted[i].ToString());
                }
            }

            return parts.Count == 0 ? "none, every clash is still at New" : string.Join(", ", parts.ToArray());
        }
    }
}
