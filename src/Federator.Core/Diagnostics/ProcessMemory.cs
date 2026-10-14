using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;

namespace Federator.Core.Diagnostics
{
    /// <summary>
    /// The memory of the process the tool runs in, F147, Bader's answer to Q155 on 2026-10-12:
    /// logged at the start and the end of each step, so a long run that slows can be read for
    /// which step grew it. Three numbers: the private bytes and the working set as the loop's life
    /// check reads them, Process.PrivateMemorySize64 and WorkingSet64, and the managed heap,
    /// GC.GetTotalMemory without a collection, so reading it never changes it. Each in megabytes
    /// of 1024 squared bytes, the life check's unit.
    ///
    /// A number not read is UNKNOWN and never nought, and the reason it was not read is kept and
    /// said, so a read that fails never stops a run and is never silent.
    /// </summary>
    public sealed class ProcessMemory
    {
        private const double Megabyte = 1024d * 1024;

        public ProcessMemory(long privateBytes, long workingSet, long managedBytes, string notReadBecause)
            : this(privateBytes, workingSet, managedBytes, -1, -1, notReadBecause)
        {
        }

        public ProcessMemory(
            long privateBytes, long workingSet, long managedBytes, long gdiObjects, long userObjects, string notReadBecause)
        {
            PrivateBytes = privateBytes;
            WorkingSet = workingSet;
            ManagedBytes = managedBytes;
            GdiObjects = gdiObjects;
            UserObjects = userObjects;
            NotReadBecause = notReadBecause;
        }

        /// <summary>The process's GDI object count, or below zero where not read. Windows ends a process's drawing at 10000 by default.</summary>
        public long GdiObjects { get; private set; }

        /// <summary>The process's USER object count, or below zero where not read.</summary>
        public long UserObjects { get; private set; }

        [DllImport("user32.dll")]
        private static extern uint GetGuiResources(IntPtr process, uint flags);

        [DllImport("kernel32.dll")]
        private static extern IntPtr GetCurrentProcess();

        /// <summary>This process's GDI object count now, or below zero where it cannot be read. Cheap, one call, no allocation.</summary>
        public static long GdiObjectsNow()
        {
            try
            {
                return GetGuiResources(GetCurrentProcess(), GdiFlag);
            }
            catch (Exception error)
            {
                if (!(error is DllNotFoundException || error is EntryPointNotFoundException))
                {
                    throw;
                }

                return -1;
            }
        }

        private const uint GdiFlag = 0;

        private const uint UserFlag = 1;

        /// <summary>The process's private bytes, or below zero where not read.</summary>
        public long PrivateBytes { get; private set; }

        /// <summary>The process's working set, or below zero where not read.</summary>
        public long WorkingSet { get; private set; }

        /// <summary>The managed heap, or below zero where not read.</summary>
        public long ManagedBytes { get; private set; }

        /// <summary>Why a number was not read, or null where every one was.</summary>
        public string NotReadBecause { get; private set; }

        /// <summary>
        /// Reads the process the tool runs in now. Never throws for a refused read: the process's
        /// two numbers are UNKNOWN where Windows refused them, the managed heap is read either way,
        /// and the reason is kept for the line.
        /// </summary>
        public static ProcessMemory Read()
        {
            long privateBytes = -1;
            long workingSet = -1;
            long gdi = -1;
            long user = -1;
            string why = null;

            try
            {
                using (Process self = Process.GetCurrentProcess())
                {
                    privateBytes = self.PrivateMemorySize64;
                    workingSet = self.WorkingSet64;
                    gdi = ReadGui(self, GdiFlag);
                    user = ReadGui(self, UserFlag);
                }
            }
            catch (Exception error)
            {
                if (!(error is Win32Exception || error is InvalidOperationException || error is NotSupportedException))
                {
                    throw;
                }

                why = error.GetType().Name + ": " + error.Message;
            }

            return new ProcessMemory(privateBytes, workingSet, GC.GetTotalMemory(false), gdi, user, why);
        }

        /// <summary>One GUI object count of a process, or below zero where Windows refuses it or the call is not there.</summary>
        private static long ReadGui(Process self, uint flag)
        {
            try
            {
                return GetGuiResources(self.Handle, flag);
            }
            catch (Exception error)
            {
                if (!(error is DllNotFoundException || error is EntryPointNotFoundException
                    || error is Win32Exception || error is InvalidOperationException))
                {
                    throw;
                }

                return -1;
            }
        }

