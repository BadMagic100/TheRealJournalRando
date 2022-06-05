using RandomizerCore.Logic;
using System.Collections.Generic;

namespace TheRealJournalRando.Rando
{
    public class LogicEnemyKillCost : LogicCost
    {
        public Term CanBenchWaypoint { get; init; }
        public Term DefeatWaypoint { get; init; }

        public int Amount { get; set; }
        public string EnemyIcName { get; init; }

        public LogicEnemyKillCost() { }

        public LogicEnemyKillCost(LogicManager lm, string enemyIcName, int amount)
        {
            EnemyIcName = enemyIcName;
            Amount = amount;
            CanBenchWaypoint = lm.GetTerm("Can_Bench");
            // use our own custom-defined "Defeated_Any" waypoints to make sure we can cover our special cases
            DefeatWaypoint = lm.GetTerm($"Defeated_Any_{enemyIcName}");
        }

        public override bool CanGet(ProgressionManager pm)
        {
            bool canDefeatEnemy = pm.Has(DefeatWaypoint.Id);
            bool hasRequiredBench = Amount < 2 || pm.Has(CanBenchWaypoint.Id);
            return canDefeatEnemy && hasRequiredBench;
        }

        public override IEnumerable<Term> GetTerms()
        {
            yield return DefeatWaypoint;
            yield return CanBenchWaypoint;
        }

        public override string ToString()
        {
            return $"{nameof(LogicEnemyKillCost)} {{{Amount} {EnemyIcName}}}";
        }
    }
}
