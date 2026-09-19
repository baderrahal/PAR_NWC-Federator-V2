using System;
using Autodesk.Navisworks.Api;
using Autodesk.Navisworks.Api.DocumentParts;

namespace Broken
{
    // Correct, and the other half of the map. The same type as a LOCAL rather than a
    // parameter, so the two shapes the real tree uses are both pinned. A type only one
    // other file names teaches the check nothing, which is why there are two of these and
    // not one.
    public static class HasImportAsALocal
    {
        public static bool FromTheDocument(Document document)
        {
            DocumentSelectionSets sets = document.SelectionSets;

            return sets != null;
        }
    }
}
