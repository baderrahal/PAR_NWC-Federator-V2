using System;
using System.Collections.Generic;
using Federator.Core.Clash;
using Federator.Core.Teams;
using Federator.Core.Views;
using NUnit.Framework;

namespace Federator.Core.Tests
{
    /// <summary>
    /// F114, Q114 point 13, what one view of a test's open clashes shows. Only the models its
    /// clashing items live in are shown, Bader's answer B to Q119 on 2026-10-05, a home in a third
    /// team's model is shown and named, Q118 A, and every other model is hidden, a model of the
    /// pair's own teams and a model whose code will not read among them. The items are red and
    /// green, and the camera is framed on a box over the open clash centres. The file names here
    /// are sample data only.
    /// </summary>
    [TestFixture]
    public class WhatAViewShowsTests
    {
        private static TeamMap Map()
        {
            return TeamMapTests.MapOf(TeamMapTests.BadersMap);
        }

        private static ModelTeam Model(string code, string suffix = "000001")
        {
            string name = "1104-PAR-1A02MM-ZZZ-" + code + "-MOD-" + suffix + ".nwc";
            return new ModelTeam(name, code, Map().TeamOf(code));
        }

        private static TeamPair PairOf(string codeA, string codeB)
        {
            TeamMap map = Map();
            return TeamPair.For(map.TeamOf(codeA), map.TeamOf(codeB), map, ViewpointSettings.DefaultPairSeparator);
        }

        private static List<string> CodesOf(IEnumerable<ModelTeam> models)
        {
            List<string> codes = new List<string>();

            foreach (ModelTeam model in models)
            {
                codes.Add(model.Code);
            }

            return codes;
        }

        private static ItemPath At(params int[] indexes)
        {
            return new ItemPath(indexes);
        }

        private static ViewClash Clash(string name, ItemPath first, ItemPath second, Point3 centre = null)
        {
            return new ViewClash("T", name, "BLD-ME-Ducts", "BLD-ST-Columns", ClashStatus.New, ClashPriority.A,
                null, first, second, centre, null, null);
        }

        // ---------- ShownModels ----------

        /// <summary>
        /// Q119 B: a view of Structure vs Electrical whose clashing items all live in the EL and ST
        /// models shows those two and hides the rest.
        /// </summary>
        [Test]
        public void StructureVsElectricalShowsTheModelsItsItemsLiveInAndHidesTheRest()
        {
            ModelTeam[] models = { Model("AR"), Model("EL"), Model("ME"), Model("ST") };

            ShownModels shown = ShownModels.For(PairOf("ST", "EL"), models, new[] { Model("ST").FileName, Model("EL").FileName });

            Assert.That(CodesOf(shown.Shown), Is.EqualTo(new[] { "EL", "ST" }));
            Assert.That(CodesOf(shown.Hidden), Is.EqualTo(new[] { "AR", "ME" }));
            Assert.That(shown.Exceptions, Is.Empty);
        }

        /// <summary>Q119 B, Bader's words: a view shows only the models its clashing items live in, so a model of the pair's own team no item lives in is hidden.</summary>
        [Test]
        public void AModelOfThePairsTeamNoItemLivesInIsHidden()
        {
            ModelTeam[] models = { Model("AR"), Model("EL"), Model("ME"), Model("ST") };

            ShownModels shown = ShownModels.For(PairOf("ST", "EL"), models, new[] { Model("EL").FileName });

            Assert.That(CodesOf(shown.Shown), Is.EqualTo(new[] { "EL" }));
            Assert.That(CodesOf(shown.Hidden), Is.EqualTo(new[] { "AR", "ME", "ST" }));
        }

        /// <summary>A drainage item living in the ME model is in its own team, Mechanical, and is no exception.</summary>
        [Test]
        public void AnItemInAModelOfItsOwnTeamIsNoException()
        {
            ModelTeam[] models = { Model("AR"), Model("EL"), Model("ME"), Model("ST") };

            ShownModels shown = ShownModels.For(PairOf("AR", "DR"), models, new[] { Model("ME").FileName });

            Assert.That(CodesOf(shown.Shown), Is.EqualTo(new[] { "ME" }));
            Assert.That(shown.Exceptions, Is.Empty);
        }

