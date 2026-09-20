using System;
using System.Collections.Generic;
using Autodesk.Navisworks.Api;
using Federator.Core.Units;
using Federator.Core.Views;

namespace Federator.Addin.Engine
{
    /// <summary>
    /// Reads the size properties off one model item and hands back what it found, in the
    /// document's units, with no judgement of any kind.
    ///
    /// The add-in reads properties. Core decides. Whether a thing is big enough for a
    /// viewpoint is Federator.Core.Views.SizeRule and nothing here has an opinion about
    /// 150 or about any other number, which is why the threshold can be changed and tested
    /// without Navisworks anywhere near it.
    ///
    /// A VALUE IS READ BY ITS KIND, never with ToDisplayString and never with a cast.
    /// Every accessor on VariantData is kind specific and throws on any other kind, which
    /// is what once threw 426 times on one run and took three report columns with it. The
    /// five double kinds all go through ToAnyDouble, which is what its name says and what
    /// section 4n measured. Anything that is not one of those five is not a size, so it is
    /// left out rather than coerced, and an item with no readable size comes back empty,
    /// which SizeRule treats as INCLUDE.
    /// </summary>
    public static class ItemSizes
    {
        /// <summary>
        /// The wanted properties that are on this item, by name, in the document's units.
        /// Never throws: anything that cannot be read is left out, and an empty lookup is a
        /// real answer meaning no size was found.
        /// </summary>
        public static IDictionary<string, double> Read(ModelItem item, SizeSettings settings)
        {
            return Read(item, settings, null);
        }

        /// <summary>
        /// The same, told which unit the document measures in, so a size written as WORDS
        /// can be put back into those units. Without it a worded size is left out, which
        /// is what this reader did for every one of them until 5s.
        /// </summary>
        public static IDictionary<string, double> Read(ModelItem item, SizeSettings settings, string unitEnumName)
        {
            Dictionary<string, double> found = new Dictionary<string, double>(StringComparer.Ordinal);

            if (item == null || settings == null || settings.PropertyNames == null)
            {
                return found;
            }

            PropertyCategoryCollection categories = item.PropertyCategories;

            if (categories == null)
            {
                return found;
            }

            for (int i = 0; i < settings.PropertyNames.Count; i++)
            {
                string wanted = settings.PropertyNames[i];

                if (string.IsNullOrEmpty(wanted) || found.ContainsKey(wanted))
                {
                    continue;
                }

                double value;

                if (TryReadOne(categories, wanted, MillimetresPerUnit(unitEnumName), out value))
                {
                    found.Add(wanted, value);
                }
            }

            return found;
        }

        /// <summary>
        /// One named property as a number, or false where it is not there or is not a kind
        /// that carries a length. Matched the way the existing reader matches, case blind on
        /// the display name, because an exporter writing Diameter and another writing
        /// DIAMETER are the same property to anyone reading the model.
        /// </summary>
        private static bool TryReadOne(
            PropertyCategoryCollection categories, string wanted, double millimetresPerUnit, out double value)
        {
            value = 0.0;

            foreach (PropertyCategory category in categories)
            {
                foreach (DataProperty property in category.Properties)
                {
                    if (!string.Equals(property.DisplayName, wanted, StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    // Its own try, because one property that throws must not cost the ones
                    // after it. That rule was written after a single throw lost three
                    // columns of every row on one run.
                    try
                    {
                        using (VariantData data = property.Value)
                        {
                            if (IsALength(data.DataType))
                            {
                                value = data.ToAnyDouble();
                                return true;
                            }

                            // A SIZE WRITTEN AS WORDS IS STILL A SIZE, 5s. Revit writes a
                            // conduit's Size as the DisplayString "53 mmø" and a cable
                            // tray fitting's as "600 mmx100 mm", and taking only the two
                            // numeric kinds left 29 of 74 services in one group reported
                            // as having no readable size while every one of them carried
                            // one. The text names its own unit, so Federator.Core.Views
                            // .SizeText reads it through UnitTable and refuses a number
                            // with no unit rather than guessing at one.
                            if (data.DataType == VariantDataType.DisplayString)
                            {
                                double? millimetres = SizeText.LargestMillimetres(data.ToDisplayString());

                                if (millimetres.HasValue && millimetresPerUnit > 0.0)
                                {
                                    // Put back into DOCUMENT units, because every other
                                    // value in this dictionary is in them and SizeRule
                                    // stays the one place that converts to millimetres.
                                    value = millimetres.Value / millimetresPerUnit;
                                    return true;
                                }
                            }
                        }
                    }
                    catch (Exception)
                    {
                        // Left out rather than guessed at. SizeRule takes a missing size as
                        // a reason to INCLUDE the item, so nothing is lost by failing here.
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// The kinds ToAnyDouble covers, measured and recorded in docs\history\scan.md
        /// section 4n. A size written as a display string is NOT read, because parsing
        /// "150 mm" would mean guessing at the unit written in it while the number this
        /// tool converts is in the document's units.
        /// </summary>
        /// <summary>How many millimetres one document unit is, or zero where the unit is not known.</summary>
        private static double MillimetresPerUnit(string unitEnumName)
        {
            if (string.IsNullOrEmpty(unitEnumName))
            {
                return 0.0;
            }

            UnitRow row = UnitTable.FindByEnumName(unitEnumName);
            return row == null ? 0.0 : row.MillimetresPerUnit;
        }

        private static bool IsALength(VariantDataType kind)
        {
            return kind == VariantDataType.DoubleLength
                || kind == VariantDataType.Double;
        }
    }
}
