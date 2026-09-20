using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using NUnit.Framework;

namespace Federator.Core.Tests
{
    /// <summary>
    /// Paths for tests, written the way the machine running them writes a path.
    ///
    /// The tests used to type C:\out\reports into both the input and the expectation. Off
    /// Windows a backslash is an ordinary character, so Path.Combine joined with a forward
    /// slash and Path.GetDirectoryName found no folder at all, and 37 tests failed in the
    /// container on the separator alone while all of them passed on the Windows runner.
    /// The rule under test is the same on both, so the path is BUILT here rather than typed.
    /// </summary>
    public static class TestPaths
    {
        /// <summary>
        /// A rooted folder or file path in this machine's own spelling. C:\a\b on Windows
        /// and /a/b anywhere else.
        /// </summary>
        public static string At(params string[] parts)
        {
            List<string> all = new List<string>();
            all.Add(Root);

            if (parts != null)
            {
                all.AddRange(parts);
            }

            return Path.Combine(all.ToArray());
        }

        /// <summary>The root of the only drive these tests name. Never typed anywhere else.</summary>
        public static string Root
        {
            get
            {
                return Path.DirectorySeparatorChar == '\\'
                    ? "C:" + Path.DirectorySeparatorChar
                    : Path.DirectorySeparatorChar.ToString();
            }
        }

        /// <summary>
        /// Skips a test that is about a Windows file system rule rather than about a rule
        /// of this tool. Case blind paths and share locking are two of them, and neither
        /// exists off Windows, so the test says what it needs and skips rather than failing
        /// and being read as a fault in Core.
        /// </summary>
        public static void OnWindowsOnly(string whatItNeeds)
        {
            if (Path.DirectorySeparatorChar == '\\')
            {
                return;
            }

            Assert.Ignore("This is about " + whatItNeeds
                + ", which only Windows does. It runs on the Windows runner.");
        }
    }

    /// <summary>
    /// A folder under the temp folder for one fixture, removed afterwards.
    ///
    /// The same three lines and the same sentence sat above the same catch block in
    /// nineteen test files. One rule lives in one place, so it lives here.
    /// </summary>
    public static class TempFolder
    {
        /// <summary>A folder of its own, created, named after whatever asked for it.</summary>
        public static string Make(string name)
        {
            string folder = Path.Combine(
                Path.GetTempPath(),
                (string.IsNullOrEmpty(name) ? "test" : name) + "-" + Guid.NewGuid().ToString("N"));

            Directory.CreateDirectory(folder);
            return folder;
        }

        /// <summary>
        /// Removes it and everything under it. A leftover temp folder is not worth failing
        /// a test over, so a folder still held open by something is left where it is.
        /// </summary>
        public static void Remove(string folder)
        {
            try
            {
                if (!string.IsNullOrEmpty(folder) && Directory.Exists(folder))
                {
                    Directory.Delete(folder, true);
                }
            }
            catch (IOException)
            {
            }
            catch (UnauthorizedAccessException)
            {
            }
        }
    }

    /// <summary>
    /// Finds the samples folder by walking up from the test assembly, so the tests read
    /// the real files rather than a copy that could drift.
    /// </summary>
    public static class Samples
    {
        /// <summary>
        /// The clash export. The name on disk carries spaces and brackets. The underscore
        /// spelling is accepted too, in case the file is ever renamed on the way in.
        /// </summary>
        public static readonly string[] AllInOneNames =
        {
            "1104-PAR_CLASH_AllInOne (2) (1).xml",
            "1104-PAR_CLASH_AllInOne__2___1_.xml"
        };

        public static readonly string[] BuildingNames =
        {
            "Search Set Building.xml",
            "Search_Set_Building.xml"
        };

        public static readonly string[] InfraNames =
        {
            "Search Set Infra.xml",
            "Search_Set_Infra.xml"
        };

        public static string AllInOne()
        {
            return Resolve(AllInOneNames);
        }

        /// <summary>
        /// The client's matrix as it stands today, at 25 mm, which is the SOURCE the
        /// corrections are applied to. It holds the same 61 sets and 1830 tests as the
        /// export above and differs in the tolerance and in the faults F87 corrects.
        /// </summary>
        public static readonly string[] MatrixNames =
        {
            "1104-PAR_CLASH_AllInOne_25mm.xml"
        };

        public static string Matrix()
        {
            return Resolve(MatrixNames);
        }

