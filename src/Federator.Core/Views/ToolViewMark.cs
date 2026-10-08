using System;
using System.Collections.Generic;
using System.Globalization;

namespace Federator.Core.Views
{
    /// <summary>
    /// The mark on every view and folder this tool makes, F114, Q114 point 16 and Q120 by its
    /// default A. Only a view this tool made is ever replaced or removed, and a person's is never
    /// touched wherever it sits, so the tool has to know its own. Measured on 2026-10-04,
    /// measure-views section 8: nothing marked one before, a viewpoint was the tool's when it
    /// sat at the path the plan gave, so a person's at that path read as the tool's.
    ///
    /// ONE COMMENT, TWO PARTS. A sentence a person reads in the Comments window, saying how to
    /// keep a view, then a line no person would type: the MarkTag setting, the run's stamp, the
    /// folders the view was written in, its name, its camera to a thousandth and a Guid field
    /// the add-in writes EMPTY: probe P10 read no Guid on any item the tool's routes make, before
    /// or after a save, docs\history\scan.md 5z-p, so the judge compares a Guid only where a mark
    /// carries one. Each text is written as its length, a colon and the text, so any name, a
    /// trailing space or the mark's own words in it included, reads back exactly. The mark is
    /// found wherever it sits in the body, because whether a comment keeps its line break was
    /// UNKNOWN until probe P9.
    ///
    /// THE JUDGE, S1 of the design. Ours only with exactly one comment, which is the mark, the
    /// folders and the name equal, Ordinal, the camera within CameraReadBackTolerance, the Guid
    /// equal where the mark carries one, and no redline. Renamed, moved, turned, commented on,
    /// drawn on or copied, it is a person's from then on, kept and named every run. So is one
    /// whose redlines, Guid or camera could not be read, because what cannot be proved the
    /// tool's is kept. No mark, or one that does not read, is not the tool's at all.
    /// </summary>
    public sealed class ToolViewMark
    {
        private const string StampFormat = "yyyy-MM-dd'T'HH:mm:ss'Z'";
        private const string CameraFormat = "0.000";
        private const string NoCamera = "-";
        private const string StampField = " stamp=";
        private const string PathField = " path=";
        private const string NameField = " name=";
        private const string CameraField = " camera=";
        private const string GuidField = " guid=";

        private ToolViewMark(string stamp, string folderPath, string name, Point3 camera, string guid)
        {
            Stamp = stamp;
            FolderPath = folderPath;
            Name = name;
            Camera = camera;
            Guid = guid;
        }

        /// <summary>The stamp of the run that wrote it.</summary>
        public string Stamp { get; private set; }

        /// <summary>The folders it was written in, joined by a slash.</summary>
        public string FolderPath { get; private set; }

        /// <summary>The name it was written with.</summary>
        public string Name { get; private set; }

        /// <summary>The camera position it was written with, or null for a folder.</summary>
        public Point3 Camera { get; private set; }

        /// <summary>Its Guid as written, or null where the mark carries none.</summary>
        public string Guid { get; private set; }

        /// <summary>A run's stamp, the same text under every culture and calendar.</summary>
        public static string StampOf(DateTime utc)
        {
            return utc.ToString(StampFormat, CultureInfo.InvariantCulture);
        }

        /// <summary>The whole comment body the tool writes on a view or a folder, in those folders, outermost first.</summary>
        public static string Body(string stamp, IList<string> folders, string name, Point3 camera, string guid, ViewpointSettings settings)
        {
            if (settings == null)
            {
                throw new ArgumentNullException("settings");
            }

            string cameraText = camera == null
                ? NoCamera
                : Number(camera.X) + "," + Number(camera.Y) + "," + Number(camera.Z);

            return settings.MarkSentence + "\n" + settings.MarkTag
                + StampField + Counted(stamp)
                + PathField + Counted(ViewPlace.FolderPath(folders))
                + NameField + Counted(name)
                + CameraField + cameraText
                + GuidField + Counted(guid);
        }

        /// <summary>The mark in that comment body, or null where it carries none or one that does not read.</summary>
        public static ToolViewMark Read(string body, ViewpointSettings settings)
        {
            if (settings == null)
            {
                throw new ArgumentNullException("settings");
            }

            if (string.IsNullOrEmpty(body) || string.IsNullOrEmpty(settings.MarkTag))
            {
                return null;
            }

            int at = body.IndexOf(settings.MarkTag, StringComparison.Ordinal);

            if (at < 0)
            {
                return null;
            }

            at += settings.MarkTag.Length;

            string stamp, path, name, guid;

            if (!Field(body, ref at, StampField) || !CountedAt(body, ref at, out stamp)
                || !Field(body, ref at, PathField) || !CountedAt(body, ref at, out path)
                || !Field(body, ref at, NameField) || !CountedAt(body, ref at, out name)
                || !Field(body, ref at, CameraField))
            {
                return null;
            }

            int guidAt = body.IndexOf(GuidField, at, StringComparison.Ordinal);

            if (guidAt < 0)
            {
                return null;
            }

            Point3 camera;

            if (!CameraIn(body.Substring(at, guidAt - at), out camera))
            {
                return null;
            }

            at = guidAt;

            if (!Field(body, ref at, GuidField) || !CountedAt(body, ref at, out guid))
            {
                return null;
            }

            return new ToolViewMark(stamp, path, name, camera, guid.Length == 0 ? null : guid);
        }

