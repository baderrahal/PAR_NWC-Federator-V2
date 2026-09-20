using System;
using System.Globalization;

namespace Federator.Core.Views
{
    /// <summary>
    /// A colour a viewpoint paints one clashing item, as three parts from 0 to 1, F85
    /// and Q58. Core holds no Navisworks type, so the add-in turns one of these into the
    /// API's own Color at the one place it calls the API and nowhere else.
    ///
    /// WHY THREE DOUBLES AND NOT A NAME. Red and green are the defaults because that is
    /// what Clash Detective paints, and they are SETTINGS because which colours a
    /// viewpoint carries is a judgement about what the client's people read, not
    /// something this code decides for them. A name would be a list this tool would have
    /// to keep, and a person who wants a softer red can write one here without one.
    /// </summary>
    public sealed class ViewpointColour
    {
        /// <summary>
        /// How far two colours may sit apart and still be called the same when a
        /// viewpoint is read back.
        ///
        /// A hundredth, because the file stores a colour as a byte per part, a 255th,
        /// and a round trip through the NWF and back can land one step either side.
        /// Anything a person could see is far larger than this and anything this small
        /// is the file and not the tool.
        /// </summary>
        public const double SameTolerance = 0.01;

        /// <summary>What the first clashing item is painted, the way Clash Detective paints it.</summary>
        public static ViewpointColour DefaultFirst()
        {
            return new ViewpointColour(1.0, 0.0, 0.0);
        }

        /// <summary>What the second clashing item is painted, the way Clash Detective paints it.</summary>
        public static ViewpointColour DefaultSecond()
        {
            return new ViewpointColour(0.0, 1.0, 0.0);
        }

        /// <summary>
        /// Three parts from 0 to 1. A part outside that range is refused rather than
        /// clamped, because a typo that silently becomes a different colour is the kind
        /// of thing nobody notices until a person asks why the clash is orange.
        /// </summary>
        public ViewpointColour(double red, double green, double blue)
        {
            Refuse(red, "red");
            Refuse(green, "green");
            Refuse(blue, "blue");

            Red = red;
            Green = green;
            Blue = blue;
        }

        private static void Refuse(double part, string which)
        {
            if (part < 0.0 || part > 1.0 || double.IsNaN(part))
            {
                throw new ArgumentOutOfRangeException(
                    which,
                    part,
                    "A colour part runs from 0 to 1 and \"" + which + "\" was "
                    + part.ToString("0.###", CultureInfo.InvariantCulture) + ".");
            }
        }

        /// <summary>The red part, 0 to 1.</summary>
        public double Red { get; private set; }

        /// <summary>The green part, 0 to 1.</summary>
        public double Green { get; private set; }

        /// <summary>The blue part, 0 to 1.</summary>
        public double Blue { get; private set; }

        /// <summary>
        /// Whether that colour is this one to within the tolerance a file round trip
        /// costs. Null is never the same as a colour, because not knowing what a
        /// viewpoint shows is not the same as knowing it shows the right thing.
        /// </summary>
        public bool Same(double red, double green, double blue)
        {
            return Math.Abs(Red - red) <= SameTolerance
                && Math.Abs(Green - green) <= SameTolerance
                && Math.Abs(Blue - blue) <= SameTolerance;
        }

        public override string ToString()
        {
            return "("
                + Red.ToString("0.###", CultureInfo.InvariantCulture) + ", "
                + Green.ToString("0.###", CultureInfo.InvariantCulture) + ", "
                + Blue.ToString("0.###", CultureInfo.InvariantCulture) + ")";
        }
    }
}
