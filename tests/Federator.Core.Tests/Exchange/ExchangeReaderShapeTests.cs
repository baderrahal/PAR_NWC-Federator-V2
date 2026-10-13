using System.IO;
using Federator.Core.Clash;
using Federator.Core.Exchange;
using Federator.Core.Health;
using NUnit.Framework;

namespace Federator.Core.Tests
{
    /// <summary>
    /// One reader has to cope with all three shapes, and with the awkward cases the real
    /// files do not happen to show.
    /// </summary>
    [TestFixture]
    public class ExchangeReaderShapeTests
    {
        private const string TestsOnly =
            "<exchange units=\"ft\">" +
            "  <batchtest name=\"B\" internal_name=\"B\" units=\"ft\">" +
            "    <clashtests>" +
            "      <clashtest name=\"T1\" test_type=\"hard_conservative\" status=\"new\"" +
            "                 tolerance=\"0.2460629921\" merge_composites=\"1\">" +
            "        <linkage mode=\"none\"/>" +
            "        <left><clashselection selfintersect=\"0\" primtypes=\"1\">" +
            "          <locator>lcop_selection_set_tree/A/One</locator></clashselection></left>" +
            "        <right><clashselection selfintersect=\"1\" primtypes=\"3\">" +
            "          <locator>lcop_selection_set_tree/A/Two</locator></clashselection></right>" +
            "        <rules/>" +
            "      </clashtest>" +
            "    </clashtests>" +
            "  </batchtest>" +
            "</exchange>";

        private const string SetsOnly =
            "<exchange units=\"m\">" +
            "  <selectionsets>" +
            "    <viewfolder name=\"A\">" +
            "      <selectionset name=\"One\" guid=\"g1\">" +
            "        <findspec mode=\"all\" disjoint=\"0\">" +
            "          <conditions>" +
            "            <condition test=\"equals\" flags=\"0\">" +
            "              <category><name internal=\"C\">Cat</name></category>" +
            "              <property><name internal=\"P\">Prop</name></property>" +
            "              <value><data type=\"wstring\">Walls</data></value>" +
            "            </condition>" +
            "          </conditions>" +
            "          <locator>/</locator>" +
            "        </findspec>" +
            "      </selectionset>" +
            "    </viewfolder>" +
            "    <selectionset name=\"Loose\" guid=\"g2\">" +
            "      <findspec mode=\"all\" disjoint=\"1\">" +
            "        <conditions>" +
            "          <condition test=\"contains\" flags=\"64\">" +
            "            <property><name internal=\"S\">Source File</name></property>" +
            "            <value><data type=\"wstring\">-AR-</data></value>" +
            "          </condition>" +
            "        </conditions>" +
            "        <locator>/</locator>" +
            "      </findspec>" +
            "    </selectionset>" +
            "  </selectionsets>" +
            "</exchange>";

        [Test]
        public void ReadsAFileThatHoldsTestsOnly()
        {
            ExchangeDocument document = new ExchangeReader().ReadText(TestsOnly);

            Assert.That(document.Tests.Count, Is.EqualTo(1));
            Assert.That(document.Sets.Count, Is.EqualTo(0));
            Assert.That(document.HasSets, Is.False);
            Assert.That(document.Tests[0].ToleranceInFileUnits, Is.EqualTo(0.2460629921).Within(1e-10));
            Assert.That(document.Tests[0].FileUnits, Is.EqualTo("ft"));
            Assert.That(document.Tests[0].Right.SelfIntersect, Is.True);
            Assert.That(document.Tests[0].Right.PrimitiveTypes, Is.EqualTo(3));
        }

        [Test]
        public void ReadsAFileThatHoldsSetsOnly()
        {
            ExchangeDocument document = new ExchangeReader().ReadText(SetsOnly);

            Assert.That(document.Tests.Count, Is.EqualTo(0));
            Assert.That(document.Sets.Count, Is.EqualTo(2));
            Assert.That(document.Sets[0].Path, Is.EqualTo("lcop_selection_set_tree/A/One"));
            Assert.That(document.Sets[1].Path, Is.EqualTo("lcop_selection_set_tree/Loose"));
            Assert.That(document.Sets[1].IsAtRoot, Is.True);
            Assert.That(document.Sets[1].Disjoint, Is.True);
        }

