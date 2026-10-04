using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using Federator.Core.Exchange;
using NUnit.Framework;

namespace Federator.Core.Tests
{
    /// <summary>
    /// The workset list inside Core is exactly what was measured off the models and nothing
    /// typed beside it, so a spelling the matrix corrections act on is always one a model
    /// was seen to carry. It is read off the two measurements themselves, never a copy.
    /// </summary>
    [TestFixture]
    public class RevitWorksetsTests
    {
        /// <summary>The C06 run of set 03, whose EXPORT CHECK lines F116 added to the list.</summary>
        private static string C06Log()
        {
            return Path.Combine(Samples.Repo(), "steps", "runs", "03", "item1-C06", "run-20261001-140037.log");
        }

        private static IList<string> Lines(string path)
        {
            List<string> lines = new List<string>();

            using (StreamReader reader = new StreamReader(path, Encoding.UTF8))
            {
                string line;

                while ((line = reader.ReadLine()) != null)
                {
                    lines.Add(line);
                }
            }

            return lines;
        }

        /// <summary>
        /// Every name 5t read, each model's line saying how many names follow it, one a line,
        /// off the probe's own result file.
        /// </summary>
        private static IList<string> MeasuredOnC02()
        {
            List<string> names = new List<string>();
            Regex model = new Regex(@"^\d\d:\d\d:\d\d\.\d{3}\s+\S+\s+\S+\.nwc\s+site \[.*\]\s+(\d+) workset name\(s\)$");
            Regex name = new Regex(@"^\d\d:\d\d:\d\d\.\d{3}        (.+)$");
            int following = 0;

            foreach (string line in Lines(Samples.ProbeResult("5t-5u-result-20260920.txt")))
            {
                Match isModel = model.Match(line);

                if (isModel.Success)
                {
                    following = int.Parse(isModel.Groups[1].Value);
                    continue;
                }

                Match isName = following > 0 ? name.Match(line) : Match.Empty;

                if (isName.Success)
                {
                    following--;

                    if (!names.Contains(isName.Groups[1].Value))
                    {
                        names.Add(isName.Groups[1].Value);
                    }
                }
            }

            return names;
        }

        /// <summary>Every name the C06 run's "worksets seen" lines list, the count of the rest left off.</summary>
        private static IList<string> ListedOnC06()
        {
            List<string> names = new List<string>();
            Regex seen = new Regex(@"  worksets seen: (.*)$");
            Regex rest = new Regex(@", and \d+ more, counted and not listed$");

            foreach (string line in Lines(C06Log()))
            {
                Match match = seen.Match(line);

                if (!match.Success || match.Groups[1].Value.StartsWith("NONE", StringComparison.Ordinal))
                {
                    continue;
                }

                foreach (string one in rest.Replace(match.Groups[1].Value, string.Empty).Split(new[] { ", " }, StringSplitOptions.None))
                {
                    if (!names.Contains(one))
                    {
                        names.Add(one);
                    }
                }
            }

            return names;
        }

        /// <summary>
        /// The list is the 39 names 5t measured on C02 and the 30 more the C06 run listed,
        /// F116, each once. The header has said a test proves it since 5t and none did.
        /// </summary>
        [Test]
        public void TheListIsExactlyTheNamesMeasuredOnC02AndListedOnC06()
        {
            IList<string> list = RevitWorksets.All();
            List<string> measured = new List<string>(MeasuredOnC02());

            Assert.That(measured.Count, Is.EqualTo(39), "5t's own count, scan.md");

            foreach (string name in ListedOnC06())
            {
                if (!measured.Contains(name))
                {
                    measured.Add(name);
                }
            }

            Assert.That(list, Is.Unique);
            Assert.That(list, Is.EquivalentTo(measured));
            Assert.That(list.Count, Is.EqualTo(69));
        }

        /// <summary>
        /// The spellings Q102 was asked about, each measured in both forms, which is what
        /// lets a set ask both.
        /// </summary>
        [Test]
        public void BothSpellingsOfTheFourWorksetsTheBuildingsSpellTwoWaysAreMeasured()
        {
            IList<string> list = RevitWorksets.All();

            foreach (string name in new[]
            {
                "ME-Ductwork", "ME-DUCTWORK", "ME-Equipment", "ME-EQUIPMENT",
                "ME-Piping", "ME-PIPING", "PL-Domestic water", "PL-Domestic Water"
            })
            {
                Assert.That(list, Does.Contain(name));
            }
        }
    }
}
