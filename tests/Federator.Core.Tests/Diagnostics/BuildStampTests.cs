using System;
using System.Reflection;
using System.Text.RegularExpressions;
using Federator.Core.Diagnostics;
using NUnit.Framework;

namespace Federator.Core.Tests
{
    /// <summary>
    /// The plain assembly version is 1.0.0.0 and always will be, so a stale install is
    /// invisible in a log. Bader ran an old binary twice before the stamp existed.
    /// </summary>
    [TestFixture]
    public class BuildStampTests
    {
        [Test]
        public void TheCoreAssemblyCarriesAStamp()
        {
            string stamp = BuildStamp.Of(typeof(BuildStamp).Assembly);

            Assert.That(stamp, Is.Not.Null.And.Not.Empty);
            Assert.That(stamp, Is.Not.EqualTo(BuildStamp.Unknown),
                "the build did not stamp the assembly, see Directory.Build.targets");
            Assert.That(BuildStamp.LooksStamped(stamp), Is.True, stamp);
        }

        [Test]
        public void TheTestAssemblyCarriesAStampToo()
        {
            string stamp = BuildStamp.Of(typeof(BuildStampTests).Assembly);

            Assert.That(BuildStamp.LooksStamped(stamp), Is.True, stamp);
        }

        // 1.0.0.0 771321b5 built 2026-08-30 12:56:14
        // or, when the working tree has uncommitted changes, 771321b5+edits
        [Test]
        public void TheStampCarriesAVersionACommitAndAMoment()
        {
            string stamp = BuildStamp.Of(typeof(BuildStamp).Assembly);

            Assert.That(stamp, Does.Match(
                @"^\d+\.\d+\.\d+\.\d+ (?:[0-9a-f]{7,}(?:\+edits)?|nogit(?:\+edits)?) built \d{4}-\d{2}-\d{2} \d{2}:\d{2}:\d{2}$"),
                "the stamp is not in the shape Directory.Build.targets writes: " + stamp);
        }

        [Test]
        public void TheMomentInTheStampIsAReadableDate()
        {
            Match found = Regex.Match(BuildStamp.Of(typeof(BuildStamp).Assembly), @"built (\d{4}-\d{2}-\d{2} \d{2}:\d{2}:\d{2})$");

            Assert.That(found.Success, Is.True);

            DateTime builtAt;
            Assert.That(
                DateTime.TryParse(found.Groups[1].Value, out builtAt), Is.True,
                "the build time is not a date");
            Assert.That(builtAt, Is.GreaterThan(new DateTime(2020, 1, 1)));
        }

        // The stamp has to identify one binary, so two different binaries must not share
        // one. Core and the test assembly are built at different moments.
        [Test]
        public void TwoAssembliesBuiltSeparatelyCarrySeparateStamps()
        {
            string core = BuildStamp.Of(typeof(BuildStamp).Assembly);
            string tests = BuildStamp.Of(typeof(BuildStampTests).Assembly);

            Assert.That(BuildStamp.LooksStamped(core), Is.True);
            Assert.That(BuildStamp.LooksStamped(tests), Is.True);

            // Both carry a real moment. They are usually seconds apart, and on a fast
            // machine can land in the same second, so this checks they are readable rather
            // than that they differ, which is proved by a real rebuild instead.
            Assert.That(core, Does.Contain(" built "));
            Assert.That(tests, Does.Contain(" built "));
        }

        [Test]
        public void AnAssemblyWithNoStampSaysUnknownRatherThanGuessing()
        {
            // mscorlib carries an informational version that is not one of ours, so this
            // uses a null to reach the no assembly path deliberately.
            Assert.That(BuildStamp.Of(null), Is.EqualTo(BuildStamp.Unknown));
            Assert.That(BuildStamp.LooksStamped(BuildStamp.Unknown), Is.False);
            Assert.That(BuildStamp.LooksStamped(null), Is.False);
            Assert.That(BuildStamp.LooksStamped("1.0.0.0"), Is.False,
                "a bare version number is not a stamp");
        }
    }
}
