using System;
using System.Collections.Generic;
using System.Globalization;
using Federator.Core.Diagnostics;
using Federator.Core.Disciplines;
using Federator.Core.Health;

namespace Federator.Core.Communities
{
    /// <summary>
    /// How one group of a package ended in its own Navisworks, and since F222 what its run read for the discipline reports,
    /// its files and the facts of its models, so the window that started the run writes each package's workbooks once every
    /// group of it has run in a Navisworks of its own.
    /// </summary>
    public sealed class PackageRunGroup
    {
        private readonly List<string> files = new List<string>();
        private List<ModelFacts> facts;

        public PackageRunGroup(string group, GroupOutcome outcome, long clashes, string reason)
        {
            Group = group ?? string.Empty;
            Outcome = outcome;
            Clashes = clashes;
            Reason = reason;
            FactsNotRead = "UNKNOWN, no facts were handed in";
        }

        /// <summary>The NWC files the group's run was handed, on the NWC page. None on the NWF page, whose files are read off its models' facts.</summary>
        public IList<string> Files
        {
            get { return files; }
        }

        /// <summary>The facts read off the group's models for the discipline reports, or null with FactsNotRead.</summary>
        public IList<ModelFacts> Facts
        {
            get { return facts == null ? null : facts.AsReadOnly(); }
        }

        /// <summary>Why no facts were read, or null where they were.</summary>
        public string FactsNotRead { get; private set; }

        public PackageRunGroup WithFacts(IEnumerable<ModelFacts> read)
        {
            facts = new List<ModelFacts>(read ?? new List<ModelFacts>());
            FactsNotRead = null;
            return this;
        }

        public PackageRunGroup FactsNot(string why)
        {
            facts = null;
            FactsNotRead = string.IsNullOrEmpty(why) ? "UNKNOWN why" : why;
            return this;
        }

        /// <summary>The group as the package tally names it, the building on the NWC page and the NWF on the NWF page.</summary>
        public string Group { get; private set; }

        public GroupOutcome Outcome { get; private set; }

        /// <summary>The clashes the group found, minus one where the count was not taken.</summary>
        public long Clashes { get; private set; }

        /// <summary>Why the group is not DONE, or null.</summary>
        public string Reason { get; private set; }
    }

    /// <summary>
    /// What one group's Navisworks wrote once its run ended, F214 and since F222 one group a Navisworks: the package, its
    /// own log, the group with how it ended and what it read for the discipline reports, and the lines of its own RESULT
    /// block, so the parent joins them into one RESULT. A Navisworks that ended before it wrote this has no result, and the
    /// parent says so with why.
    /// </summary>
    public sealed class PackageRunResult
    {
        /// <summary>
        /// The first line of a result file, its last word the shape, 2 since F222 carried the discipline facts, 3 since F234
        /// each model's placement in place of its box and the files of a group not built, 4 since F243 the shared coordinates
        /// record of each group.
        /// </summary>
        public const string Header = "parsons nwc federator, the result of one group of a run of packages, 4";

        private const string ReadWord = "read";
        private const string NotReadWord = "not";

        public PackageRunResult(string package, string log)
        {
            Package = package ?? string.Empty;
            Log = log;
            Groups = new List<PackageRunGroup>();
            ResultLines = new List<string>();
            NotBuiltFiles = new List<string>();
            OffCoordinates = new List<OffCoordinatesGroup>();
        }

        public string Package { get; private set; }

        /// <summary>The package's own log, or null where it opened none.</summary>
        public string Log { get; private set; }

        public IList<PackageRunGroup> Groups { get; private set; }

        public IList<string> ResultLines { get; private set; }

        /// <summary>Why the package ran no group, or null where one ran.</summary>
        public string NoGroupBecause { get; set; }

        /// <summary>
        /// F234 on row F233-R2. The NWC files of a group its own Navisworks found to be one discipline and did not build, read
        /// off the models of its NWF, so the discipline reports name them. None for any other group.
        /// </summary>
        public IList<string> NotBuiltFiles { get; private set; }

        /// <summary>
        /// F243 attempt 2, the lead's decision on item 1. The shared coordinates check's own record of each group this
        /// Navisworks ran, its tally's groups, so the window that started the run makes the one list of models not on the
        /// same shared coordinates over every group, OffCoordinatesOf. Empty where the check reached no group.
        /// </summary>
        public IList<OffCoordinatesGroup> OffCoordinates { get; private set; }

