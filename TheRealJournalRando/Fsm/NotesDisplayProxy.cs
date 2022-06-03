using HutongGames.PlayMaker;
using TheRealJournalRando.IC;

namespace TheRealJournalRando.Fsm
{
    internal class NotesDisplayProxy : FsmStateAction
    {
        private readonly JournalControlModule module;

        public NotesDisplayProxy(JournalControlModule module)
        {
            this.module = module;
        }

        public override void OnEnter()
        {
            string pdKillsName = Fsm.GetFsmString("pdKillsName").Value;
            string notesConvo = Fsm.GetFsmString("Item Notes Convo").Value;
            FsmString result = Fsm.GetFsmString("Item notes String");

            string enemyName = pdKillsName.Substring(5);

            result.Value = Language.Language.Get(notesConvo, "Journal").Replace("<br>", "\n");

            if (module.EnemyNotesIsRegistered(enemyName))
            {
                if (module.GetNotesPreview(enemyName) is string preview)
                {
                    result.Value += "\n\n" + preview;
                }
            }

            Finish();
        }
    }
}
