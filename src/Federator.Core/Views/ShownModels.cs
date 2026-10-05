using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Federator.Core.Teams;

namespace Federator.Core.Views
{
    /// <summary>
    /// Which models one view shows and which it hides, F114, Q114 point 13, with Q119 and Q118
    /// by their defaults A.
    ///
    /// SHOWN: every model whose team is one of the pair's two, so a building whose mechanical
    /// work is split over HV, FP and four ME models shows all six in a Mechanical pair, plus the
    /// model each clashing item lives in, plus every model whose code will not read, which is
    /// never hidden on a guess and is counted. A home in a model of a third team is shown and
    /// named as an exception, the VIEWS TREE check 3 reports it. Every other model is hidden.
    /// File names compare Ordinal, as the document holds them.
    ///
    /// A HOME NOT KNOWN IS SAID, F114 attempt 2. A home that could not be read is counted, and one
    /// that names no model of the group is named, because whether the model of that clashing item
    /// is shown is UNKNOWN, and check 3 says so for its view.
    /// </summary>
    public sealed class ShownModels
    {
        private readonly List<ModelTeam> shown = new List<ModelTeam>();
        private readonly List<ModelTeam> hidden = new List<ModelTeam>();
        private readonly List<ModelTeam> exceptions = new List<ModelTeam>();
        private readonly List<ModelTeam> noCode = new List<ModelTeam>();
        private readonly List<string> homesNotInGroup = new List<string>();

        private ShownModels()
        {
        }

        /// <summary>The models the view shows.</summary>
        public ReadOnlyCollection<ModelTeam> Shown
        {
            get { return new ReadOnlyCollection<ModelTeam>(shown); }
        }

        /// <summary>The models the view hides.</summary>
        public ReadOnlyCollection<ModelTeam> Hidden
        {
            get { return new ReadOnlyCollection<ModelTeam>(hidden); }
        }

        /// <summary>The models of a third team shown because a clashing item lives in them, Q118 A.</summary>
        public ReadOnlyCollection<ModelTeam> Exceptions
        {
            get { return new ReadOnlyCollection<ModelTeam>(exceptions); }
        }

        /// <summary>The models whose code would not read, shown and counted, never hidden on a guess.</summary>
        public ReadOnlyCollection<ModelTeam> NoCode
        {
            get { return new ReadOnlyCollection<ModelTeam>(noCode); }
        }

        /// <summary>How many of the homes handed in could not be read, null or empty.</summary>
        public int HomesNotRead { get; private set; }

        /// <summary>Each home handed in that names no model of the group, once, in the order handed in.</summary>
        public ReadOnlyCollection<string> HomesNotInGroup
        {
            get { return new ReadOnlyCollection<string>(homesNotInGroup); }
        }

        /// <summary>What a view of that pair shows, given the group's models and the models its clashing items live in.</summary>
        public static ShownModels For(TeamPair pair, IEnumerable<ModelTeam> models, IEnumerable<string> homes)
        {
            if (pair == null)
            {
                throw new ArgumentNullException("pair");
            }

            ShownModels outcome = new ShownModels();
            List<string> homeNames = new List<string>();

            if (homes != null)
            {
                foreach (string home in homes)
                {
                    if (string.IsNullOrEmpty(home))
                    {
                        outcome.HomesNotRead = outcome.HomesNotRead + 1;
                    }
                    else if (!homeNames.Contains(home))
                    {
                        homeNames.Add(home);
                    }
                }
            }

            HashSet<string> fileNames = new HashSet<string>(StringComparer.Ordinal);

            foreach (ModelTeam model in models ?? new ModelTeam[0])
            {
                if (model == null)
                {
                    continue;
                }

                fileNames.Add(model.FileName);

                bool ofThePair = string.Equals(model.Team, pair.First, StringComparison.Ordinal)
                    || string.Equals(model.Team, pair.Second, StringComparison.Ordinal);

                if (model.Code.Length == 0)
                {
                    outcome.noCode.Add(model);
                    outcome.shown.Add(model);
                }
                else if (ofThePair)
                {
                    outcome.shown.Add(model);
                }
                else if (homeNames.Contains(model.FileName))
                {
                    outcome.exceptions.Add(model);
                    outcome.shown.Add(model);
                }
                else
                {
                    outcome.hidden.Add(model);
                }
            }

            outcome.homesNotInGroup.AddRange(homeNames.FindAll(home => !fileNames.Contains(home)));
            return outcome;
        }
    }
}
