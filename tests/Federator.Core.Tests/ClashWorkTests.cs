using Federator.Core.Clash;
using Federator.Core.Exchange;
using NUnit.Framework;

namespace Federator.Core.Tests
{
    /// <summary>
    /// What the run does about clash, given whatever was picked. Nothing picked is a step
    /// switched off, so the run does the model side and nothing else. A file holding only
    /// sets, only tests, or both are all normal, so each half is decided on its own.
    /// </summary>
    [TestFixture]
    public class ClashWorkTests
    {
        [Test]
        public void NoFilePickedMeansTheModelSideAndNothingElse()
        {
            Assert.That(ClashWork.Any(null), Is.False);
            Assert.That(ClashWork.BuildsSets(null), Is.False);
            Assert.That(ClashWork.CreatesTests(null), Is.False);
            Assert.That(ClashWork.Describe(null), Is.EqualTo(ClashWork.NothingPicked));
            Assert.That(ClashWork.Describe(null), Does.Contain("no set built and no test created"));
        }

        [Test]
        public void AFileHoldingBothDoesBothHalves()
        {
            ExchangeDocument both = new ExchangeReader().ReadFile(Samples.AllInOne());

            Assert.That(ClashWork.BuildsSets(both), Is.True);
            Assert.That(ClashWork.CreatesTests(both), Is.True);
            Assert.That(ClashWork.Any(both), Is.True);
            Assert.That(ClashWork.Describe(both), Does.Contain("61 sets"));
            Assert.That(ClashWork.Describe(both), Does.Contain("1830 tests"));
        }

        [Test]
        public void AFileHoldingSetsOnlyBuildsSetsAndCreatesNoTest()
        {
            ExchangeDocument setsOnly = new ExchangeReader().ReadFile(Samples.Infra());

            Assert.That(ClashWork.BuildsSets(setsOnly), Is.True);
            Assert.That(ClashWork.CreatesTests(setsOnly), Is.False);
            Assert.That(ClashWork.Any(setsOnly), Is.True);
        }

        // A project keeping its sets in the model and supplying only tests is normal.
        [Test]
        public void AFileHoldingTestsOnlyCreatesTestsAndBuildsNoSet()
        {
            ExchangeDocument testsOnly = new ExchangeReader().ReadText(
                "<exchange units=\"ft\"><batchtest name=\"b\">"
                + "<clashtest name=\"T\" test_type=\"hard\" tolerance=\"0.25\" merge_composites=\"1\">"
                + "<left><clashselection><locator>lcop_selection_set_tree/A</locator>"
                + "</clashselection></left>"
                + "<right><clashselection><locator>lcop_selection_set_tree/B</locator>"
                + "</clashselection></right>"
                + "</clashtest></batchtest></exchange>");

            Assert.That(ClashWork.BuildsSets(testsOnly), Is.False);
            Assert.That(ClashWork.CreatesTests(testsOnly), Is.True);
            Assert.That(ClashWork.Any(testsOnly), Is.True);
        }

        [Test]
        public void AFileHoldingNeitherIsNothingToDoRatherThanAnError()
        {
            ExchangeDocument empty = new ExchangeReader().ReadText("<exchange units=\"ft\" />");

            Assert.That(ClashWork.Any(empty), Is.False);
            Assert.That(ClashWork.Describe(empty), Does.Contain("nothing to do with it"));
        }

        [Test]
        public void TheDescriptionCountsWhatIsActuallyInTheFile()
        {
            ExchangeDocument one = new ExchangeReader().ReadText(
                "<exchange units=\"ft\"><batchtest name=\"b\">"
                + "<clashtest name=\"T\" test_type=\"hard\" tolerance=\"0.25\" merge_composites=\"1\">"
                + "<left><clashselection><locator>a</locator></clashselection></left>"
                + "<right><clashselection><locator>b</locator></clashselection></right>"
                + "</clashtest></batchtest></exchange>");

            Assert.That(ClashWork.Describe(one), Does.Contain("0 sets and 1 test"));
        }
    }
}
