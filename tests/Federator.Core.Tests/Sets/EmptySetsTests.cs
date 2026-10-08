using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using Federator.Core.Exchange;
using Federator.Core.Health;
using Federator.Core.Naming;
using Federator.Core.Sets;
using NUnit.Framework;

namespace Federator.Core.Tests.Sets
{
    /// <summary>
    /// 3b of the drift round, and the thing Bader's own report made urgent. His 1A02MM
    /// workbook holds 1,830 clash test blocks, 1,781 of which found zero, and 1,677 of
    /// the tests touch one of the 43 sets that never produce a clash. Nothing in this
    /// tool told him which of those sets is WRONG and which is a model with no such
    /// content, and those are two completely different problems.
    /// </summary>
    [TestFixture]
    public class EmptySetsTests
    {
        private static ReadCondition Category(string value)
        {
            return new ReadCondition("LcRevitData_Element", EmptySets.CategoryProperty, "equals", value);
        }

        private static ReadCondition Workset(string value)
        {
            return new ReadCondition("LcRevitData_Element", EmptySets.WorksetProperty, "equals", value);
        }

        /// <summary>
        /// Judged against the names inside Core alone, which is a file read with no list beside
        /// it, for a group of the project the lists were measured on, FR-011.
        /// </summary>
        private static EmptySet Why(string path, List<ReadCondition> asked)
        {
            return EmptySets.Why(path, asked, Judge(null, RevitCategories.Project));
        }

        /// <summary>A judge holding the names inside Core and those listed, for a group of that project.</summary>
        private static EmptySetJudge Judge(IEnumerable<string> listed, string project)
        {
            return new EmptySetJudge(RevitWorksets.With(listed), project, null);
        }

        private static string Joined(IList<string> lines)
        {
            return string.Join("\n", new List<string>(lines).ToArray());
        }

        /// <summary>
        /// Bucket one. `ME-DUCTWORK` is what his saved sets ask and no model carries it,
        /// and the nearest the models do carry is `ME-Ductwork`, which is a suggestion
        /// and never a correction.
        /// </summary>
        [Test]
        public void ASetAskingForAValueNoModelCarriesIsNamedWithTheNearestOneThatIs()
        {
            EmptySet why = Why(
                "a/BLD-ME-Ducts", new List<ReadCondition> { Workset("ME-DUCTWORK") });

            Assert.That(why.Reason, Is.EqualTo(EmptyReason.NoModelCarriesTheValue));
            Assert.That(why.Asked, Is.EqualTo("ME-DUCTWORK"));
            Assert.That(why.Nearest, Is.EqualTo("ME-Ductwork"));
            Assert.That(why.Line(), Does.Contain("NO MODEL MEASURED SO FAR IN THIS PROJECT CARRIES IT"));
            Assert.That(why.Line(), Does.Contain("a suggestion and not a correction"));
        }

        /// <summary>
        /// A SET IS WRONG ONLY WHERE EVERY OR GROUP OF IT ASKS A VALUE NO MODEL CARRIES, the
        /// breaker's finding on F115's third pass. The judge read the conditions one by one and
        /// called the set wrong on the first value nothing carries, so a set of two Or groups,
        /// (Ducts and a workset nobody has) or (Ducts and a workset the models carry), the shape
        /// every also-ask line of F131 writes, was told its condition is wrong and to fix the very
        /// spelling that is harmless, while the set can match through its other group.
        /// </summary>
        [Test]
        public void ASetWithAnOrGroupAskingACarriedValueIsNotCalledWrongForTheOtherGroup()
        {
            EmptySet why = Why(
                "a/BLD-ME-Ducts",
                new List<ReadCondition>
                {
                    Category("Ducts"),
                    Workset("ME-NOBODY"),
                    new ReadCondition("LcRevitData_Element", EmptySets.CategoryProperty, "equals", "Ducts", PlannedCondition.StartGroupFlag),
                    Workset("ME-Ductwork")
                });

            Assert.That(why.Reason, Is.EqualTo(EmptyReason.TheValueIsThereAnyway), why.Line());
            Assert.That(why.Line(), Does.Not.Contain("ME-NOBODY"));
        }

