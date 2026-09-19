using System;
using Autodesk.Navisworks.Api;
using Autodesk.Navisworks.Api.DocumentParts;

namespace Broken
{
    // Correct, and half the map. The same type in the same position as MissingImport.cs
    // puts it, with the import that covers it.
    public static class HasImportAsAParameter
    {
        public static bool Counted(DocumentSelectionSets sets)
        {
            return sets != null;
        }
    }
}
