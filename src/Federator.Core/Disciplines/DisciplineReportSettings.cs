using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Federator.Core.Generic;

namespace Federator.Core.Disciplines
{
    /// <summary>
    /// Every number and word that shapes the discipline reports, F209, a setting each and never a constant, as CLAUDE.md
    /// asks. Each is refused where it is set if it cannot work.
    /// </summary>
    public sealed class DisciplineReportSettings
    {
        /// <summary>The disciplines that get a workbook of their own, Bader's four, in his order.</summary>
        public static readonly ReadOnlyCollection<string> DefaultDisciplines = new ReadOnlyCollection<string>(new[] { "AR", "ST", "ME", "EL" });

        /// <summary>The discipline whose names the naming check learns from and whose models the others are measured from.</summary>
        public const string DefaultReference = "AR";

        /// <summary>A file older than this many days is flagged, Bader's 7.</summary>
        public const int DefaultOlderThanDays = 7;

        /// <summary>
        /// How far apart two boxes may sit before a discipline is far from the reference, in metres. A choice and not a
        /// measurement: the gap between two boxes, which is nought where they overlap, and the C02 case is 2,823 km.
        /// </summary>
        public const double DefaultFarMetres = 1000.0;

        /// <summary>The words in a cell whose value could not be read, Bader's words for an element id.</summary>
        public const string DefaultNotReadable = "not readable";

        /// <summary>The words after the package and the discipline in a workbook's name.</summary>
        public const string DefaultWorkbookSuffix = "checks";

        private ReadOnlyCollection<string> disciplines = DefaultDisciplines;
        private string reference = DefaultReference;
        private int olderThanDays = DefaultOlderThanDays;
        private double farMetres = DefaultFarMetres;
        private string notReadable = DefaultNotReadable;
        private string workbookSuffix = DefaultWorkbookSuffix;
        private string genericCategory = GenericModelsSettings.DefaultCategoryValue;

        public IList<string> Disciplines
        {
            get { return disciplines; }
            set
            {
                if (value == null || value.Count == 0)
                {
                    throw new ArgumentException("At least one discipline gets a workbook.", "value");
                }

                foreach (string code in value)
                {
                    if (string.IsNullOrEmpty(code) || code.Trim().Length != code.Length)
                    {
                        throw new ArgumentException("A discipline code is empty or has a space at its end.", "value");
                    }
                }

                disciplines = new ReadOnlyCollection<string>(new List<string>(value));
            }
        }

        public string Reference
        {
            get { return reference; }
            set
            {
                if (string.IsNullOrEmpty(value) || value.Trim().Length != value.Length)
                {
                    throw new ArgumentException("The reference discipline is empty or has a space at its end.", "value");
                }

                reference = value;
            }
        }

        public int OlderThanDays
        {
            get { return olderThanDays; }
            set
            {
                if (value < 0)
                {
                    throw new ArgumentOutOfRangeException("value", "A file cannot be older than fewer than no days.");
                }

                olderThanDays = value;
            }
        }

        public double FarMetres
        {
            get { return farMetres; }
            set
            {
                if (double.IsNaN(value) || double.IsInfinity(value) || value < 0.0)
                {
                    throw new ArgumentOutOfRangeException("value", "A distance in metres is a number at or above nought.");
                }

                farMetres = value;
            }
        }

        public string NotReadable
        {
            get { return notReadable; }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    throw new ArgumentException("The words for a value not read cannot be empty, an empty cell reads as nothing found.", "value");
                }

                notReadable = value;
            }
        }

        public string WorkbookSuffix
        {
            get { return workbookSuffix; }
            set
            {
                if (string.IsNullOrEmpty(value) || value.IndexOfAny(System.IO.Path.GetInvalidFileNameChars()) >= 0)
                {
                    throw new ArgumentException("The workbook suffix must be words a file name can hold.", "value");
                }

                workbookSuffix = value;
            }
        }

        /// <summary>The Revit category the Generic Models check lists, the value F128's sets ask, measured in scan.md 5z-zb.</summary>
        public string GenericCategory
        {
            get { return genericCategory; }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    throw new ArgumentException("The Generic Models category cannot be empty.", "value");
                }

                genericCategory = value;
            }
        }
    }
}
