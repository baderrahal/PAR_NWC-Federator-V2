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

        /// <summary>
        /// Q68, the VALUE corrections, built from the worksets measured off the models
        /// and never from a list typed here. Every value the matrix asks for is offered
        /// its case-only candidates and the rule decides, so a value with none and a
        /// value with two are both left exactly as they were.
        /// </summary>
        private static IList<ValueRewrite> ProjectValues()
        {
            return ValueRewrite.For(WorksetValuesIn(Read(Samples.Matrix())), RevitWorksets.All());
        }

        /// <summary>
        /// Every value a WORKSET condition in that file asks for. Read off the file, so
        /// a matrix that starts asking for a different workset is covered without a line
        /// of this being changed.
        /// </summary>
        private static IList<string> WorksetValuesIn(string xml)
        {
            List<string> values = new List<string>();
            int at = 0;

            while (true)
            {
                int property = xml.IndexOf(WorksetProperty, at, StringComparison.Ordinal);

                if (property < 0)
                {
                    return values;
                }

                int opens = xml.IndexOf(DataOpens, property, StringComparison.Ordinal);
                int closes = opens < 0 ? -1 : xml.IndexOf(DataCloses, opens, StringComparison.Ordinal);

                if (opens < 0 || closes < 0)
                {
                    return values;
                }

                string value = xml.Substring(opens + DataOpens.Length, closes - opens - DataOpens.Length);

                if (value.Length > 0 && !values.Contains(value))
                {
                    values.Add(value);
                }

                at = closes;
            }
        }

        /// <summary>The internal name of the Revit Workset parameter, as the client file writes it.</summary>
        private const string WorksetProperty = "lcldrevit_parameter_-1002053";

        private const string DataOpens = "<data type=\"wstring\">";

        private const string DataCloses = "</data>";

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
                xml, ProjectRenames(), null, ProjectConditions(), ProjectValues());

            Assert.That(outcome.Counts[0].Count, Is.EqualTo(121), "the hyphen, measured");
            Assert.That(outcome.Counts[1].Count, Is.EqualTo(7), "one contains and six negated");

            // Q68 on 2026-09-20. Four workset values the matrix spells in capitals and
            // every model spells in title case, 17 conditions between them, 5t:
            // ME-DUCTWORK 5, ME-PIPING 5, PL-Domestic Water 6 and ME-EQUIPMENT 1.
            Assert.That(ValuesChangedIn(outcome), Is.EqualTo(17), "the worksets, measured off the models");
            Assert.That(outcome.TotalChanged, Is.EqualTo(145), "121 hyphens, 7 conditions and 17 workset values");

            Assert.That(outcome.Text, Does.Contain("<condition test=\"contains\" flags=\"0\">"));
            Assert.That(outcome.Text, Does.Contain("<condition test=\"equals\" flags=\"32\">"));
        }

        /// <summary>
        /// The four values the models settle, and the three the rule leaves alone: two
        /// no model carries a case variant of, and one the models already spell exactly
        /// as the matrix does. A rule that corrected any of those three would be guessing.
        /// </summary>
        [Test]
        public void TheWorksetValuesTheModelsSettleAreCorrectedAndTheOthersAreLeftAlone()
        {
            CorrectionOutcome outcome = MatrixCorrections.Apply(
                Read(Samples.Matrix()), ProjectRenames(), null, ProjectConditions(), ProjectValues());

            string said = string.Join("\n", new List<string>(outcome.Lines()).ToArray());

            Assert.That(said, Does.Contain("the value ME-DUCTWORK becomes ME-Ductwork"));
            Assert.That(said, Does.Contain("the value ME-PIPING becomes ME-Piping"));
            Assert.That(said, Does.Contain("the value ME-EQUIPMENT becomes ME-Equipment"));
            Assert.That(said, Does.Contain("the value PL-Domestic Water becomes PL-Domestic water"));

            Assert.That(said, Does.Contain("the value FP-PIPING is left alone"));
            Assert.That(said, Does.Contain("the value FF-FIRE FIGHTING is left alone"));
            Assert.That(said, Does.Contain("the value PL-Drainage is left alone"));

            Assert.That(outcome.Text, Does.Not.Contain("ME-DUCTWORK"), "no set still asks in capitals");
            Assert.That(outcome.Text, Does.Contain("ME-Ductwork"));
        }

        /// <summary>How many conditions the VALUE corrections changed, added across them.</summary>
        private static int ValuesChangedIn(CorrectionOutcome outcome)
        {
            int changed = 0;

            foreach (CorrectionCount count in outcome.Counts)
            {
                if (count.What.IndexOf("the value ", StringComparison.Ordinal) == 0)
                {
                    changed += count.Count;
                }
            }

            return changed;
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
                source, ProjectRenames(), null, ProjectConditions(), ProjectValues());

            Assert.That(outcome.Text, Is.EqualTo(committed));
        }

        [Test]
        public void TheCorrectedFileCarriesNeitherFaultAndRunningItAgainChangesNothing()
        {
            string committed = Read(Samples.CorrectedMatrix());

            Assert.That(committed, Does.Not.Contain(BrokenName));
            Assert.That(committed, Does.Contain(CorrectName));

            CorrectionOutcome again = MatrixCorrections.Apply(
                committed, ProjectRenames(), null, ProjectConditions(), ProjectValues());

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

        // ---------- Q68, the value corrected to the spelling the models carry ----------

        /// <summary>
        /// The four the client's own models settle, 5t. The matrix asks in capitals and
        /// every model writes it in title case, and a search condition compares a value
        /// CASE SENSITIVELY unless a flag nothing sets is set.
        /// </summary>
        [Test]
        public void AValueTheModelsSpellOneOtherWayIsCorrectedToTheirSpelling()
        {
            IList<ValueRewrite> rewrites = ValueRewrite.For(
                new[] { "ME-DUCTWORK", "ME-PIPING", "ME-EQUIPMENT", "PL-Domestic Water" },
                new[] { "ME-Ductwork", "ME-Piping", "ME-Equipment", "PL-Domestic water", "AR-EXTERIOR", "ST-SUB" });

            Assert.That(rewrites.Count, Is.EqualTo(4));

            foreach (ValueRewrite rewrite in rewrites)
            {
                Assert.That(rewrite.Corrects, Is.True, rewrite.From);
            }

            Assert.That(rewrites[0].To, Is.EqualTo("ME-Ductwork"));
            Assert.That(rewrites[3].To, Is.EqualTo("PL-Domestic water"));

            string xml = "<data type=\"wstring\">ME-DUCTWORK</data>";
            CorrectionOutcome outcome = MatrixCorrections.Apply(xml, null, null, null, rewrites);

            Assert.That(outcome.Text, Is.EqualTo("<data type=\"wstring\">ME-Ductwork</data>"));
        }

        /// <summary>
        /// The refusal, which is the half that keeps this safe. A rule that guesses
        /// between two real worksets is worse than a set that finds nothing, so the value
        /// is left EXACTLY as it was and both spellings are named.
        /// </summary>
        [Test]
        public void AValueTheModelsSpellTwoWaysIsRefusedAndBothAreNamed()
        {
            IList<ValueRewrite> rewrites = ValueRewrite.For(
                new[] { "EL-FIRE ALARM" },
                new[] { "EL-Fire Alarm", "EL-Fire alarm" });

            Assert.That(rewrites[0].Candidates.Count, Is.EqualTo(2));
            Assert.That(rewrites[0].To, Is.Null);
            Assert.That(rewrites[0].Corrects, Is.False);

            string xml = "<data type=\"wstring\">EL-FIRE ALARM</data>";
            CorrectionOutcome outcome = MatrixCorrections.Apply(xml, null, null, null, rewrites);

            Assert.That(outcome.Text, Is.EqualTo(xml), "left exactly as it was");
            Assert.That(Words(outcome), Does.Contain("EL-Fire Alarm and EL-Fire alarm"));
            Assert.That(Words(outcome), Does.Contain("worse than a set that finds nothing"));
        }

        [Test]
        public void AValueNoModelSpellsAnyOtherWayIsLeftAloneAndSaysWhy()
        {
            IList<ValueRewrite> rewrites = ValueRewrite.For(
                new[] { "FP-PIPING", "PL-Drainage" },
                new[] { "PL-Drainage", "ME-Piping" });

            Assert.That(rewrites[0].Corrects, Is.False, "no model carries an FP workset at all");
            Assert.That(rewrites[1].Corrects, Is.False, "the models spell it exactly as the matrix does");

            string xml = "<data type=\"wstring\">PL-Drainage</data>";
            CorrectionOutcome outcome = MatrixCorrections.Apply(xml, null, null, null, rewrites);

            Assert.That(outcome.Text, Is.EqualTo(xml));
            Assert.That(Words(outcome), Does.Contain("no model in this run carries a workset spelled that way"));
            Assert.That(Words(outcome), Does.Contain("the models spell it exactly as the matrix does"));
        }

        /// <summary>
        /// A workset name can sit inside a set name or another value, so the rewrite is
        /// scoped to a whole data element and never replaced across the file. Without
        /// that, correcting ME-PIPING would also rewrite a set called ME-PIPING MAINS.
        /// </summary>
        [Test]
        public void OnlyAWholeValueIsRewrittenAndNeverATextThatMerelyHoldsIt()
        {
            IList<ValueRewrite> rewrites = ValueRewrite.For(
                new[] { "ME-PIPING" }, new[] { "ME-Piping" });

            string xml = "<selectionset name=\"ME-PIPING MAINS\">"
                + "<data type=\"wstring\">ME-PIPING MAINS</data>"
                + "<data type=\"wstring\">ME-PIPING</data>";

            CorrectionOutcome outcome = MatrixCorrections.Apply(xml, null, null, null, rewrites);

            Assert.That(outcome.Text, Does.Contain("<selectionset name=\"ME-PIPING MAINS\">"), "the set name is untouched");
            Assert.That(outcome.Text, Does.Contain("<data type=\"wstring\">ME-PIPING MAINS</data>"), "the longer value is untouched");
            Assert.That(outcome.Text, Does.Contain("<data type=\"wstring\">ME-Piping</data>"), "the whole value is corrected");
        }

        [Test]
        public void TheCorrectionsAreNamedInTheLogWithBothSpellingsSideBySide()
        {
            IList<ValueRewrite> rewrites = ValueRewrite.For(
                new[] { "ME-DUCTWORK" }, new[] { "ME-Ductwork" });

            CorrectionOutcome outcome = MatrixCorrections.Apply(
                "<data type=\"wstring\">ME-DUCTWORK</data>", null, null, null, rewrites);

            Assert.That(Words(outcome),
                Does.Contain("the value ME-DUCTWORK becomes ME-Ductwork, which is how the models spell it"));
        }

        private static string Words(CorrectionOutcome outcome)
        {
            List<string> lines = new List<string>(outcome.Lines());
            return string.Join("\n", lines.ToArray());
        }

        /// <summary>
        /// The generator, run by hand when the rule or the measured workset list changes.
        /// It is a TEST and not a script so it reads the same samples the byte for byte
        /// test reads and can never produce something that test would then reject.
        /// </summary>
        [Test]
        [Explicit("Writes the corrected matrix into the exchange folder. Run by hand.")]
        public void WriteTheCorrectedFile()
        {
            string source = Read(Samples.Matrix());

            CorrectionOutcome outcome = MatrixCorrections.Apply(
                source, ProjectRenames(), null, ProjectConditions(), ProjectValues());

            using (StreamWriter writer = new StreamWriter(Samples.CorrectedMatrix(), false, new UTF8Encoding(false)))
            {
                writer.Write(outcome.Text);
            }

            foreach (string line in outcome.Lines())
            {
                TestContext.WriteLine(line);
            }
        }

        // ---------- Q69, the Or row for a workset his models spell two ways ----------

        /// <summary>
        /// The Or row is flags="64", StartGroup, which F78 measured: a condition with
        /// that bit starts a new group, conditions inside a group are ANDed and groups
        /// are ORed. So the set finds both spellings while the models are still wrong.
        /// </summary>
        [Test]
        public void AWorksetSpelledTwoWaysBuildsAnOrRowCarryingBoth()
        {
            string xml = Set("BLD-ME-Ducts", "PL-Drainage equipment");

            CorrectionOutcome outcome = MatrixCorrections.Apply(
                xml, null, null, null, null,
                new List<ValueOrRow> { new ValueOrRow("PL-Drainage equipment", "PL-Drainage equipmen") });

            Assert.That(outcome.Text, Does.Contain("<data type=\"wstring\">PL-Drainage equipment</data>"));
            Assert.That(outcome.Text, Does.Contain("<data type=\"wstring\">PL-Drainage equipmen</data>"));
            Assert.That(outcome.Text, Does.Contain("<condition test=\"equals\" flags=\"64\">"));
            Assert.That(outcome.TotalChanged, Is.EqualTo(1));
        }

        [Test]
        public void AWorksetSpelledOneWayBuildsOneConditionAndNoOrRow()
        {
            string xml = Set("BLD-ME-Ducts", "ME-Ductwork");

            CorrectionOutcome outcome = MatrixCorrections.Apply(
                xml, null, null, null, null,
                new List<ValueOrRow> { new ValueOrRow("ME-Ductwork", "ME-Ductwork") });

            Assert.That(outcome.Text, Is.EqualTo(xml), "left exactly as it was");
            Assert.That(outcome.Text, Does.Not.Contain("flags=\"64\""));
            Assert.That(outcome.TotalChanged, Is.EqualTo(0));
        }

        [Test]
        public void AddingTheOrRowTwiceAddsItOnce()
        {
            IList<ValueOrRow> rows = new List<ValueOrRow>
            {
                new ValueOrRow("PL-Drainage equipment", "PL-Drainage equipmen")
            };

            CorrectionOutcome once = MatrixCorrections.Apply(
                Set("BLD-ME-Ducts", "PL-Drainage equipment"), null, null, null, null, rows);

            CorrectionOutcome twice = MatrixCorrections.Apply(once.Text, null, null, null, null, rows);

            Assert.That(twice.Text, Is.EqualTo(once.Text));
            Assert.That(twice.TotalChanged, Is.EqualTo(0));
        }

        /// <summary>
        /// 5t on the real matrix: NOT ONE of the five disagreeing pairs is a value any
        /// set filters on, so the Or row correctly produces nothing here. Saying that is
        /// more use than an Or row that changes no number, and this test pins it so a
        /// later matrix that DOES ask for one of them fails and gets looked at.
        /// </summary>
        [Test]
        public void NoneOfHisDisagreeingWorksetsIsAValueTheMatrixAsksFor()
        {
            IList<string> asked = WorksetValuesIn(Read(Samples.CorrectedMatrix()));

            foreach (string disagreeing in new[]
            {
                "EL-Fire Alarm", "EL-Fire alarm", "EV-Access Control", "EV-Access control",
                "EL-Lightning Protection", "EL-Lightining Protection",
                "EV-Cctv System", "EV-Ccctv system",
                "PL-Drainage equipment", "PL-Drainage equipmen"
            })
            {
                Assert.That(asked, Does.Not.Contain(disagreeing),
                    disagreeing + " is now a value the matrix filters on, so Q69's Or row has work to do and "
                        + "the round report saying it produced nothing is out of date");
            }
        }
    }
}
