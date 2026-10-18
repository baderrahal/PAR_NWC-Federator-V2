using System;
using System.Collections.Generic;
using System.Linq;
using Federator.Core.Disciplines;
using Federator.Core.Naming;
using NUnit.Framework;

namespace Federator.Core.Tests
{
    /// <summary>
    /// F209 and F234, the checks of the discipline reports. Coordinates are proved on the translations the ALIGNMENT block of
    /// 1B06BC read on 19 Oct 2026, in millimetres, and each check is broken one way and asserted to name it, or to name
    /// nothing where nothing needs action.
    /// </summary>
    [TestFixture]
    public class DisciplineChecksTests
    {
        private static readonly ContainerNameSettings Names = new ContainerNameSettings();
        private static readonly DisciplineReportSettings Settings = new DisciplineReportSettings();

        private static string Nwc(string building, string discipline, string number = "000001")
        {
            return TestPaths.At("Parent", "C06", "1104-PAR-" + building + "-ZZZ-" + discipline + "-MOD-" + number + ".nwc");
        }

        private static ModelFacts Model(string file, double x, double y, double z, string site)
        {
            return new ModelFacts(file).WithPlacement(x, y, z).WithSite(site).WithCad(new List<ElementFact>()).WithGeneric(new List<ElementFact>());
        }

        /// <summary>1B06BC as its ALIGNMENT block read it on 19 Oct 2026, run-20261019-010707, AR the reference on Internal.</summary>
        private static List<DisciplineGroup> Bc()
        {
            ModelFacts[] models =
            {
                Model(Nwc("1B06BC", "AR"), 0, 0, 0, "Internal"),
                Model(Nwc("1B06BC", "EL"), 323886396.13, 2755364306.53, 11033.3, "SITEWIDE PHASE 3"),
                Model(Nwc("1B06BC", "ME"), 0, 0, 0, "16-S01"),
                Model(Nwc("1B06BC", "ST"), -33975.57, -1624.92, 0, "T1"),
                Model(Nwc("1B06BC", "ST", "000002"), 34195.56, 452.06, -4331, "BCTANK")
            };

            return new List<DisciplineGroup> { new DisciplineGroup("1B06BC", models.Select(m => m.File), models, null) };
        }

        // ---------- coordinates ----------

        [Test]
        public void OnTheTranslationsOf1B06BCBothStModelsSit34MetresFromArEachARowOfTheStWorkbook()
        {
            CheckFindings found = CoordinatesCheck.Of(Bc(), Settings, Names);

            List<DisciplineIssue> st = found.Issues.Where(i => i.Discipline == "ST").ToList();
            Assert.That(st.Count, Is.EqualTo(2));
            Assert.That(st[0].Value, Is.EqualTo("34.014 m, ST on T1, AR on Internal"));
            Assert.That(st[1].Value, Is.EqualTo("34.472 m, ST on BCTANK, AR on Internal"));
            Assert.That(st[0].WhatIsWrong, Is.EqualTo("more than 30 m from the AR model"));
            Assert.That(st[0].Action, Is.EqualTo("Export again on the project shared coordinates"));
            Assert.That(st[0].Building, Is.EqualTo("1B06BC"));
            Assert.That(st[0].File, Is.EqualTo("1104-PAR-1B06BC-ZZZ-ST-MOD-000001.nwc"), "the file's name, never its folder");
            Assert.That(st[0].Severity, Is.EqualTo(CheckWords.High));
            Assert.That(found.Issues.Single(i => i.Discipline == "EL").Value, Does.StartWith("2774335.030 m, EL on SITEWIDE PHASE 3"));
            Assert.That(found.Issues.Any(i => i.Discipline == "ME"), Is.False, "ME sits where AR sits and needs nothing");
            Assert.That(found.FilesCheckedFor("ST"), Is.EqualTo(2));
            Assert.That(found.FilesCheckedFor("AR"), Is.EqualTo(1), "the reference is read too");
        }

        [Test]
        public void AReferenceOnInternalWhileAnotherModelNamesASharedSiteIsOneRowOfTheReferencesWorkbook()
        {
            DisciplineIssue ar = CoordinatesCheck.Of(Bc(), Settings, Names).Issues.Single(i => i.Discipline == "AR");

            Assert.That(ar.WhatIsWrong, Is.EqualTo("on Revit internal origin"));
            Assert.That(ar.Value, Is.EqualTo("the other models on SITEWIDE PHASE 3, 16-S01, T1, BCTANK"));
            Assert.That(ar.Action, Is.EqualTo("Export on the project shared coordinates"));
            Assert.That(ar.File, Is.EqualTo("1104-PAR-1B06BC-ZZZ-AR-MOD-000001.nwc"));
        }

