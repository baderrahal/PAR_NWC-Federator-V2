using System;
using System.Collections.Generic;
using System.IO;
using Federator.Core.Diagnostics;
using NUnit.Framework;

namespace Federator.Core.Tests
{
    /// <summary>
    /// F223, Bader's answer to Q207 of 18 Oct 2026: Auto-Save off while the tool works and put back exactly as it was, even
    /// when a group fails or Navisworks closes. The reading of the registry's value, the keeper of the value to put back on
    /// a fake registry and a fake record, the record's file, and every line the runner and the window write, each broken one
    /// thing at a time: a runner that died, two runs at once, a Navisworks still running, a put back that throws, a switch
    /// that will not read.
    /// </summary>
    [TestFixture]
    public class AutoSaveSwitchTests
    {
        private sealed class FakeRegistry : IAutoSaveRegistry
        {
            public string Value = "0";
            public bool Readable = true;
            public bool WriteThrows;
            public int Writes;

            public bool TryRead(out string value, out string why)
            {
                value = Readable ? Value : null;
                why = Readable ? null : "the key is not there and a key is never made";
                return Readable;
            }

            public void Write(string value)
            {
                if (WriteThrows)
                {
                    throw new UnauthorizedAccessException("denied");
                }

                Writes++;
                Value = value;
            }

            public void Remove()
            {
                Writes++;
                Value = null;
            }
        }

        private sealed class FakeRecord : IAutoSaveRecord
        {
            public string Text;
            public bool WriteThrows;

            public string Read()
            {
                return Text;
            }

            public void Write(string text)
            {
                if (WriteThrows)
                {
                    throw new IOException("the disk is full");
                }

                Text = text;
            }

            public void Delete()
            {
                Text = null;
            }
        }

        [Test]
        public void TheRegistryValueReadsByTheFormsMeasuredAndAnythingElseIsUnknown()
        {
            Assert.That(AutoSaveSwitch.OfRegistryValue("3 0"), Is.EqualTo(AutoSaveState.Off));
            Assert.That(AutoSaveSwitch.OfRegistryValue("3 1"), Is.EqualTo(AutoSaveState.On));
            Assert.That(AutoSaveSwitch.OfRegistryValue("0"), Is.EqualTo(AutoSaveState.NeverSet), "a value never set is Navisworks' default, on");
            Assert.That(AutoSaveSwitch.OfRegistryValue(null), Is.EqualTo(AutoSaveState.Unknown));
            Assert.That(AutoSaveSwitch.OfRegistryValue("3 0 "), Is.EqualTo(AutoSaveState.Unknown), "never trimmed");
            Assert.That(AutoSaveSwitch.Words(AutoSaveState.NeverSet), Does.Contain("never set, Navisworks' own default"));
        }

        [Test]
        public void ARunKeepsTheValueOnItsRecordSwitchesOffPerGroupAndPutsItBackOnceAsItEnds()
        {
            FakeRegistry registry = new FakeRegistry();
            FakeRecord record = new FakeRecord();
            List<string> lines = new List<string>();

            AutoSaveKeeper keeper = AutoSaveKeeper.Begin(registry, record, lines.Add);
            Assert.That(keeper.Known, Is.True);
            Assert.That(record.Text, Is.EqualTo(AutoSaveKeeper.RecordText("0")), "the record is written before anything is switched");
            Assert.That(lines[0], Does.Contain("reads '0', never set").And.Contain("kept in " + AutoSaveKeeper.RecordFileName));

            keeper.SwitchOffFor("C06  1B06BC");
            Assert.That(registry.Value, Is.EqualTo("3 0"));
            Assert.That(lines[1], Is.EqualTo("C06  1B06BC: AUTOSAVE the registry's switch read '0', switched off for this group's Navisworks, it reads '3 0'"));

            keeper.ReadAfter("C06  1B06BC");
            keeper.SwitchOffFor("C06  1B06BS");
            Assert.That(lines[3], Does.Contain("off already"));

            Assert.That(keeper.PutBack(false), Is.True);
            Assert.That(registry.Value, Is.EqualTo("0"));
            Assert.That(record.Text, Is.Null, "the record goes once the switch reads as it was");
            Assert.That(lines[4], Is.EqualTo("AUTOSAVE the registry's switch read '3 0', put back to '0', it reads '0', " + AutoSaveKeeper.RecordFileName + " deleted"));
        }

