using MenuChanger;
using MenuChanger.Extensions;
using MenuChanger.MenuElements;
using MenuChanger.MenuPanels;
using RandomizerMod;
using RandomizerMod.Menu;
using System.Linq;
using UnityEngine;

namespace TheRealJournalRando.Rando
{
    public class ConnectionMenu
    {
        private const int VSPACE_MED = 200;
        private const int VSPACE_LARGE = 300;
        private const int VSPACE_SMALL = 50;
        private const int HSPACE_LARGE = 300;
        private const int HSPACE_XLARGE = 750;

        internal MenuPage journalRandoPage;

        private static ConnectionMenu? instance = null;
        internal static ConnectionMenu Instance => instance ??= new();

        public static void OnExitMenu()
        {
            instance = null;
        }

        public static void Hook()
        {
            RandomizerMenuAPI.AddMenuPage(Instance.ConstructMenu, Instance.HandleButton);
            MenuChangerMod.OnExitMainMenu += OnExitMenu;
        }

        private bool HandleButton(MenuPage landingPage, out SmallButton button)
        {
            SmallButton connectionMenuNavButton = new SmallButton(landingPage, Localization.Localize("Journal Entries (Extended)"));
            connectionMenuNavButton.AddHideAndShowEvent(landingPage, journalRandoPage);
            button = connectionMenuNavButton;
            return true;
        }

        private void ConstructMenu(MenuPage landingPage)
        {
            journalRandoPage = new MenuPage(Localization.Localize("Journal Entries (Extended)"), landingPage);
            VerticalItemPanel toplevelVip = new(journalRandoPage, new Vector2(0, 400), VSPACE_LARGE, true);

            MenuElementFactory<JournalRandomizationSettings> toplevelMef = new(journalRandoPage, RandoInterop.Settings);
            MenuLabel headingLabel = new(journalRandoPage, "Journal Entries (Extended)");
            VerticalItemPanel toplevelSettingHolder = new(journalRandoPage, Vector2.zero, VSPACE_SMALL, false, toplevelMef.Elements);
            toplevelSettingHolder.Insert(0, headingLabel);
            toplevelVip.Add(toplevelSettingHolder);

            GridItemPanel categoryGrid1 = new(journalRandoPage, Vector2.zero, 2, VSPACE_MED, HSPACE_XLARGE, false);
            GridItemPanel categoryGrid2 = new(journalRandoPage, Vector2.zero, 1, VSPACE_MED, HSPACE_XLARGE, false);
            VerticalItemPanel categoryVip = new(journalRandoPage, Vector2.zero, VSPACE_MED, false, categoryGrid1, categoryGrid2);
            toplevelVip.Add(categoryVip);

            MenuElementFactory<JournalRandomizationSettings.PoolSettings> poolMef = new(journalRandoPage, RandoInterop.Settings.Pools);
            MenuLabel poolsLabel = new(journalRandoPage, "Pool Settings");
            GridItemPanel poolSettingsHolder1 = new(journalRandoPage, Vector2.zero, 2, VSPACE_SMALL, HSPACE_LARGE, false, poolMef.Elements.Take(2).ToArray());
            GridItemPanel poolSettingsHolder2 = new(journalRandoPage, Vector2.zero, 1, VSPACE_SMALL, HSPACE_LARGE, false, poolMef.Elements[2]);
            categoryGrid1.Add(new VerticalItemPanel(journalRandoPage, Vector2.zero, VSPACE_SMALL, false, poolsLabel, poolSettingsHolder1, poolSettingsHolder2));

            MenuElementFactory<JournalRandomizationSettings.CostSettings> costsMef = new(journalRandoPage, RandoInterop.Settings.Costs);
            MenuLabel costsLabel = new(journalRandoPage, "Cost Randomization Settings");
            GridItemPanel costsSettingsHolder = new(journalRandoPage, Vector2.zero, 2, VSPACE_SMALL, HSPACE_LARGE, false, costsMef.Elements);
            categoryGrid1.Add(new VerticalItemPanel(journalRandoPage, Vector2.zero, VSPACE_SMALL + 35, false, costsLabel, costsSettingsHolder));

            MenuElementFactory<JournalRandomizationSettings.LongLocationSettings> llsMef = new(journalRandoPage, RandoInterop.Settings.LongLocations);
            MenuLabel llsLabel = new(journalRandoPage, "Long Location Settings");
            VerticalItemPanel llsBoolSettingsHolder = new(journalRandoPage, Vector2.zero, VSPACE_SMALL, false, llsMef.Elements.Take(3).ToArray());
            VerticalItemPanel llsSettingsHolder = new(journalRandoPage, Vector2.zero, VSPACE_SMALL * 3f + 35, false, llsBoolSettingsHolder, llsMef.Elements[3]);
            categoryGrid2.Add(new VerticalItemPanel(journalRandoPage, Vector2.zero, VSPACE_SMALL, false, llsLabel, llsSettingsHolder));

            Localization.Localize(headingLabel);
            Localization.Localize(toplevelMef);
            Localization.Localize(poolsLabel);
            Localization.Localize(poolMef);
            Localization.Localize(costsLabel);
            Localization.Localize(costsMef);
            Localization.Localize(llsLabel);
            Localization.Localize(llsMef);

            toplevelVip.ResetNavigation();
            toplevelVip.SymSetNeighbor(Neighbor.Down, journalRandoPage.backButton);
        }
    }
}
