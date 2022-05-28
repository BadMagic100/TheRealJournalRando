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
            ItemChangerMod.Modules.GetOrAdd<JournalControlModule>().RegisterEnemyNotes(playerDataName);
            ItemChangerMod.Modules.GetOrAdd<ProgressiveJournalPairModule>().Register(this);
        }

        public override void GiveImmediate(GiveInfo info)
        {
            PlayerData.instance.SetBool(nameof(JournalControlModule.hasNotes) + playerDataName, true);
        }

        public override bool Redundant()
        {
            return PlayerData.instance.GetBool(nameof(JournalControlModule.hasNotes) + playerDataName);
        }
    }
}
