namespace Federator.Core.Health
{
    /// <summary>
    /// What the EXPORT CHECK blocks came to across one run, for its one run line, added up
    /// model by model by the rules each group's block judges by, so the line and the blocks
    /// cannot disagree. The engine kept its own four counters until c5d8aa8 and had none for
    /// a model holding no Revit element, so a run whose blocks named one read as a clean run
    /// on this line, the breaker's fifth finding.
    /// </summary>
    public sealed class ExportCheckAcrossTheRun
    {
        private int carryNoWorkset;
        private int carryOneOnSome;
        private int missAnId;
        private int holdNoElement;
        private int notCounted;

        /// <summary>One model, as its group's block is written.</summary>
        public void Add(ModelExport model)
        {
            if (model == null)
            {
                return;
            }

            if (!model.Counted)
            {
                notCounted++;
            }

            if (model.HoldsNoElement)
            {
                holdNoElement++;
            }

            if (model.CarriesNoWorkset)
            {
                carryNoWorkset++;
            }

            if (model.CarriesAWorksetOnSomeElements)
            {
                carryOneOnSome++;
            }

            if (model.MissesAnId)
            {
                missAnId++;
            }
        }

        /// <summary>
        /// The run line, written even when every count is nought, because a line that only
        /// appears when something is wrong reads as a check that did not run.
        /// </summary>
        public string Line()
        {
            bool clean = carryNoWorkset == 0 && carryOneOnSome == 0 && missAnId == 0 && holdNoElement == 0 && notCounted == 0;

            return "EXPORT CHECK across the run: " + carryNoWorkset + " model(s) carry no workset at all, "
                + carryOneOnSome + " carry one on only some of their elements, "
                + missAnId + " do not carry an element id on every element, "
                + holdNoElement + " hold no Revit element, and "
                + notCounted + " could not be counted"
                + (clean ? string.Empty : ". Nothing was changed in any model.");
        }
    }
}
