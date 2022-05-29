using ItemChanger;
using ItemChanger.Modules;
using ItemChanger.Placements;
using ItemChanger.Tags;
using ItemChanger.UIDefs;
using Modding;
using System;
using TheRealJournalRando.Data;
using TheRealJournalRando.IC;

namespace TheRealJournalRando
{
    public class TheRealJournalRando : Mod
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

        public override string GetVersion() => GetType().Assembly.GetName().Version.ToString();

        public TheRealJournalRando() : base()
        {
            _instance = this;
        }

        (string, string)[] bothMappings =
        {
            ("Squit", "Mosquito"),
            ("Tiktik", "Climber"),
            ("Lifeseed", "HealthScuttler"),
            ("Crawlid", "Crawler")
        };
        (string, string)[] entryOnlyMappings =
        {
            ("Pure Vessel", "HollowKnightPrime"),
            ("Husk Hive", "ZombieHive")
        };
        (string, string)[] noteOnlyMappings =
        {
            ("Vengefly", "Buzzer"),
            ("Gruzzer", "Bouncer"),
        };

        public override void Initialize()
        {
            Log("Initializing");

            On.UIManager.StartNewGame += UIManager_StartNewGame;
            AbstractItem.ModifyRedundantItemGlobal += AbstractItem_ModifyRedundantItemGlobal;

            HookIC();

            Log("Initialized");
        }

        private void AbstractItem_ModifyRedundantItemGlobal(GiveEventArgs obj)
        {
            obj.Item = null;
        }

        private void UIManager_StartNewGame(On.UIManager.orig_StartNewGame orig, UIManager self, bool permaDeath, bool bossRush)
        {
            ItemChangerMod.CreateSettingsProfile(false, false);
            ItemChangerMod.Modules.GetOrAdd<AutoUnlockIselda>();

            AbstractPlacement iseldaShop = Finder.GetLocation(LocationNames.Iselda).Wrap();
            iseldaShop.Items.Add(Finder.GetItem(ItemNames.Hunters_Journal));
            foreach ((string name, _) in bothMappings)
            {
                string icName = name.Replace(' ', '_');
                iseldaShop.Items.Add(Finder.GetItem($"Journal_Entry_Only-{icName}"));
                iseldaShop.Items.Add(Finder.GetItem($"Hunter's_Notes-{icName}"));
            }
            foreach ((string name, _) in entryOnlyMappings)
            {
                string icName = name.Replace(' ', '_');
                iseldaShop.Items.Add(Finder.GetItem($"Journal_Entry_Only-{icName}"));
            }
            foreach ((string name, _) in noteOnlyMappings)
            {
                string icName = name.Replace(' ', '_');
                iseldaShop.Items.Add(Finder.GetItem($"Hunter's_Notes-{icName}"));
            }
            iseldaShop.Items[0].AddTag<CostTag>().Cost = new EnemyKillCost("Crawler", 2);

            AbstractPlacement tiktikEntry = Finder.GetLocation("Journal_Entry_Only-Tiktik").Wrap();
            ((ISingleCostPlacement)tiktikEntry).Cost = new EnemyKillCost("Climber", 1);
            tiktikEntry.Items.Add(Finder.GetItem("Soul_Totem-Path_of_Pain"));
            tiktikEntry.Items.Add(Finder.GetItem("Vengeful_Spirit"));

            AbstractPlacement maskflyNotes = Finder.GetLocation("Hunter's_Notes-Maskfly").Wrap();
            ((ISingleCostPlacement)maskflyNotes).Cost = new EnemyKillCost("Pigeon", 15);
            maskflyNotes.Items.Add(Finder.GetItem("Mantis_Claw"));

            AbstractPlacement vengeflyScamEntry = Finder.GetLocation("Journal_Entry_Only-Vengefly").Wrap();
            ((ISingleCostPlacement)vengeflyScamEntry).Cost = new MultiCost(new EnemyKillCost("Buzzer", 1), new GeoCost(50));
            vengeflyScamEntry.Items.Add(Finder.GetItem("Rancid_Egg"));

            ItemChangerMod.AddPlacements(new[] {iseldaShop, tiktikEntry, maskflyNotes, vengeflyScamEntry});

            orig(self, permaDeath, bossRush);
        }

        public void HookIC()
        {
            Events.OnItemChangerHook += LanguageData.Hook;
            Events.OnItemChangerUnhook += LanguageData.Unhook;

            foreach (MinimalEnemyDef enemyDef in EnemyData.Data)
            {
                string icName = enemyDef.name.Replace(' ', '_');
                string entryName = $"Journal_Entry_Only-{icName}";
                string notesName = $"Hunter's_Notes-{icName}";

                Finder.DefineCustomItem(new EnemyJournalEntryOnlyItem(enemyDef.pdName)
                {
                    name = entryName,
                    UIDef = new MsgUIDef
                    {
                        name = new FormatString(new LanguageString("Fmt", "ENTRY_ITEM_NAME"), enemyDef.name),
                        shopDesc = new FormatString(new LanguageString("Fmt", "ENTRY_SHOP_DESC"), enemyDef.name),
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
                        name = new FormatString(new LanguageString("Fmt", "NOTES_ITEM_NAME"), enemyDef.name),
                        shopDesc = new FormatString(new LanguageString("Fmt", "NOTES_SHOP_DESC"), enemyDef.name),
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
                            Cost = new EnemyKillCost(enemyDef.pdName, 1),
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
                            Cost = new EnemyKillCost(enemyDef.pdName, enemyDef.notesCost),
                            Inherent = false,
                        },
                        InteropTagFactory.CmiSharedTag(poolGroup: JOURNAL_ENTRIES),
                        InteropTagFactory.RecentItemsLocationTag(sourceOverride: "the Hunter")
                    }
                });
            }
        }
    }
}
