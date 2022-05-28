using ItemChanger;
using ItemChanger.Locations;
using ItemChanger.Placements;
using System;

namespace TheRealJournalRando.IC
{
    public enum EnemyJournalLocationType
    {
        Entry,
        Notes,
    }

    public class EnemyJournalLocation : AutoLocation
    {
        public string playerDataName = "";
        public EnemyJournalLocationType locationType;

        private JournalControlModule? controlModule;
        private JournalKillCounterModule? killCounterModule;

        public EnemyJournalLocation(string pdName, EnemyJournalLocationType locationType)
        {
            this.playerDataName = pdName;
            this.locationType = locationType;
        }

        protected override void OnLoad()
        {
            controlModule = ItemChangerMod.Modules.GetOrAdd<JournalControlModule>();
            if (locationType == EnemyJournalLocationType.Entry)
            {
                controlModule.RegisterEnemyEntry(playerDataName);
            }
            else if (locationType == EnemyJournalLocationType.Notes)
            {
                controlModule.RegisterEnemyNotes(playerDataName);
            }
            else
            {
                throw new NotImplementedException("BadMagic added a new journal location type and forgot to handle it properly :zote:");
            }

            killCounterModule = ItemChangerMod.Modules.GetOrAdd<JournalKillCounterModule>();
            killCounterModule.OnKillCountChanged += KillCountChanged;
        }

        private void KillCountChanged(string pdName)
        {
            if (playerDataName == pdName && Placement is ISingleCostPlacement cp)
            {
                if (cp.Cost.Paid)
                {
                    GiveAll();
                }
                else if (cp.Cost.CanPay())
                {
                    cp.Cost.Pay();
                    GiveAll();
                }
            }
        }

        protected override void OnUnload()
        {
            killCounterModule!.OnKillCountChanged -= KillCountChanged;
        }
    }
}
