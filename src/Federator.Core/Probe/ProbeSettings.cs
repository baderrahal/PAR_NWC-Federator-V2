using System;
using System.Collections.Generic;
using System.IO;
using Federator.Core.Clash;

namespace Federator.Core.Probe
{
    /// <summary>
    /// What the property probe reads and where it writes, F86.
    ///
    /// WHY IT EXISTS. The mechanical selection sets ask for categories, and nobody knows
    /// what the models actually carry. Whether Fire Suppression pipework is told apart
    /// from domestic pipework by a property, and what that property is called, has been
    /// guessed at rather than read. So this walks the properties and writes what it found,
    /// and the rewriting of the sets is a separate piece of work afterwards.
    ///
    /// IT CHANGES NOTHING. It never opens an NWF, never saves, never publishes and never
    /// touches a clash test. It reads and writes one CSV per file. That is the whole of
    /// it, and the guard below is what makes the never enforceable rather than a promise
    /// in a comment.
    ///
    /// THE SEVENTEEN CATEGORIES ARE READ OFF THE CLIENT'S MATRIX AND NOT TYPED. They are
    /// every category a SERVICE DISCIPLINE set asks for, which is the thirteen this tool
    /// calls a service plus the four it has decided are not one. Both lists live in
    /// PenetrationSettings and this reads them, because one rule lives in one place and
    /// two copies of a category list would drift. A test asserts the seventeen against the
    /// matrix on every run, so a category added to the client's file cannot be missed.
    /// </summary>
    public sealed class ProbeSettings
    {
        /// <summary>
        /// How many distinct values are listed per property. A property such as a comment
        /// or a mark carries a different value on every element, so an uncapped probe on
        /// one model would write hundreds of thousands of rows nobody reads. What was
        /// dropped is SAID rather than left out, in the CSV and in the verdict block.
        /// </summary>
        public const int DefaultDistinctValueCap = 100;

        /// <summary>What is put on the end of the model's name to make the CSV's name.</summary>
        public const string CsvSuffix = "-properties.csv";

        /// <summary>The words on the button.</summary>
        public const string ButtonLabel = "Probe model properties";

        /// <summary>The grey line under it. Twelve words, which is the limit.</summary>
        public const string HelpLine =
            "Reads a folder of NWC, or the open file. Writes one CSV";

        /// <summary>
        /// The seventeen, worked out from the two lists in PenetrationSettings rather than
        /// typed here. In the order a person reads them: the ducts, then the pipes, then
        /// the electrical services, then the four that are not services.
        /// </summary>
        public static IList<string> DefaultCategories()
        {
            List<string> categories = new List<string>();
            categories.AddRange(PenetrationSettings.DefaultServiceCategories);
            categories.AddRange(PenetrationSettings.DefaultNotAServiceCategories);
            return categories;
        }

        public ProbeSettings()
        {
            Categories = DefaultCategories();
            CategoryNames = new List<string>(PenetrationSettings.DefaultCategoryNames);
            DistinctValueCap = DefaultDistinctValueCap;
        }

        /// <summary>The categories whose items are read. A setting, so a project can narrow it.</summary>
        public IList<string> Categories { get; set; }

        /// <summary>Where a category is read from, in order, the first that answers winning.</summary>
        public IList<string> CategoryNames { get; set; }

        /// <summary>How many distinct values are listed per property.</summary>
        public int DistinctValueCap { get; set; }

        /// <summary>
        /// Whether that category is one the probe asks for, F86, read the way the
        /// penetration rule reads a category against its own lists, through the one
        /// comparison in PenetrationSettings.Names, so the probe finds exactly the items
        /// the rule would judge and the two can never disagree about a trailing space or
        /// a capital.
        /// </summary>
        public bool Asks(string category)
        {
            return PenetrationSettings.Names(Categories, category);
        }

        /// <summary>The CSV this file gets, beside the file and named after it.</summary>
        public static string CsvPathFor(string modelPath)
        {
            if (string.IsNullOrEmpty(modelPath))
            {
                throw new ArgumentException("A model with no path has nowhere to put a CSV.", "modelPath");
            }

            string folder = Path.GetDirectoryName(modelPath);
            string name = Path.GetFileNameWithoutExtension(modelPath) + CsvSuffix;

            return string.IsNullOrEmpty(folder) ? name : Path.Combine(folder, name);
        }

        /// <summary>
        /// Whether the probe may read that file, and why not where it may not. NWC only.
        ///
        /// AN NWF IS REFUSED BY NAME. The probe never opens one, because an NWF holds the
        /// clash tests and every clash result in them and there is no second copy of any
        /// of it. A read that cannot write is still a read that replaces the open
        /// document, and a person who probed a folder and lost the federation they had
        /// open would be right to stop trusting the tool. An NWD is refused the same way:
        /// it is an output of this tool and probing one reads yesterday's model.
        /// </summary>
        public static bool MayRead(string path, out string why)
        {
            if (string.IsNullOrEmpty(path))
            {
                why = "no file was named";
                return false;
            }

            string extension = Path.GetExtension(path);

            if (string.Equals(extension, ".nwc", StringComparison.OrdinalIgnoreCase))
            {
                why = null;
                return true;
            }

            if (string.Equals(extension, ".nwf", StringComparison.OrdinalIgnoreCase))
            {
                why = "the probe never opens an NWF, because that is where the clash tests "
                    + "and every clash result live and opening one replaces whatever is open";
                return false;
            }

            if (string.Equals(extension, ".nwd", StringComparison.OrdinalIgnoreCase))
            {
                why = "an NWD is something this tool writes, so probing one reads the model "
                    + "as it stood rather than the model as it is";
                return false;
            }

            why = "the probe reads NWC files, and \"" + extension + "\" is not one";
            return false;
        }
    }
}
