namespace TheRealJournalRando.Rando
{
    internal static class RandoInterop
    {
        public static JournalRandomizationSettings Settings => TheRealJournalRando.Instance.GS.RandoSettings;

        public static void HookRandomizer()
        {
            ConnectionMenu.Hook();
            LogicPatcher.Hook();
        }
    }
}
