using System.IO;
using Federator.Core.Exchange;
using Federator.Core.Teams;
using NUnit.Framework;

namespace Federator.Core.Tests
{
    /// <summary>
    /// This project's team map, the file in the exchange folder beside the corrected XML, F131.
    /// It is read the way the tool reads it beside a picked XML, and it must read exactly to
    /// Bader's map of Q114 point 1, in his order of point 12, with the size folder of point 11.
    /// </summary>
    [TestFixture]
    public class ProjectTeamMapTests
    {
        [Test]
        public void TheMapIsNamedByTheRuleBesideTheCorrectedXml()
        {
            Assert.That(new TeamMapSettings().PathBeside(Samples.CorrectedMatrix()), Is.EqualTo(Samples.TeamMap()));
        }

        [Test]
        public void TheProjectsMapReadsExactlyToBadersMapInHisOrder()
        {
            TeamMap map = TeamMap.Beside(Samples.CorrectedMatrix(), new TeamMapSettings());
            TeamMap his = TeamMapTests.MapOf(TeamMapTests.BadersMap);

            Assert.That(map.Missing, Is.False, map.ListPath);
            Assert.That(map.Unread, Is.Null);
            Assert.That(map.IsRead, Is.True);
            TeamMapTests.Same(map.Teams, "Architecture", "Structure", "Mechanical", "Electrical");
            TeamMapTests.Same(map.Codes, "AR", "ST", "HV", "PL", "FP", "ME", "DR", "FF", "EL", "EV");

            foreach (string code in his.Codes)
            {
                Assert.That(map.TeamOf(code), Is.EqualTo(his.TeamOf(code)), code);
            }

            // Every TEAMS line after the first, which names where each was read: the teams with
            // their codes, the order of a pair and the teams whose pairs carry the size folder.
            System.Collections.Generic.IList<string> read = map.Lines();
            System.Collections.Generic.IList<string> want = his.Lines();

            Assert.That(read.Count, Is.EqualTo(want.Count));

            for (int i = 1; i < want.Count; i++)
            {
                Assert.That(read[i], Is.EqualTo(want[i]), "at " + i);
            }

            Assert.That(read[read.Count - 1], Is.EqualTo("TEAMS    a pair holding Mechanical or Electrical carries the size folder"));
        }

        /// <summary>
        /// Q114 point 2, read when the XML is picked: ReadPicked, the one way the add-in reads the
        /// picked file, reads the map beside it with the list of corrections. Q115 answered
        /// A, each its own file, so a map that cannot be read leaves the corrections made, a list
        /// that cannot be read leaves the map read, and an XML with neither has both said missing.
        /// </summary>
        [Test]
        public void ThePickReadsTheMapBesideTheXmlAndAFaultInOneListLeavesTheOtherRead()
        {
            ExchangeDocument corrected = MatrixCorrections.ReadPicked(Samples.CorrectedMatrix());

            Assert.That(corrected.Teams.IsRead, Is.True);
            Assert.That(corrected.Teams.ListPath, Is.EqualTo(Samples.TeamMap()));
            Assert.That(corrected.Teams.TeamOf("HV"), Is.EqualTo("Mechanical"));

            string folder = TempFolder.Make("f131-pick");

            try
            {
                string badMap = Path.Combine(folder, "bad-map.xml");
                File.Copy(Samples.Matrix(), badMap);
                File.Copy(Samples.CorrectionList(), Path.Combine(folder, "bad-map.corrections.txt"));
                File.WriteAllText(Path.Combine(folder, "bad-map.teams.txt"), "not a team line\n");

                ExchangeDocument mapUnread = MatrixCorrections.ReadPicked(badMap);

                Assert.That(mapUnread.Teams.Unread, Is.EqualTo("line 1, \"not a team line\", is not a line of a team map this tool knows"));
                Assert.That(mapUnread.Corrections[0], Does.StartWith("MATRIX   the corrections are read from "));

                string badList = Path.Combine(folder, "bad-list.xml");
                File.Copy(Samples.Matrix(), badList);
                File.WriteAllText(Path.Combine(folder, "bad-list.corrections.txt"), "not a correction\n");
                File.Copy(Samples.TeamMap(), Path.Combine(folder, "bad-list.teams.txt"));

                ExchangeDocument listUnread = MatrixCorrections.ReadPicked(badList);

                Assert.That(listUnread.Teams.IsRead, Is.True);
                Assert.That(listUnread.Teams.TeamOf("EV"), Is.EqualTo("Electrical"));
                Assert.That(listUnread.Corrections[0], Does.StartWith("MATRIX   NO CORRECTION WAS MADE TO THIS FILE"));

                string neither = Path.Combine(folder, "neither.xml");
                File.Copy(Samples.Matrix(), neither);

                ExchangeDocument bare = MatrixCorrections.ReadPicked(neither);

                Assert.That(bare.Teams.Missing, Is.True);
                Assert.That(bare.Teams.ListPath, Is.EqualTo(Path.Combine(folder, "neither.teams.txt")));
                Assert.That(bare.Corrections[0], Does.StartWith("MATRIX   no correction was made to this file, because no list of corrections is beside it"));
            }
            finally
            {
                TempFolder.Remove(folder);
            }
        }

        /// <summary>
        /// Q117 answered C by Bader on 2026-10-05, Electrical for this one: the corrected XML read
        /// with this project's map holds one set whose name carries no code of the map,
        /// BLD-Security Devices, which takes the team Electrical its folder names, and the pick's
        /// TEAMS lines name it. Every other set carries a code of the map.
        /// </summary>
        [Test]
        public void TheClientsOneSetWithNoCodeIsNamedAndTakesElectricalFromItsFolder()
        {
            ExchangeDocument corrected = MatrixCorrections.ReadPicked(Samples.CorrectedMatrix());

            TeamMapTests.Same(
                corrected.Teams.SetLines(corrected.Sets, '-'),
                "TEAMS    BLD-Security Devices carries no discipline code the map lists, so its team is Electrical, which a folder"
                    + " above it in the clash XML's set tree names, unless a model of its group carries a code its name holds");

            SelectionSetDefinition security = new System.Collections.Generic.List<SelectionSetDefinition>(corrected.Sets).Find(
                set => set.Name == "BLD-Security Devices");

            TeamMapTests.Same(security.Folders, "Electrical");
            Assert.That(corrected.Teams.TeamOfSet(string.Empty, security.Folders), Is.EqualTo("Electrical"));
        }
    }
}