        [Test]
        public void ARunnerThatDiedLeavesItsRecordAndTheNextRunPutsBackThePersonsValueNeverTheOff()
        {
            FakeRegistry registry = new FakeRegistry();
            FakeRecord record = new FakeRecord();

            AutoSaveKeeper died = AutoSaveKeeper.Begin(registry, record, null);
            died.SwitchOffFor("g");
            Assert.That(registry.Value, Is.EqualTo("3 0"), "the runner died here, before its put back");

            List<string> lines = new List<string>();
            AutoSaveKeeper next = AutoSaveKeeper.Begin(registry, record, lines.Add);
            Assert.That(next.Before, Is.EqualTo("0"), "the breaker's first finding: the off a dead run left is never taken as the person's value");
            Assert.That(lines[0], Does.Contain("is there already"));
            next.PutBack(false);
            Assert.That(registry.Value, Is.EqualTo("0"));

            died.SwitchOffFor("g");
            List<string> window = new List<string>();
            Assert.That(AutoSaveKeeper.PutBackWhatARunLeft(registry, record, window.Add, false), Is.True);
            Assert.That(registry.Value, Is.EqualTo("0"));
            Assert.That(record.Text, Is.Null);
            Assert.That(window[0], Does.Contain("a run ended before it put the switch back").And.Contain("put back to '0'"));

            window.Clear();
            Assert.That(AutoSaveKeeper.PutBackWhatARunLeft(registry, record, window.Add, false), Is.True);
            Assert.That(window, Is.Empty, "no record, nothing said");
        }

        [Test]
        public void TwoRunsAtOnceEndOnThePersonsValue()
        {
            FakeRegistry registry = new FakeRegistry();
            FakeRecord record = new FakeRecord();

            AutoSaveKeeper a = AutoSaveKeeper.Begin(registry, record, null);
            a.SwitchOffFor("a1");
            AutoSaveKeeper b = AutoSaveKeeper.Begin(registry, record, null);
            Assert.That(b.Before, Is.EqualTo("0"), "B reads A's record, never A's off");

            a.PutBack(false);
            Assert.That(record.Text, Is.Null);
            b.SwitchOffFor("b2");
            Assert.That(record.Text, Is.EqualTo(AutoSaveKeeper.RecordText("0")), "B writes the record again before it switches");
            b.PutBack(false);
            Assert.That(registry.Value, Is.EqualTo("0"));
            Assert.That(record.Text, Is.Null);
        }

        [Test]
        public void ANavisworksStillRunningKeepsTheRecordAndAFailedPutBackKeepsItToo()
        {
            FakeRegistry registry = new FakeRegistry();
            FakeRecord record = new FakeRecord();
            List<string> lines = new List<string>();

            AutoSaveKeeper keeper = AutoSaveKeeper.Begin(registry, record, lines.Add);
            keeper.SwitchOffFor("g");
            Assert.That(keeper.PutBack(true), Is.True);
            Assert.That(registry.Value, Is.EqualTo("0"));
            Assert.That(record.Text, Is.Not.Null, "a Navisworks of the run may still write the switch as it quits");
            Assert.That(lines[lines.Count - 1], Does.Contain("still runs").And.Contain("is kept"));

            keeper.SwitchOffFor("g");
            registry.WriteThrows = true;
            Assert.That(keeper.PutBack(false), Is.False);
            Assert.That(registry.Value, Is.EqualTo("3 0"));
            Assert.That(record.Text, Is.Not.Null);
            Assert.That(lines[lines.Count - 1], Does.Contain("could not be put back to '0'").And.Contain("UnauthorizedAccessException: denied").And.Contain("is kept"));
        }

        [Test]
        public void AValueThatWasNotThereIsTakenOutAgain()
        {
            FakeRegistry registry = new FakeRegistry { Value = null };
            FakeRecord record = new FakeRecord();

            AutoSaveKeeper keeper = AutoSaveKeeper.Begin(registry, record, null);
            keeper.SwitchOffFor("g");
            Assert.That(registry.Value, Is.EqualTo("3 0"));
            keeper.PutBack(false);
            Assert.That(registry.Value, Is.Null);
            Assert.That(record.Text, Is.Null);
        }

        [Test]
        public void NothingIsSwitchedWhereTheValueOrTheRecordCannotBeKept()
        {
            FakeRegistry unreadable = new FakeRegistry { Readable = false };
            List<string> lines = new List<string>();
            AutoSaveKeeper keeper = AutoSaveKeeper.Begin(unreadable, new FakeRecord(), lines.Add);
            keeper.SwitchOffFor("g");
            Assert.That(keeper.Known, Is.False);
            Assert.That(unreadable.Writes, Is.EqualTo(0));
            Assert.That(lines[0], Does.Contain("could not be read").And.Contain("never written this run"));
            Assert.That(lines[1], Does.Contain("g: AUTOSAVE not switched off"));
            Assert.That(keeper.PutBack(false), Is.True, "nothing switched, nothing to put back");

            FakeRegistry registry = new FakeRegistry();
            AutoSaveKeeper noRecord = AutoSaveKeeper.Begin(registry, new FakeRecord { WriteThrows = true }, null);
            noRecord.SwitchOffFor("g");
            Assert.That(registry.Value, Is.EqualTo("0"), "a switch with no record of what to put back is never made");

            FakeRegistry other = new FakeRegistry();
            AutoSaveKeeper bad = AutoSaveKeeper.Begin(other, new FakeRecord { Text = "not a record" }, null);
            bad.SwitchOffFor("g");
            Assert.That(other.Value, Is.EqualTo("0"), "a record that is not one is left as it is and nothing is switched");
        }

