using System;
using System.IO;
using Federator.Core.Tests;
using NUnit.Framework;

namespace Federator.Core.Tests
{
    /// <summary>
    /// Where the fixtures find the files, and the Windows only fault that cost one CI run.
    ///
    /// `ExchangeFolder` walked up looking for a folder called "exchange" and returned the
    /// first one it found. There is a folder called "Exchange" under this test project,
    /// one per Core folder, and `Directory.Exists` is CASE BLIND on Windows and case
    /// sensitive off it. So the walk stopped at the test folder on the runner and at the
    /// checkout root in the container, and twelve tests passed here and failed there.
    ///
    /// The rules already name matching two paths without case as a Windows file system
    /// difference, and this is what one looks like when nobody is watching for it.
    /// </summary>
    [TestFixture]
    public class SamplesPathTests
    {
        /// <summary>
        /// The trap itself, asserted rather than described. A folder whose name differs
        /// from "exchange" only in case sits above the test assembly, and it is NOT what
        /// the helper returns.
        /// </summary>
        [Test]
        public void TheExchangeFolderIsTheCheckoutsAndNotTheTestFolderOfTheSameName()
        {
            string decoy = Path.Combine(Samples.Repo(), "tests", "Federator.Core.Tests", "Exchange");

            Assert.That(Directory.Exists(decoy), Is.True,
                "the test folder this walk used to stop at is still there, so the trap is real");

            string exchange = Samples.ExchangeFolder();

            Assert.That(Path.GetFullPath(exchange),
                Is.EqualTo(Path.GetFullPath(Path.Combine(Samples.Repo(), "exchange"))));
            Assert.That(Path.GetFullPath(exchange),
                Is.Not.EqualTo(Path.GetFullPath(decoy)),
                "it stopped at the test folder, which is what it did on the Windows runner");
        }

        [Test]
        public void TheCorrectedMatrixIsWhereTheFolderSaysItIs()
        {
            Assert.That(File.Exists(Samples.CorrectedMatrix()), Is.True, Samples.CorrectedMatrix());
        }

        [Test]
        public void TheSamplesFolderIsTheCheckoutsToo()
        {
            Assert.That(Path.GetFullPath(Samples.Folder()),
                Is.EqualTo(Path.GetFullPath(Path.Combine(Samples.Repo(), "samples"))));
            Assert.That(File.Exists(Samples.Matrix()), Is.True);
            Assert.That(File.Exists(Samples.PriorityMap()), Is.True);
            Assert.That(File.Exists(Samples.ByDesign()), Is.True);
        }

        /// <summary>
        /// Both folders are found off the ONE walk, which looks for the solution FILE by
        /// its exact name and cannot match anything else. A second walk for a folder name
        /// is what this fixture exists to stop coming back.
        /// </summary>
        [Test]
        public void BothFoldersSitDirectlyUnderTheCheckout()
        {
            Assert.That(Samples.Repo(), Is.Not.Null);
            Assert.That(File.Exists(Path.Combine(Samples.Repo(), "ParsonsNwcFederator.sln")), Is.True);
            Assert.That(Path.GetDirectoryName(Path.GetFullPath(Samples.ExchangeFolder())),
                Is.EqualTo(Path.GetFullPath(Samples.Repo()).TrimEnd(Path.DirectorySeparatorChar)));
            Assert.That(Path.GetDirectoryName(Path.GetFullPath(Samples.Folder())),
                Is.EqualTo(Path.GetFullPath(Samples.Repo()).TrimEnd(Path.DirectorySeparatorChar)));
        }
    }
}