        /// <summary>
        /// The one tally of a run of packages over the facts every group's result carried, one result a unit in the order of
        /// the units, each group in the order of its result. A unit whose result is null or carries no record adds nothing, so
        /// it is counted among the groups not judged, never clean, and since attempt 3, the breaker's 2 and 3, it is named by
        /// its label in words true for the window that started the run, PackageRunWords.NoCoordinatesRecordCameBack. Its
        /// ForModellers is the one list the window writes, its first line naming the run.
        /// </summary>
        public static OffCoordinatesAcrossTheRun OffCoordinatesOf(DateTime runStarted, IList<PackageRunUnit> units, IList<PackageRunResult> results)
        {
            if (units == null || results == null || units.Count != results.Count)
            {
                throw new ArgumentException("one result or null for each group");
            }

            OffCoordinatesAcrossTheRun run = new OffCoordinatesAcrossTheRun(runStarted, units.Count);
            List<string> none = new List<string>();

            for (int i = 0; i < units.Count; i++)
            {
                if (results[i] == null || results[i].OffCoordinates.Count == 0)
                {
                    none.Add(units[i].Label);
                    continue;
                }

                foreach (OffCoordinatesGroup group in results[i].OffCoordinates)
                {
                    run.Add(group.Building, group.Off, group.RunsATest);
                }
            }

            run.SayTheGroupsNotReachedAs(PackageRunWords.NoCoordinatesRecordCameBack(none));
            return run;
        }

        /// <summary>
        /// The lines of the last RESULT block of a log, every line after the rule under its title, the title and the rule
        /// RunLog.Section writes it with, or none where the log holds no RESULT block.
        /// </summary>
        public static IList<string> ResultLinesOf(IList<string> logLines)
        {
            List<string> lines = new List<string>();

            if (logLines == null)
            {
                return lines;
            }

            int start = -1;

            for (int i = 1; i + 1 < logLines.Count; i++)
            {
                if (logLines[i] == RunLog.ResultTitle && logLines[i - 1] == RunLog.SectionRule && logLines[i + 1] == RunLog.SectionRule)
                {
                    start = i + 2;
                }
            }

            if (start < 0)
            {
                return lines;
            }

            for (int i = start; i < logLines.Count; i++)
            {
                lines.Add(logLines[i]);
            }

            while (lines.Count > 0 && lines[lines.Count - 1].Length == 0)
            {
                lines.RemoveAt(lines.Count - 1);
            }

            return lines;
        }

        public void Write(string path)
        {
            List<string> lines = new List<string>
            {
                PackageRunFile.Line("package", Package),
                PackageRunFile.Line("log", Log ?? string.Empty)
            };

            if (NoGroupBecause != null)
            {
                lines.Add(PackageRunFile.Line("nogroup", NoGroupBecause));
            }

            foreach (string file in NotBuiltFiles)
            {
                lines.Add(PackageRunFile.Line("notbuiltfile", file));
            }

            foreach (PackageRunGroup group in Groups)
            {
                lines.Add(PackageRunFile.Line(
                    "group",
                    group.Group,
                    group.Outcome.ToString(),
                    group.Clashes.ToString(CultureInfo.InvariantCulture),
                    group.Reason ?? string.Empty));
                WriteTheFactsOf(group, lines);
            }

            foreach (string line in ResultLines)
            {
                lines.Add(PackageRunFile.Line("result", line));
            }

            foreach (OffCoordinatesGroup group in OffCoordinates)
            {
                WriteTheOffCoordinatesOf(group, lines);
            }

            lines.Add(PackageRunFile.Line("end", "written whole"));
            PackageRunFile.Write(path, Header, lines);
        }

        private const string Yes = "yes";
        private const string No = "no";

        /// <summary>
        /// F243 attempt 2. One group's shared coordinates record: an offgroup line, its name, whether a clash test was to run,
        /// whether the check read it, whether a reference was placed and which, the models not judged and the models read,
        /// then one offmodel line a model off, one offinternal line a model on Internal and one offnosite line a model naming
        /// no site, under it. A read that threw writes the word not and nothing under it.
        /// </summary>
        private static void WriteTheOffCoordinatesOf(OffCoordinatesGroup group, List<string> lines)
        {
            OffCoordinates off = group.Off;

            lines.Add(PackageRunFile.Line(
                "offgroup",
                group.Building,
                group.RunsATest ? Yes : No,
                off == null ? NotReadWord : ReadWord,
                off == null || off.Reference == null ? NotReadWord : ReadWord,
                off == null ? string.Empty : off.Reference ?? string.Empty,
                Number(off == null ? 0 : off.NotJudged),
                Number(off == null ? 0 : off.ModelsRead)));

            if (off == null)
            {
                return;
            }

            foreach (string model in off.Models)
            {
                lines.Add(PackageRunFile.Line("offmodel", model));
            }

            foreach (string model in off.OnInternal)
            {
                lines.Add(PackageRunFile.Line("offinternal", model));
            }

            foreach (string model in off.NoSite)
            {
                lines.Add(PackageRunFile.Line("offnosite", model));
            }
        }

