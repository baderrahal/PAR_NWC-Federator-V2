using System;
using System.Globalization;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Federator.Core.Diagnostics
{
    /// <summary>
    /// The SHA-256 of a file's bytes, F127, read once per run for the picked XML and named
    /// in the COVERAGE block and on the sheet, turn5\f127-design.md section 1.7. Set 03's C06
    /// run carries no hash of the XML it picked, so which bytes it read had to be worked out
    /// from its SET lines, which ask ME-PIPING where the exchange file asks ME-Piping. A
    /// fingerprint says it outright.
    ///
    /// A file that is not there throws, naming the path, so the caller says UNKNOWN in its
    /// own words. Nothing here is caught, because a fingerprint that could not be read is not
    /// a fingerprint.
    /// </summary>
    public static class FileFingerprint
    {
        /// <summary>The fingerprint as 64 lower case hexadecimal characters.</summary>
        public static string Sha256(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                throw new ArgumentException("A fingerprint needs the path of a file.", "path");
            }

            if (!File.Exists(path))
            {
                throw new FileNotFoundException("There is no file at " + path + " to fingerprint.", path);
            }

            byte[] hash;

            using (FileStream stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            using (SHA256 sha = SHA256.Create())
            {
                hash = sha.ComputeHash(stream);
            }

            StringBuilder hex = new StringBuilder(hash.Length * 2);

            foreach (byte b in hash)
            {
                hex.Append(b.ToString("x2", CultureInfo.InvariantCulture));
            }

            return hex.ToString();
        }
    }
}