        [Test]
        public void AReferenceOnASharedSiteOrWithEveryOtherModelOnInternalGetsNoInternalRow()
        {
            List<DisciplineGroup> shared = new List<DisciplineGroup>
            {
                new DisciplineGroup("1B06PS", new[] { Nwc("1B06PS", "AR"), Nwc("1B06PS", "EL") },
                    new[] { Model(Nwc("1B06PS", "AR"), 0, 0, 0, "S1"), Model(Nwc("1B06PS", "EL"), 0, 0, 0, "Internal") }, null)
            };
            List<DisciplineGroup> allInternal = new List<DisciplineGroup>
            {
                new DisciplineGroup("1B06PS", new[] { Nwc("1B06PS", "AR"), Nwc("1B06PS", "EL") },
                    new[] { Model(Nwc("1B06PS", "AR"), 0, 0, 0, "Internal"), Model(Nwc("1B06PS", "EL"), 0, 0, 0, "Internal") }, null)
            };

            Assert.That(CoordinatesCheck.Of(shared, Settings, Names).Issues, Is.Empty);
            Assert.That(CoordinatesCheck.Of(allInternal, Settings, Names).Issues, Is.Empty);
        }

        [Test]
        public void FarIsMoreThanTheSettingSoExactly30MetresIsNoRow()
        {
            Func<double, int> rows = metres => CoordinatesCheck.Of(new List<DisciplineGroup>
            {
                new DisciplineGroup("1B06PS", new[] { Nwc("1B06PS", "AR"), Nwc("1B06PS", "EL") },
                    new[] { Model(Nwc("1B06PS", "AR"), 0, 0, 0, "S"), Model(Nwc("1B06PS", "EL"), metres * 1000.0, 0, 0, "S") }, null)
            }, Settings, Names).Issues.Count;

            Assert.That(rows(29.999), Is.EqualTo(0));
            Assert.That(rows(30.0), Is.EqualTo(0));
            Assert.That(rows(30.001), Is.EqualTo(1));
            Assert.That(CoordinatesCheck.Of(new List<DisciplineGroup>
            {
                new DisciplineGroup("1B06PS", new[] { Nwc("1B06PS", "AR"), Nwc("1B06PS", "EL") },
                    new[] { Model(Nwc("1B06PS", "AR"), 0, 0, 0, "S"), Model(Nwc("1B06PS", "EL"), 5000.0, 0, 0, "S") }, null)
            }, new DisciplineReportSettings { FarMetres = 4.0 }, Names).Issues.Single().WhatIsWrong, Is.EqualTo("more than 4 m from the AR model"),
                "the metres are a setting");
        }

        [Test]
        public void APlacementNotReadIsNoRowAndIsSaidInTheLogWithWhy()
        {
            ModelFacts el = new ModelFacts(Nwc("1B06PS", "EL")).PlacementNot("the model has no transform").WithSite("S");
            List<DisciplineGroup> groups = new List<DisciplineGroup>
            {
                new DisciplineGroup("1B06PS", new[] { Nwc("1B06PS", "AR"), el.File }, new[] { Model(Nwc("1B06PS", "AR"), 0, 0, 0, "S"), el }, null),
                new DisciplineGroup("1B06BS", new[] { Nwc("1B06BS", "EL") }, null, "its Navisworks wrote no result")
            };

            CheckFindings found = CoordinatesCheck.Of(groups, Settings, Names);

            Assert.That(found.Issues, Is.Empty);
            Assert.That(found.FilesCheckedFor("EL"), Is.EqualTo(0));
            Assert.That(found.Notes, Has.Some.EqualTo("Coordinates  1B06PS  1104-PAR-1B06PS-ZZZ-EL-MOD-000001.nwc not read, its placement was not read, the model has no transform"));
            Assert.That(found.Notes, Has.Some.EqualTo("Coordinates  1B06BS not read, its models were not read, its Navisworks wrote no result"));
        }

        [Test]
        public void APlacementOfNoNumberIsNeverThreeNoughts()
        {
            ModelFacts model = new ModelFacts("a.nwc").WithPlacement(double.NaN, 0, 0);

            Assert.That(model.IsPlaced, Is.False);
            Assert.That(model.PlacementNotRead, Is.EqualTo("its transform gave no number"));
        }