        private static string Number(int value)
        {
            return value.ToString(CultureInfo.InvariantCulture);
        }

        /// <summary>One group's shared coordinates record read back, its lists gathered until the result has been read whole.</summary>
        private sealed class OffRead
        {
            public string Building;
            public bool RunsATest;
            public bool Read;
            public string Reference;
            public int NotJudged;
            public int ModelsRead;
            public readonly List<string> Models = new List<string>();
            public readonly List<string> OnInternal = new List<string>();
            public readonly List<string> NoSite = new List<string>();

            public OffCoordinatesGroup Group()
            {
                return new OffCoordinatesGroup(
                    Building,
                    Read ? new OffCoordinates(Reference, Models, NotJudged, ModelsRead, OnInternal, NoSite) : null,
                    RunsATest);
            }
        }

        /// <summary>The fault of a shared coordinates line, or null where it read into the record it belongs to.</summary>
        private static string TakeAnOffLine(string[] line, List<OffRead> offs)
        {
            if (line[0] == "offgroup")
            {
                int notJudged;
                int modelsRead;

                if (line.Length != 8)
                {
                    return "an offgroup line holds " + (line.Length - 1) + " values where it holds 7";
                }

                if ((line[2] != Yes && line[2] != No) || (line[3] != ReadWord && line[3] != NotReadWord)
                    || (line[4] != ReadWord && line[4] != NotReadWord))
                {
                    return "the offgroup line of " + line[1] + " reads " + line[2] + " " + line[3] + " " + line[4]
                        + " where it reads yes or no, then read or not twice";
                }

                if (!int.TryParse(line[6], NumberStyles.None, CultureInfo.InvariantCulture, out notJudged)
                    || !int.TryParse(line[7], NumberStyles.None, CultureInfo.InvariantCulture, out modelsRead))
                {
                    return "the counts " + line[6] + " and " + line[7] + " of " + line[1] + " are not counts";
                }

                offs.Add(new OffRead
                {
                    Building = line[1],
                    RunsATest = line[2] == Yes,
                    Read = line[3] == ReadWord,
                    Reference = line[4] == ReadWord ? line[5] : null,
                    NotJudged = notJudged,
                    ModelsRead = modelsRead
                });
                return null;
            }

            if (line.Length != 2)
            {
                return "an " + line[0] + " line holds " + (line.Length - 1) + " values where it holds 1";
            }

            OffRead last = offs.Count == 0 ? null : offs[offs.Count - 1];

            if (last == null || !last.Read)
            {
                return "an " + line[0] + " line comes with no offgroup line read above it";
            }

            (line[0] == "offmodel" ? last.Models : line[0] == "offinternal" ? last.OnInternal : last.NoSite).Add(line[1]);
            return null;
        }

        /// <summary>
        /// F222. The lines under a group line: each file, then whether the facts were read, then a model line each and the
        /// CAD and Generic Models lines of the model above them. A value not read is written with the word not and its why,
        /// so a box, a site or a walk not read never reads back as a zero or as none found.
        /// </summary>
        private static void WriteTheFactsOf(PackageRunGroup group, List<string> lines)
        {
            foreach (string file in group.Files)
            {
                lines.Add(PackageRunFile.Line("file", file));
            }

            if (group.Facts == null)
            {
                lines.Add(PackageRunFile.Line("facts", NotReadWord, group.FactsNotRead));
                return;
            }

            lines.Add(PackageRunFile.Line("facts", ReadWord, string.Empty));

            foreach (ModelFacts model in group.Facts)
            {
                lines.Add(PackageRunFile.Line(
                    "model",
                    model.File,
                    model.IsPlaced ? ReadWord : NotReadWord,
                    model.PlacementNotRead ?? string.Empty,
                    Number(model.X),
                    Number(model.Y),
                    Number(model.Z),
                    model.SiteNotRead == null ? ReadWord : NotReadWord,
                    model.SiteNotRead ?? model.Site,
                    model.CadNotRead == null ? ReadWord : NotReadWord,
                    model.CadNotRead ?? string.Empty,
                    model.GenericNotRead == null ? ReadWord : NotReadWord,
                    model.GenericNotRead ?? string.Empty));

                foreach (ElementFact cad in model.Cad)
                {
                    lines.Add(ElementLine("cad", cad));
                }

                foreach (ElementFact generic in model.Generic)
                {
                    lines.Add(ElementLine("generic", generic));
                }
            }
        }

