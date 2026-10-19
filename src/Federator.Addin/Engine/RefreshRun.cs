using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Security.Cryptography;
using Autodesk.Navisworks.Api;
using Autodesk.Navisworks.Api.Clash;
using Federator.Core.Clash;
using Federator.Core.Diagnostics;
using Federator.Core.Report;
using Federator.Core.Rerun;
using NavisworksApplication = Autodesk.Navisworks.Api.Application;

namespace Federator.Addin.Engine
{
    /// <summary>
    /// The refresh of an NWF whose NWC files were written again, F187, Bader's order of 14 Oct 2026, task 7. The NWF
    /// page's run opens the NWF where it sits, keeps its tests, search sets and viewpoints, asks Navisworks once to
    /// bring the linked files up to date with the files on disk, and runs each test again in place, so what a
    /// coordinator wrote on a clash stays. This holds the pieces that touch Navisworks and the disk. The rules are
    /// Core's, NwcManifest, NwfBackupName and RefreshComparison.
    ///
    /// WHAT IT NEVER DOES. It removes no model from the tree and appends none again, moves and renames no NWC, and
    /// deletes and overwrites nothing of the person's: the one file it writes is the manifest, by Bader's order, and the
    /// one it copies is the NWF into the Backup folder under the NWF's own name, over the last backup, F243. A linked file that does not resolve
    /// stops the run and is named. Every refusal says why.
    /// </summary>
    internal sealed class RefreshRun
    {
        private readonly RunLog log;
        private readonly List<string> unresolved = new List<string>();
        private EventHandler<FileInteractiveResolvingEventArgs> interactive;
        private EventHandler<FileResolvingEventArgs> resolving;
        private int resolvedCalls;

        internal RefreshRun(RunLog log)
        {
            if (log == null)
            {
                throw new ArgumentNullException("log");
            }

            this.log = log;
        }

        /// <summary>The files Navisworks said it could not resolve while the NWF opened, each once.</summary>
        internal IList<string> Unresolved
        {
            get { return unresolved; }
        }

