using Federator.Core.Clash;
using Federator.Core.Report;
using NUnit.Framework;

namespace Federator.Core.Tests
{
    /// <summary>
    /// F132's add-in half. A clash is the unordered pair of its two items' keys,
    /// MirrorMerge, and the key of one item is ClashItem.MergeKey, read off what the harvest
    /// read of the item: the file it came from, its id and the name of the geometry the clash
    /// was found on. An item with no id is not read, UNKNOWN, and the merge then fails closed
    /// on its own rule. The names in here are sample data.
    /// </summary>
    [TestFixture]
    public class MirrorItemKeyTests
    {
        private static ClashItem Item(string source, string id, string name)
        {
            ClashItem item = new ClashItem();
            item.SourceFile = source;
            item.ElementId = id;
            item.Name = name;
            return item;
        }

        [Test]
        public void TheKeyIsTheFileTheIdAndTheGeometryName()
        {
            ClashItem item = Item("1104-PAR-1A02WE-ZZZ-ME-MOD-000001.nwc", "702888", "Concrete");

            Assert.That(item.MergeKey(), Is.EqualTo("1104-PAR-1A02WE-ZZZ-ME-MOD-000001.nwc|702888|Concrete"));
        }

        [Test]
        public void TwoReadsOfOneItemGiveOneKeyAndAnotherItemAnother()
        {
            ClashItem one = Item("a.nwc", "1", "Concrete");
            ClashItem same = Item("a.nwc", "1", "Concrete");
            ClashItem otherFile = Item("b.nwc", "1", "Concrete");
            ClashItem otherId = Item("a.nwc", "2", "Concrete");
            ClashItem otherGeometry = Item("a.nwc", "1", "Plaster");

            Assert.That(same.MergeKey(), Is.EqualTo(one.MergeKey()));
            Assert.That(otherFile.MergeKey(), Is.Not.EqualTo(one.MergeKey()));
            Assert.That(otherId.MergeKey(), Is.Not.EqualTo(one.MergeKey()));
            Assert.That(otherGeometry.MergeKey(), Is.Not.EqualTo(one.MergeKey()));
        }

        [Test]
        public void AnItemWithNoIdIsNotRead()
        {
            Assert.That(Item("a.nwc", string.Empty, "Concrete").MergeKey(), Is.EqualTo(TestSettings.UnknownLocator));
            Assert.That(Item("a.nwc", null, "Concrete").MergeKey(), Is.EqualTo(TestSettings.UnknownLocator));
            Assert.That(TestDrift.WasRead(Item("a.nwc", string.Empty, "Concrete").MergeKey()), Is.False);
        }

        [Test]
        public void AnItemWithNoFileOrNoNameIsStillKeyedOnItsId()
        {
            Assert.That(Item(string.Empty, "7", string.Empty).MergeKey(), Is.EqualTo("|7|"));
            Assert.That(Item(null, "7", null).MergeKey(), Is.EqualTo("|7|"));
        }
    }
}
