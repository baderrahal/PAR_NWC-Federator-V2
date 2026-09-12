using System;
using System.Collections.Generic;
using System.Globalization;

namespace Federator.Core.Clash
{
    /// <summary>
    /// What one clash test is set to, with no Navisworks types in it, so the file and the
    /// document can be compared without Navisworks.
    /// </summary>
    public sealed class TestSettings
    {
        /// <summary>
        /// What a side reads when this tool could not read which set it points at. It is
        /// here rather than in the add-in because the comparison is in Core and has to
        /// leave such a side out, and a marker written in two places is a rule in two
        /// places.
        /// </summary>
        public const string UnknownLocator = "UNKNOWN";

        public TestSettings()
        {
            TestTypeName = string.Empty;
            LeftLocator = string.Empty;
            RightLocator = string.Empty;
        }

        public double Tolerance { get; set; }

        public ClashTestKind TestType { get; set; }

        /// <summary>The words, for the report. The kind is what is compared.</summary>
        public string TestTypeName { get; set; }

        public bool MergeComposites { get; set; }

        public string LeftLocator { get; set; }

        public string RightLocator { get; set; }

        public bool LeftSelfIntersect { get; set; }

        public bool RightSelfIntersect { get; set; }

        public int LeftPrimitiveTypes { get; set; }

        public int RightPrimitiveTypes { get; set; }

        /// <summary>What the file says this test should be, taken off the plan.</summary>
        public static TestSettings FromFile(PlannedClashTest planned)
        {
            if (planned == null)
            {
                throw new ArgumentNullException("planned");
            }

            return new TestSettings
            {
                Tolerance = planned.Tolerance,
                TestType = planned.TestType,
                TestTypeName = planned.TestTypeName,
                MergeComposites = planned.MergeComposites,
                LeftLocator = planned.Left.Locator,
                RightLocator = planned.Right.Locator,
                LeftSelfIntersect = planned.Left.SelfIntersect,
                RightSelfIntersect = planned.Right.SelfIntersect,
                LeftPrimitiveTypes = planned.Left.PrimitiveTypes,
                RightPrimitiveTypes = planned.Right.PrimitiveTypes
            };
        }
    }

    /// <summary>One thing the file and the document disagree about, for one test.</summary>
    public sealed class TestDifference
    {
        internal TestDifference(string testName, string field, string inFile, string inDocument)
        {
            TestName = testName;
            Field = field;
            InFile = inFile;
            InDocument = inDocument;
        }

        public string TestName { get; private set; }

        /// <summary>Tolerance, test type, merge composites, or a side.</summary>
        public string Field { get; private set; }

        public string InFile { get; private set; }

        public string InDocument { get; private set; }

        public string Sentence()
        {
            return TestName + ": the file says " + Field + " " + InFile
                + " and the test in the document has " + InDocument + ".";
        }

        public override string ToString()
        {
            return Sentence();
        }
    }

    /// <summary>
    /// What has drifted between the clash file and a test already in the document.
    ///
    /// A test already there is left exactly as it is, because that is where its Active and
    /// Resolved clashes live and rebuilding it would reset every one of them. So a
    /// tolerance changed in the XML never reaches it, and until now nothing said so. This
    /// says so. It changes nothing. Bader decides.
    /// </summary>
    public static class TestDrift
    {
        /// <summary>
        /// How close two tolerances have to be to count as the same. They travel through a
        /// unit conversion on the way in, so an exact comparison would report drift on
        /// every test in the file.
        /// </summary>
        public const double ToleranceEpsilon = 0.0000001;

        public static IList<TestDifference> Compare(
            string testName, TestSettings inFile, TestSettings inDocument)
        {
            if (inFile == null)
            {
                throw new ArgumentNullException("inFile");
            }

            if (inDocument == null)
            {
                throw new ArgumentNullException("inDocument");
            }

            List<TestDifference> found = new List<TestDifference>();

            if (Math.Abs(inFile.Tolerance - inDocument.Tolerance) > ToleranceEpsilon)
            {
                Add(found, testName, "the tolerance",
                    Number(inFile.Tolerance), Number(inDocument.Tolerance));
            }

            if (inFile.TestType != inDocument.TestType)
            {
                Add(found, testName, "the test type",
                    inFile.TestType.ToString(), inDocument.TestType.ToString());
            }

            if (inFile.MergeComposites != inDocument.MergeComposites)
            {
                Add(found, testName, "merge composites",
                    OnOff(inFile.MergeComposites), OnOff(inDocument.MergeComposites));
            }

            Side(found, testName, "the left side", inFile.LeftLocator, inDocument.LeftLocator,
                inFile.LeftSelfIntersect, inDocument.LeftSelfIntersect,
                inFile.LeftPrimitiveTypes, inDocument.LeftPrimitiveTypes);

            Side(found, testName, "the right side", inFile.RightLocator, inDocument.RightLocator,
                inFile.RightSelfIntersect, inDocument.RightSelfIntersect,
                inFile.RightPrimitiveTypes, inDocument.RightPrimitiveTypes);

            return found;
        }

