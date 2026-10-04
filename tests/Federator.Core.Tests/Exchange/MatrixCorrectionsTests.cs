using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Federator.Core.Exchange;
using Federator.Core.Sets;
using NUnit.Framework;

namespace Federator.Core.Tests
{
    /// <summary>
    /// The corrections applied to the client's matrix before it is used, F87.
    ///
    /// The project's own set names below are SAMPLE DATA for the generic tests, which is
    /// CLAUDE.md's rule. The project's corrections themselves are not in src: they are its
    /// list, kept beside the picked XML and read when it is picked, and this project's list
    /// is in the exchange folder beside the corrected XML, Q113 answered B on 2026-10-04.
    ///
    /// The test that matters most is TheCorrectedFileIsExactlyWhatTheRuleProduces. It asserts
    /// that the file committed under exchange is exactly what the rule produces from the file
    /// under samples with that list, so the artifact can never drift away from the rule and
    /// the list that made it.
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
        /// This project's list, read the way the tool reads it beside a picked file, off the one
        /// kept in the exchange folder beside the corrected XML, Q113.
        /// </summary>
        private static MatrixCorrectionList TheList()
        {
            return MatrixCorrectionList.Beside(Samples.CorrectedMatrix(), new CorrectionListSettings());
        }

        /// <summary>
        /// WHAT THE CORRECTED FILE IS MADE WITH SINCE F116: the corrections the tool applies
        /// to whichever XML is picked, Q104, this project's list out of the exchange folder,
        /// Q113, and the measured workset list inside Core, and never a copy of them here.
        /// BLD-EL-Devices asks for a category holding Devices and none of the six its siblings
        /// claim, read off the file, F87 in full since 5g measured on 2026-09-20 that a negated
        /// condition imports. The project's corrections above are kept as sample data for the
        /// generic tests.
        /// </summary>
        private static CorrectionOutcome Picked(string xml)
        {
            return MatrixCorrections.ForPickedFile(xml, TheList(), RevitWorksets.All());
        }

        /// <summary>The internal name of the Revit Workset parameter, as the client file writes it.</summary>
        private const string WorksetProperty = "lcldrevit_parameter_-1002053";

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

        /// <summary>
        /// FR-026. The rewrite replaced its value across the whole set block, and the block
        /// starts with the set's own name, so a set whose name holds the value it asks for
        /// was renamed inside its block, and every clash test pointing at the old name would
        /// point at nothing. Only the value is rewritten.
        /// </summary>
        [Test]
        public void ACategoryRewriteNeverRenamesTheSetItIsIn()
        {
            string xml = Set("BLD-EL-Electrical Fixtures", "Electrical Fixtures");

            CorrectionOutcome outcome = MatrixCorrections.Apply(
                xml, null,
                new List<CategoryRewrite> { new CategoryRewrite("BLD-EL-Electrical Fixtures", "Electrical Fixtures", "Lighting Fixtures") });

            Assert.That(outcome.Text, Does.Contain("<selectionset name=\"BLD-EL-Electrical Fixtures\""), "the name is not a value");
            Assert.That(outcome.Text, Does.Contain("<data type=\"wstring\">Lighting Fixtures</data>"));
            Assert.That(outcome.Counts[0].Count, Is.EqualTo(1), "one value, and the name is not counted");
        }

