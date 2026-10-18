namespace NwcFederatorLoop.StandIn
{
    /// <summary>
    /// The GROUPS block the scanned run wrote before F232, kept word for word for the stand-in alone, since the harness reads
    /// the log the stand-in writes in that shape. F232 took the scanned run out of the tool, and its RunLog members with it.
    /// </summary>
    internal static class OldGroupLines
    {
        public const string Title = "GROUPS";

        public static string UntickedLine(int unticked)
        {
            return unticked + (unticked == 1 ? " group" : " groups")
                + " unticked in the Run column, nothing else drops a group";
        }
    }
}
