using ItemChanger;
using ItemChanger.Modules;
using ItemChanger.Placements;
using ItemChanger.UIDefs;
using MagicUI.Core;
using MagicUI.Elements;
using Modding;
using System;
using System.Linq;
using TheRealJournalRando.IC;

namespace TheRealJournalRando
{
    public class TheRealJournalRando : Mod
    {
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
        private LayoutRoot? layout;

        // if you need preloads, you will need to implement GetPreloadNames and use the other signature of Initialize.
        public override void Initialize()
        {
            Log("Initializing");

            On.HeroController.Awake += GameStarted;
            On.QuitToMenu.Start += GameExited;

            On.UIManager.StartNewGame += UIManager_StartNewGame;
            AbstractItem.ModifyRedundantItemGlobal += AbstractItem_ModifyRedundantItemGlobal;


            foreach ((string, string)[] mappings in new[] { bothMappings, entryOnlyMappings, noteOnlyMappings })
            {
                foreach ((string name, string pdName) in mappings)
                {
                    string icName = name.Replace(' ', '_');
                    AbstractItem entry = new EnemyJournalEntryOnlyItem(pdName)
                    {
                        name = $"Journal_Entry_Only-{icName}",
                        UIDef = new MsgUIDef
                        {
                            name = new BoxedString($"{name} Journal Entry"),
                            shopDesc = new BoxedString("something something monty python pet shop"),
                            sprite = new JournalBadgeSprite(pdName)
                        }
                    };
                    AbstractItem notes = new EnemyJournalNotesOnlyItem(pdName)
                    {
                        name = $"Hunter's_Notes-{icName}",
                        UIDef = new MsgUIDef
                        {
                            name = new BoxedString($"{name} Hunter's Notes"),
                            shopDesc = new BoxedString("something something monty python pet shop 2"),
                            sprite = new JournalBadgeSprite(pdName)
                        }
                    };
                    Finder.DefineCustomItem(entry);
                    Finder.DefineCustomItem(notes);
                }
            }
            Finder.DefineCustomLocation(new EnemyJournalLocation("Pigeon", EnemyJournalLocationType.Notes)
            {
                name = $"Hunter's_Notes-Maskfly"
            });
            Finder.DefineCustomLocation(new EnemyJournalLocation("Climber", EnemyJournalLocationType.Entry)
            {
                name = "Journal_Entry_Only-Tiktik"
            });


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

            AbstractPlacement tiktikEntry = Finder.GetLocation("Journal_Entry_Only-Tiktik").Wrap();
            ((ISingleCostPlacement)tiktikEntry).Cost = new EnemyKillCost("Climber", 1);
            tiktikEntry.Items.Add(Finder.GetItem("Soul_Totem-Path_of_Pain"));
            tiktikEntry.Items.Add(Finder.GetItem("Vengeful_Spirit"));

            AbstractPlacement maskflyNotes = Finder.GetLocation("Hunter's_Notes-Maskfly").Wrap();
            ((ISingleCostPlacement)maskflyNotes).Cost = new EnemyKillCost("Pigeon", 15);
            maskflyNotes.Items.Add(Finder.GetItem("Mantis_Claw"));

            ItemChangerMod.AddPlacements(new[] {iseldaShop, tiktikEntry, maskflyNotes});

            orig(self, permaDeath, bossRush);
        }

        private void GameStarted(On.HeroController.orig_Awake orig, HeroController self)
        {
            orig(self);
            layout = new LayoutRoot(true)
            {
                VisibilityCondition = GameManager.instance.IsGamePaused,
            };
            var btn = new Button(layout)
            {
                Content = "Dump Enemy Names",
                Margin = 20,
                Font = UI.TrajanBold,
                HorizontalAlignment = HorizontalAlignment.Right,
                VerticalAlignment = VerticalAlignment.Bottom,
                Padding = new Padding(15, 200),
            };
            btn.Click += (_) =>
            {
                foreach (string mapping in GameCameras.instance.hudCamera.GetComponentsInChildren<JournalEntryStats>(true)
                    .Select(s => $"{Language.Language.Get(s.nameConvo, "Journal")} => {s.playerDataName}"))
                {
                    LogDebug(mapping);
                }
            };
        }

        private System.Collections.IEnumerator GameExited(On.QuitToMenu.orig_Start orig, QuitToMenu self)
        {
            layout?.Destroy();
            return orig(self);
        }

    }
}
