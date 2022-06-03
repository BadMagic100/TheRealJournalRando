using ItemChanger;
using ItemChanger.Tags;
using ItemChanger.UIDefs;
using Modding;
using System;
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
                Rando.RandoInterop.HookRandomizer();
            }

            Log("Initialized");
        }

        public void HookIC()
        {
            Events.OnItemChangerHook += LanguageData.Hook;
            Events.OnItemChangerUnhook += LanguageData.Unhook;

            foreach (MinimalEnemyDef enemyDef in EnemyData.NormalData.Values)
            {
                string entryName = $"Journal_Entry_Only-{enemyDef.icName}";
                string notesName = $"Hunter's_Notes-{enemyDef.icName}";
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
                        new ImplicitCostTag()
                        {
                            Cost = EnemyKillCost.ConstructEntryCost(enemyDef.icName),
                            Inherent = false,
                        },
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
                        new ImplicitCostTag()
                        {
                            Cost = EnemyKillCost.ConstructNotesCost(enemyDef.icName),
                            Inherent = false,
                        },
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
