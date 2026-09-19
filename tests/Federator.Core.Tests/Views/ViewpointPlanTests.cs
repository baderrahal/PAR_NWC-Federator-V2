using System;
using System.Collections.Generic;
using Federator.Core.Views;
using NUnit.Framework;

namespace Federator.Core.Tests
{
    /// <summary>
    /// One folder per discipline with one viewpoint in each.
    ///
    /// The rule most worth pinning here is the single discipline group, because it is the
    /// one a reasonable person would optimise away and it is the one that would make every
    /// NWF this tool writes a different shape from the others.
    /// </summary>
    [TestFixture]
    public class ViewpointPlanTests
    {
        /// <summary>
        /// The discipline codes alone, which is what the engine holds per job and now the
        /// only way in. The overload that took a BuildingGroup was called by no code in
        /// src, only by this fixture, so it went with its tests, which is the rule about a
        /// public member nothing calls.
        /// </summary>
        private static IList<string> Group(params string[] disciplines)
        {
            return new List<string>(disciplines);
        }

        private static ViewpointSettings Settings()
        {
            return new ViewpointSettings();
        }

        [Test]
        public void OneFolderAndOneViewpointPerDiscipline()
        {
            // Five and not three, because F53 gives Mechanical and Electrical a sub group
            // for their large items and Architecture has no pipe in it.
            IList<PlannedViewpoint> planned = ViewpointPlan.For(Group("AR", "ME", "EL"), Settings());

            Assert.That(planned.Count, Is.EqualTo(5));
            Assert.That(planned[0].Path, Is.EqualTo("AR/AR only"));
            Assert.That(planned[1].Path, Is.EqualTo("ME/ME only"));
            Assert.That(planned[2].Path, Is.EqualTo("ME/Over 150mm/ME over 150mm"));
            Assert.That(planned[3].Path, Is.EqualTo("EL/EL only"));
            Assert.That(planned[4].Path, Is.EqualTo("EL/Over 150mm/EL over 150mm"));
        }

        [Test]
        public void EachViewpointShowsItsOwnDisciplineAndHidesTheOthers()
        {
            IList<PlannedViewpoint> planned = ViewpointPlan.For(Group("AR", "ME", "EL"), Settings());

            Assert.That(planned[0].Shows, Is.EqualTo("AR"));
            Assert.That(planned[0].Hides, Is.EqualTo(new[] { "ME", "EL" }).AsCollection);

            Assert.That(planned[1].Shows, Is.EqualTo("ME"));
            Assert.That(planned[1].Hides, Is.EqualTo(new[] { "AR", "EL" }).AsCollection);
        }

        // ---------- F53, the sub groups ----------

        /// <summary>
        /// The large pipes, ducts and cable trays of Mechanical and Electrical get a sub
        /// group of their own. Architecture does not, because an architecture model has no
        /// pipe in it and a sub group holding everything is not a sub group.
        /// </summary>
        [Test]
        public void OnlyTheNamedDisciplinesGetASubGroupForTheirLargeItems()
        {
            IList<PlannedViewpoint> planned = ViewpointPlan.For(Group("AR", "ME"), Settings());

            Assert.That(planned[0].SubFolder, Is.Null, "AR has no sub group");
            Assert.That(planned[0].LargeItemsOnly, Is.False);

            Assert.That(planned[1].SubFolder, Is.Null, "the plain ME viewpoint holds everything mechanical");
            Assert.That(planned[1].LargeItemsOnly, Is.False);

            Assert.That(planned[2].SubFolder, Is.EqualTo("Over 150mm"));
            Assert.That(planned[2].LargeItemsOnly, Is.True);
            Assert.That(planned[2].Shows, Is.EqualTo("ME"));
        }

        /// <summary>
        /// The folder name is built from the threshold, so a folder reading Over 150mm
        /// beside a rule using 250 cannot happen. That is the kind of drift nobody notices.
        /// </summary>
        [Test]
        public void TheSubGroupFolderIsNamedFromTheThresholdItUses()
        {
            ViewpointSettings settings = new ViewpointSettings();
            settings.Sizes.ThresholdMillimetres = 250;

            IList<PlannedViewpoint> planned = ViewpointPlan.For(Group("ME"), settings);

            Assert.That(planned[1].SubFolder, Is.EqualTo("Over 250mm"));
            Assert.That(planned[1].Path, Is.EqualTo("ME/Over 250mm/ME over 250mm"));
        }

        [Test]
        public void WhichDisciplinesGetASubGroupIsASetting()
        {
            ViewpointSettings settings = new ViewpointSettings();
            settings.SubGroupDisciplines = new List<string> { "AR" };

            IList<PlannedViewpoint> planned = ViewpointPlan.For(Group("AR", "ME"), settings);

            Assert.That(planned.Count, Is.EqualTo(3));
            Assert.That(planned[1].SubFolder, Is.EqualTo("Over 150mm"), "AR has one now");
            Assert.That(planned[2].SubFolder, Is.Null, "and ME does not");
        }

