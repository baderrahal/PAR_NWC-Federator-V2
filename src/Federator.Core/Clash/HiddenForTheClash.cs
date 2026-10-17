using System.Globalization;

namespace Federator.Core.Clash
{
    /// <summary>
    /// F204. Clash Detective finds no clash on a hidden item. Measured on 17 Oct 2026 on a copy of the C04 NWF Bader saved
    /// on 15 Oct at 14:51 with 8 of its 10 models hidden: one test run by TestsRunTest took 0.04 s and turned its 232
    /// results Resolved, the same test on the same NWCs with nothing hidden took 0.4 s and found the 232, and the same NWF
    /// with the 8 items shown in memory took 1.74 s and found the 232. A refresh of that NWF on the NWF page marked 10016 of
    /// its 13286 clashes Resolved. So the clash step shows everything for the run and hides again exactly what was hidden,
    /// and says so. These are its words, the add-in's ClashRunner.Run does the showing and the putting back.
    /// </summary>
    public static class HiddenForTheClash
    {
        /// <summary>What the step found hidden before the tests ran: the hidden items and the models hidden whole.</summary>
        public static string Found(int hiddenItems, int hiddenModels, int models)
        {
            if (hiddenItems <= 0 && hiddenModels <= 0)
            {
                return "CLASH    HIDDEN   nothing is hidden in the document, so every item is clashed as it stands";
            }

            return "CLASH    HIDDEN   " + Count(hiddenItems) + " hidden item(s), " + Count(hiddenModels) + " of " + Count(models)
                + " model(s) hidden whole. Clash Detective finds no clash on a hidden item and would mark the clashes of a hidden "
                + "model Resolved, so everything is shown for the clash and hidden again after it";
        }

        /// <summary>What the step says once the hidden state is put back, read back off the document.</summary>
        public static string PutBack(bool readBackHidden, int hiddenItems)
        {
            if (readBackHidden)
            {
                return "CLASH    HIDDEN   the " + Count(hiddenItems) + " item(s) hidden before the clash are hidden again, read back";
            }

            return "CLASH    HIDDEN   NOTICE the " + Count(hiddenItems) + " item(s) hidden before the clash do not all read hidden again. "
                + "The clash results are right. Only what the saved NWF shows hidden may differ, and a person may hide them again by hand";
        }

        /// <summary>What the step says where showing or putting back threw, so the run goes on and the reader knows.</summary>
        public static string Threw(string what, string message)
        {
            return "CLASH    HIDDEN   NOTICE " + what + " threw: " + (message ?? string.Empty)
                + ". The run goes on, and the clashes of any item still hidden may read Resolved";
        }

        private static string Count(int n)
        {
            return n.ToString(CultureInfo.InvariantCulture);
        }
    }
}
