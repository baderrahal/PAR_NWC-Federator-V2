using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Xml;
using System.Xml.Linq;

namespace Federator.Core.Exchange
{
    /// <summary>
    /// Reads a Navisworks exchange XML. One reader handles all three shapes: tests
    /// only, sets only, or both in one file.
    /// </summary>
    public sealed class ExchangeReader
    {
        /// <summary>The root of a set path, the same prefix a test locator is written with.</summary>
        public const string SelectionSetTreeRoot = "lcop_selection_set_tree";

        public ExchangeReader()
        {
            SetTreeRoot = SelectionSetTreeRoot;
        }

        /// <summary>The prefix every set path is built with. A setting, not a constant.</summary>
        public string SetTreeRoot { get; set; }

        public ExchangeDocument ReadFile(string path)
        {
            if (path == null)
            {
                throw new ArgumentNullException("path");
            }

            if (!File.Exists(path))
            {
                throw new FileNotFoundException("Exchange file not found.", path);
            }

            XDocument document;

            using (XmlReader reader = XmlReader.Create(path, SafeSettings()))
            {
                document = XDocument.Load(reader);
            }

            return Read(document, path);
        }

        internal ExchangeDocument ReadText(string xml)
        {
            if (xml == null)
            {
                throw new ArgumentNullException("xml");
            }

            using (StringReader text = new StringReader(xml))
            using (XmlReader reader = XmlReader.Create(text, SafeSettings()))
            {
                return Read(XDocument.Load(reader), null);
            }
        }

        private ExchangeDocument Read(XDocument document, string sourcePath)
        {
            XElement root = document.Root;

            if (root == null)
            {
                throw new InvalidDataException("The exchange file held no root element.");
            }

            if (!string.Equals(root.Name.LocalName, "exchange", StringComparison.Ordinal))
            {
                throw new InvalidDataException(
                    "Expected a root element named exchange, found " + root.Name.LocalName + ".");
            }

            string fileUnits = Attribute(root, "units");

            List<BatchTestDefinition> batches = new List<BatchTestDefinition>();
            List<ClashTestDefinition> tests = new List<ClashTestDefinition>();

            foreach (XElement batch in Descendants(root, "batchtest"))
            {
                string batchUnits = Attribute(batch, "units");
                string unitsForBatch = string.IsNullOrEmpty(batchUnits) ? fileUnits : batchUnits;

                int countBefore = tests.Count;

                foreach (XElement test in Descendants(batch, "clashtest"))
                {
                    tests.Add(ReadTest(test, unitsForBatch));
                }

                batches.Add(new BatchTestDefinition(
                    Attribute(batch, "name"),
                    Attribute(batch, "internal_name"),
                    unitsForBatch,
                    tests.Count - countBefore));
            }

            // A clashtest can also sit outside a batchtest. Pick up anything not already read.
            foreach (XElement test in Descendants(root, "clashtest"))
            {
                if (Ancestor(test, "batchtest") == null)
                {
                    tests.Add(ReadTest(test, fileUnits));
                }
            }

            List<SelectionSetDefinition> sets = new List<SelectionSetDefinition>();

            foreach (XElement setsRoot in Descendants(root, "selectionsets"))
            {
                ReadSets(setsRoot, new List<string>(), sets);
            }

            return new ExchangeDocument(sourcePath, fileUnits, batches, tests, sets);
        }

        private ClashTestDefinition ReadTest(XElement test, string units)
        {
            string name = Attribute(test, "name");
            string toleranceText = Attribute(test, "tolerance");
            double tolerance = ReadDouble(toleranceText, "tolerance", name);

            // Read as written, in the file units, and never converted here. A unit the
            // tool does not know is judged once, in ClashTestPlan.Convert, where the test
            // is skipped by name. Converting here threw on the whole file instead.
            XElement linkage = Child(test, "linkage");

            return new ClashTestDefinition(
                name,
                Attribute(test, "test_type"),
                Attribute(test, "status"),
                tolerance,
                units,
                ReadFlag(Attribute(test, "merge_composites")),
                linkage == null ? null : Attribute(linkage, "mode"),
                ReadRules(test),
                ReadSide(test, "left"),
                ReadSide(test, "right"));
        }

        private static IList<string> ReadRules(XElement test)
        {
            List<string> rules = new List<string>();
            XElement container = Child(test, "rules");

            if (container == null)
            {
                return rules;
            }

            foreach (XElement rule in container.Elements())
            {
                string ruleName = Attribute(rule, "name");
                rules.Add(string.IsNullOrEmpty(ruleName) ? rule.Value.Trim() : ruleName);
            }

            return rules;
        }

        private static ClashSideDefinition ReadSide(XElement test, string sideName)
        {
            XElement side = Child(test, sideName);

            if (side == null)
            {
                return null;
            }

            XElement selection = Child(side, "clashselection");

            if (selection == null)
            {
                return null;
            }

            XElement locator = Child(selection, "locator");

            return new ClashSideDefinition(
                ReadFlag(Attribute(selection, "selfintersect")),
                ReadInt(Attribute(selection, "primtypes")),
                locator == null ? null : locator.Value);
        }