        /// <summary>The same two groups, each asking a workset nothing carries, is one wrong set, named on its first group's value.</summary>
        [Test]
        public void ASetWhoseEveryOrGroupAsksAValueNoModelCarriesIsWrong()
        {
            EmptySet why = Why(
                "a/BLD-ME-Ducts",
                new List<ReadCondition>
                {
                    Category("Ducts"),
                    Workset("ME-NOBODY"),
                    new ReadCondition("LcRevitData_Element", EmptySets.CategoryProperty, "equals", "Ducts", PlannedCondition.StartGroupFlag),
                    Workset("ME-NOONE")
                });

            Assert.That(why.Reason, Is.EqualTo(EmptyReason.NoModelCarriesTheValue), why.Line());
            Assert.That(why.Asked, Is.EqualTo("ME-NOBODY"));
        }

        /// <summary>
        /// A group the judge cannot judge at all, a Source File condition alone, may still match, so
        /// a set wrong in one group and unjudged in another is one this reader cannot tell about.
        /// </summary>
        [Test]
        public void ASetWrongInOneGroupAndUnjudgedInAnotherIsOneTheReaderCannotTellAbout()
        {
            EmptySet why = Why(
                "a/BLD-AR-Ramps",
                new List<ReadCondition>
                {
                    Workset("ME-NOBODY"),
                    new ReadCondition(string.Empty, "LcOaNodeSourceFile", "contains", "-AR-", PlannedCondition.StartGroupFlag)
                });

            Assert.That(why.Reason, Is.EqualTo(EmptyReason.CannotTell), why.Line());
        }

        /// <summary>
        /// The judge knows the spellings it is handed, the names inside Core and those of the list
        /// beside the picked XML, RevitWorksets.With, the ones the corrections asked, F116 on the
        /// Q113 pass. ME-DUCTWORK is a spelling of this project's list and not of Core, so with the
        /// list models in this project carry it and with Core's names alone none does.
        /// </summary>
        [Test]
        public void ASpellingTheListBesideThePickedFileHoldsIsOneTheModelsCarry()
        {
            EmptySet withTheList = EmptySets.Why(
                "a/BLD-ME-Ducts", new List<ReadCondition> { Workset("ME-DUCTWORK") }, Judge(new[] { "ME-DUCTWORK" }, RevitWorksets.Project));

            Assert.That(withTheList.Reason, Is.EqualTo(EmptyReason.TheValueIsThereAnyway), withTheList.Line());
            Assert.That(
                EmptySets.Why("a/BLD-ME-Ducts", new List<ReadCondition> { Workset("ME-DUCTWORK") }, Judge(null, RevitWorksets.Project)).Reason,
                Is.EqualTo(EmptyReason.NoModelCarriesTheValue));
        }

        /// <summary>Bucket two. The value is really there, so the fault is somewhere else and a person looks.</summary>
        [Test]
        public void ASetAskingForAValueTheModelsDoCarrySaysSomethingElseIsWrong()
        {
            EmptySet why = Why(
                "a/BLD-ME-Piping", new List<ReadCondition> { Workset("ME-Piping") });

            Assert.That(why.Reason, Is.EqualTo(EmptyReason.TheValueIsThereAnyway));
            Assert.That(why.Line(), Does.Contain("WHICH MODELS IN THIS PROJECT DO CARRY"));
            Assert.That(why.Line(), Does.Contain("this reader cannot tell which"));

            // THE LIST IS THE WHOLE PROJECT AND NOT THIS GROUP, so the line must not
            // claim something else is wrong. A group holding two disciplines out of seven
            // lands most of the 61 sets here, 33 of 54 on 1000BS, and every one of those
            // is ordinary.
            Assert.That(why.Line(), Does.Contain("holds no model of that kind"));
        }

