using Avak.StateMachine.Core.States;
using System.Diagnostics;

namespace Avak.StateMachine.Core
{
    [DebuggerDisplay("Trigger: {Trigger}, Target: {Target}")]
    public class Transition
    {
        public Trigger Trigger { get; set; } = null!;

        public StateBase Target { get; set; } = null!;

        public Transition()
        {

        }
    }
}
