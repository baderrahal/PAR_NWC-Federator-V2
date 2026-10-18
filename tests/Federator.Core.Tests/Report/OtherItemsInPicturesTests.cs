using System.Linq;
using Federator.Core.Report;
using NUnit.Framework;

namespace Federator.Core.Tests
{
    /// <summary>
    /// F150, Bader's answer to Q151: for the pictures the tool sets Clash Detective's Hide Other, on
    /// where the ghosted box is unticked and off where it is ticked, and puts the user's value back
    /// after, the user's value being the one the run's first group read. Each test breaks one thing
    /// and asserts the rule or the IMAGES line names it.
    /// </summary>
    [TestFixture]
    public class OtherItemsInPicturesTests
    {
        private static readonly string EmDash = ((char)0x2014).ToString();

        [Test]
        public void TheBoxIsOffByDefaultAndTheLogSaysWhatIsAskedNotWhatIsDrawn()
        {
            ImageOptions options = new ImageOptions();

            Assert.That(options.GhostTheOtherItems, Is.False, "Bader's order of 2026-10-12, the ghosted look is off by default");
            Assert.That(options.Describe(), Does.Contain("Clash Detective asked to hide the other items"));
            Assert.That(options.Describe(), Does.Not.Contain("only the two clashing items"), "nothing reads what the pictures show");

            options.GhostTheOtherItems = true;

            Assert.That(options.Describe(), Does.Contain("Clash Detective asked to show the other items"));
        }

        [Test]
        public void TheLabelIsShortPlainAndClaimsNothingUnmeasured()
        {
            Assert.That(OtherItemsInPictures.TickLabel.Split(' ').Length, Is.LessThanOrEqualTo(8));
            Assert.That(OtherItemsInPictures.TickLabel, Does.Not.Contain("GhostTheOtherItems"), "no code identifier in a label");

            foreach (string word in OtherItemsInPictures.TickLabel.Split(' '))
            {
                string letters = word.Trim(',', '.');
                bool shouted = letters.Length > 2 && letters == letters.ToUpperInvariant() && letters.Any(char.IsLetter);
                Assert.That(shouted, Is.False, "no capitals for emphasis, " + word);
            }

            Assert.That(OtherItemsInPictures.TickLabel, Does.Not.Contain(";"));
            Assert.That(OtherItemsInPictures.TickLabel, Does.Not.Contain(EmDash));
        }

        [Test]
        public void ReadOffItIsSetOnForThePicturesReadBackAndPutBack()
        {
            OtherItemsInPictures rule = new OtherItemsInPictures(false, new HideOtherAcrossTheRun());

            rule.Read(false);
            Assert.That(rule.Wanted, Is.True);
            Assert.That(rule.SetsIt, Is.True, "the user's off is set on for the pictures");
            Assert.That(rule.PutsBack, Is.False, "nothing is put back before a set was tried");

            rule.Set();
            rule.ReadBack(true);
            Assert.That(rule.PutsBack, Is.True);
            Assert.That(rule.PutBackTo, Is.False, "the user's off is what is written back");

            rule.PutBack();
            rule.ReadAfter(false);

            Assert.That(rule.Line(), Is.EqualTo(
                "IMAGES   Clash Detective's Hide Other asked on for the pictures, to hide the other items: it read off before "
                + "the pictures, set on, and it read back on, then put back to off. It read off after the pictures, the user's own "
                + "setting. Whether the pictures follow it is UNKNOWN until a run's picture sizes show it, near 48 KB a picture "
                + "with the other items hidden against about 550 KB with them drawn, docs\\history\\scan.md 5z-ze"));
        }

        [Test]
        public void ASetThatDidNotTakeIsSaidByTheReadBack()
        {
            OtherItemsInPictures rule = new OtherItemsInPictures(false, new HideOtherAcrossTheRun());
            rule.Read(false);
            rule.Set();
            rule.ReadBack(false);

            Assert.That(rule.Line(), Does.Contain("set on, and it read back off"), "a set the highlighter did not keep is named");
        }

        [Test]
        public void TheGhostedLookTickedSetsItOffAndPutsTheUsersOnBack()
        {
            OtherItemsInPictures rule = new OtherItemsInPictures(true, new HideOtherAcrossTheRun());

            rule.Read(true);
            Assert.That(rule.Wanted, Is.False);
            Assert.That(rule.SetsIt, Is.True, "a user with Hide Other on gets the ghosted look all the same");

            rule.Set();
            rule.ReadBack(false);
            Assert.That(rule.PutBackTo, Is.True, "the user's on is written back");

            rule.PutBack();
            rule.ReadAfter(true);
            string said = rule.Line();

            Assert.That(said, Does.Contain("asked off for the pictures, to show the other items, since the box \""
                + OtherItemsInPictures.TickLabel + "\" is ticked"));
            Assert.That(said, Does.Contain("set off, and it read back off, then put back to on"));
        }

