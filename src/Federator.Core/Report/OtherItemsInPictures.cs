namespace Federator.Core.Report
{
    /// <summary>
    /// WHAT CLASH DETECTIVE IS ASKED TO DRAW BESIDE A PICTURE'S TWO ITEMS, F150, Bader's answer to Q151
    /// and his order of 2026-10-12, item 157 of steps\02_questions.md: every picture shows only its two
    /// clashing items, whatever the display settings of the PC it runs on, and the ghosted look of the
    /// accepted reports stays as one setting, ImageOptions.GhostTheOtherItems, off by default. The tool's
    /// picture call draws by Clash Detective's display settings, docs\history\scan.md 5z-ze, and those
    /// are per user. So for one group's picture pass the add-in sets Hide Other to the value asked for,
    /// on where the ghosted look is not asked for and off where it is, ImageOptions.GhostTheOtherItems, which has had no box
    /// since F232, and puts the user's value back after.
    ///
    /// This holds the rule of one group and its one IMAGES line. The add-in reads Hide Other once a
    /// stretch of the pass takes a picture and hands it to Read, sets it only where SetsIt says so and
    /// reads it back at once, writes PutBackTo where PutsBack says so, reads it once more after, and
    /// writes Line. Nothing is set where the value could not be read, since a value never read cannot
    /// be put back. The user's value is the run's, HideOtherAcrossTheRun, read at its first group, so a
    /// put back that failed in one group is mended in the next. Every throw is the add-in's failure line
    /// and is noted here, so the line says what it could not do.
    ///
    /// WHAT THE LINE DOES NOT KNOW. A read back says what the highlighter holds, never what the picture
    /// call drew with, so whether the pictures follow the value is UNKNOWN until a run's picture sizes
    /// show it, and the line says so. A stop between the set and the put back, Navisworks closed or
    /// killed in the pass, leaves the value set, and whether Navisworks keeps it for the user's next
    /// start is UNKNOWN, row F150-R1.
    /// </summary>
    public sealed class OtherItemsInPictures
    {
        /// <summary>Where a person sets Hide Other by hand in Clash Detective, named where the tool could not put it back.</summary>
        internal const string WhereToSetIt = "Clash Detective, Results, Display Settings, Isolation";

        /// <summary>The sizes that tell whether the pictures followed, docs\history\scan.md 5z-ze and 5z-zf.</summary>
        private const string WhatTellsIt = "Whether the pictures follow it is UNKNOWN until a run's picture sizes show it, near 48 KB a picture "
            + "with the other items hidden against about 550 KB with them drawn, docs\\history\\scan.md 5z-ze";

        private readonly bool ghosted;
        private readonly HideOtherAcrossTheRun run;
        private bool begun;
        private bool? before;
        private string whyNotRead;
        private bool setTried;
        private string whySetFailed;
        private bool? readBack;
        private string whyNotReadBack;
        private bool putBackTried;
        private string whyPutBackFailed;
        private bool? after;
        private string whyNotReadAfter;

        public OtherItemsInPictures(bool ghostTheOtherItems, HideOtherAcrossTheRun run)
        {
            ghosted = ghostTheOtherItems;
            this.run = run ?? new HideOtherAcrossTheRun();
        }

        /// <summary>The value the pictures ask for: on where the ghosted look is not asked for, off where it is.</summary>
        public bool Wanted
        {
            get { return !ghosted; }
        }

        /// <summary>The user's value read once a stretch takes a picture. The run's first read is kept as the user's.</summary>
        public void Read(bool value)
        {
            begun = true;
            before = value;
            whyNotRead = null;
            run.Heard(value);
        }

        /// <summary>The read before the pictures threw, so nothing is set.</summary>
        public void NotRead(string why)
        {
            begun = true;
            before = null;
            whyNotRead = Said(why);
        }

        /// <summary>
        /// Whether Hide Other is set for the pictures: only where it was read and reads other than
        /// the value asked for. Not read, nothing is set, since it could not be put back.
        /// </summary>
        public bool SetsIt
        {
            get { return before.HasValue && before.Value != Wanted; }
        }

        /// <summary>Hide Other was set to the value asked for.</summary>
        public void Set()
        {
            setTried = true;
            whySetFailed = null;
        }

        /// <summary>Setting Hide Other threw. It is put back all the same, since how far the set went is UNKNOWN.</summary>
        public void SetThrew(string why)
        {
            setTried = true;
            whySetFailed = Said(why);
        }

        /// <summary>Hide Other read back at once after the set, done or thrown.</summary>
        public void ReadBack(bool value)
        {
            readBack = value;
            whyNotReadBack = null;
        }

        /// <summary>The read back after the set threw.</summary>
        public void NotReadBack(string why)
        {
            readBack = null;
            whyNotReadBack = Said(why);
        }

        /// <summary>
        /// Whether a value is written back after the pictures: where a set was tried, or where this
        /// group read other than the user's value the run read first, a put back of an earlier group
        /// that did not hold. Never where nothing was read.
        /// </summary>
        public bool PutsBack
        {
            get
            {
                return before.HasValue && run.UsersValue.HasValue
                    && (setTried || before.Value != run.UsersValue.Value);
            }
        }

        /// <summary>The value written back, the user's own as the run's first group read it. Read only where PutsBack.</summary>
        public bool PutBackTo
        {
            get { return run.UsersValue.HasValue && run.UsersValue.Value; }
        }

        /// <summary>The put back was written.</summary>
        public void PutBack()
        {
            putBackTried = true;
            whyPutBackFailed = null;
        }

        /// <summary>Putting the user's value back threw.</summary>
        public void PutBackThrew(string why)
        {
            putBackTried = true;
            whyPutBackFailed = Said(why);
        }

        /// <summary>Hide Other read after the pictures and the put back. Settles what the run owes.</summary>
        public void ReadAfter(bool value)
        {
            after = value;
            whyNotReadAfter = null;
            Settle();
        }

        /// <summary>The read after the pictures threw. Settles what the run owes.</summary>
        public void NotReadAfter(string why)
        {
            after = null;
            whyNotReadAfter = Said(why);
            Settle();
        }

        /// <summary>
        /// What the run owes once this group is done: a put back where Hide Other reads other than the
        /// user's value, or where this group's put back threw and nothing read after. The group's error,
        /// for the window, only where this group set or put back and left it so.
        /// </summary>
        private void Settle()
        {
            if (!run.UsersValue.HasValue || (!after.HasValue && whyPutBackFailed == null))
            {
                return;
            }

            bool leftChanged = after.HasValue ? after.Value != run.UsersValue.Value : true;
            bool ours = setTried || putBackTried;

            run.Owe(leftChanged, leftChanged && ours
                ? "Clash Detective's Hide Other could not be put back to " + OnOff(run.UsersValue.Value)
                    + " after the pictures. Set it back in " + WhereToSetIt + "."
                : null);
        }

        /// <summary>
        /// The one IMAGES line of the group: the value asked for, what was read before the pictures,
        /// what was set and read back, what was put back and what was read after, and where the
        /// user's setting is not known to be back, where to set it by hand.
        /// </summary>
        public string Line()
        {
            string said = "IMAGES   Clash Detective's Hide Other";

            if (!begun)
            {
                return said + " not read or set, since no picture was taken in this group";
            }

            said += Wanted
                ? " asked on for the pictures, to hide the other items: "
                : " asked off for the pictures, to show the other items, since the ghosted look is asked for: ";

            if (!before.HasValue)
            {
                said += "it could not be read before the pictures, so nothing was set and they follow the user's own setting, "
                    + whyNotRead;
            }
            else
            {
                said += "it read " + OnOff(before.Value) + " before the pictures" + AgainstTheRun() + SetWords() + PutBackWords();
            }

            said += ". " + AfterWords();

            if (setTried)
            {
                said += ". " + WhatTellsIt;
            }

            return said;
        }

        private string AgainstTheRun()
        {
            if (!run.UsersValue.HasValue || before.Value == run.UsersValue.Value)
            {
                return string.Empty;
            }

            return ", where the run's first group read " + OnOff(run.UsersValue.Value)
                + ", the user's own, so an earlier group's put back did not hold";
        }

        private string SetWords()
        {
            if (!setTried)
            {
                return ", already " + OnOff(before.Value) + ", so nothing was set";
            }

            string set = whySetFailed == null
                ? ", set " + OnOff(Wanted)
                : ", setting it " + OnOff(Wanted) + " threw, " + whySetFailed;

            return set + (readBack.HasValue
                ? ", and it read back " + OnOff(readBack.Value)
                : ", and it could not be read back, " + whyNotReadBack);
        }

        private string PutBackWords()
        {
            if (!putBackTried)
            {
                return string.Empty;
            }

            return whyPutBackFailed == null
                ? ", then put back to " + OnOff(PutBackTo)
                : ", then putting back " + OnOff(PutBackTo) + " threw, " + whyPutBackFailed;
        }

        private string AfterWords()
        {
            if (!after.HasValue)
            {
                return "It could not be read after the pictures, " + whyNotReadAfter
                    + ", so whether the user's setting is as it was is UNKNOWN. " + WhereToSetIt + " shows it";
            }

            if (!run.UsersValue.HasValue)
            {
                return "It read " + OnOff(after.Value) + " after the pictures";
            }

            if (after.Value == run.UsersValue.Value)
            {
                return "It read " + OnOff(after.Value) + " after the pictures, the user's own setting";
            }

            return "It read " + OnOff(after.Value) + " after the pictures, not the " + OnOff(run.UsersValue.Value)
                + " the user had, so the user's setting is left changed. " + WhereToSetIt + " sets it back";
        }

        internal static string OnOff(bool value)
        {
            return value ? "on" : "off";
        }

        internal static string Said(string why)
        {
            return string.IsNullOrEmpty(why) ? "why is UNKNOWN" : why;
        }
    }
}
