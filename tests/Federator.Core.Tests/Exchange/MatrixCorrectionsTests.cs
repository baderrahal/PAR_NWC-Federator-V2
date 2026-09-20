using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Federator.Core.Exchange;
using NUnit.Framework;

namespace Federator.Core.Tests
{
    /// <summary>
    /// The corrections applied to the client's matrix before it is used, F87.
    ///
    /// The project's own set names are SAMPLE DATA and live here, never in src, which is
    /// CLAUDE.md's rule. What lives in src is only the rule for applying them safely.
    ///
    /// The test that matters most is the last one. It asserts that the file committed
    /// under exchange is exactly what the rule produces from the file under samples, so
    /// the artifact can never drift away from the rule that made it.
    /// </summary>
    [TestFixture]
    public class MatrixCorrectionsTests
    {
        // ---------- the project's corrections, as sample data ----------

        private const string BrokenName = "BLD-DRPipe Accessories";
        private const string CorrectName = "BLD-DR-Pipe Accessories";
        private const string DevicesSet = "BLD-EL-Devices";
        private const string DevicesAskedFor = "Electrical Fixtures";
        private const string DevicesShouldAskFor = "Nurse Call Devices";

        private static IList<SetRename> ProjectRenames()
        {
            return new List<SetRename> { new SetRename(BrokenName, CorrectName) };
        }

        private static IList<CategoryRewrite> ProjectRewrites()
        {
            return new List<CategoryRewrite>
            {
                new CategoryRewrite(
                    DevicesSet,
                    DevicesAskedFor,
                    DevicesShouldAskFor,
                    "F87 fallback: equals on one named category until scan.md 5g measures whether a negated condition imports")
            };
        }

        /// <summary>
        /// The six device categories this project's OTHER sets already claim, so the catch
        /// all must not double count them. Sample data off the client's own matrix, read
        /// out of it by TheSixDeviceSetsThatKeepTheirOwnGeometryAreAllInTheMatrix below,
        /// and in the order the file would sort them so the artefact is stable.
        /// </summary>
        private static readonly string[] DeviceCategoriesOtherSetsClaim =
        {
            "Communication Devices", "Data Devices", "Fire Alarm Devices",
            "Lighting Devices", "Security Devices", "Telephone Devices"
        };

        /// <summary>
        /// WHAT THE CORRECTED FILE CARRIES SINCE THE DIMMING ROUND. BLD-EL-Devices asks
        /// for a category holding Devices and none of the six its siblings claim, which
        /// is what F87 wanted from the start. It shipped the one equals fallback because
        /// whether a negated condition imports was not measured, and 5g measured it on
        /// 2026-09-20: it does, exactly, and the fallback found ZERO items in 1A02MM.
        /// ProjectRewrites above is kept as sample data for the generic rewrite tests and
        /// is no longer what the committed file is made with.
        /// </summary>
        private static IList<ConditionsRewrite> ProjectConditions()
        {
            return new List<ConditionsRewrite>
            {
                new ConditionsRewrite(
                    DevicesSet,
                    "Devices",
                    new List<string>(DeviceCategoriesOtherSetsClaim),
                    "F87 in full since scan.md 5g: contains, then one negated equals per sibling category")
            };
        }

        private static string Read(string path)
        {
            using (StreamReader reader = new StreamReader(path, Encoding.UTF8))
            {
                return reader.ReadToEnd();
            }
        }

        private static string Set(string name, string value)
        {
            return "<selectionset name=\"" + name + "\" guid=\"x\">"
                + "<findspec mode=\"all\" disjoint=\"0\"><conditions>"
                + "<condition test=\"equals\" flags=\"0\">"
                + "<value><data type=\"wstring\">" + value + "</data></value>"
                + "</condition></conditions></findspec></selectionset>";
        }

        // ---------- the rename ----------

        [Test]
        public void ARenameChangesEveryPlaceTheNameAppears()
        {
            string xml = Set("A", "x") + "<locator>" + BrokenName + "</locator>"
                + "<clashtest name=\"" + BrokenName + "-vs-B\"/>"
                + Set(BrokenName, "y");

            CorrectionOutcome outcome = MatrixCorrections.Apply(xml, ProjectRenames(), null);

            Assert.That(outcome.Counts[0].Count, Is.EqualTo(3));
            Assert.That(outcome.Text, Does.Not.Contain(BrokenName));
            Assert.That(outcome.Text.Split(new[] { CorrectName }, StringSplitOptions.None).Length - 1,
                Is.EqualTo(3));
        }