        private void ReadSets(XElement container, List<string> folders, List<SelectionSetDefinition> sets)
        {
            foreach (XElement child in container.Elements())
            {
                string local = child.Name.LocalName;

                if (string.Equals(local, "viewfolder", StringComparison.Ordinal))
                {
                    folders.Add(Attribute(child, "name"));
                    ReadSets(child, folders, sets);
                    folders.RemoveAt(folders.Count - 1);
                    continue;
                }

                if (string.Equals(local, "selectionset", StringComparison.Ordinal))
                {
                    sets.Add(ReadSet(child, folders));
                    continue;
                }

                if (string.Equals(local, "selectionsetgroup", StringComparison.Ordinal))
                {
                    sets.Add(ReadSet(child, folders));
                    folders.Add(Attribute(child, "name"));
                    ReadSets(child, folders, sets);
                    folders.RemoveAt(folders.Count - 1);
                }
            }
        }

        private SelectionSetDefinition ReadSet(XElement set, List<string> folders)
        {
            string name = Attribute(set, "name");
            XElement findSpec = Child(set, "findspec");

            string mode = null;
            bool disjoint = false;
            string findSpecLocator = null;
            List<SearchConditionDefinition> conditions = new List<SearchConditionDefinition>();

            if (findSpec != null)
            {
                mode = Attribute(findSpec, "mode");
                disjoint = ReadFlag(Attribute(findSpec, "disjoint"));

                XElement locator = Child(findSpec, "locator");
                findSpecLocator = locator == null ? null : locator.Value;

                XElement conditionContainer = Child(findSpec, "conditions");

                if (conditionContainer != null)
                {
                    foreach (XElement condition in Descendants(conditionContainer, "condition"))
                    {
                        conditions.Add(ReadCondition(condition));
                    }
                }
            }

            return new SelectionSetDefinition(
                name,
                new List<string>(folders),
                BuildPath(folders, name),
                mode,
                disjoint,
                findSpecLocator,
                conditions);
        }

        private static SearchConditionDefinition ReadCondition(XElement condition)
        {
            return new SearchConditionDefinition(
                Attribute(condition, "test"),
                ReadInt(Attribute(condition, "flags")),
                ReadNameReference(Child(condition, "category")),
                ReadNameReference(Child(condition, "property")),
                ReadValueReference(Child(condition, "value")));
        }

        private static NameReference ReadNameReference(XElement holder)
        {
            if (holder == null)
            {
                return null;
            }

            XElement name = Child(holder, "name");

            if (name == null)
            {
                return null;
            }

            return new NameReference(Attribute(name, "internal"), name.Value);
        }

        private static ValueReference ReadValueReference(XElement holder)
        {
            if (holder == null)
            {
                return null;
            }

            XElement data = Child(holder, "data");

            if (data == null)
            {
                return null;
            }

            return new ValueReference(Attribute(data, "type"), data.Value);
        }

        /// <summary>
        /// Builds lcop_selection_set_tree/folder/folder/name so the result can be compared
        /// to a test locator with an ordinary string comparison.
        /// </summary>
        public string BuildPath(IList<string> folders, string name)
        {
            List<string> parts = new List<string> { SetTreeRoot };

            if (folders != null)
            {
                parts.AddRange(folders);
            }

            parts.Add(name);
            return string.Join("/", parts.ToArray());
        }

        private static XmlReaderSettings SafeSettings()
        {
            return new XmlReaderSettings
            {
                DtdProcessing = DtdProcessing.Prohibit,
                XmlResolver = null,
                IgnoreComments = true,
                IgnoreProcessingInstructions = true
            };
        }

        private static IEnumerable<XElement> Descendants(XElement parent, string localName)
        {
            foreach (XElement element in parent.Descendants())
            {
                if (string.Equals(element.Name.LocalName, localName, StringComparison.Ordinal))
                {
                    yield return element;
                }
            }
        }

        private static XElement Child(XElement parent, string localName)
        {
            foreach (XElement element in parent.Elements())
            {
                if (string.Equals(element.Name.LocalName, localName, StringComparison.Ordinal))
                {
                    return element;
                }
            }

            return null;
        }

        private static XElement Ancestor(XElement element, string localName)
        {
            foreach (XElement candidate in element.Ancestors())
            {
                if (string.Equals(candidate.Name.LocalName, localName, StringComparison.Ordinal))
                {
                    return candidate;
                }
            }

            return null;
        }

        private static string Attribute(XElement element, string name)
        {
            XAttribute attribute = element.Attribute(name);
            return attribute == null ? null : attribute.Value;
        }

        private static bool ReadFlag(string text)
        {
            return ReadInt(text) != 0;
        }

        private static int ReadInt(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return 0;
            }

            int value;

            return int.TryParse(text.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out value)
                ? value
                : 0;
        }

        private static double ReadDouble(string text, string attributeName, string testName)
        {
            if (string.IsNullOrEmpty(text))
            {
                return 0.0;
            }

            double value;

            if (!double.TryParse(
                    text.Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out value))
            {
                throw new InvalidDataException(
                    "Could not read the " + attributeName + " attribute \"" + text
                        + "\" on clash test \"" + testName + "\".");
            }

            return value;
        }
    }
}
