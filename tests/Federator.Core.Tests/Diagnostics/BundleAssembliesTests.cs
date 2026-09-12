using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Federator.Core.Diagnostics;
using NUnit.Framework;

namespace Federator.Core.Tests
{
    /// <summary>
    /// A run got as far as writing the workbook and then failed because
    /// System.Numerics.Vectors 4.1.3.0 could not be loaded, while a copy of that assembly
    /// was sitting in the bundle at 4.1.4.0.
    ///
    /// These cannot reproduce that. This test process has a config file carrying the
    /// binding redirects NuGet writes, and every dependency sits beside it, so the load
    /// always succeeds here whatever the code does. What they can pin is the decision the
    /// resolver makes, which is the part that has to be right.
    /// </summary>
    [TestFixture]
    public class BundleAssembliesTests
    {
        private const string Vectors =
            "System.Numerics.Vectors, Version=4.1.3.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a";

        private string folder;

        [SetUp]
        public void MakeFolder()
        {
            folder = Path.Combine(Path.GetTempPath(), "FederatorBundle", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(folder);
        }

        [TearDown]
        public void RemoveFolder()
        {
            try
            {
                if (Directory.Exists(folder))
                {
                    Directory.Delete(folder, true);
                }
            }
            catch (IOException)
            {
                // A leftover temp folder is not worth failing a test over.
            }
            catch (UnauthorizedAccessException)
            {
                // Same, and it is what a file still held open looks like.
            }
        }

        // ---------- reading the request ----------

        [Test]
        public void TheSimpleNameIsEverythingBeforeTheFirstComma()
        {
            Assert.That(BundleAssemblies.SimpleName(Vectors), Is.EqualTo("System.Numerics.Vectors"));
            Assert.That(BundleAssemblies.SimpleName("ClosedXML"), Is.EqualTo("ClosedXML"));
            Assert.That(BundleAssemblies.SimpleName("  Spaced , Version=1.0.0.0"), Is.EqualTo("Spaced"));
            Assert.That(BundleAssemblies.SimpleName(null), Is.EqualTo(string.Empty));
            Assert.That(BundleAssemblies.SimpleName(string.Empty), Is.EqualTo(string.Empty));
        }

        // A satellite lookup is meant to fail. Answering one would recurse.
        [Test]
        public void AResourceLookupIsLeftAlone()
        {
            Assert.That(
                BundleAssemblies.IsResourceRequest("Federator.Core.resources, Version=1.0.0.0"),
                Is.True);
            Assert.That(BundleAssemblies.IsResourceRequest(Vectors), Is.False);

            File.WriteAllText(Path.Combine(folder, "Federator.Core.resources.dll"), "x");

            Assert.That(
                BundleAssemblies.FileFor("Federator.Core.resources, Version=1.0.0.0", folder),
                Is.Null,
                "a resource lookup was answered, which recurses");
        }

        // ---------- the version is deliberately ignored ----------

        // The whole point. The file is there, at a different version, and that is exactly
        // the case .NET Framework refuses and this has to accept.
        [Test]
        public void AFileIsFoundByNameEvenWhenTheVersionAskedForIsDifferent()
        {
            string path = Path.Combine(folder, "System.Numerics.Vectors.dll");
            File.WriteAllText(path, "pretend assembly at 4.1.4.0");

            Assert.That(BundleAssemblies.FileFor(Vectors, folder), Is.EqualTo(path));
        }

        [Test]
        public void EveryOneOfTheFourMismatchesResolvesToItsFile()
        {
            string[] wanted =
            {
                "System.Numerics.Vectors, Version=4.1.3.0",
                "System.Runtime.CompilerServices.Unsafe, Version=4.0.4.1",
                "System.Buffers, Version=4.0.2.0",
                "System.Memory, Version=4.0.1.1"
            };

            foreach (string name in wanted)
            {
                File.WriteAllText(
                    Path.Combine(folder, BundleAssemblies.SimpleName(name) + ".dll"), "x");
            }

            foreach (string name in wanted)
            {
                Assert.That(BundleAssemblies.FileFor(name, folder), Is.Not.Null, name);
            }
        }

        [Test]
        public void AnAssemblyThatIsNotThereGivesNothingRatherThanAGuess()
        {
            Assert.That(BundleAssemblies.FileFor(Vectors, folder), Is.Null);
        }

        [Test]
        public void AnExeIsFoundAsWellAsADll()
        {
            string path = Path.Combine(folder, "Something.exe");
            File.WriteAllText(path, "x");

            Assert.That(BundleAssemblies.FileFor("Something, Version=1.0.0.0", folder),
                Is.EqualTo(path));
        }

        [Test]
        public void NoFolderMeansNothingToAnswerWith()
        {
            Assert.That(BundleAssemblies.FileFor(Vectors, null), Is.Null);
            Assert.That(BundleAssemblies.FileFor(Vectors, string.Empty), Is.Null);
            Assert.That(BundleAssemblies.FileFor(null, folder), Is.Null);
        }

        [Test]
        public void ANameThatCannotBeAPathIsRefusedRatherThanThrowing()
        {
            Assert.That(BundleAssemblies.FileFor("bad|name, Version=1.0.0.0", folder), Is.Null);
        }

        [Test]
        public void TheFolderIsTheOneThisAssemblyIsIn()
        {
            string found = BundleAssemblies.FolderOfThisAssembly();

            Assert.That(found, Is.Not.Null.And.Not.Empty);
            Assert.That(Directory.Exists(found), Is.True);
            Assert.That(File.Exists(Path.Combine(found, "Federator.Core.dll")), Is.True,
                "the resolver would look in a folder that does not hold the add-in");
        }

        // ---------- the install time check ----------

        /// <summary>
        /// Two real assemblies where one references the other, so the check is exercised
        /// against genuine metadata rather than against files pretending to be assemblies.
        /// </summary>
        private void CopyRealAssemblies(params string[] names)
        {
            string from = BundleAssemblies.FolderOfThisAssembly();

            foreach (string name in names)
            {
                File.Copy(Path.Combine(from, name), Path.Combine(folder, name), true);
            }
        }

        // The one the brief asks for by name.
        [Test]
        public void AMissingAssemblyIsCaughtRatherThanLeftForTheRun()
        {
            // Federator.Core references ClosedXML. Ship one and not the other.
            CopyRealAssemblies("Federator.Core.dll");

            IList<string> missing = BundleAssemblies.MissingFrom(folder, new string[0]);

            Assert.That(missing, Does.Contain("ClosedXML"),
                "a missing assembly was not caught at install time");
        }

        [Test]
        public void ShippingBothLeavesNothingMissingBetweenThem()
        {
            CopyRealAssemblies("Federator.Core.dll", "ClosedXML.dll");

            IList<string> missing = BundleAssemblies.MissingFrom(folder, new string[0]);

            Assert.That(missing, Does.Not.Contain("ClosedXML"));
        }

        // The framework and Navisworks supply their own, so those are not missing.
        [Test]
        public void SomethingSuppliedElsewhereIsNotReportedMissing()
        {
            CopyRealAssemblies("Federator.Core.dll");

            IList<string> missing = BundleAssemblies.MissingFrom(
                folder, new[] { "ClosedXML", "mscorlib", "System", "System.Core", "System.Xml" });

            Assert.That(missing, Does.Not.Contain("ClosedXML"));
            Assert.That(missing, Does.Not.Contain("mscorlib"));
        }

        [Test]
        public void EachMissingNameIsReportedOnceAndInOrder()
        {
            CopyRealAssemblies("Federator.Core.dll");

            IList<string> missing = BundleAssemblies.MissingFrom(folder, new string[0]);
            List<string> sorted = new List<string>(missing);
            sorted.Sort(StringComparer.OrdinalIgnoreCase);

            Assert.That(missing, Is.EqualTo(sorted), "the report is not in a settled order");
            Assert.That(new HashSet<string>(missing, StringComparer.OrdinalIgnoreCase).Count,
                Is.EqualTo(missing.Count), "a name was reported twice");
        }

        [Test]
        public void AFolderThatIsNotThereReportsNothingRatherThanThrowing()
        {
            Assert.That(BundleAssemblies.MissingFrom(null, new string[0]).Count, Is.EqualTo(0));
            Assert.That(
                BundleAssemblies.MissingFrom(Path.Combine(folder, "nope"), new string[0]).Count,
                Is.EqualTo(0));
        }

        [Test]
        public void SomethingInTheFolderThatIsNotAnAssemblyIsSteppedOver()
        {
            CopyRealAssemblies("Federator.Core.dll", "ClosedXML.dll");
            File.WriteAllText(Path.Combine(folder, "notanassembly.dll"), "this is not a PE file");

            Assert.That(
                delegate { BundleAssemblies.MissingFrom(folder, new string[0]); },
                Throws.Nothing);
        }

        // ---------- what this cannot do ----------

        // Stated as a test so nobody later reads a green run as proof of the load fix.
        [Test]
        public void ThisProcessCannotReproduceTheFailureBecauseItHasTheRedirects()
        {
            string config = AppDomain.CurrentDomain.SetupInformation.ConfigurationFile;

            Assert.That(config, Is.Not.Null.And.Not.Empty);
            Assert.That(File.Exists(config), Is.True,
                "with no config this process would fail the way the add-in did, and these "
                    + "tests would be proving something. It has one, so they are not.");

            // And the assembly the add-in could not load is loadable here.
            Assert.That(
                delegate { Assembly.Load("System.Numerics.Vectors, Version=4.1.3.0, "
                    + "Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"); },
                Throws.Nothing,
                "this process resolves the very version the add-in could not");
        }
    }
}