        /// <summary>
        /// The corrected matrix, which this tool WRITES rather than reads as evidence, so
        /// it lives beside samples rather than in it. samples holds what came from the
        /// project and exchange holds what this tool made from it.
        /// </summary>
        public const string CorrectedMatrixName = "1104-PAR_CLASH_AllInOne_25mm_FIXED.xml";

        /// <summary>
        /// The exchange folder, found off the CHECKOUT and never by walking up for a
        /// folder of that name.
        ///
        /// IT USED TO WALK AND THAT WAS A WINDOWS ONLY FAULT. The walk looked for a folder
        /// called "exchange" and returned the first one it found. There is a folder called
        /// "Exchange" under this test project, one per Core folder, and Directory.Exists
        /// is CASE BLIND on Windows and case sensitive off it. So the walk stopped at the
        /// test folder on the runner and at the checkout root in the container, and twelve
        /// tests passed here and failed there with a path nobody could explain.
        ///
        /// Repo() walks for the solution FILE by its exact name, which is one walk already
        /// written and cannot match anything else. This joins onto it.
        /// </summary>
        public static string ExchangeFolder()
        {
            string repo = Repo();

            if (repo == null)
            {
                throw new DirectoryNotFoundException(
                    "No checkout found above the test assembly, so there is no exchange folder.");
            }

            string folder = Path.Combine(repo, "exchange");

            if (!Directory.Exists(folder))
            {
                throw new DirectoryNotFoundException("No exchange folder at " + folder + ".");
            }

            return folder;
        }

        public static string CorrectedMatrix()
        {
            return Path.Combine(ExchangeFolder(), CorrectedMatrixName);
        }

        /// <summary>
        /// A result file the viewpoint probe wrote, kept beside the probe under tools, found
        /// off the checkout the same way the exchange folder is. The category list in
        /// Federator.Core is proved to be exactly one of these, 5i.
        /// </summary>
        public static string ProbeResult(string name)
        {
            string repo = Repo();

            if (repo == null)
            {
                throw new DirectoryNotFoundException(
                    "No checkout found above the test assembly, so there is no probe folder.");
            }

            return Path.Combine(repo, "tools", "probes", "ViewpointProbe", name);
        }

        /// <summary>The clash priority file, F83, one priority per test off the matrix.</summary>
        public static readonly string[] PriorityMapNames = { "clash-priority-map.csv" };

        public static string PriorityMap()
        {
            return Resolve(PriorityMapNames);
        }

        /// <summary>The by design pairs, F72b, one pair of set names and a reason each.</summary>
        public static readonly string[] ByDesignNames = { "by-design-pairs.csv" };

        public static string ByDesign()
        {
            return Resolve(ByDesignNames);
        }

        public static string Building()
        {
            return Resolve(BuildingNames);
        }

        public static string Infra()
        {
            return Resolve(InfraNames);
        }

        /// <summary>
        /// The checkout itself, found the same way the samples folder is. The fixtures
        /// that read the client's exports and the stylesheet need the root rather than
        /// the samples folder, and this is the one place that walk is written.
        /// </summary>
        public static string Repo()
        {
            DirectoryInfo directory = new DirectoryInfo(
                Path.GetDirectoryName(new Uri(typeof(Samples).Assembly.CodeBase).LocalPath));

            while (directory != null)
            {
                if (File.Exists(Path.Combine(directory.FullName, "ParsonsNwcFederator.sln")))
                {
                    return directory.FullName;
                }

                directory = directory.Parent;
            }

            return null;
        }

        /// <summary>
        /// The samples folder, found off the CHECKOUT, for the same reason ExchangeFolder
        /// is. This one has never been caught by it, because the only thing named Samples
        /// under the test project is a FILE and Directory.Exists says no to a file. That
        /// is luck and not a rule, so it is joined onto the one walk as well.
        /// </summary>
        public static string Folder()
        {
            string repo = Repo();

            if (repo == null)
            {
                throw new DirectoryNotFoundException(
                    "No checkout found above the test assembly, so there is no samples folder.");
            }

            string folder = Path.Combine(repo, "samples");

            if (!Directory.Exists(folder))
            {
                throw new DirectoryNotFoundException("No samples folder at " + folder + ".");
            }

            return folder;
        }

        private static string Resolve(IEnumerable<string> candidates)
        {
            string folder = Folder();
            List<string> tried = new List<string>();

            foreach (string candidate in candidates)
            {
                string full = Path.Combine(folder, candidate);
                tried.Add(candidate);

                if (File.Exists(full))
                {
                    return full;
                }
            }

            throw new FileNotFoundException(
                "None of these sample files exist in " + folder + ": "
                    + string.Join(", ", tried.ToArray()));
        }
    }
}
