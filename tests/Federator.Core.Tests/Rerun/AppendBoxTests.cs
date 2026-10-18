using System;
using System.Collections.Generic;
using Federator.Core.Report;
using Federator.Core.Rerun;
using NUnit.Framework;

namespace Federator.Core.Tests
{
    /// <summary>
    /// Which message box an append of a bad NWC opened is answered, with what, and the words it leaves, F213, F210-R6.
    /// The boxes are the three the probe of scan.md 5z-zi read off Navisworks: one Button of id 2, an empty Static of id
    /// 20 and a Static of id 65535 holding the text. Each check is broken one thing at a time.
    /// </summary>
    [TestFixture]
    public class AppendBoxTests
    {
        private static readonly string File = TestPaths.At("work", "C06", "1104-PAR-1B06KI-ZZZ-AR-MOD-000001.nwc");

        private const string CorruptText =
            "1104-PAR-1B06KI-ZZZ-AR-MOD-000001.nwc could not be opened because the contents are corrupt or it is currently unavailable.\n\n"
            + "It is recommended to re-open your model to avoid data loss.\n\n\nC:\\work\\C06\\1104-PAR-1B06KI-ZZZ-AR-MOD-000001.nwc";

        private const string HeldText =
            "1104-PAR-1B06KI-ZZZ-AR-MOD-000001.nwc could not be opened or is unavailable.\n\n"
            + "It is recommended to re-open your model to avoid data loss.\n\n\nC:\\work\\C06\\1104-PAR-1B06KI-ZZZ-AR-MOD-000001.nwc";

        private static List<BoxPart> Box(string text)
        {
            return new List<BoxPart>
            {
                new BoxPart(AppendBox.ButtonClass, 2, "OK"),
                new BoxPart(AppendBox.TextClass, 20, string.Empty),
                new BoxPart(AppendBox.TextClass, 65535, text)
            };
        }

        [Test]
        public void TheBoxOfTheCorruptFileIsTheBoxOfItsAppend()
        {
            Assert.That(AppendBox.IsTheBoxOf(File, "#32770", Box(CorruptText)), Is.True);
            Assert.That(AppendBox.IsTheBoxOf(File, "#32770", Box(HeldText)), Is.True);
        }

        [Test]
        public void ItIsAnsweredWithTheIdOfItsOneButtonAndNeverIdOk()
        {
            // 5z-zi: IDOK, 1, left the box standing, and 2, the id of its button, closed it.
            Assert.That(AppendBox.ButtonToAnswer(Box(CorruptText)), Is.EqualTo(2));
        }

        [Test]
        public void AWindowOfAnotherClassIsNotTheBox()
        {
            const string NavisworksMain = "WindowsForms10.Window.8.app.0.143da96_r6_ad1";

            Assert.That(AppendBox.IsAMessageBox(NavisworksMain), Is.False);
            Assert.That(AppendBox.IsAMessageBox("#32770"), Is.True);
            Assert.That(AppendBox.IsTheBoxOf(File, NavisworksMain, Box(CorruptText)), Is.False);
        }

        [Test]
        public void ABoxNamingAFileWhoseNameEndsWithThisOnesIsNotItsBox()
        {
            // The reviewer's finding 7: the name was matched anywhere in the text, so a box of my-a.nwc was the box of a.nwc.
            string other = "X-1104-PAR-1B06KI-ZZZ-AR-MOD-000001.nwc could not be opened because the contents are corrupt.\n\n"
                + "C:\\work\\C06\\X-1104-PAR-1B06KI-ZZZ-AR-MOD-000001.nwc";
            string spaced = other.Replace("X-1104", "X 1104");

            Assert.That(AppendBox.IsTheBoxOf(File, "#32770", Box(other)), Is.False);
            Assert.That(AppendBox.IsTheBoxOf(File, "#32770", Box(spaced)), Is.False);
            Assert.That(AppendBox.IsTheBoxOf(File, "#32770", Box("\"1104-PAR-1B06KI-ZZZ-AR-MOD-000001.nwc\" could not be opened")),
                Is.True, "a name in quotes is still the whole name");
        }

        [Test]
        public void ABoxThatDoesNotNameTheFileIsLeftAlone()
        {
            List<BoxPart> box = Box(CorruptText.Replace("1B06KI", "1B06BS"));

            Assert.That(AppendBox.IsTheBoxOf(File, "#32770", box), Is.False,
                "a box about another file, or any other box, is never answered by the append's watch");
        }

        [Test]
        public void ABoxOfTwoButtonsIsLeftAloneAndHasNoAnswer()
        {
            List<BoxPart> box = Box(CorruptText);
            box.Add(new BoxPart(AppendBox.ButtonClass, 7, "No"));

            Assert.That(AppendBox.IsTheBoxOf(File, "#32770", box), Is.False,
                "a box asking a choice is a question, and the tool answers no question");
            Assert.That(AppendBox.ButtonToAnswer(box), Is.Null);
        }

