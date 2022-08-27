using ItemChanger;

namespace TheRealJournalRando.IC
{
    public class HunterNotesPreviewTag : Tag
    {
        public string? pdName;

        public override void Load(object parent)
        {
            base.Load(parent);

            AbstractLocation? loc = parent as AbstractLocation;
            if (loc != null && pdName != null)
            {
                ItemChangerMod.Modules.GetOrAdd<JournalControlModule>().RegisterNotesPreviewHandler(pdName, loc.Placement);
            }
        }

        public override void Unload(object parent)
        {
            base.Unload(parent);

            AbstractLocation? loc = parent as AbstractLocation;
            if (loc != null && pdName != null)
            {
                ItemChangerMod.Modules.Get<JournalControlModule>().DeregisterNotesPreviewHandler(pdName, loc.Placement);
            }
        }
    }
}
