using ItemChanger;
using System.Linq;
using UnityEngine;

namespace TheRealJournalRando.IC
{
    public class JournalBadgeSprite : ISprite
    {
        public string PlayerDataName { get; set; }

        public JournalBadgeSprite(string playerDataName)
        {
            PlayerDataName = playerDataName;
        }

        [Newtonsoft.Json.JsonIgnore]
        public Sprite Value
        {
            get
            {
                JournalEntryStats? stat = GameCameras.instance.hudCamera.GetComponentsInChildren<JournalEntryStats>(true)
                    .Where(j => j.playerDataName == PlayerDataName)
                    .FirstOrDefault();
                if (stat != null)
                {
                    return stat.transform.Find($"Portrait")
                        .GetComponent<SpriteRenderer>().sprite;
                }
                return Modding.CanvasUtil.NullSprite();
            }
        }

        public ISprite Clone() => (ISprite)MemberwiseClone();
    }
}
