using System.Linq;
using Federator.Core.Report;
using NUnit.Framework;

namespace Federator.Core.Tests
{
    /// <summary>
    /// F150, Bader's answer to Q151: every clash picture shows only its two clashing items, the tool
    /// setting Clash Detective's Hide Other for the pictures and putting the user's value back after,
    /// and the ghosted look stays as one setting, off by default. Each test breaks one thing and
    /// asserts the rule or the IMAGES line names it.
    /// </summary>
    [TestFixture]
    public class OtherItemsInPicturesTests
    {
        private static readonly string EmDash = ((char)0x2014).ToString();

        [Test]
        public void TheBoxIsOffByDefaultAndTheLogSaysWhatThePicturesShow()
        {
            ImageOptions options = new ImageOptions();

            Assert.That(options.GhostTheOtherItems, Is.False, "Bader's order of 2026-10-12, the ghosted look is off by default");
            Assert.That(options.Describe(), Does.Contain("only the two clashing items in each"));

            options.GhostTheOtherItems = true;

            Assert.That(options.Describe(), Does.Contain("the other items as Clash Detective shows them"));
            Assert.That(options.Describe(), Does.Not.Contain("only the two clashing items"));
        }

        [Test]
        public void TheLabelAndItsGreyLineAreShortAndPlain()
        {
            Assert.That(OtherItemsInPictures.TickLabel.Split(' ').Length, Is.LessThanOrEqualTo(8));
            Assert.That(OtherItemsInPictures.HelpLine.Split(' ').Length, Is.LessThanOrEqualTo(12));
            Assert.That(OtherItemsInPictures.TickLabel, Does.Not.Contain("GhostTheOtherItems"), "no code identifier in a label");
            Assert.That(OtherItemsInPictures.HelpLine, Does.Not.Contain("HideOther"), "no code identifier in a label");

            foreach (string word in OtherItemsInPictures.TickLabel.Split(' ').Concat(OtherItemsInPictures.HelpLine.Split(' ')))
            {
                string letters = word.Trim(',', '.');
                bool shouted = letters.Length > 2 && letters == letters.ToUpperInvariant() && letters.Any(char.IsLetter);
                Assert.That(shouted, Is.False, "no capitals for emphasis, " + word);
            }

            Assert.That(OtherItemsInPictures.TickLabel + OtherItemsInPictures.HelpLine, Does.Not.Contain(";"));
            Assert.That(OtherItemsInPictures.TickLabel + OtherItemsInPictures.HelpLine, Does.Not.Contain(EmDash));
        }

        [Test]
        public void ReadOffItIsSetOnForThePicturesAndPutBack()
        {
            OtherItemsInPictures rule = new OtherItemsInPictures(false);

            Assert.That(rule.Read(false), Is.True, "the user's off is set on for the pictures");
            Assert.That(rule.SetsIt, Is.True);
            Assert.That(rule.PutsBack, Is.False, "nothing is put back before a set was tried");

            rule.Set();
            Assert.That(rule.PutsBack, Is.True);

            rule.ReadAfter(false);

            Assert.That(rule.Line(), Is.EqualTo(
                "IMAGES   Clash Detective's Hide Other read off before the pictures, set on for them so each shows only "
                + "its two clashing items, then put back to off. It read off after the pictures, the user's own setting"));
        }

        [Test]
        public void TheGhostedLookTickedNeverSetsIt()
        {
            OtherItemsInPictures rule = new OtherItemsInPictures(true);

            Assert.That(rule.Read(false), Is.False, "never set where the ghosted look is asked for");
            Assert.That(rule.SetsIt, Is.False);
            Assert.That(rule.PutsBack, Is.False);

            rule.ReadAfter(false);
            string said = rule.Line();

            Assert.That(said, Does.Contain("left as the user has it"));
            Assert.That(said, Does.Contain(OtherItemsInPictures.TickLabel), "the line names the box that was ticked");
            Assert.That(said, Does.Not.Contain("set on"));
        }

