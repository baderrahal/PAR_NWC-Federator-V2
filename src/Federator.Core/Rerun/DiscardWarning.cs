using System.Text;

namespace Federator.Core.Rerun
{
    /// <summary>
    /// The words that name what clearing the open document would throw away, FR-169. They
    /// reach the confirm before a run, the confirm before a file is opened and the PREVIEW
    /// line. A name, no name and a name that could not be read are three different things:
    /// a document whose name would not read was called the current unsaved document, which
    /// it may well not be, and the throw went nowhere. A read that threw is said with the
    /// reason it gave, and so is a count of models that would not read, which was left out
    /// as though the document held none.
    /// </summary>
    public static class DiscardWarning
    {
        /// <summary>
        /// The text. A reason that is not null means that read threw, and its value is then
        /// not read. A file name that is null or empty with no reason is a document never
        /// saved.
        /// </summary>
        public static string For(string fileName, string whyNameUnread, int models, string whyModelsUnread)
        {
            StringBuilder what = new StringBuilder();

            if (whyNameUnread != null)
            {
                what.Append("The open document, whose file name could not be read (")
                    .Append(Reason(whyNameUnread)).Append(')');
            }
            else if (!string.IsNullOrEmpty(fileName))
            {
                what.Append("The open file ").Append(fileName);
            }
            else
            {
                what.Append("The current unsaved document");
            }

            if (whyModelsUnread != null)
            {
                what.Append(", and how many models it holds could not be read (")
                    .Append(Reason(whyModelsUnread)).Append(')');
            }
            else if (models > 0)
            {
                what.Append(", holding ").Append(models).Append(models == 1 ? " model" : " models");
            }

            what.Append('.');
            return what.ToString();
        }

        private static string Reason(string why)
        {
            return why.Trim().Length == 0 ? "no reason given" : why.Trim();
        }
    }
}
