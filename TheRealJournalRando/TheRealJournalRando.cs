using ItemChanger;
using ItemChanger.UIDefs;
using Modding;
using Newtonsoft.Json;
using RandomizerCore.Logic;
using System;
using System.Collections.Generic;
using TheRealJournalRando.Data;
using TheRealJournalRando.IC;

namespace TheRealJournalRando
{
    public class TheRealJournalRando : Mod, IGlobalSettings<GlobalSettings>
    {
        const string JOURNAL_ENTRIES = "Journal Entries";

        private static TheRealJournalRando? _instance;

        internal static TheRealJournalRando Instance
        {
            get
            {
                if (_instance == null)
                {
                    throw new InvalidOperationException($"{nameof(TheRealJournalRando)} was never initialized");
                }
                return _instance;
            }
        }

        public GlobalSettings GS { get; set; } = new();

        public override string GetVersion() => GetType().Assembly.GetName().Version.ToString();

        public TheRealJournalRando() : base()
        {
            _instance = this;
        }

        public override void Initialize()
        {
            Log("Initializing");

            HookIC();

            if (ModHooks.GetMod("Randomizer 4") is Mod)
            {
                List<RawLogicDef> logic = new();
                foreach(EnemyDef def in EnemyData.NormalData.Values)
                {
                    logic.Add(new($"Defeated_Any_{def.icName}", "ANY"));
                }
                Log(JsonConvert.SerializeObject(logic));
                Rando.RandoInterop.HookRandomizer();
            }

            Log("Initialized");
        }

        public void HookIC()
        {
            Events.OnItemChangerHook += LanguageData.Hook;
            Events.OnItemChangerUnhook += LanguageData.Unhook;

            foreach (EnemyDef enemyDef in EnemyData.NormalData.Values)
            {
                string entryName = enemyDef.icName.AsEntryName();
                string notesName = enemyDef.icName.AsNotesName();
                LanguageString localizedEnemyName = new("Journal", $"NAME_{enemyDef.convoName}");

                Finder.DefineCustomItem(new EnemyJournalEntryOnlyItem(enemyDef.pdName)
                {
                    name = entryName,
                    UIDef = new MsgUIDef
                    {
                        name = new FormatString(new LanguageString("Fmt", "ENTRY_ITEM_NAME"), localizedEnemyName.Clone()),
                        shopDesc = new FormatString(new LanguageString("Fmt", "ENTRY_SHOP_DESC"), localizedEnemyName.Clone()),
                        sprite = new JournalBadgeSprite(enemyDef.pdName),
                    },
                    tags = new()
                    {
                        InteropTagFactory.CmiSharedTag(poolGroup: JOURNAL_ENTRIES)
                    }
                });
                Finder.DefineCustomItem(new EnemyJournalNotesOnlyItem(enemyDef.pdName)
                {
                    name = notesName,
                    UIDef = new MsgUIDef
                    {
                        name = new FormatString(new LanguageString("Fmt", "NOTES_ITEM_NAME"), localizedEnemyName.Clone()),
                        shopDesc = new FormatString(new LanguageString("Fmt", "NOTES_SHOP_DESC"), localizedEnemyName.Clone()),
                        sprite = new JournalBadgeSprite(enemyDef.pdName),
                    },
                    tags = new()
                    {
                        InteropTagFactory.CmiSharedTag(poolGroup: JOURNAL_ENTRIES)
                    }
                });

                Finder.DefineCustomLocation(new EnemyJournalLocation(enemyDef.pdName, EnemyJournalLocationType.Entry)
                {
                    name = entryName,
                    flingType = FlingType.Everywhere,
                    tags = new()
                    {
                        InteropTagFactory.CmiSharedTag(poolGroup: JOURNAL_ENTRIES),
                        InteropTagFactory.RecentItemsLocationTag(sourceOverride: "the Hunter")
                    }
                });
                Finder.DefineCustomLocation(new EnemyJournalLocation(enemyDef.pdName, EnemyJournalLocationType.Notes)
                {
                    name = notesName,
                    flingType = FlingType.Everywhere,
                    tags = new()
                    {
                        InteropTagFactory.CmiSharedTag(poolGroup: JOURNAL_ENTRIES),
                        InteropTagFactory.RecentItemsLocationTag(sourceOverride: "the Hunter")
                    }
                });
            }
        }

        public void OnLoadGlobal(GlobalSettings s) => GS = s;

        public GlobalSettings OnSaveGlobal() => GS;
    }
}
