using System;
using System.Collections.Generic;
using System.Linq;
using Federator.Core.Disciplines;
using Federator.Core.Naming;
using NUnit.Framework;

namespace Federator.Core.Tests
{
    /// <summary>
    /// F209, the discipline reports. Each check is proved on data a test builds, the coordinates on the boxes and sites
    /// the probe read off C02 on 18 Oct 2026, scan.md 5z-zg, and each check is broken one way and asserted to name it.
    /// </summary>
    [TestFixture]
    public class DisciplineChecksTests
    {
        private const double Foot = 0.3048;
        private static readonly ContainerNameSettings Names = new ContainerNameSettings();
        private static readonly DisciplineReportSettings Settings = new DisciplineReportSettings();

        private static string Nwc(string building, string discipline, string number = "000001")
        {
            return TestPaths.At("Parent", "C02", "1104-PAR-" + building + "-ZZZ-" + discipline + "-MOD-" + number + ".nwc");
        }

        /// <summary>A model with a box given in feet, as the probe read it, put in metres, and its site.</summary>
        private static ModelFacts Model(string file, double x1, double y1, double z1, double x2, double y2, double z2, string site)
        {
            return new ModelFacts(file).WithBox(x1 * Foot, y1 * Foot, z1 * Foot, x2 * Foot, y2 * Foot, z2 * Foot).WithSite(site)
                .WithCad(new List<ElementFact>()).WithGeneric(new List<ElementFact>());
        }

        /// <summary>1A02MM as the probe read it: AR and ME near Revit's origin, EL and ST about 2,823 km away.</summary>
        private static DisciplineGroup C02()
        {
            return new DisciplineGroup("1A02MM", new[] { Nwc("1A02MM", "AR"), Nwc("1A02MM", "EL"), Nwc("1A02MM", "ME"), Nwc("1A02MM", "ST") },
                new[]
                {
                    Model(Nwc("1A02MM", "AR"), 9.757, -29.403, 0, 39.891, 0.699, 32.644, "SWLS-02-SharedCoordinate"),
                    Model(Nwc("1A02MM", "EL"), 2158005.809, 9009231.096, 2227.227, 2158052.473, 9009286.938, 2265.433, "LTB2"),
                    Model(Nwc("1A02MM", "ME"), 6.31, -34.838, -3.281, 35.835, 1.506, 31.824, "PW3_Shared_Location"),
                    Model(Nwc("1A02MM", "ST"), 2158007.099, 9009239.412, 2228.129, 2158053.584, 9009286.436, 2265.039, "Internal")
                },
                null);
        }

        // ---------- check 1, coordinates ----------

        [Test]
        public void OnC02ElAndStAreFarFromArBy2823KmAndMeIsNear()
        {
            IList<CheckRow> rows = CoordinatesCheck.Rows("C02", "C02", C02(), Settings, Names);

            CheckRow el = rows.Single(r => r.Discipline == "EL");
            CheckRow st = rows.Single(r => r.Discipline == "ST");
            CheckRow me = rows.Single(r => r.Discipline == "ME");
            CheckRow ar = rows.Single(r => r.Discipline == "AR");

            Assert.That(el.Result, Is.EqualTo(CheckWords.Far));
            Assert.That(el.Value, Does.StartWith("2823."));
            Assert.That(el.Value, Does.Contain(" km, "));
            Assert.That(el.Value, Does.Contain("from AR, its site LTB2, AR site SWLS-02-SharedCoordinate"));
            Assert.That(st.Result, Is.EqualTo(CheckWords.Far));
            Assert.That(st.Value, Does.StartWith("2823."));
            Assert.That(st.Value, Does.Contain("its site Internal"));
            Assert.That(me.Result, Is.EqualTo(CheckWords.Near));
            Assert.That(me.Value, Does.StartWith("0.000 m from AR, its site PW3_Shared_Location"));
            Assert.That(ar.Result, Is.EqualTo(CheckWords.Reference));
            Assert.That(ar.Value, Does.Contain("far from it: EL 2823."));
            Assert.That(ar.Value, Does.Contain(", ST 2823."));
            Assert.That(el.File, Is.EqualTo("1104-PAR-1A02MM-ZZZ-EL-MOD-000001.nwc"), "the file cell is the file's name, never its folder");
        }

