namespace Federator.Core.Report
{
    /// <summary>
    /// WHAT A CLASH PICTURE SHOWS BESIDE ITS TWO ITEMS, F150, Bader's answer to Q151 and his
    /// order of 2026-10-12, item 157 of steps\02_questions.md: every picture shows only its two
    /// clashing items, whatever the display settings of the PC it runs on. The tool's picture call
    /// draws by Clash Detective's display settings, docs\history\scan.md 5z-ze, and those are per
    /// user, so one PC drew 48 KB pictures of the two items alone and another 510 KB pictures of
    /// every model around them. So the add-in sets Clash Detective's Hide Other on for the picture
    /// pass and puts the user's own value back after it, and the ghosted look of the accepted
    /// reports stays as one setting, ImageOptions.GhostTheOtherItems, off by default.
    ///
    /// This holds the rule and the one IMAGES line. The add-in reads Hide Other before the pass
    /// and hands it to Read, sets it on only where SetsIt says so, writes PutBackTo back where
    /// PutsBack says so, reads it once more after, and writes Line. Nothing is set where the value
    /// could not be read, because a value never read cannot be put back, and nothing is set where
    /// the ghosted look is asked for. Every throw is the add-in's failure line and is noted here,
    /// so the line says what it could not do and the pass goes on with the user's own setting.
    /// </summary>
    public sealed class OtherItemsInPictures
    {
        /// <summary>The tick box beside the photo settings, read off here and never typed into the window.</summary>
        public const string TickLabel = "Ghost the other items in clash photos";

        /// <summary>The grey line under it.</summary>
        public const string HelpLine = "Off shows only the two clashing items, the same on every PC";

        /// <summary>Where a person sets Hide Other by hand in Clash Detective, named where the tool could not put it back.</summary>
        private const string WhereToSetIt = "Clash Detective, Results, Display Settings, Isolation";

        private readonly bool ghosted;

        public OtherItemsInPictures(bool ghostTheOtherItems)
        {
            ghosted = ghostTheOtherItems;
        }

        /// <summary>The user's Hide Other read before the pictures, or null where it was not read.</summary>
        private bool? Before { get; set; }

        /// <summary>Why Hide Other could not be read before the pictures, or null.</summary>
        private string WhyNotRead { get; set; }

        /// <summary>Whether a set of Hide Other on was tried, done or not.</summary>
        private bool SetTried { get; set; }

        /// <summary>Why setting Hide Other on threw, or null.</summary>
        private string WhySetFailed { get; set; }

        /// <summary>Why putting the user's value back threw, or null.</summary>
        private string WhyPutBackFailed { get; set; }

        /// <summary>Hide Other read after the pictures and the put back, or null where it was not read.</summary>
        private bool? After { get; set; }

        /// <summary>Why Hide Other could not be read after the pictures, or null.</summary>
        private string WhyNotReadAfter { get; set; }

        /// <summary>The user's value read before the pictures. Hands back SetsIt.</summary>
        public bool Read(bool usersValue)
        {
            Before = usersValue;
            WhyNotRead = null;
            return SetsIt;
        }

        /// <summary>The read before the pictures threw, so nothing is set.</summary>
        public void NotRead(string why)
        {
            Before = null;
            WhyNotRead = Said(why);
        }

        /// <summary>
        /// Whether Hide Other is set on for the pictures: only where the ghosted look is not asked
        /// for and the user's value was read as off. Read as on, it is already what the pictures
        /// want and nothing is set. Not read, nothing is set, since it could not be put back.
        /// </summary>
        public bool SetsIt
        {
            get { return !ghosted && Before.HasValue && !Before.Value; }
        }

        /// <summary>Hide Other was set on.</summary>
        public void Set()
        {
            SetTried = true;
            WhySetFailed = null;
        }

        /// <summary>Setting Hide Other on threw. It is put back all the same, since how far the set went is UNKNOWN.</summary>
        public void SetThrew(string why)
        {
            SetTried = true;
            WhySetFailed = Said(why);
        }

        /// <summary>Whether the user's value is written back after the pictures: wherever a set was tried.</summary>
        public bool PutsBack
        {
            get { return SetTried && Before.HasValue; }
        }

        /// <summary>The value written back after the pictures, the user's own read before them. Read only where PutsBack.</summary>
        public bool PutBackTo
        {
            get { return Before.HasValue && Before.Value; }
        }

        /// <summary>Putting the user's value back threw.</summary>
        public void PutBackThrew(string why)
        {
            WhyPutBackFailed = Said(why);
        }

        /// <summary>Hide Other read after the pictures and the put back.</summary>
        public void ReadAfter(bool value)
        {
            After = value;
            WhyNotReadAfter = null;
        }

        /// <summary>The read after the pictures threw.</summary>
        public void NotReadAfter(string why)
        {
            After = null;
            WhyNotReadAfter = Said(why);
        }

        /// <summary>
        /// Whether the user's setting reads after the pictures as it read before them. False where
        /// either read is missing, since then it is not known to be back.
        /// </summary>
        private bool LeftAsFound
        {
            get { return Before.HasValue && After.HasValue && Before.Value == After.Value; }
        }

        /// <summary>
        /// The one IMAGES line of the pass: what was read before the pictures, what was set, and what
        /// was read after the put back, and where the user's setting is not known to be back, where
        /// to set it by hand.
        /// </summary>
        public string Line()
        {
            string said = "IMAGES   Clash Detective's Hide Other ";

            if (!Before.HasValue)
            {
                said += "could not be read before the pictures, so nothing was set and they follow the user's own setting, "
                    + WhyNotRead;
            }
            else if (ghosted)
            {
                said += "read " + OnOff(Before.Value) + " before the pictures and left as the user has it, since the box \""
                    + TickLabel + "\" is ticked";
            }
            else if (!SetTried)
            {
                said += "read " + OnOff(Before.Value) + " before the pictures, the user's own, so each shows only its two "
                    + "clashing items and nothing was set or put back";
            }
            else if (WhySetFailed == null)
            {
                said += "read " + OnOff(Before.Value) + " before the pictures, set on for them so each shows only its two "
                    + "clashing items, then " + PutBackWords();
            }
            else
            {
                said += "read " + OnOff(Before.Value) + " before the pictures, setting it on threw, " + WhySetFailed
                    + ", so they follow the user's own setting where the set did not reach it, then " + PutBackWords();
            }

            said += ". " + AfterWords();
            return said;
        }

        private string PutBackWords()
        {
            return WhyPutBackFailed == null
                ? "put back to " + OnOff(Before.Value)
                : "putting back " + OnOff(Before.Value) + " threw, " + WhyPutBackFailed;
        }

        private string AfterWords()
        {
            if (!After.HasValue)
            {
                return "It could not be read after the pictures, " + WhyNotReadAfter
                    + ", so whether the user's setting is as it was is UNKNOWN. " + WhereToSetIt + " shows it";
            }

            if (!Before.HasValue)
            {
                return "It read " + OnOff(After.Value) + " after the pictures";
            }

            if (LeftAsFound)
            {
                return "It read " + OnOff(After.Value) + " after the pictures, the user's own setting";
            }

            return "It read " + OnOff(After.Value) + " after the pictures, not the " + OnOff(Before.Value)
                + " the user had, so the user's setting is left changed. " + WhereToSetIt + " sets it back";
        }

        private static string OnOff(bool value)
        {
            return value ? "on" : "off";
        }

        private static string Said(string why)
        {
            return string.IsNullOrEmpty(why) ? "why is UNKNOWN" : why;
        }
    }
}
