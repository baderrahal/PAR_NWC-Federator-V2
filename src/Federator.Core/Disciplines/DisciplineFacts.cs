using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Federator.Core.Disciplines
{
    /// <summary>
    /// One element the add-in read out of a model for the discipline reports, F209: a CAD file imported or linked into the
    /// Revit model, or an element whose Revit category is Generic Models. Plain strings, so Core holds no Navisworks type.
    /// A value not read is null, and the report writes the setting's words for it, never an empty cell.
    /// </summary>
    public sealed class ElementFact
    {
        public ElementFact(string elementId, string name, string family, string type, string level)
        {
            ElementId = Blank(elementId);
            Name = Blank(name);
            Family = Blank(family);
            Type = Blank(type);
            Level = Blank(level);
        }

        /// <summary>The Revit element id, or null where it could not be read.</summary>
        public string ElementId { get; private set; }

        /// <summary>For a CAD element the CAD file's name, for a Generic Models element its own name, or null.</summary>
        public string Name { get; private set; }

        public string Family { get; private set; }

        public string Type { get; private set; }

        public string Level { get; private set; }

        private static string Blank(string value)
        {
            return string.IsNullOrEmpty(value) || value.Trim().Length == 0 ? null : value;
        }
    }

    /// <summary>
    /// What the add-in read off one model of a group for the discipline reports, F209: its file, the translation of its
    /// transform in millimetres as the ALIGNMENT block reads it, F234, the shared site it names, and the CAD and Generic Models
    /// elements found in it. Each part that could not be read carries why and is never read as a zero or as none found.
    /// </summary>
    public sealed class ModelFacts
    {
        private readonly List<ElementFact> cad = new List<ElementFact>();
        private readonly List<ElementFact> generic = new List<ElementFact>();

        public ModelFacts(string file)
        {
            File = file ?? string.Empty;
            PlacementNotRead = "UNKNOWN, the placement was not handed in";
            SiteNotRead = "UNKNOWN, the site was not handed in";
            CadNotRead = "UNKNOWN, the walk was not handed in";
            GenericNotRead = "UNKNOWN, the walk was not handed in";
        }

        /// <summary>The model's file, the NWC, a full path or a name.</summary>
        public string File { get; private set; }

        /// <summary>The translation of the model's transform in X, in millimetres, ModelPlacement's numbers.</summary>
        public double X { get; private set; }

        public double Y { get; private set; }

        public double Z { get; private set; }

        /// <summary>Why the placement was not read, or null where it was.</summary>
        public string PlacementNotRead { get; private set; }

        public bool IsPlaced
        {
            get { return PlacementNotRead == null; }
        }

        /// <summary>The shared site the model names, Revit's Project Location. Null with SiteNotRead where not read.</summary>
        public string Site { get; private set; }

        public string SiteNotRead { get; private set; }

        public IList<ElementFact> Cad
        {
            get { return cad.AsReadOnly(); }
        }

        public IList<ElementFact> Generic
        {
            get { return generic.AsReadOnly(); }
        }

        /// <summary>Why the model's items were not walked for CAD, or null where they were.</summary>
        public string CadNotRead { get; private set; }

        public string GenericNotRead { get; private set; }

        /// <summary>The translation in millimetres, each a number, so a placement not read is never three noughts.</summary>
        public ModelFacts WithPlacement(double x, double y, double z)
        {
            if (double.IsNaN(x) || double.IsNaN(y) || double.IsNaN(z) || double.IsInfinity(x) || double.IsInfinity(y) || double.IsInfinity(z))
            {
                return PlacementNot("its transform gave no number");
            }

            X = x;
            Y = y;
            Z = z;
            PlacementNotRead = null;
            return this;
        }

        public ModelFacts PlacementNot(string why)
        {
            X = 0.0;
            Y = 0.0;
            Z = 0.0;
            PlacementNotRead = string.IsNullOrEmpty(why) ? "UNKNOWN why" : why;
            return this;
        }

        public ModelFacts WithSite(string site)
        {
            Site = site ?? string.Empty;
            SiteNotRead = null;
            return this;
        }

        public ModelFacts SiteNot(string why)
        {
            Site = null;
            SiteNotRead = string.IsNullOrEmpty(why) ? "UNKNOWN why" : why;
            return this;
        }

        /// <summary>The CAD elements found, an empty list a real answer, read whole.</summary>
        public ModelFacts WithCad(IEnumerable<ElementFact> found)
        {
            cad.Clear();
            cad.AddRange(found ?? new List<ElementFact>());
            CadNotRead = null;
            return this;
        }

        public ModelFacts CadNot(string why)
        {
            cad.Clear();
            CadNotRead = string.IsNullOrEmpty(why) ? "UNKNOWN why" : why;
            return this;
        }

        public ModelFacts WithGeneric(IEnumerable<ElementFact> found)
        {
            generic.Clear();
            generic.AddRange(found ?? new List<ElementFact>());
            GenericNotRead = null;
            return this;
        }

        public ModelFacts GenericNot(string why)
        {
            generic.Clear();
            GenericNotRead = string.IsNullOrEmpty(why) ? "UNKNOWN why" : why;
            return this;
        }
    }

    /// <summary>
    /// One group of a package for the discipline reports, F209: its name, every NWC file it holds, and the facts read off
    /// its models where its run read them. A group that did not run in this run, or whose models were not read, has its
    /// files and no facts, with why, so the checks of the file names still run on it and the others say not read.
    /// </summary>
    public sealed class DisciplineGroup
    {
        public DisciplineGroup(string name, IEnumerable<string> files, IEnumerable<ModelFacts> models, string whyNoModels)
        {
            Name = name ?? string.Empty;
            Files = new ReadOnlyCollection<string>(new List<string>(files ?? new List<string>()));
            Models = models == null ? null : new ReadOnlyCollection<ModelFacts>(new List<ModelFacts>(models));
            WhyNoModels = Models == null ? (string.IsNullOrEmpty(whyNoModels) ? "UNKNOWN why" : whyNoModels) : null;
        }

        public string Name { get; private set; }

        public ReadOnlyCollection<string> Files { get; private set; }

        /// <summary>The facts of its models, or null where none were read, with WhyNoModels.</summary>
        public ReadOnlyCollection<ModelFacts> Models { get; private set; }

        public string WhyNoModels { get; private set; }
    }

    /// <summary>One NWC file's date as the disk gave it, or null with why where it could not be read.</summary>
    public sealed class FileDate
    {
        public FileDate(string file, DateTime? written, string whyNotRead)
        {
            File = file ?? string.Empty;
            Written = written;
            WhyNotRead = written.HasValue ? null : (string.IsNullOrEmpty(whyNotRead) ? "UNKNOWN why" : whyNotRead);
        }

        public string File { get; private set; }

        public DateTime? Written { get; private set; }

        public string WhyNotRead { get; private set; }
    }
}
