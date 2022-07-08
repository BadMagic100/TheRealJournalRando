using ItemChanger;
using ItemChanger.Items;
using ItemChanger.Locations;
using ItemChanger.Tags;
using ItemChanger.UIDefs;
using Modding;
using System;
using System.Collections.Generic;
using TheRealJournalRando.Data;
using TheRealJournalRando.Data.Generated;
using TheRealJournalRando.IC;
using FormatString = TheRealJournalRando.IC.FormatString;

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

            Finder.GetItemOverride += ReplaceOriginalJournalUIDefs;

            Container.DefineContainer<MossCorpseContainer>();

            // todo - deal with special ones

            foreach (EnemyDef enemyDef in EnemyData.NormalData.Values)
            {
                DefineStandardEntryAndNoteItems(enemyDef);
                DefineStandardEntryAndNoteLocations(enemyDef);
            }

            // mossy vagabond items and locations
            DefineStandardEntryAndNoteItems(EnemyData.SpecialData.Mossy_Vagabond);
            Finder.DefineCustomLocation(new DualLocation()
            {
                name = EnemyNames.Mossy_Vagabond.AsEntryName(),
                sceneName = SceneNames.Fungus3_39,
                flingType = FlingType.Everywhere,
                falseLocation = new EnemyJournalLocation(EnemyData.SpecialData.Mossy_Vagabond.pdName, EnemyJournalLocationType.Entry),
                trueLocation = new ExistingFsmContainerLocation()
                {
                    sceneName = SceneNames.Fungus3_39,
                    objectName = "Mossman Inspect",
                    fsmName = "Conversation Control",
                    containerType = MossCorpseContainer.MossCorpse,
                    tags = new()
                    {
                        new DualLocationMutableContainerTag(),
                        new DestroyOnECLReplaceTag()
                        {
                            sceneName = SceneNames.Fungus3_39,
                            objectPath = "corpse set/fat_moss_knight_dead0000"
                        },
                        new SuppressSingleCostTag()
                        {
                            costMatcher = new MossyVagabondKillCostMatcher(),
                        },
                        // these will be unloaded, but they will still be returned in GetPlacementAndLocationTags and our soft deps don't check load state.
                        InteropTagFactory.CmiSharedTag(poolGroup: JOURNAL_ENTRIES),
                        InteropTagFactory.RecentItemsLocationTag(sourceOverride: "the Hunter"),
                    }
                },
                Test = new PDBool(nameof(PlayerData.crossroadsInfected))
            });
            Finder.DefineCustomLocation(new DualLocation()
            {
                name = EnemyNames.Mossy_Vagabond.AsNotesName(),
                sceneName = SceneNames.Fungus3_39,
                flingType = FlingType.Everywhere,
                falseLocation = new EnemyJournalLocation(EnemyData.SpecialData.Mossy_Vagabond.pdName, EnemyJournalLocationType.Notes),
                trueLocation = new ExistingFsmContainerLocation()
                {
                    sceneName = SceneNames.Fungus3_39,
                    objectName = "Mossman Inspect (1)",
                    fsmName = "Conversation Control",
                    containerType = MossCorpseContainer.MossCorpse,
                    tags = new()
                    {
                        new DualLocationMutableContainerTag(),
                        new DestroyOnECLReplaceTag()
                        {
                            sceneName = SceneNames.Fungus3_39,
                            objectPath = "corpse set/fat_moss_knight_dead0000 (2)"
                        },
                        new SuppressSingleCostTag()
                        {
                            costMatcher = new MossyVagabondKillCostMatcher(),
                        },
                        InteropTagFactory.CmiSharedTag(poolGroup: JOURNAL_ENTRIES),
                        InteropTagFactory.RecentItemsLocationTag(sourceOverride: "the Hunter"),
                    }
                },
                Test = new PDBool(nameof(PlayerData.crossroadsInfected))
            });

            // weathered mask items and locations
            DefineFullEntryItem(EnemyData.SpecialData.Weathered_Mask);
            Finder.DefineCustomLocation(new ObjectLocation()
            {
                name = EnemyNames.Weathered_Mask.AsEntryName(),
                sceneName = SceneNames.GG_Land_of_Storms,
                objectName = "Shiny Item GG Storms",
                flingType = FlingType.DirectDeposit,
                forceShiny = true,
                tags = new List<Tag>()
                {
                    new ChangeSceneTag()
                    {
                        changeTo = new Transition(SceneNames.GG_Atrium_Roof, "door_Land_of_Storms_return"),
                        dreamReturn = true,
                        deactivateNoCharms = true,
                    }
                }
            });
        }

        private void ReplaceOriginalJournalUIDefs(GetItemEventArgs args)
        {
            switch (args.ItemName)
            {
                case ItemNames.Journal_Entry_Goam:
                case ItemNames.Journal_Entry_Garpede:
                case ItemNames.Journal_Entry_Charged_Lumafly:
                case ItemNames.Journal_Entry_Void_Tendrils:
                case ItemNames.Journal_Entry_Seal_of_Binding:
                    if (Finder.GetItemInternal(args.ItemName) is JournalEntryItem jei && jei.UIDef is MsgUIDef md)
                    {
                        md.sprite = new JournalBadgeSprite(jei.playerDataName);
                        args.Current = jei;
                    }
                    break;
                default:
                    break;
            }
        }

        private void DefineStandardEntryAndNoteItems(EnemyDef enemyDef)
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
        }

        private void DefineFullEntryItem(EnemyDef enemyDef)
        {
            string name = enemyDef.icName.AsEntryName();
            LanguageString localizedEnemyName = new("Journal", $"NAME_{enemyDef.convoName}");
            Finder.DefineCustomItem(new JournalEntryItem()
            {
                name = name,
                playerDataName = enemyDef.pdName,
                UIDef = new MsgUIDef()
                {
                    name = new FormatString(new LanguageString("Fmt", "ENTRY_ITEM_NAME"), localizedEnemyName.Clone()),
                    shopDesc = new PaywallString("Journal", $"NOTE_{enemyDef.convoName}"),
                    sprite = new JournalBadgeSprite(enemyDef.pdName),
                }
            });
        }

        private void DefineStandardEntryAndNoteLocations(EnemyDef enemyDef)
        {
            string entryName = enemyDef.icName.AsEntryName();
            string notesName = enemyDef.icName.AsNotesName();

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

        public void OnLoadGlobal(GlobalSettings s) => GS = s;

        public GlobalSettings OnSaveGlobal() => GS;
    }
}
