using System;

namespace Federator.Core.Views
{
    /// <summary>
    /// A point in the document's own units, three doubles, F114. A clash centre or a camera
    /// position as the add-in read it, carried into Core with no Navisworks type.
    /// </summary>
    public sealed class Point3
    {
        public Point3(double x, double y, double z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public double X { get; private set; }

        public double Y { get; private set; }

        public double Z { get; private set; }

        /// <summary>The straight distance to another point, in the same units.</summary>
        public double DistanceTo(Point3 other)
        {
            if (other == null)
            {
                throw new ArgumentNullException("other");
            }

            double dx = X - other.X;
            double dy = Y - other.Y;
            double dz = Z - other.Z;

            return Math.Sqrt((dx * dx) + (dy * dy) + (dz * dz));
        }
    }
}