        /// <summary>
        /// The idempotency law, refused where the rename is built rather than discovered
        /// on the second run. A new name still holding the old one grows the file forever.
        /// </summary>
        [Test]
        public void ARenameThatWouldRenameItsOwnOutputIsRefused()
        {
            Assert.That(
                () => new SetRename("BLD-X", "BLD-BLD-X"),
                Throws.ArgumentException.With.Message.Contains("safe to run on its own output"));

            Assert.That(() => new SetRename("A", "A"), Throws.ArgumentException);
            Assert.That(() => new SetRename(null, "B"), Throws.ArgumentException);
            Assert.That(() => new SetRename("A", null), Throws.ArgumentException);
        }

        [Test]
        public void ARenameThatFindsNothingIsCountedAndSaysWhy()
        {
            CorrectionOutcome outcome = MatrixCorrections.Apply(Set("A", "x"), ProjectRenames(), null);

            Assert.That(outcome.Counts[0].Count, Is.EqualTo(0));
            Assert.That(outcome.Counts[0].Note, Does.Contain("does not hold that name"));
            Assert.That(outcome.TotalChanged, Is.EqualTo(0));
        }

        // ---------- the rewrite ----------

        [Test]
        public void ARewriteChangesTheNamedSetAndNoOther()
        {
            string xml = Set("BLD-EL-Electrical Fixtures", DevicesAskedFor)
                + Set(DevicesSet, DevicesAskedFor)
                + Set("BLD-EL-Lighting Fixtures", DevicesAskedFor);

            CorrectionOutcome outcome = MatrixCorrections.Apply(xml, null, ProjectRewrites());

            Assert.That(outcome.Counts[0].Count, Is.EqualTo(1));

            // The other two sets ask for the same value legitimately and are untouched.
            // Counted as VALUES and not as raw text, because BLD-EL-Electrical Fixtures
            // carries the words in its NAME as well, and a correction that went by raw
            // text would rename the set while it was at it.
            string asked = "<data type=\"wstring\">" + DevicesAskedFor + "</data>";

            Assert.That(
                outcome.Text.Split(new[] { asked }, StringSplitOptions.None).Length - 1,
                Is.EqualTo(2));
            Assert.That(outcome.Text, Does.Contain(DevicesShouldAskFor));
            Assert.That(outcome.Lines(), Has.Some.Contains("scan.md 5g"), "the note rides on the outcome line, A15");
            Assert.That(outcome.Text, Does.Contain("name=\"BLD-EL-Electrical Fixtures\""),
                "the set name is not a value and is never rewritten");
        }

        [Test]
        public void ARewriteForASetThatIsNotThereSaysSoAndChangesNothing()
        {
            string xml = Set("BLD-EL-Electrical Fixtures", DevicesAskedFor);
            CorrectionOutcome outcome = MatrixCorrections.Apply(xml, null, ProjectRewrites());

            Assert.That(outcome.Counts[0].Count, Is.EqualTo(0));
            Assert.That(outcome.Counts[0].Note, Does.Contain("no set of that name"));
            Assert.That(outcome.Text, Is.EqualTo(xml));
        }

        [Test]
        public void ARewriteWhereTheSetAlreadyAsksForSomethingElseSaysThat()
        {
            string xml = Set(DevicesSet, DevicesShouldAskFor);
            CorrectionOutcome outcome = MatrixCorrections.Apply(xml, null, ProjectRewrites());

            Assert.That(outcome.Counts[0].Count, Is.EqualTo(0));
            Assert.That(outcome.Counts[0].Note, Does.Contain("already asks for something else"));
        }

        // ---------- safe to run twice ----------

        /// <summary>
        /// The whole point. Applying the corrections to their own output changes nothing,
        /// and TotalChanged coming back zero is how that is proved rather than argued.
        /// </summary>
        [Test]
        public void ApplyingTheCorrectionsToTheirOwnOutputChangesNothing()
        {
            string xml = Set(DevicesSet, DevicesAskedFor)
                + "<locator>" + BrokenName + "</locator>";

            CorrectionOutcome once = MatrixCorrections.Apply(xml, ProjectRenames(), ProjectRewrites());
            Assert.That(once.TotalChanged, Is.EqualTo(2));

            CorrectionOutcome twice = MatrixCorrections.Apply(
                once.Text, ProjectRenames(), ProjectRewrites());

            Assert.That(twice.TotalChanged, Is.EqualTo(0));
            Assert.That(twice.Text, Is.EqualTo(once.Text));
            Assert.That(twice.Lines(), Has.Some.Contains("already carries every correction"));
        }

        // ---------- the real files ----------

        [Test]
        public void TheMatrixOnDiskHoldsTheBrokenNameOneHundredAndTwentyOneTimes()
        {
            string xml = Read(Samples.Matrix());

            CorrectionOutcome outcome = MatrixCorrections.Apply(xml, ProjectRenames(), ProjectRewrites());

            Assert.That(outcome.Counts[0].Count, Is.EqualTo(121), "the hyphen, measured");
            Assert.That(outcome.Counts[1].Count, Is.EqualTo(1), "BLD-EL-Devices");
            Assert.That(outcome.TotalChanged, Is.EqualTo(122));
        }

