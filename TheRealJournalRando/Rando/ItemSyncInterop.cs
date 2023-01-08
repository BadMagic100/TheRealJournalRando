using Modding;

namespace TheRealJournalRando.Rando
{
    internal static class ItemSyncInterop
    {
        public static bool IsItemSync
        {
            get
            {
                if (ModHooks.GetMod("ItemSyncMod") is Mod)
                {
                    return IsItemSyncInternal;
                }
                return false;
            }
        }

        private static bool IsItemSyncInternal => ItemSyncMod.ItemSyncMod.ISSettings.IsItemSync;
    }
}