        /// <summary>
        /// Bucket three. A property nobody measured a list for is one this reader has no
        /// opinion about, and it says so rather than reporting every set as wrong.
        /// </summary>
        [Test]
        public void ASetAskingOnAPropertyWithNoMeasuredListSaysItCannotTell()
        {
            EmptySet why = Why(
                "a/BLD-AR-Source",
                new List<ReadCondition> { new ReadCondition(string.Empty, "LcOaNodeSourceFile", "contains", "-AR-") });

            Assert.That(why.Reason, Is.EqualTo(EmptyReason.CannotTell));
            Assert.That(why.Line(), Does.Contain("CANNOT TELL WHY"));
        }

        [Test]
        public void ASetWithNoConditionsAtAllAlsoSaysItCannotTell()
        {
            Assert.That(
                Why("a/b", new List<ReadCondition>()).Reason,
                Is.EqualTo(EmptyReason.CannotTell));
        }

        /// <summary>
        /// The cost line, which is the number his report made urgent. A set that finds
        /// nothing is not one dead set, it is every clash test that points at it.
        /// </summary>
        [Test]
        public void TheBlockSaysWhatTheEmptySetsCostInClashTests()
        {
            IList<EmptySet> empty = new List<EmptySet>
            {
                Why("a/BLD-ME-Ducts", new List<ReadCondition> { Workset("ME-DUCTWORK") })
            };

            string block = Joined(EmptySets.Lines(empty, 1677, 1830));

            Assert.That(block, Does.Contain("IT COSTS 1677 of this group's 1830 clash tests"));
            Assert.That(block, Does.Contain("can never report a clash"));
        }

        [Test]
        public void TheBlockCountsTheThreeBucketsSeparately()
        {
            IList<EmptySet> empty = new List<EmptySet>
            {
                Why("a", new List<ReadCondition> { Workset("ME-DUCTWORK") }),
                Why("b", new List<ReadCondition> { Workset("ME-Piping") }),
                Why("c", new List<ReadCondition>())
            };

            string block = Joined(EmptySets.Lines(empty, 10, 100));

            Assert.That(block, Does.Contain("3 set(s) found nothing"));
            Assert.That(block, Does.Contain("1 ask for a value NO MODEL MEASURED SO FAR IN THIS PROJECT CARRIES"));
            Assert.That(block, Does.Contain("1 ask for a value models in this project DO carry"));
            Assert.That(block, Does.Contain("1 this reader cannot tell about"));
        }

        /// <summary>
        /// The block is ABSENT when no set is empty, rather than written saying none.
        /// A block that appears on every run saying nothing is wrong is one people stop
        /// reading, which costs the runs where something is.
        /// </summary>
        [Test]
        public void TheBlockIsAbsentWhenNoSetIsEmpty()
        {
            Assert.That(EmptySets.Lines(new List<EmptySet>(), 0, 1830), Is.Empty);
            Assert.That(EmptySets.Lines(null, 0, 1830), Is.Empty);
        }

        [Test]
        public void ACostLineWithNoTestCountSaysUnknownRatherThanZero()
        {
            IList<EmptySet> empty = new List<EmptySet>
            {
                Why("a", new List<ReadCondition> { Workset("ME-DUCTWORK") })
            };

            Assert.That(Joined(EmptySets.Lines(empty, 0, 0)), Does.Contain("UNKNOWN"));
        }

