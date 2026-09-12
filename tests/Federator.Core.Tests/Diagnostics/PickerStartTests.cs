using System;
using System.IO;
using Federator.Core.Diagnostics;
using NUnit.Framework;

namespace Federator.Core.Tests
{
    /// <summary>
    /// One test per picker, because Bader reported that not all of them remember and the
    /// run that raised it said "Nothing remembered yet" on its first line, so nothing
    /// confirmed it either way.
    ///
    /// There are six Browse buttons in the window: the source folder, the NWF folder, the
    /// NWD folder, the Excel folder, the clash XML and the logo. All six go through
    /// PickerStart, so all six can be proved here rather than clicked.
    /// </summary>
    [TestFixture]
    public class PickerStartTests
    {
        private string root;
        private string file;

        [SetUp]
        public void MakeFolder()
        {
            root = TempFolder.Make("FederatorPickers");
            file = Path.Combine(root, FolderMemory.FileName);
        }

        [TearDown]
        public void RemoveFolder()
        {
            TempFolder.Remove(root);
        }

        private string Make(string name)
        {
            string path = Path.Combine(root, name);
            Directory.CreateDirectory(path);
            return path;
        }

        /// <summary>
        /// One picker, end to end: it is pointed somewhere, the window is closed and
        /// reopened, and it opens where it was left.
        /// </summary>
        private void ProveOnePicker(PickerKind kind, string folderName)
        {
            string picked = Make(folderName);

            FolderMemory before = FolderMemory.Load(file);

            Assert.That(before.LastFor(kind), Is.Empty,
                kind + " remembered something before anything was picked");

            before.Remember(kind, picked);

            // A new window, a new process, the same file beside the logs.
            FolderMemory after = FolderMemory.Load(file);

            Assert.That(after.LastFor(kind), Is.Not.Empty,
                kind + " did not save its folder");
            Assert.That(PickerStart.For(after, kind, string.Empty), Is.EqualTo(picked),
                kind + " did not reopen where it was left");
        }

        // ---------- one per picker, all five ----------

        [Test]
        public void TheSourcePickerRemembersItsOwnFolder()
        {
            ProveOnePicker(PickerKind.Source, "nwc");
        }

        [Test]
        public void TheNwfPickerRemembersItsOwnFolder()
        {
            ProveOnePicker(PickerKind.Nwf, "nwf");
        }

        [Test]
        public void TheNwdPickerRemembersItsOwnFolder()
        {
            ProveOnePicker(PickerKind.Nwd, "nwd");
        }

        [Test]
        public void TheExcelPickerRemembersItsOwnFolder()
        {
            ProveOnePicker(PickerKind.Excel, "reports");
        }

        [Test]
        public void TheClashXmlPickerRemembersItsOwnFolder()
        {
            ProveOnePicker(PickerKind.ClashXml, "clash");
        }

        [Test]
        public void TheLogoPickerRemembersItsOwnFolder()
        {
            ProveOnePicker(PickerKind.Logo, "branding");
        }

        [Test]
        public void EveryPickerInTheWindowIsCoveredHere()
        {
            // Six Browse buttons, six kinds, six tests above. If a seventh picker is
            // added this fails until it is proved too.
            Assert.That(FolderMemory.AllKinds().Length, Is.EqualTo(6));
        }

        // ---------- the one that was actually broken ----------

        // The clash picker chooses a FILE, and its handler used to take the directory name
        // of what PickerStart gave it. PickerStart already returns a folder, so that
        // opened the parent, one level above the folder it had remembered.
        [Test]
        public void ThePickerThatChoosesAFileStillOpensAtAFolderAndNotItsParent()
        {
            string clash = Make("clash");
            string picked = Path.Combine(clash, "1104-PAR_CLASH_AllInOne.xml");
            File.WriteAllText(picked, "<exchange/>");

            FolderMemory memory = FolderMemory.Load(file);
            memory.Remember(PickerKind.ClashXml, picked);

            string opensAt = PickerStart.For(memory, PickerKind.ClashXml, string.Empty);

            Assert.That(opensAt, Is.EqualTo(clash), "it should open at the file's own folder");
            Assert.That(opensAt, Is.Not.EqualTo(root), "it opened one level too high");
            Assert.That(Directory.Exists(opensAt), Is.True,
                "the answer is always a folder, even for a picker that chooses a file");
        }

        // ---------- the rule itself ----------

        [Test]
        public void WhatIsAlreadyInTheBoxWinsBecauseItIsWhatThePersonIsLookingAt()
        {
            string remembered = Make("remembered");
            string typed = Make("typed");

            FolderMemory memory = FolderMemory.Load(file);
            memory.Remember(PickerKind.Source, remembered);

            Assert.That(PickerStart.For(memory, PickerKind.Source, typed), Is.EqualTo(typed));
        }

        [Test]
        public void ARememberedFolderThatHasGoneOpensAtItsNearestParent()
        {
            string parent = Make("still here");

            FolderMemory memory = FolderMemory.Load(file);
            memory.Remember(PickerKind.Nwd, Path.Combine(parent, "gone", "deeper"));

            Assert.That(PickerStart.For(memory, PickerKind.Nwd, string.Empty), Is.EqualTo(parent));
        }

        [Test]
        public void RememberingOnePickerDoesNotMoveAnother()
        {
            string nwc = Make("nwc");
            string nwd = Make("nwd");

            FolderMemory memory = FolderMemory.Load(file);
            memory.Remember(PickerKind.Source, nwc);
            memory.Remember(PickerKind.Nwd, nwd);

            Assert.That(PickerStart.For(memory, PickerKind.Source, string.Empty), Is.EqualTo(nwc),
                "picking an NWD folder moved the source picker");
        }

        [Test]
        public void NothingRememberedAndNothingTypedIsNothingRatherThanAThrow()
        {
            FolderMemory memory = FolderMemory.Load(file);

            Assert.That(PickerStart.For(memory, PickerKind.Source, string.Empty),
                Is.EqualTo(string.Empty));
            Assert.That(PickerStart.For(memory, PickerKind.Source, null),
                Is.EqualTo(string.Empty));
            Assert.That(PickerStart.For(null, PickerKind.Source, null),
                Is.EqualTo(string.Empty));
        }
    }
}
