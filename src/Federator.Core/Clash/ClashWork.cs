using Federator.Core.Exchange;

namespace Federator.Core.Clash
{
    /// <summary>
    /// The three things the clash step can do. Tests from the picked XML, tests already
    /// saved in the document when no XML was picked, or nothing.
    /// </summary>
    public enum ClashSource
    {
        TestsFromXml,
        TestsSavedInDocument,
        Nothing
    }

    /// <summary>
    /// What the run should do about clash, given whatever was picked in the Clash step.
    ///
    /// Nothing picked is a step switched off, not a failure. The run does the model side
    /// and nothing else, and says so once. A file holding only sets, only tests, or both
    /// are all normal, so each half is decided on its own rather than together.
    /// </summary>
    public static class ClashWork
    {
        public const string NothingPicked =
            "no file picked in the Clash step, no set built and no test created";

        /// <summary>
        /// Which of the three things the clash step does. Decided once, in Core, so the
        /// scanned run and the open file run cannot answer it differently.
        /// </summary>
        public static ClashSource SourceFor(ExchangeDocument exchange, int savedTests)
        {
            if (Any(exchange))
            {
                return ClashSource.TestsFromXml;
            }

            if (exchange == null && savedTests > 0)
            {
                return ClashSource.TestsSavedInDocument;
            }

            return ClashSource.Nothing;
        }

        /// <summary>
        /// The one line the log carries for the source, in the same shape whichever way the
        /// run was started. Three shapes and no fourth.
        /// </summary>
        public static string DescribeSource(ClashSource source, ExchangeDocument exchange, int savedTests)
        {
            switch (source)
            {
                case ClashSource.TestsFromXml:
                    return "tests from XML, " + Describe(exchange);
                case ClashSource.TestsSavedInDocument:
                    return "tests saved in the document, " + savedTests + " of them, no XML picked";
                default:
                    return exchange == null
                        ? "nothing, no XML picked and the document holds no clash test, so nothing ran"
                        : "nothing, " + Describe(exchange);
            }
        }

        /// <summary>True when there is any clash work at all for this run.</summary>
        public static bool Any(ExchangeDocument exchange)
        {
            return BuildsSets(exchange) || CreatesTests(exchange);
        }

        public static bool BuildsSets(ExchangeDocument exchange)
        {
            return exchange != null && exchange.HasSets;
        }

        public static bool CreatesTests(ExchangeDocument exchange)
        {
            return exchange != null && exchange.HasTests;
        }

        /// <summary>One line saying what was picked and what will happen to it.</summary>
        public static string Describe(ExchangeDocument exchange)
        {
            if (exchange == null)
            {
                return NothingPicked;
            }

            if (!Any(exchange))
            {
                return "the picked file holds no set and no clash test, so there is nothing to do with it";
            }

            return "the picked file holds " + exchange.Sets.Count
                + (exchange.Sets.Count == 1 ? " set and " : " sets and ")
                + exchange.Tests.Count
                + (exchange.Tests.Count == 1 ? " test" : " tests");
        }
    }
}
