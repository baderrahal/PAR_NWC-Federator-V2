using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Federator.Core.Grouping;

namespace Federator.Core.Naming
{
    /// <summary>Two groups that would be written to the same name.</summary>
    public sealed class NameCollision
    {
        internal NameCollision(string kind, string name, IList<string> groups)
        {
            Kind = kind;
            Name = name;
            Groups = new ReadOnlyCollection<string>(groups);
        }

        /// <summary>NWF, NWD or Workbook.</summary>
        public string Kind { get; private set; }

        public string Name { get; private set; }

        /// <summary>Every group that would be written to that one name.</summary>
        public ReadOnlyCollection<string> Groups { get; private set; }

        public string Sentence()
        {
            return Groups.Count + " groups would be written to the same "
                + Kind + " name, " + Name + ". They are "
                + string.Join(", ", new List<string>(Groups).ToArray())
                + ". One would overwrite the other, so the run does not start.";
        }

        public override string ToString()
        {
            return Sentence();
        }
    }
}
