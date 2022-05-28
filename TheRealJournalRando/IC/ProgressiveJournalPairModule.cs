using ItemChanger.Modules;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace TheRealJournalRando.IC
{
    public class JournalItemPair
    {
        public EnemyJournalEntryOnlyItem? entryItem;
        public EnemyJournalNotesOnlyItem? notesItem;
        public bool IsPair => entryItem != null && notesItem != null;
    }

    public class ProgressiveJournalPairModule : Module
    {
        [JsonIgnore]
        public Dictionary<string, JournalItemPair> journalPairs = new();

        public override void Initialize() { }

        public override void Unload() { }

        private void EnsurePairLookup(string pdName)
        {
            if (!journalPairs.ContainsKey(pdName))
            {
                journalPairs[pdName] = new();
            }
        }

        public void Register(EnemyJournalEntryOnlyItem item)
        {
            EnsurePairLookup(item.playerDataName);
            journalPairs[item.playerDataName].entryItem = item;
        }

        public void Register(EnemyJournalNotesOnlyItem item)
        {
            EnsurePairLookup(item.playerDataName);
            journalPairs[item.playerDataName].notesItem = item;
        }
    }
}