        [Test]
        public void TheGapBetweenTwoBoxesIsNoughtWhereTheyOverlapAndTheStraightLineOfTheAxisGapsWhereNot()
        {
            ModelFacts a = new ModelFacts("a").WithBox(0, 0, 0, 10, 10, 10);
            ModelFacts overlaps = new ModelFacts("b").WithBox(5, 5, 5, 20, 20, 20);
            ModelFacts apart = new ModelFacts("c").WithBox(13, 14, 10, 20, 20, 20);

            Assert.That(CoordinatesCheck.Gap(a, overlaps), Is.EqualTo(0.0));
            Assert.That(CoordinatesCheck.Gap(a, apart), Is.EqualTo(5.0).Within(1e-9), "3 on x and 4 on y make 5");
            Assert.That(CoordinatesCheck.Gap(apart, a), Is.EqualTo(5.0).Within(1e-9));
        }

        [Test]
        public void AGroupWithNoArIsMeasuredFromItsFirstDisciplineAndSaysSo()
        {
            DisciplineGroup group = new DisciplineGroup("1B06BS", new[] { Nwc("1B06BS", "EL"), Nwc("1B06BS", "ME") },
                new[] { Model(Nwc("1B06BS", "EL"), 0, 0, 0, 10, 10, 10, "S"), Model(Nwc("1B06BS", "ME"), 0, 0, 0, 10, 10, 10, "S") }, null);

            IList<CheckRow> rows = CoordinatesCheck.Rows("C06", "C06", group, Settings, Names);

            Assert.That(rows.Single(r => r.Discipline == "EL").Result, Is.EqualTo(CheckWords.Reference));
            Assert.That(rows.Single(r => r.Discipline == "EL").Value, Does.Contain("since it holds no AR"));
            Assert.That(rows.Single(r => r.Discipline == "ME").Value, Does.Contain("from EL, the first discipline of the group with a box, since it holds no AR"));
        }

        [Test]
        public void AModelWhoseBoxOrSiteWasNotReadSaysSoAndIsNeverNearOrFar()
        {
            DisciplineGroup group = new DisciplineGroup("1A02MM", new[] { Nwc("1A02MM", "AR"), Nwc("1A02MM", "EL") },
                new[]
                {
                    Model(Nwc("1A02MM", "AR"), 0, 0, 0, 10, 10, 10, "S"),
                    new ModelFacts(Nwc("1A02MM", "EL")).BoxNot("its root item has no box").SiteNot("the Location tab threw")
                },
                null);

            CheckRow el = CoordinatesCheck.Rows("C02", "C02", group, Settings, Names).Single(r => r.Discipline == "EL");

            Assert.That(el.Result, Is.EqualTo(CheckWords.NotRead));
            Assert.That(el.Value, Is.EqualTo("its box was not read, its root item has no box"));
        }

        [Test]
        public void AGroupWhoseModelsWereNotReadSaysNotReadForEachFile()
        {
            DisciplineGroup group = new DisciplineGroup("1A02MM", new[] { Nwc("1A02MM", "AR"), Nwc("1A02MM", "EL") }, null, "its NWF would not open");

            IList<CheckRow> rows = CoordinatesCheck.Rows("C02", "C02", group, Settings, Names);

            Assert.That(rows.Count, Is.EqualTo(2));
            Assert.That(rows.All(r => r.Result == CheckWords.NotRead && r.Value.EndsWith("its NWF would not open")), Is.True);
        }