        // ---------- missing model and one discipline group ----------

        [Test]
        public void AGroupOfThreeDisciplinesMissingElIsOneRowOfTheElWorkbook()
        {
            List<DisciplineGroup> groups = new List<DisciplineGroup>
            {
                new DisciplineGroup("1B06PS", new[] { Nwc("1B06PS", "AR"), Nwc("1B06PS", "ME"), Nwc("1B06PS", "ST") }, null, "not run")
            };

            CheckFindings found = GroupDisciplinesCheck.Missing(groups, Settings, Names);

            DisciplineIssue el = found.Issues.Single();
            Assert.That(el.Discipline, Is.EqualTo("EL"));
            Assert.That(el.WhatIsWrong, Is.EqualTo("no EL model in this group"));
            Assert.That(el.Value, Is.EqualTo("AR, ME, ST"));
            Assert.That(el.Action, Is.EqualTo("Issue the EL model for this building"));
            Assert.That(el.Building, Is.EqualTo("1B06PS"));
            Assert.That(el.File, Is.Empty);
            Assert.That(el.Severity, Is.EqualTo(CheckWords.High));
            Assert.That(found.FilesCheckedFor("EL"), Is.EqualTo(3));
        }

        [Test]
        public void AGroupOfOneDisciplineMissesNothingAndIsOneRowOfItsOwnWorkbookWithItsFiles()
        {
            List<DisciplineGroup> groups = new List<DisciplineGroup>
            {
                new DisciplineGroup("1B06PG", new[] { Nwc("1B06PG", "EL"), Nwc("1B06PG", "EL", "000002") }, null, "this group did not run in this run"),
                new DisciplineGroup("1B06PS", new[] { Nwc("1B06PS", "AR"), Nwc("1B06PS", "EL") }, null, "not run")
            };

            Assert.That(GroupDisciplinesCheck.Missing(groups, Settings, Names).Issues.Any(i => i.Building == "1B06PG"), Is.False);
            DisciplineIssue one = GroupDisciplinesCheck.OneDiscipline(groups, Names).Issues.Single();
            Assert.That(one.Discipline, Is.EqualTo("EL"));
            Assert.That(one.WhatIsWrong, Is.EqualTo("only discipline in this group, not federated"));
            Assert.That(one.Value, Is.EqualTo("1104-PAR-1B06PG-ZZZ-EL-MOD-000001.nwc, 1104-PAR-1B06PG-ZZZ-EL-MOD-000002.nwc"));
            Assert.That(one.Building, Is.EqualTo("1B06PG"));
            Assert.That(one.Severity, Is.EqualTo(CheckWords.Medium));
        }

        [Test]
        public void AGroupWhoseFilesWereNotReadIsNeverCountedAsMissingADiscipline()
        {
            List<DisciplineGroup> groups = new List<DisciplineGroup> { new DisciplineGroup("x.nwf", new string[0], null, "its Navisworks wrote no result") };

            CheckFindings found = GroupDisciplinesCheck.Missing(groups, Settings, Names);

            Assert.That(found.Issues, Is.Empty);
            Assert.That(found.Notes.Single(), Does.Contain("whether it misses a discipline is UNKNOWN"));
        }

        // ---------- naming ----------

        [Test]
        public void TheArPatternIsLearnedAndAFileBreakingAFixedPartIsNamedWithWhatArHasThere()
        {
            ReferenceNaming naming = ReferenceNaming.Learn(new[] { Nwc("1B06BC", "AR"), Nwc("1B06G1", "AR") }, Settings, Names);

            Assert.That(naming.Describe(), Is.EqualTo("learned from 2 AR names, 7 parts split on -: part 1 1104, part 2 PAR, part 3 shaped like 1B06BC, part 4 ZZZ, part 5 the discipline, part 6 MOD, part 7 shaped like 000001"));
            Assert.That(naming.Breaks(Nwc("1B06BC", "ME")), Is.Empty);

            NamingBreak wrong = naming.Breaks(TestPaths.At("Parent", "1105-PAR-1B06BC-ZZZ-ME-MOD-000001.nwc")).Single();
            Assert.That(wrong.WhatIsWrong, Is.EqualTo("part 1 reads 1105"));
            Assert.That(wrong.ReferenceHas, Is.EqualTo("AR has 1104 there"));
        }

