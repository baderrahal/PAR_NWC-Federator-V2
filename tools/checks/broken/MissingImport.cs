namespace Broken
{
    // The shape the build failed on for real on 2026-09-19. DocumentSelectionSets is a
    // PARAMETER type, the file names no namespace that has it, and that is CS0246. The
    // two files beside this one use the same type and carry the import, which is how the
    // check learns where it comes from without an Autodesk DLL anywhere near it.
    public static class MissingImport
    {
        public static bool TheFault(DocumentSelectionSets sets)
        {
            return sets != null;
        }
    }
}
