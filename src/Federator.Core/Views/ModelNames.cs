using System;
using System.Collections.Generic;
using Federator.Core.Naming;

namespace Federator.Core.Views
{
    /// <summary>
    /// The group's models found by name, F114 attempt 5, the one place a name handed to the
    /// views is tied to a model: a clashing item's home, for the plan, the tree line and check 3,
    /// and a hidden model read back, for the tree line and check 3. A name is tied by its
    /// ContainerName.Stem under ContainerName.StemComparer, so a path, a bare file name and a
    /// display name with no extension all reach their model.
    ///
    /// A NAME THAT REACHES NO MODEL, OR MORE THAN ONE, IS NOT TIED, and Tie lists it. More than
    /// one is the same file name in two folders, which a group gathered with subfolders can hold.
    /// A model whose file name has no stem can be reached by no name at all, and is listed in
    /// Unnamed. A check that meets any of these did not run for it, and says so.
    /// </summary>
    internal sealed class ModelNames
    {
        private readonly List<ModelTeam> group;
        private readonly List<ModelTeam> unnamed = new List<ModelTeam>();
        private readonly Dictionary<string, List<ModelTeam>> byStem =
            new Dictionary<string, List<ModelTeam>>(ContainerName.StemComparer);

        /// <summary>The models handed in, a null among them left out, the one place that is decided.</summary>
        internal ModelNames(IEnumerable<ModelTeam> models)
        {
            group = new List<ModelTeam>(models ?? new ModelTeam[0]).FindAll(model => model != null);

            foreach (ModelTeam model in group)
            {
                string stem = ContainerName.Stem(model.FileName);

                if (stem.Length == 0)
                {
                    unnamed.Add(model);
                    continue;
                }

                List<ModelTeam> named;

                if (!byStem.TryGetValue(stem, out named))
                {
                    named = new List<ModelTeam>();
                    byStem.Add(stem, named);
                }

                named.Add(model);
            }
        }

        /// <summary>The group's models in the order handed in.</summary>
        internal List<ModelTeam> Group
        {
            get { return new List<ModelTeam>(group); }
        }

        /// <summary>The models whose file name has no stem, which no name can reach.</summary>
        internal List<ModelTeam> Unnamed
        {
            get { return new List<ModelTeam>(unnamed); }
        }

        /// <summary>
        /// Those names tied to the group's models. A name is read once, the first of those that
        /// share a stem standing for them all, so the Stem of each text is taken once however many
        /// clashes repeat it.
        /// </summary>
        internal NamesTied Tie(IEnumerable<string> names)
        {
            NamesTied tied = new NamesTied();
            HashSet<string> texts = new HashSet<string>(StringComparer.Ordinal);
            HashSet<string> stems = new HashSet<string>(ContainerName.StemComparer);
            HashSet<ModelTeam> reached = new HashSet<ModelTeam>();

            foreach (string name in names ?? new string[0])
            {
                if (string.IsNullOrWhiteSpace(name))
                {
                    tied.Blank++;
                    continue;
                }

                if (!texts.Add(name))
                {
                    continue;
                }

                string stem = ContainerName.Stem(name);

                if (!stems.Add(stem))
                {
                    continue;
                }

                List<ModelTeam> named;

                if (!byStem.TryGetValue(stem, out named))
                {
                    tied.OfNoModel.Add(name);
                    continue;
                }

                if (named.Count > 1)
                {
                    tied.OfManyModels.Add(name);
                }

                reached.UnionWith(named);
            }

            foreach (ModelTeam model in group)
            {
                (reached.Contains(model) ? tied.Reached : tied.NotReached).Add(model);
            }

            return tied;
        }
    }
}