        [Test]
        public void AFileWithAnotherPartCountOrAPartOfAnotherShapeIsNamed()
        {
            ReferenceNaming naming = ReferenceNaming.Learn(new[] { Nwc("1B06BC", "AR"), Nwc("1B06G1", "AR", "000002") }, Settings, Names);

            NamingBreak six = naming.Breaks(TestPaths.At("P", "1104-PAR-1B06BC-ZZZ-ME-000001.nwc"))[0];
            Assert.That(six.WhatIsWrong + ", " + six.ReferenceHas, Is.EqualTo("it has 6 parts split on -, AR names have 7"));

            NamingBreak shape = naming.Breaks(TestPaths.At("P", "1104-PAR-1B06BC-ZZZ-ME-MOD-00001.nwc")).Single();
            Assert.That(shape.WhatIsWrong + ", " + shape.ReferenceHas, Is.EqualTo("part 7 reads 00001, AR has a part shaped like 000001 there"));

            NamingBreak building = naming.Breaks(TestPaths.At("P", "1104-PAR-B06BC-ZZZ-ME-MOD-000001.nwc")).Single();
            Assert.That(building.WhatIsWrong + ", " + building.ReferenceHas, Is.EqualTo("part 3 reads B06BC, AR has a part shaped like 1B06BC there"));
        }

        [Test]
        public void OnlyTheBreakingFilesAreRowsAndTheReferencesOwnFilesAreNotJudged()
        {
            List<DisciplineGroup> groups = new List<DisciplineGroup>
            {
                new DisciplineGroup("1B06BC", new[] { Nwc("1B06BC", "AR"), Nwc("1B06BC", "EL"), TestPaths.At("P", "1104-PAR-1B06BC-ZZZ-ME-MOD-01.nwc") }, null, "not run")
            };

            CheckFindings found = ReferenceNaming.Of(groups, Settings, Names);

            DisciplineIssue me = found.Issues.Single();
            Assert.That(me.Discipline, Is.EqualTo("ME"));
            Assert.That(me.WhatIsWrong, Is.EqualTo("part 7 reads 01"));
            Assert.That(me.Value, Is.EqualTo("AR has a part shaped like 000001 there"));
            Assert.That(me.Severity, Is.EqualTo(CheckWords.Medium));
            Assert.That(found.FilesCheckedFor("EL"), Is.EqualTo(1));
            Assert.That(found.FilesCheckedFor("AR"), Is.EqualTo(0), "the reference's own files are the pattern");
            Assert.That(found.Notes[0], Does.StartWith("Naming  learned from 1 AR names"));
        }

        [Test]
        public void WithNoArFileNoPatternIsLearnedNoNameIsJudgedAndTheLogSaysSo()
        {
            List<DisciplineGroup> groups = new List<DisciplineGroup> { new DisciplineGroup("1B06BS", new[] { Nwc("1B06BS", "EL") }, null, "not run") };

            CheckFindings found = ReferenceNaming.Of(groups, Settings, Names);

            Assert.That(found.Issues, Is.Empty);
            Assert.That(found.Notes.Single(), Is.EqualTo("Naming  no AR file in this package, so no pattern was learned and no name was checked"));
        }

        [Test]
        public void TheMergedShapeOfC06BuildingCodesTakesEitherWhereALetterAndADigitMeet()
        {
            string shape = ReferenceNaming.MergedShape(new[] { "1B06BC", "1B06G1", "1C06PK" });

            Assert.That(shape, Is.EqualTo("9A99A#"));
            Assert.That(ReferenceNaming.Fits("1B06K1", shape), Is.True);
            Assert.That(ReferenceNaming.Fits("1B06B", shape), Is.False, "one short");
            Assert.That(ReferenceNaming.Fits("XB06BC", shape), Is.False, "a letter where every code holds a digit");
            Assert.That(ReferenceNaming.MergedShape(new[] { "1B06BC", "1B06BCX" }), Is.Null, "two lengths share no shape");
            Assert.That(ReferenceNaming.MergedShape(new[] { "A-1", "A_1" }), Is.Null, "two marks share no shape");
        }

