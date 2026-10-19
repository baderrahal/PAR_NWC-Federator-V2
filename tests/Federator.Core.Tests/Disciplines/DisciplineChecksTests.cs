using System;
using System.Collections.Generic;
using System.Linq;
using Federator.Core.Disciplines;
using Federator.Core.Naming;
using NUnit.Framework;

namespace Federator.Core.Tests
{
    /// <summary>
    /// F209, F234 and F244, the checks of the discipline reports. Coordinates are proved on the translations the ALIGNMENT block
    /// of 1B06BC read on 19 Oct 2026, in millimetres, and each check is broken one way and asserted to name it, or to name
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

        private static CheckFindings Coordinates(List<DisciplineGroup> groups, DisciplineReportSettings settings = null)
        {
            return CoordinatesCheck.Of(groups, ModelRows.Of(groups, Names), settings ?? Settings, Names);
        }

        /// <summary>
        /// 1B06BC as its ALIGNMENT block read it, steps\logs\2026-10-18-C06-C07-F229-NWC-page-C06-group-run-20261018-202521.log
        /// lines 244 to 249, AR the reference, on Internal as read or on a shared site where a test asks it to be.
        /// </summary>
        private static List<DisciplineGroup> Bc(string arSite = "Internal")
        {
            ModelFacts[] models =
            {
                Model(Nwc("1B06BC", "AR"), 0, 0, 0, arSite),
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
            CheckFindings found = Coordinates(Bc("PWPS"));

            List<DisciplineIssue> st = found.Issues.Where(i => i.Discipline == "ST").ToList();
            Assert.That(st.Count, Is.EqualTo(2));
            Assert.That(Math.Round(st[0].Value.Value, 3), Is.EqualTo(34.014));
            Assert.That(Math.Round(st[1].Value.Value, 3), Is.EqualTo(34.472));
            Assert.That(st[0].Unit, Is.EqualTo("m"));
            Assert.That(st[0].Finding, Is.EqualTo("34.0 m from the AR model. ST on site T1, AR on PWPS"));
            Assert.That(st[1].Finding, Is.EqualTo("34.5 m from the AR model. ST on site BCTANK, AR on PWPS"));
            Assert.That(st[0].Fix, Is.EqualTo("Export the NWC again on the project shared coordinates"));
            Assert.That(st[0].Building, Is.EqualTo("1B06BC"));
            Assert.That(st[0].Row.File, Is.EqualTo("1104-PAR-1B06BC-ZZZ-ST-MOD-000001.nwc"), "the file's name, never its folder");
            Assert.That(st[0].Row.Model, Is.EqualTo("ST-000001"));
            Assert.That(st[0].Priority, Is.EqualTo(CheckWords.High));
            Assert.That(found.Issues.Single(i => i.Discipline == "EL").Finding, Does.StartWith("2,774,335.0 m from the AR model. EL on site SITEWIDE PHASE 3"));
            Assert.That(found.Issues.Any(i => i.Discipline == "ME"), Is.False, "ME sits where AR sits and needs nothing");
            Assert.That(found.CheckedFor("ST"), Is.EqualTo(2));
            Assert.That(found.CheckedFor("AR"), Is.EqualTo(1), "the reference is read too");
        }

        [Test]
        public void EveryModelMeasuredCarriesItsDistanceAndTheReferenceIsSaidAsSuch()
        {
            List<DisciplineGroup> groups = Bc("PWPS");
            ModelRows rows = ModelRows.Of(groups, Names);

            CoordinatesCheck.Of(groups, rows, Settings, Names);

            ModelRow me = rows.All.Single(r => r.File == "1104-PAR-1B06BC-ZZZ-ME-MOD-000001.nwc");
            Assert.That(me.Measure, Is.EqualTo(ModelMeasure.Measured));
            Assert.That(me.Metres, Is.EqualTo(0.0), "measured, not only the far ones");
            Assert.That(me.ComparedWith, Is.EqualTo("AR"));
            ModelRow ar = rows.All.Single(r => r.Discipline == "AR");
            Assert.That(ar.Measure, Is.EqualTo(ModelMeasure.Reference));
            Assert.That(ar.ComparedWith, Is.EqualTo("AR"));
        }

        [Test]
        public void AReferenceOnInternalWhileAnotherModelNamesASharedSiteIsItsOwnHighRowAndEveryOtherAMediumOne()
        {
            CheckFindings found = Coordinates(Bc());

            DisciplineIssue ar = found.Issues.Single(i => i.Discipline == "AR");
            Assert.That(ar.Finding, Is.EqualTo("On Revit's internal origin, not on a shared site"));
            Assert.That(ar.Fix, Is.EqualTo("Export on the project shared coordinates. The other models of this building are measured from this one"));
            Assert.That(ar.Priority, Is.EqualTo(CheckWords.High));
            Assert.That(ar.Value, Is.Null);
            Assert.That(ar.Row.File, Is.EqualTo("1104-PAR-1B06BC-ZZZ-AR-MOD-000001.nwc"));
            Assert.That(ar.Row.Measure, Is.EqualTo(ModelMeasure.InternalOrigin));

            List<DisciplineIssue> others = found.Issues.Where(i => i.Discipline != "AR").ToList();
            Assert.That(others.Count, Is.EqualTo(4), "EL, ME and both ST, the ME under a metre too");
            Assert.That(others.All(i => i.Priority == CheckWords.Medium), Is.True, "never a High row measured from internal origin");
            Assert.That(others.All(i => i.Finding == "AR of this building is on Revit's internal origin, so this distance cannot be judged"), Is.True);
            Assert.That(others.All(i => i.Fix == "Fix AR first, then run again"), Is.True);
            Assert.That(Math.Round(others.First(i => i.Discipline == "ST").Value.Value, 3), Is.EqualTo(34.014));
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

            Assert.That(Coordinates(shared).Issues, Is.Empty);
            Assert.That(Coordinates(allInternal).Issues, Is.Empty);
        }

        [Test]
        public void FarIsMoreThanTheSettingSoExactly30MetresIsNoRow()
        {
            Func<double, DisciplineReportSettings, int> rows = (metres, settings) => Coordinates(new List<DisciplineGroup>
            {
                new DisciplineGroup("1B06PS", new[] { Nwc("1B06PS", "AR"), Nwc("1B06PS", "EL") },
                    new[] { Model(Nwc("1B06PS", "AR"), 0, 0, 0, "S"), Model(Nwc("1B06PS", "EL"), metres * 1000.0, 0, 0, "S") }, null)
            }, settings).Issues.Count;

            Assert.That(rows(29.999, Settings), Is.EqualTo(0));
            Assert.That(rows(30.0, Settings), Is.EqualTo(0));
            Assert.That(rows(30.001, Settings), Is.EqualTo(1));
            Assert.That(rows(5.0, new DisciplineReportSettings { FarMetres = 4.0 }), Is.EqualTo(1), "the metres are a setting");
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
            ModelRows rows = ModelRows.Of(groups, Names);

            CheckFindings found = CoordinatesCheck.Of(groups, rows, Settings, Names);

            Assert.That(found.Issues, Is.Empty);
            Assert.That(found.CheckedFor("EL"), Is.EqualTo(0));
            Assert.That(rows.All.Where(r => r.Discipline == "EL").All(r => r.Measure == ModelMeasure.Unknown), Is.True, "UNKNOWN, never a nought");
            Assert.That(found.Notes, Has.Some.EqualTo("Coordinates  1B06PS  1104-PAR-1B06PS-ZZZ-EL-MOD-000001.nwc not read, its placement was not read, the model has no transform"));
            Assert.That(found.Notes, Has.Some.EqualTo("Coordinates  1B06BS not read, its models were not read, its Navisworks wrote no result"));
        }

        [Test]
        public void WithTheArModelNotPlacedTheRowNamesTheModelItWasMeasuredFromAndNeverABlank()
        {
            ModelFacts ar = new ModelFacts(Nwc("1B06PS", "AR")).PlacementNot("the model has no transform").WithSite("S");
            List<DisciplineGroup> st = new List<DisciplineGroup>
            {
                new DisciplineGroup("1B06PS", new[] { ar.File, Nwc("1B06PS", "ST"), Nwc("1B06PS", "EL") },
                    new[] { ar, Model(Nwc("1B06PS", "ST"), 0, 0, 0, "S"), Model(Nwc("1B06PS", "EL"), 40000, 0, 0, "S") }, null)
            };
            string odd = TestPaths.At("Parent", "C06", "plain.nwc");
            List<DisciplineGroup> unread = new List<DisciplineGroup>
            {
                new DisciplineGroup("1B06PS", new[] { odd, Nwc("1B06PS", "EL") },
                    new[] { Model(odd, 0, 0, 0, "S"), Model(Nwc("1B06PS", "EL"), 40000, 0, 0, "S") }, null)
            };

            DisciplineIssue fromSt = Coordinates(st).Issues.Single();
            DisciplineIssue fromOdd = Coordinates(unread).Issues.Single();

            Assert.That(fromSt.Finding, Is.EqualTo("40.0 m from the ST model. EL on site S, ST on S"), "ALIGNMENT's rule, the first placed model where AR is not");
            Assert.That(fromSt.Row.ComparedWith, Is.EqualTo("ST"));
            Assert.That(fromOdd.Finding, Is.EqualTo("40.0 m from the plain.nwc model. EL on site S, plain.nwc on S"));
        }

        [Test]
        public void APlacementOfNoNumberIsNeverThreeNoughts()
        {
            ModelFacts model = new ModelFacts("a.nwc").WithPlacement(double.NaN, 0, 0);

            Assert.That(model.IsPlaced, Is.False);
            Assert.That(model.PlacementNotRead, Is.EqualTo("its transform gave no number"));
        }

        // ---------- missing model ----------

        [Test]
        public void AGroupOfThreeDisciplinesMissingElIsOneRowOfTheElWorkbookAndOneBuildingChecked()
        {
            List<DisciplineGroup> groups = new List<DisciplineGroup>
            {
                new DisciplineGroup("1B06PS", new[] { Nwc("1B06PS", "AR"), Nwc("1B06PS", "ME"), Nwc("1B06PS", "ST") }, null, "not run")
            };

            CheckFindings found = GroupDisciplinesCheck.Missing(groups, ModelRows.Of(groups, Names), Settings, Names);

            DisciplineIssue el = found.Issues.Single();
            Assert.That(el.Discipline, Is.EqualTo("EL"));
            Assert.That(el.Finding, Is.EqualTo("No EL model. AR, ME and ST are issued"));
            Assert.That(el.Value, Is.Null);
            Assert.That(el.Fix, Is.EqualTo("Issue the EL model for this building"));
            Assert.That(el.Building, Is.EqualTo("1B06PS"));
            Assert.That(el.Row.IsMissing, Is.True);
            Assert.That(el.Row.Present, Is.EqualTo("AR, ME and ST"));
            Assert.That(el.Row.Status, Is.EqualTo("MISSING"));
            Assert.That(el.Priority, Is.EqualTo(CheckWords.High));
            Assert.That(found.CheckedFor("EL"), Is.EqualTo(1), "one building, never its three files");
        }

        [Test]
        public void AGroupOfOneDisciplineMissesNothingAndItsModelsAreAloneAndNotChecked()
        {
            List<DisciplineGroup> groups = new List<DisciplineGroup>
            {
                new DisciplineGroup("1B06PG", new[] { Nwc("1B06PG", "EL"), Nwc("1B06PG", "EL", "000002") }, null, "this group did not run in this run").NotBuilt(),
                new DisciplineGroup("1B06PS", new[] { Nwc("1B06PS", "AR"), Nwc("1B06PS", "EL") }, null, "not run")
            };
            ModelRows rows = ModelRows.Of(groups, Names);

            Assert.That(GroupDisciplinesCheck.Missing(groups, rows, Settings, Names).Issues.Any(i => i.Building == "1B06PG"), Is.False);
            CoordinatesCheck.Of(groups, rows, Settings, Names);
            List<ModelRow> pg = rows.All.Where(r => r.Building == "1B06PG").ToList();
            Assert.That(pg.Count, Is.EqualTo(2));
            Assert.That(pg.All(r => r.Measure == ModelMeasure.Alone && !r.Checked && r.Status == "NOT CHECKED"), Is.True);
        }

        /// <summary>
        /// The reviewer's finding on F234's first pass: an NWF that links a file not on the disk is built, F233's NOT JUDGED,
        /// while its facts hold only the models that loaded, one discipline of them, and a model of the federated discipline or a
        /// name that does not read make F219's rule say UNKNOWN. None of them is a Missing model row.
        /// </summary>
        [Test]
        public void AGroupTheRunBuiltIsCheckedAndADisciplineUnknownMissesNothing()
        {
            List<DisciplineGroup> groups = new List<DisciplineGroup>
            {
                new DisciplineGroup("1B06BC", new[] { Nwc("1B06BC", "AR") }, null, "built, a link not on the disk"),
                new DisciplineGroup("1B06PS", new[] { Nwc("1B06PS", "AR"), Nwc("1B06PS", "BM") }, null, "not run"),
                new DisciplineGroup("1B06PK", new[] { Nwc("1B06PK", "AR"), TestPaths.At("P", "plain.nwc") }, null, "not run")
            };
            ModelRows rows = ModelRows.Of(groups, Names);

            Assert.That(rows.All.Single(r => r.Building == "1B06BC").Checked, Is.True, "a built group is checked");
            CheckFindings missing = GroupDisciplinesCheck.Missing(groups, rows, Settings, Names);
            Assert.That(missing.Issues, Is.Empty);
            Assert.That(missing.Notes.Count(n => n.Contains("whether it misses a discipline is UNKNOWN")), Is.EqualTo(2));
        }

        [Test]
        public void AGroupWhoseFilesWereNotReadIsNeverCountedAsMissingADiscipline()
        {
            List<DisciplineGroup> groups = new List<DisciplineGroup> { new DisciplineGroup("x.nwf", new string[0], null, "its Navisworks wrote no result") };

            CheckFindings found = GroupDisciplinesCheck.Missing(groups, ModelRows.Of(groups, Names), Settings, Names);

            Assert.That(found.Issues, Is.Empty);
            Assert.That(found.Notes.Single(), Does.Contain("whether it misses a discipline is UNKNOWN"));
        }

        // ---------- naming ----------

        [Test]
        public void TheArPatternIsLearnedAndAFileBreakingAFixedPartIsNamedWithWhatArHasThere()
        {
            ReferenceNaming naming = ReferenceNaming.Learn(new[] { Nwc("1B06BC", "AR"), Nwc("1B06G1", "AR") }, Settings, Names);

            Assert.That(naming.Describe(), Is.EqualTo("learned from 2 AR names, 7 parts split on -: part 1 1104, part 2 PAR, part 3 shaped like 1B06BC, part 4 ZZZ, part 5 the discipline, part 6 MOD, part 7 shaped like 000001"));
            Assert.That(naming.Pattern("ME"), Is.EqualTo("1104-PAR-<building>-ZZZ-ME-MOD-<number>"));
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
        public void OnlyTheBreakingFilesAreRowsEachPartALineOfTheLogAndTheReferencesOwnFilesAreNotJudged()
        {
            List<DisciplineGroup> groups = new List<DisciplineGroup>
            {
                new DisciplineGroup("1B06BC", new[] { Nwc("1B06BC", "AR"), Nwc("1B06BC", "EL"), TestPaths.At("P", "1104-PAR-1B06BC-ZZZ-ME-MOD-01.nwc") }, null, "not run")
            };
            ModelRows rows = ModelRows.Of(groups, Names);
            ReferenceNaming naming;

            CheckFindings found = ReferenceNaming.Of(groups, rows, Settings, Names, out naming);

            DisciplineIssue me = found.Issues.Single();
            Assert.That(me.Discipline, Is.EqualTo("ME"));
            Assert.That(me.Finding, Is.EqualTo("Name should be 1104-PAR-1B06BC-ZZZ-ME-MOD-<number>"), "one AR name holds its building alike");
            Assert.That(me.Fix, Is.EqualTo("Rename the NWC to the project pattern"));
            Assert.That(me.Priority, Is.EqualTo(CheckWords.Medium));
            Assert.That(me.Row.NameFollows, Is.False);
            Assert.That(rows.All.Single(r => r.Discipline == "EL").NameFollows, Is.True);
            Assert.That(rows.All.Single(r => r.Discipline == "AR").NameFollows, Is.Null, "the pattern is not judged");
            Assert.That(found.Notes, Has.Some.EqualTo("Naming  1104-PAR-1B06BC-ZZZ-ME-MOD-01.nwc  part 7 reads 01, AR has a part shaped like 000001 there"));
            Assert.That(found.CheckedFor("EL"), Is.EqualTo(1));
            Assert.That(found.CheckedFor("AR"), Is.EqualTo(0), "the reference's own files are the pattern");
            Assert.That(found.Notes[0], Does.StartWith("Naming  learned from 1 AR names"));
        }

        [Test]
        public void WithNoArFileNoPatternIsLearnedNoNameIsJudgedAndTheLogSaysSo()
        {
            List<DisciplineGroup> groups = new List<DisciplineGroup> { new DisciplineGroup("1B06BS", new[] { Nwc("1B06BS", "EL") }, null, "not run") };
            ReferenceNaming naming;

            CheckFindings found = ReferenceNaming.Of(groups, ModelRows.Of(groups, Names), Settings, Names, out naming);

            Assert.That(found.Issues, Is.Empty);
            Assert.That(found.Notes.Single(), Is.EqualTo("Naming  no AR file in this package, so no pattern was learned and no name was checked"));
            Assert.That(naming.Pattern("EL"), Is.EqualTo("no AR file in this package, so no pattern was learned and no name was checked"));
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
        public void OnlyAFileOlderThanTheLimitIsARowEveryAgeIsOnItsRowAndADateNotReadIsSaidInTheLog()
        {
            DateTime now = new DateTime(2026, 10, 18, 1, 0, 0);
            List<DisciplineGroup> groups = new List<DisciplineGroup> { new DisciplineGroup("1B06BC", new[] { Nwc("1B06BC", "AR"), Nwc("1B06BC", "EL"), Nwc("1B06BC", "ME") }, null, "not run") };
            List<FileDate> dates = new List<FileDate>
            {
                new FileDate(Nwc("1B06BC", "AR"), now.AddDays(-8), null),
                new FileDate(Nwc("1B06BC", "EL"), now.AddDays(-7), null),
                new FileDate(Nwc("1B06BC", "ME"), null, "the file is gone")
            };
            ModelRows rows = ModelRows.Of(groups, Names);

            CheckFindings found = FileAgeCheck.Of(groups, rows, dates, now, Settings, Names);

            DisciplineIssue ar = found.Issues.Single();
            Assert.That(ar.Discipline, Is.EqualTo("AR"), "exactly the limit is not older than it");
            Assert.That(ar.Finding, Is.EqualTo("Saved 10 Oct 2026"));
            Assert.That(ar.Value, Is.EqualTo(8));
            Assert.That(ar.Unit, Is.EqualTo("days"));
            Assert.That(ar.Fix, Is.EqualTo("Export a fresh NWC from the current model"));
            Assert.That(rows.All.Single(r => r.Discipline == "EL").AgeDays, Is.EqualTo(7));
            Assert.That(rows.All.Single(r => r.Discipline == "ME").AgeDays, Is.Null);
            Assert.That(found.CheckedFor("EL"), Is.EqualTo(1));
            Assert.That(found.CheckedFor("ME"), Is.EqualTo(0));
            Assert.That(found.Notes.Single(), Is.EqualTo("File age  1104-PAR-1B06BC-ZZZ-ME-MOD-000001.nwc not read, its date was not read, the file is gone"));
        }

        /// <summary>F244 attempt 2, the breaker's WRONG: the age is judged on the whole days the cell writes, so a cell of 7 is never a row.</summary>
        [Test]
        public void TheAgeIsJudgedOnTheWholeDaysTheCellWritesSo7Days3HoursIsNoRowAnd8DaysIs()
        {
            DateTime now = new DateTime(2026, 10, 18, 12, 0, 0);
            List<DisciplineGroup> groups = new List<DisciplineGroup> { new DisciplineGroup("1B06BC", new[] { Nwc("1B06BC", "AR"), Nwc("1B06BC", "EL") }, null, "not run") };
            List<FileDate> dates = new List<FileDate>
            {
                new FileDate(Nwc("1B06BC", "AR"), now.AddDays(-7).AddHours(-3), null),
                new FileDate(Nwc("1B06BC", "EL"), now.AddDays(-8), null)
            };
            ModelRows rows = ModelRows.Of(groups, Names);

            CheckFindings found = FileAgeCheck.Of(groups, rows, dates, now, Settings, Names);

            Assert.That(rows.All.Single(r => r.Discipline == "AR").AgeDays, Is.EqualTo(7));
            Assert.That(found.Issues.Any(i => i.Discipline == "AR"), Is.False, "7 days 3 hours writes 7, which is not over 7");
            Assert.That(found.Issues.Single().Discipline, Is.EqualTo("EL"));
            Assert.That(found.Issues.Single().Value, Is.EqualTo(8));
        }

        /// <summary>F244 attempt 2, the breaker's S9: the short name is the discipline and the number part of the naming settings, never the last part.</summary>
        [Test]
        public void TheShortNameIsTheDisciplineAndTheNumberPartNeverTheLastPart()
        {
            string one = TestPaths.At("P", "1104-PAR-1B06BC-ZZZ-ME-MOD-000001-A.nwc");
            string two = TestPaths.At("P", "1104-PAR-1B06BC-ZZZ-ME-MOD-000002-A.nwc");
            List<DisciplineGroup> groups = new List<DisciplineGroup> { new DisciplineGroup("1B06BC", new[] { one, two, Nwc("1B06BC", "AR") }, null, "not run") };

            ModelRows rows = ModelRows.Of(groups, Names);

            Assert.That(rows.All.Select(r => r.Model).ToArray(), Is.EqualTo(new[] { "ME-000001", "ME-000002", "AR-000001" }), "an 8 part name, two models apart by their number alone");
            Assert.That(ContainerName.Parse(one, Names).Number, Is.EqualTo("000001"), "part 7, the naming settings' NumberPart");
            ParsedContainerName six = ContainerName.Parse(TestPaths.At("P", "1104-PAR-1B06BC-ZZZ-ME-MOD.nwc"), Names);
            Assert.That(six.IsReadable, Is.True, "the number never raises the parts a name needs");
            Assert.That(six.Number, Is.Null);
        }

        // ---------- CAD imports and Generic Models ----------

        [Test]
        public void OneRowPerModelAndCadFileWithItsElementsEachIdOnceAndOnePerFamilyAndType()
        {
            ModelFacts me = new ModelFacts(Nwc("1B06PH", "ME", "000002")).WithPlacement(0, 0, 0).WithSite("S")
                .WithCad(new[]
                {
                    new ElementFact("1819523", "Chem-Feed Pkg-R0.dwg", "MEC-MEC-Doising Pump-PAR", "Standard", "GRF"),
                    new ElementFact("1819524", "Chem-Feed Pkg-R0.dwg", "MEC-MEC-Doising Pump-PAR", "Standard", "GRF"),
                    new ElementFact("1819524", "Chem-Feed Pkg-R0.dwg", "MEC-MEC-Doising Pump-PAR", "Standard", "GRF"),
                    new ElementFact(null, null, "F", "T", null)
                })
                .WithGeneric(new[]
                {
                    new ElementFact("2342282", "PAR-ST-600 X 600", "REC_OPENING COVER", "PAR-ST-600 X 600", "TANK ROOF"),
                    new ElementFact("2342283", "PAR-ST-600 X 600", "REC_OPENING COVER", "PAR-ST-600 X 600", "TANK ROOF")
                });
            List<DisciplineGroup> groups = new List<DisciplineGroup> { new DisciplineGroup("1B06PH", new[] { me.File }, new[] { me }, null) };
            ModelRows rows = ModelRows.Of(groups, Names);

            List<DisciplineIssue> cad = ElementChecks.Cad(groups, rows, Settings).Issues.ToList();
            DisciplineIssue generic = ElementChecks.Generic(groups, rows, Settings).Issues.Single();

            Assert.That(cad.Count, Is.EqualTo(2));
            Assert.That(cad[0].Finding, Is.EqualTo("Imported Chem-Feed Pkg-R0.dwg"));
            Assert.That(cad[0].Value, Is.EqualTo(2), "1819524 read twice is one element");
            Assert.That(cad[0].Unit, Is.EqualTo("elements"));
            Assert.That(cad[0].ElementIds.ToArray(), Is.EqualTo(new[] { "1819523", "1819524" }));
            Assert.That(cad[0].Source, Is.EqualTo("Chem-Feed Pkg-R0.dwg"));
            Assert.That(cad[1].Finding, Is.EqualTo("Imported not readable"));
            Assert.That(cad[1].ElementIds.ToArray(), Is.EqualTo(new string[] { null }));
            Assert.That(cad[1].Unit, Is.EqualTo("element"));
            Assert.That(cad[0].Priority, Is.EqualTo(CheckWords.Low));
            Assert.That(cad[0].Fix, Is.EqualTo("Replace with a Revit family, or delete it"));
            Assert.That(generic.Finding, Is.EqualTo("Family REC_OPENING COVER, type PAR-ST-600 X 600"));
            Assert.That(generic.Value, Is.EqualTo(2));
            Assert.That(generic.ElementIds.ToArray(), Is.EqualTo(new[] { "2342282", "2342283" }));
            Assert.That(generic.Fix, Is.EqualTo("Move the family to its right category"));
            Assert.That(rows.All.Single().CadFiles, Is.EqualTo(2), "the distinct CAD files, the one not readable among them");
            Assert.That(rows.All.Single().GenericElements, Is.EqualTo(2));
        }

        [Test]
        public void AModelWithNoneIsNoRowAndOneNotWalkedIsSaidInTheLog()
        {
            ModelFacts none = new ModelFacts(Nwc("1B06BC", "AR")).WithCad(new List<ElementFact>()).GenericNot("the search threw");
            List<DisciplineGroup> groups = new List<DisciplineGroup> { new DisciplineGroup("1B06BC", new[] { none.File }, new[] { none }, null) };
            ModelRows rows = ModelRows.Of(groups, Names);

            CheckFindings cad = ElementChecks.Cad(groups, rows, Settings);
            CheckFindings generic = ElementChecks.Generic(groups, rows, Settings);

            Assert.That(cad.Issues, Is.Empty);
            Assert.That(cad.CheckedFor("AR"), Is.EqualTo(1));
            Assert.That(rows.All.Single().CadFiles, Is.EqualTo(0));
            Assert.That(generic.Issues, Is.Empty);
            Assert.That(generic.CheckedFor("AR"), Is.EqualTo(0));
            Assert.That(rows.All.Single().GenericElements, Is.Null, "not walked is UNKNOWN, never a nought");
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

        // ---------- the priorities ----------

        [Test]
        public void ThePrioritiesAreBadersAndACheckNotOfTheSixIsRefused()
        {
            Assert.That(CheckWords.All.Select(CheckWords.PriorityOf).ToArray(),
                Is.EqualTo(new[] { "High", "High", "Medium", "Medium", "Low", "Low" }));
            Assert.That(() => CheckWords.PriorityOf("Box gap"), Throws.ArgumentException);
            Assert.That(CheckWords.SeverityRank("High"), Is.LessThan(CheckWords.SeverityRank("Medium")));
            Assert.That(CheckWords.SeverityRank("Medium"), Is.LessThan(CheckWords.SeverityRank("Low")));
        }
    }
}
