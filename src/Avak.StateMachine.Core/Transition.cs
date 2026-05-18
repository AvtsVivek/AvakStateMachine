namespace Avak.StateMachine.Core
{
    public class Transition
    {
        public Trigger Trigger { get; set; } = null!;

        public StateBase Target { get; set; } = null!;
    }
}