        /// <summary>
        /// Read on the smoke copy of 18 Oct 2026: one AR name, 1104-PAR-1B06BC-ZZZ-AR-MOD-000001, made every part of it
        /// fixed, so ST-MOD-000002 read as a break of part 7. The number is judged by its shape alone, a setting.
        /// </summary>
        [Test]
        public void TheNumberIsJudgedByItsShapeAloneSoAnotherNumberOfOneArNameFollowsAndAShortOneBreaks()
        {
            ReferenceNaming naming = ReferenceNaming.Learn(new[] { Nwc("1B06BC", "AR") }, Settings, Names);

            Assert.That(naming.Breaks(Nwc("1B06BC", "ST", "000002")), Is.Empty);
            Assert.That(naming.Breaks(Nwc("1B06BC", "ME", "00002")).Single().ReferenceHas, Is.EqualTo("AR has a part shaped like 000001 there"));

            DisciplineReportSettings strict = new DisciplineReportSettings { ShapeOnlyParts = new int[0] };
            Assert.That(ReferenceNaming.Learn(new[] { Nwc("1B06BC", "AR") }, strict, Names).Breaks(Nwc("1B06BC", "ST", "000002")).Single().ReferenceHas,
                Is.EqualTo("AR has 000001 there"), "with no part judged by shape alone the number is a fixed part");
            Assert.That(() => strict.ShapeOnlyParts = new[] { 0 }, Throws.TypeOf<ArgumentOutOfRangeException>());
        }

        [Test]
        public void TheReferenceIsASettingSoStLearnsThePatternWhenAsked()
        {
            DisciplineReportSettings st = new DisciplineReportSettings { Reference = "ST" };
            ReferenceNaming naming = ReferenceNaming.Learn(new[] { Nwc("1B06BC", "ST") }, st, Names);

            Assert.That(naming.Describe(), Does.StartWith("learned from 1 ST names"));
        }

        // ---------- file age ----------

        [Test]
        public void OnlyAFileOlderThanTheLimitIsARowAndADateNotReadIsSaidInTheLog()
        {
            DateTime now = new DateTime(2026, 10, 18, 1, 0, 0);
            List<DisciplineGroup> groups = new List<DisciplineGroup> { new DisciplineGroup("1B06BC", new[] { Nwc("1B06BC", "AR"), Nwc("1B06BC", "EL"), Nwc("1B06BC", "ME") }, null, "not run") };
            List<FileDate> dates = new List<FileDate>
            {
                new FileDate(Nwc("1B06BC", "AR"), now.AddDays(-8), null),
                new FileDate(Nwc("1B06BC", "EL"), now.AddDays(-7), null),
                new FileDate(Nwc("1B06BC", "ME"), null, "the file is gone")
            };

            CheckFindings found = FileAgeCheck.Of(groups, dates, now, Settings, Names);

            DisciplineIssue ar = found.Issues.Single();
            Assert.That(ar.Discipline, Is.EqualTo("AR"), "exactly the limit is not older than it");
            Assert.That(ar.WhatIsWrong, Is.EqualTo("older than 7 days"));
            Assert.That(ar.Value, Is.EqualTo("2026-10-10, 8 days"));
            Assert.That(found.FilesCheckedFor("EL"), Is.EqualTo(1));
            Assert.That(found.FilesCheckedFor("ME"), Is.EqualTo(0));
            Assert.That(found.Notes.Single(), Is.EqualTo("File age  1104-PAR-1B06BC-ZZZ-ME-MOD-000001.nwc not read, its date was not read, the file is gone"));
        }

        // ---------- CAD and Generic Models ----------

