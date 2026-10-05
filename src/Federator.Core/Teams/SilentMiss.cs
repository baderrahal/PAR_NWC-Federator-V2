using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Federator.Core.Teams
{
    /// <summary>
    /// One set that can find nothing in one model of its own team with another code, F131, FR-181,
    /// Q114 point 3: a set of team T and code C, and a model of team T whose code is not C, where
    /// every group of the set asks a workset that model does not carry or a Source File its name
    /// does not hold. SilentMisses finds them and says them.
    /// </summary>
    public sealed class SilentMiss
    {
        internal SilentMiss(
            string setName,
            string setCode,
            string team,
            string model,
            string modelCode,
            IList<string> worksetsAsked,
            IList<string> fileNameAsks,
            IList<string> categories,
            int? uncaught,
            bool uncaughtWhole,
            IList<string> drafted)
        {
            SetName = setName;
            SetCode = setCode;
            Team = team;
            Model = model;
            ModelCode = modelCode;
            WorksetsAsked = new ReadOnlyCollection<string>(new List<string>(worksetsAsked));
            FileNameAsks = new ReadOnlyCollection<string>(new List<string>(fileNameAsks));
            Categories = new ReadOnlyCollection<string>(new List<string>(categories));
            Uncaught = uncaught;
            UncaughtWhole = uncaughtWhole;
            Drafted = new ReadOnlyCollection<string>(new List<string>(drafted));
        }

        /// <summary>The set, as the XML names it.</summary>
        public string SetName { get; private set; }

        /// <summary>The code its name carries.</summary>
        public string SetCode { get; private set; }

        /// <summary>The team of the set and of the model.</summary>
        public string Team { get; private set; }

        /// <summary>The model's file name.</summary>
        public string Model { get; private set; }

        /// <summary>The model's code, part 5 of its file name, not the set's.</summary>
        public string ModelCode { get; private set; }

        /// <summary>The workset values the set's groups ask that the model does not carry, each once.</summary>
        public ReadOnlyCollection<string> WorksetsAsked { get; private set; }

        /// <summary>What the set's groups ask a Source File to hold that the model's file name does not, each once.</summary>
        public ReadOnlyCollection<string> FileNameAsks { get; private set; }

        /// <summary>The categories the set's groups ask, each once.</summary>
        public ReadOnlyCollection<string> Categories { get; private set; }

        /// <summary>
        /// How many items of those categories the model holds that no set catches, the coverage
        /// count of Q112 request 2, added up over the categories counted. Null where it is
        /// UNKNOWN: no count was handed in, the set asks no category by its whole name, or no
        /// category counted above zero and one was not counted. A count not taken, null or
        /// below zero as ModelExport.NotCounted is, is never read as a zero.
        /// </summary>
        public int? Uncaught { get; private set; }

        /// <summary>
        /// Whether every category of the set was counted, so Uncaught is the whole count. False
        /// where one above zero stands beside one not taken, and Uncaught is then a lower bound.
        /// </summary>
        public bool UncaughtWhole { get; private set; }

        /// <summary>
        /// The correction drafted for Bader to approve, one line of the list of corrections each,
        /// built the way the both-spellings rows of Q102 are and NEVER APPLIED by the tool. Empty
        /// where no spelling of the model could be drafted.
        /// </summary>
        public ReadOnlyCollection<string> Drafted { get; private set; }

        /// <summary>Whether the coverage count shows the model holding items of the set's categories that no set catches.</summary>
        public bool Confirmed
        {
            get { return Uncaught.HasValue && Uncaught.Value > 0; }
        }
    }
}
