using System.IO;
using NUnit.Framework;

namespace Federator.Core.Tests
{
    /// <summary>
    /// F182, Bader's answer B to Q175. The engine cannot be built here, because it needs Navisworks, so the order
    /// a group runs its steps in is read off its source, the way a test for a list read off a file reads the file.
    /// The NWF is saved and the NWD published BEFORE the first picture is taken, and every report that links a
    /// picture is written after the pictures.
    /// </summary>
    [TestFixture]
    public class GroupOrderSourceTests
    {
        private static string Read(params string[] parts)
        {
            string repo = Samples.Repo();
            Assert.That(repo, Is.Not.Null, "the checkout was not found from the test assembly");

            string path = Path.Combine(repo, Path.Combine(parts));
            Assert.That(File.Exists(path), Is.True, path);
            return File.ReadAllText(path);
        }

        private static string BodyOf(string source, string signature, string endMarker)
        {
            int start = source.IndexOf(signature, System.StringComparison.Ordinal);
            Assert.That(start, Is.GreaterThanOrEqualTo(0), signature + " is not in the source");

            int end = source.IndexOf(endMarker, start, System.StringComparison.Ordinal);
            Assert.That(end, Is.GreaterThan(start), endMarker + " does not follow " + signature);

            return source.Substring(start, end - start);
        }

        private static int At(string body, string token)
        {
            int index = body.IndexOf(token, System.StringComparison.Ordinal);
            Assert.That(index, Is.GreaterThanOrEqualTo(0), token + " is not called in the group's tail");
            return index;
        }

        private static string TheGroupsTail()
        {
            return BodyOf(
                Read("src", "Federator.Addin", "Engine", "FederationEngine.cs"),
                "private void FinishTheGroup(",
                "// ---------- the live line, F62 ----------");
        }

        [Test]
        public void TheNwdIsPublishedAndTheNwfConfirmedBeforeThePicturesAreTaken()
        {
            string tail = TheGroupsTail();
            int views = At(tail, "BuildViewpoints(document, job, outcome)");
            int saved = At(tail, "SaveTheNwfAgain(document, job, outcome)");
            int nwd = At(tail, "WriteNwd(document, job, outcome)");
            int confirm = At(tail, "ConfirmTheNwfSurvived(job, outcome)");
            int pictures = At(tail, "TakeThePicturesOfTheGroup(job, outcome, ended)");

            Assert.That(views, Is.LessThan(saved), "the viewpoints are built before the NWF is saved");
            Assert.That(saved, Is.LessThan(nwd));
            Assert.That(nwd, Is.LessThan(confirm));
            Assert.That(confirm, Is.LessThan(pictures), "the NWD step is reached before any picture is taken");
        }

        [Test]
        public void EveryReportThatLinksAPictureIsWrittenAfterThePicturesAreTaken()
        {
            string tail = TheGroupsTail();
            int pictures = At(tail, "TakeThePicturesOfTheGroup(job, outcome, ended)");

            Assert.That(At(tail, "WriteWorkbook(job, outcome)"), Is.GreaterThan(pictures), "the Excel, HTML and XML link the jpgs");
            Assert.That(At(tail, "TakeTheCoverage(document, job, outcome)"), Is.GreaterThan(pictures));
            Assert.That(At(tail, "WriteTheInternalWorkbook(job, outcome)"), Is.GreaterThan(pictures));
        }

        [Test]
        public void TheGroupsOwnClashStepLeavesThePicturesToTheEngineAndTheHandButtonDoesNot()
        {
            string engine = Read("src", "Federator.Addin", "Engine", "FederationEngine.cs");

            Assert.That(engine, Does.Contain("CreateAndRunTheTests(document, job, outcome, source, true)"));
            Assert.That(engine, Does.Contain("CreateAndRunTheTests(document, job, outcome, ClashSource.TestsFromXml, false)"));
        }

        [Test]
        public void TheRunnerTakesThePicturesInsideRunOnlyWhenTheyAreNotDeferred()
        {
            string runner = Read("src", "Federator.Addin", "Engine", "ClashRunner.cs");
            string run = BodyOf(runner, "public ClashRunOutcome Run(ClashTestPlan plan)", "private void EndTheHidingOfTheModels()");
            Assert.That(
                System.Text.RegularExpressions.Regex.IsMatch(
                    run, @"if \(!PicturesDeferred\)\s*\{\s*RenderThePicturesWaiting\(document, clashTests\);"),
                Is.True,
                "the pass in Run sits directly under the deferral check");
        }
    }
}
