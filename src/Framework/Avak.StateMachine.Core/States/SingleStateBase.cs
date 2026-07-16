using Avak.StateMachine.Core.Contracts;

namespace Avak.StateMachine.Core.States
{
    public class SingleStateBase : StateBase
    {
        public override IStateViewModel GetStateViewModel()
        {
            throw new NotImplementedException();
        }
    }
}
