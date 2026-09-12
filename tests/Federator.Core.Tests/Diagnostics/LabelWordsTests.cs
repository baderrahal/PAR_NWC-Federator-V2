using System.Collections.Generic;
using Federator.Core.Clash;
using Federator.Core.Diagnostics;
using NUnit.Framework;

namespace Federator.Core.Tests
{
    /// <summary>
    /// No code identifier and no framework message ever reaches a label. Each of these
    /// hands the wording the very thing a label must not carry and asserts it is not in
    /// what comes back, because a test that reads only the good case proves nothing.
    ///
    /// The words live in Core for this reason, the way ReportPaths.WhereTheyGo does.
    /// </summary>
    [TestFixture]
    public class LabelWordsTests
    {
        private const string TypeAndMessage =
            "UnauthorizedAccessException: Access to the path 'C:\\Logs\\run.log' is denied.";

        [Test]
        public void TheLogReasonNamesEveryFolderThatWasTried()
        {
            string said = RunLog.NoLogFileOpened(
                new List<string> { "C:\\Users\\b\\logs", "C:\\Temp\\" });

            Assert.That(said, Does.Contain("C:\\Users\\b\\logs"));
            Assert.That(said, Does.Contain("C:\\Temp\\"));
        }

        [Test]
        public void TheLogReasonCarriesNoTypeNameAndNoMessage()
        {
            string said = RunLog.NoLogFileOpened(new List<string> { "C:\\Temp\\" });

            Assert.That(said, Does.Not.Contain("Exception"));
            Assert.That(said, Does.Not.Contain("Access to the path"));
        }

        [Test]
        public void NoFolderTriedStillSaysSomething()
        {
            Assert.That(RunLog.NoLogFileOpened(null), Is.Not.Empty);
            Assert.That(RunLog.NoLogFileOpened(new List<string>()), Is.Not.Empty);
        }

        [Test]
        public void TheWindowLineNamesThePathWhileTheLogIsOnDisk()
        {
            Assert.That(
                RunLog.WhereTheLogIs(true, "C:\\Logs\\run.log", null),
                Is.EqualTo("Log: C:\\Logs\\run.log"));
        }

        [Test]
        public void TheWindowLineWarnsAndCarriesTheReasonWhenItIsNot()
        {
            string said = RunLog.WhereTheLogIs(
                false, null, RunLog.NoLogFileOpened(new List<string> { "C:\\Temp\\" }));

            Assert.That(said, Does.StartWith("WARNING"));
            Assert.That(said, Does.Contain("C:\\Temp\\"));
            Assert.That(said, Does.Not.Contain("Exception"));
        }

        [Test]
        public void TheErrorCountSaysHowManyAndNeverWhatTheySay()
        {
            Assert.That(RunLog.ErrorsAreInTheLog(0), Is.Empty);
            Assert.That(RunLog.ErrorsAreInTheLog(1), Does.Contain("1 error"));
            Assert.That(RunLog.ErrorsAreInTheLog(1), Does.Not.Contain("errors"));
            Assert.That(RunLog.ErrorsAreInTheLog(3), Does.Contain("3 errors"));
            Assert.That(RunLog.ErrorsAreInTheLog(3), Does.Contain("the log says"));
        }

        [Test]
        public void AFailedReadPointsAtTheLogAndNotAtTheException()
        {
            Assert.That(RunLog.TheLogSaysWhy(), Does.Contain("log"));
            Assert.That(RunLog.TheLogSaysWhy(), Does.Not.Contain("Exception"));
        }

        [Test]
        public void TheGuardSaysNothingUntilItHasFired()
        {
            RepeatedFailureGuard guard = new RepeatedFailureGuard(3);
            guard.RecordFailure(TypeAndMessage);

            Assert.That(guard.ReasonInPlainWords, Is.Null);
            Assert.That(guard.Reason, Is.Null);
        }

        [Test]
        public void TheGuardsLabelCountsTheTestsAndCarriesNoneOfWhatWasThrown()
        {
            RepeatedFailureGuard guard = new RepeatedFailureGuard(3);
            guard.RecordFailure(TypeAndMessage);
            guard.RecordFailure(TypeAndMessage);
            guard.RecordFailure(TypeAndMessage);

            Assert.That(guard.ReasonInPlainWords, Does.Contain("first 3 tests"));
            Assert.That(guard.ReasonInPlainWords, Does.Not.Contain("Exception"));
            Assert.That(guard.ReasonInPlainWords, Does.Not.Contain("Access to the path"));
            Assert.That(guard.ReasonInPlainWords, Does.Contain("The log says"));
        }

        [Test]
        public void TheGuardsLogReasonStillCarriesWhatWasThrown()
        {
            RepeatedFailureGuard guard = new RepeatedFailureGuard(2);
            guard.RecordFailure(TypeAndMessage);
            guard.RecordFailure(TypeAndMessage);

            Assert.That(guard.Reason, Does.Contain(TypeAndMessage));
        }
    }
}