        /// <summary>
        /// Copies the NWF into the Backup folder beside the Log folder before anything is opened. Since F243, Bader's order
        /// of 19 Oct 2026, ONE copy under the NWF's own name, overwritten each time, the default until he answers Q217.
        /// Returns the path, or null with why on the log, and a backup that could not be made stops the refresh, since
        /// nothing may change an NWF that has no copy. A backup another program holds open is not written under another
        /// name: the one line of FileInUse is said and the refresh of this NWF stops as for any backup not made. Where the
        /// NWF itself is the file held, the line names the NWF, read by opening it before the copy, attempt 2.
        /// </summary>
        internal string BackupTheNwf(string nwfPath, string logFolder)
        {
            string target = null;
            bool reading = false;

            try
            {
                if (!File.Exists(nwfPath))
                {
                    log.Line("REFRESH  no backup, " + nwfPath + " is not there");
                    return null;
                }

                string folder = NwfBackupName.FolderBeside(logFolder);

                if (folder == null)
                {
                    log.Line("REFRESH  no backup, there is no Log folder to put a Backup folder beside");
                    return null;
                }

                target = NwfBackupName.PathFor(nwfPath, folder);
                Directory.CreateDirectory(folder);

                // Attempt 2, the breaker's W6. The NWF is opened for reading first, with the sharing the copy reads it with,
                // so a held NWF is the file named, and only a refusal after it is the backup's.
                reading = true;

                using (new FileStream(nwfPath, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                }

                reading = false;
                File.Copy(nwfPath, target, true);

                long size = new FileInfo(target).Length;
                log.Line("REFRESH  backup   " + target + "  " + size.ToString("#,##0", CultureInfo.InvariantCulture) + " bytes, a copy of "
                    + nwfPath + " made before anything was opened, over the one backup of this NWF");
                return target;
            }
            catch (Exception error)
            {
                // Attempt 3, the reviewer's N3: the NWF held is not read, the backup held is not written.
                if (target != null && FileInUse.IsInUse(error))
                {
                    if (reading)
                    {
                        log.NotReadWhileOpen(nwfPath);
                    }
                    else
                    {
                        log.NotWrittenWhileOpen(target);
                    }

                    return null;
                }

                log.Failure(
                    "copying the NWF into the Backup folder before the refresh",
                    error,
                    "stopped, an NWF with no copy is not refreshed");
                return null;
            }
        }

        /// <summary>
        /// Listens for the files Navisworks cannot resolve while the NWF opens. The dialog it would show is answered
        /// with Ignore, so the model stays in the tree and the open goes on, and the file is named here. Every call
        /// is logged as FileInteractiveResolving, with what refers to it and what it points at.
        /// </summary>
        internal void WatchTheLinks()
        {
            interactive = delegate (object sender, FileInteractiveResolvingEventArgs e)
            {
                try
                {
                    string what = e.FileReference ?? string.Empty;

                    if (!unresolved.Contains(what))
                    {
                        unresolved.Add(what);
                    }

                    log.Line("REFRESH  FileInteractiveResolving  " + what + ", referred to by "
                        + (e.ReferringFileName ?? "UNKNOWN") + ", answered Ignore so the model stays in the tree");
                    e.Handled = true;
                    e.Response = FileResolutionResponse.Ignore;
                }
                catch (Exception)
                {
                    // An event handler that throws into Navisworks is worse than a file not named.
                }
            };

            resolving = delegate (object sender, FileResolvingEventArgs e)
            {
                resolvedCalls++;
            };

            NavisworksApplication.FileInteractiveResolving += interactive;
            NavisworksApplication.FileResolving += resolving;
        }

        internal void StopWatching()
        {
            if (resolvedCalls > 0)
            {
                log.Line("REFRESH  FileResolving fired " + resolvedCalls + " times while the NWF opened, one for each file it links and each file those link");
                resolvedCalls = 0;
            }

            if (interactive != null)
            {
                NavisworksApplication.FileInteractiveResolving -= interactive;
                interactive = null;
            }

            if (resolving != null)
            {
                NavisworksApplication.FileResolving -= resolving;
                resolving = null;
            }
        }

        /// <summary>The files the NWF links that are not on disk, by their path as the model reports it, and the ones the event named.</summary>
        internal IList<string> MissingFiles(Document document)
        {
            List<string> missing = new List<string>();

            foreach (Model model in document.Models)
            {
                string path = model.FileName;

                if (!string.IsNullOrEmpty(path) && !File.Exists(path) && !missing.Contains(path))
                {
                    missing.Add(path);
                }
            }

            foreach (string name in unresolved)
            {
                if (name.Length > 0 && !missing.Contains(name) && !File.Exists(name))
                {
                    missing.Add(name);
                }
            }

            return missing;
        }

        /// <summary>Every file the document links, with its size and last write time read off the disk.</summary>
        internal static IList<ManifestEntry> LinkedFiles(Document document)
        {
            List<ManifestEntry> files = new List<ManifestEntry>();
            HashSet<string> seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (Model model in document.Models)
            {
                string path = model.FileName;

                if (string.IsNullOrEmpty(path) || !seen.Add(path) || !File.Exists(path))
                {
                    continue;
                }

                FileInfo info = new FileInfo(path);
                files.Add(new ManifestEntry(path, info.Length, info.LastWriteTimeUtc, null));
            }

            return files;
        }

        /// <summary>The SHA-256 of a file in hex, or null where it cannot be read.</summary>
        internal static string Sha256Of(string path)
        {
            try
            {
                using (FileStream stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                using (SHA256 sha = SHA256.Create())
                {
                    byte[] hash = sha.ComputeHash(stream);
                    return BitConverter.ToString(hash).Replace("-", string.Empty);
                }
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>The last manifest in the Log folder, or null where there is none or it does not read.</summary>
        internal NwcManifest ReadLastManifest(string logFolder)
        {
            try
            {
                string path = ManifestPath(logFolder);

                if (path == null || !File.Exists(path))
                {
                    return null;
                }

                return NwcManifest.Parse(File.ReadAllLines(path));
            }
            catch (Exception error)
            {
                log.Failure("reading the manifest of the linked files", error, "kept going, every file is taken as first seen");
                return null;
            }
        }

        /// <summary>
        /// Writes the manifest of the files linked now, to its one fixed name in the Log folder, overwriting the last,
        /// which is the tool's own state file and the one file of the Log folder the tool overwrites by Bader's order.
        /// </summary>
        internal void WriteManifest(Document document, string logFolder, NwcManifest withHashes, int outsideDeleted)
        {
            try
            {
                string path = ManifestPath(logFolder);

                if (path == null)
                {
                    return;
                }

                NwcManifest next = withHashes;

                if (next == null)
                {
                    next = new NwcManifest();

                    foreach (ManifestEntry file in LinkedFiles(document))
                    {
                        next.Add(new ManifestEntry(file.Path, file.Size, file.LastWriteUtc, Sha256Of(file.Path)));
                    }
                }

                // F188. What the NWF page says before it opens anything: the views under the tool folder now, and the
                // tests. Read here, once the run has done its work, and kept in the same file.
                try
                {
                    int toolViews = 0;

                    foreach (Federator.Core.Views.ViewNode node in Federator.Core.Views.ToolViewFolder.NodesUnder(SavedViewpoints.ReadTree(document, null).Nodes))
                    {
                        if (!node.IsFolder)
                        {
                            toolViews++;
                        }
                    }

                    next.SetFact("toolviews", toolViews);

                    if (outsideDeleted >= 0)
                    {
                        next.SetFact("outside", outsideDeleted);
                    }

                    next.SetFact("tests", DocumentCensusReader.Read(document).Of(CensusCount.Tests));
                }
                catch (Exception error)
                {
                    log.Failure("counting the tool views and the tests for the manifest", error, "kept going, the page says UNKNOWN for them");
                }

                Directory.CreateDirectory(logFolder);
                File.WriteAllLines(path, next.ToLines());
                log.Line("REFRESH  manifest " + path + "  " + next.Entries.Count + " linked files, size, last write time and hash"
                    + ", written for the next refresh to compare with");
            }
            catch (Exception error)
            {
                log.Failure("writing the manifest of the linked files", error, "kept going, the next refresh takes every file as first seen");
            }
        }

        private static string ManifestPath(string logFolder)
        {
            return string.IsNullOrEmpty(logFolder) ? null : Path.Combine(logFolder, NwcManifest.FileName);
        }

        /// <summary>
        /// Asks Navisworks once to bring every linked file up to date with the file on disk, Document.UpdateFiles,
        /// the API's form of Refresh. The units of the document and the transform of each model are read before and
        /// after and said, which is what answers whether they stay. Returns what UpdateFiles returned.
        /// </summary>
        internal bool UpdateTheFiles(Document document)
        {
            string before = UnitsAndOrigins(document);
            log.Line("REFRESH  before UpdateFiles  " + before);

            Stopwatch clock = Stopwatch.StartNew();
            bool done;

            try
            {
                done = document.UpdateFiles();
            }
            catch (Exception error)
            {
                log.Failure("Document.UpdateFiles", error, "stopped, the linked files were not brought up to date");
                return false;
            }

            string after = UnitsAndOrigins(document);
            log.Line("REFRESH  UpdateFiles returned " + done + " after " + clock.Elapsed.TotalSeconds.ToString("0.0", CultureInfo.InvariantCulture) + " s");
            log.Line("REFRESH  after UpdateFiles   " + after);
            log.Line("REFRESH  units and origin " + (string.Equals(before, after, StringComparison.Ordinal)
                ? "are as they were before UpdateFiles"
                : "DIFFER before and after UpdateFiles, said above"));

            try
            {
                bool closed = document.TryCloseFileHandles();
                log.Line("REFRESH  TryCloseFileHandles returned " + closed + ", so a new export may overwrite a linked NWC");
            }
            catch (Exception error)
            {
                log.Failure("Document.TryCloseFileHandles", error, "kept going, a linked NWC may stay locked by this session");
            }

            return done;
        }

        private static string UnitsAndOrigins(Document document)
        {
            List<string> parts = new List<string>();
            parts.Add("document units " + document.Units);

            foreach (Model model in document.Models)
            {
                string transform;

                try
                {
                    using (Transform3D t = model.Transform)
                    {
                        Vector3D shift = t.Translation;
                        transform = shift.X.ToString("0.###", CultureInfo.InvariantCulture) + " "
                            + shift.Y.ToString("0.###", CultureInfo.InvariantCulture) + " "
                            + shift.Z.ToString("0.###", CultureInfo.InvariantCulture);
                    }
                }
                catch (Exception)
                {
                    transform = "UNKNOWN";
                }

                parts.Add(Path.GetFileName(model.FileName ?? string.Empty) + " " + model.Units + " at " + transform);
            }

            return string.Join("; ", parts.ToArray());
        }

        /// <summary>
        /// Every clash result of every test as it stands, one pass down the tests tree: status, assigned to, comments,
        /// approver and the groups it sits in. A result the walk cannot read is left out and counted by the caller as
        /// the difference of the totals. Never throws into the run.
        /// </summary>
        internal ResultSnapshot Snapshot(Document document)
        {
            ResultSnapshot snapshot = new ResultSnapshot();

            try
            {
                DocumentClashTests tests = document.GetClash().TestsData;
                WalkTests(tests.Tests, snapshot);
            }
            catch (Exception error)
            {
                log.Failure("reading the clash results for the refresh snapshot", error, "kept going, the comparison after the run says it has no snapshot");
            }

            return snapshot;
        }

        private static void WalkTests(SavedItemCollection items, ResultSnapshot snapshot)
        {
            if (items == null)
            {
                return;
            }

            for (int i = 0; i < items.Count; i++)
            {
                using (SavedItem item = items[i])
                {
                    ClashTest test = item as ClashTest;

                    if (test != null)
                    {
                        string name = test.DisplayName ?? string.Empty;
                        snapshot.AddTest(name);
                        WalkResults(test.Children, name, string.Empty, snapshot);
                        continue;
                    }

                    GroupItem folder = item as GroupItem;

                    if (folder != null)
                    {
                        WalkTests(folder.Children, snapshot);
                    }
                }
            }
        }

        private static void WalkResults(SavedItemCollection children, string test, string group, ResultSnapshot snapshot)
        {
            if (children == null)
            {
                return;
            }

            for (int i = 0; i < children.Count; i++)
            {
                using (SavedItem child = children[i])
                {
                    ClashResultGroup inner = child as ClashResultGroup;

                    if (inner != null)
                    {
                        string name = inner.DisplayName ?? string.Empty;
                        WalkResults(inner.Children, test, group.Length == 0 ? name : group + "/" + name, snapshot);
                        continue;
                    }

                    ClashResult result = child as ClashResult;

                    if (result == null)
                    {
                        continue;
                    }

                    List<string> comments = new List<string>();

                    try
                    {
                        CommentCollection all = result.Comments;

                        for (int c = 0; c < all.Count; c++)
                        {
                            using (Comment comment = all[c])
                            {
                                comments.Add(comment == null ? string.Empty : comment.Body ?? string.Empty);
                            }
                        }
                    }
                    catch (Exception)
                    {
                        comments.Add("UNKNOWN, the comments did not read");
                    }

                    snapshot.Add(new ResultRecord(
                        test,
                        result.DisplayName,
                        result.Guid.ToString(),
                        (ClashStatus)(int)result.Status,
                        result.AssignedTo,
                        comments,
                        result.ApprovedBy,
                        group));
                }
            }
        }
    }
}
