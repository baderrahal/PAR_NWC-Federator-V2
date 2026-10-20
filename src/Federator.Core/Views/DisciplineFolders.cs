using System;
using System.Collections.Generic;
using Federator.Core.Clash;
using Federator.Core.Teams;

namespace Federator.Core.Views
{
    /// <summary>
    /// The discipline folder of a test under its letter folder, F199, Bader's order of round 6, Q183: Clash Status, then
    /// A, B and C, then Architecture, Structure, Mechanical, Electrical or Other, then the test's views. A discipline folder
    /// is made only where it holds a view, because the add-in makes a folder only when a view is written into it.
    ///
    /// THE ONE PLACE A TEST'S DISCIPLINE FOLDER IS DECIDED, DisciplineFolderFor. Nothing else reads a code into a folder,
    /// so a later switch to the team that fixes a clash, off the pack's team map and both sides of the test, changes that
    /// one function and nothing else. It is handed the test, whether the pack names it, and the group's ViewTeams, which
    /// carries the run's team map and the clash XML's set tree.
    ///
    /// THE DEFAULT RULE, the first side of the test. The first side is the test's left set, ViewClash.LeftSide, read off
    /// its set name by CodeOf.Set, the one place a set name's code is read. In the pack 1104-PAR every one of the 1830
    /// tests is named its left set, -vs-, its right set, so the left set is the first side of its name, read off the pack
    /// on 15 Oct 2026. The words of the order come first, OrderWords. A code they do not name goes by the team the pack's
    /// team line gives it, where that team is one of the four discipline folders, so HV and DR are Mechanical and EV is
    /// Electrical by the line Bader wrote in the pack, Q184.
    ///
    /// A SIDE WITH NO CODE, F246, Bader's answer Q231 of 20 Oct 2026, takes the team ViewTeams.TeamOfSet reads off the
    /// folder above the set in the clash XML's set tree, the one reader the TEAMS lines use, so BLD-Security Devices,
    /// under Electrical in the pack's set tree, is Electrical. Where that team is not one of the four, and for a saved
    /// test the pack does not name, the folder is Other.
    ///
    /// ARCHITECTURE AGAINST STRUCTURE, F246, Bader's answer Q229 of 20 Oct 2026: a test whose two sides go in
    /// Architecture and Structure, in either order, is Architecture. Every other test keeps its first side's folder, so
    /// a test of AR against a service stays with the service team where the service is its first side.
    /// </summary>
    public static class DisciplineFolders
    {
        public const string Architecture = "Architecture";

        public const string Structure = "Structure";

        public const string Mechanical = "Mechanical";

        public const string Electrical = "Electrical";

        public const string Other = "Other";

        /// <summary>The five folders in the order they sit under a letter folder.</summary>
        public static string[] Names
        {
            get { return new[] { Architecture, Structure, Mechanical, Electrical, Other }; }
        }

        /// <summary>
        /// The words of Bader's order and the folder each goes in, matched Ordinal as CodeOf matches a code. A code
        /// is a part of a set name between two separators, so a word is read the same way a code is.
        /// </summary>
        private static readonly string[][] OrderWords =
        {
            new[] { "AR", Architecture },
            new[] { Architecture, Architecture },
            new[] { "ST", Structure },
            new[] { Structure, Structure },
            new[] { "ME", Mechanical },
            new[] { "MEP", Mechanical },
            new[] { Mechanical, Mechanical },
            new[] { "PL", Mechanical },
            new[] { "FP", Mechanical },
            new[] { "FF", Mechanical },
            new[] { "EL", Electrical },
            new[] { Electrical, Electrical }
        };

        /// <summary>
        /// The discipline folder of a test: Other where the pack does not name it, Architecture where its two sides go in
        /// Architecture and Structure in either order, else the folder of its first side. The ONE place this is decided.
        /// </summary>
        public static string DisciplineFolderFor(ViewClash test, bool inThePack, ViewTeams teams, char setNameSeparator)
        {
            if (test == null || !inThePack)
            {
                return Other;
            }

            string first = SideFolder(test.LeftSide, teams, setNameSeparator);
            string second = SideFolder(test.RightSide, teams, setNameSeparator);

            if (string.Equals(first, Structure, StringComparison.Ordinal) && string.Equals(second, Architecture, StringComparison.Ordinal))
            {
                return Architecture;
            }

            return first;
        }

        /// <summary>
        /// The folder of one side: its code by the order's words and then the pack's team line, and for a side with no code
        /// the team the folder above its set names, ViewTeams.TeamOfSet, where that team is one of the four, else Other.
        /// </summary>
        private static string SideFolder(string side, ViewTeams teams, char setNameSeparator)
        {
            TeamMap map = teams == null ? null : teams.Map;
            List<string> known = new List<string>();

            foreach (string[] word in OrderWords)
            {
                known.Add(word[0]);
            }

            if (map != null && map.IsRead)
            {
                known.AddRange(map.Codes);
            }

            string code = CodeOf.Set(ByDesignRule.SetNameIn(side), known, setNameSeparator);

            foreach (string[] word in OrderWords)
            {
                if (code.Length > 0 && string.Equals(word[0], code, StringComparison.Ordinal))
                {
                    return word[1];
                }
            }

            string team = null;

            if (map != null && map.IsRead)
            {
                team = code.Length > 0 ? map.TeamOf(code) : teams.TeamOfSet(side);
            }

            foreach (string folder in Names)
            {
                if (!string.Equals(folder, Other, StringComparison.Ordinal) && string.Equals(folder, team, StringComparison.Ordinal))
                {
                    return folder;
                }
            }

            return Other;
        }

        /// <summary>
        /// Whether that discipline folder may hold the Over 150mm folder, Bader's order of round 6: inside Mechanical and
        /// Electrical of C and nowhere else. The letter is TestViewPlan's to check.
        /// </summary>
        public static bool HoldsTheSizeFolder(string discipline)
        {
            return string.Equals(discipline, Mechanical, StringComparison.Ordinal)
                || string.Equals(discipline, Electrical, StringComparison.Ordinal);
        }

        /// <summary>The place of that folder in Names, for the order of the tree, Other and anything unnamed last.</summary>
        internal static int Order(string discipline)
        {
            int at = Array.IndexOf(Names, discipline);
            return at < 0 ? Names.Length : at;
        }
    }
}
