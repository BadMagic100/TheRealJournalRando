using ItemChanger;

namespace TheRealJournalRando.IC
{
    public class EnemyJournalEntryOnlyItem : AbstractItem
    {
        public string playerDataName = "";
        private ProgressiveJournalPairModule? pjp;

        public EnemyJournalEntryOnlyItem(string playerDataName)
        {
            this.playerDataName = playerDataName;
        }

        protected override void OnLoad()
        {
            JournalControlModule journal = ItemChangerMod.Modules.GetOrAdd<JournalControlModule>();
            journal.RegisterEnemyEntry(playerDataName);

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
            string hasEntryBool = nameof(JournalControlModule.hasEntry) + playerDataName;
            string firstKilledBool = "newData" + playerDataName;
            if (!PlayerData.instance.GetBool(hasEntryBool))
            {
                PlayerData.instance.SetBool(hasEntryBool, true);
                PlayerData.instance.SetBool(firstKilledBool, true);
            }
        }

        private void OnModifyItem(GiveEventArgs args)
        {
            JournalItemPair? pair = pjp?.journalPairs[playerDataName];
            if (args.Item.Redundant() && pair?.IsPair == true)
            {
                args.Item = pair.notesItem!.Clone();
            }
        }

        public override bool Redundant()
        {
            string hasEntryBool = nameof(JournalControlModule.hasEntry) + playerDataName;
            return PlayerData.instance.GetBool(hasEntryBool);
        }
    }
}