        /// <summary>The EL model of a group whose box starts this many metres east of the AR box, every other side lined up.</summary>
        private static string ResultAtAGapOf(double metres)
        {
            DisciplineGroup group = new DisciplineGroup("G", new[] { Nwc("1B06BC", "AR"), Nwc("1B06BC", "EL") },
                new[]
                {
                    new ModelFacts(Nwc("1B06BC", "AR")).WithBox(0, 0, 0, 10, 10, 10).WithSite("S"),
                    new ModelFacts(Nwc("1B06BC", "EL")).WithBox(10 + metres, 0, 0, 20 + metres, 10, 10).WithSite("S")
                },
                null);

            return CoordinatesCheck.Rows("C", "C", group, new DisciplineReportSettings(), Names).Single(r => r.Discipline == "EL").Result;
        }

        // F218, Bader's answer to Q202 of 18 Oct 2026: far is more than 30 m from the reference discipline, 30 m the default.
        [Test]
        public void FarIsMoreThanThirtyMetresByDefaultSoThirtyItselfIsNear()
        {
            Assert.That(DisciplineReportSettings.DefaultFarMetres, Is.EqualTo(30.0));
            Assert.That(ResultAtAGapOf(29.9), Is.EqualTo(CheckWords.Near));
            Assert.That(ResultAtAGapOf(30.0), Is.EqualTo(CheckWords.Near), "far is more than 30 m, so exactly 30 m is near");
            Assert.That(ResultAtAGapOf(30.1), Is.EqualTo(CheckWords.Far));
        }

        [Test]
        public void TheFarSettingDecidesFarOrNear()
        {
            DisciplineReportSettings close = new DisciplineReportSettings { FarMetres = 4.0 };
            DisciplineGroup group = new DisciplineGroup("G", new[] { Nwc("1B06BC", "AR"), Nwc("1B06BC", "EL") },
                new[] { new ModelFacts(Nwc("1B06BC", "AR")).WithBox(0, 0, 0, 10, 10, 10).WithSite("S"), new ModelFacts(Nwc("1B06BC", "EL")).WithBox(13, 14, 10, 20, 20, 20).WithSite("S") }, null);

            Assert.That(CoordinatesCheck.Rows("C", "C", group, close, Names).Single(r => r.Discipline == "EL").Result, Is.EqualTo(CheckWords.Far));
            Assert.That(CoordinatesCheck.Rows("C", "C", group, Settings, Names).Single(r => r.Discipline == "EL").Result, Is.EqualTo(CheckWords.Near));
            Assert.That(() => close.FarMetres = -1.0, Throws.TypeOf<ArgumentOutOfRangeException>());
        }

        // ---------- check 2, a missing discipline ----------

        [Test]
        public void ArPresentInOtherGroupsAndNotInThisOneIsMissingThere()
        {
            List<DisciplineGroup> groups = new List<DisciplineGroup>
            {
                new DisciplineGroup("1B06BC", new[] { Nwc("1B06BC", "AR"), Nwc("1B06BC", "EL"), Nwc("1B06BC", "ME"), Nwc("1B06BC", "ST") }, null, "not run"),
                new DisciplineGroup("1B06BS", new[] { Nwc("1B06BS", "EL"), Nwc("1B06BS", "ME"), Nwc("1B06BS", "ST") }, null, "not run")
            };

            IList<CheckRow> rows = MissingDisciplineCheck.Rows("C06", "C06", groups, Settings, Names);

            CheckRow ar = rows.Single(r => r.Discipline == "AR");
            Assert.That(ar.Result, Is.EqualTo(CheckWords.MissingHere));
            Assert.That(ar.Group, Is.EqualTo("1B06BS"));
            Assert.That(ar.Value, Is.EqualTo("AR is in 1 of 2 groups of this package and not in 1B06BS, which holds EL, ME, ST"));
            Assert.That(rows.Single(r => r.Discipline == "EL").Result, Is.EqualTo(CheckWords.NoneMissing));
        }

