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

        public override bool SupportsCost => true;

        public EnemyJournalLocation(string pdName, EnemyJournalLocationType locationType)
        {
            this.playerDataName = pdName;
            this.locationType = locationType;
        }

        public override GiveInfo GetGiveInfo() => new()
        {
            FlingType = flingType,
            Callback = null,
            Container = Container.Unknown,
            MessageType = MessageType.Corner,
        };

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
                controlModule.RegisterNotesPreviewHandler(playerDataName, Placement);
            }
            else
            {
                throw new NotImplementedException("BadMagic added a new journal location type and forgot to handle it properly :zote:");
            }

            killCounterModule = ItemChangerMod.Modules.GetOrAdd<JournalKillCounterModule>();
            killCounterModule.OnKillCountChanged += KillCountChanged;
        }

        protected override void OnUnload()
        {
            killCounterModule!.OnKillCountChanged -= KillCountChanged;
            if (locationType == EnemyJournalLocationType.Notes)
            {
                controlModule!.DeregisterNotesPreviewHandler(playerDataName, Placement);
            }
        }

        private void KillCountChanged(string pdName, KillSourceType sourceType)
        {
            if (playerDataName == pdName)
            {
                if (Placement is ISingleCostPlacement cp && cp.Cost != null)
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
                else
                {
                    GiveAll();
                }
            }
        }
    }
}
