using ItemChanger;

namespace TheRealJournalRando.IC
{
    public class EnemyJournalNotesOnlyItem : AbstractItem
    {
        public string playerDataName = "";

        protected override void OnLoad()
        {
            EnemyJournalInterceptModule journal = ItemChangerMod.Modules.GetOrAdd<EnemyJournalInterceptModule>();
            journal.RegisterEnemyNotes(playerDataName);
        }

        public override void GiveImmediate(GiveInfo info)
        {
            TheRealJournalRando.Instance.LogDebug($"Giving {this.name}");
            PlayerData.instance.SetBool(nameof(EnemyJournalInterceptModule.hasNotes) + playerDataName, true);
        }

        public override bool Redundant()
        {
            return PlayerData.instance.GetBool(nameof(EnemyJournalInterceptModule.hasNotes) + playerDataName);
        }
    }
}