        [Test]
        public void AGroupWhoseFilesWereNotReadIsNeverSaidToMissADiscipline()
        {
            List<DisciplineGroup> groups = new List<DisciplineGroup>
            {
                new DisciplineGroup("1B06BC", new[] { Nwc("1B06BC", "AR"), Nwc("1B06BC", "EL") }, null, "not run"),
                new DisciplineGroup("1B06BS", new[] { Nwc("1B06BS", "AR") }, null, "not run"),
                new DisciplineGroup("1B06KI", new string[0], null, "its Navisworks wrote no result, the file is not there")
            };

            IList<CheckRow> rows = MissingDisciplineCheck.Rows("C06", "C06", groups, Settings, Names);

            Assert.That(rows.Where(r => r.Group == "1B06KI" && r.Result == CheckWords.MissingHere).Count(), Is.EqualTo(0),
                "F222, a group with no file read is UNKNOWN and never missing");
            CheckRow ki = rows.Single(r => r.Group == "1B06KI" && r.Discipline == "AR");
            Assert.That(ki.Result, Is.EqualTo(CheckWords.NotRead));
            Assert.That(ki.Value, Is.EqualTo("whether AR is in 1B06KI is UNKNOWN, its files were not read, its Navisworks wrote no result, the file is not there"));
            Assert.That(rows.Single(r => r.Discipline == "AR" && r.Group == string.Empty).Result, Is.EqualTo(CheckWords.NoneMissing),
                "AR is in every group whose files were read");
            Assert.That(rows.Single(r => r.Discipline == "EL" && r.Group == "1B06BS").Value,
                Is.EqualTo("EL is in 1 of 2 groups of this package whose files were read and not in 1B06BS, which holds AR"));

            IList<CheckRow> none = MissingDisciplineCheck.Rows("C06", "C06", groups.GetRange(2, 1), Settings, Names);
            Assert.That(none.All(r => r.Result == CheckWords.NotRead), Is.True, "with no group read, nothing is said in or not in the package");
        }

        [Test]
        public void ADisciplineInNoGroupOfThePackageSaysSoOnce()
        {
            List<DisciplineGroup> groups = new List<DisciplineGroup> { new DisciplineGroup("1B06BC", new[] { Nwc("1B06BC", "ME") }, null, "not run") };

            CheckRow ar = MissingDisciplineCheck.Rows("C06", "C06", groups, Settings, Names).Single(r => r.Discipline == "AR");

            Assert.That(ar.Result, Is.EqualTo(CheckWords.NotInThePackage));
        }

        // ---------- check 3, naming against AR ----------

        [Test]
        public void TheArPatternIsLearnedAndAFileBreakingAFixedPartIsNamedWithWhatArHasThere()
        {
            ReferenceNaming naming = ReferenceNaming.Learn(new[] { Nwc("1B06BC", "AR"), Nwc("1B06G1", "AR") }, Settings, Names);

            Assert.That(naming.Describe(), Is.EqualTo("learned from 2 AR names, 7 parts split on -: part 1 1104, part 2 PAR, part 3 shaped like 1B06BC, part 4 ZZZ, part 5 the discipline, part 6 MOD, part 7 shaped like 000001"));
            Assert.That(naming.Breaks(Nwc("1B06BC", "ME")), Is.Empty);

            string wrong = TestPaths.At("Parent", "1105-PAR-1B06BC-ZZZ-ME-MOD-000001.nwc");
            Assert.That(naming.Breaks(wrong).Single(), Is.EqualTo("part 1 reads 1105, AR has 1104 there"));
        }

        [Test]
        public void AFileWithAnotherPartCountOrAPartOfAnotherShapeIsNamed()
        {
            ReferenceNaming naming = ReferenceNaming.Learn(new[] { Nwc("1B06BC", "AR"), Nwc("1B06G1", "AR", "000002") }, Settings, Names);

            IList<string> six = naming.Breaks(TestPaths.At("P", "1104-PAR-1B06BC-ZZZ-ME-000001.nwc"));
            Assert.That(six[0], Is.EqualTo("it has 6 parts split on -, AR names have 7"));

            IList<string> shape = naming.Breaks(TestPaths.At("P", "1104-PAR-1B06BC-ZZZ-ME-MOD-00001.nwc"));
            Assert.That(shape.Single(), Is.EqualTo("part 7 reads 00001, AR has a part shaped like 000001 there"));

            IList<string> building = naming.Breaks(TestPaths.At("P", "1104-PAR-B06BC-ZZZ-ME-MOD-000001.nwc"));
            Assert.That(building.Single(), Is.EqualTo("part 3 reads B06BC, AR has a part shaped like 1B06BC there"));
        }

