using ItemChanger;
using ItemChanger.Locations;
using ItemChanger.Placements;
using ItemChanger.Tags;
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
                controlModule.RegisterNotesPreviewHandler(playerDataName, GetPreviewText);
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

        private string GetPreviewText()
        {
            string costText = $"({Language.Language.Get("FREE", "IC")})";
            if (Placement.AllObtained())
            {
                string s1 = Language.Language.Get("OBTAINED", "IC");
                Placement.OnPreview(s1);
                return s1;
            }
            else if (Placement is ISingleCostPlacement cp)
            {
                if (cp.Cost is Cost c)
                {
                    if (c.Paid)
                    {
                        string s2 = string.Format(Language.Language.Get("HUNTER_NOTES_COMPLETE", "Fmt"), Placement.GetUIName());
                        Placement.OnPreview(s2);
                        return s2;
                    }
                    else if (Placement.HasTag<DisableCostPreviewTag>())
                    {
                        costText = Language.Language.Get("???", "IC");
                    }
                    else
                    {
                        costText = c.GetCostText();
                    }
                }
            }
            string s3 = string.Format(Language.Language.Get("HUNTER_NOTES_HINT", "Fmt"), costText, Placement.GetUIName());
            Placement.OnPreview(s3);
            return s3;
        }

        protected override void OnUnload()
        {
            killCounterModule!.OnKillCountChanged -= KillCountChanged;
            if (locationType == EnemyJournalLocationType.Notes)
            {
                controlModule!.DeregisterNotesPreviewHandler(playerDataName, GetPreviewText);
            }
        }
    }
}
