using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Federator.Core.Exchange;
using Federator.Core.Naming;
using Federator.Core.Sets;

namespace Federator.Core.Generic
{
    /// <summary>
    /// One model of a group, as the Generic Models plan is handed it, F128. The file name names the
    /// model and its set. The text to find in the Source File of an item is the file's stem unless
    /// the caller hands another, because which text of the model the Source File property of an
    /// item carries, the NWC's name or the name of the Revit file it was published from, is UNKNOWN
    /// until the probe of the laptop lane reads it. A different text is the caller's to hand in
    /// after that, and nothing here guesses it.
    /// </summary>
    public sealed class GenericModelInput
    {
        public GenericModelInput(string modelFile)
            : this(modelFile, null)
        {
        }

        public GenericModelInput(string modelFile, string matchText)
        {
            if (!string.IsNullOrEmpty(matchText) && string.IsNullOrWhiteSpace(matchText))
            {
                throw new ArgumentException("The text to find in a Source File cannot be only spaces.", "matchText");
            }

            ModelFile = modelFile;
            MatchText = matchText;
        }

        /// <summary>The model's file name or its path, as the document holds it.</summary>
        public string ModelFile { get; private set; }

        /// <summary>The text its items' Source File holds, or null to use the stem of the file name.</summary>
        public string MatchText { get; private set; }
    }

    /// <summary>One model's set in the plan, F128.</summary>
    public sealed class GenericModelSet
    {
        internal GenericModelSet(string modelFile, string modelName, string matchText, PlannedSet set)
        {
            ModelFile = modelFile;
            ModelName = modelName;
            MatchText = matchText;
            Set = set;
        }

        /// <summary>The model's file name or path as it was handed in.</summary>
        public string ModelFile { get; private set; }

        /// <summary>The model's file name with any folder and extension taken off, which names its set.</summary>
        public string ModelName { get; private set; }

        /// <summary>What the set's Source File condition looks for.</summary>
        public string MatchText { get; private set; }

        /// <summary>The set the add-in builds, a planned set like any other, in the folder of the plan.</summary>
        public PlannedSet Set { get; private set; }
    }

    /// <summary>
    /// The search sets that count the Generic Models of a group, one set for each model, F128 and
    /// FR-177, Bader's request 3 under Q112: a folder named Generic Models in the saved sets tree
    /// and in it one search set for each model of the group, whose conditions are the category and the
    /// model's file, and no clash test for any of them.
    ///
    /// A SET IS A PLANNED SET, THE SAME TYPE THE PICKED FILE'S SETS ARE, so the add-in builds it with the
    /// machinery that builds those and nothing else reads a second shape. Its two conditions are the
    /// two the client's own file writes, a category condition and a Source File condition, in one group
    /// so they are ANDed: the Category property of the Element tab equals the value, and the Source File
    /// contains the model's text. The words are read by a test off the client's file, so the plan
    /// answers to that file and not to whoever typed it.
    ///
    /// THE PLAN SAYS WHAT IT NOTICED AND NEVER ACTS ON IT. Two models of one name are one set, and the
    /// plan says so, and where the two were handed different texts it says the second is not looked for.
    /// A model with no file name has no set, and the plan says so. A text that one model's condition looks
    /// for and another model's text also holds finds the other model's items too, and the plan names both,
    /// because the count of the first is then the sum of the two, and models given the same text count the
    /// same items twice. Nothing is changed and nothing is left out, Bader decides.
    /// </summary>
    public sealed class GenericModelsPlan
    {
        private readonly List<GenericModelSet> sets = new List<GenericModelSet>();
        private readonly List<string> notes = new List<string>();

        private GenericModelsPlan(GenericModelsSettings settings)
        {
            Folder = settings.FolderName;
            CategoryValue = settings.CategoryValue;
            Sets = new ReadOnlyCollection<GenericModelSet>(sets);
            Notes = new ReadOnlyCollection<string>(notes);
        }

        /// <summary>The folder under the root of the saved sets tree that holds the sets.</summary>
        public string Folder { get; private set; }

