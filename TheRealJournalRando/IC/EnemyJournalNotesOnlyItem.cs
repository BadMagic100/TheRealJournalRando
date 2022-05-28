using ItemChanger;

namespace TheRealJournalRando.IC
{
    public class EnemyJournalNotesOnlyItem : AbstractItem
    {
        public string playerDataName = "";
        private ProgressiveJournalPairModule? pjp;

        public EnemyJournalNotesOnlyItem(string playerDataName)
        {
            this.playerDataName = playerDataName;
        }

        protected override void OnLoad()
        {
            JournalControlModule journal = ItemChangerMod.Modules.GetOrAdd<JournalControlModule>();
            journal.RegisterEnemyNotes(playerDataName);

            pjp = ItemChangerMod.Modules.GetOrAdd<ProgressiveJournalPairModule>();
            pjp.Register(this);

            ModifyItem += OnModifyItem;
        }

        protected override void OnUnload()
        {
            ModifyItem -= OnModifyItem;
        }

        public override void GiveImmediate(GiveInfo info)
        {
            TheRealJournalRando.Instance.LogDebug($"Giving {this.name}");
            PlayerData.instance.SetBool(nameof(JournalControlModule.hasNotes) + playerDataName, true);
        }

        private void OnModifyItem(GiveEventArgs args)
        {
            JournalItemPair? pair = pjp?.journalPairs[playerDataName];
            if (!args.Item.Redundant() && pair?.IsPair == true)
            {
                AbstractItem entry = pair.entryItem!.Clone();
                if (!entry.Redundant())
                {
                    args.Item = entry;
                }
            }
        }

        public override bool Redundant()
        {
            return PlayerData.instance.GetBool(nameof(JournalControlModule.hasNotes) + playerDataName);
        }
    }
}
