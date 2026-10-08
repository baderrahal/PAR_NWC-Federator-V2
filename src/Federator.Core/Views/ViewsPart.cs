namespace Federator.Core.Views
{
    /// <summary>
    /// One call the VIEWS step's seconds go to, FR-073, in the order a view meets them. The
    /// words for each are ViewsSeconds.Describe and nothing else spells them. The parts of a
    /// per test view, the frame, the mark, the inventory, the removal and the tree read, came
    /// with F114's add-in pass, each timing one call of the design's sequence, probe P18.
    /// </summary>
    public enum ViewsPart
    {
        /// <summary>Walk one over every clash the report holds rows for, and the plan run over them.</summary>
        ReadingTheClashes = 0,

        /// <summary>The live line in the window, and the progress line in the log.</summary>
        SayingHowFar = 1,

        /// <summary>The hidden state read once, then the models the viewpoint hides hidden and the rest shown.</summary>
        ShowingAndHiding = 3,

        /// <summary>Everything but the clashing items made transparent, and the clashing items painted.</summary>
        Dimming = 4,

        /// <summary>A copy of the camera clash's viewpoint zoomed to the box over the view's clash centres, P16.</summary>
        Framing = 5,

        /// <summary>Every folder on the path found, or made where it is not there yet.</summary>
        MakingTheFolders = 6,

        /// <summary>The COM view made, named, its flags set and the clash camera put on it.</summary>
        MakingTheView = 7,

        /// <summary>The same folder found again, name by name, in the COM tree.</summary>
        FindingTheFolder = 8,

        /// <summary>The COM add, into the folder's own collection or at the root.</summary>
        AddingTheView = 9,

        /// <summary>The root route only: the copy into the folder and the root one removed.</summary>
        MovingIntoTheFolder = 10,

        /// <summary>The mark written on the view and on each folder the tool made, P9.</summary>
        Marking = 11,

        /// <summary>The written viewpoint read back off the tree and counted.</summary>
        ReadingBack = 12,

        /// <summary>The walk of the whole tree before and after the views are written, and the inventory decided over it.</summary>
        TakingTheInventory = 13,

        /// <summary>The removals the inventory gave, each RemoveAt with the parent resolved fresh, P13 and P14.</summary>
        Removing = 14,

        /// <summary>The dimming taken off and the hidden state the document held put back.</summary>
        PuttingBack = 15,

        /// <summary>The fresh walk after the removals that the VIEWS TREE block and its checks read.</summary>
        ReadingTheTree = 16
    }
}
