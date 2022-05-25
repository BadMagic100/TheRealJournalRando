using ItemChanger;

namespace TheRealJournalRando.IC
{
    public class JournalEntryOnlyItem : AbstractItem
    {
        public string PlayerDataName { get; set; }

        public override void GiveImmediate(GiveInfo info)
        {
            string everKilledBool = "killed" + PlayerDataName;
            string firstKilledBool = "newData" + PlayerDataName;
            PlayerData.instance.SetBool(everKilledBool, true);
            PlayerData.instance.SetBool(firstKilledBool, true);
        }

        public override bool Redundant()
        {
            string everKilledBool = "killed" + PlayerDataName;
            return PlayerData.instance.GetBool(everKilledBool);
        }
    }
}