        [Test]
        public void AConditionWithNoCategoryIsReadRatherThanDropped()
        {
            ExchangeDocument document = new ExchangeReader().ReadText(SetsOnly);
            SearchConditionDefinition condition = document.Sets[1].Conditions[0];

            Assert.That(condition.Category, Is.Null);
            Assert.That(condition.Property.DisplayName, Is.EqualTo("Source File"));
            Assert.That(condition.Test, Is.EqualTo("contains"));
            Assert.That(condition.Flags, Is.EqualTo(64));
            Assert.That(condition.Value.Data, Is.EqualTo("-AR-"));
        }

        /// <summary>
        /// 5g. The exchange file's flags attribute IS Navisworks' SearchConditionOptions,
        /// F78 measured that off the five conditions carrying 64, which are the five that
        /// start a group. NegateCondition is 32 in the same enum, so a negated condition
        /// reads as flags 32, or 96 where it also starts a group, and the reader has to
        /// carry the number through untouched for SetBuilder to hand it to the API. This
        /// is the half of 5g that needs no Navisworks.
        /// </summary>
        [Test]
        public void ANegatedConditionsFlagsAreCarriedThroughUntouched()
        {
            ExchangeReader reader = new ExchangeReader();

            SearchConditionDefinition negated =
                reader.ReadText(SetsOnly.Replace("flags=\"64\"", "flags=\"32\"")).Sets[1].Conditions[0];
            Assert.That(negated.Flags, Is.EqualTo(32));

            SearchConditionDefinition both =
                reader.ReadText(SetsOnly.Replace("flags=\"64\"", "flags=\"96\"")).Sets[1].Conditions[0];
            Assert.That(both.Flags, Is.EqualTo(96), "32 negate and 64 start a group, together");

            // And a negation is not a different question, the same way flags never are:
            // two sets asking the same thing one negated still read as one rule, so F84's
            // duplicate check would still pair them and a person reads why.
            Assert.That(negated.RuleSignature, Is.EqualTo(both.RuleSignature));
        }

        [Test]
        public void FlagsDoNotChangeTheRuleAConditionAsksFor()
        {
            ExchangeReader reader = new ExchangeReader();
            string withZero = SetsOnly.Replace("flags=\"64\"", "flags=\"0\"");

            SearchConditionDefinition a = reader.ReadText(SetsOnly).Sets[1].Conditions[0];
            SearchConditionDefinition b = reader.ReadText(withZero).Sets[1].Conditions[0];

            Assert.That(a.Flags, Is.Not.EqualTo(b.Flags));
            Assert.That(a.RuleSignature, Is.EqualTo(b.RuleSignature));
        }

        [Test]
        public void TheToleranceIsReadInTheFileUnitsAndNotConverted()
        {
            // The reader reads. Converting is ClashTestPlan.Convert's job, F33, so a
            // file in metres carries 0.075 and m, and nothing derived from either.
            string inMetres = TestsOnly
                .Replace("units=\"ft\"", "units=\"m\"")
                .Replace("tolerance=\"0.2460629921\"", "tolerance=\"0.075\"");

            ExchangeDocument document = new ExchangeReader().ReadText(inMetres);

            Assert.That(document.Tests[0].FileUnits, Is.EqualTo("m"));
            Assert.That(document.Tests[0].ToleranceInFileUnits, Is.EqualTo(0.075).Within(1e-12));
        }

        [Test]
        public void AFileInUnitsTheToolDoesNotKnowIsStillRead()
        {
            // The reader used to throw on the whole file here, from a conversion it had
            // no business doing. Now the file reads and the plan skips each test by name.
            string inCubits = TestsOnly.Replace("units=\"ft\"", "units=\"cubits\"");

            ExchangeDocument document = new ExchangeReader().ReadText(inCubits);

            Assert.That(document.Tests.Count, Is.EqualTo(1));
            Assert.That(document.Tests[0].FileUnits, Is.EqualTo("cubits"));
            Assert.That(document.Tests[0].ToleranceInFileUnits, Is.EqualTo(0.2460629921).Within(1e-10));
        }

