using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Federator.Core.Grouping;

namespace Federator.Core.Naming
{
    /// <summary>Builds a collision from outside this file, for the name table.</summary>
    public static class NameCollisions
    {
        public static NameCollision Make(string kind, string name, IList<string> groups)
        {
            return new NameCollision(kind, name, groups);
        }
    }
}
