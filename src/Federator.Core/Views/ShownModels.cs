using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Federator.Core.Teams;

namespace Federator.Core.Views
{
    /// <summary>
    /// Which models one view shows and which it hides, F114, Q114 point 13, on Bader's answer B
    /// to Q119 on 2026-10-05, in his words: a view shows only the models its clashing items live
    /// in. With Q118 by its answer A.
    ///
    /// SHOWN: the model each clashing item lives in, and nothing else. Every other model is
    /// hidden, a model of the pair's own two teams and a model whose code will not read among
    /// them. A home in a model of a third team is shown and named as an exception, Q118 A, and
    /// the VIEWS TREE check 3 reports it. A home whose model's code will not read is shown, and
    /// whether it is of a third team is UNKNOWN, which check 3 says for its view. File names
    /// compare Ordinal, as the document holds them.
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
        private readonly List<string> homesNotInGroup = new List<string>();

        private ShownModels()
        {
        }

        /// <summary>The models the view shows, the ones its clashing items live in.</summary>
        public ReadOnlyCollection<ModelTeam> Shown
        {
            get { return new ReadOnlyCollection<ModelTeam>(shown); }
        }

        /// <summary>The models the view hides, every one no clashing item of it lives in.</summary>
        public ReadOnlyCollection<ModelTeam> Hidden
        {
            get { return new ReadOnlyCollection<ModelTeam>(hidden); }
        }

        /// <summary>The models of a third team shown because a clashing item lives in them, Q118 A.</summary>
        public ReadOnlyCollection<ModelTeam> Exceptions
        {
            get { return new ReadOnlyCollection<ModelTeam>(exceptions); }
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

                if (!homeNames.Contains(model.FileName))
                {
                    outcome.hidden.Add(model);
                    continue;
                }

                outcome.shown.Add(model);

                if (model.Code.Length > 0 && !IsOfThePair(model, pair))
                {
                    outcome.exceptions.Add(model);
                }
            }

            outcome.homesNotInGroup.AddRange(homeNames.FindAll(home => !fileNames.Contains(home)));
            return outcome;
        }

        /// <summary>Whether the model's team is one of the pair's two, Ordinal, the one place a view's model is judged against its pair.</summary>
        internal static bool IsOfThePair(ModelTeam model, TeamPair pair)
        {
            return string.Equals(model.Team, pair.First, StringComparison.Ordinal)
                || string.Equals(model.Team, pair.Second, StringComparison.Ordinal);
        }
    }
}
