﻿using ItemChanger;

namespace TheRealJournalRando.IC
{
    public record EnemyKillCost(string EnemyType, int Total) : Cost
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
            return Total - module!.GetKillCount(EnemyType);
        }

        public override bool CanPay() => GetBalanceDue() <= 0;

        public override void OnPay() { }

        public override string GetCostText()
        {
            int bal = GetBalanceDue();
            if (bal == 1)
            {
                return string.Format(Language.Language.Get("DEFEAT_ENEMY", "Fmt"), EnemyType);
            }
            else if (bal > 1)
            {
                return string.Format(Language.Language.Get("DEFEAT_ENEMIES", "Fmt"), bal, EnemyType);
            }
            else
            {
                return string.Format(Language.Language.Get("DEFEATED_ENEMIES", "Fmt"), EnemyType);
            }
        }
    }
}