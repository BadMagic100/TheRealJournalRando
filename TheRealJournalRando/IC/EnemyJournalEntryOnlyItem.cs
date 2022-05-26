using ItemChanger;

namespace TheRealJournalRando.IC
{
    public class EnemyJournalEntryOnlyItem : AbstractItem
    {
        public string playerDataName = "";

        protected override void OnLoad()
        {
            EnemyJournalInterceptModule journal = ItemChangerMod.Modules.GetOrAdd<EnemyJournalInterceptModule>();
            journal.RegisterEnemyEntry(playerDataName);
        }

        public override void GiveImmediate(GiveInfo info)
        {
            string hasEntryBool = nameof(EnemyJournalInterceptModule.hasEntry) + playerDataName;
            string firstKilledBool = "newData" + playerDataName;
            if (!PlayerData.instance.GetBool(hasEntryBool))
            {
                PlayerData.instance.SetBool(hasEntryBool, true);
                PlayerData.instance.SetBool(firstKilledBool, true);
            }
        }

        public override bool Redundant()
        {
            string hasEntryBool = nameof(EnemyJournalInterceptModule.hasEntry) + playerDataName;
            return PlayerData.instance.GetBool(hasEntryBool);
        }
    }
}