        [Test]
        public void TheRowsSayFollowsOrBreaksForEveryFileOfTheOtherDisciplinesAndThePatternInEachWorkbook()
        {
            List<DisciplineGroup> groups = new List<DisciplineGroup>
            {
                new DisciplineGroup("1B06BC", new[] { Nwc("1B06BC", "AR"), Nwc("1B06BC", "EL"), TestPaths.At("P", "1104-PAR-1B06BC-ZZZ-ME-MOD-01.nwc") }, null, "not run")
            };

            IList<CheckRow> rows = ReferenceNaming.Rows("C06", "C06", groups, Settings, Names);

            Assert.That(rows.Count(r => r.Result == CheckWords.Pattern), Is.EqualTo(4), "one line of the pattern in each of the four workbooks");
            Assert.That(rows.Single(r => r.Discipline == "EL" && r.File.Length > 0).Result, Is.EqualTo(CheckWords.Follows));
            Assert.That(rows.Single(r => r.Discipline == "ME" && r.File.Length > 0).Result, Is.EqualTo(CheckWords.Breaks));
            Assert.That(rows.Any(r => r.Discipline == "AR" && r.File.Length > 0), Is.False, "the reference's own files are the pattern and are not judged");
        }

        [Test]
        public void WithNoArFileNoPatternIsLearnedAndNoNameIsJudged()
        {
            List<DisciplineGroup> groups = new List<DisciplineGroup> { new DisciplineGroup("1B06BS", new[] { Nwc("1B06BS", "EL") }, null, "not run") };

            IList<CheckRow> rows = ReferenceNaming.Rows("C06", "C06", groups, Settings, Names);

            Assert.That(rows.All(r => r.Result == CheckWords.NoPattern), Is.True);
            Assert.That(rows[0].Value, Is.EqualTo("no AR file in this package, so no pattern was learned and no name was checked"));
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
            Assert.That(naming.Breaks(Nwc("1B06BC", "ME", "00002")).Single(), Is.EqualTo("part 7 reads 00002, AR has a part shaped like 000001 there"));

            DisciplineReportSettings strict = new DisciplineReportSettings { ShapeOnlyParts = new int[0] };
            Assert.That(ReferenceNaming.Learn(new[] { Nwc("1B06BC", "AR") }, strict, Names).Breaks(Nwc("1B06BC", "ST", "000002")).Single(),
                Is.EqualTo("part 7 reads 000002, AR has 000001 there"), "with no part judged by shape alone the number is a fixed part");
            Assert.That(() => strict.ShapeOnlyParts = new[] { 0 }, Throws.TypeOf<ArgumentOutOfRangeException>());
        }

        [Test]
        public void TheReferenceIsASettingSoStLearnsThePatternWhenAsked()
        {
            DisciplineReportSettings st = new DisciplineReportSettings { Reference = "ST" };
            ReferenceNaming naming = ReferenceNaming.Learn(new[] { Nwc("1B06BC", "ST") }, st, Names);

            Assert.That(naming.Describe(), Does.StartWith("learned from 1 ST names"));
        }

        // ---------- check 4, file age ----------

