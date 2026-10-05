using Federator.Core.Clash;

namespace Federator.Core.Views
{
    /// <summary>
    /// One clash as the per test view plan is handed it, F114, Q114 points 13 and 14. The
    /// add-in reads each of these off the clash result after the clash step, and no Navisworks
    /// type reaches Core.
    /// </summary>
    public sealed class ViewClash
    {
        public ViewClash(
            string testName,
            string clashName,
            string leftSet,
            string rightSet,
            ClashStatus status,
            ClashPriority priority,
            SizeVerdict? serviceSize,
            ItemPath firstItem,
            ItemPath secondItem,
            Point3 centre,
            string firstHome,
            string secondHome)
        {
            TestName = testName ?? string.Empty;
            ClashName = clashName ?? string.Empty;
            LeftSet = leftSet ?? string.Empty;
            RightSet = rightSet ?? string.Empty;
            Status = status;
            Priority = priority;
            ServiceSize = serviceSize;
            FirstItem = firstItem;
            SecondItem = secondItem;
            Centre = centre;
            FirstHome = firstHome ?? string.Empty;
            SecondHome = secondHome ?? string.Empty;
        }

        /// <summary>The clash test's name, exactly as Navisworks holds it, never trimmed.</summary>
        public string TestName { get; private set; }

        /// <summary>The clash's own name, unique within its test, such as Clash1.</summary>
        public string ClashName { get; private set; }

        /// <summary>The set name of the test's left side, the text after the last slash of its locator.</summary>
        public string LeftSet { get; private set; }

        /// <summary>The set name of the test's right side.</summary>
        public string RightSet { get; private set; }

        public ClashStatus Status { get; private set; }

        /// <summary>The test's priority off the picked priority file, or None.</summary>
        public ClashPriority Priority { get; private set; }

        /// <summary>
        /// The size verdict of the larger service side, read the way F72a reads a side, its
        /// larger dimension, or null where no side is a service. Read only in a pair that
        /// carries the size folder, and null elsewhere.
        /// </summary>
        public SizeVerdict? ServiceSize { get; private set; }

        /// <summary>The first clashing item, painted red, or null where it could not be pointed at.</summary>
        public ItemPath FirstItem { get; private set; }

        /// <summary>The second clashing item, painted green, or null where it could not be pointed at.</summary>
        public ItemPath SecondItem { get; private set; }

        /// <summary>The clash centre in document units, or null where it could not be read.</summary>
        public Point3 Centre { get; private set; }

        /// <summary>The name of the model the first item lives in, a path or a file name, matched to a model by ModelTeam.IsAmong, or empty where it could not be read.</summary>
        public string FirstHome { get; private set; }

        /// <summary>The file name of the model the second item lives in, or empty.</summary>
        public string SecondHome { get; private set; }

        /// <summary>The test and the clash, the one key a clash is known by across views.</summary>
        public string Key
        {
            get { return TestName + "\n" + ClashName; }
        }
    }
}