        /// <summary>
        /// Whether any of those comments carries the mark's tag, read or not. The add-in finds
        /// the view it just recorded as the child of its folder with its name and no mark, P12,
        /// and where a person's unmarked view of that name already sits there it marks nothing,
        /// P22 unrun, so a comment carrying the tag counts as a mark even where it does not read:
        /// that view is not the one just recorded either way.
        /// </summary>
        public static bool CarriesAMark(IList<string> comments, ViewpointSettings settings)
        {
            if (settings == null)
            {
                throw new ArgumentNullException("settings");
            }

            if (comments == null || string.IsNullOrEmpty(settings.MarkTag))
            {
                return false;
            }

            foreach (string body in comments)
            {
                if (body != null && body.IndexOf(settings.MarkTag, StringComparison.Ordinal) >= 0)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>Whose a view or folder is, from the folders it sits in now, outermost first, and what it carries.</summary>
        public static MarkJudgement Judge(
            IList<string> folders,
            string name,
            Point3 camera,
            IList<string> comments,
            int? redlines,
            string guid,
            ViewpointSettings settings)
        {
            if (settings == null)
            {
                throw new ArgumentNullException("settings");
            }

            ToolViewMark mark = null;
            bool carriesTheTag = false;

            if (comments != null)
            {
                foreach (string body in comments)
                {
                    if (body != null && !string.IsNullOrEmpty(settings.MarkTag)
                        && body.IndexOf(settings.MarkTag, StringComparison.Ordinal) >= 0)
                    {
                        carriesTheTag = true;
                    }

                    ToolViewMark read = Read(body, settings);

                    if (read != null && mark == null)
                    {
                        mark = read;
                    }
                }
            }

            if (mark == null)
            {
                return new MarkJudgement(
                    ViewOwner.NotOurs,
                    carriesTheTag ? "it carries a mark that does not read" : "it carries no mark of this tool",
                    null);
            }

            string why = Changed(mark, ViewPlace.FolderPath(folders), name, camera, comments.Count, redlines, guid, settings);

            return why == null
                ? new MarkJudgement(ViewOwner.Ours, "made by this tool and unchanged", mark)
                : new MarkJudgement(ViewOwner.ChangedByAPerson, why, mark);
        }

        private static string Changed(
            ToolViewMark mark, string folderPath, string name, Point3 camera, int comments,
            int? redlines, string guid, ViewpointSettings settings)
        {
            if (comments != 1)
            {
                return "it carries another comment beside the mark";
            }

            if (!string.Equals(mark.FolderPath, folderPath ?? string.Empty, StringComparison.Ordinal))
            {
                return "it was written in " + mark.FolderPath + " and sits in " + folderPath;
            }

            if (!string.Equals(mark.Name, name ?? string.Empty, StringComparison.Ordinal))
            {
                return "it was written as " + mark.Name + " and is now named " + name;
            }

            if (mark.Camera == null && camera != null)
            {
                return "it was written with no camera and now has one";
            }

            if (mark.Camera != null && camera == null)
            {
                return "its camera could not be read, so it is not proved unchanged";
            }

            // BOTH ROUNDED TO THE THREE DECIMALS THE MARK STORES, the breaker's B6 of F114's
            // add-in pass: the mark's camera was written through Number, so the read camera
            // goes through Number too before the distance is taken, and rounding alone can
            // never read as moved, however far from the origin the camera sits.
            if (mark.Camera != null && mark.Camera.DistanceTo(Rounded(camera)) > settings.CameraReadBackTolerance)
            {
                return "its camera was moved";
            }

            if (mark.Guid != null && guid == null)
            {
                return "its Guid could not be read, so it is not proved unchanged";
            }

            if (mark.Guid != null && !string.Equals(mark.Guid, guid, StringComparison.Ordinal))
            {
                return "its Guid is not the one written, so it is a copy";
            }

            if (!redlines.HasValue)
            {
                return "its redlines could not be read, so it is not proved unchanged";
            }

            if (redlines.Value > 0)
            {
                return "it carries " + redlines.Value + " redlines";
            }

            return null;
        }

        private static string Counted(string text)
        {
            string value = text ?? string.Empty;
            return value.Length.ToString(CultureInfo.InvariantCulture) + ":" + value;
        }

        private static string Number(double value)
        {
            return value.ToString(CameraFormat, CultureInfo.InvariantCulture);
        }

        /// <summary>The point as the mark would store it, each part through Number and read back.</summary>
        private static Point3 Rounded(Point3 point)
        {
            return new Point3(
                double.Parse(Number(point.X), CultureInfo.InvariantCulture),
                double.Parse(Number(point.Y), CultureInfo.InvariantCulture),
                double.Parse(Number(point.Z), CultureInfo.InvariantCulture));
        }

        private static bool Field(string body, ref int at, string field)
        {
            if (string.CompareOrdinal(body, at, field, 0, field.Length) != 0 || at + field.Length > body.Length)
            {
                return false;
            }

            at += field.Length;
            return true;
        }

        private static bool CountedAt(string body, ref int at, out string value)
        {
            value = null;
            int colon = body.IndexOf(':', at);
            int length;

            if (colon <= at
                || !int.TryParse(body.Substring(at, colon - at), NumberStyles.None, CultureInfo.InvariantCulture, out length)
                || colon + 1 + length > body.Length)
            {
                return false;
            }

            value = body.Substring(colon + 1, length);
            at = colon + 1 + length;
            return true;
        }

        private static bool CameraIn(string text, out Point3 camera)
        {
            camera = null;

            if (text == NoCamera)
            {
                return true;
            }

            string[] parts = text.Split(',');
            double x, y, z;

            if (parts.Length != 3
                || !double.TryParse(parts[0], NumberStyles.Float, CultureInfo.InvariantCulture, out x)
                || !double.TryParse(parts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out y)
                || !double.TryParse(parts[2], NumberStyles.Float, CultureInfo.InvariantCulture, out z))
            {
                return false;
            }

            camera = new Point3(x, y, z);
            return true;
        }
    }
}
