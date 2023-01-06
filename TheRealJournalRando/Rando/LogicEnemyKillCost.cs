using Newtonsoft.Json;
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
        public bool Respawns { get; init; }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        [JsonConstructor]
        private LogicEnemyKillCost() { }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

        public LogicEnemyKillCost(LogicManager lm, string enemyIcName, bool respawns, int amount)
        {
            EnemyIcName = enemyIcName;
            Amount = amount;
            Respawns = respawns;
            CanBenchWaypoint = lm.GetTerm("Can_Bench")
                ?? throw new MissingTermException($"Can_Bench not found");
            // use our own custom-defined "Defeated_Any" waypoints to make sure we can cover our special cases
            DefeatWaypoint = lm.GetTerm($"Defeated_Any_{enemyIcName}") 
                ?? throw new MissingTermException($"Defeated_Any_{enemyIcName} not found");
        }

        public override bool CanGet(ProgressionManager pm)
        {
            if (Amount == 0)
            {
                return true;
            }

            bool canDefeatEnemy = pm.Has(DefeatWaypoint.Id);
            bool hasRequiredBench = Amount < 2 || Respawns || pm.Has(CanBenchWaypoint.Id);
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