        [Test]
        public void ReadAsAskedNothingIsSetOrPutBack()
        {
            OtherItemsInPictures rule = new OtherItemsInPictures(false, new HideOtherAcrossTheRun());

            rule.Read(true);
            Assert.That(rule.SetsIt, Is.False);
            Assert.That(rule.PutsBack, Is.False);

            rule.ReadAfter(true);

            Assert.That(rule.Line(), Does.Contain("it read on before the pictures, already on, so nothing was set"));
            Assert.That(rule.Line(), Does.Not.Contain("put back"));
            Assert.That(rule.Line(), Does.Not.Contain("Whether the pictures follow"), "said only where the tool set it");
        }

        [Test]
        public void AValueThatCouldNotBeReadIsNeverSetSinceItCouldNotBePutBack()
        {
            OtherItemsInPictures rule = new OtherItemsInPictures(false, new HideOtherAcrossTheRun());

            rule.NotRead("Object has been Disposed");

            Assert.That(rule.SetsIt, Is.False);
            Assert.That(rule.PutsBack, Is.False);

            rule.ReadAfter(false);
            string said = rule.Line();

            Assert.That(said, Does.Contain("could not be read before the pictures, so nothing was set"));
            Assert.That(said, Does.Contain("Object has been Disposed"), "the line names why");
        }

        [Test]
        public void APassThatTookNoPictureTouchesNothingAndSaysSo()
        {
            OtherItemsInPictures rule = new OtherItemsInPictures(false, new HideOtherAcrossTheRun());

            Assert.That(rule.SetsIt, Is.False);
            Assert.That(rule.PutsBack, Is.False);
            Assert.That(rule.Line(), Is.EqualTo(
                "IMAGES   Clash Detective's Hide Other not read or set, since no picture was taken in this group"));
        }

        /// <summary>The check that matters most: a put back that did not take is named, with where to set it by hand.</summary>
        [Test]
        public void ASettingLeftChangedAfterThePicturesIsNamedAndOwed()
        {
            HideOtherAcrossTheRun goodRun = new HideOtherAcrossTheRun();
            OtherItemsInPictures good = new OtherItemsInPictures(false, goodRun);
            good.Read(false);
            good.Set();
            good.PutBack();
            good.ReadAfter(false);

            HideOtherAcrossTheRun brokenRun = new HideOtherAcrossTheRun();
            OtherItemsInPictures broken = new OtherItemsInPictures(false, brokenRun);
            broken.Read(false);
            broken.Set();
            broken.PutBack();
            broken.ReadAfter(true);

            Assert.That(good.Line(), Does.Not.Contain("left changed"));
            Assert.That(goodRun.OwesAPutBack, Is.False);
            Assert.That(goodRun.TakeTheGroupError(), Is.Null);

            Assert.That(broken.Line(), Does.Contain("It read on after the pictures, not the off the user had, so the user's setting is left changed"));
            Assert.That(broken.Line(), Does.Contain("Clash Detective, Results, Display Settings, Isolation sets it back"));
            Assert.That(brokenRun.OwesAPutBack, Is.True);
            Assert.That(brokenRun.TakeTheGroupError(), Is.EqualTo(
                "Clash Detective's Hide Other could not be put back to off after the pictures. Set it back in Clash Detective, Results, Display Settings, Isolation."));
            Assert.That(brokenRun.TakeTheGroupError(), Is.Null, "taken once, for the one group");
        }

        [Test]
        public void APutBackThatThrewIsNamedAndOwed()
        {
            HideOtherAcrossTheRun run = new HideOtherAcrossTheRun();
            OtherItemsInPictures rule = new OtherItemsInPictures(false, run);
            rule.Read(false);
            rule.Set();
            rule.ReadBack(true);
            rule.PutBackThrew("the handle was released");
            rule.NotReadAfter("no state");

            string said = rule.Line();

            Assert.That(said, Does.Contain("then putting back off threw, the handle was released"));
            Assert.That(said, Does.Contain("whether the user's setting is as it was is UNKNOWN"));
            Assert.That(run.OwesAPutBack, Is.True);
            Assert.That(run.TakeTheGroupError(), Does.Contain("could not be put back to off"));
        }

