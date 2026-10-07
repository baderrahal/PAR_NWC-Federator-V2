using System.Collections.Generic;

namespace Federator.Core.Views
{
    /// <summary>
    /// What ModelNames.Tie made of a list of names, F114 attempt 5: the models the names reach and
    /// the rest, each in the group's order, and each name it could not tie to one model.
    /// </summary>
    internal sealed class NamesTied
    {
        internal NamesTied()
        {
            Reached = new List<ModelTeam>();
            NotReached = new List<ModelTeam>();
            OfNoModel = new List<string>();
            OfManyModels = new List<string>();
        }

        /// <summary>The models some name reaches, a name of two models reaching both.</summary>
        internal List<ModelTeam> Reached { get; private set; }

        /// <summary>The models no name reaches.</summary>
        internal List<ModelTeam> NotReached { get; private set; }

        /// <summary>How many names were null, empty or blank.</summary>
        internal int Blank { get; set; }

        /// <summary>Each name whose stem is no model's, once, as first handed in.</summary>
        internal List<string> OfNoModel { get; private set; }

        /// <summary>Each name whose stem is more than one model's, once, as first handed in.</summary>
        internal List<string> OfManyModels { get; private set; }

        /// <summary>How many names could not be tied to one model: blank, of no model or of more than one.</summary>
        internal int NotTied
        {
            get { return Blank + OfNoModel.Count + OfManyModels.Count; }
        }
    }
}