        /// <summary>
        /// A CONTAINS CONDITION IS A STEM AND NOT A WHOLE VALUE, FR-010. The corrected matrix asks
        /// contains Cable Tray, Conduit and Devices, and the models carry Cable Trays, Conduits and
        /// five Devices categories. Compared as equals, each was called a value NO MODEL IN THIS
        /// PROJECT CARRIES, where the HEALTH block, by the same list, says they are carried.
        /// </summary>
        [Test]
        public void AContainsConditionIsJudgedByThePartOfANameItAsksFor()
        {
            foreach (string stem in new[] { "Cable Tray", "Conduit", "Devices" })
            {
                EmptySet why = Why("a/BLD-EL-" + stem, new List<ReadCondition>
                {
                    new ReadCondition("LcRevitData_Element", EmptySets.CategoryProperty, "contains", stem)
                });

                Assert.That(why.Reason, Is.EqualTo(EmptyReason.TheValueIsThereAnyway), stem);
            }

            Assert.That(
                Why("a", new List<ReadCondition> { new ReadCondition("LcRevitData_Element", EmptySets.CategoryProperty, "contains", "Nurse Call") }).Reason,
                Is.EqualTo(EmptyReason.NoModelCarriesTheValue),
                "a stem no measured category holds is still carried by no model");
        }

        /// <summary>
        /// A NEGATED CONDITION ASKS FOR EVERYTHING BUT ITS VALUE, FR-010 with FR-023's rule. A
        /// negation of a category no model carries leaves out nothing and stops nothing, 5g, so
        /// BLD-EL-Devices is never judged on the Telephone Devices it leaves out.
        /// </summary>
        [Test]
        public void ANegatedConditionIsNotJudgedAsAValueTheSetAsksFor()
        {
            EmptySet why = Why("a/BLD-EL-Devices", new List<ReadCondition>
            {
                new ReadCondition("LcRevitData_Element", EmptySets.CategoryProperty, "contains", "Devices"),
                new ReadCondition("LcRevitData_Element", EmptySets.CategoryProperty, "equals", "Telephone Devices", PlannedCondition.NegateFlag)
            });

            Assert.That(why.Reason, Is.EqualTo(EmptyReason.TheValueIsThereAnyway));
            Assert.That(why.Asked, Is.EqualTo("Devices"));
        }

        /// <summary>A comparison the file never writes is one this reader cannot judge, never read as equals.</summary>
        [Test]
        public void AnotherComparisonIsNotJudged()
        {
            Assert.That(
                Why("a", new List<ReadCondition> { new ReadCondition("LcRevitData_Element", EmptySets.CategoryProperty, "NotEqual", "Nurse Call Devices") }).Reason,
                Is.EqualTo(EmptyReason.CannotTell));
        }

        // ---------- the worksets this group's models carry, FR-027 ----------

        /// <summary>
        /// A WORKSET THIS GROUP'S MODELS CARRY IS CARRIED, FR-027, measured by this run's own
        /// EXPORT CHECK. With no list beside the picked file the names inside Core are the C02
        /// census, and 1B06BC, whose models carry ME-DUCTWORK, set 03 log line 605, would be
        /// told no model in this project carries it.
        /// </summary>
        [Test]
        public void AWorksetThisGroupsModelsCarryIsCarried()
        {
            List<ModelExport> models = new List<ModelExport>
            {
                new ModelExport("1104-PAR-1B06BC-ZZZ-ME-MOD-000001.nwc", "ME", 10, 10, 10, new List<string> { "ME-DUCTWORK", "ME-EQUIPMENT" })
            };

            EmptySetJudge group = EmptySetJudge.For(Plan(), models, new ContainerNameSettings());

            Assert.That(new List<string>(group.GroupWorksets), Is.EqualTo(new[] { "ME-DUCTWORK", "ME-EQUIPMENT" }));
            Assert.That(
                EmptySets.Why("a/BLD-ME-Ducts", new List<ReadCondition> { Workset("ME-DUCTWORK") }, group).Reason,
                Is.EqualTo(EmptyReason.TheValueIsThereAnyway));
        }

