using System;
using System.IO;
using Federator.Core.Exchange;
using Federator.Core.Teams;
using NUnit.Framework;

namespace Federator.Core.Tests
{
    /// <summary>
    /// Q123 answered B by Bader on 2026-10-05: the window keeps the last team map used, for runs
    /// with no XML, and the log names it. Kept the way FolderMemory keeps the picker folders, one
    /// file beside the logs holding one full path, read when the window opens.
    /// </summary>
    [TestFixture]
    public class TeamMapMemoryTests
    {
        private const string NothingApplies =
            ". Every discipline code is a team of its own and no pair carries the size folder";

        [Test]
        public void NothingKeptYetMapsNothingAndSaysSo()
        {
            string folder = TempFolder.Make("f131-memory-none");

            try
            {
                TeamMapMemory memory = TeamMapMemory.Load(Path.Combine(folder, TeamMapMemory.FileName));

                Assert.That(memory.LastMap, Is.EqualTo(string.Empty));
                Assert.That(memory.Unread, Is.Null);

                TeamMap map = memory.ForNoXml(new TeamMapSettings());

                Assert.That(map.NoXmlPicked, Is.True);
                Assert.That(map.ListPath, Is.Null);
                Assert.That(map.Teams.Count, Is.EqualTo(0));
                TeamMapTests.Same(map.Lines(), "TEAMS    no clash matrix was read and no team map is kept from a run with one" + NothingApplies);
                Assert.That(TeamMapMemory.FileName, Is.EqualTo("team-map.txt"));
            }
            finally
            {
                TempFolder.Remove(folder);
            }
        }

        /// <summary>
        /// A memory that cannot be read is said, never read in part and never thrown: bytes that
        /// are not UTF-8, a line this does not know, and a path that is not a full one, because a
        /// map is found by one full path tested with File.Exists. A run with no XML then maps
        /// nothing and its TEAMS line says why. Since F155 nothing writes the memory, the pack's map being read.
        /// </summary>
        [Test]
        public void AMemoryThatCannotBeReadIsSaidAndNeverThrown()
        {
            string folder = TempFolder.Make("f131-memory-unread");

            try
            {
                string memoryPath = Path.Combine(folder, TeamMapMemory.FileName);
                string[][] faults =
                {
                    new[] { "kept: a.teams.txt\n", "line 1, \"kept: a.teams.txt\", names a map by a path that is not a full one" },
                    new[] { "kept a.teams.txt\n", "line 1, \"kept a.teams.txt\", is not a line of this memory" },
                    new[] { "kept: " + Path.Combine(folder, "a.teams.txt") + "\nkept: " + Path.Combine(folder, "b.teams.txt") + "\n", "line 2, \"kept: " + Path.Combine(folder, "b.teams.txt") + "\", names a second map" }
                };

                foreach (string[] fault in faults)
                {
                    File.WriteAllText(memoryPath, fault[0]);

                    TeamMapMemory memory = TeamMapMemory.Load(memoryPath);

                    Assert.That(memory.Unread, Is.EqualTo(fault[1]), fault[0]);
                    Assert.That(memory.LastMap, Is.EqualTo(string.Empty), fault[0]);

                    TeamMap map = memory.ForNoXml(new TeamMapSettings());

                    Assert.That(map.Teams.Count, Is.EqualTo(0), fault[0]);
                    // The reviewer's finding on F131's add-in half, K26: the file that could not be
                    // read is the memory, so the line names it as the memory and never as the map.
                    TeamMapTests.Same(
                        map.Lines(),
                        "TEAMS    no clash matrix was read, and THE MEMORY OF THE TEAM MAP KEPT FROM THE LAST RUN WITH ONE, " + memoryPath
                            + ", COULD NOT BE READ: " + fault[1] + NothingApplies);
                }

                File.WriteAllBytes(memoryPath, new byte[] { 0x6B, 0x65, 0x70, 0x74, 0x3A, 0x20, 0xC3, 0x28, 0x0A });
                Assert.That(TeamMapMemory.Load(memoryPath).Unread, Is.EqualTo("it is not UTF-8 text"));
            }
            finally
            {
                TempFolder.Remove(folder);
            }
        }

        /// <summary>
        /// The map a run uses, the one rule the window reads for every run: the map beside the
        /// picked XML, read by ReadPicked, or the kept one where no XML is picked, Q123 answered
        /// B. A document not read by ReadPicked carries no map, and is refused rather than given
        /// one it was never read with.
        /// </summary>
        [Test]
        public void ARunUsesTheMapBesideItsXmlOrTheKeptOneWhereNoneIsPicked()
        {
            string folder = TempFolder.Make("f131-memory-run");

            try
            {
                string xml = Path.Combine(folder, "a.xml");
                string mapPath = Path.Combine(folder, "a.teams.txt");

                File.WriteAllText(xml, "<exchange/>");
                File.WriteAllText(mapPath, TeamMapTests.BadersMap);

                File.WriteAllText(Path.Combine(folder, TeamMapMemory.FileName), "kept: " + mapPath + Environment.NewLine);
                TeamMapMemory memory = TeamMapMemory.Load(Path.Combine(folder, TeamMapMemory.FileName));

                ExchangeDocument picked = MatrixCorrections.ReadPicked(Samples.CorrectedMatrix());

                Assert.That(memory.ForRun(picked, new TeamMapSettings()), Is.SameAs(picked.Teams));

                TeamMap none = memory.ForRun(null, new TeamMapSettings());

                Assert.That(none.NoXmlPicked, Is.True);
                Assert.That(none.ListPath, Is.EqualTo(mapPath));
                Assert.That(none.IsRead, Is.True);

                ExchangeDocument asItStands = new ExchangeReader().ReadText("<?xml version='1.0' encoding='UTF-8'?>\n<exchange units=\"ft\"/>\n");

                Assert.That(() => memory.ForRun(asItStands, new TeamMapSettings()), Throws.ArgumentException);
            }
            finally
            {
                TempFolder.Remove(folder);
            }
        }
    }
}
