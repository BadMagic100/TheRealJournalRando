using ItemChanger;
using TheRealJournalRando.Data;

namespace TheRealJournalRando.IC
{
    public record EnemyKillCost(string EnemyPdName, string EnemyConvoName, int Total) : Cost
    {
        private JournalKillCounterModule? module;

        public override void Load()
        {
            module = ItemChangerMod.Modules.GetOrAdd<JournalKillCounterModule>();
        }

        public override bool HasPayEffects() => false;

        public int GetBalanceDue()
        {
            if (module == null)
            {
                Load();
            }
            return Total - module!.GetKillCount(EnemyPdName);
        }

        public override bool Includes(Cost c) => c is EnemyKillCost ekc && ekc.EnemyPdName == EnemyPdName && ekc.Total <= Total;

        public override bool CanPay() => GetBalanceDue() <= 0;

        public override void OnPay() { }

        public override string GetCostText()
        {
            int bal = GetBalanceDue();
            string localizedEnemyName = Language.Language.Get($"NAME_{EnemyConvoName}", "Journal");
            if (bal == 1)
            {
                return string.Format(Language.Language.Get("DEFEAT_ENEMY", "Fmt"), localizedEnemyName);
            }
            else if (bal > 1)
            {
                return string.Format(Language.Language.Get("DEFEAT_ENEMIES", "Fmt"), bal, localizedEnemyName);
            }
            else
            {
                return string.Format(Language.Language.Get("DEFEATED_ENEMIES", "Fmt"), localizedEnemyName);
            }
        }

        public static EnemyKillCost ConstructCost(string icKey, int amount)
        {
            try
            {
                EnemyDef def = EnemyData.Enemies[icKey];
                return new EnemyKillCost(def.pdName, def.convoName, amount);
            }
            catch
            {
                TheRealJournalRando.Instance.LogError($"Failed to construct cost: {amount} {icKey}");
                throw;
            }
        }
    }
}
