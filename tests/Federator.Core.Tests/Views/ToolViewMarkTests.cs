using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using Federator.Core.Views;
using NUnit.Framework;

namespace Federator.Core.Tests
{
    /// <summary>
    /// F114, Q114 point 16 and Q120 by its default A. Every view and folder this tool makes
    /// carries one comment: a sentence a person reads and a fingerprint of where it was written,
    /// its name, its camera and, where P10 shows it holds, its Guid. A view is the tool's only
    /// while that one comment is there and every part still reads as written. Renamed, moved,
    /// turned, commented on, drawn on, copied or not provable, it is a person's from then on.
    /// </summary>
    [TestFixture]
    public class ToolViewMarkTests
    {
        private static readonly string[] Path = { "A", "Structure vs Mechanical" };
        private const string Name = "BLD-ME-Ducts-vs-BLD-ST-Columns";

        private static readonly ViewpointSettings Settings = new ViewpointSettings();

        private static readonly Point3 Camera = new Point3(12.3456, -7.0, 1000.25);

        private static string Stamp()
        {
            return ToolViewMark.StampOf(new DateTime(2026, 10, 5, 9, 52, 53, DateTimeKind.Utc));
        }

        private static string BodyOf(string[] path, string name, Point3 camera, string guid = null)
        {
            return ToolViewMark.Body(Stamp(), path, name, camera, guid, Settings);
        }

        private static MarkJudgement Judge(string[] path, string name, Point3 camera, IList<string> comments, int? redlines = 0, string guid = null)
        {
            return ToolViewMark.Judge(path, name, camera, comments, redlines, guid, Settings);
        }

        /// <summary>
        /// The add-in finds the view it just recorded as the child of its folder with its name
        /// and no mark, P12 YES, and refuses to mark where a person's unmarked view of that name
        /// already sits there, P22 unrun. No mark means no comment carrying the tag, so a mark that
        /// does not read still counts as one, because that view is not the one just recorded.
        /// </summary>
        [Test]
        public void ACommentCarryingTheTagIsAMarkAndAPersonsCommentIsNot()
        {
            Assert.That(ToolViewMark.CarriesAMark(new[] { BodyOf(Path, Name, Camera) }, Settings), Is.True);
            Assert.That(ToolViewMark.CarriesAMark(new[] { "Please keep this one, it shows the riser", BodyOf(Path, Name, Camera) }, Settings), Is.True);
            Assert.That(ToolViewMark.CarriesAMark(new[] { Settings.MarkTag + " and nothing that reads" }, Settings), Is.True);
            Assert.That(ToolViewMark.CarriesAMark(new[] { "Please keep this one, it shows the riser" }, Settings), Is.False);
            Assert.That(ToolViewMark.CarriesAMark(new string[0], Settings), Is.False);
            Assert.That(ToolViewMark.CarriesAMark(null, Settings), Is.False);
            Assert.That(ToolViewMark.CarriesAMark(new string[] { null }, Settings), Is.False);
            Assert.That(() => ToolViewMark.CarriesAMark(new string[0], null), Throws.ArgumentNullException);
        }

        /// <summary>The comment's author is a setting, the way every word the add-in writes is, and names the tool and no person.</summary>
        [Test]
        public void TheMarkAuthorIsASettingThatNamesTheTool()
        {
            Assert.That(ViewpointSettings.DefaultMarkAuthor, Is.Not.Empty);
            Assert.That(ViewpointSettings.DefaultMarkAuthor, Does.Contain("Federator"));
            Assert.That(new ViewpointSettings().MarkAuthor, Is.EqualTo(ViewpointSettings.DefaultMarkAuthor));
        }

