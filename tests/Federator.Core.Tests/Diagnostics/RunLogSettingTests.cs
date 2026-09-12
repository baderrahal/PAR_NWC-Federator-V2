using System;
using Federator.Core.Diagnostics;
using NUnit.Framework;

namespace Federator.Core.Tests
{
    /// <summary>
    /// How many log files are kept is a setting and not a constant. It is static because
    /// the log opens on the first line of the button handler, before a window or any
    /// options object exists.
    /// </summary>
    [TestFixture]
    public class RunLogSettingTests
    {
        [TearDown]
        public void PutTheSettingBack()
        {
            RunLog.KeepLogs = RunLog.DefaultKeepLogs;
        }

        [Test]
        public void ItStartsAtTheDefault()
        {
            Assert.That(RunLog.KeepLogs, Is.EqualTo(RunLog.DefaultKeepLogs));
            Assert.That(RunLog.DefaultKeepLogs, Is.EqualTo(30));
        }

        [Test]
        public void ItCanBeChanged()
        {
            RunLog.KeepLogs = 5;

            Assert.That(RunLog.KeepLogs, Is.EqualTo(5));
        }

        [Test]
        public void KeepingOnlyTheLiveOneIsARealAnswer()
        {
            RunLog.KeepLogs = 0;

            Assert.That(RunLog.KeepLogs, Is.EqualTo(0));
        }

        [Test]
        public void FewerThanNoneIsRefused()
        {
            Assert.Throws<ArgumentOutOfRangeException>(delegate { RunLog.KeepLogs = -1; });
            Assert.That(RunLog.KeepLogs, Is.EqualTo(RunLog.DefaultKeepLogs));
        }
    }
}
