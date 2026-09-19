using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using Autodesk.Navisworks.Api;
using Federator.Core.Diagnostics;
using Federator.Core.Probe;

namespace Federator.Addin.Engine
{
    /// <summary>
    /// The property probe, F86. Walks every item of one model, keeps the ones whose
    /// category is one the settings ask for, and writes every property tab, property name
    /// and distinct value they carry into one CSV beside the model's file, so the
    /// mechanical sets can be rewritten against what the models actually hold rather than
    /// against what a set name implies.
    ///
    /// IT READS AND CHANGES NOTHING. Nothing here saves, publishes or touches a set, a
    /// test or a viewpoint. Which files it may read at all is Federator.Core.Probe
    /// .ProbeSettings.MayRead, and the counting, the cap, the order, the CSV and the
    /// verdict are all Core, so the only thing here is the reading.
    ///
    /// THE CATEGORY IS READ THE WAY THE PENETRATION RULE READS IT, through the one reader
    /// ClashHarvest already has over the same property names, and whether it is one of
    /// the seventeen is the settings' own answer, so the probe finds exactly the items the
    /// rule would judge. A value is read by its KIND through the harvest's one reader,
    /// because ToDisplayString throws on anything that is not a display string, which is
    /// the fault that once lost three columns on every row. scan.md 5f.
    ///
    /// THE WALK IS PER ITEM, Model.RootItem.DescendantsAndSelf, and what it costs is
    /// measured and said per model, because the number is what decides whether a search
    /// per category would be worth writing instead.
    /// </summary>
    public sealed class PropertyProbe
    {
        private readonly RunLog log;
        private readonly ProbeSettings settings;
        private readonly List<string> lines = new List<string>();

        public PropertyProbe(RunLog log, ProbeSettings settings)
        {
            if (log == null)
            {
                throw new ArgumentNullException("log");
            }

            this.log = log;
            this.settings = settings ?? new ProbeSettings();
        }

        /// <summary>Every line the probe wrote, for the window's box.</summary>
        public IList<string> Lines
        {
            get { return lines; }
        }

        /// <summary>
        /// One model: the walk, the CSV beside its file, and the PROBE block. A model
        /// whose file the settings refuse is said and not read.
        /// </summary>
        public void ProbeModel(Model model)
        {
            if (model == null)
            {
                return;
            }

            string path = Words.Or(model.FileName, string.Empty);
            string name = path.Length == 0 ? "an unnamed model" : Path.GetFileName(path);
            string why;

            if (!ProbeSettings.MayRead(path, out why))
            {
                Say("PROBE    " + name + "  not read, " + why);
                return;
            }

            ProbeTally tally = new ProbeTally(settings.DistinctValueCap);
            Stopwatch clock = Stopwatch.StartNew();
            int items = 0;

            try
            {
                ModelItem root = model.RootItem;

                if (root != null)
                {
                    foreach (ModelItem item in root.DescendantsAndSelf)
                    {
                        items++;
                        ReadOne(item, tally);
                    }
                }
            }
            catch (Exception error)
            {
                log.Failure(
                    "walking the items of " + name,
                    error,
                    "kept going, the CSV carries what was read before it threw");
            }

            clock.Stop();
            Say("PROBE    " + name + "  walked " + items + (items == 1 ? " item" : " items") + " in "
                + clock.Elapsed.TotalSeconds.ToString("0.0", System.Globalization.CultureInfo.InvariantCulture)
                + "s");

            string csv = ProbeSettings.CsvPathFor(path);
            log.WriteAttempted("CSV", csv);

            try
            {
                File.WriteAllText(csv, ProbeCsv.Text(tally.Rows()), Encoding.UTF8);
            }
            catch (Exception error)
            {
                log.Failure(
                    "writing " + csv,
                    error,
                    "kept going, the block below still says what was read");
            }

            log.WriteFinished("CSV", csv);

            foreach (string line in ProbeVerdict.Lines(name, csv, tally, settings.Categories))
            {
                Say(line);
            }
        }

        /// <summary>
        /// One item. Its category first, and nothing more is read off an item whose
        /// category is not asked for, because that is most of a model.
        /// </summary>
        private void ReadOne(ModelItem item, ProbeTally tally)
        {
            string category;

            try
            {
                category = ClashHarvest.FirstPropertyOn(item, settings.CategoryNames);
            }
            catch (Exception)
            {
                return;
            }

            if (category.Length == 0 || !settings.Asks(category))
            {
                return;
            }

            tally.AddElement(category);

            try
            {
                using (PropertyCategoryCollection tabs = item.PropertyCategories)
                {
                    if (tabs == null)
                    {
                        return;
                    }

                    foreach (PropertyCategory tab in tabs)
                    {
                        using (tab)
                        {
                            ReadTab(tab, category, tally);
                        }
                    }
                }
            }
            catch (Exception)
            {
                // The element is counted and its rows are whatever was read before the
                // throw. One item that will not read must not cost the rest of the model.
            }
        }

        private static void ReadTab(PropertyCategory tab, string category, ProbeTally tally)
        {
            string tabName = Words.Or(tab.DisplayName, string.Empty);

            using (DataPropertyCollection properties = tab.Properties)
            {
                if (properties == null)
                {
                    return;
                }

                for (int i = 0; i < properties.Count; i++)
                {
                    using (DataProperty property = properties[i])
                    {
                        string value;

                        try
                        {
                            using (VariantData data = property.Value)
                            {
                                value = data == null ? string.Empty : ClashHarvest.Text(data);
                            }
                        }
                        catch (Exception)
                        {
                            // Its own try, so one value that will not read costs one cell.
                            value = string.Empty;
                        }

                        tally.Add(category, tabName, Words.Or(property.DisplayName, string.Empty), value);
                    }
                }
            }
        }

        private void Say(string line)
        {
            log.Line(line);
            lines.Add(line);
        }
    }
}
