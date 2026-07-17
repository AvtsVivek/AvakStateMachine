using Avak.StateMachine.Core.Contracts;

namespace Avak.StateMachine.Core.States
{
    public class SingleStateBase : StateBase
    {
        public short HyrarchyLevel { get; set; }

        public override IStateViewModel GetStateViewModel()
        {
            throw new NotImplementedException();
        }
    }
}