        [Test]
        public void ALocatorThatMatchesNoSetIsReportedByTestName()
        {
            ExchangeReader reader = new ExchangeReader();
            HealthCheckResult health = HealthCheck.Run(
                reader.ReadText(TestsOnly), reader.ReadText(SetsOnly));

            Assert.That(health.ResolvedLocatorCount, Is.EqualTo(1));
            Assert.That(health.UnresolvedLocatorCount, Is.EqualTo(1));
            Assert.That(health.UnresolvedLocators[0].Locator, Is.EqualTo("lcop_selection_set_tree/A/Two"));
            Assert.That(health.UnresolvedLocators[0].TestNames, Is.EqualTo(new[] { "T1" }));
            Assert.That(health.TestsWithUnresolvedSide, Is.EqualTo(new[] { "T1" }));
        }

        [Test]
        public void ATestWithNoLocatorOnASideIsReportedRatherThanImported()
        {
            string missingSide = TestsOnly.Replace(
                "<locator>lcop_selection_set_tree/A/Two</locator>", string.Empty);

            ExchangeReader reader = new ExchangeReader();
            HealthCheckResult health = HealthCheck.Run(
                reader.ReadText(missingSide), reader.ReadText(SetsOnly));

            Assert.That(health.TestsWithUnresolvedSide, Is.EqualTo(new[] { "T1" }));
            Assert.That(reader.ReadText(missingSide).Tests[0].Right.HasLocator, Is.False);
        }

        [Test]
        public void OneSetOnItsOwnIsNeverCalledUnusable()
        {
            string single = "<exchange units=\"ft\"><selectionsets>" +
                "<selectionset name=\"Only\"><findspec mode=\"all\" disjoint=\"0\"><conditions>" +
                "<condition test=\"equals\" flags=\"0\">" +
                "<property><name internal=\"P\">P</name></property>" +
                "<value><data type=\"wstring\">V</data></value>" +
                "</condition></conditions><locator>/</locator></findspec></selectionset>" +
                "</selectionsets></exchange>";

            HealthCheckResult health = HealthCheck.Run(new ExchangeReader().ReadText(single));

            Assert.That(health.SetCount, Is.EqualTo(1));
            Assert.That(health.DistinctRuleCount, Is.EqualTo(1));
            Assert.That(health.AllSetsShareOneRule, Is.False);
        }

        [Test]
        public void ExactlyRepeatedSetNamesAreReportedAsExact()
        {
            string repeated = "<exchange units=\"ft\"><selectionsets>" +
                "<viewfolder name=\"A\"><selectionset name=\"Same\"/></viewfolder>" +
                "<viewfolder name=\"B\"><selectionset name=\"Same\"/></viewfolder>" +
                "</selectionsets></exchange>";

            HealthCheckResult health = HealthCheck.Run(new ExchangeReader().ReadText(repeated));

            Assert.That(health.DuplicateNames.Count, Is.EqualTo(1));
            Assert.That(health.DuplicateNames[0].IsExact, Is.True);
            Assert.That(health.DuplicateNames[0].Count, Is.EqualTo(2));
        }

        [Test]
        public void TheSetTreeRootIsASetting()
        {
            ExchangeReader reader = new ExchangeReader { SetTreeRoot = "other_root" };

            Assert.That(reader.ReadText(SetsOnly).Sets[0].Path, Is.EqualTo("other_root/A/One"));
        }
        // D1. The one sentence the window puts under the picked file.
        [Test]
        public void TheHealthLineOfATestsOnlyFileSaysTheModelSetsAreUsed()
        {
            ExchangeDocument document = new ExchangeReader().ReadText(TestsOnly);
            HealthCheckResult health = HealthCheck.Run(document);

            Assert.That(health.Line(document.HasSets),
                Is.EqualTo("It holds no sets, so any test in it looks for the sets already in the model, which is not checked here."));
        }

