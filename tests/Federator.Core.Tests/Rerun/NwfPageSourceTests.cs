using System.IO;
using NUnit.Framework;

namespace Federator.Core.Tests
{
    /// <summary>
    /// F188, Bader's order of 14 Oct 2026, task 9. The window cannot be built here, so what the two pages hold is read
    /// off its XAML and its code, as a test for a list read off a file reads the file. What a run showed of them is
    /// in steps\03_bader_next.md.
    /// </summary>
    [TestFixture]
    public class NwfPageSourceTests
    {
        private static string Xaml()
        {
            return File.ReadAllText(Path.Combine(Samples.Repo(), "src", "Federator.Addin", "Ui", "FederatorWindow.xaml"));
        }

        private static string Code(string name)
        {
            return File.ReadAllText(Path.Combine(Samples.Repo(), "src", "Federator.Addin", "Ui", name));
        }

        [Test]
        public void TheFirstPageIsNwcAndTheSecondBesideItIsNwf()
        {
            string xaml = Xaml();
            int pages = xaml.IndexOf("x:Name=\"Pages\"", System.StringComparison.Ordinal);
            int nwc = xaml.IndexOf("<TabItem Header=\"NWC\">", System.StringComparison.Ordinal);
            int nwf = xaml.IndexOf("<TabItem Header=\"NWF\">", System.StringComparison.Ordinal);

            Assert.That(pages, Is.GreaterThan(0));
            Assert.That(nwc, Is.GreaterThan(pages));
            Assert.That(nwf, Is.GreaterThan(nwc), "the NWF page comes next to the NWC page");
        }

        [Test]
        public void ARebuildIsConfirmedWithTheClashHistoryNamedAndCancelTheDefault()
        {
            string page = Code("FederatorWindow.NwfPage.cs");

            Assert.That(page, Does.Contain("NwcPageRule.RebuildConfirm(GroupsWithAnNwf())"));
            Assert.That(page, Does.Contain("MessageBoxButton.OKCancel,\n                    MessageBoxImage.Warning,\n                    MessageBoxResult.Cancel")
                .Or.Contain("MessageBoxButton.OKCancel,\r\n                    MessageBoxImage.Warning,\r\n                    MessageBoxResult.Cancel"));
        }

        [Test]
        public void TheRebuildBoxAsksItsConfirmationOnCheckedSoAnAutomationTickAsksToo()
        {
            string xaml = Xaml();
            int at = xaml.IndexOf("x:Name=\"RebuildExisting\"", System.StringComparison.Ordinal);
            int end = xaml.IndexOf("/>", at, System.StringComparison.Ordinal);
            string tag = xaml.Substring(at, end - at);

            Assert.That(tag, Does.Contain("Checked=\"OnRebuildToggled\""));
            Assert.That(tag, Does.Contain("Unchecked=\"OnRebuildToggled\""));
            Assert.That(tag, Does.Not.Contain("Click="), "a Click is not raised by UI Automation's Toggle, a person's key press or an automation tick would skip the confirmation");
        }
    }
}