        private static string ElementLine(string key, ElementFact element)
        {
            return PackageRunFile.Line(key, element.ElementId, element.Name, element.Family, element.Type, element.Level);
        }

        private static string Number(double value)
        {
            return value.ToString("G17", CultureInfo.InvariantCulture);
        }

        /// <summary>One model read back, its CAD and Generic Models lines gathered until the result has been read whole.</summary>
        private sealed class ModelRead
        {
            public ModelFacts Model;
            public string CadWhy;
            public string GenericWhy;
            public readonly List<ElementFact> Cad = new List<ElementFact>();
            public readonly List<ElementFact> Generic = new List<ElementFact>();
        }

        /// <summary>One group's facts read back, null models where its facts line said not read.</summary>
        private sealed class FactsRead
        {
            public PackageRunGroup Group;
            public List<ModelRead> Models;
        }

        /// <summary>The fault of a line under a group, or null where it read into the group above it.</summary>
        private static string TakeAFactsLine(string[] line, PackageRunGroup group, Dictionary<PackageRunGroup, FactsRead> facts)
        {
            string key = line[0];

            if (group == null)
            {
                return "a " + key + " line comes before any group line";
            }

            FactsRead read;
            facts.TryGetValue(group, out read);

            if (key == "file")
            {
                if (line.Length != 2)
                {
                    return "a file line holds " + (line.Length - 1) + " values where it holds 1";
                }

                group.Files.Add(line[1]);
                return null;
            }

            if (key == "facts")
            {
                if (line.Length != 3 || (line[1] != ReadWord && line[1] != NotReadWord))
                {
                    return "a facts line reads " + string.Join(" ", line, 1, line.Length - 1) + " where it reads read or not and why";
                }

                if (read != null)
                {
                    return "the group " + group.Group + " has two facts lines";
                }

                facts.Add(group, new FactsRead { Group = group, Models = line[1] == ReadWord ? new List<ModelRead>() : null });

                if (line[1] == NotReadWord)
                {
                    group.FactsNot(line[2]);
                }

                return null;
            }

            if (read == null || read.Models == null)
            {
                return "a " + key + " line of " + group.Group + " comes with no facts line saying its facts were read";
            }

            if (key == "model")
            {
                if (line.Length != 13)
                {
                    return "a model line holds " + (line.Length - 1) + " values where it holds 12";
                }

                double[] at = new double[3];

                for (int i = 0; i < 3; i++)
                {
                    if (!double.TryParse(line[4 + i], NumberStyles.Float, CultureInfo.InvariantCulture, out at[i]))
                    {
                        return "the placement of " + line[1] + " holds " + line[4 + i] + ", not a number";
                    }
                }

                foreach (int flag in new[] { 2, 7, 9, 11 })
                {
                    if (line[flag] != ReadWord && line[flag] != NotReadWord)
                    {
                        return "the model line of " + line[1] + " reads " + line[flag] + " where it reads read or not";
                    }
                }

                ModelFacts model = new ModelFacts(line[1]);
                model = line[2] == ReadWord ? model.WithPlacement(at[0], at[1], at[2]) : model.PlacementNot(line[3]);
                model = line[7] == ReadWord ? model.WithSite(line[8]) : model.SiteNot(line[8]);
                read.Models.Add(new ModelRead
                {
                    Model = model,
                    CadWhy = line[9] == ReadWord ? null : line[10],
                    GenericWhy = line[11] == ReadWord ? null : line[12]
                });
                return null;
            }

            if (key == "cad" || key == "generic")
            {
                if (line.Length != 6)
                {
                    return "a " + key + " line holds " + (line.Length - 1) + " values where it holds 5";
                }

                if (read.Models.Count == 0)
                {
                    return "a " + key + " line of " + group.Group + " comes before any model line";
                }

                ModelRead last = read.Models[read.Models.Count - 1];
                ElementFact element = new ElementFact(line[1], line[2], line[3], line[4], line[5]);

                if ((key == "cad" ? last.CadWhy : last.GenericWhy) != null)
                {
                    return "a " + key + " line of " + last.Model.File + " comes where its walk was not read";
                }

                (key == "cad" ? last.Cad : last.Generic).Add(element);
                return null;
            }

            return "the key " + key + " is not one a result holds";
        }

