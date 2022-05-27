using ItemChanger;

namespace TheRealJournalRando.IC
{
    public class EnemyJournalNotesOnlyItem : AbstractItem
    {
        public string playerDataName = "";

        public EnemyJournalNotesOnlyItem(string playerDataName)
        {
            this.playerDataName = playerDataName;
        }

        protected override void OnLoad()
        {
            JournalControlModule journal = ItemChangerMod.Modules.GetOrAdd<JournalControlModule>();
            journal.RegisterEnemyNotes(playerDataName);
        }

        public override void GiveImmediate(GiveInfo info)
        {
            TheRealJournalRando.Instance.LogDebug($"Giving {this.name}");
            PlayerData.instance.SetBool(nameof(JournalControlModule.hasNotes) + playerDataName, true);
        }

        public override bool Redundant()
        {
            return PlayerData.instance.GetBool(nameof(JournalControlModule.hasNotes) + playerDataName);
        }
    }
}