        [Test]
        public void TheHealthLineNamesHowManyLocatorsMissWhenTheFileHoldsSets()
        {
            // One set, One, and a test whose right side names Two, which is not there.
            string oneSet = TestsOnly.Replace("</exchange>",
                "<selectionsets><selectionset name=\"One\"><findspec mode=\"all\"><conditions>"
                + "<condition test=\"equals\" flags=\"10\">"
                + "<property><name internal=\"LcOaNodeSourceFile\">Source File</name></property>"
                + "<value><data type=\"wstring\">a.nwc</data></value></condition>"
                + "</conditions><locator>/</locator></findspec></selectionset>"
                + "</selectionsets></exchange>")
                .Replace("lcop_selection_set_tree/A/One", "lcop_selection_set_tree/One");

            ExchangeDocument document = new ExchangeReader().ReadText(oneSet);
            HealthCheckResult health = HealthCheck.Run(document);

            Assert.That(document.HasSets, Is.True);
            Assert.That(health.Line(true),
                Is.EqualTo("1 of 2 locators name no set in the file, so 1 test would be skipped by name."));
        }

        // ---------- a setting left out or written wrong, FR-155 ----------

        /// <summary>
        /// FR-155. A test whose file leaves out merge composites, or a side's self intersect or
        /// primitive types, became false or 0 with no word said and was planned as buildable,
        /// where a test with no tolerance is skipped by name. Each is skipped by name now,
        /// naming the setting left out.
        /// </summary>
        [Test]
        public void ATestWhoseFileLeavesOutASettingIsSkippedByNameNamingIt()
        {
            string[][] cases =
            {
                new[] { " merge_composites=\"1\"", "merge_composites" },
                new[] { " primtypes=\"1\"", "left primtypes" },
                new[] { " selfintersect=\"1\"", "right selfintersect" }
            };

            foreach (string[] one in cases)
            {
                ClashTestPlan plan = ClashTestPlan.From(
                    new ExchangeReader().ReadText(TestsOnly.Replace(one[0], string.Empty)), "ft");

                Assert.That(plan.Buildable.Count, Is.EqualTo(0), one[1] + " left out is still planned");
                Assert.That(plan.Skipped.Count, Is.EqualTo(1), one[1]);
                Assert.That(plan.Skipped[0].Name, Is.EqualTo("T1"), "the test has to be reported by name");
                Assert.That(plan.Skipped[0].Kind, Is.EqualTo(ClashSkipReason.MissingSetting), one[1]);
                Assert.That(plan.Skipped[0].Reason, Does.Contain(one[1]));
            }
        }

        /// <summary>
        /// FR-155. A setting written but not a whole number read as 0 or false, where a
        /// tolerance written wrong is refused with its name and the test's. These are refused
        /// the same way, and a condition's flags too, since 0 there drops a negation or turns
        /// an OR into an AND.
        /// </summary>
        [Test]
        public void ASettingWrittenWrongIsRefusedNamingItAndWhereItIs()
        {
            string[][] cases =
            {
                new[] { TestsOnly.Replace("merge_composites=\"1\"", "merge_composites=\"x\""), "merge_composites", "T1" },
                new[] { TestsOnly.Replace("primtypes=\"3\"", "primtypes=\"many\""), "primtypes", "T1" },
                new[] { TestsOnly.Replace("selfintersect=\"1\"", "selfintersect=\"yes\""), "selfintersect", "T1" },
                new[] { SetsOnly.Replace("flags=\"64\"", "flags=\"or\""), "flags", "Loose" }
            };

            foreach (string[] one in cases)
            {
                InvalidDataException refused = Assert.Throws<InvalidDataException>(
                    delegate { new ExchangeReader().ReadText(one[0]); }, one[1] + " written wrong read as a number");

                Assert.That(refused.Message, Does.Contain(one[1]));
                Assert.That(refused.Message, Does.Contain(one[2]));
            }
        }
    }
}
