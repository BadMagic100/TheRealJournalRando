using ItemChanger;
using ItemChanger.Modules;
using ItemChanger.Placements;
using ItemChanger.Tags;
using ItemChanger.UIDefs;
using Modding;
using System;
using System.Linq;
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
            iseldaShop.Items.Add(Finder.GetItem(ItemNames.Grimmchild2));
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
            iseldaShop.Items[iseldaShop.Items.Count - 1].AddTag<CostTag>().Cost = EnemyKillCost.ConstructCustomCost("Crawlid", 2);

            AbstractPlacement crawlidNotes = Finder.GetLocation("Hunter's_Notes-Crawlid").Wrap();
            ((ISingleCostPlacement)crawlidNotes).Cost = EnemyKillCost.ConstructCustomCost("Crawlid", 5);
            crawlidNotes.AddTag<DisableItemPreviewTag>();
            crawlidNotes.Items.Add(Finder.GetItem(ItemNames.Abyss_Shriek));

            AbstractPlacement tiktikEntry = Finder.GetLocation("Journal_Entry_Only-Tiktik").Wrap();
            ((ISingleCostPlacement)tiktikEntry).Cost = tiktikEntry.GetPlacementAndLocationTags().OfType<ImplicitCostTag>().First().Cost;
            tiktikEntry.Items.Add(Finder.GetItem("Soul_Totem-Path_of_Pain"));
            tiktikEntry.Items.Add(Finder.GetItem("Vengeful_Spirit"));

            AbstractPlacement tiktikNotes = Finder.GetLocation("Hunter's_Notes-Tiktik").Wrap();
            ((ISingleCostPlacement)tiktikNotes).Cost = EnemyKillCost.ConstructCustomCost("Tiktik", 5);
            tiktikNotes.Items.Add(Finder.GetItem("Lifeblood_Cocoon_Small"));
            tiktikNotes.Items.Add(Finder.GetItem("Descending_Dark"));

            AbstractPlacement maskflyNotes = Finder.GetLocation("Hunter's_Notes-Maskfly").Wrap();
            ((ISingleCostPlacement)maskflyNotes).Cost = maskflyNotes.GetPlacementAndLocationTags().OfType<ImplicitCostTag>().First().Cost;
            maskflyNotes.AddTag<DisableCostPreviewTag>();
            maskflyNotes.Items.Add(Finder.GetItem("Mantis_Claw"));

            AbstractPlacement vengeflyScamEntry = Finder.GetLocation("Journal_Entry_Only-Vengefly").Wrap();
            ((ISingleCostPlacement)vengeflyScamEntry).Cost = new MultiCost(EnemyKillCost.ConstructEntryCost("Vengefly"), new GeoCost(50));
            vengeflyScamEntry.Items.Add(Finder.GetItem("Rancid_Egg"));

            AbstractPlacement lifeseedNotes = Finder.GetLocation("Hunter's_Notes-Lifeseed").Wrap();
            ((ISingleCostPlacement)lifeseedNotes).Cost = lifeseedNotes.GetPlacementAndLocationTags().OfType<ImplicitCostTag>().First().Cost;
            lifeseedNotes.Items.Add(Finder.GetItem("Grub"));
            lifeseedNotes.Items.Add(Finder.GetItem("Mimic_Grub"));

            ItemChangerMod.AddPlacements(new[] {iseldaShop, crawlidNotes, tiktikEntry, tiktikNotes, maskflyNotes, vengeflyScamEntry, lifeseedNotes});

            orig(self, permaDeath, bossRush);
        }

        public void HookIC()
        {
            Events.OnItemChangerHook += LanguageData.Hook;
            Events.OnItemChangerUnhook += LanguageData.Unhook;

            foreach (MinimalEnemyDef enemyDef in EnemyData.Data.Values)
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
    }
}
