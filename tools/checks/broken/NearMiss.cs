using System;
using Autodesk.Navisworks.Api;

namespace Broken
{
    // The near miss, and it MUST PASS. Every DocumentSelectionSets in this file is a word
    // and not a type: one here in a comment and one inside a string. The property the code
    // actually reads is a member and not a type name at all.
    //
    // This is SavedViewpoints.cs in the real tree, which names DocumentSavedViewpoints four
    // times, every one of them in a comment recording what the API is assumed to look like,
    // and correctly carries no DocumentParts import. A check that matched on the word alone
    // would add an import that file does not need.
    public static class NearMiss
    {
        public static string Says(Document document)
        {
            Document held = document;

            return held == null
                ? "nothing"
                : "DocumentSelectionSets is what the SelectionSets property hands back";
        }
    }
}
