using System;
using System.Collections.Generic;
using Federator.Core.Units;

namespace Federator.Core.Views
{
    /// <summary>
    /// The box a view of several clashes is framed on, F114, Q114 point 13: the camera framed
    /// around the test's open clashes all at once.
    ///
    /// The box over every open clash centre, padded on every side by the margin, a setting in
    /// millimetres converted to the document's units through UnitTable, the one unit table, so
    /// a feet document and a metres one frame the same place. A unit the table does not know is
    /// refused rather than guessed, F33's rule. A view of one open clash gets no box and keeps
    /// Clash Detective's own camera for that clash, which 5l measured beside its items. The
    /// add-in builds a BoundingBox3D from the two corners, P5 measured that constructor on
    /// 2026-10-05, and whether ZoomBox frames it is probe P16's to say.
    /// </summary>
    public sealed class FramingBox
    {
        private FramingBox(Point3 min, Point3 max)
        {
            Min = min;
            Max = max;
        }

        /// <summary>The low corner, in document units.</summary>
        public Point3 Min { get; private set; }

        /// <summary>The high corner, in document units.</summary>
        public Point3 Max { get; private set; }

        /// <summary>The box over those clash centres padded by the margin, or null where there is none to frame.</summary>
        public static FramingBox For(IEnumerable<Point3> centres, double marginMillimetres, string unitEnumName)
        {
            if (marginMillimetres < 0.0)
            {
                throw new ArgumentOutOfRangeException("marginMillimetres", marginMillimetres, "A framing margin is never below zero.");
            }

            UnitRow unit = UnitTable.ByEnumName(unitEnumName);
            List<Point3> points = new List<Point3>();

            if (centres != null)
            {
                foreach (Point3 centre in centres)
                {
                    if (centre != null)
                    {
                        points.Add(centre);
                    }
                }
            }

            if (points.Count < 2)
            {
                return null;
            }

            double margin = marginMillimetres / unit.MillimetresPerUnit;
            double minX = points[0].X, minY = points[0].Y, minZ = points[0].Z;
            double maxX = minX, maxY = minY, maxZ = minZ;

            foreach (Point3 point in points)
            {
                minX = Math.Min(minX, point.X);
                minY = Math.Min(minY, point.Y);
                minZ = Math.Min(minZ, point.Z);
                maxX = Math.Max(maxX, point.X);
                maxY = Math.Max(maxY, point.Y);
                maxZ = Math.Max(maxZ, point.Z);
            }

            return new FramingBox(
                new Point3(minX - margin, minY - margin, minZ - margin),
                new Point3(maxX + margin, maxY + margin, maxZ + margin));
        }
    }
}
