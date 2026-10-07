using System;
using System.Collections.Generic;
using System.Globalization;
using Federator.Core.Health;

namespace Federator.Core.Diagnostics
{
    /// <summary>
    /// What one group held, beside how long it took, Q101. Bader's answer on 2026-10-04:
    /// the 45 minutes is not judged in this round, VIEWS is made faster with the same
    /// viewpoints, and each group's time is reported beside the sizes of its NWC files and
    /// its item counts, so the target can be set after the proof run.
    ///
    /// EVERY NUMBER IS ONE THE RUN ALREADY READ, and none costs a Navisworks call: the
    /// group clock, the files the group was handed and their sizes on the disk, the Revit
    /// elements the EXPORT CHECK counted, the clash total and the viewpoints created. A
    /// number that could not be read is Unknown and never zero, because zero reads as a
    /// real count, and a total over parts of which one could not be read is Unknown too,
    /// because a smaller number reads as a real one.
    /// </summary>
    public sealed class GroupSize
    {
        /// <summary>A number that could not be read. Never zero, because zero reads as a real count.</summary>
        public const long Unknown = -1;

        public GroupSize(
            string building, double seconds, int nwcFiles, long nwcBytes, long elements, long clashes, long viewpoints)
        {
            if (building == null)
            {
                throw new ArgumentNullException("building");
            }

            Building = building;
            Seconds = seconds < 0 ? 0 : seconds;
            NwcFiles = nwcFiles < 0 ? 0 : nwcFiles;
            NwcBytes = nwcBytes < 0 ? Unknown : nwcBytes;
            Elements = elements < 0 ? Unknown : elements;
            Clashes = clashes < 0 ? Unknown : clashes;
            Viewpoints = viewpoints < 0 ? Unknown : viewpoints;
        }

        public string Building { get; private set; }

        /// <summary>The group clock, the same reading its GROUP finished line carries.</summary>
        public double Seconds { get; private set; }

        /// <summary>How many files the group was handed.</summary>
        public int NwcFiles { get; private set; }

        /// <summary>Their sizes on the disk added up, or Unknown where one could not be read.</summary>
        public long NwcBytes { get; private set; }

        /// <summary>The Revit elements the EXPORT CHECK counted, or Unknown.</summary>
        public long Elements { get; private set; }

        /// <summary>Every clash of every test the group ran, or Unknown where no clash step ran.</summary>
        public long Clashes { get; private set; }

        /// <summary>The viewpoints the VIEWS step created, or Unknown where it did not run.</summary>
        public long Viewpoints { get; private set; }

        /// <summary>
        /// The sizes of a group's files added up. Unknown where any one is below zero, which
        /// is how a size that could not be read off the disk comes back, because a total
        /// that left a file out reads as the size of the group and is not.
        /// </summary>
        public static long BytesOf(IList<long> sizes)
        {
            long total = 0;

            if (sizes == null)
            {
                return total;
            }

            foreach (long size in sizes)
            {
                if (size < 0)
                {
                    return Unknown;
                }

                total += size;
            }

            return total;
        }

        /// <summary>
        /// The Revit elements of every model the EXPORT CHECK read, added up. Unknown where
        /// no model was read or one could not be counted, for the same reason as the bytes.
        /// </summary>
        public static long ElementsIn(IList<ModelExport> models)
        {
            if (models == null || models.Count == 0)
            {
                return Unknown;
            }

            long total = 0;

            foreach (ModelExport model in models)
            {
                if (model == null || model.Elements < 0)
                {
                    return Unknown;
                }

                total += model.Elements;
            }

            return total;
        }

        /// <summary>The words of the row, with thousands separators, which is how a person reads a size.</summary>
        internal string Words()
        {
            return Of(NwcFiles, "NWC file", "NWC files", true)
                + "  " + Of(NwcBytes, "bytes", "bytes", true)
                + "  " + Of(Elements, "element", "elements", true)
                + "  " + Of(Clashes, "clash", "clashes", true)
                + "  " + Of(Viewpoints, "viewpoint", "viewpoints", true);
        }

        /// <summary>
        /// The same numbers for the machine readable log, with no separators, so a script
        /// adding them up across a run reads them as numbers.
        /// </summary>
        public string RowText()
        {
            return Of(NwcFiles, "NWC file", "NWC files", false)
                + ", " + Of(NwcBytes, "bytes", "bytes", false)
                + ", " + Of(Elements, "element", "elements", false)
                + ", " + Of(Clashes, "clash", "clashes", false)
                + ", " + Of(Viewpoints, "viewpoint", "viewpoints", false);
        }

        private static string Of(long number, string one, string many, bool separated)
        {
            if (number < 0)
            {
                return "UNKNOWN " + many;
            }

            string shown = separated
                ? number.ToString("N0", CultureInfo.InvariantCulture)
                : number.ToString(CultureInfo.InvariantCulture);

            return shown + " " + (number == 1 ? one : many);
        }
    }
}
