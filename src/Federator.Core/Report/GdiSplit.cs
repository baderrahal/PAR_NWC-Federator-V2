using System;
using System.Globalization;

namespace Federator.Core.Report
{
    /// <summary>
    /// Where the GDI objects of a picture pass go, F161. Run 20261014-105308 climbed one GDI object a
    /// picture, 1092 at the first and 9999 at the 8907th, and Navisworks crashed in lcodwin.dll five
    /// seconds after the 10000 a process is allowed. Whether the object is made inside Navisworks's
    /// TestsImageForResult or in the save and the dispose of the bitmap is UNKNOWN. Each picture adds
    /// the count before the render, after it and after the save, and each MEMORY line is followed
    /// by one line of the sums since the last, so the next run answers it.
    /// </summary>
    public sealed class GdiSplit
    {
        private long render;
        private long save;
        private int pictures;
        private int unread;

        /// <summary>One picture's three readings. A reading below zero is not read and the picture is counted as such.</summary>
        public void Add(long before, long afterRender, long afterSave)
        {
            if (before < 0 || afterRender < 0 || afterSave < 0)
            {
                unread++;
                return;
            }

            pictures++;
            render += afterRender - before;
            save += afterSave - afterRender;
        }

        /// <summary>Null where no picture was read since the last line, and otherwise the line, with the sums put back to nought.</summary>
        public string TakeLine()
        {
            if (pictures == 0 && unread == 0)
            {
                return null;
            }

            string line = "GDI      PICTURES in the last " + pictures.ToString(CultureInfo.InvariantCulture)
                + " pictures the GDI objects moved " + Signed(render) + " in TestsImageForResult and "
                + Signed(save) + " in the save and the dispose of the bitmap"
                + (unread > 0 ? ", and " + unread.ToString(CultureInfo.InvariantCulture) + " not read" : string.Empty);

            render = 0;
            save = 0;
            pictures = 0;
            unread = 0;
            return line;
        }

        private static string Signed(long number)
        {
            return (number >= 0 ? "+" : string.Empty) + number.ToString(CultureInfo.InvariantCulture);
        }
    }
}