        /// <summary>
        /// The corrections the committed file is actually made with: the hyphen 121 times
        /// and the catch all set rewritten into seven conditions, one contains and six
        /// negated equals.
        /// </summary>
        [Test]
        public void TheCatchAllSetIsRewrittenIntoOneContainsAndSixNegations()
        {
            string xml = Read(Samples.Matrix());

            CorrectionOutcome outcome = MatrixCorrections.Apply(
                xml, ProjectRenames(), null, ProjectConditions());

            Assert.That(outcome.Counts[0].Count, Is.EqualTo(121), "the hyphen, measured");
            Assert.That(outcome.Counts[1].Count, Is.EqualTo(7), "one contains and six negated");
            Assert.That(outcome.TotalChanged, Is.EqualTo(128));

            Assert.That(outcome.Text, Does.Contain("<condition test=\"contains\" flags=\"0\">"));
            Assert.That(outcome.Text, Does.Contain("<condition test=\"equals\" flags=\"32\">"));
        }

        /// <summary>
        /// THE ONE THAT KEEPS THE ARTIFACT HONEST. The file committed under exchange is
        /// byte for byte what the rule produces from the file under samples. If anyone
        /// edits either by hand, this fails.
        /// </summary>
        [Test]
        public void TheCorrectedFileIsExactlyWhatTheRuleProduces()
        {
            string source = Read(Samples.Matrix());
            string committed = Read(Samples.CorrectedMatrix());

            CorrectionOutcome outcome = MatrixCorrections.Apply(
                source, ProjectRenames(), null, ProjectConditions());

            Assert.That(outcome.Text, Is.EqualTo(committed));
        }

        [Test]
        public void TheCorrectedFileCarriesNeitherFaultAndRunningItAgainChangesNothing()
        {
            string committed = Read(Samples.CorrectedMatrix());

            Assert.That(committed, Does.Not.Contain(BrokenName));
            Assert.That(committed, Does.Contain(CorrectName));

            CorrectionOutcome again = MatrixCorrections.Apply(
                committed, ProjectRenames(), null, ProjectConditions());

            Assert.That(again.TotalChanged, Is.EqualTo(0),
                "a second run reading zero is what proves the corrections are idempotent");
        }

        /// <summary>
        /// What the catch all set asks for, read off the committed file rather than off
        /// the rule that wrote it, so the artefact is checked and not the intention. One
        /// contains and six negated equals, and the fallback value is gone.
        /// </summary>
        [Test]
        public void TheCommittedFileCarriesTheNegatedFormAndNotTheFallback()
        {
            string committed = Read(Samples.CorrectedMatrix());
            int at = committed.IndexOf("<selectionset name=\"" + DevicesSet + "\"", StringComparison.Ordinal);
            Assert.That(at, Is.GreaterThan(-1));

            int ends = committed.IndexOf("</selectionset>", at, StringComparison.Ordinal);
            string block = committed.Substring(at, ends - at);

            Assert.That(Occurrences(block, "<condition test=\"contains\" flags=\"0\">"), Is.EqualTo(1));
            Assert.That(Occurrences(block, "<condition test=\"equals\" flags=\"32\">"), Is.EqualTo(6));
            Assert.That(block, Does.Contain(">Devices<"));
            Assert.That(block, Does.Not.Contain(DevicesShouldAskFor), "the one equals fallback is gone");

            foreach (string category in DeviceCategoriesOtherSetsClaim)
            {
                Assert.That(block, Does.Contain(">" + category + "<"), category);
            }
        }

        private static int Occurrences(string text, string what)
        {
            int count = 0;
            int at = text.IndexOf(what, StringComparison.Ordinal);

            while (at >= 0)
            {
                count++;
                at = text.IndexOf(what, at + what.Length, StringComparison.Ordinal);
            }

            return count;
        }

        [Test]
        public void TheSixDeviceSetsThatKeepTheirOwnGeometryAreAllInTheMatrix()
        {
            string committed = Read(Samples.CorrectedMatrix());

            // BLD-EL-Devices must not double count what these six already hold. Security
            // Devices breaks the BLD-EL- pattern its siblings follow, which is F84's warning.
            foreach (string set in new[]
            {
                "BLD-EL-Lighting Devices", "BLD-EL-Fire Alarm Devices", "BLD-Security Devices",
                "BLD-EL-Communication Devices", "BLD-EL-Telephone Devices", "BLD-EL-Data Devices"
            })
            {
                Assert.That(committed, Does.Contain("<selectionset name=\"" + set + "\""), set);
            }
        }
    }
}