        /// <summary>Q118 A: an electrical item living in the AR model, in Mechanical vs Electrical, shows the AR model and names it.</summary>
        [Test]
        public void AnItemInAThirdTeamsModelShowsThatModelAndNamesIt()
        {
            ModelTeam[] models = { Model("AR"), Model("EL"), Model("ME"), Model("ST") };

            ShownModels shown = ShownModels.For(PairOf("ME", "EL"), models, new[] { Model("AR").FileName, string.Empty });

            Assert.That(CodesOf(shown.Shown), Is.EqualTo(new[] { "AR" }));
            Assert.That(CodesOf(shown.Exceptions), Is.EqualTo(new[] { "AR" }));
            Assert.That(CodesOf(shown.Hidden), Is.EqualTo(new[] { "EL", "ME", "ST" }));
        }

        /// <summary>
        /// F114 attempt 2, the breaker's finding 2. A home that could not be read, or that names
        /// no model of the group, was dropped without a word. Each is counted or named, since
        /// whether the model of that clashing item is shown is UNKNOWN.
        /// </summary>
        [Test]
        public void AHomeNotReadOrOfNoModelOfTheGroupIsCountedAndNamed()
        {
            ModelTeam[] models = { Model("AR"), Model("EL"), Model("ME"), Model("ST") };

            ShownModels shown = ShownModels.For(
                PairOf("ME", "EL"), models, new[] { string.Empty, null, "elsewhere.nwc", Model("ME").FileName, "elsewhere.nwc" });

            Assert.That(shown.HomesNotRead, Is.EqualTo(2));
            Assert.That(shown.HomesNotInGroup, Is.EqualTo(new[] { "elsewhere.nwc" }));
            Assert.That(CodesOf(shown.Shown), Is.EqualTo(new[] { "ME" }), "only the home that was read and is of the group");
        }

        /// <summary>
        /// F114 attempt 4, the breaker's blocking finding of attempt 3. A home was matched to a
        /// model by its exact text, so a home written as a path, or in another case, missed every
        /// model and the view showed nothing. A home and a model are matched by the one rule of
        /// what a name is, ContainerName.Stem compared without case, as SimilarNames compares a
        /// name in the NWF folder.
        /// </summary>
        [Test]
        public void AHomeWrittenAsAPathOrInAnotherCaseIsTheModelOfThatFileName()
        {
            ModelTeam[] models = { Model("AR"), Model("EL"), Model("ME"), Model("ST") };

            ShownModels shown = ShownModels.For(PairOf("ST", "EL"), models, new[]
            {
                @"C:\Projects\1A02MM\" + Model("EL").FileName,
                "D:/federated/" + Model("ST").FileName.ToUpperInvariant()
            });

            Assert.That(CodesOf(shown.Shown), Is.EqualTo(new[] { "EL", "ST" }));
            Assert.That(CodesOf(shown.Hidden), Is.EqualTo(new[] { "AR", "ME" }));
            Assert.That(shown.HomesNotInGroup, Is.Empty);
        }

