using RandomizerCore.Logic;
using System.Collections.Generic;
using System.Linq;

namespace TheRealJournalRando.Rando
{
    public class ForkedLogicCost : LogicCost
    {
        public LogicCost Cost1 { get; init; }

        public LogicCost Cost2 { get; init; }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public ForkedLogicCost() { }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

        public ForkedLogicCost(LogicCost cost1, LogicCost cost2)
        {
            Cost1 = cost1;
            Cost2 = cost2;
        }

        public override bool CanGet(ProgressionManager pm) => Cost1.CanGet(pm) || Cost2.CanGet(pm);

        public override IEnumerable<Term> GetTerms() => Cost1.GetTerms().Concat(Cost2.GetTerms());

        public override string ToString()
        {
            return $"{nameof(ForkedLogicCost)} {{{Cost1} | {Cost2}}}";
        }
    }
}