        /// <summary>What the Category property of an item reads for a Generic Models item.</summary>
        public string CategoryValue { get; private set; }

        /// <summary>One set for each model of the group, in the order the models were handed in.</summary>
        public ReadOnlyCollection<GenericModelSet> Sets { get; private set; }

        /// <summary>What the plan noticed, a line each, the group's models never changed by it.</summary>
        public ReadOnlyCollection<string> Notes { get; private set; }

        /// <summary>
        /// True where the text of one set also finds the items another set finds, so the total over the sets is
        /// not a count of items. The notes name which.
        /// </summary>
        public bool TextsMeet { get; private set; }

        /// <summary>What every set asks of the category, in the words a log carries.</summary>
        public string Asked
        {
            get { return "Category equals \"" + CategoryValue + "\""; }
        }

        /// <summary>
        /// The sets as the plan the set builder already takes, so the add-in builds them with the machinery
        /// that builds the picked file's sets and nothing reads a second shape. Every set is buildable and
        /// none is skipped.
        /// </summary>
        public SetBuildPlan ToBuildPlan()
        {
            List<PlannedSet> planned = new List<PlannedSet>();

            foreach (GenericModelSet model in sets)
            {
                planned.Add(model.Set);
            }

            return SetBuildPlan.Of(planned);
        }

        /// <summary>The full path of the set of a model name, in the shape the picked file's sets are named.</summary>
        internal string PathOf(string modelName)
        {
            return new ExchangeReader().BuildPath(new List<string> { Folder }, modelName);
        }

        /// <summary>
        /// The plan for the models of one group. No model hands a plan with no set. A null model among
        /// them is a model with no file name, which has no set and is counted on a note.
        /// </summary>
        public static GenericModelsPlan For(IEnumerable<GenericModelInput> models, GenericModelsSettings settings)
        {
            if (settings == null)
            {
                throw new ArgumentNullException("settings");
            }

            GenericModelsPlan plan = new GenericModelsPlan(settings);

            if (models == null)
            {
                return plan;
            }

            Dictionary<string, SharedName> named = new Dictionary<string, SharedName>(ContainerName.StemComparer);
            List<SharedName> sharedNames = new List<SharedName>();
            int nameless = 0;

            foreach (GenericModelInput model in models)
            {
                string name = model == null || model.ModelFile == null ? string.Empty : ContainerName.Stem(model.ModelFile);

                if (name.Length == 0)
                {
                    nameless++;
                    continue;
                }

                string match = string.IsNullOrEmpty(model.MatchText) ? name : model.MatchText;
                SharedName first;

                if (named.TryGetValue(name, out first))
                {
                    // Said by the spelling the set carries, the first, and once however many models share it.
                    if (first.Models == 1)
                    {
                        sharedNames.Add(first);
                    }

                    first.Models++;

                    if (!string.Equals(first.Text, match, StringComparison.OrdinalIgnoreCase)
                        && !first.OtherTexts.Contains(match, StringComparer.OrdinalIgnoreCase))
                    {
                        first.OtherTexts.Add(match);
                    }

                    continue;
                }

                named.Add(name, new SharedName(name, match));
                plan.sets.Add(new GenericModelSet(model.ModelFile, name, match, plan.SetOf(name, match, settings)));
            }

            if (nameless > 0)
            {
                plan.notes.Add(nameless + (nameless == 1 ? " model of the group has" : " models of the group have")
                    + " no file name, so " + (nameless == 1 ? "it has" : "they have") + " no Generic Models set");
            }

            foreach (SharedName shared in sharedNames)
            {
                if (shared.OtherTexts.Count > 0)
                {
                    plan.notes.Add(shared.Models + " models of the group are named " + shared.Name + " and were handed the texts "
                        + shared.Text + " and " + string.Join(" and ", shared.OtherTexts)
                        + ", so one set serves them and looks for " + shared.Text + " only, and the items that carry the others are not counted");
                }
                else if (shared.Models == 2)
                {
                    plan.notes.Add("two models of the group are named " + shared.Name + ", so one set serves both and counts both");
                }
                else
                {
                    plan.notes.Add(shared.Models + " models of the group are named " + shared.Name
                        + ", so one set serves all of them and counts all of them");
                }
            }

            plan.NoteTextsThatMeet();
            return plan;
        }