        /// <summary>
        /// FR-026, the other half. A rewrite whose new value still holds the old one found the
        /// old one again inside the new one on a second run and grew it, Electrical Electrical
        /// Fixtures, which is the loop the class doc says safe to run twice is there to stop.
        /// </summary>
        [Test]
        public void ACategoryRewriteWhoseNewValueHoldsTheOldOneChangesNothingTheSecondTime()
        {
            IList<CategoryRewrite> rewrite = new List<CategoryRewrite> { new CategoryRewrite("S", "Fixtures", "Electrical Fixtures") };

            CorrectionOutcome once = MatrixCorrections.Apply(Set("S", "Fixtures"), null, rewrite);
            CorrectionOutcome twice = MatrixCorrections.Apply(once.Text, null, rewrite);

            Assert.That(once.TotalChanged, Is.EqualTo(1));
            Assert.That(twice.TotalChanged, Is.EqualTo(0));
            Assert.That(twice.Text, Is.EqualTo(once.Text));
            Assert.That(twice.Text, Does.Contain("<data type=\"wstring\">Electrical Fixtures</data>"));
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

            // The rename finds no broken name and the set asks for something other than the
            // old value, which is all the text can say, F116.
            Assert.That(twice.Lines()[twice.Lines().Count - 1],
                Is.EqualTo("MATRIX   no correction was applied to this file: 2 found nothing in it to change"));
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
        /// The corrections the committed file is made with, which since F116 are the ones the
        /// tool applies to a picked file: the hyphen 121 times, the catch all set rewritten
        /// into seven conditions, one contains and six negated equals, the workset values,
        /// and the four AR sets given Source File contains -AR-.
        /// </summary>
        [Test]
        public void TheCatchAllSetIsRewrittenIntoOneContainsAndSixNegations()
        {
            CorrectionOutcome outcome = Picked(Read(Samples.Matrix()));

            Assert.That(outcome.Counts[0].Count, Is.EqualTo(121), "the hyphen, measured");
            Assert.That(outcome.Counts[1].Count, Is.EqualTo(7), "one contains and six negated");

            // Q68 on 2026-09-20 and Q102 on 2026-10-04. Four workset values the C02 models
            // spell in title case and the C06 models also spell as the matrix does, asked in
            // both spellings: ME-DUCTWORK 5 conditions, ME-PIPING 5, PL-Domestic Water 6 and
            // ME-EQUIPMENT 1. And FF-FIRE FIGHTING, 2 conditions, corrected to FF-Fire
            // Fighting, the one spelling a model was measured carrying, 1B06PK on C06.
            Assert.That(ValuesChangedIn(outcome), Is.EqualTo(19), "the worksets, measured off the models");
            Assert.That(outcome.TotalChanged, Is.EqualTo(151),
                "121 hyphens, 7 conditions, 19 workset values and 4 Source File conditions");

            Assert.That(outcome.Text, Does.Contain("<condition test=\"contains\" flags=\"0\">"));
            Assert.That(outcome.Text, Does.Contain("<condition test=\"equals\" flags=\"32\">"));
        }

        /// <summary>
        /// What the measured spellings do to the client's own matrix since Q102. The four
        /// values the buildings spell two ways are asked in both, every condition asking one
        /// now asking each, the one value a single building spells another way is corrected
        /// to that, and the two the models spell exactly as the matrix does are left alone.
        /// Until C06 was measured the four were corrected to the C02 spelling, and the sets
        /// then found nothing in the C06 buildings writing capitals, FR-008.
        /// </summary>
        [Test]
        public void TheWorksetValuesTheBuildingsSpellTwoWaysAreAskedInBothAndTheRestAsBefore()
        {
            CorrectionOutcome outcome = Picked(Read(Samples.Matrix()));

            string said = string.Join("\n", new List<string>(outcome.Lines()).ToArray());

            Assert.That(said, Does.Contain("the value ME-DUCTWORK is asked as ME-DUCTWORK or ME-Ductwork"));
            Assert.That(said, Does.Contain("the value ME-PIPING is asked as ME-PIPING or ME-Piping"));
            Assert.That(said, Does.Contain("the value ME-EQUIPMENT is asked as ME-EQUIPMENT or ME-Equipment"));
            Assert.That(said, Does.Contain("the value PL-Domestic Water is asked as PL-Domestic Water or PL-Domestic water"));
            Assert.That(said, Does.Contain("the value FF-FIRE FIGHTING becomes FF-Fire Fighting"));

            Assert.That(said, Does.Contain("the value FP-PIPING is left alone"));
            Assert.That(said, Does.Contain("the value PL-Drainage is left alone"));

            // Every one of the 5 conditions asking it is now there in both spellings.
            Assert.That(Occurrences(outcome.Text, "<data type=\"wstring\">ME-DUCTWORK</data>"), Is.EqualTo(5));
            Assert.That(Occurrences(outcome.Text, "<data type=\"wstring\">ME-Ductwork</data>"), Is.EqualTo(5));
            Assert.That(Occurrences(outcome.Text, "<data type=\"wstring\">PL-Domestic Water</data>"), Is.EqualTo(6));
            Assert.That(Occurrences(outcome.Text, "<data type=\"wstring\">PL-Domestic water</data>"), Is.EqualTo(6));
            Assert.That(outcome.Text, Does.Not.Contain("FF-FIRE FIGHTING"));
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
        /// byte for byte what the rule produces from the file under samples, and since F116
        /// the rule is the one the tool applies to a picked file, Q104, so the file and the
        /// tool cannot drift apart either. Since Q113 the sample is read with this project's
        /// list, the one beside the corrected file in the exchange folder. If anyone edits the
        /// file or the list by hand, this fails.
        /// </summary>
        [Test]
        public void TheCorrectedFileIsExactlyWhatTheRuleProduces()
        {
            string source = Read(Samples.Matrix());
            string committed = Read(Samples.CorrectedMatrix());

            Assert.That(Picked(source).Text, Is.EqualTo(committed));
        }

        [Test]
        public void TheCorrectedFileCarriesNeitherFaultAndRunningItAgainChangesNothing()
        {
            string committed = Read(Samples.CorrectedMatrix());

            Assert.That(committed, Does.Not.Contain(BrokenName));
            Assert.That(committed, Does.Contain(CorrectName));

            CorrectionOutcome again = Picked(committed);

            Assert.That(again.TotalChanged, Is.EqualTo(0),
                "a second run reading zero is what proves the corrections are idempotent");
            Assert.That(again.Text, Is.EqualTo(committed));
            Assert.That(again.Lines()[again.Lines().Count - 1], Is.EqualTo(
                "MATRIX   no correction was applied to this file: 6 were already made in it and 4 found nothing in it to change"));
        }

        // ---------- Q104, the corrections applied to whichever file is picked ----------

        /// <summary>
        /// Every set the way the add-in reads it, the name, the folders and every condition
        /// in order with its test, flags, category, property and value, and every test with
        /// its sides, one line each, so two files can be compared set for set.
        /// </summary>
        private static List<string> WhatItAsks(ExchangeDocument document)
        {
            List<string> asks = new List<string>();

            foreach (SelectionSetDefinition set in document.Sets)
            {
                StringBuilder one = new StringBuilder("SET " + set.Path);

                foreach (SearchConditionDefinition condition in set.Conditions)
                {
                    one.Append(" | ").Append(condition.Test).Append(' ').Append(condition.Flags)
                        .Append(' ').Append(condition.Category == null ? "-" : condition.Category.InternalName)
                        .Append(' ').Append(condition.Property == null ? "-" : condition.Property.InternalName)
                        .Append(' ').Append(condition.Value == null ? "-" : condition.Value.Data);
                }

                asks.Add(one.ToString());
            }

            foreach (ClashTestDefinition test in document.Tests)
            {
                asks.Add("TEST " + test.Name + " | " + test.Left.Locator + " | " + test.Right.Locator
                    + " | " + test.ToleranceInFileUnits.ToString("R", System.Globalization.CultureInfo.InvariantCulture));
            }

            return asks;
        }

        /// <summary>
        /// THE TEST BADER ASKED FOR, Q104 on 2026-10-04: the old uncorrected XML and the
        /// exchange file give the same sets once corrected. Both are read the way the tool
        /// reads a picked file, and every set, condition for condition, and every test come
        /// out the same. The file picked before F116 is the third: Bader's old matrix is the
        /// sample with the hyphen alone corrected, measured on 2026-10-04 by applying that one
        /// rename to the sample and comparing the two files, 1,443,383 bytes each and sha256
        /// 36ab2739 both, byte for byte, and it comes out the same too. Since Q113 each is read
        /// with this project's list beside it, out of the exchange folder, as Bader keeps them:
        /// the exchange file has it beside it there, and the other two are copied with it.
        /// </summary>
        [Test]
        public void TheOldUncorrectedMatrixAndTheExchangeFileGiveTheSameSetsOnceCorrected()
        {
            string folder = TempFolder.Make("f116-picked");

            try
            {
                string sample = PickedCopy(folder, Samples.Matrix(), true);
                string hyphenOnly = Path.Combine(folder, "hyphen-only.xml");
                File.WriteAllText(hyphenOnly, MatrixCorrections.Apply(Read(Samples.Matrix()), ProjectRenames(), null).Text, new UTF8Encoding(false));
                File.Copy(Samples.CorrectionList(), ListBeside(hyphenOnly));

                List<string> fromTheSample = WhatItAsks(MatrixCorrections.ReadPicked(sample));
                List<string> fromTheExchange = WhatItAsks(MatrixCorrections.ReadPicked(Samples.CorrectedMatrix()));
                List<string> fromTheHyphenOnly = WhatItAsks(MatrixCorrections.ReadPicked(hyphenOnly));

                Assert.That(fromTheSample.Count, Is.EqualTo(61 + 1830));
                Assert.That(fromTheExchange, Is.EqualTo(fromTheSample));
                Assert.That(fromTheHyphenOnly, Is.EqualTo(fromTheSample));

                // And they are not the sets the uncorrected file asks as it stands.
                Assert.That(WhatItAsks(new ExchangeReader().ReadFile(Samples.Matrix())), Is.Not.EqualTo(fromTheSample));
            }
            finally
            {
                TempFolder.Remove(folder);
            }
        }

        /// <summary>
        /// Q104, the log names every correction it made: the lines ride on the document the
        /// tool reads, the list first, Q113, then one per correction and one for the total, and
        /// the run writes them.
        /// </summary>
        [Test]
        public void ThePickedFileCarriesALineForEveryCorrectionItMade()
        {
            string folder = TempFolder.Make("f116-lines");

            try
            {
                string sample = PickedCopy(folder, Samples.Matrix(), true);
                ExchangeDocument picked = MatrixCorrections.ReadPicked(sample);
                string said = string.Join("\n", new List<string>(picked.Corrections).ToArray());

                Assert.That(picked.SourcePath, Is.EqualTo(sample));
                Assert.That(said, Does.Contain("MATRIX   BLD-DRPipe Accessories to BLD-DR-Pipe Accessories  121 occurrences"));
                Assert.That(said, Does.Contain("MATRIX   BLD-EL-Devices asks for a category holding Devices and none of the 6 its siblings claim  7 occurrences"));
                Assert.That(said, Does.Contain("the value ME-DUCTWORK is asked as ME-DUCTWORK or ME-Ductwork"));
                Assert.That(said, Does.Contain("the value FF-FIRE FIGHTING becomes FF-Fire Fighting"));
                Assert.That(said, Does.Contain("BLD-AR-Ramps asks Source File contains -AR- as well"));
                Assert.That(said, Does.Contain("MATRIX   151 changes in all"));

                // The list, 1 rename, 1 catch-all, 7 workset values, 4 sets given Source File, the
                // line about sets already in an NWF and the total.
                Assert.That(picked.Corrections.Count, Is.EqualTo(16));
            }
            finally
            {
                TempFolder.Remove(folder);
            }

            ExchangeDocument already = MatrixCorrections.ReadPicked(Samples.CorrectedMatrix());

            Assert.That(already.Corrections[already.Corrections.Count - 1], Is.EqualTo(
                "MATRIX   no correction was applied to this file: 6 were already made in it and 4 found nothing in it to change"));
            Assert.That(new ExchangeReader().ReadFile(Samples.Matrix()).Corrections, Is.Empty, "a file read as it stands carries none");
        }

        /// <summary>
        /// A list that cannot be read corrects nothing and says so first, rather than passing
        /// the file on as one that needed nothing. One line it does not know is enough.
        /// </summary>
        [Test]
        public void AListThatCannotBeReadCorrectsNothingAndSaysSo()
        {
            MatrixCorrectionList broken = MatrixCorrectionList.Read(new StringReader("# a comment\nrename: " + BrokenName + "\n"), "a list in a test");
            string source = Read(Samples.Matrix());

            Assert.That(broken.Unread, Does.Contain("line 2"));

            CorrectionOutcome outcome = MatrixCorrections.ForPickedFile(source, broken, RevitWorksets.All());

            Assert.That(outcome.Text, Is.EqualTo(source));
            Assert.That(outcome.Lines()[0], Does.StartWith("MATRIX   NO CORRECTION WAS MADE TO THIS FILE"));
            Assert.That(outcome.Lines(), Has.None.Contains("already carries every correction"));
            Assert.That(outcome.Lines()[outcome.Lines().Count - 1],
                Is.EqualTo("MATRIX   no correction was applied to this file, for the reason the first line gives"));
        }

        /// <summary>
        /// A set whose text the correction cannot find, here one whose name is not its first
        /// attribute, is still read by the reader and so would be built uncorrected. That is
        /// said loudly and counted, never passed on in silence.
        /// </summary>
        [Test]
        public void ASetTheCorrectionCannotReadAsTextIsSaidLoudly()
        {
            string xml = WrittenExchange(
                WrittenSet("BLD-ME-Duct Accessory", CategoryAndWorkset(0, "Duct Accessories", "ME-DUCTWORK"))
                + WrittenSet("BLD-ME-Flex Ducts", CategoryAndWorkset(0, "Flex Ducts", "ME-DUCTWORK"))
                    .Replace("<selectionset name=\"BLD-ME-Flex Ducts\" guid=\"x\">", "<selectionset guid=\"x\" name=\"BLD-ME-Flex Ducts\">"));

            CorrectionOutcome outcome = Picked(xml);
            Dictionary<string, PlannedSet> sets = PlannedByName(outcome.Text);

            Assert.That(sets["BLD-ME-Duct Accessory"].GroupCount, Is.EqualTo(2), "the one it can read is corrected");
            Assert.That(sets["BLD-ME-Flex Ducts"].GroupCount, Is.EqualTo(1), "the one it cannot is left as the file asks");

            // The first line names the list, Q113, and this is the first after it.
            Assert.That(outcome.Lines()[1], Is.EqualTo(
                "MATRIX   NOT EVERY SET COULD BE READ FOR CORRECTION. The file holds 2 sets with conditions"
                    + " and 1 could be read as text, so 1 are built exactly as the file asks"));
        }

        /// <summary>
        /// This project's list, kept in the exchange folder beside the corrected XML and read
        /// the way the tool reads it off a picked file, Q113, holds F87's two decisions and
        /// Q103's measured categories, and those categories are exactly what the logs show: a
        /// set of the Architecture folder asking for its category alone found items in a group
        /// holding no AR model. Read off the logs themselves, never a copy of them.
        /// </summary>
        [Test]
        public void TheExchangeListHoldsTheDecisionsAndExactlyTheCategoriesTheLogsMeasured()
        {
            MatrixCorrectionList list = TheList();

            Assert.That(list.ListPath, Is.EqualTo(Samples.CorrectionList()));
            Assert.That(list.Unread, Is.Null);
            Assert.That(list.Renames.Count, Is.EqualTo(1));
            Assert.That(list.Renames[0].From, Is.EqualTo(BrokenName));
            Assert.That(list.Renames[0].To, Is.EqualTo(CorrectName));
            Assert.That(list.CatchAlls.Count, Is.EqualTo(1));
            Assert.That(list.CatchAlls[0], Is.EqualTo(new[] { DevicesSet, "Devices" }));
            Assert.That(list.SourceFiles.Count, Is.EqualTo(1));
            Assert.That(list.SourceFiles[0].Asks, Is.EqualTo("-AR-"));

            Dictionary<string, PlannedSet> matrix = PlannedByName(Read(Samples.Matrix()));
            List<string> measured = new List<string>();

            foreach (string log in new[]
            {
                Path.Combine(Samples.Repo(), "steps", "runs", "03", "item1-C06", "run-20261001-140037.log"),
                Path.Combine(Samples.Repo(), "steps", "logs", "c04-partial-run-20260921-085105.log")
            })
            {
                foreach (string category in FoundWithNoArModel(log, matrix))
                {
                    if (!measured.Contains(category))
                    {
                        measured.Add(category);
                    }
                }
            }

            Assert.That(list.SourceFiles[0].MeasuredElsewhere, Is.EquivalentTo(measured));
            Assert.That(measured, Is.EquivalentTo(MeasuredInOtherDisciplines));
        }

        /// <summary>
        /// The categories of the Architecture sets asking for one condition that found items
        /// in a group the log says holds no AR model.
        /// </summary>
        private static IList<string> FoundWithNoArModel(string log, Dictionary<string, PlannedSet> matrix)
        {
            List<string> categories = new List<string>();
            System.Text.RegularExpressions.Regex group = new System.Text.RegularExpressions.Regex(@"^ALIGNMENT (\S+)");
            System.Text.RegularExpressions.Regex found = new System.Text.RegularExpressions.Regex(
                @"SET\s+ok\s+lcop_selection_set_tree/Architecture/(.+?)  1 condition  (\d+) items?$");
            bool noArModel = false;

            foreach (string line in File.ReadAllLines(log))
            {
                if (group.IsMatch(line))
                {
                    noArModel = false;
                    continue;
                }

                if (line.IndexOf("this group carries no AR model", StringComparison.Ordinal) >= 0)
                {
                    noArModel = true;
                    continue;
                }

                System.Text.RegularExpressions.Match set = found.Match(line);

                if (noArModel && set.Success && int.Parse(set.Groups[2].Value) > 0)
                {
                    string category = matrix[set.Groups[1].Value].Conditions[0].Value;

                    if (!categories.Contains(category))
                    {
                        categories.Add(category);
                    }
                }
            }

            return categories;
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

        /// <summary>
        /// Q102 read off the committed file rather than off the rule that wrote it. Every
        /// set asking a workset the models carry in two spellings asks both, and every group
        /// of it still asks for exactly one category, FR-025. Seventeen set and category
        /// pairs ask one of the four worksets the buildings spell two ways: ME-DUCTWORK 5,
        /// ME-PIPING 5, ME-EQUIPMENT 1 and PL-Domestic Water 6.
        /// </summary>
        [Test]
        public void TheCommittedFileAsksEveryMeasuredSpellingWithItsCategoryInEveryGroup()
        {
            SetBuildPlan plan = SetBuildPlan.From(new ExchangeReader().ReadFile(Samples.CorrectedMatrix()));
            List<string> measured = new List<string>(RevitWorksets.All());

            foreach (string spelling in TheList().Worksets)
            {
                if (!measured.Contains(spelling))
                {
                    measured.Add(spelling);
                }
            }

            int askedInEverySpelling = 0;

            foreach (PlannedSet set in plan.Buildable)
            {
                Dictionary<string, List<string>> spellingsByCategory = new Dictionary<string, List<string>>(StringComparer.Ordinal);

                foreach (IList<PlannedCondition> group in set.Groups())
                {
                    List<string> worksets = ValuesOn(group, WorksetProperty);

                    if (worksets.Count == 0)
                    {
                        continue;
                    }

                    List<string> categories = ValuesOn(group, CategoryProperty);
                    Assert.That(categories.Count, Is.EqualTo(1), set.Name + ": " + set.Describe());

                    string key = categories[0] + "|" + worksets[0].ToLowerInvariant();

                    if (!spellingsByCategory.ContainsKey(key))
                    {
                        spellingsByCategory[key] = new List<string>();
                    }

                    spellingsByCategory[key].Add(worksets[0]);
                }

                foreach (List<string> asked in spellingsByCategory.Values)
                {
                    List<string> family = measured.FindAll(name => string.Equals(name, asked[0], StringComparison.OrdinalIgnoreCase));

                    if (family.Count > 1)
                    {
                        askedInEverySpelling++;
                        Assert.That(asked, Is.EquivalentTo(family), set.Name + ": " + set.Describe());
                    }
                }
            }

            Assert.That(askedInEverySpelling, Is.EqualTo(17));
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

            // A workset condition of a set, since F116 the only thing a value correction reads.
            string xml = WrittenExchange(WrittenSet("BLD-ME-Ducts", CategoryAndWorkset(0, "Ducts", "ME-DUCTWORK")));
            CorrectionOutcome outcome = MatrixCorrections.Apply(xml, null, null, null, rewrites);

            Assert.That(outcome.Text, Is.EqualTo(xml.Replace(
                "<data type=\"wstring\">ME-DUCTWORK</data>", "<data type=\"wstring\">ME-Ductwork</data>")));
        }

        /// <summary>
        /// Q102, answered by Bader on 2026-10-04, which replaced the refusal this test pinned
        /// until then. A value the models spell two ways was left exactly as it was, so the
        /// set found nothing in the buildings writing either of the others, which is what the
        /// C06 run showed for ME-DUCTWORK and ME-Ductwork. It no longer guesses and no longer
        /// refuses: the set asks every spelling a model was measured carrying, each group
        /// copied whole with its category, FR-025, and both are named in the log. A spelling
        /// no model carries, the matrix's own here, is not asked.
        /// </summary>
        [Test]
        public void AValueTheModelsSpellTwoWaysIsAskedInBothEachGroupWithItsCategory()
        {
            IList<ValueRewrite> rewrites = ValueRewrite.For(
                new[] { "EL-FIRE ALARM" },
                new[] { "EL-Fire Alarm", "EL-Fire alarm" });

            Assert.That(rewrites[0].Candidates.Count, Is.EqualTo(2));
            Assert.That(rewrites[0].To, Is.Null, "there is no one spelling to correct it to");
            Assert.That(rewrites[0].Corrects, Is.False);

            string xml = WrittenExchange(WrittenSet(
                "BLD-EL-Fire Alarm Devices", CategoryAndWorkset(0, "Fire Alarm Devices", "EL-FIRE ALARM")));
            CorrectionOutcome outcome = MatrixCorrections.Apply(xml, null, null, null, rewrites);

            PlannedSet set = PlannedOnly(outcome.Text);
            List<string> spellings = new List<string>();

            Assert.That(set.Groups().Count, Is.EqualTo(2), set.Describe());

            foreach (IList<PlannedCondition> group in set.Groups())
            {
                Assert.That(ValuesOn(group, CategoryProperty), Is.EqualTo(new[] { "Fire Alarm Devices" }), set.Describe());
                spellings.AddRange(ValuesOn(group, WorksetProperty));
            }

            Assert.That(spellings, Is.EqualTo(new[] { "EL-Fire Alarm", "EL-Fire alarm" }), "every measured spelling, in Ordinal order");
            Assert.That(Words(outcome), Does.Contain("the value EL-FIRE ALARM is asked as EL-Fire Alarm or EL-Fire alarm"));
            Assert.That(outcome.TotalChanged, Is.EqualTo(1));
        }

        /// <summary>
        /// Whichever spelling a file asks, the set comes out the same, condition for
        /// condition, so the old matrix and the corrected one build the same sets, Q104.
        /// </summary>
        [Test]
        public void EitherSpellingComesOutAsTheSameSet()
        {
            string[] measured = { "ME-Ductwork", "ME-DUCTWORK" };
            string capitals = WrittenExchange(WrittenSet("S", CategoryAndWorkset(0, "Ducts", "ME-DUCTWORK")));
            string titles = WrittenExchange(WrittenSet("S", CategoryAndWorkset(0, "Ducts", "ME-Ductwork")));

            CorrectionOutcome fromCapitals = MatrixCorrections.Apply(
                capitals, null, null, null, ValueRewrite.For(new[] { "ME-DUCTWORK" }, measured));
            CorrectionOutcome fromTitles = MatrixCorrections.Apply(
                titles, null, null, null, ValueRewrite.For(new[] { "ME-Ductwork" }, measured));

            Assert.That(fromTitles.Text, Is.EqualTo(fromCapitals.Text));

            CorrectionOutcome again = MatrixCorrections.Apply(
                fromCapitals.Text, null, null, null, ValueRewrite.For(new[] { "ME-DUCTWORK", "ME-Ductwork" }, measured));

            Assert.That(again.Text, Is.EqualTo(fromCapitals.Text), "the second run changes nothing");
            Assert.That(again.TotalChanged, Is.EqualTo(0));
            Assert.That(Words(again), Does.Contain("already asks every spelling"));
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
            Assert.That(Words(outcome), Does.Contain("no model measured so far in this project carries a workset spelled that way"));
            Assert.That(Words(outcome), Does.Contain("the models measured so far in this project spell it exactly as the matrix does"));
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

            string xml = WrittenExchange(
                WrittenSet("ME-PIPING MAINS", CategoryAndWorkset(0, "Pipes", "ME-PIPING MAINS"))
                + WrittenSet("BLD-ME-Pipes", CategoryAndWorkset(0, "Pipes", "ME-PIPING")));

            CorrectionOutcome outcome = MatrixCorrections.Apply(xml, null, null, null, rewrites);

            Assert.That(outcome.Text, Does.Contain("<selectionset name=\"ME-PIPING MAINS\""), "the set name is untouched");
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
                Does.Contain("the value ME-DUCTWORK becomes ME-Ductwork, the one spelling of it measured so far in this project's models"));
        }

        private static string Words(CorrectionOutcome outcome)
        {
            List<string> lines = new List<string>(outcome.Lines());
            return string.Join("\n", lines.ToArray());
        }

        /// <summary>
        /// The generator, run by hand when the rule, this project's list of corrections in the
        /// exchange folder or the measured workset list changes. It is a TEST and not a script
        /// so it reads the same samples and the same list the byte for byte test reads and can
        /// never produce something that test would then reject, and since F116 it writes what
        /// the tool makes of a picked file, Q104.
        /// </summary>
        [Test]
        [Explicit("Writes the corrected matrix into the exchange folder. Run by hand.")]
        public void WriteTheCorrectedFile()
        {
            CorrectionOutcome outcome = Picked(Read(Samples.Matrix()));

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
        /// <summary>
        /// A set asking one workset, the condition on the workset property, which since F116 is
        /// the only condition an Or row or a spelling correction reads.
        /// </summary>
        private static string WorksetSet(string name, string workset)
        {
            return WrittenExchange(WrittenSet(name, WrittenCondition(0, WorksetProperty, "Workset", workset)));
        }

        [Test]
        public void AWorksetSpelledTwoWaysBuildsAnOrRowCarryingBoth()
        {
            string xml = WorksetSet("BLD-ME-Ducts", "PL-Drainage equipment");

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
            string xml = WorksetSet("BLD-ME-Ducts", "ME-Ductwork");

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
                WorksetSet("BLD-ME-Ducts", "PL-Drainage equipment"), null, null, null, null, rows);

            CorrectionOutcome twice = MatrixCorrections.Apply(once.Text, null, null, null, null, rows);

            Assert.That(once.TotalChanged, Is.EqualTo(1), "the first run adds the row");
            Assert.That(twice.Text, Is.EqualTo(once.Text));
            Assert.That(twice.TotalChanged, Is.EqualTo(0));
        }

        // ---------- FR-025, the Or row copies its whole group ----------

        /// <summary>The category property's internal name, as the client file writes it.</summary>
        private const string CategoryProperty = "LcRevitPropertyElementCategory";

        /// <summary>
        /// One condition the way the client's matrix writes one, an element a line, so a
        /// group copied out of it is read back the way the file itself would be read.
        /// </summary>
        private static string WrittenCondition(int flags, string property, string display, string value)
        {
            return "            <condition test=\"equals\" flags=\"" + flags + "\">\n"
                + "              <category>\n"
                + "                <name internal=\"LcRevitData_Element\">Element</name>\n"
                + "              </category>\n"
                + "              <property>\n"
                + "                <name internal=\"" + property + "\">" + display + "</name>\n"
                + "              </property>\n"
                + "              <value>\n"
                + "                <data type=\"wstring\">" + value + "</data>\n"
                + "              </value>\n"
                + "            </condition>\n";
        }

        /// <summary>A set holding those conditions, inside a folder, the shape of the client's matrix.</summary>
        private static string WrittenSet(string name, string conditions)
        {
            return "        <selectionset name=\"" + name + "\" guid=\"x\">\n"
                + "          <findspec mode=\"all\" disjoint=\"0\">\n"
                + "            <conditions>\n"
                + conditions
                + "            </conditions>\n"
                + "            <locator>/</locator>\n"
                + "          </findspec>\n"
                + "        </selectionset>\n";
        }

        private static string WrittenExchange(string sets)
        {
            return "<?xml version='1.0' encoding='UTF-8'?>\n<exchange units=\"ft\">\n  <selectionsets>\n"
                + "    <viewfolder name=\"Mechanical\">\n" + sets + "    </viewfolder>\n"
                + "  </selectionsets>\n</exchange>\n";
        }

        /// <summary>A category and a workset in one group, the shape of BLD-ME-Duct Accessory.</summary>
        private static string CategoryAndWorkset(int flags, string category, string workset)
        {
            return WrittenCondition(flags, CategoryProperty, "Category", category)
                + WrittenCondition(0, WorksetProperty, "Workset", workset);
        }

        private static PlannedSet PlannedOnly(string xml)
        {
            return SetBuildPlan.From(new ExchangeReader().ReadText(xml)).Buildable[0];
        }

        /// <summary>The values one group asks on one property, in order.</summary>
        private static List<string> ValuesOn(IList<PlannedCondition> group, string property)
        {
            List<string> values = new List<string>();

            foreach (PlannedCondition condition in group)
            {
                if (string.Equals(condition.PropertyInternalName, property, StringComparison.Ordinal))
                {
                    values.Add(condition.Value);
                }
            }

            return values;
        }

        /// <summary>
        /// FR-025. The Or row was one flags 64 condition put straight after the workset, which
        /// starts a group holding only the workset, so a set of Duct Accessories on ME-Ductwork
        /// became (Duct Accessories and ME-Ductwork) or (ME-DUCTWORK), and the second group takes
        /// every element on that workset whatever its category. The Or row is the whole group
        /// copied with the other spelling, so each group still asks for its category.
        /// </summary>
        [Test]
        public void AnOrRowCopiesItsWholeGroupSoEveryGroupStillAsksForItsCategory()
        {
            string xml = WrittenExchange(WrittenSet("BLD-ME-Duct Accessory", CategoryAndWorkset(0, "Duct Accessories", "ME-Ductwork")));

            CorrectionOutcome outcome = MatrixCorrections.Apply(
                xml, null, null, null, null,
                new List<ValueOrRow> { new ValueOrRow("ME-Ductwork", "ME-DUCTWORK") });

            PlannedSet set = PlannedOnly(outcome.Text);
            IList<IList<PlannedCondition>> groups = set.Groups();
            List<string> spellings = new List<string>();

            Assert.That(groups.Count, Is.EqualTo(2), set.Describe());

            foreach (IList<PlannedCondition> group in groups)
            {
                Assert.That(ValuesOn(group, CategoryProperty), Is.EqualTo(new[] { "Duct Accessories" }),
                    "every group asks for the category: " + set.Describe());
                spellings.AddRange(ValuesOn(group, WorksetProperty));
            }

            Assert.That(spellings, Is.EquivalentTo(new[] { "ME-Ductwork", "ME-DUCTWORK" }), set.Describe());
            Assert.That(outcome.TotalChanged, Is.EqualTo(1), "one condition asked for the value");
        }

        /// <summary>
        /// FR-025 on a set that is already an Or, the shape of BLD-ME-Ducts&amp;Duct Fittings.
        /// Each of its two groups is copied with the other spelling, so the set asks four
        /// groups and every one of them still holds its own category, and applying the row
        /// again changes nothing.
        /// </summary>
        [Test]
        public void TheOrRowKeepsEveryGroupOfAnOrSetWithItsOwnCategory()
        {
            string xml = WrittenExchange(WrittenSet(
                "BLD-ME-Ducts&amp;Duct Fittings",
                CategoryAndWorkset(0, "Ducts", "ME-Ductwork") + CategoryAndWorkset(MatrixCorrections.StartGroup, "Duct Fittings", "ME-Ductwork")));

            IList<ValueOrRow> row = new List<ValueOrRow> { new ValueOrRow("ME-Ductwork", "ME-DUCTWORK") };
            CorrectionOutcome once = MatrixCorrections.Apply(xml, null, null, null, null, row);

            PlannedSet set = PlannedOnly(once.Text);
            IList<IList<PlannedCondition>> groups = set.Groups();
            List<string> asked = new List<string>();

            Assert.That(groups.Count, Is.EqualTo(4), set.Describe());

            foreach (IList<PlannedCondition> group in groups)
            {
                Assert.That(group.Count, Is.EqualTo(2), "a category and a workset in every group: " + set.Describe());
                asked.Add(ValuesOn(group, CategoryProperty)[0] + " on " + ValuesOn(group, WorksetProperty)[0]);
            }

            Assert.That(asked, Is.EquivalentTo(new[]
            {
                "Ducts on ME-Ductwork", "Ducts on ME-DUCTWORK", "Duct Fittings on ME-Ductwork", "Duct Fittings on ME-DUCTWORK"
            }));

            CorrectionOutcome twice = MatrixCorrections.Apply(once.Text, null, null, null, null, row);

            Assert.That(twice.Text, Is.EqualTo(once.Text));
            Assert.That(twice.TotalChanged, Is.EqualTo(0));
        }

        // ---------- Q103, the Source File condition of the AR sets ----------

        /// <summary>
        /// The categories measured in another discipline's models that no other folder's set
        /// asks for, as sample data: in groups holding no AR model, BLD-AR-Ramps found 36
        /// items in 1B06PK and 17 in 1C06PK and BLD-AR-Railings 18 in 1B06PK, set 03 log lines
        /// 4304, 7979 and 4316, and BLD-AR-Furniture 29 and BLD-AR-Site 18 in 1A0415 of the
        /// partial C04 run of 2026-09-21, its log lines 200 and 203. Those two are the landscape
        /// models' items in a group with no AR model, and Bader kept all four, Q113 answered D on
        /// 2026-10-04: an AR set only takes items from an AR file, and a set left empty in
        /// a landscape group shows on the coverage sheet of wave 2.
        /// </summary>
        private static readonly string[] MeasuredInOtherDisciplines = { "Ramps", "Railings", "Furniture", "Site" };

        private const string SourceFileProperty = "LcOaNodeSourceFile";

        /// <summary>A Source File condition the way the client's matrix writes one, with no category element.</summary>
        private static string SourceFileCondition(string asks)
        {
            return "            <condition test=\"contains\" flags=\"0\">\n"
                + "              <property>\n"
                + "                <name internal=\"" + SourceFileProperty + "\">Source File</name>\n"
                + "              </property>\n"
                + "              <value>\n"
                + "                <data type=\"wstring\">" + asks + "</data>\n"
                + "              </value>\n"
                + "            </condition>\n";
        }

        private static string Category(string category)
        {
            return WrittenCondition(0, CategoryProperty, "Category", category);
        }

        private static string Folders(string architecture, string structure)
        {
            return "<?xml version='1.0' encoding='UTF-8'?>\n<exchange units=\"ft\">\n  <selectionsets>\n"
                + "    <viewfolder name=\"Architecture\">\n" + architecture + "    </viewfolder>\n"
                + "    <viewfolder name=\"Structure\">\n" + structure + "    </viewfolder>\n"
                + "  </selectionsets>\n</exchange>\n";
        }

        private static IList<SourceFileRule> AskAr(params string[] measured)
        {
            return new List<SourceFileRule> { new SourceFileRule("-AR-", measured) };
        }

        private static CorrectionOutcome WithSourceFile(string xml, IList<SourceFileRule> rules)
        {
            return MatrixCorrections.Apply(xml, null, null, null, null, null, rules);
        }

        /// <summary>Every set of that file by name, read back the way the add-in plans it.</summary>
        private static Dictionary<string, PlannedSet> PlannedByName(string xml)
        {
            Dictionary<string, PlannedSet> sets = new Dictionary<string, PlannedSet>(StringComparer.Ordinal);

            foreach (PlannedSet set in SetBuildPlan.From(new ExchangeReader().ReadText(xml)).Buildable)
            {
                sets[set.Name] = set;
            }

            return sets;
        }

        private static string FloorsAskingAr()
        {
            return WrittenSet("BLD-AR-Floors", Category("Floors") + SourceFileCondition("-AR-"));
        }

        /// <summary>
        /// Q103 read off the matrix. A set beside the ones asking Source File contains -AR-
        /// asks it too where a set of another folder asks for its category, Walls here, and a
        /// set whose category nobody else asks is left alone. The Structure set is never given
        /// the AR condition, because it is not beside the sets asking it.
        /// </summary>
        [Test]
        public void ASetBesideTheOnesAskingSourceFileAsksItWhereAnotherFolderAsksItsCategory()
        {
            string xml = Folders(
                FloorsAskingAr() + WrittenSet("BLD-AR-Walls", Category("Walls")) + WrittenSet("BLD-AR-Doors", Category("Doors")),
                WrittenSet("BLD-ST-Walls", Category("Walls") + SourceFileCondition("-ST-")));

            CorrectionOutcome outcome = WithSourceFile(xml, AskAr());
            Dictionary<string, PlannedSet> sets = PlannedByName(outcome.Text);

            Assert.That(sets["BLD-AR-Walls"].Describe(), Is.EqualTo(
                "LcRevitData_Element/LcRevitPropertyElementCategory (Category) equals \"Walls\""
                    + " and LcOaNodeSourceFile (Source File) contains \"-AR-\""));
            Assert.That(sets["BLD-AR-Doors"].ConditionCount, Is.EqualTo(1), "nobody else asks for Doors");
            Assert.That(sets["BLD-ST-Walls"].Describe(), Does.Not.Contain("-AR-"));
            Assert.That(sets["BLD-AR-Floors"].ConditionCount, Is.EqualTo(2), "it asks it already");
            Assert.That(Words(outcome), Does.Contain(
                "BLD-AR-Walls asks Source File contains -AR- as well, because a set in another folder also asks for Walls"));
            Assert.That(outcome.TotalChanged, Is.EqualTo(1));
        }

        /// <summary>
        /// Q103 for a category no other folder's set asks for and another discipline's models
        /// carry, which only a measurement can say, Ramps in 1B06PK of set 03.
        /// </summary>
        [Test]
        public void ACategoryMeasuredInAnotherDisciplinesModelsGetsItToo()
        {
            string xml = Folders(
                FloorsAskingAr() + WrittenSet("BLD-AR-Ramps", Category("Ramps")) + WrittenSet("BLD-AR-Doors", Category("Doors")),
                WrittenSet("BLD-ST-Framing", Category("Structural Framing")));

            CorrectionOutcome outcome = WithSourceFile(xml, AskAr("Ramps"));
            Dictionary<string, PlannedSet> sets = PlannedByName(outcome.Text);

            Assert.That(sets["BLD-AR-Ramps"].Describe(), Does.EndWith("LcOaNodeSourceFile (Source File) contains \"-AR-\""));
            Assert.That(sets["BLD-AR-Doors"].ConditionCount, Is.EqualTo(1));
            Assert.That(Words(outcome), Does.Contain("because another discipline's models were measured carrying Ramps"));
        }

        /// <summary>A set that is an Or asks it in every group, so no group of it finds another discipline's items.</summary>
        [Test]
        public void EveryGroupOfAnOrSetAsksIt()
        {
            string xml = Folders(
                FloorsAskingAr() + WrittenSet(
                    "BLD-AR-Stairs&amp;Ramps",
                    Category("Stairs") + WrittenCondition(MatrixCorrections.StartGroup, CategoryProperty, "Category", "Ramps")),
                WrittenSet("BLD-ST-Stair", Category("Stairs") + SourceFileCondition("-ST-")));

            CorrectionOutcome once = WithSourceFile(xml, AskAr("Ramps"));
            PlannedSet set = PlannedByName(once.Text)["BLD-AR-Stairs&Ramps"];

            Assert.That(set.Groups().Count, Is.EqualTo(2), set.Describe());

            foreach (IList<PlannedCondition> group in set.Groups())
            {
                Assert.That(ValuesOn(group, SourceFileProperty), Is.EqualTo(new[] { "-AR-" }), set.Describe());
            }

            Assert.That(once.TotalChanged, Is.EqualTo(2), "one condition in each of the two groups");
            Assert.That(WithSourceFile(once.Text, AskAr("Ramps")).Text, Is.EqualTo(once.Text), "the second run changes nothing");
        }

        [Test]
        public void AFileWhereNoSetAsksItSaysSoAndChangesNothing()
        {
            string xml = Folders(WrittenSet("BLD-AR-Ramps", Category("Ramps")), WrittenSet("BLD-ST-Walls", Category("Walls")));
            CorrectionOutcome outcome = WithSourceFile(xml, AskAr("Ramps"));

            Assert.That(outcome.Text, Is.EqualTo(xml));
            Assert.That(Words(outcome), Does.Contain("no set asks for -AR-"));
        }

        /// <summary>
        /// FR-009 on the client's own matrix. The four AR sets asking a category another
        /// discipline's models were measured carrying ask Source File contains -AR- and no
        /// other set changes at all: Floors, Stairs and Walls asked it already, and every
        /// other AR category is asked by no other folder's set and was not seen finding items
        /// in a group with no AR model in the two logs read, set 03 on C06 and the partial C04
        /// run. A category another discipline carries only in a group that also holds an AR
        /// model cannot be told apart by a set count, so for the nine AR sets left without the
        /// condition, Roofs, Ceilings, Columns, Windows, Curtain Panels, Curtain Mullions,
        /// Doors, Casework and Parking, whether another discipline uses the category is
        /// UNKNOWN. A second run changes nothing.
        /// </summary>
        [Test]
        public void OnTheClientsMatrixTheFourArSetsAnotherDisciplineUsesAskSourceFileAndNoOtherSetChanges()
        {
            string source = Read(Samples.Matrix());
            Dictionary<string, PlannedSet> before = PlannedByName(source);

            CorrectionOutcome outcome = WithSourceFile(source, AskAr(MeasuredInOtherDisciplines));
            Dictionary<string, PlannedSet> after = PlannedByName(outcome.Text);
            List<string> changed = new List<string>();

            foreach (KeyValuePair<string, PlannedSet> one in before)
            {
                if (!string.Equals(one.Value.Describe(), after[one.Key].Describe(), StringComparison.Ordinal))
                {
                    changed.Add(one.Key);
                    Assert.That(after[one.Key].Describe(), Is.EqualTo(
                        one.Value.Describe() + " and LcOaNodeSourceFile (Source File) contains \"-AR-\""), one.Key);
                }
            }

            Assert.That(changed, Is.EquivalentTo(new[] { "BLD-AR-Ramps", "BLD-AR-Furniture", "BLD-AR-Railings", "BLD-AR-Site" }));
            Assert.That(outcome.TotalChanged, Is.EqualTo(4));

            CorrectionOutcome again = WithSourceFile(outcome.Text, AskAr(MeasuredInOtherDisciplines));

            Assert.That(again.Text, Is.EqualTo(outcome.Text));
            Assert.That(again.TotalChanged, Is.EqualTo(0));
            Assert.That(Words(again), Does.Contain("asks it already"));
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
            IList<string> asked = MatrixCorrections.WorksetValuesIn(new ExchangeReader().ReadFile(Samples.CorrectedMatrix()));

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

        // ---------- a negation is never widened, and only a workset is, F116 ----------

        /// <summary>Another property of the Element tab, sample data for a value that equals a workset spelling and is not a workset.</summary>
        private const string CommentsProperty = "lcldrevit_parameter_-1010106";

        private static IList<ValueRewrite> DuctworkBothWays()
        {
            return ValueRewrite.For(new[] { "ME-DUCTWORK" }, new[] { "ME-DUCTWORK", "ME-Ductwork" });
        }

        /// <summary>
        /// A negated workset condition asked in a second spelling takes the whole category:
        /// (Ducts and not ME-DUCTWORK) or (Ducts and not ME-Ductwork) is every duct, because no
        /// element sits on both spellings. So a condition carrying the negate flag is never
        /// asked in another spelling, the set comes out exactly as the file asks, and the log
        /// says it was left.
        /// </summary>
        [Test]
        public void ANegatedWorksetConditionIsNeverAskedInASecondSpelling()
        {
            string xml = WrittenExchange(WrittenSet(
                "BLD-ME-Ducts off the ductwork",
                Category("Ducts") + WrittenCondition(MatrixCorrections.NegateCondition, WorksetProperty, "Workset", "ME-DUCTWORK")));

            CorrectionOutcome outcome = MatrixCorrections.Apply(xml, null, null, null, DuctworkBothWays());

            Assert.That(outcome.Text, Is.EqualTo(xml), "the negation is left as the file asks");
            Assert.That(outcome.TotalChanged, Is.EqualTo(0));
            Assert.That(MatrixCorrections.WorksetValuesIn(new ExchangeReader().ReadText(xml)), Is.Empty,
                "a value asked only negated is not one the corrections act on");

            CorrectionOutcome picked = Picked(xml);

            Assert.That(picked.Text, Is.EqualTo(xml));
            Assert.That(Words(picked), Does.Contain(
                "1 workset condition carries the negate flag and is left exactly as the file asks"));
        }

        /// <summary>
        /// Widening reads the property as well as the value. A condition on another property
        /// whose value happens to be a workset spelling is not a workset and is never asked in a
        /// second spelling, where the workset condition of the set beside it is.
        /// </summary>
        [Test]
        public void OnlyAConditionOnTheWorksetPropertyIsAskedInEverySpelling()
        {
            string xml = WrittenExchange(
                WrittenSet("BLD-ME-Ducts", CategoryAndWorkset(0, "Ducts", "ME-DUCTWORK"))
                + WrittenSet("BLD-ME-Noted", Category("Ducts") + WrittenCondition(0, CommentsProperty, "Comments", "ME-DUCTWORK")));

            CorrectionOutcome outcome = MatrixCorrections.Apply(xml, null, null, null, DuctworkBothWays());
            Dictionary<string, PlannedSet> sets = PlannedByName(outcome.Text);

            Assert.That(sets["BLD-ME-Ducts"].GroupCount, Is.EqualTo(2), sets["BLD-ME-Ducts"].Describe());
            Assert.That(sets["BLD-ME-Noted"].GroupCount, Is.EqualTo(1), sets["BLD-ME-Noted"].Describe());
            Assert.That(sets["BLD-ME-Noted"].Describe(), Does.Not.Contain("ME-Ductwork"));
            Assert.That(outcome.TotalChanged, Is.EqualTo(1), "the one workset condition");
        }

        /// <summary>
        /// The one spelling correction of Q68 keeps the same two rules: it rewrites a workset
        /// condition that is not negated, and leaves a negated one and a condition on another
        /// property exactly as the file asks.
        /// </summary>
        [Test]
        public void TheOneSpellingCorrectionTouchesOnlyAWorksetConditionThatIsNotNegated()
        {
            string xml = WrittenExchange(
                WrittenSet("BLD-FF-Pipes", CategoryAndWorkset(0, "Pipes", "FF-FIRE FIGHTING"))
                + WrittenSet("BLD-FF-Noted", Category("Pipes") + WrittenCondition(0, CommentsProperty, "Comments", "FF-FIRE FIGHTING"))
                + WrittenSet("BLD-FF-Others", Category("Pipes")
                    + WrittenCondition(MatrixCorrections.NegateCondition, WorksetProperty, "Workset", "FF-FIRE FIGHTING")));

            CorrectionOutcome outcome = MatrixCorrections.Apply(
                xml, null, null, null, ValueRewrite.For(new[] { "FF-FIRE FIGHTING" }, new[] { "FF-Fire Fighting" }));
            Dictionary<string, PlannedSet> sets = PlannedByName(outcome.Text);

            Assert.That(ValuesOn(sets["BLD-FF-Pipes"].Groups()[0], WorksetProperty), Is.EqualTo(new[] { "FF-Fire Fighting" }));
            Assert.That(ValuesOn(sets["BLD-FF-Noted"].Groups()[0], CommentsProperty), Is.EqualTo(new[] { "FF-FIRE FIGHTING" }));
            Assert.That(ValuesOn(sets["BLD-FF-Others"].Groups()[0], WorksetProperty), Is.EqualTo(new[] { "FF-FIRE FIGHTING" }));
            Assert.That(outcome.TotalChanged, Is.EqualTo(1));
        }

        // ---------- one escape, one way to edit a condition, one lookup, F116 ----------

        /// <summary>
        /// The catch-all finds its set by the name as the file writes it, escapes and all, the
        /// way the Source File rule finds its sets, and every value it builds is written with
        /// the escapes an XML file needs. Before, it looked for the raw name, so a set whose
        /// name held an ampersand was never found, and it wrote a category holding one raw,
        /// which no reader reads.
        /// </summary>
        [Test]
        public void TheCatchAllFindsASetWhoseNameHoldsAnAmpersandAndEscapesWhatItWrites()
        {
            string xml = WrittenExchange(WrittenSet("BLD-EL-Data&amp;Comms", Category("Electrical Fixtures")));

            CorrectionOutcome outcome = MatrixCorrections.Apply(xml, null, null, new List<ConditionsRewrite>
            {
                new ConditionsRewrite("BLD-EL-Data&Comms", "Devices", new[] { "Data & Comms Devices" })
            });

            Assert.That(outcome.TotalChanged, Is.EqualTo(2), Words(outcome));

            IList<SearchConditionDefinition> asked = new ExchangeReader().ReadText(outcome.Text).Sets[0].Conditions;

            Assert.That(asked.Count, Is.EqualTo(2));
            Assert.That(asked[0].Test, Is.EqualTo("contains"));
            Assert.That(asked[0].Value.Data, Is.EqualTo("Devices"));
            Assert.That(asked[1].Flags, Is.EqualTo(MatrixCorrections.NegateCondition));
            Assert.That(asked[1].Value.Data, Is.EqualTo("Data & Comms Devices"));
        }

        /// <summary>
        /// The escape on its own, on a set the old lookup found: a category holding an
        /// ampersand was written raw and the corrected file no longer read as XML.
        /// </summary>
        [Test]
        public void TheCatchAllEscapesTheValuesItWrites()
        {
            string xml = WrittenExchange(WrittenSet("BLD-EL-Devices", Category("Electrical Fixtures")));

            CorrectionOutcome outcome = MatrixCorrections.Apply(xml, null, null, new List<ConditionsRewrite>
            {
                new ConditionsRewrite("BLD-EL-Devices", "Devices", new[] { "Data & Comms Devices" })
            });

            Assert.That(
                new ExchangeReader().ReadText(outcome.Text).Sets[0].Conditions[1].Value.Data,
                Is.EqualTo("Data & Comms Devices"));
        }

        /// <summary>
        /// A category rewrite finds its set the same way and edits its condition the same way,
        /// so the new value is escaped and the name is matched as the file writes it.
        /// </summary>
        [Test]
        public void ACategoryRewriteFindsASetWhoseNameHoldsAnAmpersandAndEscapesTheNewValue()
        {
            string xml = WrittenExchange(WrittenSet("BLD-AR-Doors&amp;Windows", Category("Doors")));

            CorrectionOutcome outcome = MatrixCorrections.Apply(
                xml, null, new List<CategoryRewrite> { new CategoryRewrite("BLD-AR-Doors&Windows", "Doors", "Doors & Windows") });

            Assert.That(outcome.TotalChanged, Is.EqualTo(1), Words(outcome));
            Assert.That(new ExchangeReader().ReadText(outcome.Text).Sets[0].Conditions[0].Value.Data, Is.EqualTo("Doors & Windows"));
        }

        // ---------- the picked file read as ReadFile read it, F116 ----------

        /// <summary>
        /// ReadPicked reads a file the way ExchangeReader.ReadFile did, in the encoding the file
        /// declares. A file declared windows-1252 holding an accented set name, read as UTF-8,
        /// came out with a replacement character, a name no NWF holds, so its set would be built
        /// again beside the one already there.
        /// </summary>
        [Test]
        public void ThePickedFileIsReadInTheEncodingItDeclares()
        {
            string folder = TempFolder.Make("f116-encoding");

            try
            {
                string path = Path.Combine(folder, "declared-1252.xml");
                string xml = WrittenExchange(WrittenSet("BLD-AR-Façade", Category("Walls")))
                    .Replace("encoding='UTF-8'", "encoding='windows-1252'");

                File.WriteAllBytes(path, Encoding.GetEncoding(1252).GetBytes(xml));

                Assert.That(MatrixCorrections.ReadPicked(path).Sets[0].Name, Is.EqualTo("BLD-AR-Façade"));
            }
            finally
            {
                TempFolder.Remove(folder);
            }
        }

        /// <summary>
        /// A set whose conditions the corrections cannot read as text is counted and said, and
        /// the pick goes on, as ReadFile's did on the same file. Here a condition written as
        /// one empty tag, which the text walk read as running into the next and threw.
        /// </summary>
        [Test]
        public void ASetWithAnEmptyConditionTagIsCountedAndSaidAndNeverThrows()
        {
            SaidAndNotThrown(WrittenExchange(WrittenSet(
                "BLD-ME-Ducts",
                "            <condition test=\"equals\" flags=\"0\"/>\n" + CategoryAndWorkset(0, "Ducts", "ME-DUCTWORK"))));
        }

        /// <summary>
        /// The same for a value element under a namespace prefix, which the reader reads and the
        /// text walk cannot rewrite, so asking it in a second spelling threw.
        /// </summary>
        [Test]
        public void ASetWithAValueTheTextCannotRewriteIsCountedAndSaidAndNeverThrows()
        {
            SaidAndNotThrown(WrittenExchange(WrittenSet(
                "BLD-ME-Ducts",
                Category("Ducts")
                    + "            <condition test=\"equals\" flags=\"0\" xmlns:nw=\"urn:sample\">\n"
                    + "              <property>\n"
                    + "                <name internal=\"" + WorksetProperty + "\">Workset</name>\n"
                    + "              </property>\n"
                    + "              <value>\n"
                    + "                <nw:data type=\"wstring\">ME-DUCTWORK</nw:data>\n"
                    + "              </value>\n"
                    + "            </condition>\n")));
        }

        private static void SaidAndNotThrown(string xml)
        {
            CorrectionOutcome outcome = null;

            Assert.That(() => outcome = Picked(xml), Throws.Nothing);
            Assert.That(outcome.Text, Is.EqualTo(xml), "built exactly as the file asks");

            // The first line names the list, Q113, and this is the first after it.
            Assert.That(outcome.Lines()[1], Is.EqualTo(
                "MATRIX   NOT EVERY SET COULD BE READ FOR CORRECTION. The file holds 1 sets with conditions"
                    + " and 0 could be read as text, so 1 are built exactly as the file asks"));
        }

        /// <summary>
        /// Two sets of one name in two folders. The Architecture one is given the Source File
        /// condition and the Structure one is not. Keyed by its name alone the condition went to
        /// the first block of that name in the file, the Structure one, and the line said the
        /// Architecture one had changed.
        /// </summary>
        [Test]
        public void ASetIsGivenTheSourceFileConditionByItsFolderAndItsNameNeverItsNameAlone()
        {
            string xml = "<?xml version='1.0' encoding='UTF-8'?>\n<exchange units=\"ft\">\n  <selectionsets>\n"
                + "    <viewfolder name=\"Structure\">\n" + WrittenSet("Ramps", Category("Ramps") + SourceFileCondition("-ST-")) + "    </viewfolder>\n"
                + "    <viewfolder name=\"Architecture\">\n" + FloorsAskingAr() + WrittenSet("Ramps", Category("Ramps")) + "    </viewfolder>\n"
                + "  </selectionsets>\n</exchange>\n";

            CorrectionOutcome outcome = WithSourceFile(xml, AskAr());
            Dictionary<string, string> asked = new Dictionary<string, string>(StringComparer.Ordinal);

            foreach (SelectionSetDefinition set in new ExchangeReader().ReadText(outcome.Text).Sets)
            {
                List<string> values = new List<string>();

                foreach (SearchConditionDefinition condition in set.Conditions)
                {
                    values.Add(condition.Value.Data);
                }

                asked[set.Path] = string.Join(" and ", values.ToArray());
            }

            Assert.That(asked["lcop_selection_set_tree/Architecture/Ramps"], Is.EqualTo("Ramps and -AR-"));
            Assert.That(asked["lcop_selection_set_tree/Structure/Ramps"], Is.EqualTo("Ramps and -ST-"));
            Assert.That(outcome.TotalChanged, Is.EqualTo(1));
        }

        // ---------- the MATRIX lines claim only what was measured, F116 ----------

        /// <summary>
        /// The spellings are the C02 census inside Core and, in this project's list, at most the
        /// first ten names of each C06 group, so a line says every spelling measured so far in
        /// this project's models, and never every spelling the models carry. A value no measured
        /// name matches is said to be in no model measured so far, and never in no model of this
        /// run, which neither list can know.
        /// </summary>
        [Test]
        public void TheValueLinesClaimOnlyWhatWasMeasured()
        {
            string said = Words(Picked(Read(Samples.Matrix())));

            Assert.That(said, Does.Contain(
                "the value ME-DUCTWORK is asked as ME-DUCTWORK or ME-Ductwork, every spelling measured so far in this project's models"));
            Assert.That(said, Does.Contain(
                "the value FF-FIRE FIGHTING becomes FF-Fire Fighting, the one spelling of it measured so far in this project's models"));
            Assert.That(said, Does.Contain(
                "the value FP-PIPING is left alone  0 occurrences. the models measured so far in this project spell it exactly as the matrix does, and no other way"));
            Assert.That(said, Does.Not.Contain("the models in this project carry"));
            Assert.That(said, Does.Not.Contain("which is how the models spell it"));

            string none = Words(MatrixCorrections.Apply(
                WorksetSet("BLD-XX-Nowhere", "XX-NOWHERE"), null, null, null,
                ValueRewrite.For(new[] { "XX-NOWHERE" }, RevitWorksets.All())));

            Assert.That(none, Does.Contain("no model measured so far in this project carries a workset spelled that way but for its case"));
            Assert.That(none, Does.Not.Contain("in this run"));
        }

        /// <summary>
        /// A file no correction acts on says that none was applied and why, never that it
        /// already carries every correction. The Infra sets file holds none of the names, no
        /// workset condition and no Source File condition, and its last line said the opposite
        /// of every line above it.
        /// </summary>
        [Test]
        public void AFileNoCorrectionActsOnSaysNoneWasAppliedAndWhy()
        {
            IList<string> lines = Picked(Read(Samples.Infra())).Lines();

            Assert.That(lines[lines.Count - 1], Is.EqualTo(
                "MATRIX   no correction was applied to this file: 3 found nothing in it to change"));
            Assert.That(lines, Has.None.Contains("already carries every correction"));
        }

        /// <summary>
        /// The corrections reach the sets a run builds and not a set already in an NWF, which
        /// keeps the conditions it was built with unless the rebuild box is ticked, Q72. One line
        /// after the corrections says so and names the box and the SETS block, so a log naming
        /// corrections over an NWF built before them is not read as corrected sets. Written for
        /// every picked file, the one corrected, the one needing nothing, the one with no list
        /// beside it, here the sample where it sits, and the one whose list could not be read,
        /// just before the last line.
        /// </summary>
        [Test]
        public void ThePickedFileSaysASetAlreadyInAnNwfKeepsItsOldConditionsUnlessTheBoxIsTicked()
        {
            string expected = "MATRIX   a set already in an NWF keeps the conditions it was built with and is not given"
                + " what this file asks unless the box \"" + SetRebuildSettings.TickLabel + "\" is ticked."
                + " The SETS block of each group names every such set as DRIFTED";

            MatrixCorrectionList broken = MatrixCorrectionList.Read(new StringReader("not a correction\n"), "a list in a test");
            string folder = TempFolder.Make("f116-nwf-line");

            try
            {
                foreach (IList<string> lines in new List<IList<string>>
                {
                    MatrixCorrections.ReadPicked(PickedCopy(folder, Samples.Matrix(), true)).Corrections,
                    MatrixCorrections.ReadPicked(Samples.CorrectedMatrix()).Corrections,
                    MatrixCorrections.ReadPicked(Samples.Matrix()).Corrections,
                    MatrixCorrections.ForPickedFile(Read(Samples.Matrix()), broken, RevitWorksets.All()).Lines()
                })
                {
                    Assert.That(lines[lines.Count - 2], Is.EqualTo(expected));
                }
            }
            finally
            {
                TempFolder.Remove(folder);
            }
        }

        // ---------- the list beside the picked XML, Q113 ----------

        /// <summary>
        /// A copy of that XML in that folder, with this project's list out of the exchange folder
        /// copied beside it and named after it where asked, the way Bader keeps the two.
        /// </summary>
        private static string PickedCopy(string folder, string xml, bool withTheList)
        {
            string copy = Path.Combine(folder, Path.GetFileName(xml));
            File.Copy(xml, copy);

            if (withTheList)
            {
                File.Copy(Samples.CorrectionList(), ListBeside(copy));
            }

            return copy;
        }

        /// <summary>U+00A0, which one measured workset spelling carries between its words.</summary>
        private const char NoBreakSpace = (char)0xA0;

        /// <summary>The one path the list of that XML is looked for at, written out here as the rule says it.</summary>
        private static string ListBeside(string xml)
        {
            return Path.Combine(Path.GetDirectoryName(xml), Path.GetFileNameWithoutExtension(xml) + ".corrections.txt");
        }

        /// <summary>
        /// Q113 B. The list kept beside the picked XML and named after it is what corrects it,
        /// and the first MATRIX line names that list in full and what it holds, before any
        /// correction. The client's matrix with this project's list beside it gives the sets and
        /// tests of the exchange file.
        /// </summary>
        [Test]
        public void ThePickedFileIsCorrectedByTheListBesideItAndTheFirstLineNamesIt()
        {
            string folder = TempFolder.Make("f116-beside");

            try
            {
                string picked = PickedCopy(folder, Samples.Matrix(), true);
                ExchangeDocument document = MatrixCorrections.ReadPicked(picked);

                Assert.That(document.Corrections[0], Is.EqualTo(
                    "MATRIX   the corrections are read from " + ListBeside(picked) + ", the list beside this file."
                        + " It holds 3 corrections, 1 rename, 1 catch-all and 1 Source File rule, and 30 workset spellings"));
                Assert.That(document.Corrections[document.Corrections.Count - 1], Is.EqualTo("MATRIX   151 changes in all"));
                Assert.That(WhatItAsks(document), Is.EqualTo(WhatItAsks(new ExchangeReader().ReadFile(Samples.CorrectedMatrix()))));
            }
            finally
            {
                TempFolder.Remove(folder);
            }
        }

        /// <summary>
        /// Q113 B. A picked XML with no list beside it is corrected by nothing and goes on as
        /// written, and the first MATRIX line names the one path looked for. A list named any
        /// other way is never found, the name of another file or the XML's name with its
        /// extension kept, because the list is one full path and never a search.
        /// </summary>
        [Test]
        public void AnXmlWithNoListBesideItIsLeftAsWrittenAndTheLogSaysSo()
        {
            string folder = TempFolder.Make("f116-no-list");

            try
            {
                string picked = PickedCopy(folder, Samples.Matrix(), false);
                File.Copy(Samples.CorrectionList(), Path.Combine(folder, "another.corrections.txt"));
                File.Copy(Samples.CorrectionList(), picked + ".corrections.txt");

                ExchangeDocument document = MatrixCorrections.ReadPicked(picked);

                Assert.That(WhatItAsks(document), Is.EqualTo(WhatItAsks(new ExchangeReader().ReadFile(Samples.Matrix()))));
                Assert.That(document.Corrections[0], Is.EqualTo(
                    "MATRIX   no correction was made to this file, because no list of corrections is beside it: "
                        + ListBeside(picked) + " was looked for and is not there. Every set is built exactly as the file asks"));
                Assert.That(document.Corrections[document.Corrections.Count - 1], Is.EqualTo(
                    "MATRIX   no correction was applied to this file, for the reason the first line gives"));
            }
            finally
            {
                TempFolder.Remove(folder);
            }
        }

        /// <summary>
        /// Q113 B. A list beside the XML that cannot be read corrects nothing, the first line says
        /// so and why, and the pick goes on, never a throw. One line the list does not know is
        /// enough, and so is a list that is not UTF-8 text, whose no-break space would otherwise
        /// read as a replacement character and ask a spelling no model carries.
        /// </summary>
        [Test]
        public void AListBesideItThatCannotBeReadCorrectsNothingAndSaysWhy()
        {
            string folder = TempFolder.Make("f116-unread-list");

            try
            {
                string unknown = Path.Combine(folder, "unknown-line.xml");
                File.Copy(Samples.Matrix(), unknown);
                File.WriteAllText(
                    ListBeside(unknown),
                    "# a comment\nrename: " + BrokenName + " | " + CorrectName + "\nnot a correction\n",
                    new UTF8Encoding(false));

                string ansi = Path.Combine(folder, "not-utf8.xml");
                File.Copy(Samples.Matrix(), ansi);
                File.WriteAllBytes(ListBeside(ansi), Encoding.GetEncoding(1252).GetBytes("workset: EL-Fire" + NoBreakSpace + "alarm\n"));

                List<string> asWritten = WhatItAsks(new ExchangeReader().ReadFile(Samples.Matrix()));

                foreach (string[] one in new[]
                {
                    new[] { unknown, "line 3, \"not a correction\", is not a correction this tool knows" },
                    new[] { ansi, "it is not UTF-8 text" }
                })
                {
                    ExchangeDocument document = null;

                    Assert.That(() => document = MatrixCorrections.ReadPicked(one[0]), Throws.Nothing, one[0]);
                    Assert.That(WhatItAsks(document), Is.EqualTo(asWritten), one[0]);
                    Assert.That(document.Corrections[0], Is.EqualTo(
                        "MATRIX   NO CORRECTION WAS MADE TO THIS FILE, because the list of corrections beside it, "
                            + ListBeside(one[0]) + ", could not be read: " + one[1] + ". Every set is built exactly as the file asks"));
                }
            }
            finally
            {
                TempFolder.Remove(folder);
            }
        }

        /// <summary>
        /// The list's name is the XML's name without its extension and the suffix, a setting
        /// whose default is .corrections.txt, so the list in the exchange folder is the one the
        /// tool finds beside the corrected XML. A suffix set another way finds the list named
        /// that way and no other.
        /// </summary>
        [Test]
        public void TheListIsFoundByTheXmlsNameAndTheSuffixSetting()
        {
            Assert.That(CorrectionListSettings.DefaultSuffix, Is.EqualTo(".corrections.txt"));
            Assert.That(new CorrectionListSettings().PathBeside(Samples.CorrectedMatrix()), Is.EqualTo(Samples.CorrectionList()));

            string folder = TempFolder.Make("f116-suffix");

            try
            {
                string xml = Path.Combine(folder, "a.xml");
                File.WriteAllText(xml, "<exchange/>");
                File.WriteAllText(Path.Combine(folder, "a.fixes.txt"), "rename: X-A | Y-B\n");
                File.WriteAllText(Path.Combine(folder, "a.corrections.txt"), "not a correction\n");

                CorrectionListSettings settings = new CorrectionListSettings { Suffix = ".fixes.txt" };
                MatrixCorrectionList list = MatrixCorrectionList.Beside(xml, settings);

                Assert.That(settings.PathBeside(xml), Is.EqualTo(Path.Combine(folder, "a.fixes.txt")));
                Assert.That(list.ListPath, Is.EqualTo(Path.Combine(folder, "a.fixes.txt")));
                Assert.That(list.Unread, Is.Null);
                Assert.That(list.Renames.Count, Is.EqualTo(1));
                Assert.That(list.Renames[0].To, Is.EqualTo("Y-B"));
            }
            finally
            {
                TempFolder.Remove(folder);
            }
        }

        /// <summary>
        /// Q113 B, nothing of this project's list is inside Core: no list is embedded in the
        /// assembly, so a build of the tool carries no project's corrections.
        /// </summary>
        [Test]
        public void NoListOfCorrectionsIsInsideCore()
        {
            foreach (string resource in typeof(MatrixCorrections).Assembly.GetManifestResourceNames())
            {
                Assert.That(resource, Does.Not.Contain("correction"), resource);
            }
        }
    }
}
