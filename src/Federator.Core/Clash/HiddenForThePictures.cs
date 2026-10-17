using System.Globalization;

namespace Federator.Core.Clash
{
    /// <summary>
    /// F206. A picture of a clash whose item is hidden shows nothing of that item. Measured on 17 Oct 2026 on the audit
    /// round's run 2 of the C04 copy, saved with 8 of its 10 models hidden whole: F204 hid them again after the clash step,
    /// the pictures are taken after the NWF is saved, F182, and of the 3080 pictures 744 showed only the sky and the grid
    /// label, and 2127 belonged to tests with a side in a hidden model. So the picture pass shows everything as the clash
    /// step does and hides again exactly what was hidden, and says so. These are its words, the add-in's
    /// ClashRunner.TakeTheDeferredPictures does the showing and the putting back.
    /// </summary>
    public static class HiddenForThePictures
    {
        /// <summary>What the pass found hidden before the pictures: the hidden items and the models hidden whole.</summary>
        public static string Found(int hiddenItems, int hiddenModels, int models)
        {
            if (hiddenItems <= 0 && hiddenModels <= 0)
            {
                return "IMAGES   HIDDEN   nothing is hidden in the document, so every picture shows its clash as it stands";
            }

            return "IMAGES   HIDDEN   " + Count(hiddenItems) + " hidden item(s), " + Count(hiddenModels) + " of " + Count(models)
                + " model(s) hidden whole. A picture shows nothing of a hidden item, so everything is shown for the pictures "
                + "and hidden again after them";
        }

        /// <summary>What the pass says once the hidden state is put back, read back off the document.</summary>
        public static string PutBack(bool readBackHidden, int hiddenItems)
        {
            if (readBackHidden)
            {
                return "IMAGES   HIDDEN   the " + Count(hiddenItems) + " item(s) hidden before the pictures are hidden again, read back";
            }

            return "IMAGES   HIDDEN   NOTICE the " + Count(hiddenItems) + " item(s) hidden before the pictures do not all read hidden "
                + "again. The pictures are right, and the NWF and the NWD were saved before them. Only the open document may "
                + "differ, and a person may hide them again by hand";
        }

        /// <summary>What the pass says where showing or putting back threw, so the run goes on and the reader knows.</summary>
        public static string Threw(string what, string message)
        {
            return "IMAGES   HIDDEN   NOTICE " + what + " threw: " + (message ?? string.Empty)
                + ". The run goes on, and a picture of an item still hidden may show nothing of it";
        }

        private static string Count(int n)
        {
            return n.ToString(CultureInfo.InvariantCulture);
        }
    }
}
