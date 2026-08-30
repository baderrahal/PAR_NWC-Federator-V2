using System.Collections.Generic;
using Federator.Core.Rerun;
using NUnit.Framework;

namespace Federator.Core.Tests
{
    /// <summary>
    /// A real run reported CHANGED for 22 of 22 groups where nothing had changed, so no
    /// NWF was reused, no set was built and no test ran. The comparison was using
    /// Model.SourceFileName, which holds the Revit container in Autodesk Docs, against the
    /// scanned NWC path, which it can never equal.
    ///
    /// The shapes in here are the real ones off that log.
    /// </summary>
    [TestFixture]
    public class ModelFileNameCompareTests
    {
        private const string Nwc = @"C:\in\1104-PAR-1B06BC-ZZZ-AR-MOD-000001.nwc";

        private const string RevitSource =
            "Autodesk Docs://KSA_New Murabba/1104-PAR-0000BC-ZZZ-AR-MOD-003000.rvt";

        // ---------- which of the two names identifies the file ----------

        [Test]
        public void TheFileNameIsTheOneThatIdentifiesTheFile()
        {
            Assert.That(ModelFileNames.PathOf(Nwc, RevitSource), Is.EqualTo(Nwc),
                "the Revit source was used, and it can never equal a scanned NWC path");
        }

        [Test]
        public void TheSourceNameIsUsedOnlyWhenTheFileNameIsEmpty()
        {
            Assert.That(ModelFileNames.PathOf(null, RevitSource), Is.EqualTo(RevitSource));
            Assert.That(ModelFileNames.PathOf(string.Empty, RevitSource), Is.EqualTo(RevitSource));
        }

        [Test]
        public void AModelReportingNeitherNameGivesNothingRatherThanThrowing()
        {
            Assert.That(ModelFileNames.PathOf(null, null), Is.Null);
            Assert.That(ModelFileNames.PathOf(string.Empty, null), Is.Null);
        }

        [Test]
        public void TheTwoNamesDisagreeingIsWhatPutsBothInTheLog()
        {
            Assert.That(ModelFileNames.Disagree(Nwc, RevitSource), Is.True);
            Assert.That(ModelFileNames.Disagree(Nwc, Nwc), Is.False);
            Assert.That(ModelFileNames.Disagree(Nwc, Nwc.ToUpperInvariant()), Is.False,
                "Windows does not care about case, so this must not report a disagreement");
            Assert.That(ModelFileNames.Disagree(null, null), Is.False);
            Assert.That(ModelFileNames.Disagree(Nwc, null), Is.True);
        }

        // ---------- the comparison itself, on the real shapes ----------

        // This is the one the brief asked for by name.
        [Test]
        public void ASourceNameDifferingFromTheFileNameStillComparesAsUnchanged()
        {
            List<string> scanned = new List<string> { Nwc };

            // What the opened NWF reports for that one model.
            List<string> insideTheNwf = new List<string>
            {
                ModelFileNames.PathOf(Nwc, RevitSource)
            };

            NwfComparison comparison = NwfComparison.Compare(insideTheNwf, scanned);

            Assert.That(comparison.Decision, Is.EqualTo(RerunDecision.Open),
                "an unchanged group reported CHANGED, which is the whole fault");
            Assert.That(comparison.Added.Count, Is.EqualTo(0));
            Assert.That(comparison.Removed.Count, Is.EqualTo(0));
            Assert.That(comparison.Unchanged.Count, Is.EqualTo(1));
        }

        // What the run actually did before the fix, kept so the fault cannot come back
        // without a test saying so.
        [Test]
        public void ComparingOnTheSourceNameIsWhatProducedChangedForEveryGroup()
        {
            NwfComparison comparison = NwfComparison.Compare(
                new List<string> { RevitSource }, new List<string> { Nwc });

            Assert.That(comparison.Decision, Is.EqualTo(RerunDecision.Changed));
            Assert.That(comparison.Added.Count, Is.EqualTo(1));
            Assert.That(comparison.Removed.Count, Is.EqualTo(1));
        }

        // A whole group, four disciplines, every one of them published from a Revit
        // container under a different building code.
        [Test]
        public void AWholeGroupOfDifferingSourceNamesStillComparesAsUnchanged()
        {
            string[] nwcs =
            {
                @"C:\in\1104-PAR-1B06BC-ZZZ-AR-MOD-000001.nwc",
                @"C:\in\1104-PAR-1B06BC-ZZZ-ST-MOD-000001.nwc",
                @"C:\in\1104-PAR-1B06BC-ZZZ-ME-MOD-000001.nwc",
                @"C:\in\1104-PAR-1B06BC-ZZZ-EL-MOD-000001.nwc"
            };

            string[] sources =
            {
                "Autodesk Docs://KSA_New Murabba/1104-PAR-0000BC-ZZZ-AR-MOD-003000.rvt",
                "Autodesk Docs://KSA_New Murabba/1104-PAR-0000BC-ZZZ-ST-MOD-003000.rvt",
                "Autodesk Docs://KSA_New Murabba/1104-PAR-0000BC-ZZZ-ME-MOD-003000.rvt",
                "Autodesk Docs://KSA_New Murabba/1104-PAR-0000BC-ZZZ-EL-MOD-003000.rvt"
            };

            List<string> insideTheNwf = new List<string>();

            for (int i = 0; i < nwcs.Length; i++)
            {
                insideTheNwf.Add(ModelFileNames.PathOf(nwcs[i], sources[i]));
            }

            NwfComparison comparison = NwfComparison.Compare(insideTheNwf, new List<string>(nwcs));

            Assert.That(comparison.Decision, Is.EqualTo(RerunDecision.Open));
            Assert.That(comparison.Unchanged.Count, Is.EqualTo(4));
        }

        // The fix must not hide a real change. A file genuinely added still reports it.
        [Test]
        public void AGenuineChangeIsStillReportedAsChanged()
        {
            NwfComparison comparison = NwfComparison.Compare(
                new List<string> { ModelFileNames.PathOf(Nwc, RevitSource) },
                new List<string> { Nwc, @"C:\in\1104-PAR-1B06BC-ZZZ-EL-MOD-000001.nwc" });

            Assert.That(comparison.Decision, Is.EqualTo(RerunDecision.Changed));
            Assert.That(comparison.Added.Count, Is.EqualTo(1));
            Assert.That(comparison.Removed.Count, Is.EqualTo(0));
        }

        // A model that reports no FileName at all falls back to the source, which will not
        // match the scan. That is correct: it is genuinely a file the comparison cannot
        // identify, and reporting CHANGED leaves the decision with Bader rather than
        // reusing an NWF nobody has checked.
        [Test]
        public void AModelWithNoFileNameFallsBackAndIsReportedRatherThanAssumed()
        {
            NwfComparison comparison = NwfComparison.Compare(
                new List<string> { ModelFileNames.PathOf(null, RevitSource) },
                new List<string> { Nwc });

            Assert.That(comparison.Decision, Is.EqualTo(RerunDecision.Changed));
        }
    }
}
