using System.Collections.Generic;
using Federator.Core.Exchange;
using Federator.Core.Naming;
using NUnit.Framework;

namespace Federator.Core.Tests
{
    /// <summary>
    /// F166, Bader's answer B to Q164 of 14 Oct 2026: the pack is chosen by the project code read off part 1
    /// of the NWC file names. A second pack is made here, in the test, and is never shipped.
    /// </summary>
    [TestFixture]
    public class PackChoiceTests
    {
        private static readonly ContainerNameSettings Names = new ContainerNameSettings();

        private static IList<string> Files(string code, int count)
        {
            List<string> files = new List<string>();

            for (int i = 1; i <= count; i++)
            {
                files.Add(@"C:\in\C04\" + code + "-PAR-1A04PK-ZZZ-AR-MOD-00000" + i + ".nwc");
            }

            return files;
        }

        private static IList<MatrixPack> TheBuiltInPackAlone()
        {
            return MatrixPack.All();
        }

        [Test]
        public void ThePackBuiltInCarries1104()
        {
            IList<MatrixPack> packs = MatrixPack.All();

            Assert.That(packs.Count, Is.EqualTo(1));
            Assert.That(packs[0].Name, Is.EqualTo("1104-PAR"));
            Assert.That(packs[0].ProjectCode, Is.EqualTo("1104"));
        }

        [Test]
        public void OnePackAndAMatchingCodeRuns()
        {
            PackChoice choice = PackChoice.For("1A04PK", Files("1104", 3), TheBuiltInPackAlone(), Names);

            Assert.That(choice.Stopped, Is.False, choice.Why);
            Assert.That(choice.Pack.Name, Is.EqualTo("1104-PAR"));
            Assert.That(choice.Codes, Is.EqualTo(new[] { "1104" }));
            Assert.That(choice.Why, Is.Null);
        }

        [Test]
        public void ACodeWithNoPackStopsAndSaysWhatToSend()
        {
            PackChoice choice = PackChoice.For("1A04PK", Files("2210", 2), TheBuiltInPackAlone(), Names);

            Assert.That(choice.Stopped, Is.True);
            Assert.That(choice.Why, Does.Contain(
                "No matrix pack for project code 2210. Send the clash matrix and a session will add it."));
            Assert.That(choice.Why, Does.Contain("The code found is 2210."));
            Assert.That(choice.Why, Does.Contain("before anything is opened"));
            Assert.That(choice.Codes, Is.EqualTo(new[] { "2210" }));
        }

        [Test]
        public void AnotherPackBuiltInInTheTestIsChosenByItsOwnCode()
        {
            List<MatrixPack> packs = new List<MatrixPack>(MatrixPack.All());
            packs.Add(MatrixPack.ForTest("2210-TST", "2210"));

            PackChoice one = PackChoice.For("A", Files("1104", 1), packs, Names);
            PackChoice two = PackChoice.For("A", Files("2210", 1), packs, Names);

            Assert.That(one.Pack.Name, Is.EqualTo("1104-PAR"));
            Assert.That(two.Pack.Name, Is.EqualTo("2210-TST"));
        }

        [Test]
        public void TwoPacksWithTheSameCodeStopAndNameBoth()
        {
            List<MatrixPack> packs = new List<MatrixPack>(MatrixPack.All());
            packs.Add(MatrixPack.ForTest("1104-COPY", "1104"));

            PackChoice choice = PackChoice.For("A", Files("1104", 2), packs, Names);

            Assert.That(choice.Stopped, Is.True);
            Assert.That(choice.Why, Does.Contain("More than one matrix pack carries the project code 1104"));
            Assert.That(choice.Why, Does.Contain("1104-PAR"));
            Assert.That(choice.Why, Does.Contain("1104-COPY"));
        }

        [Test]
        public void AGroupOfMixedCodesStopsAndNamesTheCodes()
        {
            List<string> mixed = new List<string>(Files("1104", 2));
            mixed.AddRange(Files("2210", 1));

            PackChoice choice = PackChoice.For("1A04PK", mixed, TheBuiltInPackAlone(), Names);

            Assert.That(choice.Stopped, Is.True);
            Assert.That(choice.Why, Does.Contain("The NWC files of the group 1A04PK carry different project codes, 1104, 2210."));
            Assert.That(choice.Why, Does.Contain("before anything is opened"));
        }

        [Test]
        public void TwoGroupsOfDifferentCodesStopBecauseOneRunReadsOnePack()
        {
            List<KeyValuePair<string, IList<string>>> groups = new List<KeyValuePair<string, IList<string>>>
            {
                PackChoice.Group("1A04PK", Files("1104", 2)),
                PackChoice.Group("1B01XX", Files("2210", 2))
            };

            PackChoice choice = PackChoice.For(groups, TheBuiltInPackAlone(), Names);

            Assert.That(choice.Stopped, Is.True);
            Assert.That(choice.Why, Does.Contain("different project codes, 1104, 2210"));
            Assert.That(choice.Why, Does.Contain("One run reads one matrix pack"));
        }

        [Test]
        public void ANameWithNoProjectCodePartStopsAndIsNamed()
        {
            PackChoice choice = PackChoice.For(
                "A", new List<string> { @"C:\in\model.nwc", @"C:\in\1104-PAR-1A04PK-ZZZ-AR-MOD-000001.nwc" },
                TheBuiltInPackAlone(), Names);

            Assert.That(choice.Stopped, Is.True);
            Assert.That(choice.Why, Does.Contain("cannot be read from model.nwc"));
        }

        [Test]
        public void NothingGivenStops()
        {
            PackChoice choice = PackChoice.For("A", new List<string>(), TheBuiltInPackAlone(), Names);

            Assert.That(choice.Stopped, Is.True);
            Assert.That(choice.Why, Does.Contain("No NWC file name was given"));
        }

        [Test]
        public void ThePackLineKeepsTheNameAndSaysTheCode()
        {
            MatrixPack pack = MatrixPack.All()[0];

            Assert.That(MatrixPack.Line(pack, null), Does.Contain("the built in matrix pack 1104-PAR, project code 1104, holding"));
        }

        [Test]
        public void ACaseOnlyDifferenceInACodeStillMatches()
        {
            List<MatrixPack> packs = new List<MatrixPack> { MatrixPack.ForTest("ABC-X", "abc") };

            Assert.That(PackChoice.For("A", Files("ABC", 1), packs, Names).Pack, Is.Not.Null);
        }
    }
}