        /// <summary>
        /// The breaker's B6 of F114's add-in pass. The mark stores the camera to three decimals
        /// and the tolerance is 0.001, so a camera far from the origin whose thousandths round
        /// read as moved by rounding alone. Both sides are rounded the same way before the
        /// distance is taken, so an unmoved camera at 500000 units is the tool's, and one moved
        /// by a hundredth is a person's. Whether the NWF keeps the camera at single precision is
        /// UNKNOWN until the timed runs.
        /// </summary>
        [Test]
        public void ACameraFarFromTheOriginIsNotReadAsMovedByRoundingAlone()
        {
            Point3 far = new Point3(500000.1234, 500000.5678, 500000.9999);
            string[] comments = { BodyOf(Path, Name, far) };

            MarkJudgement same = ToolViewMark.Judge(Path, Name, far, comments, 0, null, Settings);
            Assert.That(same.Owner, Is.EqualTo(ViewOwner.Ours), same.Why);

            Point3 nudged = new Point3(500000.1236, 500000.5676, 500000.9999);
            Assert.That(ToolViewMark.Judge(Path, Name, nudged, comments, 0, null, Settings).Owner, Is.EqualTo(ViewOwner.Ours), "within the thousandth the mark stores");

            Point3 moved = new Point3(500000.1334, 500000.5678, 500000.9999);
            MarkJudgement changed = ToolViewMark.Judge(Path, Name, moved, comments, 0, null, Settings);
            Assert.That(changed.Owner, Is.EqualTo(ViewOwner.ChangedByAPerson));
            Assert.That(changed.Why, Is.EqualTo("its camera was moved"));
        }

        [Test]
        public void AMarkWrittenThenReadGivesTheSameFingerprint()
        {
            string body = BodyOf(Path, Name, Camera, "a1b2");
            ToolViewMark mark = ToolViewMark.Read(body, Settings);

            Assert.That(body, Does.StartWith(ViewpointSettings.DefaultMarkSentence), "a person reads the sentence first");
            Assert.That(mark, Is.Not.Null);
            Assert.That(mark.Stamp, Is.EqualTo("2026-10-05T09:52:53Z"));
            Assert.That(mark.FolderPath, Is.EqualTo("A/Structure vs Mechanical"));
            Assert.That(mark.Name, Is.EqualTo(Name));
            Assert.That(mark.Camera.DistanceTo(Camera), Is.LessThan(0.001));
            Assert.That(mark.Guid, Is.EqualTo("a1b2"));
        }

        /// <summary>Names the mark's own words could break: a trailing space, the field words themselves, colons and digits.</summary>
        [Test]
        public void AnyNameReadsBackExactly()
        {
            foreach (string name in new[] { "Walls-vs-Columns ", " guid=7:x name=3:abc", "12:34", string.Empty, "Over 150mm" })
            {
                ToolViewMark mark = ToolViewMark.Read(BodyOf(new[] { "A", "Structure vs Mechanical", "Over 150mm" }, name, null), Settings);

                Assert.That(mark, Is.Not.Null, name);
                Assert.That(mark.Name, Is.EqualTo(name));
                Assert.That(mark.Camera, Is.Null, "a folder's mark carries no camera");
                Assert.That(mark.Guid, Is.Null);
            }
        }

        /// <summary>Whether a comment keeps its line break is UNKNOWN until P9, so the mark is found wherever it sits in the body.</summary>
        [Test]
        public void TheMarkIsReadWhereverItSitsInTheBody()
        {
            string body = BodyOf(Path, Name, Camera).Replace("\r\n", " ").Replace("\n", " ");

            Assert.That(ToolViewMark.Read(body, Settings), Is.Not.Null);
        }

        [Test]
        public void TheStampIsTheSameUnderEveryCulture()
        {
            CultureInfo was = Thread.CurrentThread.CurrentCulture;

            try
            {
                foreach (string culture in new[] { "th-TH", "de-DE", "ar-SA", "fa-IR" })
                {
                    Thread.CurrentThread.CurrentCulture = new CultureInfo(culture);
                    Assert.That(Stamp(), Is.EqualTo("2026-10-05T09:52:53Z"), culture);
                    Assert.That(ToolViewMark.Read(BodyOf(Path, Name, Camera), Settings).Camera.X, Is.EqualTo(12.346).Within(1e-9), culture);
                }
            }
            finally
            {
                Thread.CurrentThread.CurrentCulture = was;
            }
        }

        [Test]
        public void AViewAsTheToolWroteItIsOurs()
        {
            MarkJudgement judged = Judge(Path, Name, new Point3(12.3455, -7.0002, 1000.2501), new[] { BodyOf(Path, Name, Camera) });

            Assert.That(judged.Owner, Is.EqualTo(ViewOwner.Ours), judged.Why);
            Assert.That(judged.Mark.Stamp, Is.EqualTo(Stamp()));
        }