        private static void Side(
            IList<TestDifference> found,
            string testName,
            string which,
            string fileLocator,
            string documentLocator,
            bool fileSelf,
            bool documentSelf,
            int filePrimitives,
            int documentPrimitives)
        {
            // A side this tool could not read is not compared at all. Comparing the word
            // UNKNOWN against a real locator reports drift when the truth is that nothing
            // was read, which is worse than saying so.
            if (!WasRead(fileLocator) || !WasRead(documentLocator))
            {
                return;
            }

            // Ordinal and never trimmed. The rule, and the two set names that are the
            // reason for it, are in .claude\rules\core.md.
            if (!string.Equals(fileLocator, documentLocator, StringComparison.Ordinal))
            {
                Add(found, testName, which + " set", QuotedOrNothing(fileLocator), QuotedOrNothing(documentLocator));
            }

            if (fileSelf != documentSelf)
            {
                Add(found, testName, which + " self intersect",
                    OnOff(fileSelf), OnOff(documentSelf));
            }

            if (filePrimitives != documentPrimitives)
            {
                Add(found, testName, which + " primitive types",
                    filePrimitives.ToString(CultureInfo.InvariantCulture),
                    documentPrimitives.ToString(CultureInfo.InvariantCulture));
            }
        }

        /// <summary>
        /// True when a locator was actually read. The marker is the whole reason this
        /// exists, because it is a real string and compares like any other.
        /// </summary>
        public static bool WasRead(string locator)
        {
            return !string.Equals(
                locator == null ? string.Empty : locator,
                TestSettings.UnknownLocator,
                StringComparison.Ordinal);
        }

        /// <summary>
        /// How many of this test's two sides were left out because one end of the pair
        /// could not be read. Counted so the DRIFT block says how many were not compared
        /// rather than letting them read as matching.
        /// </summary>
        public static int SidesNotCompared(TestSettings inFile, TestSettings inDocument)
        {
            if (inFile == null || inDocument == null)
            {
                return 0;
            }

            int left = WasRead(inFile.LeftLocator) && WasRead(inDocument.LeftLocator) ? 0 : 1;
            int right = WasRead(inFile.RightLocator) && WasRead(inDocument.RightLocator) ? 0 : 1;

            return left + right;
        }

        private static void Add(
            IList<TestDifference> found, string testName, string field, string inFile, string inDocument)
        {
            found.Add(new TestDifference(testName, field, inFile, inDocument));
        }

        private static string Number(double value)
        {
            return value.ToString("0.##########", CultureInfo.InvariantCulture);
        }

        private static string OnOff(bool value)
        {
            return value ? "on" : "off";
        }

        private static string QuotedOrNothing(string value)
        {
            return string.IsNullOrEmpty(value) ? "nothing" : "\"" + value + "\"";
        }

        /// <summary>
        /// The block this contributes to the log. Every difference by test name, and one
        /// line saying nothing drifted rather than an empty block.
        /// </summary>
        public static IList<string> Lines(
            IEnumerable<TestDifference> differences,
            int testsCompared,
            bool applying,
            int sidesNotCompared)
        {
            List<string> lines = new List<string>();
            List<TestDifference> all = new List<TestDifference>(differences ?? new TestDifference[0]);

            lines.Add(testsCompared
                + (testsCompared == 1 ? " test was" : " tests were")
                + " already in the document and were compared against the file.");

            if (sidesNotCompared > 0)
            {
                lines.Add(sidesNotCompared
                    + (sidesNotCompared == 1 ? " side was" : " sides were")
                    + " left out, because which set it points at could not be read. "
                    + "Not compared is not the same as matching.");
            }

            if (all.Count == 0)
            {
                lines.Add(sidesNotCompared > 0
                    ? "Nothing has drifted in what was compared."
                    : "Nothing has drifted. Every one of them matches the file.");
                return lines;
            }

            HashSet<string> names = new HashSet<string>(StringComparer.Ordinal);

            foreach (TestDifference difference in all)
            {
                names.Add(difference.TestName);
            }

            lines.Add(all.Count + (all.Count == 1 ? " difference across " : " differences across ")
                + names.Count + (names.Count == 1 ? " test." : " tests."));

            lines.Add(applying
                ? "APPLYING these to the tests in the document, which RESETS their results. "
                    + "Every Active and Resolved clash on the tests below goes back to New."
                : "Nothing was changed. Changing a test resets its results, so the decision is "
                    + "yours. Tick apply the file's settings on the Clash step to change them.");

            lines.Add(string.Empty);

            foreach (TestDifference difference in all)
            {
                lines.Add(difference.Sentence());
            }

            return lines;
        }
    }
}