        private PlannedSet SetOf(string name, string match, GenericModelsSettings settings)
        {
            List<PlannedCondition> conditions = new List<PlannedCondition>
            {
                new PlannedCondition(
                    ConditionTest.Equals,
                    0,
                    GenericModelsSettings.ElementCategoryInternalName,
                    GenericModelsSettings.ElementCategoryDisplayName,
                    GenericModelsSettings.CategoryPropertyInternalName,
                    GenericModelsSettings.CategoryPropertyDisplayName,
                    GenericModelsSettings.ValueType,
                    settings.CategoryValue),
                new PlannedCondition(
                    ConditionTest.Contains,
                    0,
                    null,
                    null,
                    GenericModelsSettings.SourceFilePropertyInternalName,
                    GenericModelsSettings.SourceFilePropertyDisplayName,
                    GenericModelsSettings.ValueType,
                    match)
            };

            return new PlannedSet(name, PathOf(name), new List<string> { Folder }, conditions);
        }

        /// <summary>
        /// A set whose text another set's text holds, or two sets of one text, find items of more than one model,
        /// which is said and not changed. One note for each text that is shared and one for each text that another
        /// holds, however many models are in it, so n models of one text make one line and not n squared.
        /// </summary>
        private void NoteTextsThatMeet()
        {
            HashSet<int> grouped = new HashSet<int>();

            for (int i = 0; i < sets.Count; i++)
            {
                if (grouped.Contains(i))
                {
                    continue;
                }

                List<string> sameText = new List<string> { sets[i].ModelName };

                for (int j = i + 1; j < sets.Count; j++)
                {
                    if (string.Equals(sets[i].MatchText, sets[j].MatchText, StringComparison.OrdinalIgnoreCase))
                    {
                        sameText.Add(sets[j].ModelName);
                        grouped.Add(j);
                    }
                }

                if (sameText.Count > 1)
                {
                    notes.Add("the models " + Joined(sameText) + " are " + (sameText.Count == 2 ? "both" : "all")
                        + " found by the text " + sets[i].MatchText + ", so each of their sets can count the items of "
                        + (sameText.Count == 2 ? "both" : "all of them"));
                    TextsMeet = true;
                }
            }

            for (int i = 0; i < sets.Count; i++)
            {
                List<string> holders = new List<string>();

                for (int j = 0; j < sets.Count; j++)
                {
                    string inner = sets[i].MatchText;
                    string outer = sets[j].MatchText;

                    if (i != j
                        && !string.Equals(inner, outer, StringComparison.OrdinalIgnoreCase)
                        && outer.IndexOf(inner, StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        holders.Add(sets[j].ModelName);
                    }
                }

                if (holders.Count > 0)
                {
                    notes.Add("the text " + sets[i].MatchText + " that finds the model " + sets[i].ModelName
                        + " is also in the text of the " + (holders.Count == 1 ? "model " : "models ") + Joined(holders)
                        + ", so the set of " + sets[i].ModelName + " can count the items of "
                        + (holders.Count == 1 ? "that model" : "those models") + " too");
                    TextsMeet = true;
                }
            }
        }

        private static string Joined(IList<string> names)
        {
            return names.Count == 1
                ? names[0]
                : string.Join(", ", names.Take(names.Count - 1)) + " and " + names[names.Count - 1];
        }

        /// <summary>A model name more than one model of the group carries, and the texts those models were handed.</summary>
        private sealed class SharedName
        {
            public SharedName(string name, string text)
            {
                Name = name;
                Text = text;
                Models = 1;
                OtherTexts = new List<string>();
            }

            public string Name { get; private set; }

            public string Text { get; private set; }

            public int Models { get; set; }

            public List<string> OtherTexts { get; private set; }
        }
    }
}