        [Test]
        public void ASetThatThrewIsStillPutBackSinceHowFarItWentIsUnknown()
        {
            OtherItemsInPictures rule = new OtherItemsInPictures(false, new HideOtherAcrossTheRun());
            rule.Read(false);
            rule.SetThrew("the settings are read only");
            rule.ReadBack(false);

            Assert.That(rule.PutsBack, Is.True);
            Assert.That(rule.PutBackTo, Is.False, "the user's off is what is written back");

            rule.PutBack();
            rule.ReadAfter(false);
            string said = rule.Line();

            Assert.That(said, Does.Contain("setting it on threw, the settings are read only, and it read back off"));
            Assert.That(said, Does.Contain("then put back to off"));
        }

        /// <summary>
        /// The breaker's second point on attempt 1: a put back that failed in one group must not make
        /// the value it left read as the user's own in the next. PutBackTo is the run's first read.
        /// </summary>
        [Test]
        public void ALaterGroupPutsBackTheValueTheRunReadFirstAndSaysItDiffered()
        {
            HideOtherAcrossTheRun run = new HideOtherAcrossTheRun();

            OtherItemsInPictures first = new OtherItemsInPictures(false, run);
            first.Read(false);
            first.Set();
            first.PutBackThrew("x");
            first.ReadAfter(true);
            run.TakeTheGroupError();

            OtherItemsInPictures second = new OtherItemsInPictures(false, run);
            second.Read(true);

            Assert.That(second.SetsIt, Is.False, "it already reads on");
            Assert.That(second.PutsBack, Is.True, "it reads other than the user's value, so it is put back");
            Assert.That(second.PutBackTo, Is.False, "the user's off from the first group, never the on the failure left");

            second.PutBack();
            second.ReadAfter(false);

            Assert.That(second.Line(), Does.Contain(
                "it read on before the pictures, where the run's first group read off, the user's own, so an earlier group's put back did not hold"));
            Assert.That(second.Line(), Does.Contain("then put back to off. It read off after the pictures, the user's own setting"));
            Assert.That(run.OwesAPutBack, Is.False, "mended by the second group");
        }

        [Test]
        public void APutBackStillOwedIsTriedAtTheEndOfTheRun()
        {
            HideOtherAcrossTheRun run = new HideOtherAcrossTheRun();
            OtherItemsInPictures rule = new OtherItemsInPictures(false, run);
            rule.Read(false);
            rule.Set();
            rule.PutBackThrew("x");
            rule.ReadAfter(true);

            Assert.That(run.OwesAPutBack, Is.True);
            Assert.That(run.UsersValue, Is.False);

            string said = run.AtTheEnd(null, false, null);

            Assert.That(said, Does.StartWith("IMAGES   Clash Detective's Hide Other was left changed by a group of this run, so it was put back once more at the end of the run to off"));
            Assert.That(said, Does.EndWith("It read off after, the user's own setting"));
            Assert.That(run.OwesAPutBack, Is.False);

            HideOtherAcrossTheRun stillOwed = new HideOtherAcrossTheRun();
            OtherItemsInPictures again = new OtherItemsInPictures(false, stillOwed);
            again.Read(false);
            again.Set();
            again.PutBackThrew("x");
            again.ReadAfter(true);

            Assert.That(stillOwed.AtTheEnd("it threw again", true, null), Does.Contain(
                "and that threw, it threw again. It read on after, not the off the user had, so the user's setting is left changed"));
            Assert.That(stillOwed.OwesAPutBack, Is.True);
        }

        [Test]
        public void AReadAfterThatThrewSaysTheSettingIsUnknown()
        {
            OtherItemsInPictures rule = new OtherItemsInPictures(false, new HideOtherAcrossTheRun());
            rule.Read(false);
            rule.Set();
            rule.ReadBack(true);
            rule.PutBack();
            rule.NotReadAfter(null);

            string said = rule.Line();

            Assert.That(said, Does.Contain("could not be read after the pictures, why is UNKNOWN"));
            Assert.That(said, Does.Contain("whether the user's setting is as it was is UNKNOWN"));
            Assert.That(said, Does.Not.Contain("the user's own setting"));
        }

        [Test]
        public void NoLineCarriesASemicolonOrAnEmDash()
        {
            OtherItemsInPictures[] rules =
            {
                new OtherItemsInPictures(false, new HideOtherAcrossTheRun()),
                new OtherItemsInPictures(true, new HideOtherAcrossTheRun()),
            };

            foreach (OtherItemsInPictures rule in rules)
            {
                rule.Read(!rule.Wanted);
                rule.SetThrew("x");
                rule.NotReadBack("w");
                rule.PutBackThrew("y");
                rule.NotReadAfter("z");
                Assert.That(rule.Line(), Does.Not.Contain(";"));
                Assert.That(rule.Line(), Does.Not.Contain(EmDash));
                Assert.That(rule.Line(), Does.StartWith("IMAGES   "));
            }
        }
    }
}
