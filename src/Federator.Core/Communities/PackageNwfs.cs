using System;
using System.Collections.Generic;
using System.IO;
using Federator.Core.Report;
using Federator.Core.Rerun;

namespace Federator.Core.Communities
{
    /// <summary>One NWF of a package for the NWF page, with the NWD and the Log folder its run writes, under the package's report folder.</summary>
    public sealed class PackageNwf
    {
        internal PackageNwf(Package package, string nwfPath, string nwdPath, string logFolder)
        {
            Package = package;
            NwfPath = nwfPath ?? string.Empty;
            NwdPath = nwdPath ?? string.Empty;
            LogFolder = logFolder ?? string.Empty;
        }

        public Package Package { get; private set; }

        public string NwfPath { get; private set; }

        public string NwdPath { get; private set; }

        public string LogFolder { get; private set; }
    }

    /// <summary>
    /// The NWFs a ticked package runs on the NWF page, F208: the NWF files directly in the NWF folder under its report
    /// folder, where the NWC page wrote them, in name order without case. Each writes its NWD into the package's NWD folder
    /// and its logs and reports into the package's Log folder, the places the NWC page gave the same group, so the two
    /// pages write into the same folders of the same package and nowhere else. A file is judged an NWF by NwfPick.IsNwf
    /// alone and the folder is listed whole, never matched by a wildcard. No NWF folder, or one holding none, gives none.
    /// </summary>
    public static class PackageNwfs
    {
        public static IList<PackageNwf> For(string reportRoot, Package package, NwfPickDisk disk)
        {
            if (package == null)
            {
                throw new ArgumentNullException("package");
            }

            if (disk == null)
            {
                throw new ArgumentNullException("disk");
            }

            List<PackageNwf> found = new List<PackageNwf>();
            string nwfFolder = PackageOutput.NwfFolderFor(reportRoot, package);
            string nwdFolder = PackageOutput.NwdFolderFor(reportRoot, package);

            if (!disk.IsFolder(nwfFolder))
            {
                return found;
            }

            List<string> nwfs = new List<string>();

            foreach (string file in disk.FilesIn(nwfFolder))
            {
                if (NwfPick.IsNwf(file))
                {
                    nwfs.Add(file);
                }
            }

            nwfs.Sort(StringComparer.OrdinalIgnoreCase);
            string logFolder = ReportPaths.LogFolder(nwfFolder, nwdFolder, null);

            foreach (string nwf in nwfs)
            {
                found.Add(new PackageNwf(package, nwf, OutputPaths.Nwd(nwdFolder, OpenDocumentJob.NameFrom(nwf)), logFolder));
            }

            return found;
        }
    }
}