        [Test]
        public void ADisciplineCodeIsMatchedExactlyAndNeverCased()
        {
            ViewpointSettings settings = new ViewpointSettings();

            Assert.That(settings.HasSubGroup("ME"), Is.True);
            Assert.That(settings.HasSubGroup("me"), Is.False);
            Assert.That(settings.HasSubGroup("ME "), Is.False);
            Assert.That(settings.HasSubGroup(null), Is.False);
        }

        [Test]
        public void ASubGroupViewpointHidesTheSameOtherDisciplinesThePlainOneDoes()
        {
            IList<PlannedViewpoint> planned = ViewpointPlan.For(Group("AR", "ME"), Settings());

            Assert.That(planned[2].Hides, Is.EqualTo(planned[1].Hides).AsCollection);
        }

        /// <summary>
        /// The one a reasonable person would skip. It hides nothing, which is not the same
        /// as having no viewpoint, and skipping it would make one NWF in the set a
        /// different shape from every other.
        /// </summary>
        [Test]
        public void AGroupOfOneDisciplineStillGetsItsFolderAndItsViewpoint()
        {
            IList<PlannedViewpoint> planned = ViewpointPlan.For(Group("AR"), Settings());

            Assert.That(planned.Count, Is.EqualTo(1));
            Assert.That(planned[0].Folder, Is.EqualTo("AR"));
            Assert.That(planned[0].Shows, Is.EqualTo("AR"));
            Assert.That(planned[0].Hides, Is.Empty);
        }

        [Test]
        public void ThePathIsTheFolderThenTheName()
        {
            IList<PlannedViewpoint> planned = ViewpointPlan.For(Group("ME"), Settings());

            Assert.That(planned[0].Path, Is.EqualTo("ME/ME only"));
        }

        [Test]
        public void TheNameSuffixIsASettingAndCanBeEmptied()
        {
            ViewpointSettings settings = new ViewpointSettings();
            settings.NameSuffix = string.Empty;

            IList<PlannedViewpoint> planned = ViewpointPlan.For(Group("ME"), settings);

            Assert.That(planned[0].Name, Is.EqualTo("ME"));
            Assert.That(planned[0].Path, Is.EqualTo("ME/ME"));
        }

        [Test]
        public void ADisciplineCodeIsNeverTrimmedOrCased()
        {
            IList<PlannedViewpoint> planned = ViewpointPlan.For(Group("me "), Settings());

            Assert.That(planned[0].Folder, Is.EqualTo("me "));
            Assert.That(planned[0].Shows, Is.EqualTo("me "));
        }

        [Test]
        public void AnEmptyDisciplineCodeIsLeftOutRatherThanMakingAFolderWithNoName()
        {
            IList<PlannedViewpoint> planned = ViewpointPlan.For(Group("AR", string.Empty, "ME"), Settings());

            Assert.That(planned.Count, Is.EqualTo(3), "AR, ME, and ME's large items sub group");
            Assert.That(planned[0].Shows, Is.EqualTo("AR"));
            Assert.That(planned[1].Shows, Is.EqualTo("ME"));
            Assert.That(planned[1].Hides, Is.EqualTo(new[] { "AR" }).AsCollection, "the empty one is not hidden either");
        }

        [Test]
        public void NoDisciplineAtAllPlansNothingAndDoesNotThrow()
        {
            IList<PlannedViewpoint> planned = ViewpointPlan.For(Group(), Settings());

            Assert.That(planned, Is.Empty);
            Assert.That(ViewpointPlan.Describe(planned), Does.Contain("no viewpoint planned"));
        }

        [Test]
        public void DescribeNamesEveryPathSoAFailedRunStillSaysWhatItMeantToDo()
        {
            IList<PlannedViewpoint> planned = ViewpointPlan.For(Group("AR", "ME"), Settings());
            string said = ViewpointPlan.Describe(planned);

            Assert.That(said, Does.StartWith("3 viewpoints planned: "));
            Assert.That(said, Does.Contain("AR/AR only"));
            Assert.That(said, Does.Contain("ME/ME only"));
            Assert.That(said, Does.Contain("ME/Over 150mm/ME over 150mm"));
        }

        [Test]
        public void OnePlannedReadsAsOneAndNotAsOneViewpoints()
        {
            Assert.That(
                ViewpointPlan.Describe(ViewpointPlan.For(Group("AR"), Settings())),
                Does.StartWith("1 viewpoint planned: "));
        }

        [Test]
        public void NullSettingsIsRefusedAndAMissingListIsNot()
        {
            Assert.That(() => ViewpointPlan.For(Group("AR"), null), Throws.ArgumentNullException);

            // A missing list is no disciplines rather than a throw, because the open file
            // run holds none and that is a real answer there.
            Assert.That(ViewpointPlan.For((IList<string>)null, Settings()), Is.Empty);
        }

        [Test]
        public void AFolderOrNameForNoDisciplineIsRefused()
        {
            ViewpointSettings settings = Settings();

            Assert.That(() => settings.FolderNameFor(null), Throws.ArgumentException);
            Assert.That(() => settings.ViewpointNameFor(string.Empty), Throws.ArgumentException);
        }
    }
}
