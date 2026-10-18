using Federator.Core.Rerun;
using NUnit.Framework;

namespace Federator.Core.Tests
{
    /// <summary>
    /// F129, Bader's request 4 under Q112, FR-178, and since F232 the NWFs ticked in the NWF page's tree: each runs the way the
    /// open file run runs it. What is left of the pick in Core is the test of an NWF by its extension, the phrase of the
    /// DECIDE step and the reasons a group stops at its open.
    /// </summary>
    [TestFixture]
    public class NwfPickTests
    {
        /// <summary>An NWF is judged by its extension alone, .nwf read case blind, so an NWD, an NWC, a .nwf.bak and a .nwfx are never taken.</summary>
        [Test]
        public void AnNwfIsJudgedByItsExtensionAlone()
        {
            Assert.That(NwfPick.IsNwf(TestPaths.At("Feds", "a.nwf")), Is.True);
            Assert.That(NwfPick.IsNwf(TestPaths.At("Feds", "a.NWF")), Is.True);
            Assert.That(NwfPick.IsNwf(TestPaths.At("Feds", "a.nwd")), Is.False);
            Assert.That(NwfPick.IsNwf(TestPaths.At("Feds", "a.nwc")), Is.False);
            Assert.That(NwfPick.IsNwf(TestPaths.At("Feds", "a.nwf.bak")), Is.False);
            Assert.That(NwfPick.IsNwf(TestPaths.At("Feds", "a.nwfx")), Is.False);
            Assert.That(NwfPick.IsNwf(null), Is.False);
        }

        /// <summary>
        /// F129 attempt 2, the reviewer's blocking finding: the DECIDE step around a picked NWF's
        /// open said not opened for one that opened and read empty. It says what the open did.
        /// </summary>
        [Test]
        public void TheDecidePhraseSaysWhatTheOpenDid()
        {
            Assert.That(NwfPick.DecidePhrase(true, false, false), Is.EqualTo("opened and read empty"));
            Assert.That(NwfPick.DecidePhrase(true, true, true), Is.EqualTo("opened and read empty"));
            Assert.That(NwfPick.DecidePhrase(false, true, false), Is.EqualTo("not opened"));
            Assert.That(NwfPick.DecidePhrase(false, false, false), Is.EqualTo(RunPath.WeeklyRun));
            Assert.That(NwfPick.DecidePhrase(false, false, true), Is.EqualTo(RunPath.WeeklyRunPlusMatrix));
        }

        [Test]
        public void TheReasonsAGroupStopsAtTheOpenNameTheFile()
        {
            string nwf = TestPaths.At("Feds", "a.nwf");

            Assert.That(NwfPick.WouldNotOpen(nwf), Does.Contain(nwf));
            Assert.That(NwfPick.WouldNotOpen(nwf), Does.Contain("nothing was run on it"));
            Assert.That(NwfPick.ReadEmpty(nwf, 120.0), Does.Contain(nwf));
            Assert.That(NwfPick.ReadEmpty(nwf, 120.0), Does.Contain("no models"));
            Assert.That(NwfPick.ReadEmpty(nwf, 120.0), Does.Contain("120.0"));
        }
    }
}
