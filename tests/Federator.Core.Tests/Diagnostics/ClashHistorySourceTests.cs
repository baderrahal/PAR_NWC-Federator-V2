using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using NUnit.Framework;

namespace Federator.Core.Tests
{
    /// <summary>
    /// F185, Bader's order of 14 Oct 2026, task 5. A clash test that holds results keeps them: the tool changes its
    /// tolerance only through TestsEditTestFromCopy on a copy of the test, adds a test the NWF lacks only through
    /// TestsAddCopy, and never resets, compacts, removes or replaces a clash test. The add-in cannot be built
    /// here, so these read its source, comment lines left out.
    /// </summary>
    [TestFixture]
    public class ClashHistorySourceTests
    {
        private static IEnumerable<KeyValuePair<string, string>> AddinCode()
        {
            string root = Path.Combine(Samples.Repo(), "src", "Federator.Addin");

            foreach (string path in Directory.GetFiles(root, "*.cs", SearchOption.AllDirectories))
            {
                if (path.Contains(Path.DirectorySeparatorChar + "obj" + Path.DirectorySeparatorChar)
                    || path.Contains(Path.DirectorySeparatorChar + "bin" + Path.DirectorySeparatorChar))
                {
                    continue;
                }

                List<string> code = new List<string>();

                foreach (string line in File.ReadAllLines(path))
                {
                    if (!line.TrimStart().StartsWith("//"))
                    {
                        code.Add(line);
                    }
                }

                yield return new KeyValuePair<string, string>(path, string.Join("\n", code.ToArray()));
            }
        }

        [Test]
        public void NothingInTheAddinResetsCompactsRemovesOrReplacesAClashTest()
        {
            Regex forbidden = new Regex(@"\.Tests(ReplaceWithCopy|Remove\w*|Compact\w*|Reset\w*|Delete\w*)\s*\(");
            List<string> found = new List<string>();

            foreach (KeyValuePair<string, string> file in AddinCode())
            {
                foreach (Match match in forbidden.Matches(file.Value))
                {
                    found.Add(Path.GetFileName(file.Key) + ": " + match.Value);
                }
            }

            Assert.That(found, Is.Empty, "a call that loses clash history on a test that has results");
        }

        [Test]
        public void TheToleranceIsSetOnACopyOfTheTestAndThroughEditFromCopyOnly()
        {
            string runner = null;

            foreach (KeyValuePair<string, string> file in AddinCode())
            {
                if (Path.GetFileName(file.Key) == "ClashRunner.cs")
                {
                    runner = file.Value;
                }
            }

            Assert.That(runner, Is.Not.Null);

            int start = runner.IndexOf("private void ApplyChosenTolerance(", System.StringComparison.Ordinal);
            int end = runner.IndexOf("private void WriteTheToleranceLine()", System.StringComparison.Ordinal);
            Assert.That(start, Is.GreaterThan(0));
            Assert.That(end, Is.GreaterThan(start));

            string body = runner.Substring(start, end - start);

            Assert.That(body, Does.Contain("existing.CreateCopy()"), "the copy is the test's own");
            Assert.That(body, Does.Contain("copy.Tolerance = wanted;"));
            Assert.That(body, Does.Contain("clashTests.TestsEditTestFromCopy(existing, copy);"));
            Assert.That(body, Does.Not.Contain("TestsAddCopy"));
            Assert.That(body, Does.Not.Contain("new ClashTest()"), "no new test stands in for one that has results");
        }

        [Test]
        public void ATestTheDocumentLacksIsAddedByAddCopyAndNowhereElse()
        {
            int files = 0;

            foreach (KeyValuePair<string, string> file in AddinCode())
            {
                if (file.Value.Contains("TestsAddCopy("))
                {
                    files++;
                    Assert.That(Path.GetFileName(file.Key), Is.EqualTo("ClashRunner.cs"));
                }
            }

            Assert.That(files, Is.EqualTo(1));
        }
    }
}
