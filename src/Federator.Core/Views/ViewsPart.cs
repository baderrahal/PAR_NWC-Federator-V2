namespace Federator.Core.Views
{
    /// <summary>
    /// One call the VIEWS step's seconds go to, FR-073, in the order a viewpoint meets them.
    /// The words for each are ViewsSeconds.Describe and nothing else spells them.
    /// </summary>
    public enum ViewsPart
    {
        /// <summary>Walk one over every clash the report holds rows for, and the plan run over them.</summary>
        ReadingTheClashes = 0,

        /// <summary>The live line in the window, and the progress line in the log.</summary>
        SayingHowFar = 1,

        /// <summary>Whether a viewpoint of that name is already at its path.</summary>
        LookingWhetherThere = 2,

        /// <summary>The hidden state read once, then the models the viewpoint hides hidden and the rest shown.</summary>
        ShowingAndHiding = 3,

        /// <summary>Everything but the two clashing items made transparent, and the two painted.</summary>
        Dimming = 4,

        /// <summary>Every folder on the path found, or made where it is not there yet.</summary>
        MakingTheFolders = 5,

        /// <summary>The COM view made, named, its flags set and the clash camera put on it.</summary>
        MakingTheView = 6,

        /// <summary>The same folder found again, name by name, in the COM tree.</summary>
        FindingTheFolder = 7,

        /// <summary>The COM add, into the folder's own collection or at the root.</summary>
        AddingTheView = 8,

        /// <summary>The root route only: the copy into the folder and the root one removed.</summary>
        MovingIntoTheFolder = 9,

        /// <summary>The written viewpoint read back off the tree and counted.</summary>
        ReadingBack = 10,

        /// <summary>The dimming taken off and the hidden state the document held put back.</summary>
        PuttingBack = 11
    }
}
