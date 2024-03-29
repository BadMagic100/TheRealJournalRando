﻿using ItemChanger;

namespace TheRealJournalRando.IC
{
    public class EnemyJournalEntryOnlyItem : AbstractItem
    {
        public string playerDataName = "";

        public EnemyJournalEntryOnlyItem(string playerDataName)
        {
            this.playerDataName = playerDataName;
        }

        protected override void OnLoad()
        {
            ItemChangerMod.Modules.GetOrAdd<JournalControlModule>().RegisterEnemyEntry(playerDataName);
            ItemChangerMod.Modules.GetOrAdd<ProgressiveJournalPairModule>().Register(this);
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

        public override bool Redundant()
        {
            string hasEntryBool = nameof(JournalControlModule.hasEntry) + playerDataName;
            return PlayerData.instance.GetBool(hasEntryBool);
        }
    }
}