        [Test]
        public void ABoxWithNoButtonIsLeftAlone()
        {
            List<BoxPart> box = new List<BoxPart> { new BoxPart(AppendBox.TextClass, 65535, CorruptText) };

            Assert.That(AppendBox.IsTheBoxOf(File, "#32770", box), Is.False);
            Assert.That(AppendBox.ButtonToAnswer(box), Is.Null);
        }

        [Test]
        public void TheNameIsReadWithoutCaseAsWindowsReadsAPath()
        {
            Assert.That(AppendBox.IsTheBoxOf(File, "#32770", Box(CorruptText.ToLowerInvariant())), Is.True);
        }

        [Test]
        public void AnEmptyFileNameAnswersNothing()
        {
            Assert.That(AppendBox.IsTheBoxOf(string.Empty, "#32770", Box(CorruptText)), Is.False);
            Assert.That(AppendBox.IsTheBoxOf(File, "#32770", null), Is.False);
        }

        [Test]
        public void TheReasonIsWhatNavisworksSaidWithTheFileNameAndTheAdviceTakenOff()
        {
            Assert.That(AppendBox.Reason(File, Box(CorruptText)),
                Is.EqualTo("could not be opened because the contents are corrupt or it is currently unavailable"));
            Assert.That(AppendBox.Reason(File, Box(HeldText)), Is.EqualTo("could not be opened or is unavailable"));
        }

        [Test]
        public void TheReasonIsTheFirstLineEvenWhereNoBlankLineFollowsIt()
        {
            string text = "1104-PAR-1B06KI-ZZZ-AR-MOD-000001.nwc could not be opened.\nA second line of advice.";

            Assert.That(AppendBox.Reason(File, Box(text)), Is.EqualTo("could not be opened"));
            Assert.That(AppendBox.Reason(File, Box("\r\n" + text.Replace("\n", "\r\n"))), Is.EqualTo("could not be opened"),
                "a box whose text opens on a line break reads its first line with words");
        }

        [Test]
        public void AReasonThatCannotBeReadIsUnknownAndNeverEmpty()
        {
            Assert.That(AppendBox.Reason(File, Box("something else entirely")), Is.EqualTo("UNKNOWN"));
            Assert.That(AppendBox.Reason(File, Box("1104-PAR-1B06KI-ZZZ-AR-MOD-000001.nwc.")), Is.EqualTo("UNKNOWN"));
            Assert.That(AppendBox.Reason(File, null), Is.EqualTo("UNKNOWN"));
        }

        [Test]
        public void TheGroupErrorNamesTheFullPathTheReasonAndThatTheToolAnsweredAndEndsWithNoStop()
        {
            string error = AppendBox.GroupError(File, AppendBox.Reason(File, Box(CorruptText)));

            Assert.That(error, Does.StartWith(File + " could not be appended"));
            Assert.That(error, Does.Contain("corrupt or it is currently unavailable"));
            Assert.That(error, Does.Contain("the tool answered its message box"));
            Assert.That(error, Does.Not.EndWith("."), "the judgement joins the reasons with and, so a stop would read machine., and");
            Assert.That(error, Does.Not.Contain(";"));
            Assert.That(error, Does.Not.Contain("no one"), "whether anyone was at the machine is not known to the tool");
        }

        [Test]
        public void TheLinesNameTheFileAndWhatWasDone()
        {
            string answered = AppendBox.AnsweredLine(File, 2, "could not be opened or is unavailable", 205);
            string stands = AppendBox.StillStandsLine(File, 10);
            string appended = AppendBox.AnsweredAndAppendedLine(File);
            string left = AppendBox.LeftAloneLine(File, "another\r\nbox\n");

            Assert.That(answered, Does.StartWith("APPEND   "));
            Assert.That(answered, Does.Contain(File).And.Contain("id 2").And.Contain("205 ms")
                .And.Contain("could not be opened or is unavailable"));
            Assert.That(stands, Does.StartWith("APPEND   ").And.Contain(File).And.Contain("10 times").And.Contain("still stands"));
            Assert.That(appended, Does.StartWith("APPEND   ").And.Contain(File).And.Contain("not failed"));
            Assert.That(left, Does.Contain("left it alone").And.Contain("another box").And.Not.Contain("\n"));
        }

        [Test]
        public void TheWatchIsReadEveryTwoHundredMillisecondsAndRefusesLessThanFifty()
        {
            Assert.That(new ReportOptions().AppendBox.EveryMilliseconds, Is.EqualTo(200));
            AppendBoxSettings settings = new AppendBoxSettings();
            Assert.Throws<ArgumentOutOfRangeException>(() => settings.EveryMilliseconds = 49);
            settings.EveryMilliseconds = 50;
            Assert.That(settings.EveryMilliseconds, Is.EqualTo(50));
        }
    }
}