        /// <summary>
        /// F114 attempt 4: the rule runs both ways. A model handed in by its path is matched by a
        /// home that gives only its file name, or its name with no extension, as a display name
        /// can, and a home of another file of a like name is still no model of the group.
        /// </summary>
        [Test]
        public void AModelHandedInByItsPathIsMatchedByItsFileName()
        {
            ModelTeam el = new ModelTeam(@"C:\Projects\1A02MM\" + Model("EL").FileName, "EL", "Electrical");
            ModelTeam st = new ModelTeam("D:/federated/" + Model("ST").FileName, "ST", "Structure");
            string stStem = Model("ST").FileName.Substring(0, Model("ST").FileName.Length - ".nwc".Length);

            ShownModels shown = ShownModels.For(PairOf("ST", "EL"), new[] { el, st }, new[] { Model("EL").FileName, stStem, Model("ST", "000002").FileName });

            Assert.That(CodesOf(shown.Shown), Is.EqualTo(new[] { "EL", "ST" }));
            Assert.That(shown.HomesNotInGroup, Is.EqualTo(new[] { Model("ST", "000002").FileName }));
        }

        /// <summary>Q119 B: a model whose code will not read is hidden where no item lives in it, and shown where one does.</summary>
        [Test]
        public void AModelWhoseCodeWillNotReadIsShownOnlyWhereAnItemLivesInIt()
        {
            ModelTeam odd = new ModelTeam("site survey.nwc", string.Empty, Map().TeamOf(string.Empty));
            ModelTeam[] models = { Model("AR"), odd, Model("ST") };

            ShownModels away = ShownModels.For(PairOf("ST", "ST"), models, new[] { Model("ST").FileName });
            ShownModels home = ShownModels.For(PairOf("ST", "ST"), models, new[] { Model("ST").FileName, "site survey.nwc" });

            Assert.That(CodesOf(away.Shown), Is.EqualTo(new[] { "ST" }));
            Assert.That(CodesOf(away.Hidden), Is.EqualTo(new[] { "AR", string.Empty }));
            Assert.That(CodesOf(home.Shown), Is.EqualTo(new[] { string.Empty, "ST" }));
            Assert.That(home.Exceptions, Is.Empty, "its team is UNKNOWN, so it is no exception, and check 3 says so");
        }

        /// <summary>1A04PK's ten models in Structure vs Mechanical with its items in HV and the second ST: those two shown, the other eight hidden.</summary>
        [Test]
        public void TenModelsOfOneBuildingShowOnlyTheTwoItsItemsLiveIn()
        {
            ModelTeam[] models =
            {
                Model("AR"), Model("EL"), Model("FP"), Model("HV"),
                Model("ME", "000001"), Model("ME", "000002"), Model("ME", "000003"), Model("ME", "000004"),
                Model("ST", "000001"), Model("ST", "000002")
            };

            ShownModels shown = ShownModels.For(
                PairOf("HV", "ST"), models, new[] { Model("HV").FileName, Model("ST", "000002").FileName, Model("HV").FileName });

            Assert.That(CodesOf(shown.Shown), Is.EqualTo(new[] { "HV", "ST" }));
            Assert.That(shown.Shown[1].FileName, Is.EqualTo(Model("ST", "000002").FileName));
            Assert.That(CodesOf(shown.Hidden), Is.EqualTo(new[] { "AR", "EL", "FP", "ME", "ME", "ME", "ME", "ST" }));
        }

        // ---------- PaintPlan ----------

        [Test]
        public void RedIsEveryFirstItemAndGreenEverySecondNotAlreadyRed()
        {
            PaintPlan paint = PaintPlan.For(new[]
            {
                Clash("Clash1", At(0, 1), At(1, 1)),
                Clash("Clash2", At(0, 1), At(1, 2)),
                Clash("Clash3", At(1, 2), At(0, 2)),
                Clash("Clash4", At(1, 1), At(0, 3))
            });

            Assert.That(paint.Red, Is.EqualTo(new[] { At(0, 1), At(1, 2), At(1, 1) }));
            Assert.That(paint.Green, Is.EqualTo(new[] { At(0, 2), At(0, 3) }));
            Assert.That(paint.Solid.Count, Is.EqualTo(5));
            Assert.That(paint.FirstAndSecond, Is.EqualTo(2), "1/2 and 1/1 are first in one clash and second in another");
            Assert.That(paint.NotPointedAt, Is.EqualTo(0));

            foreach (ItemPath item in paint.Green)
            {
                Assert.That(paint.Red, Has.No.Member(item), "red and green never share an item");
            }
        }

        [Test]
        public void AnItemNotPointedAtIsCountedAndTheOtherIsStillPainted()
        {
            PaintPlan paint = PaintPlan.For(new[] { Clash("Clash1", null, At(4)), Clash("Clash2", At(5), null) });

            Assert.That(paint.Red, Is.EqualTo(new[] { At(5) }));
            Assert.That(paint.Green, Is.EqualTo(new[] { At(4) }));
            Assert.That(paint.NotPointedAt, Is.EqualTo(2));
        }

        [Test]
        public void TheSameClashesGiveTheSamePaint()
        {
            ViewClash[] clashes = { Clash("Clash1", At(3, 1), At(2, 9)), Clash("Clash2", At(2, 9), At(3, 4)) };

            Assert.That(PaintPlan.For(clashes).Red, Is.EqualTo(PaintPlan.For(clashes).Red));
            Assert.That(PaintPlan.For(clashes).Green, Is.EqualTo(PaintPlan.For(clashes).Green));
        }

        // ---------- FramingBox ----------

        [Test]
        public void TwoCentresGiveTheirBoxPaddedByTheMargin()
        {
            FramingBox box = FramingBox.For(
                new[] { new Point3(1000, 2000, 3000), new Point3(4000, -500, 3500) }, 1000.0, "Millimeters");

            Assert.That(box.Min.X, Is.EqualTo(0.0));
            Assert.That(box.Min.Y, Is.EqualTo(-1500.0));
            Assert.That(box.Min.Z, Is.EqualTo(2000.0));
            Assert.That(box.Max.X, Is.EqualTo(5000.0));
            Assert.That(box.Max.Y, Is.EqualTo(3000.0));
            Assert.That(box.Max.Z, Is.EqualTo(4500.0));
        }

        /// <summary>The margin is millimetres and the box is in the document's units, so a feet document and a metres one frame the same place.</summary>
        [Test]
        public void FeetAndMetresGiveTheSameBox()
        {
            FramingBox metres = FramingBox.For(new[] { new Point3(0, 0, 0), new Point3(3.048, 6.096, 0) }, 304.8, "Meters");
            FramingBox feet = FramingBox.For(new[] { new Point3(0, 0, 0), new Point3(10, 20, 0) }, 304.8, "Feet");

            Assert.That(metres.Min.X * 1000.0, Is.EqualTo(feet.Min.X * 304.8).Within(1e-9));
            Assert.That(metres.Max.Y * 1000.0, Is.EqualTo(feet.Max.Y * 304.8).Within(1e-9));
            Assert.That(feet.Min.X, Is.EqualTo(-1.0).Within(1e-12));
            Assert.That(feet.Max.Y, Is.EqualTo(21.0).Within(1e-12));
        }

        [Test]
        public void AUnitTheTableDoesNotKnowIsRefused()
        {
            Assert.Throws<NotSupportedException>(
                () => FramingBox.For(new[] { new Point3(0, 0, 0), new Point3(1, 1, 1) }, 1000.0, "Furlongs"));
        }

        /// <summary>A view of one open clash gets no box and keeps Clash Detective's own camera, 5l.</summary>
        [Test]
        public void OneCentreOrNoneGivesNoBox()
        {
            Assert.That(FramingBox.For(new[] { new Point3(1, 2, 3) }, 1000.0, "Millimeters"), Is.Null);
            Assert.That(FramingBox.For(new[] { new Point3(1, 2, 3), null }, 1000.0, "Millimeters"), Is.Null);
            Assert.That(FramingBox.For(new Point3[0], 1000.0, "Millimeters"), Is.Null);
            Assert.That(FramingBox.For(null, 1000.0, "Millimeters"), Is.Null);
        }

        [Test]
        public void ANegativeMarginIsRefused()
        {
            Assert.Throws<ArgumentOutOfRangeException>(
                () => FramingBox.For(new[] { new Point3(0, 0, 0), new Point3(1, 1, 1) }, -1.0, "Millimeters"));
        }
    }
}
