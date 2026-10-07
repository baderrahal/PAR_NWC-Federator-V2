using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

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
    /// whether it is of a third team is UNKNOWN, which check 3 says for its view. A home is tied
    /// to a model by ModelNames, the one rule, so a home written as a path or as a display name
    /// reaches its model, F114 attempts 4 and 5.
    ///
    /// A HOME NOT KNOWN IS SAID, F114 attempt 2. A home that could not be read is counted, one
    /// that names no model of the group is named, and since attempt 5 one that names more than
    /// one model is named and shows both, because which model that clashing item lives in is
    /// UNKNOWN. Check 3 does not run for that view.
    /// </summary>
    public sealed class ShownModels
    {
        private readonly List<ModelTeam> shown = new List<ModelTeam>();
        private readonly List<ModelTeam> hidden = new List<ModelTeam>();
        private readonly List<ModelTeam> exceptions = new List<ModelTeam>();
        private readonly List<string> homesNotInGroup = new List<string>();
        private readonly List<string> homesOfManyModels = new List<string>();

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

        /// <summary>How many of the homes handed in could not be read, null, empty or blank.</summary>
        public int HomesNotRead { get; private set; }

        /// <summary>Each home handed in that names no model of the group, once, in the order handed in.</summary>
        public ReadOnlyCollection<string> HomesNotInGroup
        {
            get { return new ReadOnlyCollection<string>(homesNotInGroup); }
        }

        /// <summary>Each home handed in that names more than one model of the group, once, in the order handed in, F114 attempt 5.</summary>
        public ReadOnlyCollection<string> HomesOfManyModels
        {
            get { return new ReadOnlyCollection<string>(homesOfManyModels); }
        }

        /// <summary>What a view of that pair shows, given the group's models and the models its clashing items live in.</summary>
        public static ShownModels For(TeamPair pair, IEnumerable<ModelTeam> models, IEnumerable<string> homes)
        {
            return For(pair, new ModelNames(models), homes);
        }

        /// <summary>The same, with the group's models already found by name, so a check of many views takes their stems once.</summary>
        internal static ShownModels For(TeamPair pair, ModelNames models, IEnumerable<string> homes)
        {
            if (pair == null)
            {
                throw new ArgumentNullException("pair");
            }

            ShownModels outcome = new ShownModels();
            NamesTied tied = models.Tie(homes);

            outcome.HomesNotRead = tied.Blank;
            outcome.homesNotInGroup.AddRange(tied.OfNoModel);
            outcome.homesOfManyModels.AddRange(tied.OfManyModels);
            outcome.hidden.AddRange(tied.NotReached);

            foreach (ModelTeam model in tied.Reached)
            {
                outcome.shown.Add(model);

                if (model.Code.Length > 0 && !IsOfThePair(model, pair))
                {
                    outcome.exceptions.Add(model);
                }
            }

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
