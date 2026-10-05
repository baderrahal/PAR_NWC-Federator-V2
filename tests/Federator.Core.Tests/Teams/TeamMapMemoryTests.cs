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

        private const string KeptLine = "TEAMS    this map is now the one kept for a run with no clash XML, remembered in ";

        private const string NotWhole = ", because this run's map was not read whole with a team";

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
                TeamMapTests.Same(map.Lines(), "TEAMS    no clash XML was picked and no team map is kept from a run with one" + NothingApplies);
                Assert.That(TeamMapMemory.FileName, Is.EqualTo("team-map.txt"));
            }
            finally
            {
                TempFolder.Remove(folder);
            }
        }

        /// <summary>
        /// A map read whole with a team is kept by its full path, written into a folder that is
        /// made where it is not there yet, and the next window reads it for a run with no XML.
        /// The same map used again stays kept and says so.
        /// </summary>
        [Test]
        public void AMapReadWholeIsKeptAndReadForTheNextRunWithNoXml()
        {
            string folder = TempFolder.Make("f131-memory-kept");

            try
            {
                string xml = Path.Combine(folder, "a.xml");
                string mapPath = Path.Combine(folder, "a.teams.txt");
                string memoryPath = Path.Combine(Path.Combine(folder, "logs"), TeamMapMemory.FileName);

                File.WriteAllText(xml, "<exchange/>");
                File.WriteAllText(mapPath, TeamMapTests.BadersMap);

                TeamMapMemory memory = TeamMapMemory.Load(memoryPath);
                TeamMap used = TeamMap.Beside(xml, new TeamMapSettings());

                Assert.That(memory.Remember(used), Is.EqualTo(KeptLine + memoryPath));
                Assert.That(File.Exists(memoryPath), Is.True);
                Assert.That(memory.LastMap, Is.EqualTo(mapPath));

                TeamMapMemory again = TeamMapMemory.Load(memoryPath);

                Assert.That(again.Unread, Is.Null);
                Assert.That(again.LastMap, Is.EqualTo(mapPath));

                TeamMap kept = again.ForNoXml(new TeamMapSettings());

                Assert.That(kept.NoXmlPicked, Is.True);
                Assert.That(kept.IsRead, Is.True);
                Assert.That(kept.ListPath, Is.EqualTo(mapPath));
                Assert.That(kept.TeamOf("EV"), Is.EqualTo("Electrical"));
                Assert.That(kept.WindowLine(), Is.EqualTo("Teams: no XML picked, 4 read from the kept map"));

                Assert.That(again.Remember(used), Is.EqualTo("TEAMS    this map stays the one kept for a run with no clash XML"));
            }
            finally
            {
                TempFolder.Remove(folder);
            }
        }

        /// <summary>
        /// A map missing, unread, holding no team, or the kept map itself in a run with no XML,
        /// is never kept, so one bad pick never takes the teams away from the next run with no
        /// XML. The line names the map that stays, or that none is kept.
        /// </summary>
        [Test]
        public void AMapNotReadWholeIsNeverKeptAndTheLastOneStays()
        {
            string folder = TempFolder.Make("f131-memory-stays");

            try
            {
                string memoryPath = Path.Combine(folder, TeamMapMemory.FileName);
                string good = Path.Combine(folder, "good.xml");
                string missing = Path.Combine(folder, "missing.xml");
                string bad = Path.Combine(folder, "bad.xml");
                string empty = Path.Combine(folder, "empty.xml");

                File.WriteAllText(Path.Combine(folder, "good.teams.txt"), TeamMapTests.BadersMap);
                File.WriteAllText(Path.Combine(folder, "bad.teams.txt"), "teams: B\n");
                File.WriteAllText(Path.Combine(folder, "empty.teams.txt"), "# nothing\n");

                TeamMapMemory memory = TeamMapMemory.Load(memoryPath);

                Assert.That(
                    memory.Remember(TeamMap.Beside(missing, new TeamMapSettings())),
                    Is.EqualTo("TEAMS    no map is kept for a run with no clash XML" + NotWhole));
                Assert.That(File.Exists(memoryPath), Is.False);

                memory.Remember(TeamMap.Beside(good, new TeamMapSettings()));

                string goodMap = Path.Combine(folder, "good.teams.txt");
                string stays = "TEAMS    the map kept for a run with no clash XML stays " + goodMap + NotWhole;

                Assert.That(memory.Remember(TeamMap.Beside(missing, new TeamMapSettings())), Is.EqualTo(stays));
                Assert.That(memory.Remember(TeamMap.Beside(bad, new TeamMapSettings())), Is.EqualTo(stays));
                Assert.That(memory.Remember(TeamMap.Beside(empty, new TeamMapSettings())), Is.EqualTo(stays));
                Assert.That(memory.Remember(TeamMap.NoXml(new TeamMapSettings())), Is.EqualTo(stays));
                Assert.That(memory.Remember(memory.ForNoXml(new TeamMapSettings())), Is.EqualTo(stays));
                Assert.That(memory.Remember(null), Is.EqualTo(stays));
                Assert.That(TeamMapMemory.Load(memoryPath).LastMap, Is.EqualTo(goodMap));
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
        /// nothing and its TEAMS line says why. A memory that cannot be written is said too.
        /// </summary>
        [Test]
        public void AMemoryThatCannotBeReadOrWrittenIsSaidAndNeverThrown()
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
                    TeamMapTests.Same(
                        map.Lines(),
                        "TEAMS    no clash XML was picked, and THE TEAM MAP KEPT FROM THE LAST RUN WITH ONE, " + memoryPath
                            + ", COULD NOT BE READ: " + fault[1] + NothingApplies);
                }

                File.WriteAllBytes(memoryPath, new byte[] { 0x6B, 0x65, 0x70, 0x74, 0x3A, 0x20, 0xC3, 0x28, 0x0A });
                Assert.That(TeamMapMemory.Load(memoryPath).Unread, Is.EqualTo("it is not UTF-8 text"));

                string xml = Path.Combine(folder, "a.xml");
                File.WriteAllText(Path.Combine(folder, "a.teams.txt"), TeamMapTests.BadersMap);

                string aFolder = Path.Combine(folder, "a folder where the memory would be");
                Directory.CreateDirectory(aFolder);

                string said = TeamMapMemory.Load(aFolder).Remember(TeamMap.Beside(xml, new TeamMapSettings()));

                Assert.That(said, Does.StartWith("TEAMS    this map could not be kept for a run with no clash XML, " + aFolder + " could not be written: "));
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

                TeamMapMemory memory = TeamMapMemory.Load(Path.Combine(folder, TeamMapMemory.FileName));
                memory.Remember(TeamMap.Beside(xml, new TeamMapSettings()));

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
