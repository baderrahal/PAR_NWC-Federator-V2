using System;
using System.Globalization;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Federator.Core.Diagnostics
{
    /// <summary>
    /// The SHA-256 of a file's bytes, F127, read once per run for the clash matrix and named
    /// in the COVERAGE block and on the sheet, section 1.7 of the F127 design, which the loop keeps outside the repo as turn5\f127-design.md. Set 03's C06
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

            using (FileStream stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                return Sha256(stream);
            }
        }

        /// <summary>
        /// The fingerprint of the bytes a stream holds, the same 64 characters, for a file built into the tool,
        /// F155, which has no path, MatrixPack.Sha256. The caller opens and closes the stream.
        /// </summary>
        public static string Sha256(Stream stream)
        {
            if (stream == null)
            {
                throw new ArgumentNullException("stream");
            }

            byte[] hash;

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
