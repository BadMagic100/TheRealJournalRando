using HutongGames.PlayMaker;
using ItemChanger.Extensions;

namespace TheRealJournalRando.Fsm
{
    public static class Extensions
    {
        public static void InjectState(this PlayMakerFSM self, string fromState, string fromEvent, string toState, FsmState newState, string newEvent = "FINISHED")
        {
            FsmState from = self.GetState(fromState);
            FsmState to = self.GetState(toState);

            self.AddState(newState);
            from.RemoveTransitionsOn(fromEvent);
            from.AddTransition(fromEvent, newState);
            newState.AddTransition(newEvent, to);
        }

        public static void MoveTransition(this PlayMakerFSM self, string fromState, string fromEvent, string toState)
        {
            FsmState from = self.GetState(fromState);
            FsmState to = self.GetState(toState);

            from.RemoveTransitionsOn(fromEvent);
            from.AddTransition(fromEvent, to);
        }

        public static FsmString AddFsmString(this PlayMakerFSM fsm, string name, string value)
        {
            FsmString fsmString = new()
            {
                Name = name,
                Value = value
            };
            FsmString[] array = new FsmString[fsm.FsmVariables.StringVariables.Length + 1];
            fsm.FsmVariables.StringVariables.CopyTo(array, 0);
            array[array.Length - 1] = fsmString;
            fsm.FsmVariables.StringVariables = array;
            return fsmString;
        }
    }
}
