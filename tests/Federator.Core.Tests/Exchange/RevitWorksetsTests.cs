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
    /// The workset list inside Core, and the workset lines of this project's list of
    /// corrections beside the picked XML, are exactly what was measured off the models and
    /// nothing typed beside them, so a spelling the matrix corrections act on is always one a
    /// model was seen to carry. Each is read off its measurement itself, never a copy.
    /// </summary>
    [TestFixture]
    public class RevitWorksetsTests
    {
        /// <summary>The C06 run of set 03, whose EXPORT CHECK lines this project's list carries the names of, F116.</summary>
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

        /// <summary>This project's list of corrections, read the way the tool reads it beside a picked file, Q113.</summary>
        private static MatrixCorrectionList TheProjectsList()
        {
            return MatrixCorrectionList.Beside(Samples.CorrectedMatrix(), new CorrectionListSettings());
        }

        /// <summary>
        /// The list inside Core is the 39 names 5t measured on C02, each once, and nothing
        /// more. The 30 the C06 run listed are this project's and sit in its list of
        /// corrections beside the picked XML, Q113 answered B on 2026-10-04. The header has said
        /// a test proves it since 5t and none did until F116.
        /// </summary>
        [Test]
        public void TheListInsideCoreIsExactlyTheNamesMeasuredOnC02()
        {
            IList<string> list = RevitWorksets.With(null);
            IList<string> measured = MeasuredOnC02();

            Assert.That(measured.Count, Is.EqualTo(39), "5t's own count, scan.md");
            Assert.That(list, Is.Unique);
            Assert.That(list, Is.EquivalentTo(measured));
        }

        /// <summary>
        /// The workset lines of this project's list are exactly the names the C06 run listed
        /// that the C02 census does not hold, 30 of them, each once, read off the run log itself.
        /// </summary>
        [Test]
        public void TheProjectsListHoldsExactlyTheNamesTheC06RunListedThatC02DidNot()
        {
            IList<string> c02 = MeasuredOnC02();
            List<string> onlyOnC06 = new List<string>();

            foreach (string name in ListedOnC06())
            {
                if (!c02.Contains(name))
                {
                    onlyOnC06.Add(name);
                }
            }

            IList<string> worksets = TheProjectsList().Worksets;

            Assert.That(worksets, Is.Unique);
            Assert.That(worksets, Is.EquivalentTo(onlyOnC06));
            Assert.That(worksets.Count, Is.EqualTo(30));
        }

        /// <summary>A stream whose every read throws, the list that is in the DLL and will not read.</summary>
        private sealed class ThrowingStream : MemoryStream
        {
            public override int Read(byte[] buffer, int offset, int count)
            {
                throw new IOException("this stream will not read");
            }
        }

        /// <summary>
        /// A LIST NOT IN THE DLL IS NOT AN EMPTY LIST, FR-012. It read the same as one, with no
        /// flag and no line, so the export check could name the two pairs a person already
        /// decided are not typos and the empty set judge could not tell, with no reason. The
        /// seam reads a list from a given stream and says whether it was read.
        /// </summary>
        [Test]
        public void ANullStreamIsAListNotFoundAndNeverAnEmptyOne()
        {
            List<string> names;
            List<string[]> pairs;
            string project;

            Assert.That(RevitWorksets.Read(() => null, out names, out pairs, out project), Is.False);
            Assert.That(names, Is.Empty);
            Assert.That(pairs, Is.Empty);
        }

        /// <summary>
        /// A LIST THAT WILL NOT EVEN OPEN IS A LIST NOT READ AND NEVER A THROW, the reviewer's and
        /// the breaker's finding on attempt 1. The resource was opened outside the reader's try,
        /// so a FileLoadException or a BadImageFormatException from that call would leave every
        /// caller, the plan of the sets and the EXPORT CHECK among them, while the comment said
        /// never a throw. The opening is the reader's own, inside its try.
        /// </summary>
        [Test]
        public void AListThatWillNotOpenIsAListNotReadAndNeverAThrow()
        {
            List<string> names = null;
            List<string[]> pairs = null;
            string project = null;
            bool read = true;

            Assert.DoesNotThrow(() => read = RevitWorksets.Read(
                () => { throw new FileLoadException("the resource will not load"); }, out names, out pairs, out project));
            Assert.That(read, Is.False);
            Assert.That(names, Is.Empty);
            Assert.That(pairs, Is.Empty);
        }

        /// <summary>A stream that throws is a list not read, said and never thrown, FR-012.</summary>
        [Test]
        public void AStreamThatThrowsIsAListNotRead()
        {
            List<string> names;
            List<string[]> pairs;
            string project;

            using (ThrowingStream stream = new ThrowingStream())
            {
                Assert.That(RevitWorksets.Read(() => stream, out names, out pairs, out project), Is.False);
            }

            Assert.That(names, Is.Empty);
            Assert.That(pairs, Is.Empty);
        }

        /// <summary>The list in the DLL is found and read, the names and the two decided pairs, FR-012.</summary>
        [Test]
        public void TheListInTheDllIsFoundAndRead()
        {
            Assert.That(RevitWorksets.ResourceFound, Is.True,
                "the embedded resource " + RevitWorksets.ResourceName + " is not in Federator.Core.dll");

            List<string> names;
            List<string[]> pairs;
            string project;

            using (Stream stream = typeof(RevitWorksets).Assembly.GetManifestResourceStream(RevitWorksets.ResourceName))
            {
                Assert.That(RevitWorksets.Read(() => stream, out names, out pairs, out project), Is.True);
            }

            Assert.That(names, Is.EquivalentTo(RevitWorksets.With(null)));
            Assert.That(pairs.Count, Is.EqualTo(RevitWorksets.DecidedCount));
            Assert.That(project, Is.EqualTo(RevitWorksets.Project));
        }

        /// <summary>
        /// The spellings Q102 was asked about, each measured in both forms between the list
        /// inside Core and this project's list, which is what lets a set ask both.
        /// </summary>
        [Test]
        public void BothSpellingsOfTheFourWorksetsTheBuildingsSpellTwoWaysAreMeasured()
        {
            List<string> list = new List<string>(RevitWorksets.With(null));
            list.AddRange(TheProjectsList().Worksets);

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
