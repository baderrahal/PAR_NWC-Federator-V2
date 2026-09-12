using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Federator.Core.Grouping;

namespace Federator.Core.Naming
{
    /// <summary>
    /// One pattern each for the NWF, the NWD and the workbook, because a project may want
    /// them to differ. They start identical.
    /// </summary>
    public sealed class OutputNaming
    {
        public OutputNaming()
        {
            Nwf = new NamePattern();
            Nwd = new NamePattern();
            Workbook = new NamePattern();
        }

        public NamePattern Nwf { get; set; }

        public NamePattern Nwd { get; set; }

        public NamePattern Workbook { get; set; }

        /// <summary>
        /// Write the NWD with a date in its name, so an earlier week still exists. Off by
        /// default.
        ///
        /// The NWF always overwrites and always will. Its clash results live inside it and
        /// are the record of what has been fixed, so it has to be the same file week after
        /// week or that record starts again. The NWD carries no results, it is the picture
        /// of the models as they were, so keeping one per week costs only disk and is the
        /// only way an earlier week still exists.
        /// </summary>
        public bool DateTheNwd { get; set; }

        /// <summary>The date the NWD carries, or null when it is overwriting as usual.</summary>
        public DateTime? NwdDate(DateTime today)
        {
            return DateTheNwd ? (DateTime?)today : null;
        }

        /// <summary>The three, in the order the window shows them.</summary>
        public IList<NamePattern> All()
        {
            return new List<NamePattern> { Nwf, Nwd, Workbook };
        }

        public static IList<string> Labels()
        {
            return new List<string> { "NWF", "NWD", "Workbook" };
        }

        public OutputNaming Copy()
        {
            return new OutputNaming
            {
                Nwf = Nwf.Copy(),
                Nwd = Nwd.Copy(),
                Workbook = Workbook.Copy(),
                DateTheNwd = DateTheNwd
            };
        }
    }
}