        [Test]
        public void AFolderAsTheToolWroteItIsOurs()
        {
            MarkJudgement judged = Judge(new[] { "A" }, "Structure vs Mechanical", null, new[] { BodyOf(new[] { "A" }, "Structure vs Mechanical", null) });

            Assert.That(judged.Owner, Is.EqualTo(ViewOwner.Ours), judged.Why);
        }

        [Test]
        public void EachChangeAPersonMakesMakesItTheirs()
        {
            string[] body = { BodyOf(Path, Name, Camera) };

            Assert.That(Judge(Path, Name + " mine", Camera, body).Owner, Is.EqualTo(ViewOwner.ChangedByAPerson), "renamed");
            Assert.That(Judge(new[] { "B", "Structure vs Mechanical" }, Name, Camera, body).Owner, Is.EqualTo(ViewOwner.ChangedByAPerson), "moved");
            Assert.That(Judge(Path, Name, new Point3(12.3456, -7.0, 1001.0), body).Owner, Is.EqualTo(ViewOwner.ChangedByAPerson), "turned");
            Assert.That(Judge(Path, Name, Camera, new[] { body[0], "check this one, Rami" }).Owner, Is.EqualTo(ViewOwner.ChangedByAPerson), "commented on");
            Assert.That(Judge(Path, Name, Camera, body, 1).Owner, Is.EqualTo(ViewOwner.ChangedByAPerson), "drawn on");
            Assert.That(Judge(Path, Name, Camera, body).Owner, Is.EqualTo(ViewOwner.Ours), "and unchanged it is still ours");
        }

        /// <summary>A copy a person made in another folder carries the same comment and fails the fingerprint.</summary>
        [Test]
        public void ACopyInAnotherFolderIsAPersons()
        {
            MarkJudgement judged = Judge(new[] { "My views" }, Name, Camera, new[] { BodyOf(Path, Name, Camera) });

            Assert.That(judged.Owner, Is.EqualTo(ViewOwner.ChangedByAPerson));
            Assert.That(judged.Why, Does.Contain("My views"));
        }

        /// <summary>S1: what cannot be proved unchanged is kept. Redlines or a Guid that could not be read prove nothing.</summary>
        [Test]
        public void WhatCannotBeReadToProveItKeepsTheView()
        {
            Assert.That(Judge(Path, Name, Camera, new[] { BodyOf(Path, Name, Camera) }, null).Owner,
                Is.EqualTo(ViewOwner.ChangedByAPerson), "redlines not read");
            Assert.That(Judge(Path, Name, Camera, new[] { BodyOf(Path, Name, Camera, "a1b2") }, 0, null).Owner,
                Is.EqualTo(ViewOwner.ChangedByAPerson), "the Guid not read");
            Assert.That(Judge(Path, Name, Camera, new[] { BodyOf(Path, Name, Camera, "a1b2") }, 0, "c3d4").Owner,
                Is.EqualTo(ViewOwner.ChangedByAPerson), "another Guid, a copy");
            Assert.That(Judge(Path, Name, Camera, new[] { BodyOf(Path, Name, Camera, "a1b2") }, 0, "a1b2").Owner,
                Is.EqualTo(ViewOwner.Ours), "the same Guid");
            Assert.That(Judge(Path, Name, null, new[] { BodyOf(Path, Name, Camera) }).Owner,
                Is.EqualTo(ViewOwner.ChangedByAPerson), "a camera that could not be read");
        }

        [Test]
        public void NoMarkOrAMarkThatDoesNotReadIsNotOurs()
        {
            string body = BodyOf(Path, Name, Camera);
            string broken = body.Substring(0, body.Length - 3);

            Assert.That(ToolViewMark.Read(broken, Settings), Is.Null);
            Assert.That(Judge(Path, Name, Camera, new[] { broken }).Owner, Is.EqualTo(ViewOwner.NotOurs));
            Assert.That(Judge(Path, Name, Camera, new[] { "a view Rami saved" }).Owner, Is.EqualTo(ViewOwner.NotOurs));
            Assert.That(Judge(Path, Name, Camera, new string[0]).Owner, Is.EqualTo(ViewOwner.NotOurs));
            Assert.That(Judge(Path, Name, Camera, null).Owner, Is.EqualTo(ViewOwner.NotOurs));
            Assert.That(ToolViewMark.Read(ViewpointSettings.DefaultMarkTag + " stamp=99:x", Settings), Is.Null, "a length past the end");
        }
    }
}