        [Test]
        public void OneRowPerFileAndCadFileWithTheCountAndEveryIdAndOnePerFamilyAndType()
        {
            ModelFacts me = new ModelFacts(Nwc("1B06PH", "ME", "000002")).WithPlacement(0, 0, 0).WithSite("S")
                .WithCad(new[]
                {
                    new ElementFact("1819523", "Chem-Feed Pkg-R0.dwg", "MEC-MEC-Doising Pump-PAR", "Standard", "GRF"),
                    new ElementFact("1819524", "Chem-Feed Pkg-R0.dwg", "MEC-MEC-Doising Pump-PAR", "Standard", "GRF"),
                    new ElementFact(null, null, "F", "T", null)
                })
                .WithGeneric(new[]
                {
                    new ElementFact("2342282", "PAR-ST-600 X 600", "REC_OPENING COVER", "PAR-ST-600 X 600", "TANK ROOF"),
                    new ElementFact("2342283", "PAR-ST-600 X 600", "REC_OPENING COVER", "PAR-ST-600 X 600", "TANK ROOF")
                });
            List<DisciplineGroup> groups = new List<DisciplineGroup> { new DisciplineGroup("1B06PH", new[] { me.File }, new[] { me }, null) };

            List<DisciplineIssue> cad = ElementChecks.Cad(groups, Settings, Names).Issues.ToList();
            DisciplineIssue generic = ElementChecks.Generic(groups, Settings, Names).Issues.Single();

            Assert.That(cad.Count, Is.EqualTo(2));
            Assert.That(cad[0].WhatIsWrong, Is.EqualTo("CAD file Chem-Feed Pkg-R0.dwg"));
            Assert.That(cad[0].Value, Is.EqualTo("2"));
            Assert.That(cad[0].ElementIds, Is.EqualTo("1819523, 1819524"));
            Assert.That(cad[1].WhatIsWrong, Is.EqualTo("CAD file not readable"));
            Assert.That(cad[1].ElementIds, Is.EqualTo("not readable"));
            Assert.That(cad[0].Severity, Is.EqualTo(CheckWords.Low));
            Assert.That(generic.WhatIsWrong, Is.EqualTo("Generic Models family REC_OPENING COVER, type PAR-ST-600 X 600"));
            Assert.That(generic.Value, Is.EqualTo("2"));
            Assert.That(generic.ElementIds, Is.EqualTo("2342282, 2342283"));
        }

        [Test]
        public void AModelWithNoneIsNoRowAndOneNotWalkedIsSaidInTheLog()
        {
            ModelFacts none = new ModelFacts(Nwc("1B06BC", "AR")).WithCad(new List<ElementFact>()).GenericNot("the search threw");
            List<DisciplineGroup> groups = new List<DisciplineGroup> { new DisciplineGroup("1B06BC", new[] { none.File }, new[] { none }, null) };

            CheckFindings cad = ElementChecks.Cad(groups, Settings, Names);
            CheckFindings generic = ElementChecks.Generic(groups, Settings, Names);

            Assert.That(cad.Issues, Is.Empty);
            Assert.That(cad.FilesCheckedFor("AR"), Is.EqualTo(1));
            Assert.That(generic.Issues, Is.Empty);
            Assert.That(generic.FilesCheckedFor("AR"), Is.EqualTo(0));
            Assert.That(generic.Notes.Single(), Is.EqualTo("Generic Models  1104-PAR-1B06BC-ZZZ-AR-MOD-000001.nwc not read, its items were not walked, the search threw"));
        }

        [Test]
        public void TheLevelIsTheNameBetweenTheQuotesNavisworksGives()
        {
            Assert.That(ElementChecks.LevelName("Level \"GRF\", #151434"), Is.EqualTo("GRF"));
            Assert.That(ElementChecks.LevelName("Level \"TANK ROOF\", #2310673"), Is.EqualTo("TANK ROOF"));
            Assert.That(ElementChecks.LevelName("GROUND LEVEL"), Is.EqualTo("GROUND LEVEL"));
            Assert.That(ElementChecks.LevelName(" "), Is.Null);
        }

        [Test]
        public void TheCadFileNameIsTheImportsNameWhereItEndsWithAnExtension()
        {
            Assert.That(ElementChecks.CadFileName("Chem-Feed Pkg-R0.dwg"), Is.EqualTo("Chem-Feed Pkg-R0.dwg"));
            Assert.That(ElementChecks.CadFileName("FAB200-25RF.iam"), Is.EqualTo("FAB200-25RF.iam"));
            Assert.That(ElementChecks.CadFileName("8_127_127_127"), Is.Null, "an import named by its layer carries no file name");
            Assert.That(ElementChecks.CadFileName("Chem-Feed Pkg-R0.dwg.Chem-Feed Pkg-R0"), Is.Null);
        }

        // ---------- the severities ----------

        [Test]
        public void TheSeveritiesAreBadersAndACheckNotOfTheSevenIsRefused()
        {
            Assert.That(CheckWords.All.Select(CheckWords.SeverityOf).ToArray(),
                Is.EqualTo(new[] { "High", "High", "Medium", "Medium", "Medium", "Low", "Low" }));
            Assert.That(() => CheckWords.SeverityOf("Box gap"), Throws.ArgumentException);
            Assert.That(CheckWords.SeverityRank("High"), Is.LessThan(CheckWords.SeverityRank("Medium")));
            Assert.That(CheckWords.SeverityRank("Medium"), Is.LessThan(CheckWords.SeverityRank("Low")));
        }
    }
}
