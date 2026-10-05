namespace Federator.Core.Views
{
    /// <summary>
    /// One model of the group as the views see it, F114: its file name, the discipline code
    /// part 5 of that name carries, read by ContainerName.Parse, and the team map's team of that
    /// code. An empty code is a name that would not read, and its team is UNKNOWN. A name handed
    /// to the views is tied to a model by ModelNames, F114 attempt 5.
    /// </summary>
    public sealed class ModelTeam
    {
        public ModelTeam(string fileName, string code, string team)
        {
            FileName = fileName ?? string.Empty;
            Code = code ?? string.Empty;
            Team = team ?? string.Empty;
        }

        /// <summary>The model's file name as the document holds it, a path or a bare name.</summary>
        public string FileName { get; private set; }

        /// <summary>Its discipline code, or empty where its name would not read.</summary>
        public string Code { get; private set; }

        /// <summary>The team map's team of that code.</summary>
        public string Team { get; private set; }
    }
}