        /// <summary>
        /// The line the pictures write, F157: the three numbers of the step lines and the GDI and USER
        /// object counts, which are what a run that draws thousands of pictures can run out of. The
        /// when says which moment, such as the start of the pictures, a count reached or a failure.
        /// </summary>
        public static string PicturesLine(string when, ProcessMemory at)
        {
            return "MEMORY   PICTURES " + Words.Or(when, "UNKNOWN") + ", "
                + "private bytes " + Megabytes(Of(at, m => m.PrivateBytes)) + ", "
                + "working set " + Megabytes(Of(at, m => m.WorkingSet)) + ", "
                + "managed heap " + Megabytes(Of(at, m => m.ManagedBytes)) + ", "
                + "GDI objects " + Count(Of(at, m => m.GdiObjects)) + ", "
                + "USER objects " + Count(Of(at, m => m.UserObjects))
                + Why(at);
        }

        private static string Count(long number)
        {
            return number < 0 ? "UNKNOWN" : number.ToString(CultureInfo.InvariantCulture);
        }

        /// <summary>The line as a step starts, under its STEP started line.</summary>
        public static string StartLine(string step, ProcessMemory at)
        {
            return "MEMORY   at the start of " + Words.Or(step, "UNKNOWN") + ", "
                + "private bytes " + Megabytes(Of(at, m => m.PrivateBytes)) + ", "
                + "working set " + Megabytes(Of(at, m => m.WorkingSet)) + ", "
                + "managed heap " + Megabytes(Of(at, m => m.ManagedBytes))
                + Why(at);
        }

        /// <summary>
        /// The line as a step ends, before its STEP finished line: each number now and how far it
        /// moved since the step started, UNKNOWN where either end was not read.
        /// </summary>
        public static string EndLine(string step, ProcessMemory start, ProcessMemory end)
        {
            return "MEMORY   at the end of " + Words.Or(step, "UNKNOWN") + ", "
                + "private bytes " + NowAndMoved(Of(end, m => m.PrivateBytes), Of(start, m => m.PrivateBytes)) + ", "
                + "working set " + NowAndMoved(Of(end, m => m.WorkingSet), Of(start, m => m.WorkingSet)) + ", "
                + "managed heap " + NowAndMoved(Of(end, m => m.ManagedBytes), Of(start, m => m.ManagedBytes))
                + Why(end);
        }

        /// <summary>
        /// One line for a step entered again and again in a group, written where the group's repeated steps
        /// are summed: how far each number moved from the start of the step's FIRST visit to now, so the step
        /// that grows the most is not read as flat off the end line of its first visit alone.
        /// </summary>
        public static string RepeatedLine(string step, int visits, ProcessMemory firstStart, ProcessMemory now)
        {
            return "MEMORY   across the " + visits.ToString(CultureInfo.InvariantCulture) + " visits of "
                + Words.Or(step, "UNKNOWN") + " in this group, from the start of the first to now, "
                + "private bytes " + NowAndMoved(Of(now, m => m.PrivateBytes), Of(firstStart, m => m.PrivateBytes)) + ", "
                + "working set " + NowAndMoved(Of(now, m => m.WorkingSet), Of(firstStart, m => m.WorkingSet)) + ", "
                + "managed heap " + NowAndMoved(Of(now, m => m.ManagedBytes), Of(firstStart, m => m.ManagedBytes))
                + Why(now) + ". The end line of the first visit covers that visit only";
        }

        /// <summary>A number of bytes in megabytes to one place, UNKNOWN where it was not read.</summary>
        private static string Megabytes(long bytes)
        {
            return bytes < 0
                ? "UNKNOWN"
                : (bytes / Megabyte).ToString("0.0", CultureInfo.InvariantCulture) + " MB";
        }

        private static long Of(ProcessMemory memory, Func<ProcessMemory, long> number)
        {
            return memory == null ? -1 : number(memory);
        }

        private static string NowAndMoved(long now, long before)
        {
            string moved;

            if (now < 0 || before < 0)
            {
                moved = "moved UNKNOWN";
            }
            else if (now == before)
            {
                moved = "unchanged";
            }
            else
            {
                moved = (now > before ? "up " : "down ") + Megabytes(Math.Abs(now - before));
            }

            return Megabytes(now) + " " + moved;
        }

        private static string Why(ProcessMemory memory)
        {
            if (memory == null)
            {
                return ", not read";
            }

            return string.IsNullOrEmpty(memory.NotReadBecause)
                ? string.Empty
                : ", UNKNOWN because " + memory.NotReadBecause;
        }
    }
}