        [Test]
        public void AFileOlderThanTheLimitIsFlaggedAndOneWithinItIsNot()
        {
            DateTime now = new DateTime(2026, 10, 18, 1, 0, 0);
            List<DisciplineGroup> groups = new List<DisciplineGroup> { new DisciplineGroup("1B06BC", new[] { Nwc("1B06BC", "AR"), Nwc("1B06BC", "EL"), Nwc("1B06BC", "ME") }, null, "not run") };
            List<FileDate> dates = new List<FileDate>
            {
                new FileDate(Nwc("1B06BC", "AR"), now.AddDays(-8), null),
                new FileDate(Nwc("1B06BC", "EL"), now.AddDays(-7), null),
                new FileDate(Nwc("1B06BC", "ME"), null, "the file is gone")
            };

            IList<CheckRow> rows = FileAgeCheck.Rows("C06", "C06", groups, dates, now, Settings, Names);

            Assert.That(rows.Single(r => r.Discipline == "AR").Result, Is.EqualTo(CheckWords.Older));
            Assert.That(rows.Single(r => r.Discipline == "AR").Value, Is.EqualTo("written 2026-10-10 01:00, 8 days old, the limit 7 days"));
            Assert.That(rows.Single(r => r.Discipline == "EL").Result, Is.EqualTo(CheckWords.Within), "exactly the limit is not older than it");
            Assert.That(rows.Single(r => r.Discipline == "ME").Result, Is.EqualTo(CheckWords.NotRead));
            Assert.That(rows.Single(r => r.Discipline == "ME").Value, Is.EqualTo("its date was not read, the file is gone"));
        }

        // ---------- checks 5 and 6, CAD and Generic Models ----------

        [Test]
        public void EveryElementIsListedWithItsIdAndAnIdNotReadSaysNotReadable()
        {
            ModelFacts me = new ModelFacts(Nwc("1B06PH", "ME", "000002")).WithBox(0, 0, 0, 1, 1, 1).WithSite("S")
                .WithCad(new[] { new ElementFact("1819523", "Chem-Feed Pkg-R0.dwg", "MEC-MEC-Doising Pump-PAR", "Standard", "GRF"), new ElementFact(null, null, "F", "T", null) })
                .WithGeneric(new[] { new ElementFact("2342282", "PAR-ST-600 X 600", "REC_OPENING COVER", "PAR-ST-600 X 600", "TANK ROOF") });
            DisciplineGroup group = new DisciplineGroup("1B06PH", new[] { me.File }, new[] { me }, null);

            IList<CheckRow> rows = ElementChecks.Rows("C06", "C06", group, Settings, Names);

            CheckRow first = rows.First(r => r.Check == CheckWords.Cad);
            Assert.That(first.ElementId, Is.EqualTo("1819523"));
            Assert.That(first.Value, Is.EqualTo("CAD file Chem-Feed Pkg-R0.dwg, in MEC-MEC-Doising Pump-PAR of type Standard, level GRF"));
            CheckRow second = rows.Where(r => r.Check == CheckWords.Cad).ElementAt(1);
            Assert.That(second.ElementId, Is.EqualTo("not readable"));
            Assert.That(second.Value, Is.EqualTo("CAD file not readable, in F of type T, level not readable"));
            CheckRow generic = rows.Single(r => r.Check == CheckWords.Generic);
            Assert.That(generic.Value, Is.EqualTo("family REC_OPENING COVER, type PAR-ST-600 X 600, level TANK ROOF"));
            Assert.That(generic.ElementId, Is.EqualTo("2342282"));
        }

        [Test]
        public void AModelWithNoneSaysNoneFoundAndOneNotWalkedSaysNotRead()
        {
            ModelFacts none = new ModelFacts(Nwc("1B06BC", "AR")).WithCad(new List<ElementFact>()).GenericNot("the search threw");
            DisciplineGroup group = new DisciplineGroup("1B06BC", new[] { none.File }, new[] { none }, null);

            IList<CheckRow> rows = ElementChecks.Rows("C06", "C06", group, Settings, Names);

            Assert.That(rows.Single(r => r.Check == CheckWords.Cad).Result, Is.EqualTo(CheckWords.NoneFound));
            Assert.That(rows.Single(r => r.Check == CheckWords.Generic).Result, Is.EqualTo(CheckWords.NotRead));
            Assert.That(rows.Single(r => r.Check == CheckWords.Generic).Value, Is.EqualTo("its items were not walked, the search threw"));
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
    }
}
