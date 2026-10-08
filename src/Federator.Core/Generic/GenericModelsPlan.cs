using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    /// plan says so. A model with no file name has no set, and the plan says so. A text that one model's
    /// condition looks for and another model's file name also holds finds the other model's items too,
    /// and the plan names both, because the count of the first is then the sum of the two, and two
    /// models given the same text count the same items twice. Nothing is changed and nothing is left
    /// out, Bader decides.
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
            return ExchangeReader.SelectionSetTreeRoot + "/" + Folder + "/" + modelName;
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

            Dictionary<string, string> named = new Dictionary<string, string>(ContainerName.StemComparer);
            HashSet<string> shared = new HashSet<string>(ContainerName.StemComparer);
            List<string> sharedNames = new List<string>();
            int nameless = 0;

            foreach (GenericModelInput model in models)
            {
                string name = model == null || model.ModelFile == null ? string.Empty : ContainerName.Stem(model.ModelFile);

                if (name.Length == 0)
                {
                    nameless++;
                    continue;
                }

                string firstSpelling;

                if (named.TryGetValue(name, out firstSpelling))
                {
                    // Said by the spelling the set carries, the first, and once however many models share it.
                    if (shared.Add(name))
                    {
                        sharedNames.Add(firstSpelling);
                    }

                    continue;
                }

                named.Add(name, name);

                string match = string.IsNullOrEmpty(model.MatchText) ? name : model.MatchText;
                plan.sets.Add(new GenericModelSet(model.ModelFile, name, match, plan.SetOf(name, match, settings)));
            }

            if (nameless > 0)
            {
                plan.notes.Add(nameless + (nameless == 1 ? " model of the group has" : " models of the group have")
                    + " no file name, so " + (nameless == 1 ? "it has" : "they have") + " no Generic Models set");
            }

            foreach (string name in sharedNames)
            {
                plan.notes.Add("two models of the group are named " + name + ", so one set serves both and counts both");
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
        /// A set whose text another model's text holds, or two sets of one text, find items of more than
        /// one model, which is said and not changed.
        /// </summary>
        private void NoteTextsThatMeet()
        {
            for (int i = 0; i < sets.Count; i++)
            {
                for (int j = 0; j < sets.Count; j++)
                {
                    if (i == j)
                    {
                        continue;
                    }

                    string inner = sets[i].MatchText;
                    string outer = sets[j].MatchText;

                    if (string.Equals(inner, outer, StringComparison.OrdinalIgnoreCase))
                    {
                        if (i < j)
                        {
                            notes.Add("the models " + sets[i].ModelName + " and " + sets[j].ModelName
                                + " are both found by the text " + inner + ", so each of their sets counts the items of both");
                        }

                        continue;
                    }

                    if (outer.IndexOf(inner, StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        notes.Add("the text " + inner + " that finds the model " + sets[i].ModelName
                            + " is also in the text " + outer + " of the model " + sets[j].ModelName
                            + ", so the set of the first counts the items of both");
                    }
                }
            }
        }
    }
}
