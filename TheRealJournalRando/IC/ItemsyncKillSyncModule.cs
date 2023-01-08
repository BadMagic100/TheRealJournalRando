using ItemChanger;
using ItemChanger.Modules;

namespace TheRealJournalRando.IC
{
    public class ItemsyncKillSyncModule : Module
    {
        private const string KILL_SYNC_EVENT = nameof(TheRealJournalRando) + "-KillSync";

        private JournalKillCounterModule? killCounter;

        public override void Initialize()
        {
            killCounter = ItemChangerMod.Modules.GetOrAdd<JournalKillCounterModule>();

            killCounter.OnKillCountChanged += SyncKills;

            ItemSyncMod.ItemSyncMod.Connection.OnDataReceived += DataReceived;
        }

        public override void Unload()
        {
            if (killCounter != null)
            {
                killCounter.OnKillCountChanged -= SyncKills;
            }
        }

        private void SyncKills(string pdName, KillSourceType sourceType)
        {
            if (sourceType == KillSourceType.Normal)
            {
                ItemSyncMod.ItemSyncMod.Connection.SendDataToAll(KILL_SYNC_EVENT, pdName);
            }
        }

        private void DataReceived(MultiWorldLib.DataReceivedEvent e)
        {
            if (e.Label == KILL_SYNC_EVENT && !e.Handled)
            {
                e.Handled = true;
                killCounter?.RecordSilently(e.Content);
            }
        }
    }
}