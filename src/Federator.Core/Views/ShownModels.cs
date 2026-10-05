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
    /// </summary>
    public sealed class ShownModels
    {
        private readonly List<ModelTeam> shown = new List<ModelTeam>();
        private readonly List<ModelTeam> hidden = new List<ModelTeam>();
        private readonly List<ModelTeam> exceptions = new List<ModelTeam>();
        private readonly List<ModelTeam> noCode = new List<ModelTeam>();

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

        /// <summary>What a view of that pair shows, given the group's models and the models its clashing items live in.</summary>
        public static ShownModels For(TeamPair pair, IEnumerable<ModelTeam> models, IEnumerable<string> homes)
        {
            if (pair == null)
            {
                throw new ArgumentNullException("pair");
            }

            HashSet<string> homeNames = new HashSet<string>(StringComparer.Ordinal);

            if (homes != null)
            {
                foreach (string home in homes)
                {
                    if (!string.IsNullOrEmpty(home))
                    {
                        homeNames.Add(home);
                    }
                }
            }

            ShownModels outcome = new ShownModels();

            if (models == null)
            {
                return outcome;
            }

            foreach (ModelTeam model in models)
            {
                if (model == null)
                {
                    continue;
                }

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

            return outcome;
        }
    }
}
