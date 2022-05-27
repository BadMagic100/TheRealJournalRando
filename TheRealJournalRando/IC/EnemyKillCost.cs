using ItemChanger;

namespace TheRealJournalRando.IC
{
    public record EnemyKillCost(string EnemyType, int Total) : Cost
    {
        private JournalControlModule? module;

        public override void Load()
        {
            module = ItemChangerMod.Modules.GetOrAdd<JournalControlModule>();
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
            return $"Defeat {GetBalanceDue()} more";
        }
    }
}
