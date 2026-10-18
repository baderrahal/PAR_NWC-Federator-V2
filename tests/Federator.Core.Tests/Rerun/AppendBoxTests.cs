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
            Assert.That(AppendBox.IsTheBoxOf(File, "WindowsForms10.Window.8.app.0.143da96_r6_ad1", Box(CorruptText)), Is.False);
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
        public void AReasonThatCannotBeReadIsUnknownAndNeverEmpty()
        {
            Assert.That(AppendBox.Reason(File, Box("something else entirely")), Is.EqualTo("UNKNOWN"));
            Assert.That(AppendBox.Reason(File, null), Is.EqualTo("UNKNOWN"));
        }

        [Test]
        public void TheGroupErrorNamesTheFileTheReasonAndThatTheToolAnswered()
        {
            string error = AppendBox.GroupError(File, AppendBox.Reason(File, Box(CorruptText)));

            Assert.That(error, Does.StartWith("1104-PAR-1B06KI-ZZZ-AR-MOD-000001.nwc could not be appended"));
            Assert.That(error, Does.Contain("corrupt or it is currently unavailable"));
            Assert.That(error, Does.Contain("answered by the tool"));
            Assert.That(error, Does.Not.Contain(";"));
        }

        [Test]
        public void TheLinesNameTheFileAndWhatWasDone()
        {
            string answered = AppendBox.AnsweredLine(File, "could not be opened or is unavailable", 205);
            string left = AppendBox.LeftAloneLine(File, "another\nbox");

            Assert.That(answered, Does.StartWith("APPEND   "));
            Assert.That(answered, Does.Contain(File).And.Contain("205 ms").And.Contain("could not be opened or is unavailable"));
            Assert.That(left, Does.Contain("left it alone").And.Contain("another box"));
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