        [Test]
        public void ReadOnItIsAlreadyWhatThePicturesWantAndNothingIsSet()
        {
            OtherItemsInPictures rule = new OtherItemsInPictures(false);

            Assert.That(rule.Read(true), Is.False);
            Assert.That(rule.PutsBack, Is.False);

            rule.ReadAfter(true);

            Assert.That(rule.Line(), Does.Contain("read on before the pictures, the user's own"));
            Assert.That(rule.Line(), Does.Contain("nothing was set or put back"));
            Assert.That(rule.Line(), Does.Contain("the user's own setting"));
        }

        [Test]
        public void AValueThatCouldNotBeReadIsNeverSetSinceItCouldNotBePutBack()
        {
            OtherItemsInPictures rule = new OtherItemsInPictures(false);

            rule.NotRead("Object has been Disposed");

            Assert.That(rule.SetsIt, Is.False);
            Assert.That(rule.PutsBack, Is.False);

            rule.ReadAfter(false);
            string said = rule.Line();

            Assert.That(said, Does.Contain("could not be read before the pictures, so nothing was set"));
            Assert.That(said, Does.Contain("Object has been Disposed"), "the line names why");
            Assert.That(said, Does.Contain("It read off after the pictures"));
        }

        /// <summary>The check that matters most: a put back that did not take is named, with where to set it by hand.</summary>
        [Test]
        public void ASettingLeftChangedAfterThePicturesIsNamed()
        {
            OtherItemsInPictures good = new OtherItemsInPictures(false);
            good.Read(false);
            good.Set();
            good.ReadAfter(false);

            OtherItemsInPictures broken = new OtherItemsInPictures(false);
            broken.Read(false);
            broken.Set();
            broken.ReadAfter(true);

            Assert.That(good.Line(), Does.Not.Contain("left changed"));
            Assert.That(broken.Line(), Does.Contain("It read on after the pictures, not the off the user had, so the user's setting is left changed"));
            Assert.That(broken.Line(), Does.Contain("Clash Detective, Results, Display Settings, Isolation sets it back"));
        }

        [Test]
        public void APutBackThatThrewIsNamed()
        {
            OtherItemsInPictures rule = new OtherItemsInPictures(false);
            rule.Read(false);
            rule.Set();
            rule.PutBackThrew("the handle was released");
            rule.ReadAfter(true);

            string said = rule.Line();

            Assert.That(said, Does.Contain("putting back off threw, the handle was released"));
            Assert.That(said, Does.Contain("left changed"));
            Assert.That(said, Does.Not.Contain("put back to off"));
        }

        [Test]
        public void ASetThatThrewIsStillPutBackSinceHowFarItWentIsUnknown()
        {
            OtherItemsInPictures rule = new OtherItemsInPictures(false);
            rule.Read(false);
            rule.SetThrew("the settings are read only");

            Assert.That(rule.PutsBack, Is.True);

            rule.ReadAfter(false);
            string said = rule.Line();

            Assert.That(said, Does.Contain("setting it on threw, the settings are read only"));
            Assert.That(said, Does.Contain("they follow the user's own setting"));
            Assert.That(said, Does.Contain("put back to off"));
            Assert.That(said, Does.Not.Contain("set on for them"));
        }

        [Test]
        public void AReadAfterThatThrewSaysTheSettingIsUnknown()
        {
            OtherItemsInPictures rule = new OtherItemsInPictures(false);
            rule.Read(false);
            rule.Set();
            rule.NotReadAfter(null);

            string said = rule.Line();

            Assert.That(said, Does.Contain("could not be read after the pictures, why is UNKNOWN"));
            Assert.That(said, Does.Contain("whether the user's setting is as it was is UNKNOWN"));
            Assert.That(said, Does.Not.Contain("the user's own setting"));
        }

        [Test]
        public void NoLineCarriesASemicolonOrAnEmDash()
        {
            OtherItemsInPictures[] rules = { new OtherItemsInPictures(false), new OtherItemsInPictures(true) };

            foreach (OtherItemsInPictures rule in rules)
            {
                rule.Read(false);

                if (rule.SetsIt)
                {
                    rule.SetThrew("x");
                    rule.PutBackThrew("y");
                }

                rule.NotReadAfter("z");
                Assert.That(rule.Line(), Does.Not.Contain(";"));
                Assert.That(rule.Line(), Does.Not.Contain(EmDash));
                Assert.That(rule.Line(), Does.StartWith("IMAGES   "));
            }
        }
    }
}
