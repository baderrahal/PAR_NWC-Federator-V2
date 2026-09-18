using System;
using Federator.Core.Diagnostics;
using NUnit.Framework;

namespace Federator.Core.Tests
{
    /// <summary>
    /// The line that says what this run set on the NWD it published.
    ///
    /// It exists because an NWD that will not open in ACC is diagnosed off the log alone,
    /// so every check here is about the line still being readable months later by someone
    /// holding only the log and the file.
    /// </summary>
    [TestFixture]
    public class PublishedPropertiesTests
    {
        [Test]
        public void NothingSetSaysSoRatherThanTrailingOff()
        {
            PublishedProperties set = new PublishedProperties();

            Assert.That(set.Count, Is.EqualTo(0));
            Assert.That(
                set.Line(),
                Is.EqualTo(
                    "NWD      publish properties set: none, the NWD went out with whatever the defaults are"));
        }

        [Test]
        public void EveryPropertySetIsNamedWithItsValue()
        {
            PublishedProperties set = new PublishedProperties();

            set.Set("Title", "1104-PAR-1C07BC-ZZZ-BM-MOD-000001");
            set.Set("AllowResave", true);

            Assert.That(
                set.Line(),
                Is.EqualTo(
                    "NWD      publish properties set: "
                        + "Title=1104-PAR-1C07BC-ZZZ-BM-MOD-000001, AllowResave=true"));
        }

        [Test]
        public void TheOrderIsTheOrderTheyWereSetIn()
        {
            PublishedProperties set = new PublishedProperties();

            set.Set("Second", "b");
            set.Set("First", "a");

            Assert.That(set.Line(), Does.Contain("Second=b, First=a"));
        }

        [Test]
        public void ABooleanReadsTrueOrFalseInLowerCaseTheWayTheApiNamesIt()
        {
            PublishedProperties set = new PublishedProperties();

            set.Set("AllowResave", true);
            set.Set("PreventObjectPropertyExport", false);

            Assert.That(set.ValueOf("AllowResave"), Is.EqualTo("true"));
            Assert.That(set.ValueOf("PreventObjectPropertyExport"), Is.EqualTo("false"));
        }

        /// <summary>
        /// The one thing this line must never do is claim a property was set when the
        /// add-in stopped setting it. So the break is a property left out, and the check
        /// is that the line does not carry it and WasSet says no.
        /// </summary>
        [Test]
        public void APropertyNobodySetIsNotOnTheLine()
        {
            PublishedProperties set = new PublishedProperties();

            set.Set("Title", "anything");

            Assert.That(set.WasSet("AllowResave"), Is.False);
            Assert.That(set.ValueOf("AllowResave"), Is.Null);
            Assert.That(set.Line(), Does.Not.Contain("AllowResave"));
        }

        /// <summary>
        /// Null and the empty string are different answers. A property set to nothing was
        /// still set, and reading it back as null would make the line and the check
        /// disagree about what happened.
        /// </summary>
        [Test]
        public void APropertySetToNothingIsStillSet()
        {
            PublishedProperties set = new PublishedProperties();

            set.Set("Author", null);

            Assert.That(set.WasSet("Author"), Is.True);
            Assert.That(set.ValueOf("Author"), Is.EqualTo(string.Empty));
            Assert.That(set.Line(), Does.Contain("Author="));
        }

        [Test]
        public void ANumberIsWrittenInvariantSoEveryMachineWritesTheSameLine()
        {
            PublishedProperties set = new PublishedProperties();

            set.Set("Anything", 1024);

            Assert.That(set.ValueOf("Anything"), Is.EqualTo("1024"));
        }

        [Test]
        public void APropertyWithNoNameIsRefusedRatherThanWrittenAsBlank()
        {
            PublishedProperties set = new PublishedProperties();

            Assert.That(() => set.Set(null, "a"), Throws.ArgumentException);
            Assert.That(() => set.Set(string.Empty, "a"), Throws.ArgumentException);
            Assert.That(set.Count, Is.EqualTo(0));
        }

        /// <summary>
        /// The prefix is the one the rest of the NWD lines carry, so the block reads as one
        /// thing in the log rather than as a stray sentence.
        /// </summary>
        [Test]
        public void TheLineCarriesTheSameNwdPrefixTheOtherNwdLinesDo()
        {
            PublishedProperties set = new PublishedProperties();

            set.Set("Title", "anything");

            Assert.That(set.Line(), Does.StartWith("NWD      "));
        }
    }
}
