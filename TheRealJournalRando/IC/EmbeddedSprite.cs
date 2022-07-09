using ItemChanger.Internal;

namespace TheRealJournalRando.IC
{
    public class EmbeddedSprite : ItemChanger.EmbeddedSprite
    {
        private static readonly SpriteManager manager = new(typeof(EmbeddedSprite).Assembly, "TheRealJournalRando.Resources.Sprites.");

        public EmbeddedSprite(string key)
        {
            this.key = key;
        }

        public override SpriteManager SpriteManager => manager;
    }
}