        /// <summary>Each group's facts put together once the result has been read whole, every walk with what it found.</summary>
        private static void PutTheFactsOn(Dictionary<PackageRunGroup, FactsRead> facts)
        {
            foreach (FactsRead read in facts.Values)
            {
                if (read.Models == null)
                {
                    continue;
                }

                List<ModelFacts> models = new List<ModelFacts>();

                foreach (ModelRead model in read.Models)
                {
                    ModelFacts whole = model.CadWhy == null ? model.Model.WithCad(model.Cad) : model.Model.CadNot(model.CadWhy);
                    models.Add(model.GenericWhy == null ? whole.WithGeneric(model.Generic) : whole.GenericNot(model.GenericWhy));
                }

                read.Group.WithFacts(models);
            }
        }

        /// <summary>
        /// The result at this path, or null with why: the file not there or not read, a line that does not read, or no
        /// end line, which is a file its Navisworks stopped writing part way.
        /// </summary>
        public static PackageRunResult Read(string path, out string why)
        {
            IList<string[]> lines = PackageRunFile.Read(path, Header, out why);

            if (lines == null)
            {
                return null;
            }

            string package = null;
            string log = null;
            string noGroup = null;
            bool ended = false;
            List<PackageRunGroup> groups = new List<PackageRunGroup>();
            List<string> result = new List<string>();
            List<string> notBuilt = new List<string>();
            List<OffRead> offs = new List<OffRead>();
            Dictionary<PackageRunGroup, FactsRead> facts = new Dictionary<PackageRunGroup, FactsRead>();

            foreach (string[] line in lines)
            {
                string fault = null;

                if (line[0] == "file" || line[0] == "facts" || line[0] == "model" || line[0] == "cad" || line[0] == "generic")
                {
                    fault = TakeAFactsLine(line, groups.Count == 0 ? null : groups[groups.Count - 1], facts);
                }
                else if (line[0] == "offgroup" || line[0] == "offmodel" || line[0] == "offinternal" || line[0] == "offnosite")
                {
                    fault = TakeAnOffLine(line, offs);
                }
                else if (line[0] == "group")
                {
                    GroupOutcome outcome;
                    long clashes;

                    if (line.Length != 5)
                    {
                        fault = "a group line holds " + (line.Length - 1) + " values where it holds 4";
                    }
                    else if (!Enum.TryParse(line[2], false, out outcome) || !Enum.IsDefined(typeof(GroupOutcome), outcome)
                        || line[2] != outcome.ToString())
                    {
                        fault = "the outcome " + line[2] + " of " + line[1] + " is not one a group ends in";
                    }
                    else if (!long.TryParse(line[3], NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out clashes) || clashes < -1)
                    {
                        fault = "the clashes " + line[3] + " of " + line[1] + " are not a count";
                    }
                    else
                    {
                        groups.Add(new PackageRunGroup(line[1], outcome, clashes, line[4].Length == 0 ? null : line[4]));
                    }
                }
                else if (line.Length != 2)
                {
                    fault = "the line " + line[0] + " holds " + (line.Length - 1) + " values where it holds 1";
                }
                else if (line[0] == "package")
                {
                    package = line[1];
                }
                else if (line[0] == "log")
                {
                    log = line[1].Length == 0 ? null : line[1];
                }
                else if (line[0] == "nogroup")
                {
                    noGroup = line[1];
                }
                else if (line[0] == "notbuiltfile")
                {
                    notBuilt.Add(line[1]);
                }
                else if (line[0] == "result")
                {
                    result.Add(line[1]);
                }
                else if (line[0] == "end")
                {
                    ended = true;
                }
                else
                {
                    fault = "the key " + line[0] + " is not one a result holds";
                }

                if (fault != null)
                {
                    why = "the result " + path + " has a line that does not read, " + fault;
                    return null;
                }
            }

            if (!ended || package == null)
            {
                why = "the result " + path + " stops before its end, so its Navisworks stopped writing it part way";
                return null;
            }

            PutTheFactsOn(facts);
            PackageRunResult read = new PackageRunResult(package, log) { NoGroupBecause = noGroup };

            foreach (PackageRunGroup group in groups)
            {
                read.Groups.Add(group);
            }

            foreach (string line in result)
            {
                read.ResultLines.Add(line);
            }

            foreach (string file in notBuilt)
            {
                read.NotBuiltFiles.Add(file);
            }

            foreach (OffRead off in offs)
            {
                read.OffCoordinates.Add(off.Group());
            }

            why = null;
            return read;
        }
    }
}
