using System.Collections.Generic;
using Federator.Core.Naming;

namespace Federator.Core.Views
{
    /// <summary>
    /// One model of the group as the views see it, F114: its file name, the discipline code
    /// part 5 of that name carries, read by ContainerName.Parse, and the team map's team of that
    /// code. An empty code is a name that would not read, and its team is UNKNOWN.
    /// </summary>
    public sealed class ModelTeam
    {
        public ModelTeam(string fileName, string code, string team)
        {
            FileName = fileName ?? string.Empty;
            Code = code ?? string.Empty;
            Team = team ?? string.Empty;
        }

        /// <summary>The model's file name as the document holds it, a path or a bare name.</summary>
        public string FileName { get; private set; }

        /// <summary>Its discipline code, or empty where its name would not read.</summary>
        public string Code { get; private set; }

        /// <summary>The team map's team of that code.</summary>
        public string Team { get; private set; }

        /// <summary>
        /// Whether any of those names is this model's, F114 attempt 4, the one place a name handed
        /// in, a clashing item's home or a hidden model read back, is matched to a model. Matched
        /// by ContainerName.SameName, the stem of the file name without case, so a path, a bare
        /// file name and a display name with no extension all reach the model, and a name whose
        /// stem is empty reaches none.
        /// </summary>
        internal bool IsAmong(IEnumerable<string> names)
        {
            if (names == null || ContainerName.Stem(FileName).Length == 0)
            {
                return false;
            }

            foreach (string name in names)
            {
                if (name != null && ContainerName.SameName(FileName, name))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
