using System;
using System.Collections.Generic;
using Federator.Core.Grouping;
using Federator.Core.Naming;
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
        private static BuildingGroup Group(params string[] disciplines)
        {
            List<ParsedContainerName> files = new List<ParsedContainerName>();
            List<string> codes = new List<string>(disciplines);

            return new BuildingGroup(
                "1C07BC",
                "1104",
                "PAR",
                files,
                codes,
                "1C07BC",
                codes.Count == 1 ? codes[0] : null);
        }

        private static ViewpointSettings Settings()
        {
            return new ViewpointSettings();
        }

        [Test]
        public void OneFolderAndOneViewpointPerDiscipline()
        {
            IList<PlannedViewpoint> planned = ViewpointPlan.For(Group("AR", "ME", "EL"), Settings());

            Assert.That(planned.Count, Is.EqualTo(3));
            Assert.That(planned[0].Folder, Is.EqualTo("AR"));
            Assert.That(planned[1].Folder, Is.EqualTo("ME"));
            Assert.That(planned[2].Folder, Is.EqualTo("EL"));
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

            Assert.That(planned.Count, Is.EqualTo(2));
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

            Assert.That(said, Does.StartWith("2 viewpoints planned: "));
            Assert.That(said, Does.Contain("AR/AR only"));
            Assert.That(said, Does.Contain("ME/ME only"));
        }

        [Test]
        public void OnePlannedReadsAsOneAndNotAsOneViewpoints()
        {
            Assert.That(
                ViewpointPlan.Describe(ViewpointPlan.For(Group("AR"), Settings())),
                Does.StartWith("1 viewpoint planned: "));
        }

        [Test]
        public void ANullGroupOrNullSettingsIsRefused()
        {
            Assert.That(
                () => ViewpointPlan.For((BuildingGroup)null, Settings()),
                Throws.ArgumentNullException);

            Assert.That(() => ViewpointPlan.For(Group("AR"), null), Throws.ArgumentNullException);

            // The list form takes a missing list as no disciplines rather than throwing,
            // because the open file run holds none and that is a real answer there.
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
