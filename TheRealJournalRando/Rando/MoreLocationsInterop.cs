using MoreLocations.Rando.Costs;
using RandomizerCore.Logic;
using System;
using System.Collections.Generic;
using System.Linq;
using TheRealJournalRando.Data;

namespace TheRealJournalRando.Rando
{
    internal static class MoreLocationsInterop
    {
        public static void Hook()
        {
            MoreLocations.Rando.ConnectionInterop.AddRandoCostProviderToJunkShop(CanProvideCosts, ConstructProvider);
        }

        private static bool CanProvideCosts()
        {
            bool anyPoolsEnabled = RandoInterop.Settings.Pools.RegularEntries
                || RandoInterop.Settings.Pools.BossEntries
                || RandoInterop.Settings.Pools.BonusEntries;

            return RandoInterop.Settings.Enabled 
                && RandoInterop.Settings.JournalRandomizationType != JournalRandomizationType.None
                && anyPoolsEnabled;
        }

        private static JournalCostProvider ConstructProvider() => new();
    }

    internal class JournalCostProvider : ICostProvider
    {
        public bool HasNonFreeCostsAvailable => true;

        public LogicCost Next(LogicManager lm, Random rng)
        {
            // take only costs for enemies that are costable; ignore long locations.
            List<EnemyDef> enemies = EnemyData.Enemies.Values
                .Where(x => !x.unkillable && !x.requestAddIgnore)
                .Where(RequestModifier.IsEnemyEnabled)
                .ToList();

            // pick an eligible enemy and an amount
            EnemyDef enemy = enemies[rng.Next(enemies.Count)];
            int amount = 1;
            if (RandoInterop.Settings.JournalRandomizationType.HasFlag(JournalRandomizationType.NotesOnly))
            {
                double weight = RequestModifier.ComputeCostWeight(rng);
                amount = (int)Math.Max(1, Math.Round(enemy.notesCost * weight));
            }

            return RequestModifier.BuildCostForEnemy(enemy, lm, amount);
        }

        public void PreRandomize(Random rng) { }
    }
}
