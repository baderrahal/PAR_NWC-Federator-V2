using System;
using System.IO;
using Federator.Core.Diagnostics;
using NUnit.Framework;

namespace Federator.Core.Tests
{
    /// <summary>
    /// Every picker remembers its own last folder. One shared last folder is worse than
    /// none, because picking an NWD folder would then move the source picker.
    /// </summary>
    [TestFixture]
    public class FolderMemoryTests
    {
        private string root;
        private string file;

        [SetUp]
        public void MakeFolder()
        {
            root = Path.Combine(Path.GetTempPath(), "FederatorFolders", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(root);
            file = Path.Combine(root, FolderMemory.FileName);
        }

        [TearDown]
        public void RemoveFolder()
        {
            try
            {
                if (Directory.Exists(root))
                {
                    Directory.Delete(root, true);
                }
            }
            catch (IOException)
            {
                // A leftover temp folder is not worth failing a test over.
            }
        }

        private string Make(string name)
        {
            string path = Path.Combine(root, name);
            Directory.CreateDirectory(path);
            return path;
        }

        // ---------- per picker, not one shared ----------

        [Test]
        public void EachPickerRemembersItsOwn()
        {
            FolderMemory memory = FolderMemory.Load(file);

            memory.Remember(PickerKind.Source, @"C:\in\nwc");
            memory.Remember(PickerKind.Nwf, @"C:\out\nwf");
            memory.Remember(PickerKind.Nwd, @"C:\out\nwd");
            memory.Remember(PickerKind.Excel, @"C:\out\reports");
            memory.Remember(PickerKind.ClashXml, @"C:\in\clash");

            Assert.That(memory.LastFor(PickerKind.Source), Is.EqualTo(@"C:\in\nwc"));
            Assert.That(memory.LastFor(PickerKind.Nwf), Is.EqualTo(@"C:\out\nwf"));
            Assert.That(memory.LastFor(PickerKind.Nwd), Is.EqualTo(@"C:\out\nwd"));
            Assert.That(memory.LastFor(PickerKind.Excel), Is.EqualTo(@"C:\out\reports"));
            Assert.That(memory.LastFor(PickerKind.ClashXml), Is.EqualTo(@"C:\in\clash"));
        }

        [Test]
        public void RememberingOneDoesNotMoveTheOthers()
        {
            FolderMemory memory = FolderMemory.Load(file);
            memory.Remember(PickerKind.Source, @"C:\in\nwc");
            memory.Remember(PickerKind.Nwd, @"C:\out\nwd");

            Assert.That(memory.LastFor(PickerKind.Source), Is.EqualTo(@"C:\in\nwc"),
                "picking an NWD folder moved the source picker");
            Assert.That(memory.LastFor(PickerKind.Nwf), Is.EqualTo(string.Empty));
        }

        // ---------- it survives a restart ----------

        // The one the brief asks for by name.
        [Test]
        public void WhatWasRememberedSurvivesARestart()
        {
            FolderMemory before = FolderMemory.Load(file);
            before.Remember(PickerKind.Source, @"C:\in\nwc");
            before.Remember(PickerKind.Excel, @"C:\out\reports");

            Assert.That(File.Exists(file), Is.True, "nothing was written, so nothing can survive");

            // A new window, a new process, the same file.
            FolderMemory after = FolderMemory.Load(file);

            Assert.That(after.LastFor(PickerKind.Source), Is.EqualTo(@"C:\in\nwc"));
            Assert.That(after.LastFor(PickerKind.Excel), Is.EqualTo(@"C:\out\reports"));
            Assert.That(after.LastFor(PickerKind.Nwf), Is.EqualTo(string.Empty));
        }

        [Test]
        public void NothingRememberedYetIsNotAFailure()
        {
            FolderMemory memory = FolderMemory.Load(file);

            Assert.That(memory.LastFor(PickerKind.Source), Is.EqualTo(string.Empty));
            Assert.That(memory.OpenAt(PickerKind.Source), Is.EqualTo(string.Empty));
            Assert.That(string.Join("\n", new System.Collections.Generic.List<string>(memory.Lines()).ToArray()),
                Does.Contain("Nothing remembered yet"));
        }

        [Test]
        public void AFileIsRememberedAsItsFolder()
        {
            string real = Path.Combine(root, "picked.xml");
            File.WriteAllText(real, "x");

            FolderMemory memory = FolderMemory.Load(file);
            memory.Remember(PickerKind.ClashXml, real);

            Assert.That(memory.LastFor(PickerKind.ClashXml), Is.EqualTo(root),
                "the clash picker should reopen where the file lives");
        }

        // ---------- a folder that has gone falls back ----------

        // The other one the brief asks for by name.
        [Test]
        public void AFolderThatIsGoneOpensAtItsNearestExistingParent()
        {
            string parent = Make("still here");
            string child = Path.Combine(parent, "gone", "deeper");

            FolderMemory memory = FolderMemory.Load(file);
            memory.Remember(PickerKind.Nwf, child);

            Assert.That(memory.LastFor(PickerKind.Nwf), Is.EqualTo(child),
                "what was remembered is kept exactly as it was");
            Assert.That(memory.OpenAt(PickerKind.Nwf), Is.EqualTo(parent),
                "it should open at the nearest folder that is still there");
        }

        [Test]
        public void AFolderStillThereOpensAtItself()
        {
            string real = Make("real");

            FolderMemory memory = FolderMemory.Load(file);
            memory.Remember(PickerKind.Source, real);

            Assert.That(memory.OpenAt(PickerKind.Source), Is.EqualTo(real));
        }

        [Test]
        public void NearestExistingWalksUpUntilItFindsSomething()
        {
            string real = Make("real");

            Assert.That(FolderMemory.NearestExisting(real), Is.EqualTo(real));
            Assert.That(FolderMemory.NearestExisting(Path.Combine(real, "a")), Is.EqualTo(real));
            Assert.That(FolderMemory.NearestExisting(Path.Combine(real, "a", "b", "c")),
                Is.EqualTo(real));
        }

        [Test]
        public void ADriveThatIsNotThereFallsAllTheWayBackToNothing()
        {
            Assert.That(FolderMemory.NearestExisting(@"Q:\nothing\here"), Is.EqualTo(string.Empty));
        }

        [Test]
        public void NothingAtAllIsNothingRatherThanAThrow()
        {
            Assert.That(FolderMemory.NearestExisting(null), Is.EqualTo(string.Empty));
            Assert.That(FolderMemory.NearestExisting(string.Empty), Is.EqualTo(string.Empty));
            Assert.That(FolderMemory.NearestExisting("   "), Is.EqualTo(string.Empty));
        }

        [Test]
        public void APathWindowsWillNotExpandIsRefusedRatherThanThrowing()
        {
            Assert.That(
                delegate { FolderMemory.NearestExisting("bad|path|chars"); },
                Throws.Nothing);
        }

        [Test]
        public void TheLineSaysWhenARememberedFolderHasGone()
        {
            string parent = Make("here");

            FolderMemory memory = FolderMemory.Load(file);
            memory.Remember(PickerKind.Nwd, Path.Combine(parent, "gone"));

            string block = string.Join(
                "\n", new System.Collections.Generic.List<string>(memory.Lines()).ToArray());

            Assert.That(block, Does.Contain("gone, opening at"));
            Assert.That(block, Does.Contain(parent));
        }

        // ---------- remembering never stops anything ----------

        [Test]
        public void AnUnwritableLocationIsRecordedRatherThanThrown()
        {
            FolderMemory memory = FolderMemory.Load(@"Q:\nowhere\folders.txt");
            memory.Remember(PickerKind.Source, @"C:\in\nwc");

            Assert.That(memory.LastFor(PickerKind.Source), Is.EqualTo(@"C:\in\nwc"),
                "it should still remember for this session");
            Assert.That(memory.Save(), Is.False);
            Assert.That(memory.DisabledReason, Is.Not.Null.And.Not.Empty);
        }

        [Test]
        public void AFileFullOfRubbishIsReadAsNothingRatherThanThrowing()
        {
            File.WriteAllText(file, "this is not a settings file" + Environment.NewLine + "=== nor is this");

            FolderMemory memory = FolderMemory.Load(file);

            Assert.That(memory.LastFor(PickerKind.Source), Is.EqualTo(string.Empty));
        }

        [Test]
        public void AnEmptyFolderIsNotRemembered()
        {
            FolderMemory memory = FolderMemory.Load(file);
            memory.Remember(PickerKind.Source, string.Empty);
            memory.Remember(PickerKind.Nwf, null);
            memory.Remember(PickerKind.Nwd, "   ");

            Assert.That(memory.LastFor(PickerKind.Source), Is.EqualTo(string.Empty));
            Assert.That(memory.LastFor(PickerKind.Nwf), Is.EqualTo(string.Empty));
            Assert.That(memory.LastFor(PickerKind.Nwd), Is.EqualTo(string.Empty));
        }

        [Test]
        public void EveryPickerHasItsOwnSlot()
        {
            Assert.That(FolderMemory.AllKinds().Length, Is.EqualTo(6));
            Assert.That(FolderMemory.AllKinds().Length,
                Is.EqualTo(Enum.GetValues(typeof(PickerKind)).Length),
                "a picker was added and the memory does not know about it");
        }
    }
}
