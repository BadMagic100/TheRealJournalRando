using ItemChanger;

namespace TheRealJournalRando.IC
{
    public class JournalNotesOnlyItem : AbstractItem
    {
        public string PlayerDataName { get; set; }

        public override void GiveImmediate(GiveInfo info)
        {
            TheRealJournalRando.Instance.LogDebug($"Giving {this.name}");
            PlayerData.instance.SetInt("kills" + PlayerDataName, 0);
        }

        public override bool Redundant()
        {
            return PlayerData.instance.GetInt("kills" + PlayerDataName) <= 0;
        }
    }
}
