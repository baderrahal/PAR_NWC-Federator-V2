using System;
using System.Collections.Generic;
using System.IO;
using Federator.Core.Exchange;
using NUnit.Framework;

namespace Federator.Core.Tests
{
    /// <summary>
    /// TWO FILES CARRY THE NAME 1104-PAR_CLASH_AllInOne_25mm_FIXED.xml AND THEY ARE NOT
    /// THE SAME FILE. One is in samples, uploaded to main on 2026-09-19 while this round
    /// was being worked. One is in exchange, written by F87 from the sample.
    ///
    /// Neither is deleted and neither is edited. samples is evidence and a hook refuses
    /// the edit, and exchange is proved by its own test to be exactly what the rule
    /// produces. So the DIFFERENCE is pinned here instead, because two files of one name
    /// quietly disagreeing is how the wrong one gets picked in the tool a month from now.
    ///
    /// WHAT THE DIFFERENCE IS, measured. Both do F87's first job, the 121 occurrences of
    /// the missing hyphen in BLD-DRPipe Accessories. Only the exchange one does the
    /// second, which is BLD-EL-Devices asking for something other than Electrical
    /// Fixtures. Which of the two the tool should be pointed at is Q52.
    /// </summary>
    [TestFixture]
    public class SuppliedCorrectedMatrixTests
    {
        [Test]
        public void BothFilesExistAndBothCarryTheSameName()
        {
            Assert.That(File.Exists(Samples.SuppliedCorrectedMatrix()), Is.True);
            Assert.That(File.Exists(Samples.CorrectedMatrix()), Is.True);
            Assert.That(Path.GetFileName(Samples.SuppliedCorrectedMatrix()),
                Is.EqualTo(Path.GetFileName(Samples.CorrectedMatrix())));
            Assert.That(Path.GetDirectoryName(Samples.SuppliedCorrectedMatrix()),
                Is.Not.EqualTo(Path.GetDirectoryName(Samples.CorrectedMatrix())));
        }

        /// <summary>
        /// The break. Exactly ONE line differs, and it is named, so a change to either
        /// file that makes them differ anywhere else fails this rather than passing
        /// unnoticed.
        /// </summary>
        [Test]
        public void TheTwoDifferInExactlyOneLineAndItIsTheOneF87Rewrites()
        {
            string[] supplied = File.ReadAllLines(Samples.SuppliedCorrectedMatrix());
            string[] written = File.ReadAllLines(Samples.CorrectedMatrix());

            Assert.That(written.Length, Is.EqualTo(supplied.Length), "same number of lines");

            List<int> differ = new List<int>();

            for (int i = 0; i < supplied.Length; i++)
            {
                if (!string.Equals(supplied[i], written[i], StringComparison.Ordinal))
                {
                    differ.Add(i);
                }
            }

            Assert.That(differ.Count, Is.EqualTo(1),
                "the two corrected matrices differ in more than the one known line");
            Assert.That(supplied[differ[0]].Trim(),
                Is.EqualTo("<data type=\"wstring\">Electrical Fixtures</data>"));
            Assert.That(written[differ[0]].Trim(),
                Is.EqualTo("<data type=\"wstring\">Nurse Call Devices</data>"));
        }

        /// <summary>
        /// Both do the FIRST job. The hyphen is corrected in both, so neither carries the
        /// name that breaks its folder's pattern and neither leaves 60 test names
        /// unmatched by the priority file.
        /// </summary>
        [Test]
        public void BothCorrectTheMissingHyphen()
        {
            foreach (string path in new[] { Samples.SuppliedCorrectedMatrix(), Samples.CorrectedMatrix() })
            {
                string text = File.ReadAllText(path);

                Assert.That(text, Does.Not.Contain("BLD-DRPipe Accessories"), path);
                Assert.That(Occurrences(text, "BLD-DR-Pipe Accessories"), Is.EqualTo(121), path);
            }
        }

        /// <summary>
        /// Only the written one does the SECOND job. The supplied file still has
        /// BLD-EL-Devices asking for Electrical Fixtures, so it is still the same set as
        /// BLD-EL-Electrical Fixtures and F84 still reports the pair.
        /// </summary>
        [Test]
        public void OnlyTheWrittenOneMakesDevicesADifferentSetFromElectricalFixtures()
        {
            ExchangeDocument supplied = new ExchangeReader().ReadFile(Samples.SuppliedCorrectedMatrix());
            ExchangeDocument written = new ExchangeReader().ReadFile(Samples.CorrectedMatrix());

            Assert.That(Federator.Core.Health.SetWarnings.FindIdentical(supplied.Sets).Count,
                Is.EqualTo(2), "Devices is still Electrical Fixtures in the supplied file");
            Assert.That(Federator.Core.Health.SetWarnings.FindIdentical(written.Sets).Count,
                Is.EqualTo(1), "and it is not in the one F87 wrote");
        }

        [Test]
        public void NeitherIsTheUncorrectedSample()
        {
            string uncorrected = File.ReadAllText(Samples.Matrix());

            Assert.That(uncorrected, Does.Contain("BLD-DRPipe Accessories"),
                "the sample F87 reads is still the broken one, which is what makes it the source");
        }

        private static int Occurrences(string text, string what)
        {
            int count = 0;
            int at = text.IndexOf(what, StringComparison.Ordinal);

            while (at >= 0)
            {
                count++;
                at = text.IndexOf(what, at + 1, StringComparison.Ordinal);
            }

            return count;
        }
    }
}
