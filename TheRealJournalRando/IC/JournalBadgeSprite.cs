using ItemChanger;
using ItemChanger.Internal;
using UnityEngine;

namespace TheRealJournalRando.IC
{
    public class JournalBadgeSprite : ISprite
    {
        private static readonly SpriteManager badgeManager = new(typeof(JournalBadgeSprite).Assembly, "TheRealJournalRando.Resources.Sprites.Badges.");

        public string PlayerDataName { get; set; }

        public JournalBadgeSprite(string playerDataName)
        {
            PlayerDataName = playerDataName;
        }

        [Newtonsoft.Json.JsonIgnore]
        public Sprite Value => badgeManager.GetSprite(PlayerDataName);

        public ISprite Clone() => (ISprite)MemberwiseClone();
    }
}
