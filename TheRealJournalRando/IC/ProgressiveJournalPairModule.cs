using ItemChanger;
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

        public override void Unload()
        {
            foreach (JournalItemPair pair in journalPairs.Values)
            {
                if (pair.entryItem != null)
                {
                    pair.entryItem.ModifyItem -= OnModifyItem;
                    pair.entryItem = null;
                }
                if (pair.notesItem != null)
                {
                    pair.notesItem.ModifyItem -= OnModifyItem;
                    pair.notesItem = null;
                }
            }
            journalPairs.Clear();
        }

        private void EnsurePairLookup(string pdName)
        {
            if (!journalPairs.ContainsKey(pdName))
            {
                journalPairs[pdName] = new();
            }
        }

        private void OnModifyItem(GiveEventArgs args)
        {
            if (args.Item is EnemyJournalEntryOnlyItem ei && ei.Redundant())
            {
                JournalItemPair pair = journalPairs[ei.playerDataName];
                if (pair.IsPair)
                {
                    args.Item = pair.notesItem!.Clone();
                }
            }
            else if (args.Item is EnemyJournalNotesOnlyItem ni && !ni.Redundant())
            {
                JournalItemPair pair = journalPairs[ni.playerDataName];
                if (pair.IsPair)
                {
                    AbstractItem ei2 = pair.entryItem!.Clone();
                    if (!ei2.Redundant())
                    {
                        args.Item = ei2;
                    }
                }
            }
        }

        public void Register(EnemyJournalEntryOnlyItem item)
        {
            EnsurePairLookup(item.playerDataName);
            journalPairs[item.playerDataName].entryItem = item;
            item.ModifyItem += OnModifyItem;
        }

        public void Register(EnemyJournalNotesOnlyItem item)
        {
            EnsurePairLookup(item.playerDataName);
            journalPairs[item.playerDataName].notesItem = item;
            item.ModifyItem += OnModifyItem;
        }
    }
}