        [Test]
        public void TheRecordReadsBackEveryValueAndRefusesAnythingElse()
        {
            string before;
            string why;

            Assert.That(AutoSaveKeeper.TryReadRecord(AutoSaveKeeper.RecordText("0"), out before, out why), Is.True);
            Assert.That(before, Is.EqualTo("0"));
            Assert.That(AutoSaveKeeper.TryReadRecord(AutoSaveKeeper.RecordText(null), out before, out why), Is.True);
            Assert.That(before, Is.Null);
            Assert.That(AutoSaveKeeper.TryReadRecord(AutoSaveKeeper.RecordText(string.Empty), out before, out why), Is.True);
            Assert.That(before, Is.EqualTo(string.Empty), "an empty value is not no value");
            Assert.That(AutoSaveKeeper.TryReadRecord(AutoSaveKeeper.RecordText("3 0 "), out before, out why), Is.True);
            Assert.That(before, Is.EqualTo("3 0 "), "never trimmed");

            Assert.That(AutoSaveKeeper.TryReadRecord("something else\r\nbefore\tvalue 0", out before, out why), Is.False);
            Assert.That(why, Does.Contain("first line"));
            Assert.That(AutoSaveKeeper.TryReadRecord(AutoSaveKeeper.RecordText("0").Replace("value 0", "0"), out before, out why), Is.False);
        }

        [Test]
        public void TheRecordsFileIsWrittenWholeReadBackAndDeleted()
        {
            string folder = TempFolder.Make("FederatorAutoSaveRecord");

            try
            {
                FileAutoSaveRecord record = new FileAutoSaveRecord(Path.Combine(folder, "tool", AutoSaveKeeper.RecordFileName));
                Assert.That(record.Read(), Is.Null);
                record.Write(AutoSaveKeeper.RecordText("0"));
                record.Write(AutoSaveKeeper.RecordText("3 1"));
                Assert.That(record.Read(), Is.EqualTo(AutoSaveKeeper.RecordText("3 1")));
                Assert.That(File.Exists(Path.Combine(folder, "tool", AutoSaveKeeper.RecordFileName + ".writing")), Is.False);
                record.Delete();
                Assert.That(record.Read(), Is.Null);
                record.Delete();
            }
            finally
            {
                TempFolder.Remove(folder);
            }
        }

        [Test]
        public void TheWindowsLinesSayTheOptionBeforeAndAsItWasPutBack()
        {
            Assert.That(AutoSaveSwitch.AsTheRunStarts(true, null, true, false),
                Is.EqualTo("AUTOSAVE the option general.autosave.enable reads on as the run starts, switched off for the run, the set returned true and it reads off."
                    + " Whether a value set off in a Navisworks started with it on stops its autosaves is UNKNOWN, scan.md 5z-zl"),
                "the reviewer's and the breaker's finding: the line never claims more than 5z-zl measured");
            Assert.That(AutoSaveSwitch.AsTheRunStarts(true, null, true, true), Does.EndWith("so whether this run can autosave is UNKNOWN"));
            Assert.That(AutoSaveSwitch.AsTheRunStarts(true, "COMException: no", null, null), Does.Contain("switching it off threw COMException: no").And.Contain("set back on as the run ends"));
            Assert.That(AutoSaveSwitch.AsTheRunStarts(false, null, null, null), Does.Contain("reads off as the run starts, left as it is"));
            Assert.That(AutoSaveSwitch.AsTheRunStarts(null, "COMException: no", null, null), Does.Contain("could not be read, COMException: no"));

            Assert.That(AutoSaveSwitch.AsTheRunEnds(true, true, true, true, null),
                Is.EqualTo("AUTOSAVE the option general.autosave.enable put back to on as the run ends, the set returned true and it reads on"));
            Assert.That(AutoSaveSwitch.AsTheRunEnds(true, true, true, false, null), Does.Contain("NOT AS IT WAS").And.Contain("in Options"));
            Assert.That(AutoSaveSwitch.AsTheRunEnds(false, false, null, false, null), Does.Contain("was not changed by this run"));
            Assert.That(AutoSaveSwitch.AsTheRunEnds(false, false, null, null, "COMException: gone"), Does.Contain("reading it threw COMException: gone"));
            Assert.That(AutoSaveSwitch.AsTheRunEnds(true, true, null, null, "COMException: gone"), Does.Contain("could not be put back").And.Contain("It read on"));
            Assert.That(AutoSaveSwitch.SessionPutBack(true, true), Does.Contain("set back on").And.EndWith("it reads on"));
        }
    }
}
