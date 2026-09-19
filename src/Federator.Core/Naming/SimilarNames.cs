using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Federator.Core.Naming
{
    /// <summary>
    /// Whether a name already in the NWF folder is close enough to the one a group would
    /// write that the person meant it to be the same file.
    ///
    /// WHY THIS EXISTS. The tool reuses an NWF that is already at the output path and
    /// builds a new one when nothing is there, and that is the right rule. On the run of
    /// 2026-09-19 Bader got First run on buildings that already had an NWF, because the
    /// NWF folder and the name pattern together did not resolve to his file, and nothing
    /// anywhere said so. A group that is about to build a second NWF beside one that is
    /// nearly the same name is the one case where First run is probably not what was
    /// wanted, and this is what notices it.
    ///
    /// IT REPORTS AND IT NEVER ACTS. Nothing here changes a name, picks a file or unticks
    /// a group, which is the rule this repo keeps for every finding. The person types the
    /// NWF Name cell over if that is what they meant, and the run does what it is told
    /// either way.
    ///
    /// NO FILE SYSTEM ANYWHERE IN HERE. It is handed a list of names and it compares
    /// names, so every rule in it has a test and none of them needs a disk.
    /// </summary>
    public static class SimilarNames
    {
        /// <summary>
        /// Why one name is close to another. The reason is carried rather than worked out
        /// again by the caller, because the window and the log both say it.
        /// </summary>
        public enum Closeness
        {
            /// <summary>Not close. The two names have nothing in common that matters.</summary>
            NotClose = 0,

            /// <summary>The same building code sits in both names.</summary>
            SameBuilding = 1,

            /// <summary>
            /// Project, originator and building all agree and at least one of the four
            /// supplied fields differs. That is the level, the discipline, the type or the
            /// number, which are the four this tool supplies rather than reads.
            /// </summary>
            SuppliedFieldsDiffer = 2
        }

        /// <summary>One name in the folder that is close, and why.</summary>
        public sealed class NearbyName
        {
            internal NearbyName(string name, Closeness how, string reason)
            {
                Name = name;
                How = how;
                Reason = reason;
            }

            /// <summary>The name as it was handed in, never tidied.</summary>
            public string Name { get; private set; }

            public Closeness How { get; private set; }

            /// <summary>Why it is close, in the words a person would use.</summary>
            public string Reason { get; private set; }

            public override string ToString()
            {
                return Name + " [" + Reason + "]";
            }
        }

        /// <summary>
        /// Every name in the folder that is close to the wanted one, in the order they
        /// were handed in. An empty list is the ordinary answer and is never null.
        ///
        /// A name that is EXACTLY the wanted one is not a match, because that group is not
        /// a First run at all and the engine will open the file. Only a near miss is worth
        /// saying anything about.
        /// </summary>
        public static IList<NearbyName> In(
            string wantedName, IEnumerable<string> namesInFolder, ContainerNameSettings settings)
        {
            List<NearbyName> found = new List<NearbyName>();

            if (string.IsNullOrEmpty(wantedName) || namesInFolder == null)
            {
                return new ReadOnlyCollection<NearbyName>(found);
            }

            ContainerNameSettings how = settings ?? new ContainerNameSettings();
            string wantedStem = ContainerName.Stem(wantedName);
            string[] wanted = wantedStem.Split(how.Separator);

            foreach (string candidate in namesInFolder)
            {
                if (string.IsNullOrEmpty(candidate))
                {
                    continue;
                }

                // THE EXACT MATCH IS COMPARED ON THE STEM and not on the string handed in.
                // A folder listing gives 1104-PAR-1B06PH-ZZZ-BM-MOD-000001.nwf and the
                // name a group would write has no extension on it, so comparing the two
                // raw would call every exact match a near miss, which is the opposite of
                // the truth: that file being there is what makes the group a Weekly run.
                if (string.Equals(
                        ContainerName.Stem(candidate), wantedStem, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                NearbyName nearby = Compare(wantedName, wanted, candidate, how);

                if (nearby != null)
                {
                    found.Add(nearby);
                }
            }

            return new ReadOnlyCollection<NearbyName>(found);
        }

        /// <summary>
        /// The closest single name, or null where none is close. The first of the two
        /// reasons wins where both are present, because a name differing only in the
        /// supplied fields is the more specific answer and the one worth showing.
        /// </summary>
        public static NearbyName ClosestIn(
            string wantedName, IEnumerable<string> namesInFolder, ContainerNameSettings settings)
        {
            IList<NearbyName> all = In(wantedName, namesInFolder, settings);
            NearbyName best = null;

            for (int i = 0; i < all.Count; i++)
            {
                if (all[i].How == Closeness.SuppliedFieldsDiffer)
                {
                    return all[i];
                }

                if (best == null)
                {
                    best = all[i];
                }
            }

            return best;
        }

        /// <summary>
        /// The sentence the Run as column and the group list carry after the label. It
        /// lives here so the window, the log and the tests read the same words.
        /// </summary>
        public static string Note(NearbyName found)
        {
            if (found == null)
            {
                return string.Empty;
            }

            return ", but the NWF folder holds a similar name: " + found.Name;
        }

        /// <summary>
        /// The one line that goes under the group table when any group is in this state.
        /// It says what to do about it and nothing else, because the tool does not do it.
        /// </summary>
        public static string WhatToDo(int groupCount)
        {
            string groups = groupCount == 1 ? "1 group is" : groupCount + " groups are";

            return groups + " set to build a new NWF beside a file whose name is nearly the same. "
                + "Type the NWF Name cell over to point at the existing file, or leave it to build a new one.";
        }

        /// <summary>
        /// The name split into its parts, with the extension and any folder taken off
        /// first so a name with an extension and one without compare the same.
        /// </summary>
        private static string[] Fields(string name, ContainerNameSettings settings)
        {
            return ContainerName.Stem(name).Split(settings.Separator);
        }

        private static NearbyName Compare(
            string wantedName, string[] wanted, string candidate, ContainerNameSettings settings)
        {
            string[] theirs = Fields(candidate, settings);

            // The more specific reason is tested first, so a name that qualifies under
            // both is reported as the one that says more.
            string differing = OnlySuppliedFieldsDiffer(wanted, theirs, settings);

            if (differing != null)
            {
                return new NearbyName(
                    candidate,
                    Closeness.SuppliedFieldsDiffer,
                    "the same name with a different " + differing);
            }

            string building = SharedBuilding(wanted, theirs, settings);

            if (building != null)
            {
                return new NearbyName(
                    candidate,
                    Closeness.SameBuilding,
                    "it carries the building code " + building);
            }

            return null;
        }

        /// <summary>
        /// The building code where both names carry one and the two agree, or null.
        /// Compared without case, because a file name on Windows is matched that way and
        /// a folder listing hands back whatever case is on the disk.
        /// </summary>
        private static string SharedBuilding(
            string[] wanted, string[] theirs, ContainerNameSettings settings)
        {
            string mine = At(wanted, settings.BuildingPart);
            string yours = At(theirs, settings.BuildingPart);

            if (mine.Length == 0 || yours.Length == 0)
            {
                return null;
            }

            return string.Equals(mine, yours, StringComparison.OrdinalIgnoreCase) ? mine : null;
        }

        /// <summary>
        /// Which supplied field differs, where project, originator and building all agree
        /// and the two names have the same number of parts, or null where that is not the
        /// shape.
        ///
        /// THE FOUR SUPPLIED FIELDS ARE THE LEVEL, THE DISCIPLINE, THE TYPE AND THE
        /// NUMBER, which are exactly the four NamePattern supplies rather than reads out
        /// of the input. Those are the four a person is most likely to have set
        /// differently from the file already on disk, which is what made the run build a
        /// second NWF beside the first.
        ///
        /// Where more than one of them differs the answer names the first, because a
        /// sentence listing four fields is not one anybody reads.
        /// </summary>
        private static string OnlySuppliedFieldsDiffer(
            string[] wanted, string[] theirs, ContainerNameSettings settings)
        {
            if (wanted.Length != theirs.Length)
            {
                return null;
            }

            if (!Agrees(wanted, theirs, settings.ProjectPart)
                || !Agrees(wanted, theirs, settings.OriginatorPart)
                || !Agrees(wanted, theirs, settings.BuildingPart))
            {
                return null;
            }

            string first = null;

            for (int position = 1; position <= wanted.Length; position++)
            {
                if (position == settings.ProjectPart
                    || position == settings.OriginatorPart
                    || position == settings.BuildingPart)
                {
                    continue;
                }

                if (!Agrees(wanted, theirs, position))
                {
                    if (first == null)
                    {
                        first = FieldName(position, settings);
                    }
                }
            }

            return first;
        }

        private static bool Agrees(string[] wanted, string[] theirs, int position)
        {
            return string.Equals(
                At(wanted, position), At(theirs, position), StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>One based, the way the naming standard is written down. Empty off the end.</summary>
        private static string At(string[] parts, int position)
        {
            return position >= 1 && position <= parts.Length ? parts[position - 1] : string.Empty;
        }

        /// <summary>
        /// What to call the part at that position, in the words the naming standard uses.
        /// The three read off the input are named from the settings that read them, and
        /// the rest are the seven field order: level, discipline, type, number.
        /// </summary>
        private static string FieldName(int position, ContainerNameSettings settings)
        {
            if (position == settings.ProjectPart)
            {
                return "project code";
            }

            if (position == settings.OriginatorPart)
            {
                return "originator";
            }

            if (position == settings.BuildingPart)
            {
                return "building";
            }

            if (position == settings.DisciplinePart)
            {
                return "discipline";
            }

            switch (position)
            {
                case 4:
                    return "level";
                case 6:
                    return "type code";
                case 7:
                    return "number";
                default:
                    return "part " + position;
            }
        }
    }
}