        /// <summary>And for a group of another project, a workset its own models carry is still carried.</summary>
        [Test]
        public void AWorksetAGroupOfAnotherProjectCarriesIsCarried()
        {
            List<ModelExport> models = new List<ModelExport>
            {
                new ModelExport("2207-PAR-0001AA-ZZZ-ME-MOD-000001.nwc", "ME", 10, 10, 10, new List<string> { "ME-DUCTWORK" })
            };

            EmptySetJudge group = EmptySetJudge.For(Plan(), models, new ContainerNameSettings());

            Assert.That(
                EmptySets.Why("a/BLD-ME-Ducts", new List<ReadCondition> { Workset("ME-DUCTWORK") }, group).Reason,
                Is.EqualTo(EmptyReason.TheValueIsThereAnyway));
            Assert.That(
                EmptySets.Why("a/BLD-ME-Pipes", new List<ReadCondition> { Workset("ME-PIPING") }, group).Reason,
                Is.EqualTo(EmptyReason.CannotTell), "a workset this group lacks says nothing about another project");
        }

        /// <summary>
        /// THE JUDGE AND THE EXPORT CHECK READ ONE LIST OF THE GROUP'S WORKSETS, the reviewer's
        /// finding on attempt 1. The judge gathered its own copy of the rule, every workset the
        /// models carry each once first seen first, and the copy already differed from the EXPORT
        /// CHECK's: one dropped an empty name and the other kept it. A model whose walk stopped
        /// carries no names at all, ModelExport, so it adds none.
        /// </summary>
        [Test]
        public void TheJudgeAndTheExportCheckReadOneListOfTheGroupsWorksets()
        {
            List<ModelExport> models = new List<ModelExport>
            {
                new ModelExport("1104-PAR-1B06BC-ZZZ-ME-MOD-000001.nwc", "ME", 10, 10, 10, new List<string> { "ME-Ductwork", string.Empty, "ME-Piping" }),
                new ModelExport("1104-PAR-1B06BC-ZZZ-AR-MOD-000001.nwc", "AR", 10, 10, 10, new List<string> { "ME-Ductwork", "AR-EXTERIOR" }),
                new ModelExport("1104-PAR-1B06BC-ZZZ-ST-MOD-000001.nwc", "ST", ModelExport.NotCounted, ModelExport.NotCounted, ModelExport.NotCounted, new List<string> { "ST-SUB" })
            };

            EmptySetJudge group = EmptySetJudge.For(Plan(), models, new ContainerNameSettings());

            Assert.That(new List<string>(group.GroupWorksets), Is.EqualTo(ExportCheck.WorksetsOf(models)));
            Assert.That(new List<string>(group.GroupWorksets), Is.EqualTo(new[] { "ME-Ductwork", "ME-Piping", "AR-EXTERIOR" }));
        }

        /// <summary>
        /// A NEAREST VALUE DIFFERING ONLY BY LETTER CASE SAYS SO, FR-027, because the match is
        /// case sensitive and that is the whole of what is wrong with such a set, Q68.
        /// </summary>
        [Test]
        public void ANearestValueDifferingByCaseAloneSaysSo()
        {
            EmptySet why = Why("a/BLD-AR-Walls", new List<ReadCondition> { Workset("AR-Exterior") });

            Assert.That(why.Reason, Is.EqualTo(EmptyReason.NoModelCarriesTheValue));
            Assert.That(why.Nearest, Is.EqualTo("AR-EXTERIOR"));
            Assert.That(why.Line(), Does.EndWith(
                "The nearest the models carry is \"AR-EXTERIOR\", which differs from it by letter case alone, and the match is case sensitive. It is a suggestion and not a correction"));
        }

        private static SetBuildPlan Plan()
        {
            return SetBuildPlan.From(new List<SelectionSetDefinition>());
        }

        // ---------- the lists are one project's, FR-011 ----------

