using ItemChanger;
using ItemChanger.Items;
using ItemChanger.Locations;
using ItemChanger.Tags;
using ItemChanger.UIDefs;
using Modding;
using System;
using System.Collections.Generic;
using System.Linq;
using TheRealJournalRando.Data;
using TheRealJournalRando.Data.Generated;
using TheRealJournalRando.IC;
using EmbeddedSprite = TheRealJournalRando.IC.EmbeddedSprite;
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

            foreach (EnemyDef enemyDef in EnemyData.Enemies.Values.Where(x => !x.icIgnore))
            {
                DefineStandardEntryAndNoteItems(enemyDef);
                DefineStandardEntryAndNoteLocations(enemyDef);
            }

            // mossy vagabond items and locations
            EnemyDef mossyVagabond = EnemyData.Enemies[EnemyNames.Mossy_Vagabond];
            DefineStandardEntryAndNoteItems(mossyVagabond);
            Finder.DefineCustomLocation(new DualLocation()
            {
                name = EnemyNames.Mossy_Vagabond.AsEntryName(),
                sceneName = SceneNames.Fungus3_39,
                flingType = FlingType.Everywhere,
                falseLocation = new EnemyJournalLocation(mossyVagabond.pdName, EnemyJournalLocationType.Entry),
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
                        // this will be unloaded, but it will still be returned in GetPlacementAndLocationTags and our soft deps don't check load state.
                        InteropTagFactory.CmiSharedTag(poolGroup: JOURNAL_ENTRIES),
                    }
                },
                Test = new PDBool(nameof(PlayerData.crossroadsInfected))
            });
            Finder.DefineCustomLocation(new DualLocation()
            {
                name = EnemyNames.Mossy_Vagabond.AsNotesName(),
                sceneName = SceneNames.Fungus3_39,
                flingType = FlingType.Everywhere,
                falseLocation = new EnemyJournalLocation(mossyVagabond.pdName, EnemyJournalLocationType.Notes),
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
                    }
                },
                Test = new PDBool(nameof(PlayerData.crossroadsInfected))
            });

            // weathered mask items and locations
            DefineFullEntryItem(EnemyData.Enemies[EnemyNames.Weathered_Mask]);
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
                    },
                    InteropTagFactory.CmiSharedTag(poolGroup: JOURNAL_ENTRIES),
                }
            });

            // void idol items and locations
            for (int i = 0; i < 3; i++)
            {
                DefineVoidIdolEntryItem(i);
                DefineVoidIdolLocation(i);
            }

            // hunter's mark items and locations
            DefineHunterMarkItem();
            Finder.DefineCustomLocation(new HuntersMarkLocation()
            {
                name = EnemyNames.Hunters_Mark,
                sceneName = SceneNames.Fungus1_08,
                objectName = "Hunter Entry/Shiny Item HunterMark",
                flingType = FlingType.Everywhere,
                elevation = -1.7f,
                tags = new List<Tag>()
                {
                    InteropTagFactory.CmiSharedTag(poolGroup: JOURNAL_ENTRIES)
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
                },
                tags = new()
                {
                    InteropTagFactory.CmiSharedTag(poolGroup: JOURNAL_ENTRIES)
                }
            });
        }

        private void DefineVoidIdolEntryItem(int tier)
        {
            EnemyDef enemyDef = tier switch
            {
                0 => EnemyData.Enemies[EnemyNames.Void_Idol_1],
                1 => EnemyData.Enemies[EnemyNames.Void_Idol_2],
                2 => EnemyData.Enemies[EnemyNames.Void_Idol_3],
                _ => throw new NotImplementedException()
            };
            string name = enemyDef.icName.AsEntryName();
            LanguageString localizedEnemyName = new("Journal", $"NAME_{enemyDef.convoName}");
            string? prev = tier == 0 ? null : $"{EnemyNames.Void_Idol_Prefix}{tier}".AsEntryName();
            string? next = tier == 2 ? null : $"{EnemyNames.Void_Idol_Prefix}{tier + 2}".AsEntryName();
            Finder.DefineCustomItem(new JournalEntryItem()
            {
                name = name,
                playerDataName = enemyDef.pdName,
                UIDef = new MsgUIDef()
                {
                    name = new FormatString(new LanguageString("Fmt", "ENTRY_ITEM_NAME"), localizedEnemyName.Clone()),
                    shopDesc = new PaywallString("Journal", $"NOTE_{enemyDef.convoName}"),
                    sprite = new JournalBadgeSprite(enemyDef.pdName),
                },
                tags = new()
                {
                    InteropTagFactory.CmiSharedTag(poolGroup: JOURNAL_ENTRIES),
                    new ItemChainTag()
                    {
                        predecessor = prev,
                        successor = next,
                    }
                }
            });
        }

        private void DefineHunterMarkItem()
        {
            EnemyDef def = EnemyData.Enemies[EnemyNames.Hunters_Mark];
            string name = def.icName;
            LanguageString localizedName = new("Journal", $"NAME_{def.convoName}");
            Finder.DefineCustomItem(new JournalEntryItem()
            {
                name = name,
                playerDataName = def.pdName,
                UIDef = new BigUIDef()
                {
                    name = localizedName,
                    bigSprite = new EmbeddedSprite("HunterMark-Lg"),
                    take = new LanguageString("Prompts", "GET_ITEM_INTRO1"),
                    descOne = new LanguageString("Prompts", "GET_HUNTERMARK_1"),
                    descTwo = new LanguageString("Prompts", "GET_HUNTERMARK_2"),
                    shopDesc = new BoxedString("The mark of a true Hunter. I guess they hand these out to anyone these days."),
                    sprite = new EmbeddedSprite("HunterMark-Sm"),
                },
                tags = new()
                {
                    InteropTagFactory.CmiSharedTag(poolGroup: JOURNAL_ENTRIES)
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

        private void DefineVoidIdolLocation(int tier)
        {
            EnemyDef def = tier switch
            {
                0 => EnemyData.Enemies[EnemyNames.Void_Idol_1],
                1 => EnemyData.Enemies[EnemyNames.Void_Idol_2],
                2 => EnemyData.Enemies[EnemyNames.Void_Idol_3],
                _ => throw new NotImplementedException()
            };

            Finder.DefineCustomLocation(new VoidIdolLocation()
            {
                name = def.icName.AsEntryName(),
                tier = tier,
                flingType = FlingType.Everywhere,
                sceneName = SceneNames.GG_Workshop,
                tags = new()
                {
                    InteropTagFactory.CmiSharedTag(poolGroup: JOURNAL_ENTRIES)
                }
            });
        }

        public void OnLoadGlobal(GlobalSettings s) => GS = s;

        public GlobalSettings OnSaveGlobal() => GS;
    }
}
