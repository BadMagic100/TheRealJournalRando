namespace TheRealJournalRando
{
    public class GlobalSettings
    {
        public bool PrettyJournalSprites { get; set; } = true;

        public Rando.JournalRandomizationSettings RandoSettings { get; set; } = new();
    }
}