        /// <summary>
        /// THE LISTS INSIDE THIS TOOL ARE ONE PROJECT'S, FR-011. A value they do not hold was
        /// called one NO MODEL IN THIS PROJECT CARRIES whatever project the run's models are of,
        /// so another project's XML would be told no model carries values its models do carry.
        /// For a group of another project the judge cannot tell, and says why.
        /// </summary>
        [Test]
        public void AGroupOfAnotherProjectIsToldTheReaderCannotTellAndWhy()
        {
            EmptySetJudge another = Judge(null, "2207");

            foreach (ReadCondition asked in new[] { Workset("ME-DUCTWORK"), Category("Nurse Call Devices"), Category("Floors") })
            {
                EmptySet why = EmptySets.Why("a/BLD-X", new List<ReadCondition> { asked }, another);

                Assert.That(why.Reason, Is.EqualTo(EmptyReason.CannotTell), asked.Value);
                Assert.That(why.Line(), Does.EndWith(
                    "THIS READER CANNOT TELL WHY, because the lists inside this tool were measured on project "
                    + RevitCategories.Project + "'s models and this group's are of project 2207"));
            }
        }

        /// <summary>A group whose models' project could not be read is not one of the lists' project, FR-011.</summary>
        [Test]
        public void AGroupWhoseProjectCouldNotBeReadIsToldTheReaderCannotTell()
        {
            EmptySet why = EmptySets.Why("a/BLD-X", new List<ReadCondition> { Category("Nurse Call Devices") }, Judge(null, null));

            Assert.That(why.Reason, Is.EqualTo(EmptyReason.CannotTell));
            Assert.That(why.Line(), Does.Contain("which project this group's models are of could not be read off their names"));
        }

        /// <summary>
        /// A GROUP WHOSE MODELS WERE NOT READ IS SAID AS NOT READ, and never as names that would not
        /// read, the reviewer's finding on attempt 1. On the Build sets button no model is read,
        /// and the line said which project the models are of could not be read off their names,
        /// which reports a read that never ran.
        /// </summary>
        [Test]
        public void AGroupWhoseModelsWereNotReadIsSaidAsNotReadAndNeverAsNamesThatWouldNotRead()
        {
            foreach (List<ModelExport> none in new[] { null, new List<ModelExport>() })
            {
                EmptySet why = EmptySets.Why(
                    "a/BLD-X",
                    new List<ReadCondition> { Category("Nurse Call Devices") },
                    EmptySetJudge.For(Plan(), none, new ContainerNameSettings()));

                Assert.That(why.Reason, Is.EqualTo(EmptyReason.CannotTell));
                Assert.That(why.Line(), Does.Not.Contain("could not be read off their names"));
                Assert.That(why.Line(), Does.EndWith(
                    "'s models and this group's models were not read, so which project they are of is UNKNOWN"));
            }
        }

        /// <summary>
        /// The project of a group is the one the project part of every model's name reads, with
        /// the naming settings the scan reads, and UNKNOWN where one would not read or two differ,
        /// never a guess, FR-011.
        /// </summary>
        [Test]
        public void TheProjectOfAGroupIsTheOneEveryModelNameReads()
        {
            ContainerNameSettings names = new ContainerNameSettings();

            Assert.That(
                EmptySetJudge.ProjectOf(new List<ModelExport> { Model("1104-PAR-1A02MM-ZZZ-AR-MOD-000001.nwc"), Model("1104-PAR-1A02MM-ZZZ-ST-MOD-000001.nwc") }, names),
                Is.EqualTo("1104"));
            Assert.That(
                EmptySetJudge.ProjectOf(new List<ModelExport> { Model("1104-PAR-1A02MM-ZZZ-AR-MOD-000001.nwc"), Model("2207-PAR-1A02MM-ZZZ-ST-MOD-000001.nwc") }, names),
                Is.Null, "two projects in one group");
            Assert.That(
                EmptySetJudge.ProjectOf(new List<ModelExport> { Model("1104-PAR-1A02MM-ZZZ-AR-MOD-000001.nwc"), Model("site model.nwc") }, names),
                Is.Null, "a name that will not read");
            Assert.That(EmptySetJudge.ProjectOf(new List<ModelExport>(), names), Is.Null, "no model read");
            Assert.That(EmptySetJudge.ProjectOf(null, names), Is.Null, "the models were not read for this group");
        }

        private static ModelExport Model(string file)
        {
            return new ModelExport(file, string.Empty, 1, 1, 1, new List<string>());
        }

