using System;
using System.Collections.Generic;
using Federator.Core.Clash;

namespace Federator.Core.Views
{
    /// <summary>
    /// The per clash viewpoints F85 wrote before Q114, which carry no mark, F114, Q114 point 16:
    /// only viewpoints this tool made are removed or replaced, the per clash viewpoints earlier
    /// runs made included. Nothing marked them, measure-views section 8, so one is known by a
    /// strict shape, every part of which F85's own plan wrote:
    ///
    ///   1. the folders are an optional priority word, then a pair of two codes or the unknown
    ///      word joined by the pair separator, the first not after the second Ordinal as F85
    ///      sorted them, then an optional size folder, one to three deep
    ///   2. it is a viewpoint and not a folder
    ///   3. its name is a test's name, the name separator, the legacy clash prefix and digits
    ///   4. that test is one the document or the picked XML holds
    ///   5. it carries no comment and no redline, a person's comment making it theirs, and
    ///      redlines that could not be read proving nothing
    ///
    /// The codes are the ones the caller knows, the map's and the group's own, so a pair of
    /// codes nobody knows is kept. What is left is a person who saved a viewpoint with exactly a
    /// legacy name in exactly a legacy folder, design part 8 risk 1. Over the baseline's real
    /// tree it must read exactly 2813 and none of the 34 the NWCs brought, which probe P8's dump
    /// proves before anything of this rule removes a viewpoint on a real run.
    /// </summary>
    public static class LegacyClashView
    {
        /// <summary>The test a per clash viewpoint of an earlier run was made for, or null where it is not one.</summary>
        public static string TestOf(
            IList<string> folders,
            string leaf,
            bool isFolder,
            int comments,
            int? redlines,
            ICollection<string> knownCodes,
            ICollection<string> testNames,
            ViewpointSettings settings)
        {
            if (settings == null)
            {
                throw new ArgumentNullException("settings");
            }

            if (isFolder || comments != 0 || redlines != 0 || folders == null || leaf == null
                || knownCodes == null || testNames == null)
            {
                return null;
            }

            if (!FoldersAsF85WroteThem(folders, knownCodes, settings))
            {
                return null;
            }

            string marker = settings.NameSeparator + settings.LegacyClashPrefix;
            int at = leaf.LastIndexOf(marker, StringComparison.Ordinal);

            if (at <= 0 || string.IsNullOrEmpty(settings.LegacyClashPrefix))
            {
                return null;
            }

            string digits = leaf.Substring(at + marker.Length);

            if (digits.Length == 0)
            {
                return null;
            }

            foreach (char c in digits)
            {
                if (c < '0' || c > '9')
                {
                    return null;
                }
            }

            string test = leaf.Substring(0, at);

            foreach (string name in testNames)
            {
                if (string.Equals(name, test, StringComparison.Ordinal))
                {
                    return test;
                }
            }

            return null;
        }

        private static bool FoldersAsF85WroteThem(IList<string> folders, ICollection<string> knownCodes, ViewpointSettings settings)
        {
            if (folders.Count < 1 || folders.Count > 3)
            {
                return false;
            }

            int at = 0;

            if (IsAPriorityWord(folders[0], settings))
            {
                at++;
            }

            if (at >= folders.Count || !IsACodePair(folders[at], knownCodes, settings))
            {
                return false;
            }

            at++;

            if (at < folders.Count && string.Equals(folders[at], settings.SubGroupFolderName(), StringComparison.Ordinal))
            {
                at++;
            }

            return at == folders.Count;
        }

        private static bool IsAPriorityWord(string folder, ViewpointSettings settings)
        {
            foreach (ClashPriority priority in Priorities.All)
            {
                if (string.Equals(folder, Priorities.Words(priority), StringComparison.Ordinal))
                {
                    return true;
                }
            }

            return string.Equals(folder, settings.NoPriorityFolder, StringComparison.Ordinal);
        }

        private static bool IsACodePair(string folder, ICollection<string> knownCodes, ViewpointSettings settings)
        {
            if (string.IsNullOrEmpty(settings.PairSeparator))
            {
                return false;
            }

            int at = folder.IndexOf(settings.PairSeparator, StringComparison.Ordinal);

            if (at <= 0)
            {
                return false;
            }

            string first = folder.Substring(0, at);
            string second = folder.Substring(at + settings.PairSeparator.Length);

            return IsACode(first, knownCodes, settings)
                && IsACode(second, knownCodes, settings)
                && string.CompareOrdinal(first, second) <= 0;
        }

        private static bool IsACode(string word, ICollection<string> knownCodes, ViewpointSettings settings)
        {
            if (string.Equals(word, settings.UnknownDiscipline, StringComparison.Ordinal))
            {
                return true;
            }

            foreach (string code in knownCodes)
            {
                if (string.Equals(code, word, StringComparison.Ordinal))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