        /// <summary>
        /// Each list names the project it was measured on, and the name is read off the result
        /// file of the walk that measured it, FR-011: the project part of every federation the
        /// category walk opened, 5i, and of every model the workset census read, 5t.
        /// </summary>
        [Test]
        public void EachListNamesTheProjectOfTheModelsItWasMeasuredOn()
        {
            ContainerNameSettings names = new ContainerNameSettings();
            List<string> walked = new List<string>();
            List<string> census = new List<string>();

            foreach (string line in File.ReadAllLines(Samples.ProbeResult("5i-result-20260920.txt")))
            {
                Match opened = Regex.Match(line, @"opening .*\\([^\\]+\.nwf)$");

                if (opened.Success)
                {
                    walked.Add(ContainerName.Parse(opened.Groups[1].Value, names).Project);
                }
            }

            foreach (string line in File.ReadAllLines(Samples.ProbeResult("5t-5u-result-20260920.txt")))
            {
                Match model = Regex.Match(line, @"\s(\S+\.nwc)\s+site \[");

                if (model.Success)
                {
                    census.Add(ContainerName.Parse(model.Groups[1].Value, names).Project);
                }
            }

            Assert.That(walked.Count, Is.EqualTo(10), "the ten C02 federations of 5i");
            Assert.That(census.Count, Is.GreaterThan(0));
            Assert.That(walked, Is.All.EqualTo(RevitCategories.Project));
            Assert.That(census, Is.All.EqualTo(RevitWorksets.Project));
            Assert.That(RevitCategories.Project, Is.Not.Null);
        }

        /// <summary>
        /// ONE CONSTANT HOLDS EACH PROPERTY INTERNAL NAME, FR-011. The category name was typed in
        /// EmptySets and again in HealthCheck. Every code file under src is read for the two names
        /// in quotes, and each is found once.
        /// </summary>
        [Test]
        public void OneConstantInSrcHoldsEachPropertyInternalName()
        {
            string src = Path.Combine(Samples.Repo(), "src");
            int category = 0;
            int workset = 0;

            foreach (string file in Directory.GetFiles(src, "*.cs", SearchOption.AllDirectories))
            {
                if (file.IndexOf(Path.DirectorySeparatorChar + "obj" + Path.DirectorySeparatorChar, StringComparison.Ordinal) >= 0
                    || file.IndexOf(Path.DirectorySeparatorChar + "bin" + Path.DirectorySeparatorChar, StringComparison.Ordinal) >= 0)
                {
                    continue;
                }

                string text = File.ReadAllText(file);
                category += Count(text, "\"" + EmptySets.CategoryProperty + "\"");
                workset += Count(text, "\"" + EmptySets.WorksetProperty + "\"");
            }

            Assert.That(category, Is.EqualTo(1), "the category property's internal name, in quotes, under src");
            Assert.That(workset, Is.EqualTo(1), "the workset property's internal name, in quotes, under src");
        }

        private static int Count(string text, string what)
        {
            int count = 0;

            for (int at = text.IndexOf(what, StringComparison.Ordinal); at >= 0; at = text.IndexOf(what, at + what.Length, StringComparison.Ordinal))
            {
                count++;
            }

            return count;
        }

        /// <summary>
        /// A category value the models really carry, off the measured 374, so the bucket
        /// rule is proved against the real list and not only against worksets.
        /// </summary>
        [Test]
        public void TheCategoryListIsReadTheSameWayTheWorksetListIs()
        {
            Assert.That(
                Why("a", new List<ReadCondition> { Category("Floors") }).Reason,
                Is.EqualTo(EmptyReason.TheValueIsThereAnyway));

            Assert.That(
                Why("a", new List<ReadCondition> { Category("Nurse Call Devices") }).Reason,
                Is.EqualTo(EmptyReason.NoModelCarriesTheValue),
                "the fallback F87 removed asked for a category no model in this project has");
        }
    }
}
